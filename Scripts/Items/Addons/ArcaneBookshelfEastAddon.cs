namespace Server.Items
{
    public class ArcaneBookShelfAddonEast : BaseAddonContainer
    {
        public override BaseAddonContainerDeed Deed => new ArcaneBookShelfDeedEast();
        public override bool RetainDeedHue => true;
        public override int DefaultGumpID => 0x107;
        public override int DefaultDropSound => 0x42;

        public override bool ForceShowProperties => true;

        [Constructable]
        public ArcaneBookShelfAddonEast()
            : base(0x3086)
        {
            AddComponent(new AddonContainerComponent(0x3087), 0, -1, 0);
        }

        public ArcaneBookShelfAddonEast(Serial serial)
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

    public class ArcaneBookShelfDeedEast : BaseAddonContainerDeed
    {
        public override BaseAddonContainer Addon => new ArcaneBookShelfAddonEast();
        public override int LabelNumber => 1073371;  // arcane bookshelf (east)

        [Constructable]
        public ArcaneBookShelfDeedEast()
        {
        }

        public ArcaneBookShelfDeedEast(Serial serial)
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
