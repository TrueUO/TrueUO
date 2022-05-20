using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Weaver : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos => m_SBInfos;

        [Constructable]
        public Weaver()
            : base("the weaver")
        {
            SetSkill(SkillName.Tailoring, 65.0, 88.0);
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBWeaver());
        }

        public override VendorShoeType ShoeType => VendorShoeType.Sandals;

        public Weaver(Serial serial)
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
