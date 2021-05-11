namespace Server.Items
{
    public abstract class BaseGranite : Item, ICommodity
    {
        private CraftResource m_Resource;

        public override int Hue => CraftResources.GetHue(m_Resource);

        public BaseGranite(CraftResource resource)
            : base(0x1779)
        {
            Stackable = true;

            m_Resource = resource;
        }

        public BaseGranite(Serial serial)
            : base(serial)
        {
        }

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get => m_Resource;
            set
            {
                m_Resource = value;
                InvalidateProperties();
            }
        }

        public override double DefaultWeight => 1.0;

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.DullCopper && m_Resource <= CraftResource.Valorite)
                    return 1112060 + (m_Resource - CraftResource.DullCopper);

                return 1044607; // high quality granite
            }
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Amount > 1)
                list.Add(1050039, "{0}\t#{1}", Amount, 1044607); // ~1_NUMBER~ ~2_ITEMNAME~
            else
                list.Add(1044607); // high quality granite
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
            {
                int num = CraftResources.GetLocalizationNumber(m_Resource);

                if (num > 0)
                    list.Add(num);
                else
                    list.Add(CraftResources.GetName(m_Resource));
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Resource = (CraftResource)reader.ReadInt();
        }
    }

    public class Granite : BaseGranite
    {
        [Constructable]
        public Granite()
            : this(1)
        {
        }

        [Constructable]
        public Granite(int amount)
            : base(CraftResource.Iron)
        {
            Amount = Stackable ? amount : 1;
        }

        public Granite(Serial serial)
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

    public class DullCopperGranite : BaseGranite
    {
        [Constructable]
        public DullCopperGranite()
            : this(1)
        {
        }

        [Constructable]
        public DullCopperGranite(int amount)
            : base(CraftResource.DullCopper)
        {
            Amount = Stackable ? amount : 1;
        }

        public DullCopperGranite(Serial serial)
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

    public class ShadowIronGranite : BaseGranite
    {
        [Constructable]
        public ShadowIronGranite()
            : this(1)
        {
        }

        [Constructable]
        public ShadowIronGranite(int amount)
            : base(CraftResource.ShadowIron)
        {
            Amount = Stackable ? amount : 1;
        }

        public ShadowIronGranite(Serial serial)
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

    public class CopperGranite : BaseGranite
    {
        [Constructable]
        public CopperGranite()
            : this(1)
        {
        }

        [Constructable]
        public CopperGranite(int amount)
            : base(CraftResource.Copper)
        {
            Amount = Stackable ? amount : 1;
        }

        public CopperGranite(Serial serial)
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

    public class BronzeGranite : BaseGranite
    {
        [Constructable]
        public BronzeGranite()
            : this(1)
        {
        }

        [Constructable]
        public BronzeGranite(int amount)
            : base(CraftResource.Bronze)
        {
            Amount = Stackable ? amount : 1;
        }

        public BronzeGranite(Serial serial)
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

    public class GoldGranite : BaseGranite
    {
        [Constructable]
        public GoldGranite()
            : this(1)
        {
        }

        [Constructable]
        public GoldGranite(int amount)
            : base(CraftResource.Gold)
        {
            Amount = Stackable ? amount : 1;
        }

        public GoldGranite(Serial serial)
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

    public class AgapiteGranite : BaseGranite
    {
        [Constructable]
        public AgapiteGranite()
            : this(1)
        {
        }

        [Constructable]
        public AgapiteGranite(int amount)
            : base(CraftResource.Agapite)
        {
            Amount = Stackable ? amount : 1;
        }

        public AgapiteGranite(Serial serial)
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

    public class VeriteGranite : BaseGranite
    {
        [Constructable]
        public VeriteGranite()
            : this(1)
        {
        }

        [Constructable]
        public VeriteGranite(int amount)
            : base(CraftResource.Verite)
        {
            Amount = Stackable ? amount : 1;
        }

        public VeriteGranite(Serial serial)
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

    public class ValoriteGranite : BaseGranite
    {
        [Constructable]
        public ValoriteGranite()
            : this(1)
        {
        }

        [Constructable]
        public ValoriteGranite(int amount)
            : base(CraftResource.Valorite)
        {
            Amount = Stackable ? amount : 1;
        }

        public ValoriteGranite(Serial serial)
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
