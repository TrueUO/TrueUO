using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Armorer : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        [Constructable]
        public Armorer()
            : base("the armourer")
        {
            SetSkill(SkillName.ArmsLore, 64.0, 100.0);
            SetSkill(SkillName.Blacksmith, 60.0, 83.0);
        }

        public Armorer(Serial serial)
            : base(serial)
        {
        }

        public override VendorShoeType ShoeType => VendorShoeType.Boots;
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo()
        {
            switch (Utility.Random(4))
            {
                case 0:
                    {
                        m_SBInfos.Add(new SBLeatherArmor());
                        m_SBInfos.Add(new SBStuddedArmor());
                        m_SBInfos.Add(new SBMetalShields());
                        m_SBInfos.Add(new SBPlateArmor());
                        m_SBInfos.Add(new SBHelmetArmor());
                        m_SBInfos.Add(new SBChainmailArmor());
                        m_SBInfos.Add(new SBRingmailArmor());
                        break;
                    }
                case 1:
                    {
                        m_SBInfos.Add(new SBStuddedArmor());
                        m_SBInfos.Add(new SBLeatherArmor());
                        m_SBInfos.Add(new SBMetalShields());
                        m_SBInfos.Add(new SBHelmetArmor());
                        break;
                    }
                case 2:
                    {
                        m_SBInfos.Add(new SBMetalShields());
                        m_SBInfos.Add(new SBPlateArmor());
                        m_SBInfos.Add(new SBHelmetArmor());
                        m_SBInfos.Add(new SBChainmailArmor());
                        m_SBInfos.Add(new SBRingmailArmor());
                        break;
                    }
                case 3:
                    {
                        m_SBInfos.Add(new SBMetalShields());
                        m_SBInfos.Add(new SBHelmetArmor());
                        break;
                    }
            }
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            AddItem(new Items.HalfApron(Utility.RandomYellowHue()));
            AddItem(new Items.Bascinet());
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
