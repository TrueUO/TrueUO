namespace Server.Engines.ArenaSystem
{
    [PropertyObject]
    public class ArenaDefinition
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public string Name { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D StoneLocation { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D ManagerLocation { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D BannerLocation1 { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D BannerLocation2 { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D GateLocation { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BannerID1 { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BannerID2 { get; }

        public Rectangle2D[] EffectAreas { get; }
        public Rectangle2D[] RegionBounds { get; }
        public Rectangle2D[] GuardBounds { get; }
        public Rectangle2D[] StartLocations { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D StartLocation1 { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D StartLocation2 { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D EjectLocation { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D DeadEjectLocation { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MapIndex { get; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map Map => Map.Maps[MapIndex];

        public ArenaDefinition(
            string name,
            int mapIndex,
            Point3D stoneLoc,
            Point3D manLoc,
            Point3D banloc1,
            Point3D banloc2,
            int id1,
            int id2,
            Rectangle2D[] effectAreas,
            Rectangle2D[] startLocs,
            Point3D gateLoc,
            Rectangle2D[] bounds,
            Rectangle2D[] guardbounds,
            Rectangle2D eject,
            Rectangle2D deadEject)
        {
            Name = name;
            MapIndex = mapIndex;
            StoneLocation = stoneLoc;
            ManagerLocation = manLoc;
            BannerLocation1 = banloc1;
            BannerLocation2 = banloc2;
            BannerID1 = id1;
            BannerID2 = id2;
            EffectAreas = effectAreas;
            StartLocations = startLocs;
            StartLocation1 = startLocs[0];
            StartLocation2 = startLocs[1];
            GateLocation = gateLoc;
            RegionBounds = bounds;
            GuardBounds = guardbounds;
            EjectLocation = eject;
            DeadEjectLocation = deadEject;
        }

        public static ArenaDefinition HavenFelucca { get; }

        public static ArenaDefinition[] Definitions => _Definitions;
        private static readonly ArenaDefinition[] _Definitions = new ArenaDefinition[4];

        static ArenaDefinition()
        {
            HavenFelucca = new ArenaDefinition("New Haven (F)", 0,
                new Point3D(3782, 2766, 5),
                new Point3D(3779, 2778, 5),
                new Point3D(3749, 2765, 12),
                new Point3D(3772, 2757, 10),
                17102,
                17099,
                new[]
                {
                    new Rectangle2D(3749, 2762, 25, 1),
                    new Rectangle2D(3749, 2768, 25, 1),
                    new Rectangle2D(3754, 2757, 1, 16),
                    new Rectangle2D(3761, 2757, 1, 16),
                    new Rectangle2D(3769, 2757, 1, 16)
                },
                new[]
                {
                    new Rectangle2D(3749, 2763, 4, 4),
                    new Rectangle2D(3770, 2763, 3, 4),
                    new Rectangle2D(3755, 2757, 4, 4),
                    new Rectangle2D(3762, 2757, 4, 4),
                    new Rectangle2D(3755, 2769, 4, 4),
                    new Rectangle2D(3762, 2769, 4, 4),
                    new Rectangle2D(3749, 2757, 3, 3),
                    new Rectangle2D(3770, 2757, 3, 3),
                    new Rectangle2D(3770, 2769, 3, 3),
                    new Rectangle2D(3759, 2769, 3, 3)
                },
                new Point3D(3781, 2764, 5),
                new[]
                {
                    new Rectangle2D(3749, 2757, 25, 16)
                },
                new[]
                {
                    new Rectangle2D(3735, 2747, 68, 51)
                },
                new Rectangle2D(3780, 2763, 4, 9),
                new Rectangle2D(3779, 2776, 2, 5));

            _Definitions[0] = HavenFelucca;
        }
    }
}
