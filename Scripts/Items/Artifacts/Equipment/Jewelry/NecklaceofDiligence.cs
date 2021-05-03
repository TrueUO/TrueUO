namespace Server.Items
{
    public class NecklaceofDiligence : SilverNecklace
    {
        public override int LabelNumber { get { return 1113137; } } // Necklace of Diligence
        public override bool IsArtifact => true;

        [Constructable]
        public NecklaceofDiligence()
        {
            Hue = 221;
            Attributes.RegenMana = 1;
            Attributes.BonusInt = 5;
        }

        public NecklaceofDiligence(Serial serial)
            : base(serial)
        {
        }

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
