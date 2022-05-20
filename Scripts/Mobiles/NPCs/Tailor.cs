using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Tailor : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos => m_SBInfos;

        [Constructable]
        public Tailor()
            : base("the tailor")
        {
            SetSkill(SkillName.Tailoring, 64.0, 100.0);
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBTailor());
        }

        public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Sandals : VendorShoeType.Shoes;

        public Tailor(Serial serial)
            : base(serial)
        {
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
