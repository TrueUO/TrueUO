namespace Server.Items
{
    public class ContestMiniHouse : MiniHouseAddon
    {
        [Constructable]
        public ContestMiniHouse()
            : base(MiniHouseType.MalasMountainPass)
        {
        }

        [Constructable]
        public ContestMiniHouse(MiniHouseType type)
            : base(type)
        {
        }

        public ContestMiniHouse(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                ContestMiniHouseDeed deed = new ContestMiniHouseDeed(Type);

                return deed;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class ContestMiniHouseDeed : MiniHouseDeed
    {
        [Constructable]
        public ContestMiniHouseDeed()
            : base(MiniHouseType.MalasMountainPass)
        {
        }

        [Constructable]
        public ContestMiniHouseDeed(MiniHouseType type)
            : base(type)
        {
        }

        public ContestMiniHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                ContestMiniHouse addon = new ContestMiniHouse(Type);

                return addon;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
