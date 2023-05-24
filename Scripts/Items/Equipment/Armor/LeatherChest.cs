namespace Server.Items
{
    [Flipable(0x13cc, 0x13d3)]
    public class LeatherChest : BaseArmor
    {
        [Constructable]
        public LeatherChest()
            : base(0x13CC)
        {
            Weight = 6.0;

            Attributes.BonusHits = 5;
        }

        public LeatherChest(Serial serial)
            : base(serial)
        {
        }

        public override int BasePhysicalResistance => Quality == ItemQuality.Exceptional ? 3 : 2;

        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;
        public override int StrReq => 20;

        public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
        public override CraftResource DefaultResource => CraftResource.RegularLeather;
        public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

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
