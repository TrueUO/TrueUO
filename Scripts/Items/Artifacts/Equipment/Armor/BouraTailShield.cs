namespace Server.Items
{
    public class BouraTailShield : WoodenKiteShield
    {
        public override bool IsArtifact => true;
        [Constructable]
        public BouraTailShield()
        {
            Attributes.ReflectPhysical = 10;
            ArmorAttributes.ReactiveParalyze = 1;
        }

        public BouraTailShield(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1112361; // boura tail shield
        public override int DefaultHue => 2310;

        public override int BasePhysicalResistance => 8;
        public override int BaseFireResistance => 0;
        public override int BaseColdResistance => 0;
        public override int BasePoisonResistance => 0;
        public override int BaseEnergyResistance => 1;
        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

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
