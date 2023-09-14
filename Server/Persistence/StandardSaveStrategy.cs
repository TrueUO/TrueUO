using System;
using System.Collections.Generic;
using Server.Guilds;

namespace Server
{
	public class StandardSaveStrategy : SaveStrategy
	{
        private readonly Queue<Item> _DecayQueue;

		public StandardSaveStrategy()
		{
			_DecayQueue = new Queue<Item>();
		}

		public override string Name => "Standard";
        
		public override void Save()
		{
            SaveMobiles();
			SaveItems();
			SaveGuilds();
        }

		public override void ProcessDecay()
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

            GenericWriter idx = new BinaryFileWriter(World.MobileIndexPath, false);
            GenericWriter tdb = new BinaryFileWriter(World.MobileTypesPath, false);
            GenericWriter bin = new BinaryFileWriter(World.MobileDataPath, true);

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

		protected void SaveItems()
		{
			Dictionary<Serial, Item> items = World.Items;

            GenericWriter idx = new BinaryFileWriter(World.ItemIndexPath, false);
            GenericWriter tdb = new BinaryFileWriter(World.ItemTypesPath, false);
            GenericWriter bin = new BinaryFileWriter(World.ItemDataPath, true);

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
		}

		protected void SaveGuilds()
		{
            GenericWriter idx = new BinaryFileWriter(World.GuildIndexPath, false);
            GenericWriter bin = new BinaryFileWriter(World.GuildDataPath, true);

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
