namespace Server.Items
{
    public class PowderCharge : Item, ICommodity
    {
        public override int LabelNumber => 1116160;  // powder charge

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        [Constructable]
        public PowderCharge()
            : this(1)
        {
        }

        [Constructable]
        public PowderCharge(int amount)
            : base(0xA2BE)
        {
            Stackable = true;
            Amount = amount;
        }

        public PowderCharge(Serial serial)
            : base(serial)
        {
        }

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
