using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Server.Guilds;

namespace Server
{
    public sealed class ThreadedSaveStrategy : ISaveStrategy
    {
        private readonly ConcurrentQueue<Item> _DecayQueue = new();
        private bool _AllFilesSaved = true;

        public bool Save()
        {
            _AllFilesSaved = true;

            Thread saveItemsThread = new Thread(SaveItems)
            {
                Name = "Item Save Subset"
            };

            saveItemsThread.Start();

            SaveMobiles();
            SaveGuilds();

            saveItemsThread.Join();
            return _AllFilesSaved;
        }

        public void ProcessDecay()
        {
            while (_DecayQueue.TryDequeue(out Item item))
            {
                if (item != null && item.OnDecay())
                {
                    item.Delete();
                }
            }
        }

        private void SaveItems()
        {
            Dictionary<Serial, Item> items = World.Items;
            int itemCount = items.Count;
            DateTime saveStartUtc = DateTime.UtcNow;

            int chunkSize = 100000;
            List<List<Item>> chunks = new List<List<Item>>((itemCount + chunkSize - 1) / chunkSize);
            List<string> expectedFiles = new List<string>();

            List<Item> currentChunk = new List<Item>(Math.Min(chunkSize, itemCount));
            int index = 0;

            foreach (Item item in items.Values)
            {
                if (index % chunkSize == 0 && currentChunk.Count > 0)
                {
                    chunks.Add(currentChunk);
                    currentChunk = new List<Item>(Math.Min(chunkSize, itemCount - index));
                }

                currentChunk.Add(item);
                index++;
            }

            if (currentChunk.Count > 0)
            {
                chunks.Add(currentChunk);
            }

            int chunkCount = chunks.Count;
            string[] idxPaths = new string[chunkCount];
            string[] binPaths = new string[chunkCount];
            byte[][] idxBytes = new byte[chunkCount][];
            byte[][] binBytes = new byte[chunkCount][];

            for (int i = 0; i < chunkCount; i++)
            {
                idxPaths[i] = World.ItemIndexPath.Replace(".idx", $"_{i:D8}.idx");
                binPaths[i] = World.ItemDataPath.Replace(".bin", $"_{i:D8}.bin");
                expectedFiles.Add(idxPaths[i]);
                expectedFiles.Add(binPaths[i]);
            }

            int totalItemCount = 0;
            ConcurrentQueue<string> saveErrors = new ConcurrentQueue<string>();

            Parallel.ForEach(chunks, (chunk, state, chunkIndex) =>
            {
                try
                {
                    using MemoryStream idxStream = new MemoryStream();
                    using MemoryStream binStream = new MemoryStream();
                    using BinaryFileWriter idx = new BinaryFileWriter(idxStream, false);
                    using BinaryFileWriter bin = new BinaryFileWriter(binStream, true);

                    idx.Write(chunk.Count);

                    int itemsWritten = 0;

                    foreach (Item item in chunk)
                    {
                        if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= saveStartUtc)
                        {
                            _DecayQueue.Enqueue(item);
                        }

                        long start = bin.Position;

                        idx.Write(item.m_TypeRef);
                        idx.Write(item.Serial);
                        idx.Write(start);

                        item.Serialize(bin);

                        idx.Write((int)(bin.Position - start));

                        item.FreeCache();
                        itemsWritten++;
                    }

                    idx.Flush();
                    bin.Flush();

                    idxBytes[(int)chunkIndex] = idxStream.ToArray();
                    binBytes[(int)chunkIndex] = binStream.ToArray();
                    Interlocked.Add(ref totalItemCount, itemsWritten);
                }
                catch (Exception ex)
                {
                    saveErrors.Enqueue($"Error serializing chunk {chunkIndex}: {ex}");
                    state.Stop();
                }
            });

            for (int i = 0; i < chunkCount; i++)
            {
                if (idxBytes[i] == null || binBytes[i] == null)
                {
                    _AllFilesSaved = false;
                    continue;
                }

                try
                {
                    File.WriteAllBytes(idxPaths[i], idxBytes[i]);
                    File.WriteAllBytes(binPaths[i], binBytes[i]);
                }
                catch (Exception ex)
                {
                    _AllFilesSaved = false;
                    saveErrors.Enqueue($"Error writing chunk {i}: {ex}");
                }
            }

            using (BinaryFileWriter tdb = new BinaryFileWriter(World.ItemTypesPath, false))
            {
                tdb.Write(World.m_ItemTypes.Count);

                for (int i = 0; i < World.m_ItemTypes.Count; ++i)
                {
                    tdb.Write(World.m_ItemTypes[i].FullName);
                }
            }

            while (saveErrors.TryDequeue(out string error))
            {
                _AllFilesSaved = false;
                Console.WriteLine(error);
            }

            if (totalItemCount != itemCount)
            {
                _AllFilesSaved = false;
                Console.WriteLine($"Expected to save {itemCount}, but only saved {totalItemCount}. Un-threaded Save will be triggered");
            }

            foreach (string itemPath in expectedFiles)
            {
                if (!File.Exists(itemPath))
                {
                    _AllFilesSaved = false;
                    Console.WriteLine($"Save is missing file {itemPath}. Un-threaded Save will be triggered");
                }
            }

            Console.WriteLine("totalItemCount: " + totalItemCount + " original: " + itemCount);
        }

        private static void SaveMobiles()
        {
            Dictionary<Serial, Mobile> mobiles = World.Mobiles;

            BinaryFileWriter idx = new BinaryFileWriter(World.MobileIndexPath, false);
            BinaryFileWriter tdb = new BinaryFileWriter(World.MobileTypesPath, false);
            BinaryFileWriter bin = new BinaryFileWriter(World.MobileDataPath, true);

            idx.Write(mobiles.Count);
            foreach (Mobile m in mobiles.Values)
            {
                long start = bin.Position;

                idx.Write(m.m_TypeRef);
                idx.Write(m.Serial);
                idx.Write(start);

                m.Serialize(bin);

                idx.Write((int)(bin.Position - start));

                m.FreeCache();
            }

            tdb.Write(World.m_MobileTypes.Count);

            for (int i = 0; i < World.m_MobileTypes.Count; ++i)
                tdb.Write(World.m_MobileTypes[i].FullName);

            idx.Close();
            tdb.Close();
            bin.Close();
        }

        private static void SaveGuilds()
        {
            BinaryFileWriter idx = new BinaryFileWriter(World.GuildIndexPath, false);
            BinaryFileWriter bin = new BinaryFileWriter(World.GuildDataPath, true);

            idx.Write(BaseGuild.List.Count);
            foreach (BaseGuild guild in BaseGuild.List.Values)
            {
                long start = bin.Position;

                idx.Write(0); // guilds have no typeid
                idx.Write(guild.Id);
                idx.Write(start);

                guild.Serialize(bin);

                idx.Write((int)(bin.Position - start));
            }

            idx.Close();
            bin.Close();
        }
    }
}
