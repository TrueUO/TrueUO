namespace Server.Items
{
    public class MasterCraftsmanTalisman : BaseTalisman
    {
        public override bool IsArtifact => true;

        private int _Type;
        public virtual int Type => _Type;

        [Constructable]
        public MasterCraftsmanTalisman(int charges, int itemID, TalismanSkill skill)
            : base(itemID)
        {
            Skill = skill;

            SuccessBonus = GetRandomSuccessful();
            ExceptionalBonus = GetRandomExceptional();
            Blessed = GetRandomBlessed();

            _Type = charges;
            Charges = charges;
        }

        public MasterCraftsmanTalisman(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1157213); // Crafting Failure Protection

            if (Charges > 0)
            {
                list.Add(1049116, Charges.ToString()); // [ Charges: ~1_CHARGES~ ]
            }
        }

        public override int LabelNumber => 1157217;// Master Craftsman Talisman
        public override bool ForceShowName => true;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _Type = reader.ReadInt();
        }
    }
}
