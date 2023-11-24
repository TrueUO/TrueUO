using System;
using System.Collections.Generic;
using System.Threading;
using Server.Guilds;

namespace Server
{
	public class SaveStrategy 
	{
        private readonly Queue<Item> _DecayQueue;

		public SaveStrategy()
		{
			_DecayQueue = new Queue<Item>();
		}

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

        protected void SaveMobiles()
        {
            Dictionary<Serial, Mobile> mobiles = World.Mobiles;

            using (GenericWriter idx = new BinaryFileWriter(World.MobileIndexPath, false))
            using (GenericWriter tdb = new BinaryFileWriter(World.MobileTypesPath, false))
            using (GenericWriter bin = new BinaryFileWriter(World.MobileDataPath, true))
            {
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
                {
                    tdb.Write(World.m_MobileTypes[i].FullName);
                }
            }
        }

        protected void SaveItems()
        {
            Dictionary<Serial, Item> items = World.Items;

            using (GenericWriter idx = new BinaryFileWriter(World.ItemIndexPath, false))
            using (GenericWriter tdb = new BinaryFileWriter(World.ItemTypesPath, false))
            using (GenericWriter bin = new BinaryFileWriter(World.ItemDataPath, true))
            {
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
                {
                    tdb.Write(World.m_ItemTypes[i].FullName);
                }
            }
        }

        protected void SaveGuilds()
        {
            using (GenericWriter idx = new BinaryFileWriter(World.GuildIndexPath, false))
            using (GenericWriter bin = new BinaryFileWriter(World.GuildDataPath, true))
            {
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
            }
        }
    }
}
