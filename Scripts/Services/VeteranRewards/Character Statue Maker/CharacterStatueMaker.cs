using Server.Engines.VeteranRewards;
using Server.Mobiles;

namespace Server.Items
{
    public class CharacterStatueMaker : Item
    {
        private StatueType m_Type;

        [Constructable]
        public CharacterStatueMaker(StatueType type)
            : base(0x32F0)
        {
            m_Type = type;

            InvalidateHue();

            Weight = 5.0;
        }

        public CharacterStatueMaker(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1076173;// Character Statue Maker
        
        [CommandProperty(AccessLevel.GameMaster)]
        public StatueType StatueType
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
                InvalidateHue();
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                if (!from.IsBodyMod)
                {
                    from.SendLocalizedMessage(1076194); // Select a place where you would like to put your statue.
                    from.Target = new CharacterStatueTarget(this, m_Type);
                }
                else
                {
                    from.SendLocalizedMessage(1073648); // You may only proceed while in your original state...
                }
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version

            writer.Write((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();

            m_Type = (StatueType)reader.ReadInt();
        }

        public void InvalidateHue()
        {
            Hue = 0xB8F + (int)m_Type * 4;
        }
    }

    public class MarbleStatueMaker : CharacterStatueMaker
    {
        [Constructable]
        public MarbleStatueMaker()
            : base(StatueType.Marble)
        {
        }

        public MarbleStatueMaker(Serial serial)
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

            int version = reader.ReadEncodedInt();
        }
    }

    public class JadeStatueMaker : CharacterStatueMaker
    {
        [Constructable]
        public JadeStatueMaker()
            : base(StatueType.Jade)
        {
        }

        public JadeStatueMaker(Serial serial)
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

            int version = reader.ReadEncodedInt();
        }
    }

    public class BronzeStatueMaker : CharacterStatueMaker
    {
        [Constructable]
        public BronzeStatueMaker()
            : base(StatueType.Bronze)
        {
        }

        public BronzeStatueMaker(Serial serial)
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

            int version = reader.ReadEncodedInt();
        }
    }
}
