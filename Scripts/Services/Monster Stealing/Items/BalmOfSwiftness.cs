using Server.Mobiles;

namespace Server.Items
{
    public class BalmOfSwiftness : BaseBalmOrLotion
    {
        public override int LabelNumber => 1094942;  // Balm of Swiftness

        [Constructable]
        public BalmOfSwiftness()
            : base(0x1848)
        {
            m_EffectType = ThieveConsumableEffect.BalmOfSwiftnessEffect;
        }

        protected override void ApplyEffect(PlayerMobile pm)
        {
            pm.AddStatMod(new StatMod(StatType.Dex, "Balm", 10, m_EffectDuration));
            pm.SendLocalizedMessage(1095138);//You apply the balm and suddenly feel more agile!
            base.ApplyEffect(pm);
        }

        public BalmOfSwiftness(Serial serial)
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
