using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Server.Guilds;

namespace Server
{
	public class SaveStrategy 
	{
        private readonly Queue<Item> _DecayQueue = new();

        public void Save()
		{
            Thread saveItemsThread = new Thread(SaveItems)
            {
                Name = "Item Save Subset"
            };

            saveItemsThread.Start();

            SaveMobiles();
            SaveGuilds();

            saveItemsThread.Join();
        }

		public void ProcessDecay()
		{
			while (_DecayQueue.Count > 0)
			{
				Item item = _DecayQueue.Dequeue();

				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

        private void SaveMobiles()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
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
            sw.Stop();
            Console.WriteLine("mobiles time: " + sw.ElapsedMilliseconds);
        }

        /*Kept for backwards compatibility at this time. Will remove once the new SaveItems method
        is fully implemented/tested.*/
        private void SaveItemsOld()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            Dictionary<Serial, Item> items = World.Items;

            BinaryFileWriter idx = new BinaryFileWriter(World.ItemIndexPath, false);
            BinaryFileWriter tdb = new BinaryFileWriter(World.ItemTypesPath, false);
            BinaryFileWriter bin = new BinaryFileWriter(World.ItemDataPath, true);

            idx.Write(items.Count);
            foreach (Item item in items.Values)
            {
                if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= DateTime.UtcNow)
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

            tdb.Write(World.m_ItemTypes.Count);

            for (int i = 0; i < World.m_ItemTypes.Count; ++i)
                tdb.Write(World.m_ItemTypes[i].FullName);

            idx.Close();
            tdb.Close();
            bin.Close();
            sw.Stop();
            Console.WriteLine("item old method time: " + sw.ElapsedMilliseconds + "ms");
        }

        private void SaveItems()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            Dictionary<Serial, Item> items = World.Items;
            int itemCount = items.Count;
            Console.WriteLine($"Saving {itemCount} items");

            List<List<Item>> chunks = new List<List<Item>>();
            int chunkSize = 150000;

            List<Item> currentChunk = new List<Item>();
            int index = 0;

            foreach (var item in items.Values)
            {
                if (index % chunkSize == 0 && currentChunk.Count > 0)
                {
                    chunks.Add(currentChunk);
                    currentChunk = new List<Item>();
                }

                currentChunk.Add(item);
                index++;
            }

            if (currentChunk.Count > 0)
            {
                chunks.Add(currentChunk);
            }

            Console.WriteLine($"Time to split Items: {sw.ElapsedMilliseconds}ms");

            using (BinaryFileWriter tdb = new BinaryFileWriter(World.ItemTypesPath, false))
            {
                Parallel.ForEach(chunks, (chunk, state, chunkIndex) =>
            {
                string idxPath = World.ItemIndexPath.Replace(".idx", $"_{chunkIndex.ToString("D" + 8)}.idx");
                string binPath = World.ItemDataPath.Replace(".bin", $"_{chunkIndex.ToString("D" + 8)}.bin");

                using (BinaryFileWriter idx = new BinaryFileWriter(idxPath, false))
                using (BinaryFileWriter bin = new BinaryFileWriter(binPath, true))
                {
                    idx.Write(chunk.Count);
                    foreach (Item item in chunk)
                    {
                        if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= DateTime.UtcNow)
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
                }
            });

                tdb.Write(World.m_ItemTypes.Count);

                for (int i = 0; i < World.m_ItemTypes.Count; ++i)
                {
                    tdb.Write(World.m_ItemTypes[i].FullName);
                }
            }
            sw.Stop();
            Console.WriteLine($"Items Save complete: {sw.ElapsedMilliseconds}ms");
        }

        private void SaveGuilds()
        {
            BinaryFileWriter idx = new BinaryFileWriter(World.GuildIndexPath, false);
            BinaryFileWriter bin = new BinaryFileWriter(World.GuildDataPath, true);

            idx.Write(BaseGuild.List.Count);
            foreach (BaseGuild guild in BaseGuild.List.Values)
            {
                long start = bin.Position;

                idx.Write(0);//guilds have no typeid
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
