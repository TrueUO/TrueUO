using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Regions
{
    public class WellOfSoulsRegion : Region
    {
        public static void Initialize()
        {
            new WellOfSoulsRegion();
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

        public WellOfSoulsRegion()
            : base("Well Of Souls", Map.Ilshenar, DefaultPriority, m_Bounds.Keys.ToArray())
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
