namespace Server.Items
{
    public class ArcaneBookShelfAddonSouth : BaseAddonContainer
    {
        public override BaseAddonContainerDeed Deed => new ArcaneBookShelfDeedSouth();
        public override bool RetainDeedHue => true;
        public override int DefaultGumpID => 0x107;
        public override int DefaultDropSound => 0x42;

        public override bool ForceShowProperties => true;

        [Constructable]
        public ArcaneBookShelfAddonSouth()
            : base(0x3084)
        {
            AddComponent(new AddonContainerComponent(0x3085), -1, 0, 0);
        }

        public ArcaneBookShelfAddonSouth(Serial serial)
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

    public class ArcaneBookShelfDeedSouth : BaseAddonContainerDeed
    {
        public override BaseAddonContainer Addon => new ArcaneBookShelfAddonSouth();
        public override int LabelNumber => 1072871;  // arcane bookshelf (south)

        [Constructable]
        public ArcaneBookShelfDeedSouth()
        {
        }

        public ArcaneBookShelfDeedSouth(Serial serial)
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
