namespace Server.Items
{
    public class ElvenDresserAddonEast : BaseAddonContainer
    {
        public override BaseAddonContainerDeed Deed => new ElvenDresserDeedEast();

        public override int DefaultGumpID => 0x51;
        public override int DefaultDropSound => 0x42;
        public override bool RetainDeedHue => true;

        [Constructable]
        public ElvenDresserAddonEast() : base(0x30E4)
        {
            AddComponent(new AddonContainerComponent(0x30E3), 0, -1, 0);
        }

        public ElvenDresserAddonEast(Serial serial)
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

    public class ElvenDresserDeedEast : BaseAddonContainerDeed
    {
        public override BaseAddonContainer Addon => new ElvenDresserAddonEast();
        public override int LabelNumber => 1073388;

        [Constructable]
        public ElvenDresserDeedEast()
        {
        }

        public ElvenDresserDeedEast(Serial serial)
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
