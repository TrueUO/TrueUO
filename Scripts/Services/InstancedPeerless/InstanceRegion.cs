using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using System;

namespace Server.Engines.InstancedPeerless
{
    public class InstanceRegion : Region
    {
        private readonly PeerlessInstance m_Owner;

        public PeerlessInstance Owner => m_Owner;

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (s is RecallSpell || s is SacredJourneySpell)
            {
                m.SendLocalizedMessage(501802); // Thy spell doth not appear to work...
                return false;
            }

            return base.OnBeginSpellCast(m, s);
        }

        public InstanceRegion(PeerlessInstance instance)
            : base(null, instance.Map, Find(instance.EntranceLocation, instance.Map), instance.RegionBounds)
        {
            m_Owner = instance;

            Register();
        }

        public override TimeSpan GetLogoutDelay(Mobile m)
        {
            return TimeSpan.FromMinutes(10.0);
        }

        public static void OnLogout(Mobile m)
        {
            if (m.Region is InstanceRegion region)
            {
                region.Owner.Kick(m);
            }
        }

        public override void OnExit(Mobile m)
        {
            m_Owner.RemoveFighter(m);
        }
    }
}
