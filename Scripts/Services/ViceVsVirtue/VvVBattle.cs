using Server.Guilds;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using System;
using System.Collections.Generic;

namespace Server.Engines.VvV
{
    public enum UpdateType
    {
        Kill,
        Assist,
        Steal,
        TurnInVice,
        TurnInVirtue,
        Disarm
    }

    [PropertyObject]
    public class VvVBattle
    {
        public static readonly int Duration = 30;
        public static readonly int Cooldown = 5;
        public static readonly int Announcement = 2;
        public static readonly int KillCooldownDuration = 5;
        public static readonly int MaxTraps = 20;
        public static readonly int MaxTurrets = 10;

        //Battle Scoring
        public static readonly double ScoreToWin = 10000;
        public static readonly double OccupyPoints = 300;
        public static readonly double AltarPoints = 1000;
        public static readonly double KillPoints = 600;
        public static readonly double TurnInPoints = 500;

        public static readonly int AltarSilver = 100;
        public static readonly int TurnInSilver = 100;
        public static readonly int KillSilver = 50;
        public static readonly int WinSilver = 100;
        public static readonly int DisarmSilver = 50;

        // silver penalty in the event the battle is uncontested
        public static readonly double Penalty = 0.5;

        [CommandProperty(AccessLevel.GameMaster)]
        public double SilverPenalty
        {
            get
            {
                if (!ViceVsVirtueSystem.EnhancedRules)
                    return 1.0;

                return UnContested ? Penalty : 1.0;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ViceVsVirtueSystem System { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime StartTime { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime CooldownEnds { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastOccupationCheck { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextSigilSpawn { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextAnnouncement { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextAltarActivate { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextManaSpike { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime ManaSpikeEndEffects { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public VvVCity City { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public VvVSigil Sigil { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool OnGoing { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UnContested { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Region Region
        {
            get
            {
                CityInfo info = CityInfo.Infos[City];

                if (info != null)
                {
                    return info.Region;
                }

                return null;
            }

        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceStart
        {
            get => false;
            set
            {
                if (!OnGoing && value)
                    Begin();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ForceEnd
        {
            get => false;
            set
            {
                if (OnGoing && value)
                    EndBattle();
            }
        }

        public List<BattleTeam> Teams { get; set; }

        public Dictionary<Mobile, DateTime> KillCooldown { get; set; }
        public Dictionary<Mobile, DateTime> BattleAggression { get; set; }
        public List<string> Messages { get; set; }

        public List<VvVAltar> Altars { get; set; }
        public int AltarIndex { get; set; }

        public List<VvVTrap> Traps { get; set; }
        public List<CannonTurret> Turrets { get; set; }
        public List<Mobile> Warned { get; set; }

        public VvVPriest VicePriest { get; private set; }
        public VvVPriest VirtuePriest { get; private set; }

        public Timer Timer { get; private set; }

        public int TrapCount
        {
            get
            {
                int count = 0;
                foreach (VvVTrap trap in Traps)
                {
                    if (!trap.Deleted)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int TurretCount
        {
            get
            {
                int count = 0;
                foreach (CannonTurret turret in Turrets)
                {
                    if (!turret.Deleted)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool InCooldown => CooldownEnds > DateTime.UtcNow;

        public DateTime NextCombatHeatCycle { get; private set; }

        public VvVBattle(ViceVsVirtueSystem sys)
        {
            System = sys;
        }

        public void Begin()
        {
            OnGoing = true;
            NextCombatHeatCycle = DateTime.UtcNow;
            VvVCity newCity = City;
            List<VvVCity> cities = new List<VvVCity>();

            Teams = new List<BattleTeam>();
            KillCooldown = new Dictionary<Mobile, DateTime>();
            Messages = new List<string>();
            Altars = new List<VvVAltar>();
            Traps = new List<VvVTrap>();
            Warned = new List<Mobile>();
            Turrets = new List<CannonTurret>();

            for (int i = 0; i < 8; i++)
            {
                if (!System.ExemptCities.Contains((VvVCity)i) && (VvVCity)i != newCity)
                    cities.Add((VvVCity)i);
            }

            if (cities.Count > 0)
            {
                newCity = cities[Utility.Random(cities.Count)];
            }
            else if (System.ExemptCities.Contains(newCity))
            {
                System.SendVvVMessage("All VvV cities are currently exempt.");
                return;
            }

            ColUtility.Free(cities);
            City = newCity;
            BeginTimer();

            StartTime = DateTime.UtcNow;
            NextSigilSpawn = DateTime.UtcNow + TimeSpan.FromMinutes(Utility.RandomMinMax(1, 3));

            AltarIndex = 0;
            SpawnAltars();
            SpawnPriests(false);

            if (Region is GuardedRegion region)
            {
                GuardedRegion.Disable(region);
            }

            NextAltarActivate = DateTime.UtcNow + TimeSpan.FromMinutes(1);

            System.SendVvVMessage(1154721, $"#{ViceVsVirtueSystem.GetCityLocalization(City)}"); // A Battle between Vice and Virtue is active! To Arms! The City of ~1_CITY~ is besieged!
        }

        public void SpawnAltars()
        {
            for (var index = 0; index < CityInfo.Infos[City].AltarLocs.Length; index++)
            {
                Point3D p = CityInfo.Infos[City].AltarLocs[index];
                VvVAltar altar = new VvVAltar(this);

                altar.MoveToWorld(p, Map.Felucca);
                Altars.Add(altar);
            }
        }

        public void SpawnPriests(bool movetoworld = true)
        {
            if (VicePriest == null || VicePriest.Deleted)
                VicePriest = new VvVPriest(VvVType.Vice, this);

            if (VirtuePriest == null || VirtuePriest.Deleted)
                VirtuePriest = new VvVPriest(VvVType.Virtue, this);

            if (movetoworld)
            {
                Point3D p;

                do
                {
                    p = Map.Felucca.GetRandomSpawnPoint(CityInfo.Infos[City].PriestLocation);
                }
                while (!Map.Felucca.CanSpawnMobile(p));

                VicePriest.MoveToWorld(p, Map.Felucca);

                do
                {
                    p = Map.Felucca.GetRandomSpawnPoint(CityInfo.Infos[City].PriestLocation);
                }
                while (!Map.Felucca.CanSpawnMobile(p));

                VirtuePriest.MoveToWorld(p, Map.Felucca);
            }
        }

        public void BeginTimer()
        {
            EndTimer();

            Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), OnTick);
            Timer.Start();
        }

        public void EndTimer()
        {
            if (Timer != null)
            {
                Timer.Stop();
                Timer = null;
            }
        }

        public void OnTick()
        {
            if (!OnGoing)
                return;

            CheckParticipation();
            UpdateAllGumps();

            if (StartTime + TimeSpan.FromMinutes(Duration) < DateTime.UtcNow)
            {
                EndBattle();
                return;
            }

            if (LastOccupationCheck + TimeSpan.FromMinutes(1) < DateTime.UtcNow)
            {
                CheckOccupation();

                LastOccupationCheck = DateTime.UtcNow;
            }

            if (NextAltarActivate != DateTime.MinValue && NextAltarActivate < DateTime.UtcNow)
            {
                ActivateAltar();
                NextAltarActivate = DateTime.MinValue;
            }

            if (NextSigilSpawn != DateTime.MinValue && NextSigilSpawn < DateTime.UtcNow)
            {
                if (Sigil == null || Sigil.Deleted)
                {
                    SpawnSigil();
                }
            }

            ActivateArrows();

            if (KillCooldown != null)
            {
                List<Mobile> list = new List<Mobile>();

                foreach (var mob in KillCooldown.Keys)
                {
                    if (KillCooldown[mob] < DateTime.UtcNow)
                    {
                        list.Add(mob);
                    }
                }

                for (var index = 0; index < list.Count; index++)
                {
                    Mobile m = list[index];

                    KillCooldown.Remove(m);
                }

                ColUtility.Free(list);
            }

            if (Turrets != null)
            {
                for (var index = 0; index < Turrets.Count; index++)
                {
                    var t = Turrets[index];

                    t.Scan();
                }
            }
        }

        public void CheckParticipation()
        {
            if (Region == null)
            {
                return;
            }

            BattleTeam team = null;
            UnContested = true;
            bool checkAggression = ViceVsVirtueSystem.EnhancedRules && NextCombatHeatCycle < DateTime.UtcNow;

            foreach (Mobile mobile in Region.GetEnumeratedMobiles())
            {
                if (mobile is PlayerMobile pm)
                {
                    bool vvv = ViceVsVirtueSystem.IsVvV(pm);

                    if (!vvv && !Warned.Contains(pm) && pm.AccessLevel == AccessLevel.Player)
                    {
                        pm.SendGump(new BattleWarningGump(pm));
                        Warned.Add(pm);
                    }
                    else if (vvv && pm.Alive && !pm.Hidden && BaseBoat.FindBoatAt(pm.Location, pm.Map) == null && BaseHouse.FindHouseAt(pm) == null && pm.Guild is Guild g)
                    {
                        BattleTeam t = GetTeam(g);

                        if (team == null)
                        {
                            team = t;
                        }
                        else if (t != team && UnContested)
                        {
                            UnContested = false;
                        }
                    }

                    if (checkAggression && (vvv || ViceVsVirtueSystem.IsVvVCombatant(pm)))
                    {
                        AddAggression(pm);
                    }
                }
            }

            if (checkAggression)
            {
                CheckBattleAggression();

                NextCombatHeatCycle = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            }
        }

        public void AddAggression(Mobile m)
        {
            if (m is PlayerMobile)
            {
                if (BattleAggression == null)
                {
                    BattleAggression = new Dictionary<Mobile, DateTime>();
                }

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, m, true));
                BattleAggression[m] = DateTime.UtcNow + TimeSpan.FromMinutes(2);
            }
        }

        private void CheckBattleAggression()
        {
            if (BattleAggression == null)
                return;

            List<Mobile> list = new List<Mobile>(BattleAggression.Keys);

            for (var index = 0; index < list.Count; index++)
            {
                Mobile m = list[index];

                if (BattleAggression[m] < DateTime.UtcNow && !m.Region.IsPartOf(Region))
                {
                    BattleAggression.Remove(m);
                }
            }

            ColUtility.Free(list);
        }

        public bool HasBattleAggression(Mobile m)
        {
            if (BattleAggression == null || !BattleAggression.TryGetValue(m, out DateTime value))
            {
                return false;
            }

            if (value < DateTime.UtcNow)
            {
                BattleAggression.Remove(m);
                return false;
            }

            return true;
        }

        public void EndBattle()
        {
            OnGoing = false;
            EndTimer();

            if (Region is GuardedRegion guardedRegion)
            {
                guardedRegion.Disabled = false;

                foreach (Mobile mobile in Region.GetEnumeratedMobiles())
                {
                    if (mobile is PlayerMobile pm)
                    {
                        pm.RecheckTownProtection();
                    }
                }
            }

            for (var index = 0; index < Altars.Count; index++)
            {
                VvVAltar altar = Altars[index];

                if (!altar.Deleted)
                {
                    altar.Delete();
                }
            }

            for (var index = 0; index < Traps.Count; index++)
            {
                VvVTrap trap = Traps[index];

                if (!trap.Deleted)
                {
                    trap.Delete();
                }
            }

            for (var index = 0; index < Turrets.Count; index++)
            {
                CannonTurret turret = Turrets[index];

                if (!turret.Deleted)
                {
                    turret.Delete();
                }
            }

            if (VicePriest != null)
            {
                VicePriest.Delete();
                VicePriest = null;
            }

            if (VirtuePriest != null)
            {
                VirtuePriest.Delete();
                VirtuePriest = null;
            }

            if (Sigil != null)
            {
                Sigil.Delete();
                Sigil = null;
            }

            TallyStats();
            SendBattleStatsGump();

            System.SendVvVMessage(1154722); // A VvV battle has just concluded. The next battle will begin in less than five minutes!

            if (BattleAggression != null)
            {
                BattleAggression.Clear();
            }

            ColUtility.Free(Altars);
            ColUtility.Free(Teams);
            KillCooldown.Clear();
            ColUtility.Free(Messages);
            ColUtility.Free(Traps);
            ColUtility.Free(Warned);
            ColUtility.Free(Turrets);

            if (Region is GuardedRegion region)
            {
                region.Disabled = false;
            }

            NextSigilSpawn = DateTime.MinValue;
            LastOccupationCheck = DateTime.MinValue;
            NextAnnouncement = DateTime.MinValue;
            StartTime = DateTime.MinValue;
            NextAltarActivate = DateTime.MinValue;
            ManaSpikeEndEffects = DateTime.MinValue;
            NextManaSpike = DateTime.MinValue;

            CooldownEnds = DateTime.UtcNow + TimeSpan.FromMinutes(Cooldown);

            Timer.DelayCall(TimeSpan.FromMinutes(Cooldown), () =>
            {
                System.CheckBattleStatus();
            });
        }

        public void TallyStats()
        {
            BattleTeam leader = GetLeader();
            List<Guild> added = new List<Guild>();

            if (leader == null || leader.Guild == null)
            {
                return;
            }

            leader.Silver += AwardSilver(WinSilver + OppositionCount(leader.Guild) * 50);

            foreach (Mobile m in Region.GetEnumeratedMobiles())
            {
                Guild g = m.Guild as Guild;

                if (g == null)
                {
                    continue;
                }

                if (m is PlayerMobile pm)
                {
                    BattleTeam team = GetTeam(g);
                    VvVPlayerBattleStats stats = GetPlayerStats(pm);
                    VvVPlayerEntry entry = ViceVsVirtueSystem.Instance.GetPlayerEntry<VvVPlayerEntry>(pm);

                    if (entry != null)
                    {
                        entry.Score += team.Score;
                        entry.Points += team.Silver;
                        entry.Kills += stats.Kills;
                        entry.Deaths += stats.Deaths;
                        entry.Assists += stats.Assists;
                        entry.ReturnedSigils += stats.ReturnedSigils;
                        entry.DisarmedTraps += stats.Disarmed;
                        entry.StolenSigils += stats.Stolen;

                        if (added.Contains(g))
                        {
                            continue;
                        }

                        added.Add(g);

                        if (!ViceVsVirtueSystem.Instance.GuildStats.ContainsKey(g))
                            ViceVsVirtueSystem.Instance.GuildStats[g] = new VvVGuildStats(g);

                        VvVGuildStats gstats = ViceVsVirtueSystem.Instance.GuildStats[g];

                        gstats.Kills += team.Kills;
                        gstats.ReturnedSigils += team.ReturnedSigils;
                        gstats.Score += team.Score;
                    }
                }
            }

            ColUtility.Free(added);
        }

        public void SpawnSigil()
        {
            Point3D p = CityInfo.Infos[City].SigilLocs[Utility.Random(CityInfo.Infos[City].SigilLocs.Length)];
            Sigil = new VvVSigil(this);

            Sigil.MoveToWorld(p, Map.Felucca);

            UpdateAllGumps();
        }

        public void ActivateAltar()
        {
            if (AltarIndex == 2)
                AltarIndex = 0;
            else
                AltarIndex++;

            Altars[AltarIndex].Activate();
            ActivateArrows();

            SendStatusMessage("Fight for the altar!", true);
        }

        public void CheckArrow(PlayerMobile pm)
        {
            if (pm.NetState == null)
            {
                return;
            }

            for (var index = 0; index < Altars.Count; index++)
            {
                VvVAltar altar = Altars[index];

                if (altar.IsActive)
                {
                    pm.QuestArrow = new AltarArrow(pm, altar);
                    break;
                }
            }
        }

        public void ActivateArrows()
        {
            if (Altars == null || Region == null)
                return;

            foreach (Mobile mobile in Region.GetEnumeratedMobiles())
            {
                if (mobile is PlayerMobile pm && pm.NetState != null && pm.QuestArrow == null)
                {
                    for (var index = 0; index < Altars.Count; index++)
                    {
                        VvVAltar altar = Altars[index];

                        if (altar.IsActive)
                        {
                            pm.QuestArrow = new AltarArrow(pm, altar);
                            break;
                        }
                    }
                }
            }
        }

        public VvVPlayerBattleStats GetPlayerStats(PlayerMobile pm)
        {
            if (pm == null || pm.Guild == null)
            {
                return null;
            }

            Guild g = pm.Guild as Guild;

            BattleTeam team = GetTeam(g);
            VvVPlayerBattleStats stats = null;

            for (var index = 0; index < team.PlayerStats.Count; index++)
            {
                var s = team.PlayerStats[index];

                if (s.Player == pm)
                {
                    stats = s;
                    break;
                }
            }

            if (stats == null)
            {
                stats = new VvVPlayerBattleStats(pm);
                team.PlayerStats.Add(stats);
            }

            return stats;
        }

        public BattleTeam GetTeam(Guild g)
        {
            BattleTeam team = null;

            for (var index = 0; index < Teams.Count; index++)
            {
                var t = Teams[index];

                if (t.Guild != null && (t.Guild == g || t.Guild.IsAlly(g)))
                {
                    team = t;
                    break;
                }
            }

            if (team != null)
            {
                return team;
            }

            team = new BattleTeam(g);
            Teams.Add(team);

            return team;
        }

        public void Update(Mobile m, UpdateType type)
        {
            VvVPlayerEntry entry = System.GetPlayerEntry<VvVPlayerEntry>(m);

            if (entry != null)
            {
                Update(null, entry, type);
            }
        }

        public void Update(VvVPlayerEntry victim, VvVPlayerEntry killer, UpdateType type)
        {
            if (killer == null || killer.Player == null || killer.Guild == null)
                return;

            VvVPlayerBattleStats killerStats = GetPlayerStats(killer.Player);
            VvVPlayerBattleStats victimStats = victim == null ? null : GetPlayerStats(victim.Player);

            BattleTeam killerTeam = GetTeam(killer.Guild);
            BattleTeam victimTeam = null;

            if (victim != null)
                victimTeam = GetTeam(victim.Guild);

            switch (type)
            {
                case UpdateType.Kill:
                    if (killerStats != null) killerStats.Kills++;
                    if (victimStats != null) victimStats.Deaths++;

                    if (killerTeam != null)
                        killerTeam.Kills++;

                    if (victimTeam != null)
                        victimTeam.Deaths++;

                    if (victim != null && victim.Player != null)
                    {
                        if (!KillCooldown.ContainsKey(victim.Player) || KillCooldown[victim.Player] < DateTime.UtcNow)
                        {
                            if (killerTeam != null)
                            {
                                killerTeam.Score += (int)KillPoints;
                                killerTeam.Silver += AwardSilver(KillSilver + OppositionCount(killer.Guild) * 50);
                            }

                            SendStatusMessage($"{killer.Player.Name} has killed {victim.Player.Name}!");
                            KillCooldown[victim.Player] = DateTime.UtcNow + TimeSpan.FromMinutes(KillCooldownDuration);
                        }
                    }

                    break;
                case UpdateType.Assist:
                    if (killerStats != null)
                        killerStats.Assists++;

                    if (killerTeam != null)
                        killerTeam.Assists++;

                    break;
                case UpdateType.Steal:
                    if (killerStats != null)
                    {
                        killerStats.Stolen++;
                        SendStatusMessage($"{killer.Player.Name} has stolen the sigil!");
                    }

                    if (killerTeam != null)
                        killerTeam.Stolen++;

                    break;
                case UpdateType.TurnInVice:
                case UpdateType.TurnInVirtue:
                    if (killerTeam != null)
                    {
                        killerTeam.Score += (int)TurnInPoints;
                        killerTeam.Silver += AwardSilver(TurnInSilver + OppositionCount(killer.Guild) * 50);
                    }

                    if (killerStats != null && killerTeam != null)
                    {
                        if (type == UpdateType.TurnInVirtue)
                        {
                            killerStats.VirtueReturned++;
                            killerTeam.VirtueReturned++;
                        }
                        else
                        {
                            killerStats.ViceReturned++;
                            killerTeam.ViceReturned++;
                        }
                    }

                    SendStatusMessage($"{killer.Player.Name} has returned the sigil!");

                    NextSigilSpawn = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                    RemovePriests();

                    break;
                case UpdateType.Disarm:
                    SendStatusMessage($"{killer.Player.Name} has disarmed a trap!");

                    if (killerStats != null)
                    {
                        killerStats.Disarmed++;
                    }

                    if (killerTeam != null)
                    {
                        killerTeam.Silver += AwardSilver(DisarmSilver + OppositionCount(killer.Guild) * 50);
                        killerTeam.Disarmed++;
                    }
                    break;
            }

            CheckScore();
        }

        public int AwardSilver(int amount)
        {
            return (int)(amount * SilverPenalty);
        }

        public void RemovePriests()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(4), () =>
                {
                    if (VicePriest != null)
                        VicePriest.Internalize();

                    if (VirtuePriest != null)
                        VirtuePriest.Internalize();
                });
        }

        public void OccupyAltar(Guild g)
        {
            if (!OnGoing || g == null)
            {
                return;
            }

            BattleTeam team = GetTeam(g);

            team.Score += (int)AltarPoints;
            team.Silver += AwardSilver(AltarSilver + OppositionCount(g) * 50);

            SendStatusMessage($"{g.Abbreviation} claimed the altar!");

            foreach (Mobile p in Region.GetEnumeratedMobiles())
            {
                if (p is PlayerMobile && p.QuestArrow != null)
                {
                    p.QuestArrow = null;
                }
            }

            CheckScore();
            NextAltarActivate = DateTime.UtcNow + TimeSpan.FromMinutes(2);
        }

        public void CheckOccupation()
        {
            if (!OnGoing)
            {
                return;
            }

            if (Teams.Count == 1)
            {
                BattleTeam team = Teams[0];

                team.Score += (int)OccupyPoints;
                UpdateAllGumps();
                CheckScore();

                if (OnGoing && NextAnnouncement < DateTime.UtcNow)
                {
                    if (ViceVsVirtueSystem.EnhancedRules)
                    {
                        System.SendVvVMessage($"{team.Guild.Name} is occupying {(City == VvVCity.SkaraBrae ? "Skara Brae" : City.ToString())}!");
                    }
                    else
                    {
                        System.SendVvVMessage(1154957, team.Guild.Name); // ~1_NAME~ is occupying the city!
                    }

                    NextAnnouncement = DateTime.UtcNow + TimeSpan.FromMinutes(Announcement);
                }
            }
            else // Is this a bug?  Verified on EA this is how it behaves
            {
                if (NextAnnouncement < DateTime.UtcNow)
                {
                    if (ViceVsVirtueSystem.EnhancedRules)
                    {
                        System.SendVvVMessage(1050039, $"#{ViceVsVirtueSystem.GetCityLocalization(City).ToString()}\tis unoccupied! Slay opposing forces to claim the city for your guild!");
                    }
                    else
                    {
                        System.SendVvVMessage(1154958); // The City is unoccupied! Slay opposing forces to claim the city for your guild!
                    }

                    NextAnnouncement = DateTime.UtcNow + TimeSpan.FromMinutes(Announcement);
                }
            }
        }

        public void CheckScore()
        {
            int score;
            GetLeader(out score);

            if (score >= ScoreToWin)
            {
                EndBattle();
                return;
            }

            UpdateAllGumps();
        }

        public BattleTeam GetLeader()
        {
            int score;
            return GetLeader(out score);
        }

        /// <summary>
        /// Gets real time leader of battle, out parameter of their score
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public BattleTeam GetLeader(out int score)
        {
            score = 0;

            List<BattleTeam> teams = new List<BattleTeam>(Teams);
            teams.Sort();

            if (teams.Count > 0)
            {
                score = teams[0].Score;
                return teams[0];
            }

            return null;
        }

        /// <summary>
        /// Returns enemy count, by alliance
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public int OppositionCount(Guild g)
        {
            List<Guild> exempt = new List<Guild>();
            int count = 0;

            for (var index = 0; index < Teams.Count; index++)
            {
                BattleTeam team = Teams[index];

                if (team.Guild == null || team.Guild == g || team.Guild.IsAlly(g) || exempt.Contains(g))
                {
                    continue;
                }

                count++;

                if (team.Guild.Alliance != null)
                {
                    for (var i = 0; i < team.Guild.Alliance.Members.Count; i++)
                    {
                        Guild guil = team.Guild.Alliance.Members[i];

                        if (!exempt.Contains(guil))
                        {
                            exempt.Add(guil);
                        }
                    }
                }
            }

            ColUtility.Free(exempt);

            return count;
        }

        public bool IsInActiveBattle(Mobile one, Mobile two)
        {
            return IsInActiveBattle(one) && IsInActiveBattle(two);
        }

        public bool IsInActiveBattle(Mobile m)
        {
            if (!OnGoing)
                return false;

            Region r = Region.Find(m.Location, m.Map);

            return r == Region;
        }

        public void OnEnterRegion(Mobile m)
        {
            if (m is PlayerMobile mobile && OnGoing)
            {
                if (mobile.HasGump(typeof(VvVBattleStatusGump)))
                    mobile.CloseGump(typeof(VvVBattleStatusGump));

                BaseGump.SendGump(new VvVBattleStatusGump(mobile, this));
            }
        }

        public void CheckGump(Mobile m)
        {
            if (m is PlayerMobile mobile && OnGoing)
            {
                BaseGump.SendGump(new VvVBattleStatusGump(mobile, this));
            }
        }

        public void UpdateAllGumps()
        {
            if (Region == null)
            {
                return;
            }

            foreach (Mobile mobile in Region.GetEnumeratedMobiles())
            {
                if (mobile is PlayerMobile m)
                {
                    if (!ViceVsVirtueSystem.IsVvV(m) || m.NetState == null) continue;

                    VvVBattleStatusGump g = m.FindGump(typeof(VvVBattleStatusGump)) as VvVBattleStatusGump;

                    if (g == null)
                    {
                        BaseGump.SendGump(new VvVBattleStatusGump(m, this));
                    }
                    else
                    {
                        g.Refresh(true, false);
                    }
                }
            }
        }

        public void SendBattleStatsGump()
        {
            if (Region == null)
            {
                return;
            }

            foreach (Mobile mobile in Region.GetEnumeratedMobiles())
            {
                if (mobile is PlayerMobile m && ViceVsVirtueSystem.IsVvV(m))
                {
                    m.CloseGump(typeof(VvVBattleStatusGump));
                    m.SendGump(new BattleStatsGump(m, this));
                }
            }
        }

        public void SendStatusMessage(string message, bool sendgumps = false)
        {
            Messages.Add(message);

            if (sendgumps)
            {
                UpdateAllGumps();
            }
        }

        public void AddCannonTurret(CannonTurret turret)
        {
            if (!Turrets.Contains(turret))
            {
                Turrets.Add(turret);
            }
        }

        public VvVBattle(GenericReader reader, ViceVsVirtueSystem system)
        {
            int version = reader.ReadInt();
            System = system;

            Altars = new List<VvVAltar>();
            KillCooldown = new Dictionary<Mobile, DateTime>();
            Messages = new List<string>();
            Traps = new List<VvVTrap>();
            Warned = new List<Mobile>();
            Turrets = new List<CannonTurret>();
            Teams = new List<BattleTeam>();

            OnGoing = reader.ReadBool();

            if (reader.ReadInt() == 0)
            {
                StartTime = reader.ReadDateTime();
                CooldownEnds = reader.ReadDateTime();
                LastOccupationCheck = reader.ReadDateTime();
                NextSigilSpawn = reader.ReadDateTime();
                NextAnnouncement = reader.ReadDateTime();
                NextAltarActivate = reader.ReadDateTime();
                City = (VvVCity)reader.ReadInt();
                Sigil = reader.ReadItem() as VvVSigil;
                VicePriest = reader.ReadMobile() as VvVPriest;
                VirtuePriest = reader.ReadMobile() as VvVPriest;

                if (Sigil != null)
                    Sigil.Battle = this;

                if (VicePriest != null)
                    VicePriest.Battle = this;

                if (VirtuePriest != null)
                    VirtuePriest.Battle = this;

                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    if (reader.ReadItem() is VvVAltar altar)
                    {
                        altar.Battle = this;
                        Altars.Add(altar);
                    }
                }

                if (version == 1)
                {
                    count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        BattleTeam team = new BattleTeam(reader);
                        Teams.Add(team);
                    }
                }
                else
                {
                    count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        Guild g = reader.ReadGuild() as Guild;
                        VvVGuildBattleStats stats = new VvVGuildBattleStats(reader, g);
                    }
                }

                count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    if (reader.ReadItem() is VvVTrap t)
                    {
                        Traps.Add(t);
                    }
                }

                Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
                    {
                        if (Region is GuardedRegion region)
                        {
                            GuardedRegion.Disable(region);
                        }

                        BeginTimer();

                        ActivateArrows();
                    });
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(OnGoing);

            if (OnGoing)
            {
                writer.Write(0);
                writer.Write(StartTime);
                writer.Write(CooldownEnds);
                writer.Write(LastOccupationCheck);
                writer.Write(NextSigilSpawn);
                writer.Write(NextAnnouncement);
                writer.Write(NextAltarActivate);
                writer.Write((int)City);
                writer.Write(Sigil);
                writer.Write(VicePriest);
                writer.Write(VirtuePriest);

                writer.Write(Altars.Count);
                for (var index = 0; index < Altars.Count; index++)
                {
                    var altar = Altars[index];
                    writer.Write(altar);
                }

                writer.Write(Teams.Count);
                for (var index = 0; index < Teams.Count; index++)
                {
                    BattleTeam team = Teams[index];
                    team.Serialize(writer);
                }

                writer.Write(Traps.Count);
                for (var index = 0; index < Traps.Count; index++)
                {
                    var t = Traps[index];
                    writer.Write(t);
                }
            }
            else
            {
                writer.Write(1);
            }
        }
    }
}
