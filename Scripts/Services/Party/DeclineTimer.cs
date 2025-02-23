using System;
using System.Collections;

namespace Server.Engines.PartySystem
{
    public class DeclineTimer : Timer
    {
        private static readonly Hashtable _Table = new Hashtable();
        private readonly Mobile _Mobile;
        private readonly Mobile _Leader;

        private DeclineTimer(Mobile m, Mobile leader)
            : base(TimeSpan.FromSeconds(30.0))
        {
            _Mobile = m;
            _Leader = leader;
        }

        public static void Start(Mobile m, Mobile leader)
        {
            DeclineTimer t = (DeclineTimer)_Table[m];

            if (t != null)
            {
                t.Stop();
            }

            _Table[m] = t = new DeclineTimer(m, leader);
            t.Start();
        }

        protected override void OnTick()
        {
            _Table.Remove(_Mobile);

            if (_Mobile.Party == _Leader && PartyCommands.Handler != null)
            {
                PartyCommands.Handler.OnDecline(_Mobile, _Leader);
            }
        }
    }
}
