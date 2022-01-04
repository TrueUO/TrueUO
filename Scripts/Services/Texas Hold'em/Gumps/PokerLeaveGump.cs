using Server.Gumps;
using Server.Network;

namespace Server.Poker
{
	public class PokerLeaveGump : Gump
	{
		private readonly PokerGame m_Game;

		public PokerLeaveGump(Mobile from, PokerGame game)
			: base(50, 50)
		{
			m_Game = game;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);

			AddImageTiled(18, 15, 350, 180, 9274);
            AddBackground(0, 0, 390, 200, 9270);
			AddLabel(133, 25, 28, "Leave Poker Table");
			AddImageTiled(42, 47, 301, 3, 96);
			AddLabel(60, 62, 68, "You are about to leave a game of Poker.");
			AddImage(33, 38, 95, 68);
			AddImage(342, 38, 97, 68);
			AddLabel(43, 80, 68, "Are you sure you want to cash-out and leave the");
			AddLabel(48, 98, 68, "table? You will auto fold, and any current bets");
			AddLabel(40, 116, 68, "will be lost. Winnings will be deposited in your bank.");
			AddButton(163, 155, 247, 248, (int)Handlers.btnOkay, GumpButtonType.Reply, 0);
		}

		public enum Handlers
		{
			None,
			btnOkay
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;

			if (from == null)
            {
                return;
            }

            PokerPlayer player = m_Game.GetPlayer(from);

			if (player != null && info.ButtonID == 1)
			{
                if (m_Game.State == PokerGameState.Inactive)
                {
                    if (m_Game.Players.Contains(player))
                    {
                        m_Game.RemovePlayer(player);
                    }

                    return;
                }


                if (player.RequestLeave)
                {
                    from.SendMessage(0x22, "You have already submitted a request to leave.");
                }
                else
                {
                    from.SendMessage(0x22, "You have submitted a request to leave the table.");
                    player.RequestLeave = true;
                }
            }
		}
	}
}
