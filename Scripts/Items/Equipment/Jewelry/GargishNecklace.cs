namespace Server.Items
{
    public class GargishNecklace : BaseArmor
    {
        public override ArmorMaterialType MaterialType => ArmorMaterialType.Chainmail;
        public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

        public override int BasePhysicalResistance => 1;
        public override int BaseFireResistance => 2;
        public override int BaseColdResistance => 2;
        public override int BasePoisonResistance => 2;
        public override int BaseEnergyResistance => 3;

        public override int InitMinHits => 30;
        public override int InitMaxHits => 40;

        [Constructable]
        public GargishNecklace()
            : base(0x4210)
        {
            Layer = Layer.Neck;
        }

        public override int GetDurabilityBonus()
        {
            int bonus = Quality == ItemQuality.Exceptional ? 20 : 0;

            return bonus + ArmorAttributes.DurabilityBonus;
        }

        protected override void ApplyResourceResistances(CraftResource oldResource)
        {
        }

        public GargishNecklace(int itemID)
            : base(itemID)
        {
        }

        public GargishNecklace(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GargishAmulet : GargishNecklace
    {
        [Constructable]
        public GargishAmulet()
            : base(0x4D0B)
        {
        }

        public GargishAmulet(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GargishStoneAmulet : GargishNecklace
    {
        public override ArmorMaterialType MaterialType => ArmorMaterialType.Stone;
        public override int StrReq => 40;

        [Constructable]
        public GargishStoneAmulet()
            : base(0x4D0A)
        {
            Hue = 2500;
        }

        public GargishStoneAmulet(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GargishOctopusNecklace : GargishNecklace
    {
        public override int InitMaxHits => 65;

        private GemType m_GemType;

        [CommandProperty(AccessLevel.GameMaster)]
        public GemType GemType
        {
            get => m_GemType;
            set
            {
                GemType old = m_GemType;
                m_GemType = value;
                OnGemTypeChange(old);
                InvalidateProperties();
            }
        }

        [Constructable]
        public GargishOctopusNecklace()
            : base(0xA34A)
        {
            AssignRandomGem();
        }

        private void AssignRandomGem()
        {
            int ran = Utility.RandomMinMax(1, 9);
            GemType = (GemType)ran;
        }

        public virtual void OnGemTypeChange(GemType old)
        {
            if (old == GemType)
                return;

            switch (GemType)
            {
                default:
                case GemType.None: Hue = 0; break;
                case GemType.StarSapphire: Hue = 1928; break;
                case GemType.Emerald: Hue = 1914; break;
                case GemType.Sapphire: Hue = 1926; break;
                case GemType.Ruby: Hue = 1911; break;
                case GemType.Citrine: Hue = 1955; break;
                case GemType.Amethyst: Hue = 1919; break;
                case GemType.Tourmaline: Hue = 1924; break;
                case GemType.Amber: Hue = 1923; break;
                case GemType.Diamond: Hue = 2067; break;
            }
        }

        public int GemLocalization()
        {
            switch (m_GemType)
            {
                default:
                case GemType.None: return 0;
                case GemType.StarSapphire: return 1023867;
                case GemType.Emerald: return 1023887;
                case GemType.Sapphire: return 1023887;
                case GemType.Ruby: return 1023868;
                case GemType.Citrine: return 1023875;
                case GemType.Amethyst: return 1023863;
                case GemType.Tourmaline: return 1023872;
                case GemType.Amber: return 1062607;
                case GemType.Diamond: return 1062608;
            }
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (GemType != GemType.None)
            {
                list.Add(1159019, string.Format("#{0}", GemLocalization())); // ~1_type~ gargish octopus necklace
            }
            else
            {
                list.Add(1125826); // gargish octopus necklace
            }
        }

        public GargishOctopusNecklace(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)m_GemType);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_GemType = (GemType)reader.ReadInt();
        }
    }
}
