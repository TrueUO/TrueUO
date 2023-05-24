namespace Server.Items
{
    public abstract class BaseOuterTorso : BaseClothing
    {
        public BaseOuterTorso(int itemID)
            : this(itemID, 0)
        {
        }

        public BaseOuterTorso(int itemID, int hue)
            : base(itemID, Layer.OuterTorso, hue)
        {
        }

        public BaseOuterTorso(Serial serial)
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

    [Flipable(0x230E, 0x230D)]
    public class GildedDress : BaseOuterTorso
    {
        [Constructable]
        public GildedDress()
            : this(0)
        {
        }

        [Constructable]
        public GildedDress(int hue)
            : base(0x230E, hue)
        {
            Weight = 3.0;
        }

        public GildedDress(Serial serial)
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

    [Flipable(0x1F00, 0x1EFF)]
    public class FancyDress : BaseOuterTorso
    {
        [Constructable]
        public FancyDress()
            : this(0)
        {
        }

        [Constructable]
        public FancyDress(int hue)
            : base(0x1F00, hue)
        {
            Weight = 3.0;
        }

        public FancyDress(Serial serial)
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
    public class Robe : BaseOuterTorso, IArcaneEquip
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
                ItemID = 0x26AE;
            else if (ItemID == 0x26AE)
                ItemID = 0x1F04;

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
            if (ItemID == 0x1F03)
                ItemID = 0x1F04;
            else if (ItemID == 0x1F04)
                ItemID = 0x1F03;
        }

        #endregion

        [Constructable]
        public Robe()
            : this(0)
        {
        }

        [Constructable]
        public Robe(int hue)
            : base(0x1F03, hue)
        {
            Weight = 3.0;
        }

        public Robe(Serial serial)
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

    [Flipable(0x1f01, 0x1f02)]
    public class PlainDress : BaseOuterTorso
    {
        [Constructable]
        public PlainDress()
            : this(0)
        {
        }

        [Constructable]
        public PlainDress(int hue)
            : base(0x1F01, hue)
        {
            Weight = 2.0;
        }

        public PlainDress(Serial serial)
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

    [Flipable(0x2799, 0x27E4)]
    public class Kamishimo : BaseOuterTorso
    {
        [Constructable]
        public Kamishimo()
            : this(0)
        {
        }

        [Constructable]
        public Kamishimo(int hue)
            : base(0x2799, hue)
        {
            Weight = 3.0;
        }

        public Kamishimo(Serial serial)
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

    [Flipable(0x279C, 0x27E7)]
    public class HakamaShita : BaseOuterTorso
    {
        [Constructable]
        public HakamaShita()
            : this(0)
        {
        }

        [Constructable]
        public HakamaShita(int hue)
            : base(0x279C, hue)
        {
            Weight = 3.0;
        }

        public HakamaShita(Serial serial)
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

    [Flipable(0x2782, 0x27CD)]
    public class MaleKimono : BaseOuterTorso
    {
        [Constructable]
        public MaleKimono()
            : this(0)
        {
        }

        [Constructable]
        public MaleKimono(int hue)
            : base(0x2782, hue)
        {
            Weight = 3.0;
        }

        public override bool AllowFemaleWearer => false;

        public MaleKimono(Serial serial)
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

    [Flipable(0x2783, 0x27CE)]
    public class FemaleKimono : BaseOuterTorso
    {
        [Constructable]
        public FemaleKimono()
            : this(0)
        {
        }

        [Constructable]
        public FemaleKimono(int hue)
            : base(0x2783, hue)
        {
            Weight = 3.0;
        }

        public override bool AllowMaleWearer => false;

        public FemaleKimono(Serial serial)
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

    [Flipable(0x2FB9, 0x3173)]
    public class MaleElvenRobe : BaseOuterTorso
    {
        [Constructable]
        public MaleElvenRobe()
            : this(0)
        {
        }

        [Constructable]
        public MaleElvenRobe(int hue)
            : base(0x2FB9, hue)
        {
            Weight = 2.0;
        }

        public MaleElvenRobe(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    [Flipable(0x2FBA, 0x3174)]
    public class FemaleElvenRobe : BaseOuterTorso
    {
        [Constructable]
        public FemaleElvenRobe()
            : this(0)
        {
        }

        [Constructable]
        public FemaleElvenRobe(int hue)
            : base(0x2FBA, hue)
        {
            Weight = 2.0;
        }

        public override bool AllowMaleWearer => false;

        public FemaleElvenRobe(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class FloweredDress : BaseOuterTorso
    {
        public override int LabelNumber => 1109622;  // Flowered Dress

        [Constructable]
        public FloweredDress()
            : this(0)
        {
        }

        [Constructable]
        public FloweredDress(int hue)
            : base(0x781E, hue)
        {
        }

        public FloweredDress(Serial serial)
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

    public class EveningGown : BaseOuterTorso
    {
        public override int LabelNumber => 1109625;  // Evening Gown

        [Constructable]
        public EveningGown()
            : this(0)
        {
        }

        [Constructable]
        public EveningGown(int hue)
            : base(0x7821, hue)
        {
        }

        public EveningGown(Serial serial)
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

    public class Epaulette : BaseClothing
    {
        public override int LabelNumber => 1123325;  // Epaulette

        [Constructable]
        public Epaulette()
            : this(0)
        {
        }

        [Constructable]
        public Epaulette(int hue)
            : base(0x9985, Layer.OuterTorso, hue)
        {
            Weight = 1.0;
        }

        public Epaulette(Serial serial)
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
