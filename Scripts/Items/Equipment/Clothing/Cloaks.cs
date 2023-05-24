namespace Server.Items
{
    public abstract class BaseCloak : BaseClothing
    {
        public BaseCloak(int itemID)
            : this(itemID, 0)
        {
        }

        public BaseCloak(int itemID, int hue)
            : base(itemID, Layer.Cloak, hue)
        {
        }

        public BaseCloak(Serial serial)
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

    [Flipable]
    public class Cloak : BaseCloak, IArcaneEquip
    {
        #region Arcane Impl
        private int m_MaxArcaneCharges, m_CurArcaneCharges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxArcaneCharges
        {
            get => m_MaxArcaneCharges;
            set
            {
                m_MaxArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurArcaneCharges
        {
            get => m_CurArcaneCharges;
            set
            {
                m_CurArcaneCharges = value;
                InvalidateProperties();
                Update();
            }
        }

        public int TempHue { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsArcane => m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0;

        public void Update()
        {
            if (IsArcane)
                ItemID = 0x26AD;
            else if (ItemID == 0x26AD)
                ItemID = 0x1515;

            if (IsArcane && CurArcaneCharges == 0)
            {
                TempHue = Hue;
                Hue = 0;
            }
        }

        public override void AddCraftedProperties(ObjectPropertyList list)
        {
            base.AddCraftedProperties(list);

            if (IsArcane)
                list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
        }

        public void Flip()
        {
            if (ItemID == 0x1515)
                ItemID = 0x1530;
            else if (ItemID == 0x1530)
                ItemID = 0x1515;
        }
        #endregion

        [Constructable]
        public Cloak()
            : this(0)
        {
        }

        [Constructable]
        public Cloak(int hue)
            : base(0x1515, hue)
        {
            Weight = 5.0;
        }

        public Cloak(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2); // version

            if (IsArcane)
            {
                writer.Write(true);
                writer.Write(TempHue);
                writer.Write(m_CurArcaneCharges);
                writer.Write(m_MaxArcaneCharges);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        if (reader.ReadBool())
                        {
                            TempHue = reader.ReadInt();
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();
                        }

                        break;
                    }
                case 1:
                    {
                        if (reader.ReadBool())
                        {
                            m_CurArcaneCharges = reader.ReadInt();
                            m_MaxArcaneCharges = reader.ReadInt();
                        }

                        break;
                    }
            }
        }
    }

    [Flipable(0x230A, 0x2309)]
    public class FurCape : BaseCloak
    {
        [Constructable]
        public FurCape()
            : this(0)
        {
        }

        [Constructable]
        public FurCape(int hue)
            : base(0x230A, hue)
        {
            Weight = 4.0;
        }

        public FurCape(Serial serial)
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
