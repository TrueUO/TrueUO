using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Server
{
    public class BackupStrategy : ISaveStrategy
    {
        private readonly Queue<Item> _DecayQueue = new();

        public void Save()
        {
            Thread saveItemsThread = new Thread(SaveItems)
            {
                Name = "Item Save Subset"
            };

            saveItemsThread.Start();
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

        private void SaveItems()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            Dictionary<Serial, Item> items = World.Items;

            BinaryFileWriter idx = new BinaryFileWriter(World.ItemIndexPath + ".bak", false);
            BinaryFileWriter tdb = new BinaryFileWriter(World.ItemTypesPath + ".bak", false);
            BinaryFileWriter bin = new BinaryFileWriter(World.ItemDataPath + ".bak", true);

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
                new ArrayBufferWriter<Item>();
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
    }
}
