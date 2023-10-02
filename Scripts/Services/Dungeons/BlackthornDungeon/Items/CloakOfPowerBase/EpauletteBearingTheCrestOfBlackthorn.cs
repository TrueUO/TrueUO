namespace Server.Items
{
    public class EpauletteBearingTheCrestOfBlackthorn4 : Cloak
    {
        public override bool IsArtifact => true;

        public override int LabelNumber => 1123325;  // Epaulette

        [Constructable]
        public EpauletteBearingTheCrestOfBlackthorn4()
        {
            ReforgedSuffix = ReforgedSuffix.Blackthorn;
            ItemID = 0x9985;
            Attributes.BonusStr = 2;
            Attributes.BonusDex = 2;
            Attributes.BonusInt = 2;
            Hue = 2107;

            Layer = Layer.OuterTorso;
        }

        public EpauletteBearingTheCrestOfBlackthorn4(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}