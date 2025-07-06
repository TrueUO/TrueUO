using Server.Mobiles;
using Server.Spells;

namespace Server.Items
{
    public class SpellScroll : Item, ICommodity
    {
        private int m_SpellID;
        public SpellScroll(Serial serial)
            : base(serial)
        {
        }

        [Constructable]
        public SpellScroll(int spellID, int itemID)
            : this(spellID, itemID, 1)
        {
        }

        [Constructable]
        public SpellScroll(int spellID, int itemID, int amount)
            : base(itemID)
        {
            Stackable = true;
            Weight = 1.0;
            Amount = amount;

            m_SpellID = spellID;
        }

        public int SpellID => m_SpellID;
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_SpellID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_SpellID = reader.ReadInt();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Multis.DesignContext.Check(from))
            {
                return; // They are customizing
            }

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            if (from.Flying && from is PlayerMobile && BaseMount.OnFlightPath(from))
            {
                from.SendLocalizedMessage(1113749); // You may not use that while flying over such precarious terrain.
                return;
            }

            Spell spell = SpellRegistry.NewSpell(m_SpellID, from, this);

            if (spell != null)
            {
                spell.Cast();
            }
            else
            {
                from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
            }
        }
    }
}
