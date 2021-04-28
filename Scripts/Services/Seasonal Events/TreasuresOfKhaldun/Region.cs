#region

using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using Server.Spells.Sixth;

#endregion

namespace Server.Regions
{
    public class GumshoeRegion : BaseRegion
    {
        private static readonly Rectangle2D[] m_Bounds =
        {
            new Rectangle2D(6154, 2877, 18, 30),
            new Rectangle2D(6198, 2883, 23, 24),
            new Rectangle2D(6239, 2873, 26, 26),
            new Rectangle2D(6283, 2868, 31, 29)
        };

        public static void Initialize()
        {
            new GumshoeRegion();
        }

        public GumshoeRegion()
            : base("Gumshoe Region", Map.Felucca, DefaultPriority, m_Bounds)
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

                if (s is GateTravelSpell || s is RecallSpell || s is SacredJourneySpell)
                {
                    m.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
                    return false;
                }
            }

            return base.OnBeginSpellCast(m, s);
        }
    }
}
