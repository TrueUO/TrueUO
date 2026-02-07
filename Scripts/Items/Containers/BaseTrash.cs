using Server.ContextMenus;
using Server.Engines.Points;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Items
{
    public class CleanupArray
    {
        public Mobile mobiles { get; set; }
        public Item items { get; set; }
        public double points { get; set; }
        public bool confirm { get; set; }
        public Serial serials { get; set; }
    }

    public class BaseTrash : Container
    {
        internal List<CleanupArray> m_Cleanup;

        public BaseTrash(int itemID)
            : base(itemID)
        {
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (CleanUpBritanniaData.Enabled && from is PlayerMobile)
            {
                list.Add(new AppraiseforCleanup(from));
            }
        }

        private class AppraiseforCleanup : ContextMenuEntry
        {
            private readonly Mobile _Mobile;

            public AppraiseforCleanup(Mobile mobile)
                : base(1151298, 2) //Appraise for Cleanup
            {
                _Mobile = mobile;
            }

            public override void OnClick()
            {
                _Mobile.Target = new AppraiseforCleanupTarget(_Mobile);
                _Mobile.SendLocalizedMessage(1151299); //Target items to see how many Clean Up Britannia points you will receive for throwing them away. Continue targeting items until done, then press the ESC key to cancel the targeting cursor.
            }
        }

        public BaseTrash(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        private static IEnumerable<Item> EnumerateCleanupCandidates(Item root)
        {
            yield return root;

            if (root is BaseContainer container)
            {
                Container c = container;
                List<Item> list = c.FindItemsByType<Item>();

                for (int i = list.Count - 1; i >= 0; --i)
                {
                    yield return list[i];
                }
            }
        }

        public virtual bool AddCleanupItem(Mobile from, Item item)
        {
            if (!CleanUpBritanniaData.Enabled)
            {
                return false;
            }

            bool added = false;

            HashSet<Serial> existing = [];

            for (int i = 0; i < m_Cleanup.Count; i++)
            {
                existing.Add(m_Cleanup[i].serials);
            }

            foreach (Item candidate in EnumerateCleanupCandidates(item))
            {
                Serial s = candidate.Serial;

                if (existing.Contains(s))
                {
                    continue;
                }

                double points = CleanUpBritanniaData.GetPoints(candidate);

                if (points <= 0)
                {
                    continue;
                }

                m_Cleanup.Add(new CleanupArray
                {
                    mobiles = from,
                    items = candidate,
                    points = points,
                    serials = s
                });

                existing.Add(s);
                added = true;
            }

            return added;
        }

        public void ConfirmCleanupItem(Item item)
        {
            HashSet<Serial> targets = [];

            foreach (Item candidate in EnumerateCleanupCandidates(item))
            {
                targets.Add(candidate.Serial);
            }

            for (int i = 0; i < m_Cleanup.Count; i++)
            {
                CleanupArray entry = m_Cleanup[i];

                if (targets.Contains(entry.serials))
                {
                    entry.confirm = true;
                }
            }
        }
    }
}
