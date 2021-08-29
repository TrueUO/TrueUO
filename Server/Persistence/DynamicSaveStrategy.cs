using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Server.Guilds;

namespace Server
{
	public class DynamicSaveStrategy : SaveStrategy
	{
		private readonly ConcurrentBag<Item> _decayBag;

		private readonly BlockingCollection<QueuedMemoryWriter> _itemThreadWriters;
        private readonly BlockingCollection<QueuedMemoryWriter> _guildThreadWriters;

        private SequentialFileWriter _itemData, _itemIndex;
        private SequentialFileWriter _guildData, _guildIndex;

		public DynamicSaveStrategy()
		{
			_decayBag = new ConcurrentBag<Item>();
			_itemThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
            _guildThreadWriters = new BlockingCollection<QueuedMemoryWriter>();
			new BlockingCollection<QueuedMemoryWriter>();
		}

		public override string Name => "Dynamic";

		public override void Save(bool permitBackgroundWrite)
		{
            OpenFiles();

            Thread saveMobilesThread = new Thread(SaveMobiles)
            {
                Name = "Mobile Save Subset"
            };
            saveMobilesThread.Start();

			Task[] saveTasks = new Task[2];

			saveTasks[0] = SaveItems();
            saveTasks[1] = SaveGuilds();

            SaveTypeDatabases();

            saveMobilesThread.Join();

            if (permitBackgroundWrite)
			{
				//This option makes it finish the writing to disk in the background, continuing even after Save() returns.
				Task.Factory.ContinueWhenAll(saveTasks, _ =>
				{
					CloseFiles();

					World.NotifyDiskWriteComplete();
				});
			}
			else
			{
				Task.WaitAll(saveTasks);    //Waits for the completion of all of the tasks(committing to disk)
				CloseFiles();
			}
        }

		public override void ProcessDecay()
		{
            while (_decayBag.TryTake(out Item item))
			{
				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

		private static Task StartCommitTask(BlockingCollection<QueuedMemoryWriter> threadWriter, SequentialFileWriter data, SequentialFileWriter index)
		{
			Task commitTask = Task.Factory.StartNew(() =>
			{
				while (!threadWriter.IsCompleted)
				{
					QueuedMemoryWriter writer;

					try
					{
						writer = threadWriter.Take();
					}
					catch (InvalidOperationException)
					{
						//Per MSDN, it's fine if we're here, successful completion of adding can rarely put us into this state.
						break;
					}

					writer.CommitTo(data, index);
				}
			});

			return commitTask;
		}

        protected virtual void SaveMobiles()
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
            {
                tdb.Write(World.m_MobileTypes[i].FullName);
            }

            idx.Close();
            tdb.Close();
            bin.Close();
        }

		private Task SaveItems()
		{
			//Start the blocking consumer; this runs in background.
			Task commitTask = StartCommitTask(_itemThreadWriters, _itemData, _itemIndex);

			IEnumerable<Item> items = World.Items.Values;

			//Start the producer.
			Parallel.ForEach(items, () => new QueuedMemoryWriter(),
				(Item item, ParallelLoopState state, QueuedMemoryWriter writer) =>
				{
					long startPosition = writer.Position;

					item.Serialize(writer);

					int size = (int)(writer.Position - startPosition);

					writer.QueueForIndex(item, size);

					if (item.Decays && item.Parent == null && item.Map != Map.Internal && DateTime.UtcNow > (item.LastMoved + item.DecayTime))
					{
						_decayBag.Add(item);
					}

                    return writer;
				},
				writer =>
				{
					writer.Flush();

					_itemThreadWriters.Add(writer);
				});

			_itemThreadWriters.CompleteAdding();    //We only get here after the Parallel.ForEach completes.  Lets our task 

			return commitTask;
		}

		private Task SaveGuilds()
		{
			//Start the blocking consumer; this runs in background.
			Task commitTask = StartCommitTask(_guildThreadWriters, _guildData, _guildIndex);

			IEnumerable<BaseGuild> guilds = BaseGuild.List.Values;

			//Start the producer.
			Parallel.ForEach(guilds, () => new QueuedMemoryWriter(),
                (guild, state, writer) =>
				{
					long startPosition = writer.Position;

					guild.Serialize(writer);

					int size = (int)(writer.Position - startPosition);

					writer.QueueForIndex(guild, size);

                    return writer;
				},
				writer =>
				{
					writer.Flush();

					_guildThreadWriters.Add(writer);
				});

			_guildThreadWriters.CompleteAdding();   //We only get here after the Parallel.ForEach completes.  Lets our task 

			return commitTask;
		}

		private void OpenFiles()
		{
			_itemData = new SequentialFileWriter(World.ItemDataPath);
			_itemIndex = new SequentialFileWriter(World.ItemIndexPath);

            _guildData = new SequentialFileWriter(World.GuildDataPath);
			_guildIndex = new SequentialFileWriter(World.GuildIndexPath);

			WriteCount(_itemIndex, World.Items.Count);
            WriteCount(_guildIndex, BaseGuild.List.Count);
		}

		private void CloseFiles()
		{
			_itemData.Close();
			_itemIndex.Close();

            _guildData.Close();
			_guildIndex.Close();
		}

		private static void WriteCount(Stream indexFile, int count)
		{
            byte[] buffer = new byte[4];

			buffer[0] = (byte)count;
			buffer[1] = (byte)(count >> 8);
			buffer[2] = (byte)(count >> 16);
			buffer[3] = (byte)(count >> 24);

			indexFile.Write(buffer, 0, buffer.Length);
		}

		private static void SaveTypeDatabases()
		{
			SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
        }

		private static void SaveTypeDatabase(string path, IReadOnlyCollection<Type> types)
		{
			BinaryFileWriter bfw = new BinaryFileWriter(path, false);

			bfw.Write(types.Count);

			foreach (Type type in types)
			{
				bfw.Write(type.FullName);
			}

			bfw.Flush();

			bfw.Close();
		}
	}
}
