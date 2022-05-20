using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Alchemist : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        [Constructable]
        public Alchemist()
            : base("the alchemist")
        {
            SetSkill(SkillName.Alchemy, 85.0, 100.0);
            SetSkill(SkillName.TasteID, 65.0, 88.0);
        }

        public Alchemist(Serial serial)
            : base(serial)
        {
        }

        public override NpcGuild NpcGuild => NpcGuild.MagesGuild;
        public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;
        protected override List<SBInfo> SBInfos => m_SBInfos;
        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBAlchemist(this));
        }

        public override void InitOutfit()
        {
            base.InitOutfit();

            AddItem(new Items.Robe(Utility.RandomPinkHue()));
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
