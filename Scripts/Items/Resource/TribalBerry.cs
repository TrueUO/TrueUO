namespace Server.Items
{
    public class TribalBerry : Item, ICommodity
    {
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override int LabelNumber => 1040001; // tribal berry
        public override int Hue => 6;

        [Constructable]
        public TribalBerry()
            : this(1)
        {
        }

        [Constructable]
        public TribalBerry(int amount)
            : base(0x9D0)
        {
            Stackable = true;
            Amount = amount;
        }

        public TribalBerry(Serial serial)
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
