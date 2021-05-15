namespace Server.Items
{
    public class OceanSapphire : Item, ICommodity
    {
        public override int LabelNumber => 1159162;  // ocean sapphire
        public override int Hue => 1917;

        [Constructable]
        public OceanSapphire()
            : this(1)
        {
        }

        [Constructable]
        public OceanSapphire(int amount)
            : base(0xA414)
        {
            Stackable = true;
            Amount = amount;
        }

        public OceanSapphire(Serial serial)
            : base(serial)
        {
        }

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
