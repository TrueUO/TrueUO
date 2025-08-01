namespace Server.Items
{
    public class PolishedMeteorite : Item
    {
        public override int LabelNumber => 1158693;  // polished meteorite

        [Constructable]
        public PolishedMeteorite()
            : base(41422 + Utility.Random(12))
        {
        }

        public PolishedMeteorite(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 3.0;

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
