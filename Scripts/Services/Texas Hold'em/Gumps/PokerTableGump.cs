using System;
using Server.Gumps;

namespace Server.Poker
{
	public class PokerTableGump : Gump
	{
        private const int CARD_X = 300;
		private const int CARD_Y = 270;

        private const int COLOR_GOLD = 0xFFD700;
        private const int COLOR_GREEN = 0x00FF00;
		private const int COLOR_OFF_WHITE = 0xFFFACD;
        private const int COLOR_PINK = 0xFF0099;

		private readonly PokerGame m_Game;
		private readonly PokerPlayer m_Player;

		public PokerTableGump(PokerGame game, PokerPlayer player)
			: base(0, 0)
		{
			m_Game = game;
			m_Player = player;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);

			if (m_Game.State > PokerGameState.PreFlop)
            {
                DrawCards();
            }

            DrawPlayers();

			if (m_Game.State > PokerGameState.Inactive)
            {
                AddLabel(350, 340, 148, "Pot: " + m_Game.CommunityGold.ToString("#,###"));
            }
        }

		private void DrawPlayers()
		{
			int RADIUS = 240;
			int centerX = CARD_X + (m_Game.State < PokerGameState.Turn ? 15 : m_Game.State < PokerGameState.River ? 30 : 45);
			int centerY = CARD_Y + RADIUS;

			if (m_Game.State > PokerGameState.DealHoleCards)
			{
				int lastX = centerX;
				int lastY = centerY - 85;

				for (int i = 0; i < m_Player.HoleCards.Count; ++i)
				{
					AddBackground(lastX, lastY, 71, 95, 9350);
					AddLabelCropped(lastX + 10, lastY + 5, 80, 60, m_Player.HoleCards[i].GetSuitColor(), m_Player.HoleCards[i].GetRankLetter());
					AddLabelCropped(lastX + 6, lastY + 25, 75, 60, m_Player.HoleCards[i].GetSuitColor(), m_Player.HoleCards[i].GetSuitString());

					lastX += 30;
				}
			}

			int playerIndex = m_Game.GetIndexFor(m_Player.Mobile);
			int counter = m_Game.Players.Count - 1;

			for (double i = playerIndex + 1; counter >= 0; ++i)
			{
				if (i == m_Game.Players.Count)
                {
                    i = 0;
                }

                PokerPlayer current = m_Game.Players[(int)i];

				double xdist = RADIUS * Math.Sin(counter * 2.0 * Math.PI / m_Game.Players.Count);
				double ydist = RADIUS * Math.Cos(counter * 2.0 * Math.PI / m_Game.Players.Count);

				int x = centerX + (int)xdist;
				int y = CARD_Y + (int)ydist;

				AddBackground(x, y, 101, 65, 9270); //This is the gump that shows your name and gold left.

				if (current.HasBlindBet || current.HasDealerButton)
                {
                    AddHtml(x, y - 15, 101, 45, Color(Center(current.HasBigBlind ? "(Big Blind)" : current.HasSmallBlind ? "(Small Blind)" : "(Dealer Button)"), COLOR_GREEN), false, false); 
                }

                AddHtml(x, y + 5, 101, 45, Color(Center(current.Mobile.Name), m_Game.Players.Peek() == current ? COLOR_GREEN : !m_Game.Players.Round.Contains(current) ? COLOR_OFF_WHITE : COLOR_PINK), false, false);
				AddHtml(x + 2, y + 24, 101, 45, Color(Center("(" + current.Gold.ToString("#,###") + ")"), COLOR_GOLD), false, false);

				--counter;
			}
		}

		private void DrawCards()
		{
			int lastX = CARD_X;
			int lastY = CARD_Y;

			for (int i = 0; i < m_Game.CommunityCards.Count; ++i)
			{
				AddBackground(lastX, lastY, 71, 95, 9350);
				AddLabelCropped(lastX + 10, lastY + 5, 80, 60, m_Game.CommunityCards[i].GetSuitColor(), m_Game.CommunityCards[i].GetRankLetter());
				AddLabelCropped(lastX + 6, lastY + 25, 75, 60, m_Game.CommunityCards[i].GetSuitColor(), m_Game.CommunityCards[i].GetSuitString());

				lastX += 30;
			}
		}

		private string Center(string text)
		{
			return string.Format("<CENTER>{0}</CENTER>", text);
		}

		private string Color(string text, int color)
		{
			return string.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}
	}
}
