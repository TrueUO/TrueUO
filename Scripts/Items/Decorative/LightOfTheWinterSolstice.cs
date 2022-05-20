namespace Server.Items
{
    [Flipable(0x236E, 0x2371)]
    public class LightOfTheWinterSolstice : Item
    {
        private static readonly string[] m_StaffNames =
        {
            "Aenima",
            "Alkiser",
            "ASayre",
            "David",
            "Krrios",
            "Mark",
            "Merlin",
            "Merlix", 
            "Phantom",
            "Phenos",
            "psz",
            "Ryan",
            "Quantos",
            "Outkast", 
            "V", 
            "Zippy"
        };

        private string m_Dipper;

        [Constructable]
        public LightOfTheWinterSolstice()
            : this(m_StaffNames[Utility.Random(m_StaffNames.Length)])
        {
        }

        [Constructable]
        public LightOfTheWinterSolstice(string dipper)
            : base(0x236E)
        {
            m_Dipper = dipper;

            Weight = 1.0;
            Light = LightType.Circle300;
            Hue = Utility.RandomDyedHue();
        }

        public LightOfTheWinterSolstice(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Dipper { get => m_Dipper; set => m_Dipper = value; }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1070881, m_Dipper); // Hand Dipped by ~1_name~
            list.Add(1070880); // Winter 2004
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Dipper);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Dipper = reader.ReadString();

            if (m_Dipper != null)
            {
                m_Dipper = string.Intern(m_Dipper);
            }
        }
    }
}
