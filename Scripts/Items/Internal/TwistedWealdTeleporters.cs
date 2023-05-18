namespace Server.Items
{
    public class TwistedWealdTele : Teleporter
    {
        [Constructable]
        public TwistedWealdTele()
            : base(new Point3D(2189, 1253, 0), Map.Ilshenar)
        {
        }

        public TwistedWealdTele(Serial serial)
            : base(serial)
        {
        }

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
