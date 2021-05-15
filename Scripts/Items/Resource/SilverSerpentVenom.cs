namespace Server.Items
{
    public class SilverSerpentVenom : Item, ICommodity
    {
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override int LabelNumber => 1112173; // silver serpent venom
        public override int Hue => 1155;

        [Constructable]
        public SilverSerpentVenom()
            : this(1)
        {
        }

        [Constructable]
        public SilverSerpentVenom(int amount)
            : base(0xE24)
        {
            Stackable = true;
            Amount = amount;
        }

        public SilverSerpentVenom(Serial serial)
            : base(serial)
        {
        }

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
