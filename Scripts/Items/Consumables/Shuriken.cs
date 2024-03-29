using Server.Engines.Craft;
using System;

namespace Server.Items
{
    [Flipable(0x27AC, 0x27F7)]
    public class Shuriken : Item, ICraftable, INinjaAmmo
    {
        private int m_UsesRemaining;
        private Poison m_Poison;
        private int m_PoisonCharges;
        [Constructable]
        public Shuriken()
            : this(1)
        {
        }

        [Constructable]
        public Shuriken(int amount)
            : base(0x27AC)
        {
            Weight = 1.0;

            m_UsesRemaining = amount;
        }

        public Shuriken(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get => m_UsesRemaining;
            set
            {
                m_UsesRemaining = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison Poison
        {
            get => m_Poison;
            set
            {
                m_Poison = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonCharges
        {
            get => m_PoisonCharges;
            set
            {
                m_PoisonCharges = value;
                InvalidateProperties();
            }
        }

        public bool ShowUsesRemaining { get => true; set { } }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~

            if (m_Poison != null && m_PoisonCharges > 0)
            {
                if (m_Poison == Poison.Parasitic)
                {
                    list.Add(1072852, m_PoisonCharges.ToString()); // parasitic poison charges: ~1_val~
                }
                else if (m_Poison == Poison.DarkGlow)
                {
                    list.Add(1072853, m_PoisonCharges.ToString()); // darkglow poison charges: ~1_val~
                }
                else
                {
                    list.Add(1062412 + m_Poison.Level, m_PoisonCharges.ToString());
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_UsesRemaining);
            Poison.Serialize(m_Poison, writer);
            writer.Write(m_PoisonCharges);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_UsesRemaining = reader.ReadInt();
            m_Poison = Poison.Deserialize(reader);
            m_PoisonCharges = reader.ReadInt();
        }

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            if (quality == 2)
                UsesRemaining *= 2;

            return quality;
        }
    }
}
