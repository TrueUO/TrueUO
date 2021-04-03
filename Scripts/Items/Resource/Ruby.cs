namespace Server.Items
{
    public class Ruby : Item, IGem, ICommodity
    {
        [Constructable]
        public Ruby()
            : this(1)
        {
        }

        [Constructable]
        public Ruby(int amount)
            : base(0xF13)
        {
            Stackable = true;
            Amount = amount;
        }

        public Ruby(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override double DefaultWeight => 0.1;

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
