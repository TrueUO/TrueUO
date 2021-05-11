namespace Server.Items
{
    public class EssenceBalance : Item, ICommodity
    {
        [Constructable]
        public EssenceBalance()
            : this(1)
        {
        }

        [Constructable]
        public EssenceBalance(int amount)
            : base(0x571C)
        {
            Stackable = true;
            Amount = amount;
        }

        public EssenceBalance(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1113324;// essence of balance
        public override int Hue => 1268;
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
