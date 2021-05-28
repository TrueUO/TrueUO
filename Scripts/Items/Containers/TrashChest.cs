using Server.Engines.Points;
using System.Collections.Generic;

namespace Server.Items
{
    [Flipable(0xE41, 0xE40)]
    public class TrashChest : BaseTrash
    {
        [Constructable]
        public TrashChest()
            : base(0xE41)
        {
            Movable = false;
            m_Cleanup = new List<CleanupArray>();
        }

        public TrashChest(Serial serial)
            : base(serial)
        {
        }

        public override int DefaultMaxWeight => 0;// A value of 0 signals unlimited weight
        public override bool IsDecoContainer => false;
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Cleanup = new List<CleanupArray>();
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!base.OnDragDrop(from, dropped))
                return false;

            if (dropped is SpellbookStrap)
            {
                from.SendLocalizedMessage(1075256); // That is blessed; you cannot throw it away.
                return false;
            }

            if (CleanUpBritanniaData.Enabled && !AddCleanupItem(from, dropped))
            {
                if (dropped.LootType == LootType.Blessed)
                {
                    from.SendLocalizedMessage(1075256); // That is blessed; you cannot throw it away.
                    return false;
                }
            }

            PublicOverheadMessage(Network.MessageType.Regular, 0x3B2, Utility.Random(1042891, 8));
            Empty();

            return true;
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!base.OnDragDropInto(from, item, p))
                return false;

            if (item is SpellbookStrap)
            {
                from.SendLocalizedMessage(1075256); // That is blessed; you cannot throw it away.
                return false;
            }

            if (CleanUpBritanniaData.Enabled && !AddCleanupItem(from, item))
            {
                if (item.LootType == LootType.Blessed)
                {
                    from.SendLocalizedMessage(1075256); // That is blessed; you cannot throw it away.
                    return false;
                }
            }

            PublicOverheadMessage(Network.MessageType.Regular, 0x3B2, Utility.Random(1042891, 8));
            Empty();

            return true;
        }

        public void Empty()
        {
            List<Item> items = Items;

            if (items.Count > 0)
            {
                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    ConfirmCleanupItem(items[i]);

                    if (.01 > Utility.RandomDouble())
                    {
                        TrashBarrel.DropToCavernOfDiscarded(items[i]);
                    }
                    else
                    {
                        items[i].Delete();
                    }
                }

                for (var i = 0; i < m_Cleanup.Count; i++)
                {
                    var x1 = m_Cleanup[i];

                    if (x1.mobiles != null)
                    {
                        HashSet<Mobile> set = new HashSet<Mobile>();

                        for (var index1 = 0; index1 < m_Cleanup.Count; index1++)
                        {
                            var x2 = m_Cleanup[index1];
                            Mobile m = x2.mobiles;

                            if (set.Add(m) && (m_Cleanup.Find(x => x.mobiles == m && x.confirm) != null))
                            {
                                double point = 0;

                                for (var index = 0; index < m_Cleanup.Count; index++)
                                {
                                    var x = m_Cleanup[index];

                                    if (x.mobiles == m && x.confirm)
                                    {
                                        point += x.points;
                                    }
                                }

                                int count = 0;

                                for (var index = 0; index < m_Cleanup.Count; index++)
                                {
                                    var r = m_Cleanup[index];

                                    if (r.mobiles == m)
                                    {
                                        count++;
                                    }
                                }

                                m.SendLocalizedMessage(1151280,
                                    $"{point.ToString()}\t{count}"); // You have received approximately ~1_VALUE~points for turning in ~2_COUNT~items for Clean Up Britannia.

                                PointsSystem.CleanUpBritannia.AwardPoints(m, point);
                            }
                        }

                        m_Cleanup.Clear();
                        break;
                    }
                }
            }
        }
    }
}
