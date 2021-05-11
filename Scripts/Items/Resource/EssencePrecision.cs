namespace Server.Items
{
    public class EssencePrecision : Item, ICommodity
    {
        [Constructable]
        public EssencePrecision()
            : this(1)
        {
        }

        [Constructable]
        public EssencePrecision(int amount)
            : base(0x571C)
        {
            Stackable = true;
            Amount = amount;
        }

        public EssencePrecision(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1113327;// essence of precision
        public override int Hue => 1158;
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
