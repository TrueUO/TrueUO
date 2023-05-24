namespace Server.Items
{
    [Flipable(0x1411, 0x141a)]
    public class PlateLegs : BaseArmor
    {
        [Constructable]
        public PlateLegs()
            : base(0x1411)
        {
            Weight = 7.0;

            Attributes.BonusHits = 25;
            Attributes.BonusDex = -5;
        }

        public PlateLegs(Serial serial)
            : base(serial)
        {
        }

        public override int BasePhysicalResistance => Quality == ItemQuality.Exceptional ? 11 : 10;

        public override int InitMinHits => 50;
        public override int InitMaxHits => 65;
        public override int StrReq => 60;

        public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;

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
