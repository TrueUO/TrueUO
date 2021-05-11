namespace Server.Items
{
    public class EssenceOrder : Item, ICommodity
    {
        [Constructable]
        public EssenceOrder()
            : this(1)
        {
        }

        [Constructable]
        public EssenceOrder(int amount)
            : base(0x571C)
        {
            Stackable = true;
            Amount = amount;
        }

        public EssenceOrder(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1113342;// essence of order
        public override int Hue => 1153;
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

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
