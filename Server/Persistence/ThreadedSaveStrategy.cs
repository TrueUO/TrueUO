using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Server.Guilds;

namespace Server
{
    public class ThreadedSaveStrategy : ISaveStrategy
    {
        private readonly ConcurrentQueue<Item> _DecayQueue = new(); 
        private bool _AllFilesSaved = true;
        private readonly List<string> _ExpectedFiles = new();

        public bool Save()
        {
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
            while (_DecayQueue.Count > 0)
            {
                if (_DecayQueue.TryDequeue(out Item item) && item != null && item.OnDecay())
                {
                    // Thread-safe dequeuing
                    item.Delete();
                }
            }
        }

        private void SaveItems()
        {
            Dictionary<Serial, Item> items = World.Items;
            int itemCount = items.Count;

            List<List<Item>> chunks = new List<List<Item>>();
            int chunkSize = 100000; // Reduced chunk size to prevent large array copying issues

            List<Item> currentChunk = new List<Item>();
            int index = 0;

            foreach (Item item in items.Values)
            {
                if (index % chunkSize == 0 && currentChunk.Count > 0)
                {
                    chunks.Add(currentChunk);
                    currentChunk = new List<Item>();

                    int currentChunkIndex = chunks.Count - 1;

                    _ExpectedFiles.Add(World.ItemIndexPath.Replace(".idx", $"_{currentChunkIndex:D8}.idx"));
                    _ExpectedFiles.Add(World.ItemDataPath.Replace(".bin", $"_{currentChunkIndex:D8}.bin"));
                }

                currentChunk.Add(item);
                index++;
            }

            if (currentChunk.Count > 0)
            {
                chunks.Add(currentChunk);
            }

            int totalItemCount = 0;

            using (BinaryFileWriter tdb = new BinaryFileWriter(World.ItemTypesPath, false))
            {
                Parallel.ForEach(chunks, (chunk, state, chunkIndex) =>
                {
                    try
                    {
                        string idxPath = World.ItemIndexPath.Replace(".idx", $"_{chunkIndex:D8}.idx");
                        string binPath = World.ItemDataPath.Replace(".bin", $"_{chunkIndex:D8}.bin");

                        using (BinaryFileWriter idx = new BinaryFileWriter(idxPath, false))
                        using (BinaryFileWriter bin = new BinaryFileWriter(binPath, true))
                        {
                            int itemsWritten = 0;
                            idx.Write(chunk.Count);
                            foreach (Item item in chunk)
                            {
                                if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= DateTime.UtcNow)
                                {
                                    _DecayQueue.Enqueue(item); // Thread-safe enqueueing
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
                            Interlocked.Add(ref totalItemCount, itemsWritten);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving chunk {chunkIndex}: {ex.Message}");
                        state.Stop(); // Stop parallel processing on error
                    }
                });

                tdb.Write(World.m_ItemTypes.Count);

                for (int i = 0; i < World.m_ItemTypes.Count; ++i)
                {
                    tdb.Write(World.m_ItemTypes[i].FullName);
                }

                Console.WriteLine("totalItemCount: " + totalItemCount + " original: " + itemCount);
            }

            if (totalItemCount != itemCount)
            {
                _AllFilesSaved = false;
                Console.WriteLine($"Expected to save {itemCount}, but only saved {totalItemCount}. Un-threaded Save will be triggered");
            }

            foreach (string item in _ExpectedFiles)
            {
                if (!File.Exists(item))
                {
                    _AllFilesSaved = false;
                    Console.WriteLine($"Save is missing file {item}. Un-threaded Save will be triggered");
                }
            }
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
