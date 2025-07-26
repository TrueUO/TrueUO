namespace Server.Items
{
    [Flipable(0x104D, 0x104E)]
    public class ClockFrame : Item
    {
        [Constructable]
        public ClockFrame()
            : this(1)
        {
        }

        [Constructable]
        public ClockFrame(int amount)
            : base(0x104D)
        {
            Stackable = true;
            Amount = amount;
        }

        public ClockFrame(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

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
