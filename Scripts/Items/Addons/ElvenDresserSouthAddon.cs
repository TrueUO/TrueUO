namespace Server.Items
{
    public class ElvenDresserAddonSouth : BaseAddonContainer
    {
        public override BaseAddonContainerDeed Deed => new ElvenDresserDeedSouth();

        public override int DefaultGumpID => 0x51;
        public override int DefaultDropSound => 0x42;
        public override bool RetainDeedHue => true;

        [Constructable]
        public ElvenDresserAddonSouth()
            : base(0x30E6)
        {
            AddComponent(new AddonContainerComponent(0x30E5), -1, 0, 0);
        }

        public ElvenDresserAddonSouth(Serial serial)
            : base(serial)
        {
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

    public class ElvenDresserDeedSouth : BaseAddonContainerDeed
    {
        public override BaseAddonContainer Addon => new ElvenDresserAddonSouth();
        public override int LabelNumber => 1072864;

        [Constructable]
        public ElvenDresserDeedSouth()
        {
        }

        public ElvenDresserDeedSouth(Serial serial)
            : base(serial)
        {
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
