namespace Server.Items
{
    public class Hephaestus : HeaterShield
    {
        public override int LabelNumber => 1152909;  // Hephaestus
        public override bool IsArtifact => true;

        [Constructable]
        public Hephaestus() : this(true)
        {
        }

        [Constructable]
        public Hephaestus(bool antique)
        {
            Hue = 1910;
            Attributes.SpellChanneling = 1;
            Attributes.ReflectPhysical = 15;
            Attributes.DefendChance = 15;
            Attributes.CastSpeed = 1;
            Attributes.LowerManaCost = 8;

            PhysicalBonus = 15;
            ArmorAttributes.SelfRepair = 5;

            SkillBonuses.SetValues(0, SkillName.Parry, 10.0);

            if (antique)
            {
                MaxHitPoints = 250;
                NegativeAttributes.Antique = 1;
            }
            else
            {
                MaxHitPoints = 255;
                ArmorAttributes.SelfRepair = 5;
            }

            HitPoints = MaxHitPoints;
        }

        public Hephaestus(Serial serial)
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

            if (NegativeAttributes.Antique != 0 && ArmorAttributes.SelfRepair > 0)
                ArmorAttributes.SelfRepair = 0;
        }
    }
}
