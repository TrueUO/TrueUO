using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    public class SterlingSilverRing : SilverRing
    {
        public override bool IsArtifact => true;
        public override int LabelNumber => 1155606;  // Stirling Silver Ring

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        public bool HasSkillBonus => SkillBonuses.Skill_2_Value != 0;

        [Constructable]
        public SterlingSilverRing()
        {
            SkillBonuses.SetValues(0, SkillName.Meditation, 20);
            Attributes.RegenHits = 3;
            Attributes.RegenMana = 5;
            Attributes.WeaponDamage = 75;
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (IsChildOf(m.Backpack) && m is PlayerMobile mobile && !HasSkillBonus)
            {
                BaseGump.SendGump(new ApplySkillBonusGump(mobile, SkillBonuses, Skills, 20, 1));
            }
            else
            {
                base.OnDoubleClick(m);
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!HasSkillBonus)
            {
                list.Add(1155609); // Double Click to Set Skill Bonus
            }
        }

        public static SkillName[] Skills =
        {
            SkillName.Archery,
            SkillName.Fencing,
            SkillName.Macing,
            SkillName.Swords,
            SkillName.Throwing,
            SkillName.Wrestling
        };

        public SterlingSilverRing(Serial serial)
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
            int version = reader.ReadInt();

            if (version < 1)
            {
                SkillBonuses.SetValues(0, SkillName.Meditation, 20);
                SkillBonuses.Skill_2_Value = 0;
                SkillBonuses.Skill_3_Value = 0;
            }
        }
    }
}
