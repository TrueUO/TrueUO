namespace Server.Items
{
    public class FluffySpongeAddon : BaseAddon
    {
        [Constructable]
        public FluffySpongeAddon()
        {
            AddComponent(new AddonComponent(0x4C31), 0, 0, 0);
        }

        public FluffySpongeAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed => new FluffySpongeDeed();

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
    }

    [Furniture]
    public class FluffySpongeDeed : BaseAddonDeed
    {
        public override int LabelNumber => 1098377;  // Fluffy Sponge
        public override bool IsArtifact => true; // allows dying of the deed.

        [Constructable]
        public FluffySpongeDeed()
        {
            LootType = LootType.Blessed;
        }

        public FluffySpongeDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon => new FluffySpongeAddon();

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
    }
}
