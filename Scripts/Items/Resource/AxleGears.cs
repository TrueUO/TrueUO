namespace Server.Items
{
    [Flipable(0x1051, 0x1052)]
    public class AxleGears : Item
    {
        [Constructable]
        public AxleGears()
            : this(1)
        {
        }

        [Constructable]
        public AxleGears(int amount)
            : base(0x1051)
        {
            Stackable = true;
            Amount = amount;
        }

        public AxleGears(Serial serial)
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
