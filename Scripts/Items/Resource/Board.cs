namespace Server.Items
{
    [Flipable(0x1BD7, 0x1BDA)]
    public class BaseWoodBoard : Item, ICommodity, IResource
    {
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource { get => m_Resource; set { m_Resource = value; InvalidateProperties(); } }

        public override int LabelNumber
        {
            get
            {
                if (m_Resource >= CraftResource.OakWood && m_Resource <= CraftResource.YewWood)
                    return 1075052 + ((int)m_Resource - (int)CraftResource.OakWood);

                switch (m_Resource)
                {
                    case CraftResource.Bloodwood: return 1075055;
                    case CraftResource.Frostwood: return 1075056;
                    case CraftResource.Heartwood: return 1075062;   //WHY Osi.  Why?
                }

                return 1015101;
            }
        }

        TextDefinition ICommodity.Description => LabelNumber;

        bool ICommodity.IsDeedable => true;

        [Constructable]
        public BaseWoodBoard()
            : this(1)
        {
        }

        [Constructable]
        public BaseWoodBoard(int amount)
            : this(CraftResource.RegularWood, amount)
        {
        }

        public BaseWoodBoard(Serial serial)
            : base(serial)
        {
        }

        [Constructable]
        public BaseWoodBoard(CraftResource resource) : this(resource, 1)
        {
        }

        [Constructable]
        public BaseWoodBoard(CraftResource resource, int amount)
            : base(0x1BD7)
        {
            Stackable = true;
            Amount = amount;

            m_Resource = resource;
        }

        public override int Hue => CraftResources.GetHue(m_Resource);

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
            writer.Write(0);

            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Resource = (CraftResource)reader.ReadInt();
        }
    }

    public class Board : BaseWoodBoard
    {
        [Constructable]
        public Board()
            : this(1)
        {
        }

        [Constructable]
        public Board(int amount)
            : base(CraftResource.RegularWood, amount)
        {
        }

        public Board(Serial serial)
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

    public class HeartwoodBoard : BaseWoodBoard
    {
        [Constructable]
        public HeartwoodBoard()
            : this(1)
        {
        }

        [Constructable]
        public HeartwoodBoard(int amount)
            : base(CraftResource.Heartwood, amount)
        {
        }

        public HeartwoodBoard(Serial serial)
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

    public class BloodwoodBoard : BaseWoodBoard
    {
        [Constructable]
        public BloodwoodBoard()
            : this(1)
        {
        }

        [Constructable]
        public BloodwoodBoard(int amount)
            : base(CraftResource.Bloodwood, amount)
        {
        }

        public BloodwoodBoard(Serial serial)
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

    public class FrostwoodBoard : BaseWoodBoard
    {
        [Constructable]
        public FrostwoodBoard()
            : this(1)
        {
        }

        [Constructable]
        public FrostwoodBoard(int amount)
            : base(CraftResource.Frostwood, amount)
        {
        }

        public FrostwoodBoard(Serial serial)
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

    public class OakBoard : BaseWoodBoard
    {
        [Constructable]
        public OakBoard()
            : this(1)
        {
        }

        [Constructable]
        public OakBoard(int amount)
            : base(CraftResource.OakWood, amount)
        {
        }

        public OakBoard(Serial serial)
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

    public class AshBoard : BaseWoodBoard
    {
        [Constructable]
        public AshBoard()
            : this(1)
        {
        }

        [Constructable]
        public AshBoard(int amount)
            : base(CraftResource.AshWood, amount)
        {
        }

        public AshBoard(Serial serial)
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

    public class YewBoard : BaseWoodBoard
    {
        [Constructable]
        public YewBoard()
            : this(1)
        {
        }

        [Constructable]
        public YewBoard(int amount)
            : base(CraftResource.YewWood, amount)
        {
        }

        public YewBoard(Serial serial)
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
