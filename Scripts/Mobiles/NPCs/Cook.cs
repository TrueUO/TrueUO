using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Cook : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        [Constructable]
        public Cook()
            : base("the cook")
        {
            SetSkill(SkillName.Cooking, 90.0, 100.0);
            SetSkill(SkillName.TasteID, 75.0, 98.0);
        }

        public Cook(Serial serial)
            : base(serial)
        {
        }

        public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;
        protected override List<SBInfo> SBInfos => m_SBInfos;

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBCook());
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
