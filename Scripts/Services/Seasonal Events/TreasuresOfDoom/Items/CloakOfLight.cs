namespace Server.Items
{
    public class CloakOfLight : Cloak
    {
        public override bool IsArtifact => true;
        [Constructable]
        public CloakOfLight()
        {
            Attributes.NightSight = 1;
            Attributes.RegenHits = 2;
            Attributes.AttackChance = 5;
        }

        public CloakOfLight(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1155608; // Cloak of Light

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
