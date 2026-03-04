using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Server.Guilds;

namespace Server
{
    public sealed class ThreadedSaveStrategy : ISaveStrategy
    {
        private const int ChunkSize = 300000;
        private const int MaxParallelChunkWriters = 2;

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

            int chunkCount = Math.Max(1, (itemCount + ChunkSize - 1) / ChunkSize);
            int totalItemCount = 0;
            ConcurrentQueue<string> saveErrors = new ConcurrentQueue<string>();

            string[] idxPaths = new string[chunkCount];
            string[] binPaths = new string[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                idxPaths[i] = World.ItemIndexPath.Replace(".idx", $"_{i:D8}.idx");
                binPaths[i] = World.ItemDataPath.Replace(".bin", $"_{i:D8}.bin");
            }

            Item[] itemArray = new Item[itemCount];
            items.Values.CopyTo(itemArray, 0);

            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Math.Min(chunkCount, Math.Min(Environment.ProcessorCount, MaxParallelChunkWriters)))
            };

            Parallel.For(0, chunkCount, options, chunkIndex =>
            {
                try
                {
                    int startIndex = chunkIndex * ChunkSize;
                    int endIndex = Math.Min(startIndex + ChunkSize, itemCount);

                    using BinaryFileWriter idx = new BinaryFileWriter(idxPaths[chunkIndex], false);
                    using BinaryFileWriter bin = new BinaryFileWriter(binPaths[chunkIndex], true);

                    idx.Write(endIndex - startIndex);

                    int itemsWritten = 0;

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        WriteItem(itemArray[i], idx, bin, saveStartUtc);
                        itemsWritten++;
                    }

                    Interlocked.Add(ref totalItemCount, itemsWritten);
                }
                catch (Exception ex)
                {
                    saveErrors.Enqueue($"Error saving chunk {chunkIndex}: {ex}");
                }
            });

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

            Console.WriteLine("totalItemCount: " + totalItemCount + " original: " + itemCount);
        }

        private void WriteItem(Item item, BinaryFileWriter idx, BinaryFileWriter bin, DateTime saveStartUtc)
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
