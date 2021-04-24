using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;

namespace Server.Engines.ArenaSystem
{
    [PropertyObject]
    public class PVPArena
    {
        private static readonly TimeSpan PendingDuelExpirationTime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan BookedDuelBegin = TimeSpan.FromSeconds(10);

        public static int StartRank = 10000;

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaManager Manager { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaStone Stone { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaExitBanner Banner1 { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaExitBanner Banner2 { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaDefinition Definition { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool InUse => CurrentDuel != null;

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaDuel CurrentDuel { get; set; }

        public ArenaRegion Region { get; set; }
        public GuardedRegion GuardRegion { get; set; }

        public Dictionary<ArenaDuel, DateTime> PendingDuels { get; }
        public List<ArenaDuel> BookedDuels { get; }
        public List<Item> Blockers { get; }

        public List<ArenaStats> TeamRankings { get; }
        public List<ArenaStats> SurvivalRankings { get; }

        public PVPArena(ArenaDefinition definition)
        {
            Definition = definition;

            PendingDuels = new Dictionary<ArenaDuel, DateTime>();
            BookedDuels = new List<ArenaDuel>();
            Blockers = new List<Item>();

            TeamRankings = new List<ArenaStats>();
            SurvivalRankings = new List<ArenaStats>();
        }

        public override string ToString()
        {
            return "...";
        }

        public void ConfigureArena()
        {
            if (Manager == null)
            {
                Manager = new ArenaManager(this);
                Manager.MoveToWorld(Definition.ManagerLocation, Definition.Map);
            }

            if (Stone == null)
            {
                Stone = new ArenaStone(this);
                Stone.MoveToWorld(Definition.StoneLocation, Definition.Map);
            }

            if (Banner1 == null)
            {
                Banner1 = new ArenaExitBanner(Definition.BannerID1, this);
                Banner1.MoveToWorld(Definition.BannerLocation1, Definition.Map);
            }

            if (Banner2 == null)
            {
                Banner2 = new ArenaExitBanner(Definition.BannerID2, this);
                Banner2.MoveToWorld(Definition.BannerLocation2, Definition.Map);
            }

            if (Region == null)
            {
                Region = new ArenaRegion(this);
                Region.Register();
            }

            if (GuardRegion == null)
            {
                GuardRegion = new GuardedArenaRegion($"{Definition.Name}_Guarded", Definition.Map, Definition.GuardBounds);
                GuardRegion.Register();
            }
        }

        public void Unregister()
        {
            if (Region != null)
            {
                Region.Unregister();
                Region = null;
            }

            if (GuardRegion != null)
            {
                GuardRegion.Unregister();
                GuardRegion = null;
            }

            Manager?.Delete();

            if (Stone != null && !Stone.Deleted)
            {
                Stone.Delete();
            }

            Banner1?.Delete();

            Banner2?.Delete();
        }

        private readonly List<ArenaDuel> _Remove = new List<ArenaDuel>();

        public void OnTick()
        {
            CurrentDuel?.OnTick();

            foreach (KeyValuePair<ArenaDuel, DateTime> kvp in PendingDuels)
            {
                if (kvp.Value < DateTime.UtcNow)
                {
                    _Remove.Add(kvp.Key);
                }
            }

            if (_Remove.Count > 0)
            {
                for (var index = 0; index < _Remove.Count; index++)
                {
                    ArenaDuel duel = _Remove[index];

                    if (PendingDuels.ContainsKey(duel))
                    {
                        PendingDuels.Remove(duel);
                    }
                }

                _Remove.Clear();
            }
        }

        public void AddPendingDuel(ArenaDuel duel)
        {
            if (!PendingDuels.ContainsKey(duel))
            {
                PendingDuels[duel] = DateTime.UtcNow + PendingDuelExpirationTime;
            }
        }

        public void RemovePendingDuel(ArenaDuel duel, bool cancel = false)
        {
            if (PendingDuels.ContainsKey(duel))
            {
                PendingDuels.Remove(duel);

                if (cancel)
                {
                    PVPArenaSystem.SendParticipantMessage(duel, 1115947); // The session owner has canceled the duel.
                }
            }
        }

        public ArenaDuel GetPendingDuel(Mobile m)
        {
            foreach (var d in PendingDuels.Keys)
            {
                if (m is PlayerMobile mobile && d.IsParticipant(mobile))
                {
                    return d;
                }
            }

            return null;
        }

        public List<ArenaDuel> GetPendingPublic()
        {
            List<ArenaDuel> list = new List<ArenaDuel>();

            foreach (var d in PendingDuels.Keys)
            {
                if (d.RoomType == RoomType.Public && d.ParticipantCount < d.Entries)
                {
                    list.Add(d);
                }
            }

            return list;
        }

        public void TryBeginDuel(ArenaDuel duel)
        {
            if (PendingDuels.ContainsKey(duel))
            {
                PendingDuels.Remove(duel);
            }

            if (CurrentDuel == null && BookedDuels.Count == 0)
            {
                CurrentDuel = duel;
                duel.DoPreDuel();
            }
            else
            {
                BookedDuels.Add(duel);
                PVPArenaSystem.SendParticipantMessage(duel, 1115960); // There are currently no open arenas. Your duel session has been added to the booking queue.
            }
        }

        public void OnDuelEnd(ArenaDuel duel)
        {
            CurrentDuel = null;

            foreach (Item item in Region.GetEnumeratedItems())
            {
                if (item is Corpse corpse)
                {
                    if (corpse.Owner != null && corpse.Owner.InRange(corpse.Location, 30))
                    {
                        corpse.MoveToWorld(corpse.Owner.Location, corpse.Owner.Map);
                    }
                    else
                    {
                        corpse.MoveToWorld(GetRandomRemovalLocation(), Definition.Map);
                    }
                }
            }

            if (BookedDuels.Count > 0)
            {
                ArenaDuel newDuel = BookedDuels[0];
                CurrentDuel = newDuel;

                PVPArenaSystem.SendParticipantMessage(newDuel, 1153141); // Your session has been booked. Please wait a few moments to start the fight.

                Timer.DelayCall(BookedDuelBegin, () =>
                    {
                        newDuel.DoPreDuel();
                    });

                BookedDuels.Remove(newDuel);
            }
        }

        public void RemovePlayer(Mobile m, bool winner = false)
        {
            Map map = Definition.Map;
            Point3D p = GetRandomRemovalLocation(m);

            m.MoveToWorld(p, map);
            m.Delta(MobileDelta.Noto);

            // lets remove pets, too
            if (m is PlayerMobile mobile && mobile.AllFollowers.Count > 0)
            {
                for (var index = 0; index < mobile.AllFollowers.Count; index++)
                {
                    Mobile mob = mobile.AllFollowers[index];

                    if (mob.Region.IsPartOf<ArenaRegion>())
                    {
                        mob.MoveToWorld(p, map);
                        mob.Delta(MobileDelta.Noto);
                    }
                }
            }

            if (winner)
            {
                for (int i = 0; i < 5; i++)
                {
                    Timer.DelayCall(TimeSpan.FromMilliseconds(i * 1000), () =>
                        {
                            m.FixedParticles(0x373A, 10, 15, 5018, 0x36, 0, EffectLayer.Waist);
                        });
                }
            }

            if (!m.Alive)
            {
                IPooledEnumerable eable = map.GetMobilesInRange(m.Location, 5);

                foreach (Mobile mob in eable)
                {
                    if (mob is ArenaManager manager)
                    {
                        manager.OfferResurrection(m);
                        break;
                    }
                }

                eable.Free();
            }
        }

        private Point3D GetRandomRemovalLocation(Mobile m = null)
        {
            Rectangle2D rec = m == null || m.Alive ? Definition.EjectLocation : Definition.DeadEjectLocation;
            Point3D loc = m == null || m.Alive ? Definition.StoneLocation : Definition.ManagerLocation;
            Point3D p = loc;

            Map map = Definition.Map;

            while (p == loc || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                p = map.GetRandomSpawnPoint(rec);

                if (m == null || m.Alive)
                    p.Z = Definition.StoneLocation.Z;
                else
                    p.Z = Definition.ManagerLocation.Z;
            }

            return p;
        }

        public void RecordRankings(ArenaDuel duel, ArenaTeam winners)
        {
            List<ArenaStats> rankings;

            rankings = duel.BattleMode == BattleMode.Team ? TeamRankings : SurvivalRankings;

            foreach (KeyValuePair<PlayerMobile, PlayerStatsEntry> part in duel.GetParticipants())
            {
                PlayerMobile pm = part.Key;
                ArenaStats stats = null;

                for (var index = 0; index < rankings.Count; index++)
                {
                    var r = rankings[index];

                    if (r.Owner == pm)
                    {
                        stats = r;
                        break;
                    }
                }

                if (stats == null)
                {
                    stats = new ArenaStats(pm);
                    rankings.Add(stats);
                }

                ArenaTeam team = duel.GetTeam(pm);

                if (team != winners)
                {
                    stats.Ranking -= 33;
                }
                else
                {
                    stats.Ranking += 33;
                }
            }

            rankings.Sort();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(SurvivalRankings.Count);
            for (var index = 0; index < SurvivalRankings.Count; index++)
            {
                ArenaStats ranking = SurvivalRankings[index];
                ranking.Serialize(writer);
            }

            writer.Write(TeamRankings.Count);
            for (var index = 0; index < TeamRankings.Count; index++)
            {
                ArenaStats ranking = TeamRankings[index];
                ranking.Serialize(writer);
            }

            writer.Write(Blockers.Count);
            for (var index = 0; index < Blockers.Count; index++)
            {
                Item blocker = Blockers[index];
                writer.Write(blocker);
            }

            writer.Write(Stone);
            writer.Write(Manager);
            writer.Write(Banner1);
            writer.Write(Banner2);

            writer.Write(PendingDuels.Count);
            foreach (KeyValuePair<ArenaDuel, DateTime> kvp in PendingDuels)
            {
                kvp.Key.Serialize(writer);
                writer.WriteDeltaTime(kvp.Value);
            }

            writer.Write(BookedDuels.Count);
            for (var index = 0; index < BookedDuels.Count; index++)
            {
                ArenaDuel duel = BookedDuels[index];
                duel.Serialize(writer);
            }

            if (CurrentDuel != null)
            {
                writer.Write(1);
                CurrentDuel.Serialize(writer);
            }
            else
            {
                writer.Write(0);
            }
        }

        public void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                ArenaStats stats = new ArenaStats(reader);

                if (stats.Owner != null)
                    SurvivalRankings.Add(stats);
            }

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                ArenaStats stats = new ArenaStats(reader);

                if (stats.Owner != null)
                    TeamRankings.Add(stats);
            }

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Item blocker = reader.ReadItem();

                if (blocker != null)
                    Blockers.Add(blocker);
            }

            Stone = reader.ReadItem() as ArenaStone;
            Manager = reader.ReadMobile() as ArenaManager;
            Banner1 = reader.ReadItem() as ArenaExitBanner;
            Banner2 = reader.ReadItem() as ArenaExitBanner;

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                ArenaDuel duel = new ArenaDuel(reader, this);
                DateTime dt = reader.ReadDeltaTime();

                PendingDuels[duel] = dt;
            }

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                BookedDuels.Add(new ArenaDuel(reader, this));
            }

