using Server.Engines.Points;
using Server.Multis;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class TrashBarrel : BaseTrash, IChopable
    {
        private Timer m_Timer;

        [Constructable]
        public TrashBarrel()
            : base(0xE77)
        {
            Hue = 0x3B2;
            Movable = false;
            m_Cleanup = new List<CleanupArray>();
        }

        public TrashBarrel(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1041064; // a trash barrel
        public override int DefaultMaxWeight => 0; // A value of 0 signals unlimited weight
        public override bool IsDecoContainer => false;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            if (Items.Count > 0)
            {
                m_Timer = new EmptyTimer(this);
                m_Timer.Start();
            }

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

            AddCleanupItem(from, dropped);

            if (TotalItems >= 50)
            {
                Empty(501478); // The trash is full!  Emptying!
            }
            else
            {
                SendLocalizedMessageTo(from, 1010442); // The item will be deleted in three minutes

                if (m_Timer != null)
                    m_Timer.Stop();
                else
                    m_Timer = new EmptyTimer(this);

                m_Timer.Start();
            }

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

            AddCleanupItem(from, item);

            if (TotalItems >= 50)
            {
                Empty(501478); // The trash is full!  Emptying!
            }
            else
            {
                SendLocalizedMessageTo(from, 1010442); // The item will be deleted in three minutes

                if (m_Timer != null)
                    m_Timer.Stop();
                else
                    m_Timer = new EmptyTimer(this);

                m_Timer.Start();
            }

            return true;
        }

        public void OnChop(Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(from);

            if (house != null && house.IsCoOwner(from))
            {
                Effects.PlaySound(Location, Map, 0x3B3);
                from.SendLocalizedMessage(500461); // You destroy the item.
                Destroy();
            }
        }

        public void Empty(int message)
        {
            List<Item> items = Items;

            if (items.Count > 0)
            {
                PublicOverheadMessage(Network.MessageType.Regular, 0x3B2, message, "");

                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    ConfirmCleanupItem(items[i]);

                    if (0.01 > Utility.RandomDouble())
                    {
                        DropToCavernOfDiscarded(items[i]);
                    }
                    else
                    {
                        items[i].Delete();
                    }
                }

                foreach (var x1 in m_Cleanup)
                {
                    if (x1.mobiles != null)
                    {
                        HashSet<Mobile> set = new HashSet<Mobile>();

                        for (var index = 0; index < m_Cleanup.Count; index++)
                        {
                            var x2 = m_Cleanup[index];
                            Mobile m = x2.mobiles;

                            if (set.Add(m) && m_Cleanup.Find(x => x.mobiles == m && x.confirm) != null)
                            {
                                double point = 0;

                                for (var i = 0; i < m_Cleanup.Count; i++)
                                {
                                    var x = m_Cleanup[i];

                                    if (x.mobiles == m && x.confirm)
                                    {
                                        point += x.points;
                                    }
                                }

                                int count = 0;

                                for (var i = 0; i < m_Cleanup.Count; i++)
                                {
                                    var r = m_Cleanup[i];

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

            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        private class EmptyTimer : Timer
        {
            private readonly TrashBarrel m_Barrel;
            public EmptyTimer(TrashBarrel barrel)
                : base(TimeSpan.FromMinutes(3.0))
            {
                m_Barrel = barrel;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Barrel.Empty(501479); // Emptying the trashcan!
            }
        }

        public static void DropToCavernOfDiscarded(Item item)
        {
            if (item == null || item.Deleted)
            {
                return;
            }

            Rectangle2D rec = new Rectangle2D(901, 482, 40, 27);
            Map map = Map.TerMur;

            for (int i = 0; i < 50; i++)
            {
                int x = Utility.RandomMinMax(rec.X, rec.X + rec.Width);
                int y = Utility.RandomMinMax(rec.Y, rec.Y + rec.Height);
                int z = map.GetAverageZ(x, y);

                Point3D p = new Point3D(x, y, z);

                if (map.CanSpawnMobile(p))
                {
                    item.MoveToWorld(p, map);
                    return;
                }
            }

            item.Delete();
        }
    }
}
