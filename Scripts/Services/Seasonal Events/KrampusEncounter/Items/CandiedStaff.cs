namespace Server.Items
{
    [Flipable(0xA7C3, 0xA7C4)]
    public class CandiedStaff : BaseStaff
    {
        public override int LabelNumber => 1126971;  // candied staff

        [Constructable]
        public CandiedStaff()
            : base(0xA7C3)
        {

        }

        public CandiedStaff(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility => WeaponAbility.Disarm;
        public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

        public override int StrengthReq => 35;
        public override int MinDamage => 13;
        public override int MaxDamage => 16;
        public override float Speed => 2.75f;

        public override int InitMinHits => 31;
        public override int InitMaxHits => 70;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
