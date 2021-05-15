namespace Server.Items
{
    public class Sand : Item, ICommodity
    {
        public override int LabelNumber => 1044626;  // sand
        public override int Hue => 2413;
        public override double DefaultWeight => 0.1;

        [Constructable]
        public Sand()
            : this(1)
        {
        }

        [Constructable]
        public Sand(int amount)
            : base(0x423A)
        {
            Hue = 2413;
            Stackable = true;
            Amount = amount;
        }

        public Sand(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
