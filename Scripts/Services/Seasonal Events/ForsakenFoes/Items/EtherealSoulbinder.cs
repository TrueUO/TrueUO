using Server.Mobiles;

namespace Server.Items
{
    public class EtherealSoulbinder : Item
    {
        public override int LabelNumber => 1159167;  // ethereal soulbinder

        public double MaxSoulPoint { get; } = 100;

        private double m_SoulPoint;

        [CommandProperty(AccessLevel.GameMaster)]
        public double SoulPoint
        {
            get => m_SoulPoint;
            set
            {
                if (value < 0)
                {
                    m_SoulPoint = 0;
                }
                else if (value > MaxSoulPoint)
                {
                    m_SoulPoint = MaxSoulPoint;
                }
                else
                {
                    m_SoulPoint = value;
                }

                SetHue();
                InvalidateProperties();
            }
        }

        private void SetHue()
        {
            if (SoulPoint <= 0)
                Hue = 0;
            else if (SoulPoint <= 1)
                Hue = 1910; // Meager
            else if (SoulPoint <= 25)
                Hue = 1916; // Grand
            else if (SoulPoint <= 50)
                Hue = 1914; // Exalted
            else if (SoulPoint <= 90)
                Hue = 1922; // Legendary
            else
                Hue = 1919; // Mythical
        }

        private int GetDescription()
        {
            if (SoulPoint <= 0)
            {
                return 1159177; // An Empty Soulbinder
            }
            if (SoulPoint <= 1)
            {
                return 1159176; // Meager
            }
            if (SoulPoint <= 25)
            {
                return 1159175; // Grand
            }
            if (SoulPoint <= 50)
            {
                return 1159174; // Exalted
            }
            if (SoulPoint <= 90)
            {
                return 1159173; // Legendary
            }

            return 1159172; // Mythical
        }

        [Constructable]
        public EtherealSoulbinder()
            : base(0xA1E7)
        {
        }

        public EtherealSoulbinder(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            int desc = GetDescription();

            if (desc == 1159177)
            {
                list.Add(1159177); // An Empty Soulbinder
            }
            else
            {
                list.Add(1159178, string.Format("#{0}", desc)); // Contains a ~1_TYPE~ Soul
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_SoulPoint);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_SoulPoint = reader.ReadDouble();
        }

        public static void OnCreatureDeath(Mobile creature, Mobile killer)
        {
            if (killer is BaseCreature kbc && kbc.Controlled && kbc.ControlMaster != null)
            {
                killer = kbc.ControlMaster;
            }

            if (creature is BaseCreature bc && bc.IsSoulBound && killer is PlayerMobile pm && pm.Backpack != null)
            {
                EtherealSoulbinder es = null;

                // Manually searching through items to find EtherealSoulbinder.
                foreach (EtherealSoulbinder item in pm.Backpack.FindItemsByType<EtherealSoulbinder>())
                {
                    if (es == null || item.SoulPoint < es.MaxSoulPoint)
                    {
                        es = item;
                    }
                }

                if (es != null)
                {
                    int hm = bc.HitsMax;
                    int scaler = hm > 1000 ? 1000 : 100;

                    es.SoulPoint += (double)(hm / scaler) * PotionOfGloriousFortune.GetBonus(killer, PotionEventType.Soulbinder);
                }
            }
        }
    }
}
