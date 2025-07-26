namespace Server.Items
{
    public class BolaBall : Item
    {
        public override int Hue => 0x8AC;

        [Constructable]
        public BolaBall()
            : this(1)
        {
        }

        [Constructable]
        public BolaBall(int amount)
            : base(0xE73)
        {
            Stackable = true;
            Amount = amount;
        }

        public BolaBall(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 4.0;

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
