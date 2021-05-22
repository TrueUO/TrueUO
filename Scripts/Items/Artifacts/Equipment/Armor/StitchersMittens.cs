namespace Server.Items
{
    public class StitchersMittens : LeafGloves
    {
        public override bool IsArtifact => true;
        public override int LabelNumber => 1072932;// Stitcher's Mittens
        public override int DefaultHue => 0x481;
        public override int BasePhysicalResistance => 20;
        public override int BaseColdResistance => 20;

        [Constructable]
        public StitchersMittens()
        {
            SkillBonuses.SetValues(0, SkillName.Healing, 10.0);
            Attributes.BonusDex = 5;
            Attributes.LowerRegCost = 30;
        }

        public StitchersMittens(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
