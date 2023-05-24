namespace Server.Items
{
    [Flipable(0x1415, 0x1416)]
    public class PlateChest : BaseArmor
    {
        [Constructable]
        public PlateChest()
            : base(0x1415)
        {
            Weight = 10.0;

            Attributes.BonusHits = 25;
            Attributes.BonusDex = -5;
        }

        public PlateChest(Serial serial)
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
