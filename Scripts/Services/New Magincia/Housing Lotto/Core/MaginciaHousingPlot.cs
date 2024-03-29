using Server.Accounting;
using System;
using System.Collections.Generic;

namespace Server.Engines.NewMagincia
{
    [PropertyObject]
    public class MaginciaHousingPlot
    {
        private readonly string m_Identifier;
        private WritOfLease m_Writ;
        private Rectangle2D m_Bounds;
        private MaginciaPlotStone m_Stone;
        private readonly bool m_IsPrimeSpot;
        private bool m_Complete;
        private Mobile m_Winner;
        private readonly Map m_Map;
        private DateTime m_Expires;

        [CommandProperty(AccessLevel.GameMaster)]
        public string Identifier => m_Identifier;

        [CommandProperty(AccessLevel.GameMaster)]
        public WritOfLease Writ { get => m_Writ; set => m_Writ = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Bounds => m_Bounds;

        [CommandProperty(AccessLevel.GameMaster)]
        public MaginciaPlotStone Stone { get => m_Stone; set => m_Stone = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsPrimeSpot => m_IsPrimeSpot;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Complete => m_Complete;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Winner { get => m_Winner; set => m_Winner = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map Map => m_Map;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime Expires { get => m_Expires; set => m_Expires = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D RecallLoc => new Point3D(m_Bounds.X, m_Bounds.Y, m_Map.GetAverageZ(m_Bounds.X, m_Bounds.Y));

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsAvailable => !m_Complete;

        #region Lotto Info
        private DateTime m_LottoEnds;
        private readonly Dictionary<Mobile, int> m_Participants = new Dictionary<Mobile, int>();

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LottoEnds { get => m_LottoEnds; set => m_LottoEnds = value; }

        public Dictionary<Mobile, int> Participants => m_Participants;

        [CommandProperty(AccessLevel.GameMaster)]
        public int LottoPrice => m_IsPrimeSpot ? 10000 : 2000;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool LottoOngoing => IsAvailable && m_LottoEnds > DateTime.UtcNow && m_LottoEnds != DateTime.MinValue;
        #endregion

        public MaginciaHousingPlot(string identifier, Rectangle2D bounds, bool prime, Map map)
        {
            m_Identifier = identifier;
            m_Bounds = bounds;
            m_IsPrimeSpot = prime;
            m_Map = map;
            m_Writ = null;
            m_Complete = false;
            m_Expires = DateTime.MinValue;
        }

        public void AddPlotStone()
        {
            AddPlotStone(MaginciaLottoSystem.GetPlotStoneLoc(this));
        }

        public void AddPlotStone(Point3D p)
        {
            m_Stone = new MaginciaPlotStone
            {
                Plot = this
            };
            m_Stone.MoveToWorld(p, m_Map);
        }

        public override string ToString()
        {
            return "...";
        }

        public bool CanPurchaseLottoTicket(Mobile from)
        {
            if (m_IsPrimeSpot)
            {
                Account acct = from.Account as Account;

                if (acct == null)
                    return false;

                return CheckAccount(acct);
            }

            return true;
        }

        private bool CheckAccount(Account acct)
        {
            for (int i = 0; i < acct.Length; i++)
            {
                Mobile m = acct[i];

                if (m == null)
                    continue;

                foreach (MaginciaHousingPlot plot in MaginciaLottoSystem.Plots)
                {
                    if (plot.IsPrimeSpot && plot.Participants.ContainsKey(m))
                        return false;
                }
            }

            return true;
        }

        public void PurchaseLottoTicket(Mobile from, int toBuy)
        {
            if (m_Participants.ContainsKey(from))
                m_Participants[from] += toBuy;
            else
                m_Participants[from] = toBuy;
        }

        public void EndLotto()
        {
            if (m_Participants.Count == 0)
            {
                ResetLotto();
                return;
            }

            List<Mobile> raffle = new List<Mobile>();

            foreach (KeyValuePair<Mobile, int> kvp in m_Participants)
            {
                if (kvp.Value == 0)
                    continue;

                for (int i = 0; i < kvp.Value; i++)
                    raffle.Add(kvp.Key);
            }

            Mobile winner = raffle[Utility.Random(raffle.Count)];

            if (winner != null)
                OnLottoComplete(winner);
            else
                ResetLotto();

            m_Participants.Clear();
        }

        public void OnLottoComplete(Mobile winner)
        {
            m_Complete = true;
            m_Winner = winner;
            m_LottoEnds = DateTime.MinValue;
            m_Expires = DateTime.UtcNow + TimeSpan.FromDays(MaginciaLottoSystem.WritExpirePeriod);

            if (winner.HasGump(typeof(PlotWinnerGump)))
                return;

            Account acct = winner.Account as Account;

            if (acct == null)
                return;

            for (int i = 0; i < acct.Length; i++)
            {
                Mobile m = acct[i];

                if (m == null)
                    continue;

                if (m.NetState != null)
                {
                    winner.SendGump(new PlotWinnerGump(this));
                    break;
                }
            }
        }

        public void SendMessage_Callback(object o)
        {
            if (o is object[] obj)
            {
                Mobile winner = obj[0] as Mobile;
                NewMaginciaMessage message = obj[1] as NewMaginciaMessage;

                MaginciaLottoSystem.SendMessageTo(winner, message);
            }
        }

        public void ResetLotto()
        {
            if (MaginciaLottoSystem.AutoResetLotto && MaginciaLottoSystem.Instance != null)
                m_LottoEnds = DateTime.UtcNow + MaginciaLottoSystem.Instance.LottoDuration;
            else
                m_LottoEnds = DateTime.MinValue;
        }

        public MaginciaHousingPlot(GenericReader reader)
        {
            int version = reader.ReadInt();

            m_Identifier = reader.ReadString();
            m_Writ = reader.ReadItem() as WritOfLease;
            m_Stone = reader.ReadItem() as MaginciaPlotStone;
            m_LottoEnds = reader.ReadDateTime();
            m_Bounds = reader.ReadRect2D();
            m_Map = reader.ReadMap();
            m_IsPrimeSpot = reader.ReadBool();
            m_Complete = reader.ReadBool();
            m_Winner = reader.ReadMobile();
            m_Expires = reader.ReadDateTime();

            int c = reader.ReadInt();
            for (int i = 0; i < c; i++)
            {
                Mobile m = reader.ReadMobile();
                int amount = reader.ReadInt();

                if (m != null)
                    m_Participants[m] = amount;
            }

            if ((m_Stone == null || m_Stone.Deleted) && LottoOngoing && MaginciaLottoSystem.IsRegisteredPlot(this))
                AddPlotStone();
            else if (m_Stone != null)
                m_Stone.Plot = this;

            if (m_Writ != null)
                m_Writ.Plot = this;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(m_Identifier);
            writer.Write(m_Writ);
            writer.Write(m_Stone);
            writer.Write(m_LottoEnds);
            writer.Write(m_Bounds);
            writer.Write(m_Map);
            writer.Write(m_IsPrimeSpot);
            writer.Write(m_Complete);
            writer.Write(m_Winner);
            writer.Write(m_Expires);

            writer.Write(m_Participants.Count);

            foreach (KeyValuePair<Mobile, int> kvp in m_Participants)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }
    }
}
