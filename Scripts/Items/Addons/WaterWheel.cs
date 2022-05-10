using Server.Gumps;

namespace Server.Items
{
    public class WaterWheelAddon : BaseAddon, IWaterSource
    {
        [Constructable]
        public WaterWheelAddon(DirectionType type)
        {
            switch (type)
            {
                case DirectionType.South:
                    AddComponent(new LocalizedAddonComponent(0xA0F8, 1125222), 0, 0, 0);
                    break;
                case DirectionType.East:
                    AddComponent(new LocalizedAddonComponent(0xA0EE, 1125222), 0, 0, 0);
                    break;
            }
        }

        public int Quantity { get => 500; set { } }

        public WaterWheelAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                WaterWheelDeed deed = new WaterWheelDeed();

                return deed;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class WaterWheelDeed : BaseAddonDeed, IRewardOption
    {
        public override int LabelNumber => 1158881;  // Water Wheel

        public override BaseAddon Addon
        {
            get
            {
                WaterWheelAddon addon = new WaterWheelAddon(_Direction);

                return addon;
            }
        }

        private DirectionType _Direction;

        [Constructable]
        public WaterWheelDeed()
        {
        }

        public WaterWheelDeed(Serial serial)
            : base(serial)
        {
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)DirectionType.South, 1075386); // South
            list.Add((int)DirectionType.East, 1075387); // East
        }

        public void OnOptionSelected(Mobile from, int choice)
        {
            _Direction = (DirectionType)choice;

            if (!Deleted)
                base.OnDoubleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(AddonOptionGump));
                from.SendGump(new AddonOptionGump(this, 1154194)); // Choose a Facing:
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
