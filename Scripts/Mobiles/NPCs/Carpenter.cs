using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Carpenter : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        [Constructable]
        public Carpenter()
            : base("the carpenter")
        {
            SetSkill(SkillName.Carpentry, 85.0, 100.0);
            SetSkill(SkillName.Lumberjacking, 60.0, 83.0);
        }

        public Carpenter(Serial serial)
            : base(serial)
        {
        }

        protected override List<SBInfo> SBInfos => m_SBInfos;

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBStavesWeapon());
            m_SBInfos.Add(new SBCarpenter());
            m_SBInfos.Add(new SBWoodenShields());
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            AddItem(new Items.HalfApron());
        }

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
