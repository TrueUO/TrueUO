using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class ReportMurdererGump : Gump
    {
        private readonly List<Mobile> m_Killers;
        private int m_Idx;

        public ReportMurdererGump(List<Mobile> killers)
            : this(killers, 0)
        {
        }

        private ReportMurdererGump(List<Mobile> killers, int idx)
            : base(0, 0)
        {
            m_Killers = killers;
            m_Idx = idx;
            BuildGump();
        }

        public static void OnPlayerDeath(Mobile m)
        {
            List<Mobile> killers = new List<Mobile>();
            List<Mobile> toGive = new List<Mobile>();

            for (var index = 0; index < m.Aggressors.Count; index++)
            {
                AggressorInfo ai = m.Aggressors[index];

                if (ai.Attacker.Player && ai.CanReportMurder && !ai.Reported)
                {
                    if (!((PlayerMobile) m).RecentlyReported.Contains(ai.Attacker))
                    {
                        if (!killers.Contains(ai.Attacker))
                        {
                            killers.Add(ai.Attacker);
                            ai.Reported = true;
                            ai.CanReportMurder = false;
                        }
                    }
                }

                if (ai.Attacker.Player && DateTime.UtcNow - ai.LastCombatTime < TimeSpan.FromSeconds(30.0) && !toGive.Contains(ai.Attacker))
                {
                    toGive.Add(ai.Attacker);
                }
            }

            for (var index = 0; index < m.Aggressed.Count; index++)
            {
                AggressorInfo ai = m.Aggressed[index];

                if (ai.Defender.Player && DateTime.UtcNow - ai.LastCombatTime < TimeSpan.FromSeconds(30.0) && !toGive.Contains(ai.Defender))
                {
                    toGive.Add(ai.Defender);
                }
            }

            for (var index = 0; index < toGive.Count; index++)
            {
                Mobile g = toGive[index];

                int n = Notoriety.Compute(g, m);

                int theirKarma = m.Karma, ourKarma = g.Karma;

                bool innocent = n == Notoriety.Innocent;
                bool criminal = n == Notoriety.Criminal || n == Notoriety.Murderer;

                int fameAward = m.Fame / 200;
                int karmaAward = 0;

                if (innocent)
                {
                    karmaAward = (ourKarma > -2500 ? -850 : -110 - (m.Karma / 100));
                }
                else if (criminal)
                {
                    karmaAward = 50;
                }

                Titles.AwardFame(g, fameAward, false);
                Titles.AwardKarma(g, karmaAward, true);
            }

            if (m is PlayerMobile mobile && mobile.NpcGuild == NpcGuild.ThievesGuild)
            {
                return;
            }

            if (killers.Count > 0)
            {
                new GumpTimer(m, killers).Start();
            }
        }

        public static void ReportedListExpiry_Callback(object state)
        {
            object[] states = (object[])state;

            PlayerMobile from = (PlayerMobile)states[0];
            Mobile killer = (Mobile)states[1];

            from.RecentlyReported.Remove(killer);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            switch (info.ButtonID)
            {
                case 1:
                    {
                        Mobile killer = m_Killers[m_Idx];
                        if (killer != null && !killer.Deleted)
                        {
                            killer.Kills++;
                            killer.ShortTermMurders++;

                            ((PlayerMobile)from).RecentlyReported.Add(killer);
                            Timer.DelayCall(TimeSpan.FromMinutes(10), ReportedListExpiry_Callback, new object[] { from, killer });

                            if (killer is PlayerMobile pk)
                            {
                                pk.ResetKillTime();
                                pk.SendLocalizedMessage(1049067);//You have been reported for murder!

                                if (pk.Kills == 5)
                                {
                                    pk.SendLocalizedMessage(502134);//You are now known as a murderer!

                                    CheckMurderer(pk);
                                }
                                else if (SkillHandlers.Stealing.SuspendOnMurder && pk.Kills == 1 && pk.NpcGuild == NpcGuild.ThievesGuild)
                                {
                                    pk.SendLocalizedMessage(501562); // You have been suspended by the Thieves Guild.
                                }
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        break;
                    }
            }

            m_Idx++;

            if (m_Idx < m_Killers.Count)
            {
                from.SendGump(new ReportMurdererGump(m_Killers, m_Idx));
            }
        }

        public static void CheckMurderer(Mobile m)
        {
            if (m.AccessLevel == AccessLevel.Player && m.Murderer && SpellHelper.RestrictRedTravel && m.Map != null && m.Map.Rules != MapRules.FeluccaRules)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                {
                    if (m.NetState != null)
                    {
                        m.MoveToWorld(Map.Felucca.GetSpawnPosition(new Point3D(1458, 844, 5), 5), Map.Felucca);
                        m.SendLocalizedMessage(1005524, "", 0x22); // Murderers aren't allowed here, you are banished!
                    }
                });
            }
        }

        private void BuildGump()
        {
            AddBackground(265, 205, 320, 290, 5054);
            Closable = false;
            Resizable = false;

            AddPage(0);

            AddImageTiled(225, 175, 50, 45, 0xCE);   //Top left corner
            AddImageTiled(267, 175, 315, 44, 0xC9);  //Top bar
            AddImageTiled(582, 175, 43, 45, 0xCF);   //Top right corner
            AddImageTiled(225, 219, 44, 270, 0xCA);  //Left side
            AddImageTiled(582, 219, 44, 270, 0xCB);  //Right side
            AddImageTiled(225, 489, 44, 43, 0xCC);   //Lower left corner
            AddImageTiled(267, 489, 315, 43, 0xE9);  //Lower Bar
            AddImageTiled(582, 489, 43, 43, 0xCD);   //Lower right corner

            AddPage(1);

            AddHtml(260, 234, 300, 140, m_Killers[m_Idx].Name, false, false); // Player's Name
            AddHtmlLocalized(260, 254, 300, 140, 1049066, false, false); // Would you like to report...

            AddButton(260, 300, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(300, 300, 300, 50, 1046362, false, false); // Yes

            AddButton(360, 300, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0);
            AddHtmlLocalized(400, 300, 300, 50, 1046363, false, false); // No
        }

        private class GumpTimer : Timer
        {
            private readonly Mobile m_Victim;
            private readonly List<Mobile> m_Killers;
            public GumpTimer(Mobile victim, List<Mobile> killers)
                : base(TimeSpan.FromSeconds(4.0))
            {
                m_Victim = victim;
                m_Killers = killers;
            }

            protected override void OnTick()
            {
                m_Victim.SendGump(new ReportMurdererGump(m_Killers));
            }
        }
    }
}
