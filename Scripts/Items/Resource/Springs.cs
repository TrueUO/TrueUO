namespace Server.Items
{
    [Flipable(0x105D, 0x105E)]
    public class Springs : Item
    {
        [Constructable]
        public Springs()
            : this(1)
        {
        }

        [Constructable]
        public Springs(int amount)
            : base(0x105D)
        {
            Stackable = true;
            Amount = amount;
        }

        public Springs(Serial serial)
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
