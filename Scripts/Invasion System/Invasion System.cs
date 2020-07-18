using System;
using System.Collections.Generic;
using Server.Customs.Systems.Invasion_System;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells.Ninjitsu;

namespace Server.Customs.Invasion_System
{
    public class TownInvasion
    {
        public static void Initialize()
        {
            Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(30.0), GlobalSync);
        }

        #region Private Variables

        private int _MinSpawnZ;
        private int _MaxSpawnZ;

        private bool _FinalStage;

        private Point3D _Top = new Point3D(4394, 1058, 30);
        private Point3D _Bottom = new Point3D(4481, 1173, 0);
        private Map _SpawnMap = Map.Felucca;

        private List<Mobile> _Spawned;

        private TownMonsterType _TownMonsterType = TownMonsterType.OrcsandRatmen;
        private TownChampionType _TownChampionType = TownChampionType.Barracoon;
        private InvasionTowns _InvasionTown = InvasionTowns.BuccaneersDen;
        private DateTime _StartTime;
        private bool _AlwaysMurderer = false;

        private string _TownInvaded = "Moonglow";

        private Timer _SpawnTimer;

        private DateTime _lastAnnounce = DateTime.UtcNow;

        private bool WasDisabledRegion;
        private bool Active;

	    private int _SpawnAmount;

        private int _SpawnRemaining;
        #endregion

        #region Public Variables

        public int MinSpawnZ { get { return _MinSpawnZ; } set { _MinSpawnZ = value; } }
        public int MaxSpawnZ { get { return _MaxSpawnZ; } set { _MaxSpawnZ = value; } }
        public Point3D Top { get { return _Top; } set { _Top = value; } }
        public Point3D Bottom { get { return _Bottom; } set { _Bottom = value; } }
        public Map SpawnMap { get { return _SpawnMap; } set { _SpawnMap = value; } }
        public List<Mobile> Spawned { get { return _Spawned; } set { _Spawned = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TownMonsterType TownMonsterType { get { return _TownMonsterType; } set { _TownMonsterType = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TownChampionType TownChampionType { get { return _TownChampionType; } set { _TownChampionType = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public InvasionTowns InvasionTown { get { return _InvasionTown; } set { _InvasionTown = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime StartTime { get { return _StartTime; } set { _StartTime = value; } }

        public bool IsRunning { get { return _SpawnTimer != null && _SpawnTimer.Running; }}
        public string TownInvaded { get { return _TownInvaded; } set { _TownInvaded = value; } }

        public Timer SpawnTimer { get { return _SpawnTimer; } set { _SpawnTimer = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SpawnAmount { get { return _SpawnAmount; } set { _SpawnAmount = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRemaining { get { return _SpawnRemaining; } set { _SpawnRemaining = value; } }
        #endregion

        #region Constructor

        public TownInvasion(InvasionTowns town, TownMonsterType monster, TownChampionType champion, DateTime time, int spawncount)
        {
            _Spawned = new List<Mobile>();

            _InvasionTown = town;
            _TownMonsterType = monster;
            _TownChampionType = champion;
            _StartTime = time;
	        _SpawnAmount = spawncount;

            InvasionControl.Invasions.Add(this);
        }

        public TownInvasion(GenericReader reader)
        {
            Deserialize(reader);
        }

        #endregion

        public void OnStart()
        {
            if (!IsRunning)
            {
                InvasionTowns invading = InvasionTown;

                switch (invading)
                {
                    case InvasionTowns.BuccaneersDen:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Buccaneer's Den";
                        break;
                    }
                    case InvasionTowns.Cove:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Cove";
                        break;
                    }
                    case InvasionTowns.Delucia:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Delucia";
                        break;
                    }
                    case InvasionTowns.Jhelom:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Jhelom";
                        break;
                    }
                    case InvasionTowns.Minoc:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Minoc";
                        break;
                    }
                    case InvasionTowns.Moonglow:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Moonglow";
                        break;
                    }
                    case InvasionTowns.Nujel:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Nujel'm";
                        break;
                    }
                    case InvasionTowns.Ocllo:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Ocllo";
                        break;
                    }
                    case InvasionTowns.Papua:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Papua";
                        break;
                    }
                    case InvasionTowns.SkaraBrae:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Skara Brae";
                        break;
                    }
                    case InvasionTowns.Yew:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Yew";
                        break;
                    }
                    case InvasionTowns.Vesper:
                    {
                        SpawnMap = Map.Felucca;
                        TownInvaded = "Vesper";
                        break;
                    }
                }

                foreach (Region r in Region.Regions)
                {
                    if (r is GuardedRegion && r.Name == TownInvaded)
                    {
                        WasDisabledRegion = ((GuardedRegion) r).Disabled;

                        ((GuardedRegion)r).Disabled = true;
                    }
                }
                Spawn();
            }
        }

        public void OnStop()
        {
            Despawn();

            if (!WasDisabledRegion)
            {
                foreach (Region r in Region.Regions)
                {
                    if (r is GuardedRegion && r.Name == TownInvaded)
                    {
                        ((GuardedRegion)r).Disabled = false;
                    }
                }
            }

            if (SpawnTimer != null)
                _SpawnTimer.Stop();

            InvasionControl.Invasions.Remove(this);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(1);

			// Version 1
			writer.Write(SpawnAmount);

			// Version 0
            writer.Write((int)InvasionTown);
            writer.Write((int)TownMonsterType);
            writer.Write((int)TownChampionType);
            writer.Write(StartTime);
            writer.Write(Spawned);

            if (IsRunning)
                Active = true;
            else
                Active = false;

            writer.Write(Active);
        }

        public void Deserialize(GenericReader reader)
        {
            var version = reader.ReadInt();

	        if (version > 0)
	        {
		        SpawnAmount = reader.ReadInt();
	        }

            InvasionTown = (InvasionTowns)reader.ReadInt();
            TownMonsterType = (TownMonsterType)reader.ReadInt();
            TownChampionType = (TownChampionType)reader.ReadInt();
            StartTime = reader.ReadDateTime();
            Spawned = reader.ReadStrongMobileList();
            Active = reader.ReadBool();

            if (Spawned == null)
                Spawned = new List<Mobile>();

            if (Active)
                InitTimer();

            foreach (Region r in Region.Regions)
            {
                if (r is GuardedRegion && r.Name == TownInvaded)
                {
                    ((GuardedRegion)r).Disabled = true;
                }
            }
        }

        #region Private Methods

        private static void GlobalSync()
        {
            var index = InvasionControl.Invasions.Count;

            while (--index >= 0)
            {
                if (index >= InvasionControl.Invasions.Count)
                    continue;

                var obj = InvasionControl.Invasions[index];

                if (obj._StartTime <= DateTime.UtcNow)
                {
                    obj.OnStart();
                }
            }
        }

        private void InitTimer()
        {
            if (!IsRunning)
                _SpawnTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(15.0), CheckSpawn);
        }

