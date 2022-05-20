namespace Server.Items
{
    [Furniture]
    [Flipable(0x2DDD, 0x2DDE)]
    public class ElvenPodium : CraftableFurniture
    {
        [Constructable]
        public ElvenPodium()
            : base(0x2DDD)
        {
            Weight = 1.0;
        }

        public ElvenPodium(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1073399;// elven podium

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
