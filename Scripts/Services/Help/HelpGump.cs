using Server.Gumps;
using Server.Network;
using System;

namespace Server.Engines.Help
{
    public class HelpGump : Gump
    {
        public HelpGump(Mobile from)
            : base(0, 0)
        {
            from.CloseGump(typeof(HelpGump));

            AddBackground(50, 25, 540, 520, 2600);

            AddPage(0);

            AddHtml(150, 50, 360, 40, "<CENTER><U>Help System</U></CENTER>", false, false); 
            AddButton(425, 415, 2073, 2072, 0, GumpButtonType.Reply, 0); // Close

            AddPage(1);

            AddButton(80, 90, 5540, 5541, 1, GumpButtonType.Reply, 2);
            AddHtml(110, 90, 450, 70, "<u>SHARD RULES.</u> Selecting this option will open a link to the shard and community rules page.", true, true);

            AddButton(80, 170, 5540, 5541, 2, GumpButtonType.Reply, 2);
            AddHtml(110, 170, 450, 70, "<u>JOIN DISCORD CHAT CHANNEL.</u> Selecting this option will open the official TrueUO/Heritage Discord chat channel.", true, true);

            AddButton(80, 250, 5540, 5541, 3, GumpButtonType.Reply, 3);
            AddHtml(110, 250, 450, 70, "<u>REPORT A BUG.</u> Selecting this option will open a link to the Heritage bug reporting system.", true, true);

            AddButton(80, 330, 5540, 5541, 5, GumpButtonType.Reply, 0);
            AddHtml(110, 330, 450, 70, "<u>CHARACTER IS STUCK.</u> Using this option will teleport you to a pre-selected town. This option is not available in all locations.", true, true);

            AddButton(80, 410, 5540, 5541, 6, GumpButtonType.Reply, 3);
            AddHtml(110, 410, 450, 70, "<u>OTHER - OPEN A SUPPORT TICKET.</u> This option will redirect you to the Heritage ticket system. Coming soon!", true, true);
        }

        public static void Initialize()
        {
            EventSink.HelpRequest += EventSink_HelpRequest;
        }

        public static bool CheckCombat(Mobile m)
        {
            for (int i = 0; i < m.Aggressed.Count; ++i)
            {
                AggressorInfo info = m.Aggressed[i];

                if (DateTime.UtcNow - info.LastCombatTime < TimeSpan.FromSeconds(30.0))
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            switch (info.ButtonID)
            {
                case 0: // Close/Cancel
                    {
                        from.SendLocalizedMessage(501235, "", 0x35); // Help request aborted.
                        break;
                    }
                case 1: // Shard Rules
                    {
                        from.LaunchBrowser("https://trueuo.com/threads/heritage-shard-rules.85/#post-263");
                        break;
                    }
                case 2: // Join Discord
                    {
                        from.LaunchBrowser("https://discord.gg/spjepdC");
                        break;
                    }
                case 3: // Report a bug
                    {
                        from.LaunchBrowser("https://trueuo.com/forums/bug-reports-reporting.3/post-thread");
                        break;
                    }
                case 5: // Stuck
                {
                    /*
                    BaseHouse house = BaseHouse.FindHouseAt(from);

                    if (house != null)
                    {
                        from.Location = house.BanLocation;
                    }
                    else if (from.Region.IsPartOf<Regions.Jail>())
                    {
                        from.SendLocalizedMessage(1114345, "", 0x35); // You'll need a better jailbreak plan than that!
                    }
                    else if (CityLoyalty.CityTradeSystem.HasTrade(from))
                    {
                        from.SendLocalizedMessage(1151733); // You cannot do that while carrying a Trade Order.
                    }
                    else if (from.Region.CanUseStuckMenu(from) && !CheckCombat(from) && !from.Frozen && !from.Criminal)
                    {
                        StuckMenu menu = new StuckMenu(from, from, true);

                        menu.BeginClose();

                        from.SendGump(menu);
                    }
                    else
                    {
                        from.SendMessage("The stuck option can not be used from this location.");
                    }
                    */ // Disable stuck for now.
                    break;
                }
                case 6: // Ticket System
                {
                    from.SendMessage("Feature coming soon.");
                    break;
                }
            }
        }

        private static void EventSink_HelpRequest(HelpRequestEventArgs e)
        {
            for (var index = 0; index < e.Mobile.NetState.Gumps.Count; index++)
            {
                Gump g = e.Mobile.NetState.Gumps[index];

                if (g is HelpGump)
                {
                    return;
                }
            }

            e.Mobile.SendGump(new HelpGump(e.Mobile));
        }
    }
}