        private void Spawn()
        {
            Despawn();

            MonsterTownSpawnEntry[] entries = null;

            switch (_TownMonsterType)
            {
                default:
                case TownMonsterType.Abyss: entries = MonsterTownSpawnEntry.Abyss; break;
                case TownMonsterType.Arachnid: entries = MonsterTownSpawnEntry.Arachnid; break;
                case TownMonsterType.DragonKind: entries = MonsterTownSpawnEntry.DragonKind; break;
                case TownMonsterType.Elementals: entries = MonsterTownSpawnEntry.Elementals; break;
                case TownMonsterType.Humanoid: entries = MonsterTownSpawnEntry.Humanoid; break;
                case TownMonsterType.Ophidian: entries = MonsterTownSpawnEntry.Ophidian; break;
                case TownMonsterType.OrcsandRatmen: entries = MonsterTownSpawnEntry.OrcsandRatmen; break;
                case TownMonsterType.OreElementals: entries = MonsterTownSpawnEntry.OreElementals; break;
                case TownMonsterType.Snakes: entries = MonsterTownSpawnEntry.Snakes; break;
                case TownMonsterType.Undead: entries = MonsterTownSpawnEntry.Undead; break;
            }

	        for (int i = 0; i < SpawnAmount; ++i)
	        {
				int rolled = Utility.RandomMinMax(0, 100);

				for (int entry = 0; entry < entries.Length; ++entry)
		        {
					if (rolled > entries[entry].Percent)
			        {
						AddMonster(entries[entry].Monster);
				        break;
			        }
		        }
	        }

            if (_Spawned.Count == 0)
            {
                OnStop();
                return;
            }

            InitTimer();
        }

