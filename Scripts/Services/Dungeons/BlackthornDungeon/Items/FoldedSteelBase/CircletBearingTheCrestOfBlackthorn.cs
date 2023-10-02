namespace Server.Items
{
    public class CircletBearingTheCrestOfBlackthorn3 : Circlet
    {
        public override bool IsArtifact => true;

        [Constructable]
        public CircletBearingTheCrestOfBlackthorn3()
        {
            ReforgedSuffix = ReforgedSuffix.Blackthorn;
            Attributes.NightSight = 1;
            Attributes.BonusStr = 8;
            Attributes.DefendChance = 15;
            StrRequirement = 45;
            Hue = 1150;
        }

        public override int BasePhysicalResistance => 20;
        public override int BaseFireResistance => 10;
        public override int BaseColdResistance => 10;
        public override int BasePoisonResistance => 10;
        public override int BaseEnergyResistance => 10;
        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        public CircletBearingTheCrestOfBlackthorn3(Serial serial)
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