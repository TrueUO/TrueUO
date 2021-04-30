using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;
using System.Collections.Generic;

namespace Server.Items
{
    public class GargishDocumentBook : BaseLocalizedBook, ISecurable
    {
        private SecureLevel m_Level;

        public override int[] Contents => new int[] { };

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level { get => m_Level; set => m_Level = value; }

        public GargishDocumentBook()
        {
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Title is int iTitle)
                list.Add(1150928, string.Format("#{0}", iTitle)); // Gargish Document - ~1_NAME~
            else if (Title is string sTitle)
                list.Add(1150928, sTitle);
            else
                base.AddNameProperty(list);
        }

        public GargishDocumentBook(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)m_Level);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Level = (SecureLevel)reader.ReadInt();
        }
    }

    public class GargishDocumentNote : Note
    {
        public override int[] Contents => new int[] { };
        public virtual int Title => 0;

        public GargishDocumentNote()
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1150928, string.Format("#{0}", Title)); // Gargish Document - ~1_NAME~
        }

        public GargishDocumentNote(Serial serial)
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

    public class ChallengeRite : GargishDocumentBook
    {
        public override object Title => 1150904; // The Challenge Rite
        public override object Author => 1113299; // <center>(unknown)</center>
        public override int[] Contents => new[] { 1150915, 1150916, 1150917, 1150918, 1150919, 1150920, 1150921, 1150922 };

        [Constructable]
        public ChallengeRite()
        {
            Hue = 2505;
        }

        public ChallengeRite(Serial serial)
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

    public class OnTheVoid : GargishDocumentBook
    {
        public override object Title => 1150907; // On the Void
        public override object Author => 1150892; // Prugyilonus
        public override int[] Contents => new[] { 1150894, 1150895, 1150896 };

        [Constructable]
        public OnTheVoid()
        {
            Hue = 2587;
        }

        public OnTheVoid(Serial serial)
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

    public class InMemory : GargishDocumentBook
    {
        public override object Title => 1150913; // In Memory
        public override object Author => 1150898; // Zhah
        public override int[] Contents => new[] { 1151071, 1151072, 1151073 };

        [Constructable]
        public InMemory()
        {
            Hue = 2550;
        }

        public InMemory(Serial serial)
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

    public class ChronicleOfTheGargoyleQueen1 : GargishDocumentBook
    {
        public static void Initialize()
        {
            for (int i = 0; i < 34; i++)
            {
                if (i == 0)
                    m_Contents[i] = 1150901;
                else
                {
                    m_Contents[i] = 1150943 + (i - 1);
                }
            }
        }

        private static readonly int[] m_Contents = new int[34];

        public override object Title => 1150914; // Chronicle of the Gargoyle Queen Vol. 1
        public override object Author => 1151074; // Queen Zhah
        public override int[] Contents => m_Contents;

        private int m_Charges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get => m_Charges; set { m_Charges = value; InvalidateProperties(); } }

        [Constructable]
        public ChronicleOfTheGargoyleQueen1()
        {
            Hue = 2576;
            m_Charges = 500;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1153098, m_Charges.ToString());
        }

        public ChronicleOfTheGargoyleQueen1(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_Charges);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Charges = reader.ReadInt();
        }
    }

    public class AnthenaeumDecree : GargishDocumentNote
    {
        public override int Title => 1150905; // Athenaeum Decree

        public override int[] Contents => new[] { 1150891 };

        [Constructable]
        public AnthenaeumDecree()
        {
            Hue = 2515;
        }

        public AnthenaeumDecree(Serial serial)
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

    public class LetterFromTheKing : GargishDocumentNote
    {
        public override int Title => 1150906; // A Letter from the King

        public override int[] Contents => new[] { 1150923, 1150924 };

        [Constructable]
        public LetterFromTheKing()
        {
            Hue = 2514;
        }

        public LetterFromTheKing(Serial serial)
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

    public class ShilaxrinarsMemorial : GargishDocumentNote
    {
        public override int Title => 1150908;  // Shilaxrinar's Memorial

        public override int[] Contents => new[] { 1150899, 1150900 };

        [Constructable]
        public ShilaxrinarsMemorial()
        {
            Hue = 2515;
        }

        public ShilaxrinarsMemorial(Serial serial)
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

    public class ToTheHighScholar : GargishDocumentNote
    {
        public override int Title => 1150909;  // To the High Scholar

        public override int[] Contents => new[] { 1151062, 1151063 };

        [Constructable]
        public ToTheHighScholar()
        {
            Hue = 2514;
        }

        public ToTheHighScholar(Serial serial)
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

    public class ToTheHighBroodmother : GargishDocumentNote
    {
        public override int Title => 1150910;  // To the High Broodmother

        public override int[] Contents => new[] { 1151064, 1151065 };

        [Constructable]
        public ToTheHighBroodmother()
        {
            Hue = 2514;
        }

        public ToTheHighBroodmother(Serial serial)
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

    public class ReplyToTheHighScholar : GargishDocumentNote
    {
        public override int Title => 1150911;  // Reply to the High Scholar

        public override int[] Contents => new[] { 1151066, 1151067, 1151068 };

        [Constructable]
        public ReplyToTheHighScholar()
        {
            Hue = 2514;
        }

        public ReplyToTheHighScholar(Serial serial)
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

    public class AccessToTheIsle : GargishDocumentNote
    {
        public override int Title => 1150912; // Access to the Isle

        public override int[] Contents => new[] { 1151069 };

        [Constructable]
        public AccessToTheIsle()
        {
            Hue = 2515;
        }

        public AccessToTheIsle(Serial serial)
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
}