        public void CheckSpawn()
        {
            int count = 0;

            for (int i = 0; i < _Spawned.Count; ++i)
                if (_Spawned[i] != null && !_Spawned[i].Deleted && _Spawned[i].Alive)
                    ++count;

            if (!_FinalStage) //Monsters
            {
                _SpawnRemaining = count;

                foreach (Region r in Region.Regions)
                {
                    if (r.Name == TownInvaded)
                    {
                        List<Mobile> players = r.GetPlayers();

                        foreach (Mobile player in players)
                        {
                            if (player.Region.Name != TownInvaded)
                                return;

                            PlayerMobile pm = (PlayerMobile) player;
                            if (pm != null && pm.Alive)
                            {
                                pm.CloseGump(typeof(InvasionStatus));
                                pm.SendGump(new InvasionStatus(this));
                            }
                        }
                    }
                }

                if (count == 0) //All monsters have been slayed
                    SpawnChamp();
            }
            else //Champion
            {
                if (count == 0) //Champion is dead
                {
                    Timer.DelayCall(TimeSpan.FromMinutes(5), OnStop);
                }
            }

            if (DateTime.UtcNow >= _lastAnnounce + TimeSpan.FromMinutes(1))
            {
                string message = String.Format("{0} is being invaded by {1}. Please come help!", TownInvaded, TownMonsterType);

                foreach (TownCrier tc in TownCrier.Instances)
                {
                    tc.PublicOverheadMessage(MessageType.Yell, 0, false, message);
                }

                _lastAnnounce = DateTime.UtcNow;
            }
        }

        private void Despawn()
        {
            foreach (Mobile m in _Spawned)
                if (m != null && !m.Deleted)
                    m.Delete();

            _Spawned.Clear();

            _FinalStage = false;
        }

        private Point3D FindSpawnLocation()
        {
			TownSpawnPoint[] entries = null;

			switch (_InvasionTown)
	        {
				case InvasionTowns.BuccaneersDen:
			        entries = TownSpawnPoint.BuccaneersDen;
			        break;
				case InvasionTowns.Cove:
					entries = TownSpawnPoint.Cove;
					break;
				case InvasionTowns.Delucia:
					entries = TownSpawnPoint.Delucia;
					break;
				case InvasionTowns.Jhelom:
					entries = TownSpawnPoint.Jhelom;
					break;
				case InvasionTowns.Minoc:
					entries = TownSpawnPoint.Minoc;
					break;
				case InvasionTowns.Moonglow:
					entries = TownSpawnPoint.Moonglow;
					break;
				case InvasionTowns.Nujel:
					entries = TownSpawnPoint.Nujelm;
					break;
				case InvasionTowns.Ocllo:
					entries = TownSpawnPoint.Ocllo;
					break;
				case InvasionTowns.Papua:
					entries = TownSpawnPoint.Papua;
					break;
				case InvasionTowns.SkaraBrae:
					entries = TownSpawnPoint.SkaraBrea;
					break;
				case InvasionTowns.Vesper:
					entries = TownSpawnPoint.Vesper;
					break;
				case InvasionTowns.Yew:
					entries = TownSpawnPoint.Yew;
					break;
	        }

	        int rolled = Utility.RandomMinMax(0, entries.Length - 1);

	        Point3D location = entries[rolled].Spawnpoint;
	        location.X += Utility.RandomMinMax(-2, 2);
	        location.Y += Utility.RandomMinMax(-2, 2);

	        return location;
        }

        private void AddMonster(Type type)
        {
            object monster = Activator.CreateInstance(type);

            if (monster != null && monster is Mobile)
            {
                Point3D location = FindSpawnLocation();

                if (location == Point3D.Zero)
                {
                    return;
                }

                Mobile from = (Mobile)monster;

                from.OnBeforeSpawn(location, SpawnMap);
                from.MoveToWorld(location, SpawnMap);
                from.OnAfterSpawn();

                if (from is BaseCreature)
                {
                    ((BaseCreature)from).Tamable = false;
	                ((BaseCreature)from).RangeHome = 40;
				}

                _Spawned.Add(from);
            }
        }

        public void SpawnChamp()
        {
            Despawn();

            _FinalStage = true;

			World.Broadcast(0x35, true, String.Format("The Town Champion has spawned in {0}", TownInvaded));

            switch (_TownChampionType)
            {
                default:
                case TownChampionType.Barracoon: AddMonster(typeof(Barracoon)); break;
                case TownChampionType.Harrower: AddMonster(typeof(Harrower)); break;
                case TownChampionType.LordOaks: AddMonster(typeof(LordOaks)); break;
                case TownChampionType.Mephitis: AddMonster(typeof(Mephitis)); break;
                case TownChampionType.Neira: AddMonster(typeof(Neira)); break;
                case TownChampionType.Rikktor: AddMonster(typeof(Rikktor)); break;
                case TownChampionType.Semidar: AddMonster(typeof(Semidar)); break;
                case TownChampionType.Serado: AddMonster(typeof(Serado)); break;
            }
        }
        #endregion
     }
}

