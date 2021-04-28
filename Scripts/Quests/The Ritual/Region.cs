#region

using Server.Items;
using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Seventh;
using Server.Spells.Sixth;

#endregion

namespace Server.Regions
{
    public class QueenOakwhisperRegion : BaseRegion
    {
        private static readonly Rectangle2D[] m_Bounds =
        {
            new Rectangle2D(835, 2764, 32, 31)
        };

        public static void Initialize()
        {
            new QueenOakwhisperRegion();

            Map map = Map.TerMur;

            if (map.FindItem<Teleporter>(new Point3D(856, 2783, 5)) == null)
            {
                var tele = new Teleporter
                {
                    PointDest = new Point3D(4446, 3692, 0),
                    MapDest = Map.Trammel
                };
                tele.MoveToWorld(new Point3D(856, 2783, 5), map);
            }

            if (map.FindItem<Static>(new Point3D(856, 2783, 5)) == null)
            {
                var st = new Static(0x375A)
                {
                    Weight = 0
                };
                st.MoveToWorld(new Point3D(856, 2783, 5), map);
            }
        }

        public QueenOakwhisperRegion()
            : base("Queen Oakwhisper Region", Map.TerMur, DefaultPriority, m_Bounds)
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
