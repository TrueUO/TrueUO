namespace Server.Items
{
    [Flipable(0x104F, 0x1050)]
    public class ClockParts : Item
    {
        [Constructable]
        public ClockParts()
            : this(1)
        {
        }

        [Constructable]
        public ClockParts(int amount)
            : base(0x104F)
        {
            Stackable = true;
            Amount = amount;
        }

        public ClockParts(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

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
