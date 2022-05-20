using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Scribe : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        [Constructable]
        public Scribe()
            : base("the scribe")
        {
            SetSkill(SkillName.EvalInt, 60.0, 83.0);
            SetSkill(SkillName.Inscribe, 90.0, 100.0);
        }

        public Scribe(Serial serial)
            : base(serial)
        {
        }

        public override NpcGuild NpcGuild => NpcGuild.MagesGuild;
        public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBScribe(this));
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            AddItem(new Items.Robe(Utility.RandomNeutralHue()));
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
