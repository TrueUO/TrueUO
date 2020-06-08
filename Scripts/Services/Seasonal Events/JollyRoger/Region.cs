using Server.Network;
using System.Collections.Generic;
using System.Linq;
using Server.Spells.Seventh;
using Server.Spells.Sixth;

namespace Server.Regions
{
    public class WellOfSoulsRegion : Region
    {
        public static void Initialize()
        {
            new WellOfSoulsRegion();
        }

        private static Rectangle2D _Bound = new Rectangle2D(2246, 1537, 36, 40);

        public WellOfSoulsRegion()
            : base("Well Of Souls", Map.Ilshenar, DefaultPriority, _Bound)
        {
            Register();
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (m.IsPlayer())
            {
                if (s is MarkSpell)
                {
                    m.SendLocalizedMessage(501802); // Thy spell doth not appear to work...
                    return false;
                }

                if (s is GateTravelSpell)
                {
                    m.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
                    return false;
                }
            }

            return base.OnBeginSpellCast(m, s);
        }
    }

    public class WellOfSoulsVirtuesRegion : Region
    {
        public static void Initialize()
        {
            new WellOfSoulsVirtuesRegion();
        }

        private static Rectangle2D m_Spirituality = new Rectangle2D(2262, 1561, 4, 4);
        private static Rectangle2D m_Compassion = new Rectangle2D(2248, 1557, 4, 4);
        private static Rectangle2D m_Honor = new Rectangle2D(2248, 1547, 4, 4);        
        private static Rectangle2D m_Honesty = new Rectangle2D(2255, 1541, 4, 4);
        private static Rectangle2D m_Humility = new Rectangle2D(2262, 1539, 4, 4);
        private static Rectangle2D m_Justice = new Rectangle2D(2269, 1541, 4, 4);
        private static Rectangle2D m_Valor = new Rectangle2D(2276, 1547, 4, 4);
        private static Rectangle2D m_Sacrifice = new Rectangle2D(2276, 1557, 4, 4);

        private static readonly Dictionary<Rectangle2D, string> m_Bounds = new Dictionary<Rectangle2D, string>()
        {
            { m_Spirituality, "Spiritual" },
            { m_Compassion, "Compassionate" },
            { m_Honor,"Honorable" },
            { m_Honesty,"Honest" },
            { m_Humility,"Humble" },
            { m_Justice,"Just" },
            { m_Valor,"Valiant" },
            { m_Sacrifice, "Sacrificing" }
        };

        public WellOfSoulsVirtuesRegion()
            : base("Well Of Souls Virtues", Map.Ilshenar, DefaultPriority, m_Bounds.Keys.ToArray())
        {
            Register();
        }

        public override void OnEnter(Mobile m)
        {
            string s = "";

            foreach (var st in m_Bounds.Where(st => st.Key.Contains(m.Location)))
            {
                s = st.Value;
            }

            if (!string.IsNullOrEmpty(s))
            {
                m.PrivateOverheadMessage(MessageType.Regular, 0x47E, false, string.Format("*Thou are not truly {0}...*", s), m.NetState);
            }
        }
    }
}
