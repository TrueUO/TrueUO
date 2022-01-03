using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Poker
{
	public class PokerJoinGump : Gump
	{
		private readonly PokerGame m_Game;

		public PokerJoinGump(Mobile from, PokerGame game)
			: base(50, 50)
		{
			m_Game = game;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);

			AddImageTiled(18, 15, 350, 320, 9274);
			AddAlphaRegion(23, 19, 340, 310);
			AddLabel(133, 25, 1149, "Join Poker Table");
			AddImageTiled(42, 47, 301, 3, 96);
			AddLabel(65, 62, 154, "You are about to join a game of Poker.");
			AddImage(33, 38, 95);
			AddImage(342, 38, 97);
			AddLabel(52, 80, 154, "All bets involve real gold and no refunds will be");
			AddLabel(54, 98, 154, "given. If you feel uncomfortable losing gold or");
			AddLabel(46, 116, 154, "are unfamiliar with the rules of Texas Hold'em, you");
			AddLabel(100, 134, 154, "are advised against proceeding.");

			AddLabel(122, 161, 1149, "Small Blind:");
			AddLabel(129, 181, 1149, "Big Blind:");
			AddLabel(123, 201, 1149, "Min Buy-In:");
			AddLabel(120, 221, 1149, "Max Buy-In:");
			AddLabel(110, 241, 1149, "Bank Balance:");
			AddLabel(101, 261, 1149, "Buy-In Amount:");

			AddLabel(200, 161, 148, m_Game.Dealer.SmallBlind.ToString("#,###") + "gp");
			AddLabel(200, 181, 148, m_Game.Dealer.BigBlind.ToString("#,###") + "gp");
			AddLabel(200, 201, 148, m_Game.Dealer.MinBuyIn.ToString("#,###") + "gp");
			AddLabel(200, 221, 148, m_Game.Dealer.MaxBuyIn.ToString("#,###") + "gp");

			int balance = Banker.GetBalance(from);
			int balancehue = 31;
			int layout = 0;

			if (balance >= m_Game.Dealer.MinBuyIn)
			{
				balancehue = 266;
				layout = 1;
			}

			AddLabel(200, 241, balancehue, balance.ToString("#,###") + "gp");

			if (layout == 0)
			{
				AddLabel(200, 261, 1149, "(not enough gold)");
				AddButton(163, 292, 242, 241, (int)Handlers.btnCancel, GumpButtonType.Reply, 0);
			}
			else if (layout == 1)
			{
				AddImageTiled(200, 261, 80, 19, 0xBBC);
				AddAlphaRegion(200, 261, 80, 19);
				AddTextEntry(203, 261, 77, 19, 1149, (int)Handlers.txtBuyInAmount, m_Game.Dealer.MinBuyIn.ToString());
				AddButton(123, 292, 247, 248, (int)Handlers.btnOkay, GumpButtonType.Reply, 0);
				AddButton(200, 292, 242, 241, (int)Handlers.btnCancel, GumpButtonType.Reply, 0);
			}
		}

		public enum Handlers
		{
			btnOkay = 1,
			btnCancel,
			txtBuyInAmount
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			Mobile from = state.Mobile;

			int buyInAmount = 0;

			if (info.ButtonID == 1)
			{
				int balance = Banker.GetBalance(from);

				if (balance >= m_Game.Dealer.MinBuyIn)
				{
					try
					{
						buyInAmount = Convert.ToInt32(info.TextEntries[0].Text);
					}
					catch
					{
						from.SendMessage(0x22, "Use numbers without commas to input your buy-in amount (ie 25000)");
						return;
					}

					if (buyInAmount <= balance && buyInAmount >= m_Game.Dealer.MinBuyIn && buyInAmount <= m_Game.Dealer.MaxBuyIn)
					{
						PokerPlayer player = new PokerPlayer(from);

						player.Gold = buyInAmount;
						m_Game.AddPlayer(player);
					}
					else
                    {
                        from.SendMessage(0x22, "You may not join with that amount of gold. Minimum buy-in: " + Convert.ToString(m_Game.Dealer.MinBuyIn) + ", Maximum buy-in: " + Convert.ToString(m_Game.Dealer.MaxBuyIn));
                    }
                }
				else
                {
                    from.SendMessage(0x22, "You may not join with that amount of gold. Minimum buy-in: " + Convert.ToString(m_Game.Dealer.MinBuyIn) + ", Maximum buy-in: " + Convert.ToString(m_Game.Dealer.MaxBuyIn));
                }
            }
			else if (info.ButtonID == 2)
            {
                return;
            }
        }
	}
}
