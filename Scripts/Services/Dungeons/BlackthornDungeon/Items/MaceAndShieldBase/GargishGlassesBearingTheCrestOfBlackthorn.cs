namespace Server.Items
{
    public class GargishGlassesBearingTheCrestOfBlackthorn2 : GargishGlasses
    {
        public override bool IsArtifact => true;

        [Constructable]
        public GargishGlassesBearingTheCrestOfBlackthorn2()
        {
            ReforgedSuffix = ReforgedSuffix.Blackthorn;
            WeaponAttributes.HitLowerDefend = 30;
            Attributes.BonusStr = 10;
            Attributes.BonusDex = 5;
            StrRequirement = 45;
            Hue = 66;
        }

        public override int BasePhysicalResistance => 25;
        public override int BaseFireResistance => 10;
        public override int BaseColdResistance => 10;
        public override int BasePoisonResistance => 10;
        public override int BaseEnergyResistance => 10;
        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        public GargishGlassesBearingTheCrestOfBlackthorn2(Serial serial)
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
