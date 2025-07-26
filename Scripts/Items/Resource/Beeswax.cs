namespace Server.Items
{
    public class Beeswax : Item
    {
        [Constructable]
        public Beeswax()
            : this(1)
        {
        }

        [Constructable]
        public Beeswax(int amount)
            : base(0x1422)
        {
            Stackable = true;
            Amount = amount;
        }

        public Beeswax(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

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
