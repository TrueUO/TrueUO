using Server.Commands;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    [Flipable(0x407C, 0x407D)]
    public class Incubator : Container, ISecurable
    {
        public override int LabelNumber => 1112479; // an incubator

        private SecureLevel m_Level;

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get => m_Level;
            set => m_Level = value;
        }

        public override int DefaultGumpID => 0x40;

        public static List<Incubator> IncubatorInstances = new List<Incubator>();

        [Constructable]
        public Incubator()
            : base(0x407C)
        {
            m_Level = SecureLevel.CoOwners;
            Weight = 10;

            IncubatorInstances.Add(this);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            bool canDrop = base.OnDragDropInto(from, item, p);

            if (canDrop && item is ChickenLizardEgg egg)
            {
                if (egg.TotalIncubationTime > TimeSpan.FromHours(120))
                    egg.BurnEgg();
                else
                {
                    egg.IncubationStart = DateTime.UtcNow;
                    egg.Incubating = true;
                }
            }

            return canDrop;
        }

        public override bool OnDragDrop(Mobile from, Item item)
        {
            bool canDrop = base.OnDragDrop(from, item);

            if (canDrop && item is ChickenLizardEgg egg)
            {
                if (egg.TotalIncubationTime > TimeSpan.FromHours(120))
                    egg.BurnEgg();
                else
                {
                    egg.IncubationStart = DateTime.UtcNow;
                    egg.Incubating = true;
                }
            }

            return canDrop;
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            if (BaseHouse.CheckLockedDown(this))
            {
                PrivateOverheadMessage(MessageType.Regular, 0x21, 1113711, m.NetState); // The incubator must be secured for the egg to grow, not locked down.
            }

            return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
        }

        public void CheckEggs_Callback()
        {
            if (!BaseHouse.CheckSecured(this))
                return;

            List<Item> items = Items;

            if (items.Count > 0)
            {
                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    if (items[i] is ChickenLizardEgg egg)
                    {
                        egg.CheckStatus();
                    }
                }
            }
        }

        public override void OnDelete()
        {
            base.OnDelete();

            IncubatorInstances.Remove(this);
        }

        public static void Initialize()
        {
            CommandSystem.Register("IncreaseStage", AccessLevel.Counselor, IncreaseStage_OnCommand);

            EventSink.AfterWorldSave += AfterWorldSave;
        }

        public static void AfterWorldSave(AfterWorldSaveEventArgs e)
        {
            foreach (Incubator incubator in IncubatorInstances)
            {
                if (incubator.Items.Count > 0)
                    Timer.DelayCall(TimeSpan.FromSeconds(10), incubator.CheckEggs_Callback);
            }
        }

        public static void IncreaseStage_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Target the egg.");
            e.Mobile.BeginTarget(12, false, Targeting.TargetFlags.None, IncreaseStage_OnTarget);
        }

        public static void IncreaseStage_OnTarget(Mobile from, object targeted)
        {
            if (targeted is ChickenLizardEgg egg)
            {
                egg.TotalIncubationTime += TimeSpan.FromHours(24);
                egg.CheckStatus();
            }
        }

        public Incubator(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write((int)m_Level);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Level = (SecureLevel)reader.ReadInt();

            if (Items.Count > 0)
                Timer.DelayCall(TimeSpan.FromSeconds(60), CheckEggs_Callback);

            // Needed to add existing ones to new list. Do I need to keep this?
            IncubatorInstances.Add(this);
        }
    }
}