            if (reader.ReadInt() == 1)
            {
                CurrentDuel = new ArenaDuel(reader, this);
            }

            if (Stone != null)
                Stone.Arena = this;

            if (Manager != null)
                Manager.Arena = this;

            if (Banner1 != null)
                Banner1.Arena = this;

            if (Banner2 != null)
                Banner2.Arena = this;

            if (version == 0)
            {
                for (var index = 0; index < Blockers.Count; index++)
                {
                    var blocker = Blockers[index];
                    blocker?.Delete();
                }

                ColUtility.Free(Blockers);
            }
        }
    }

    public class ArenaStats : IComparable<ArenaStats>
    {
        public PlayerMobile Owner { get; }
        public int Ranking { get; set; }

        public ArenaStats(PlayerMobile pm)
        {
            Owner = pm;
            Ranking = 10000;
        }

        public int CompareTo(ArenaStats stats)
        {
            if (Ranking > stats.Ranking)
                return -1;

            if (Ranking < stats.Ranking)
                return 1;

            return 0;
        }

        public ArenaStats(GenericReader reader)
        {
            int version = reader.ReadInt();

            Owner = reader.ReadMobile() as PlayerMobile;
            Ranking = reader.ReadInt();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Owner);
            writer.Write(Ranking);
        }
    }
}
