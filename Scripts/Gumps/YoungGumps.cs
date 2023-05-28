using Server.Accounting;
using Server.Network;

namespace Server.Gumps
{
    public class RenounceYoungGump : Gump
    {
        public RenounceYoungGump()
            : base(150, 50)
        {
            AddBackground(0, 0, 450, 400, 0xA28);

            AddHtmlLocalized(0, 30, 450, 35, 1013004, false, false); // <center> Renouncing 'Young Player' Status</center>

            /* As a 'Young' player, you are currently under a system of protection that prevents
            * you from being attacked by other players and certain monsters.<br><br>
            * 
            * If you choose to renounce your status as a 'Young' player, you will lose this protection.
            * You will become vulnerable to other players, and many monsters that had only glared
            * at you menacingly before will now attack you on sight!<br><br>
            * 
            * Select OKAY now if you wish to renounce your status as a 'Young' player, otherwise
            * press CANCEL.
            */
            AddHtmlLocalized(30, 70, 390, 210, 1013005, true, true);

            AddButton(45, 298, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(78, 300, 100, 35, 1011036, false, false); // OKAY

            AddButton(178, 298, 0xFA5, 0xFA7, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(211, 300, 100, 35, 1011012, false, false); // CANCEL
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (info.ButtonID == 1)
            {
                if (from.Account is Account acc)
                {
                    acc.RemoveYoungStatus(502085); // You have chosen to renounce your `Young' player status.
                }
            }
            else
            {
                from.SendLocalizedMessage(502086); // You have chosen not to renounce your `Young' player status.
            }
        }
    }
}
