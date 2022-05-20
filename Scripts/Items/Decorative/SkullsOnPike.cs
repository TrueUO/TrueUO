namespace Server.Items
{
    public class SkullsOnPike : Item
    {
        [Constructable]
        public SkullsOnPike()
            : base(0x42B5)
        {
        }

        public SkullsOnPike(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1;

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
