using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Server.Engines.JollyRoger
{
    public enum Shrine
    {        
        Valor,
        Spirituality,
        Sacrifice,
        Justice,
        Humility,
        Honor,
        Honesty,
        Compassion
    }

    public enum MasterType
    {
        Dragons,
        Undead,
        Elementals,
        Daemons,
        Orcs,
        Wildlings,
        Frost,
        Arachnids,
        Fey,
        Nature
    }

    public class ShrineBattleRegion : BaseRegion
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public ShrineBattleController Controller { get; set; }

        public ShrineBattleRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        { }
    }

    public class ShrineBattleController : Item
    {
        private bool m_Active;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get => m_Active;
            set
            {
                if (value)
                    BeginInvasion();
                else
                    RemoveSpawn();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ShrineBattleRegion Region { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public bool ForceRespawn
        {
            get => false;
            set
            {
                if (!value)
                    return;

                RemoveSpawn();
                BeginInvasion();
            }
        }

        private static Dictionary<Shrine, Rectangle2D[]> Defs;

        [CommandProperty(AccessLevel.GameMaster)]
        public Shrine Shrine { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int FragmentCount { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnCount
        {
            get
            {
                if (Spawn == null || Spawn.Count == 0)
                {
                    return 0;
                }

                int count = 0;

                foreach (KeyValuePair<BaseCreature, List<BaseCreature>> kvp in Spawn)
                {
                    if (kvp.Key.Alive)
                    {
                        count++;
                    }

                    int count1 = 0;

                    for (var index = 0; index < kvp.Value.Count; index++)
                    {
                        var bc = kvp.Value[index];

                        if (bc.Alive)
                        {
                            count1++;
                        }
                    }

                    count += count1;
                }

                return count;
            }
        }

        public Dictionary<BaseCreature, List<BaseCreature>> Spawn { get; set; }

        [Constructable]
        public ShrineBattleController(Shrine shrine)
            : base(7960)
        {
            Name = string.Format("{0} Shrine Battle Controller", shrine.ToString());
            Shrine = shrine;
            Movable = false;
            Visible = false;
            Weight = 0;

            Spawn = new Dictionary<BaseCreature, List<BaseCreature>>();
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1157254, "Do not remove !!");
        }

        public override void OnMapChange()
        {
            RegisterRegion();
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            UnRegisterRegion();
        }

        public void RegisterRegion()
        {
            if (Region == null && Map != Map.Internal && GetRegion() is ShrineBattleRegion reg)
            {
                Region = reg;
                Region.Controller = this;
            }
        }

        public void UnRegisterRegion()
        {
            if (Region != null)
            {
                Region.Controller = null;
                Region = null;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel > AccessLevel.GameMaster)
            {
                from.SendGump(new Gumps.PropertiesGump(from, this));
            }
        }

        private readonly Type[][] _SpawnTable =
        {
            new[] { typeof(Dragon), typeof(Drake), typeof(GiantSerpent), typeof(Reptalon), typeof(Hydra) },
            new[] { typeof(Mummy), typeof(Lich), typeof(Zombie), typeof(Shade), typeof(SkeletalKnight), typeof(Spectre) },
            new[] { typeof(EnragedEarthElemental), typeof(WaterElemental), typeof(FireElemental), typeof(AirElemental), typeof(BloodElemental) },
            new[] { typeof(Daemon), typeof(Succubus), typeof(Imp), typeof(ChaosDaemon), typeof(BoneDemon) },
            new[] { typeof(OrcChopper), typeof(OrcBomber), typeof(OrcishMage), typeof(OrcishLord) },
            new[] { typeof(RagingGrizzlyBear), typeof(GreaterMongbat), typeof(DireWolf), typeof(GiantRat), typeof(Troglodyte) },
            new[] { typeof(IceElemental), typeof(SnowElemental), typeof(IceFiend), typeof(FrostTroll), typeof(IceSerpent) },
            new[] { typeof(DreadSpider), typeof(GiantBlackWidow), typeof(Scorpion), typeof(TerathanWarrior), typeof(WolfSpider) },
            new[] { typeof(MLDryad), typeof(CuSidhe), typeof(Wisp), typeof(Satyr), typeof(Centaur) },
            new[] { typeof(SwampTentacle), typeof(PlagueBeast), typeof(Bogling), typeof(FeralTreefellow) }
        };

        public void BeginInvasion()
        {
            if (m_Active || Deleted)
                return;

            m_Active = true;

            List<Rectangle2D> SpawnZones = new List<Rectangle2D>();

            for (var index = 0; index < Defs[Shrine].Length; index++)
            {
                var rectangle2D = Defs[Shrine][index];

                SpawnZones.Add(rectangle2D);
            }

            for (int i = 0; i < 3; i++)
            {
                Rectangle2D spawnrec = SpawnZones[Utility.Random(SpawnZones.Count)];
                SpawnZones.Remove(spawnrec);

                Point3D p = Point3D.Zero;

                do
                {
                    p = Map.GetRandomSpawnPoint(spawnrec);
                }
                while (p == Point3D.Zero || !Map.CanSpawnMobile(p));

                List<Point3D> points = new List<Point3D>();

                Misc.Geometry.Circle2D(p, Map, 4, (pnt, map) =>
                {
                    if (Map.CanSpawnMobile(pnt.X, pnt.Y, pnt.Z))
                        points.Add(pnt);
                });

                if (points.Count == 0)
                {
                    points.Add(p);
                }
                
                MasterType type = (MasterType)Utility.Random(9);

                List<BaseCreature> list = new List<BaseCreature>();

                for (int s = 0; s < 10; s++)
                {
                    if (Activator.CreateInstance(_SpawnTable[(int)type][Utility.Random(_SpawnTable[(int)type].Length)]) is BaseCreature bc)
                    {
                        bc.Kills = 100;

                        if (bc.FightMode == FightMode.Evil)
                        {
                            bc.FightMode = FightMode.Aggressor;
                        }

                        list.Add(bc);

                        Point3D point = points[Utility.Random(points.Count)];

                        SpawnMobile(bc, point);
                    }
                }

                ShrineMaster capt = new ShrineMaster(type, this)
                {
                    Blessed = true,
                    _Controller = this
                };
                Spawn[capt] = list;
                SpawnMobile(capt, p);
            }
        }

        public void SpawnMobile(BaseCreature bc, Point3D p)
        {
            bc.MoveToWorld(p, Map);
            bc.Home = p;
            bc.SeeksHome = true;
            bc.RangeHome = Utility.RandomMinMax(5, 10);
            bc.CanSwim = false;
            bc.Tamable = false;
        }

        public void OnMasterDestroyed()
        {
            bool any = false;

            foreach (var x in Spawn)
            {
                if (x.Key.Alive)
                {
                    any = true;
                    break;
                }
            }

            if (Spawn != null && !any)
            {
                RemoveSpawn();
            }
        }

        public bool MasterBlessCheck(ShrineMaster master)
        {
            bool any = false;

            for (var index = 0; index < Spawn[master].Count; index++)
            {
                var x = Spawn[master][index];

                if (x.Alive)
                {
                    any = true;
                    break;
                }
            }

            return Spawn != null && Spawn.ContainsKey(master) && !any;
        }

        public void CleanupSpawn()
        {
            if (Spawn == null)
                return;

            List<BaseCreature> list = null;

            foreach (KeyValuePair<BaseCreature, List<BaseCreature>> kvp in Spawn)
            {
                if (kvp.Value != null)
                {
                    list = new List<BaseCreature>(kvp.Value);

                    for (var index = 0; index < list.Count; index++)
                    {
                        BaseCreature b = list[index];

                        if (b == null || !b.Alive || b.Deleted)
                        {
                            kvp.Value.Remove(b);
                        }
                    }
                }

                if (list != null && list.Count > 0)
                {
                    list.Clear();
                    list.TrimExcess();
                }
            }
        }

        public static Item CreateItem(Mobile damager)
        {
            Item i = Loot.RandomArmorOrShieldOrWeaponOrJewelry(LootPackEntry.IsInTokuno(damager), LootPackEntry.IsMondain(damager), LootPackEntry.IsStygian(damager));

            if (i != null)
            {
                RunicReforging.GenerateRandomItem(i, damager, Utility.RandomMinMax(600, 800), damager is PlayerMobile pm ? pm.RealLuck : 0, ReforgedPrefix.None, ReforgedSuffix.Fellowship);
            }

            return i;
        }

        public void RemoveSpawn()
        {
            if (!m_Active || Deleted || Spawn == null)
                return;

            m_Active = false;

            Dictionary<BaseCreature, List<BaseCreature>> copy = Spawn;
            Spawn = new Dictionary<BaseCreature, List<BaseCreature>>();

            foreach (KeyValuePair<BaseCreature, List<BaseCreature>> kvp in copy)
            {
                for (var index = 0; index < kvp.Value.Count; index++)
                {
                    BaseCreature bc = kvp.Value[index];

                    if (bc.Alive)
                    {
                        bc.Kill();
                    }
                }

                if (kvp.Key.Alive)
                {
                    kvp.Key.Blessed = false;
                    kvp.Key.Kill();
                }
            }

            copy.Clear();
        }

        public ShrineBattleController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_Active);
            writer.Write((int)Shrine);
            writer.Write(FragmentCount);

            writer.Write(Spawn == null ? 0 : Spawn.Count);
            if (Spawn != null)
            {
                foreach (KeyValuePair<BaseCreature, List<BaseCreature>> kvp in Spawn)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Count);
                    for (var index = 0; index < kvp.Value.Count; index++)
                    {
                        var bc = kvp.Value[index];

                        writer.Write(bc);
                    }
                }
            }

            Timer.DelayCall(TimeSpan.FromSeconds(30), CleanupSpawn);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Spawn = new Dictionary<BaseCreature, List<BaseCreature>>();

            m_Active = reader.ReadBool();
            Shrine = (Shrine)reader.ReadInt();
            FragmentCount = reader.ReadInt();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                BaseCreature captain = reader.ReadMobile() as BaseCreature;
                int c = reader.ReadInt();

                List<BaseCreature> list = new List<BaseCreature>();

                for (int j = 0; j < c; j++)
                {
                    if (reader.ReadMobile() is BaseCreature spawn)
                    {
                        list.Add(spawn);
                    }
                }

                if (captain != null)
                    Spawn[captain] = list;
                else
                {
                    list.Clear();
                }
            }

            Timer.DelayCall(TimeSpan.Zero, RegisterRegion);
        }

        public static void Initialize()
        {
            Defs = new Dictionary<Shrine, Rectangle2D[]>
            {
                [Shrine.Honesty] =
                    new[]
                    {
                        new Rectangle2D(4211, 554, 11, 5), new Rectangle2D(4210, 580, 11, 6),
                        new Rectangle2D(4223, 567, 7, 10)
                    },
                [Shrine.Honor] =
                    new[]
                    {
                        new Rectangle2D(1722, 3512, 13, 6), new Rectangle2D(1708, 3523, 6, 10),
                        new Rectangle2D(1715, 3534, 11, 6)
                    },
                [Shrine.Humility] =
                    new[]
                    {
                        new Rectangle2D(4268, 3687, 7, 4), new Rectangle2D(4259, 3698, 5, 10),
                        new Rectangle2D(4268, 3707, 7, 3)
                    },
                [Shrine.Justice] =
                    new[]
                    {
                        new Rectangle2D(1287, 627, 6, 13), new Rectangle2D(1287, 617, 23, 7),
                        new Rectangle2D(1310, 625, 9, 18), new Rectangle2D(1289, 643, 19, 7)
                    },
                [Shrine.Sacrifice] =
                    new[]
                    {
                        new Rectangle2D(3334, 280, 10, 20), new Rectangle2D(3334, 303, 43, 10),
                        new Rectangle2D(3367, 279, 10, 20), new Rectangle2D(3334, 266, 43, 10)
                    },
                [Shrine.Spirituality] =
                    new[]
                    {
                        new Rectangle2D(1586, 2477, 23, 6), new Rectangle2D(1577, 2477, 8, 26),
                        new Rectangle2D(1586, 2497, 23, 6), new Rectangle2D(1611, 2477, 6, 26)
                    },
                [Shrine.Valor] =
                    new[]
                    {
                        new Rectangle2D(2483, 3921, 19, 5), new Rectangle2D(2498, 3927, 4, 14),
                        new Rectangle2D(2483, 3927, 4, 14), new Rectangle2D(2483, 3942, 19, 5)
                    },
                [Shrine.Compassion] = new[]
                {
                    new Rectangle2D(1845, 855, 25, 9), new Rectangle2D(1839, 865, 6, 20),
                    new Rectangle2D(1848, 889, 23, 5), new Rectangle2D(1870, 867, 6, 19)
                }
            };
        }
    }
}
