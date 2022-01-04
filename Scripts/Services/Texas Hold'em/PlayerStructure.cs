using System.Collections.Generic;

namespace Server.Poker
{
	public class PlayerStructure
	{
        private readonly PokerGame m_Game;

		public List<PokerPlayer> Players { get; set; }
        public List<PokerPlayer> Round { get; set; }
        public List<PokerPlayer> Turn { get; set; }

        public PokerGame Game => m_Game;

        public int Count => Players.Count;

        public PokerPlayer this[int index]
        {
			get => Players[index]; set => Players[index] = value;
        }

		public PlayerStructure(PokerGame game)
		{
			Players = new List<PokerPlayer>();
			Round = new List<PokerPlayer>();
			Turn = new List<PokerPlayer>();
			m_Game = game;
		}

		public void Push(PokerPlayer player)
		{
			if (!Turn.Contains(player))
            {
                Turn.Add(player);
            }
        }

		public PokerPlayer Peek()
		{
			return Turn.Count > 0 ? Turn[Turn.Count - 1] : null;
		}

		public PokerPlayer Prev()
		{
			return Turn.Count > 1 ? Turn[Turn.Count - 2] : null;
		}

		public PokerPlayer Next()
		{
			if (Round.Count == 1 || Turn.Count == Round.Count)
            {
                return null;
            }

            if (Peek() == null) //No turns yet for this round
			{
				if (m_Game.State == PokerGameState.PreFlop)
				{
					PokerPlayer blind = m_Game.BigBlind == null ? m_Game.SmallBlind : m_Game.BigBlind;

					if (blind != null)
                    {
                        return Round.IndexOf(blind) == Round.Count - 1 ? Round[0] : Round[Round.IndexOf(blind) + 1];
                    }
                }

				PokerPlayer dealer = m_Game.DealerButton;

				if (dealer == null)
                {
                    return null;
                }

                return Round.IndexOf(dealer) == Round.Count - 1 ? Round[0] : Round[Round.IndexOf(dealer) + 1];
			}

			return Round.IndexOf(Peek() ) == Round.Count - 1 ? Round[0] : Round[Round.IndexOf(Peek()) + 1];
		}

		public bool Contains(PokerPlayer player) { return Players.Contains(player); }

		public void Clear()
		{
			Turn.Clear();
			Round.Clear();
		}
	}
}
