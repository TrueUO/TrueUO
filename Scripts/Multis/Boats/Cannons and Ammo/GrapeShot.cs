namespace Server.Items
{
    public class Grapeshot : Item, ICommodity, ICannonAmmo
    {
        public override int LabelNumber => 1116030;  // grapeshot
        public override double DefaultWeight => 3.5;

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public AmmunitionType AmmoType => AmmunitionType.Grapeshot;

        [Constructable]
        public Grapeshot()
            : this(1)
        {
        }

        [Constructable]
        public Grapeshot(int amount)
            : base(0xA2BF)
        {
            Stackable = true;
            Amount = amount;
        }

        public Grapeshot(Serial serial)
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
