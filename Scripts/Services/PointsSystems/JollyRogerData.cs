using Server.Engines.JollyRoger;
using Server.Engines.SeasonalEvents;
using Server.Mobiles;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Points
{
    public class JollyRogerData : PointsSystem
    {
        public static List<RewardArray> _List = new List<RewardArray>();

        public override PointsType Loyalty => PointsType.JollyRogerData;
        public override TextDefinition Name => "Jolly Roger";
        public override bool AutoAdd => true;
        public override double MaxPoints => double.MaxValue;
        public override bool ShowOnLoyaltyGump => false;

        public bool InSeason => SeasonalEventSystem.IsActive(EventType.JollyRoger);

        public bool Enabled { get; set; }
        public bool QuestContentGenerated { get; set; }

        public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
        {
        }

        public static void AddReward(Mobile m, Shrine shrine)
        {
            var list = _List.FirstOrDefault(x => x.Mobile == m);

            if (list != null && list.Shrine != null)
            {
                if (list.Shrine.Any(y => y.Shrine == shrine))
                {
                    _List.FirstOrDefault(x => x.Mobile == m).Shrine.FirstOrDefault(y => y.Shrine == shrine).MasterDeath++;
                }
                else
                {
                    _List.FirstOrDefault(x => x.Mobile == m).Shrine.Add(new ShrineArray(shrine, 1));
                }
            }
            else
            {
                var sa = new List<ShrineArray>
                {
                    new ShrineArray(shrine, 1)
                };

                var ra = new RewardArray(m, sa);

                _List.Add(ra);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Enabled);
            writer.Write(QuestContentGenerated);

            writer.Write(_List.Count);

            _List.ForEach(l =>
            {
                writer.Write(l.Mobile);
                writer.Write(l.Tabard);
                writer.Write(l.Cloak);

                writer.Write(l.Shrine.Count);

                l.Shrine.ForEach(s =>
                {
                    writer.Write((int)s.Shrine);
                    writer.Write(s.MasterDeath);
                });
            });
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    Enabled = reader.ReadBool();
                    QuestContentGenerated = reader.ReadBool();

                    int count = reader.ReadInt();

                    for (int i = count; i > 0; i--)
                    {
                        Mobile m = reader.ReadMobile();
                        bool t = reader.ReadBool();
                        bool c = reader.ReadBool();

                        var temp = new List<ShrineArray>();

                        int sc = reader.ReadInt();

                        for (int s = sc; s > 0; s--)
                        {
                            Shrine sh = (Shrine)reader.ReadInt();
                            int md = reader.ReadInt();

                            temp.Add(new ShrineArray(sh, md));
                        }

                        if (m != null)
                        {
                            _List.Add(new RewardArray(m, temp, t, c));
                        }
                    }
                    break;
            }
        }
    }

    public class RewardArray
    {
        public Mobile Mobile { get; set; }
        public List<ShrineArray> Shrine { get; set; }
        public bool Tabard { get; set; }
        public bool Cloak { get; set; }

        public RewardArray(Mobile m, List<ShrineArray> s)
        {
            Mobile = m;
            Shrine = s;
        }

        public RewardArray(Mobile m, List<ShrineArray> s, bool tabard, bool cloak)
        {
            Mobile = m;
            Shrine = s;
            Tabard = tabard;
            Cloak = cloak;
        }
    }

    public class ShrineArray
    {
        public Shrine Shrine { get; set; }
        public int MasterDeath { get; set; }

        public ShrineArray(Shrine s, int c)
        {
            Shrine = s;
            MasterDeath = c;
        }
    }
}
