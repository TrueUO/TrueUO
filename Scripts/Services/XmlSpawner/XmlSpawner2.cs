using Server.Accounting;
using Server.Commands;
using Server.Commands.Generic;
using Server.ContextMenus;
using Server.Items;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Server.Mobiles
{
    public class XmlSpawner : Item, ISpawner
    {
        public enum TODModeType { Realtime, Gametime }

        public enum SpawnPositionType { Random, RowFill, ColFill, Perimeter, Player, Waypoint, RelXY, DeltaLocation, Location, Wet, Tiles, NoTiles, ItemID, NoItemID }

        public class SpawnPositionInfo
        {
            public SpawnPositionType positionType;
            public Mobile trigMob;
            public string[] positionArgs;

            public SpawnPositionInfo(SpawnPositionType positiontype, Mobile trigmob, string[] positionargs)
            {
                positionType = positiontype;
                trigMob = trigmob;
                positionArgs = positionargs;
            }
        }

        public class MovementInfo
        {
            public Mobile trigMob;
            public Point3D trigLocation = Point3D.Zero;

            public MovementInfo(Mobile m)
            {
                trigMob = m;
                if (m != null)
                    trigLocation = m.Location;
            }
        }

        public const byte MaxLoops = 10; //maximum number of recursive calls from spawner to itself. this is to prevent stack overflow from xmlspawner scripting
        private const int ShowBoundsItemId = 14089;             // 14089 Fire Column // 3555 Campfire // 8708 Skull Pole
        private const string SpawnDataSetName = "Spawns";
        private const string SpawnTablePointName = "Points";
        private const int SpawnFitSize = 16;                    // Normal wall/door height for a mobile is 20 to walk through
        private static int BaseItemId = 0x1F1C;                  // Purple Magic Crystal
        private static int ShowItemId = 0x3E57;                 // ships mast
        private static int defaultTriggerSound = 0x1F4;          // click and sparkle sound by default  (0x1F4) , click sound (0x3A4)
        public static string XmlSpawnDir = "XmlSpawner";            // default directory for saving/loading .xml files with [xmlload [xmlsave
        private const int MaxSmartSectorListSize = 1024;        // maximum sector list size for use in smart spawning. This gives a 512x512 tile range.

        private static string defwaypointname;            // default waypoint name will get assigned in Initialize
        public static AccessLevel DiskAccessLevel = AccessLevel.Administrator; // minimum access level required by commands that can access the disk such as XmlLoad, XmlSave, and the Save function of XmlEdit
        private static int MaxMoveCheck = 10; // limit number of players that can be checked for triggering in a single OnMovement tick

        // specifies the level at which smartspawning will be triggered.  Players with AccessLevel above this will not trigger smartspawning unless unhidden.
        public static AccessLevel SmartSpawnAccessLevel = AccessLevel.Player;

        // define the default values used in making spawners
        private static TimeSpan defMinDelay = TimeSpan.FromMinutes(5);
        private static TimeSpan defMaxDelay = TimeSpan.FromMinutes(10);
        private static TimeSpan defMinRefractory = TimeSpan.FromMinutes(0);
        private static TimeSpan defMaxRefractory = TimeSpan.FromMinutes(0);
        private static TimeSpan defTODStart = TimeSpan.FromMinutes(0);
        private static TimeSpan defTODEnd = TimeSpan.FromMinutes(0);
        private static TimeSpan defDuration = TimeSpan.FromMinutes(0);
        private static TimeSpan defDespawnTime = TimeSpan.FromHours(0);
        private static bool defIsGroup;
        private static int defTeam;
        private static int defProximityTriggerSound = defaultTriggerSound;
        private static int defAmount = 1;
        private static bool defRelativeHome = true;
        private static int defSpawnRange = 5;
        private static int defHomeRange = 5;
        private static double defTriggerProbability = 1;
        private static int defProximityRange = -1;
        private static readonly int defKillReset = 1;
        private static TODModeType defTODMode = TODModeType.Realtime;

        private static Timer m_GlobalSectorTimer;
        private static bool SmartSpawningSystemEnabled;

        private static WarnTimer2 m_WarnTimer;

        // hash table for optimizing HoldSmartSpawning method invocation
        private static Dictionary<Type, PropertyInfo> holdSmartSpawningHash;

        public static int seccount;

        // sector hashtable for each map
        private static readonly Dictionary<Sector, List<XmlSpawner>>[] GlobalSectorTable = new Dictionary<Sector, List<XmlSpawner>>[6];

        #region Variable declarations
        private string m_Name = string.Empty;
        private string m_UniqueId = string.Empty;
        private bool m_HomeRangeIsRelative;
        private int m_Team;
        private int m_HomeRange;
        private int m_StackAmount;
        private int m_SpawnRange;
        private int m_Count;
        private TimeSpan m_MinDelay;
        private TimeSpan m_MaxDelay;
        private TimeSpan m_Duration;
        public List<SpawnObject> m_SpawnObjects = new List<SpawnObject>(); // List of objects to spawn
        private DateTime m_End;
        private DateTime m_RefractEnd;
        private DateTime m_DurEnd;
        private SpawnerTimer m_Timer;
        private InternalTimer m_DurTimer;
        private InternalTimer3 m_RefractoryTimer;
        private bool m_Running;
        private bool m_Group;
        private int m_X;
        private int m_Y;
        private int m_Width;
        private int m_Height;
        private WayPoint m_WayPoint;

        private Static m_ShowContainerStatic;
        private bool m_proximityActivated;
        private bool m_refractActivated;
        private bool m_durActivated;
        private TimeSpan m_TODEnd;
        private TimeSpan m_MinRefractory;
        private TimeSpan m_MaxRefractory;
        private string m_ItemTriggerName;
        private string m_NoItemTriggerName;
        private Item m_ObjectPropertyItem;
        private string m_ObjectPropertyName;
        public string status_str;
        public int m_killcount;
        private int m_ProximityRange;
        private string m_ProximityTriggerMessage;
        private string m_SpeechTrigger;
        private bool m_speechTriggerActivated;
        private string m_MobPropertyName;
        private string m_MobTriggerName;
        private string m_PlayerPropertyName;
        private double m_TriggerProbability = defTriggerProbability;
        private Mobile m_mob_who_triggered;
        private Item m_SetPropertyItem;

        private bool m_skipped;
        private int m_KillReset = defKillReset; // number of spawn ticks that pass without kills before killcount gets reset to zero
        private int m_spawncheck;
        private TODModeType m_TODMode = TODModeType.Realtime;
        private bool m_ExternalTriggering;
        private bool m_ExternalTrigger;
        private int m_SequentialSpawning = -1; // off by default
        private DateTime m_SeqEnd;
        private Region m_Region;
        private string m_RegionName = string.Empty; 
        private AccessLevel m_TriggerAccessLevel = AccessLevel.Player;

        public List<XmlTextEntryBook> m_TextEntryBook;
        private XmlSpawnerGump m_SpawnerGump;

        private bool m_AllowGhostTriggering;
        private bool m_AllowNPCTriggering;
        private bool m_OnHold;
        private bool m_HoldSequence;
        private bool m_SpawnOnTrigger;

        private List<MovementInfo> m_MovementList;
        private MovementTimer m_MovementTimer;
        internal List<BaseXmlSpawner.KeywordTag> m_KeywordTagList = new List<BaseXmlSpawner.KeywordTag>();

        public List<XmlSpawner> RecentSpawnerSearchList = null;
        public List<Item> RecentItemSearchList = null;
        public List<Mobile> RecentMobileSearchList = null;
        private TimeSpan m_DespawnTime;

        private string m_SkillTrigger;
        private SkillName m_skill_that_triggered;
        private bool m_FreeRun;     // override for all other triggering modes

        private Map currentmap;

        public bool m_IsInactivated;
        private bool m_SmartSpawning;
        private SectorTimer m_SectorTimer;

        private List<Static> m_ShowBoundsItems = new List<Static>();

        public List<BaseXmlSpawner.TypeInfo> PropertyInfoList = null;   // used to optimize property info lookup used by set and get property methods.

        private Dictionary<string, List<Item>> spawnPositionWayTable;  // used to optimize #waypoint lookup

        private bool inrespawn;

        private List<Sector> sectorList;

        private Point3D mostRecentSpawnPosition = Point3D.Zero;

        #endregion

        // does not decay
        public override bool Decays => false;
        // is not counted in the normal item count
        public override bool IsVirtualItem => true;

        #region Properties
        public bool DebugThis { get; set; } = false;

        public int MovingPlayerCount { get; set; }

        public int FastestPlayerSpeed { get; set; }

        public int NearbyPlayerCount
        {
            get
            {
                int count = 0;
                if (ProximityRange >= 0)
                {
                    IPooledEnumerable eable = GetMobilesInRange(ProximityRange);

                    foreach (Mobile m in eable)
                    {
                        if (m != null && m.Player) count++;
                    }

                    eable.Free();
                }

                return count;
            }
        }

        public Point3D MostRecentSpawnPosition
        {
            get => mostRecentSpawnPosition;
            set => mostRecentSpawnPosition = value;
        }

        public TimeSpan GameTOD
        {
            get
            {
                int hours;
                int minutes;

                Clock.GetTime(Map, Location.X, Location.Y, out hours, out minutes);

                return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hours, minutes, 0).TimeOfDay;
            }
        }

        public XmlSpawnerGump SpawnerGump
        {
            get => m_SpawnerGump;
            set => m_SpawnerGump = value;
        }

        public bool m_DisableGlobalAutoReset { get; set; }

        public bool DoDefrag
        {
            get => false;
            set
            {
                if (value)
                {
                    Defrag(true);
                }
            }
        }

        private readonly bool sectorIsActive = false;
        private bool UseSectorActivate = false;

        public bool SingleSector => UseSectorActivate;

        public bool InActivationRange(Sector s1, Sector s2)
        {
            // check to see if the sectors are within +- 2 of one another
            if (s1 == null || s2 == null) return false;

            return (Math.Abs(s1.X - s2.X) < 3 && Math.Abs(s1.Y - s2.Y) < 3);
        }

        public bool HasDamagedOrDistantSpawns
        {
            get
            {
                Sector ssec = Map.GetSector(Location);
                // go through the spawn lists
                for (var index = 0; index < m_SpawnObjects.Count; index++)
                {
                    SpawnObject so = m_SpawnObjects[index];

                    for (int x = 0; x < so.SpawnedObjects.Count; x++)
                    {
                        object o = so.SpawnedObjects[x];

                        if (o is BaseCreature b)
                        {
                            // if the mob is damaged or outside of smartspawning detection range then return true
                            if (b.Hits < b.HitsMax || b.Mana < b.ManaMax || b.Stam < b.StamMax || b.Map != Map)
                            {
                                return true;
                            }

                            // if the spawn moves into a sector that is not activatable from a sector on the sector list then dont smartspawn
                            if (b.Map != null && b.Map != Map.Internal)
                            {
                                Sector bsec = b.Map.GetSector(b.Location);

                                if (UseSectorActivate)
                                {
                                    // is it in activatable range of the sector the spawner is in
                                    if (!InActivationRange(bsec, ssec))
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    bool outofsec = true;

                                    if (sectorList != null)
                                    {
                                        for (var i = 0; i < sectorList.Count; i++)
                                        {
                                            Sector s = sectorList[i];
                                            // is the creatures sector within activation range of any of the sectors in the list
                                            if (InActivationRange(bsec, s))
                                            {
                                                outofsec = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (outofsec)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        public bool HasActiveSectors
        {
            get
            {
                if (!SmartSpawning || Map == null || Map == Map.Internal)
                {
                    return false;
                }

                // is this a region spawner?
                if (m_Region != null)
                {
                    List<Mobile> players = m_Region.GetPlayers();

                    if (players == null || players.Count == 0)
                    {
                        return false;
                    }

                    // confirm that players with the proper access level are present
                    for (var index = 0; index < players.Count; index++)
                    {
                        Mobile m = players[index];

                        if (m != null && (m.AccessLevel <= SmartSpawnAccessLevel || !m.Hidden))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                // is this a single sector spawner?
                if (UseSectorActivate)
                {
                    return sectorIsActive;
                }

                // if there is no sector list made for this spawner then create one.
                if (sectorList == null)
                {
                    Point3D loc = Location;
                    sectorList = new List<Sector>();

                    // is this container held?
                    if (Parent != null)
                    {
                        if (RootParent is Mobile mobile)
                        {
                            loc = mobile.Location;
                        }
                        else if (RootParent is Item item)
                        {
                            loc = item.Location;
                        }
                    }

                    // find the max detection range by examining both spawnrange 
                    // note, sectors will activate when within +-2 sectors
                    int bufferzone = 2 * Map.SectorSize;
                    int x1 = m_X - bufferzone;
                    int width = m_Width + 2 * bufferzone;
                    int y1 = m_Y - bufferzone;
                    int height = m_Height + 2 * bufferzone;

                    // go through all of the sectors within the SpawnRange of the spawner to see if any are active
                    for (int x = x1; x <= x1 + width; x += Map.SectorSize)
                    {
                        for (int y = y1; y <= y1 + height; y += Map.SectorSize)
                        {
                            Sector s = Map.GetSector(new Point3D(x, y, loc.Z));

                            if (s == null)
                            {
                                continue;
                            }

                            // dont add any redundant sectors
                            bool duplicate = false;

                            for (var index = 0; index < sectorList.Count; index++)
                            {
                                Sector olds = sectorList[index];

                                if (olds == s)
                                {
                                    duplicate = true;
                                    break;
                                }
                            }

                            if (!duplicate)
                            {
                                sectorList.Add(s);

                                if (GlobalSectorTable[Map.MapID] == null)
                                {
                                    GlobalSectorTable[Map.MapID] = new Dictionary<Sector, List<XmlSpawner>>();
                                }

                                // add this sector and the spawner associated with it to the global sector table
                                List<XmlSpawner> spawnerlist;
                                if (GlobalSectorTable[Map.MapID].TryGetValue(s, out spawnerlist))//.Contains(s))
                                {
                                    //List<XmlSpawner> spawnerlist = GlobalSectorTable[Map.MapID][s];
                                    if (spawnerlist == null)
                                    {
                                        //GlobalSectorTable[Map.MapID].Remove(s);
                                        spawnerlist = new List<XmlSpawner>();
                                        //GlobalSectorTable[Map.MapID].Add(s, spawnerlist);
                                        GlobalSectorTable[Map.MapID][s] = spawnerlist;
                                    }

                                    if (!spawnerlist.Contains(this))
                                    {
                                        spawnerlist.Add(this);

                                    }
                                }
                                else
                                {
                                    spawnerlist = new List<XmlSpawner>();
                                    spawnerlist.Add(this);
                                    // add a new entry to the table
                                    GlobalSectorTable[Map.MapID][s] = spawnerlist;
                                }

                                // add some sanity checking here
                                if (sectorList.Count > MaxSmartSectorListSize)
                                {
                                    SmartSpawning = false;

                                    // log it
                                    try
                                    {
                                        Console.WriteLine("SmartSpawning disabled at {0} {1} : Range too large.", loc, Map);

                                        using (StreamWriter op = new StreamWriter("badspawn.log", true))
                                        {
                                            op.WriteLine("{0} SmartSpawning disabled at {1} {2} : Range too large.", DateTime.UtcNow, loc, Map);
                                            op.WriteLine();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Diagnostics.ExceptionLogging.LogException(e);
                                    }

                                    return true;
                                }
                            }
                        }
                    }

                    UseSectorActivate = false;
                }

                _TraceStart(2);
                // go through the sectorlist and see if any of the sectors are active

                for (var index = 0; index < sectorList.Count; index++)
                {
                    Sector s = sectorList[index];

                    if (s != null && s.Active && s.Players != null && s.Players.Count > 0)
                    {
                        // confirm that players with the proper access level are present
                        for (var i = 0; i < s.Players.Count; i++)
                        {
                            Mobile m = s.Players[i];

                            if (m != null && (m.AccessLevel <= SmartSpawnAccessLevel || !m.Hidden))
                            {
                                return true;
                            }
                        }

                        _TraceEnd(2);
                    }

                    seccount++;
                }

                _TraceEnd(2);
                return false;
            }
        }

        public int SecCount => seccount;

        public bool IsInactivated
        {
            get => m_IsInactivated;
            set => m_IsInactivated = value;
        }

        public int ActiveSectorCount
        {
            get
            {
                if (sectorList != null) return sectorList.Count;
                return 0;
            }
        }

        public bool OnHold
        {
            get
            {
                if (m_OnHold)
                    return true;

                // determine whether there are any keywordtags with the hold flag
                if (m_KeywordTagList == null || m_KeywordTagList.Count == 0) return false;

                for (var index = 0; index < m_KeywordTagList.Count; index++)
                {
                    BaseXmlSpawner.KeywordTag sot = m_KeywordTagList[index];

                    // check for any keyword tag with the holdspawn flag
                    if (sot != null && !sot.Deleted && (sot.Flags & BaseXmlSpawner.KeywordFlags.HoldSpawn) != 0)
                    {
                        return true;
                    }
                }

                // no hold flags were set
                return false;
            }

            set => m_OnHold = value;
        }

        public string AddSpawn
        {
            get => null;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string str = value.Trim();
                    string typestr = BaseXmlSpawner.ParseObjectType(str);

                    Type type = SpawnerType.GetType(typestr);

                    if (type != null)
                        m_SpawnObjects.Add(new SpawnObject(str, 1));
                    else
                    {
                        // check for special keywords
                        if (typestr != null && (BaseXmlSpawner.IsTypeOrItemKeyword(typestr) || typestr.IndexOf("{") != -1 || typestr.StartsWith("*") || typestr.StartsWith("#")))
                        {
                            m_SpawnObjects.Add(new SpawnObject(str, 1));
                        }
                        else
                            status_str = $"{str} is not a valid type name.";
                    }

                    InvalidateProperties();
                }
            }
        }

        public string UniqueId => m_UniqueId;

        // does not perform a defrag, so less accurate but can be used while looping through world object enums
        public int SafeCurrentCount => SafeTotalSpawnedObjects;

        public bool FreeRun
        {
            get => m_FreeRun;
            set => m_FreeRun = value;
        }

        public bool CanFreeSpawn
        {
            get
            {
                // allow free spawning if proximity sensing is off and if all of the potential free-spawning triggers are disabled
                if (Running && m_ProximityRange == -1 && string.IsNullOrEmpty(m_ObjectPropertyName) &&
                    (string.IsNullOrEmpty(m_MobPropertyName) || m_MobTriggerName == null ||
                     m_MobTriggerName.Length == 0) && !m_ExternalTriggering)
                {
                    return true;
                }

                return false;
            }
        }

        public SpawnObject[] SpawnObjects
        {
            get => m_SpawnObjects.ToArray();
            set
            {
                if (value != null && value.Length > 0)
                {
                    for (var index = 0; index < value.Length; index++)
                    {
                        SpawnObject so = value[index];

                        if (so == null)
                        {
                            continue;
                        }

                        bool AlreadyInList = false;

                        // Check if the new array has an existing spawn object
                        for (var i = 0; i < m_SpawnObjects.Count; i++)
                        {
                            SpawnObject TheSpawn = m_SpawnObjects[i];

                            if (TheSpawn.TypeName.ToUpper() == so.TypeName.ToUpper())
                            {
                                AlreadyInList = true;
                                break;
                            }
                        }

                        // Does this item need to be added
                        if (!AlreadyInList)
                        {
                            // This is a new spawn object so add it to the array (deep copy)
                            m_SpawnObjects.Add(new SpawnObject(so.TypeName, so.ActualMaxCount, so.SubGroup,
                                so.SequentialResetTime, so.SequentialResetTo, so.KillsNeeded,
                                so.RestrictKillsToSubgroup, so.ClearOnAdvance, so.MinDelay, so.MaxDelay,
                                so.SpawnsPerTick, so.PackRange));
                        }
                    }

                    if (SpawnObjects.Length < 1)
                        Stop();

                    InvalidateProperties();
                }
            }
        }

        public bool HoldSequence
        {
            get
            {
                // check to see if any keyword tags have the holdsequence flag set, or whether the spawner holdsequence flag is set
                if (m_HoldSequence)
                {
                    return true;
                }

                // determine whether there are any keywordtags with the hold flag
                if (m_KeywordTagList == null || m_KeywordTagList.Count == 0)
                {
                    return false;
                }

                for (var index = 0; index < m_KeywordTagList.Count; index++)
                {
                    BaseXmlSpawner.KeywordTag sot = m_KeywordTagList[index];
                    // check for any keyword tag with the holdsequence flag
                    if (sot != null && !sot.Deleted && (sot.Flags & BaseXmlSpawner.KeywordFlags.HoldSequence) != 0)
                    {
                        return true;
                    }
                }

                // no hold flags were set
                return false;
            }

            set => m_HoldSequence = value;
        }

        public bool CanSpawn
        {
            get
            {
                if (OnHold)
                    return false;

                if (m_Group)
                {
                    if (TotalSpawnedObjects <= 0)
                    {
                        return true;
                    }

                    return false;
                }

                if (IsFull)
                {
                    return false;
                }

                return true;
            }
        }

        public bool IsFull // test for a full spawner
        {
            get
            {
                int nobj = TotalSpawnedObjects;

                return nobj >= m_Count || nobj >= TotalSpawnObjectCount;
            }
        }

        public int SafeTotalSpawnedObjects // this can be used in loops over world objects since it will not defrag and potentially modify the world object lists
        {
            get
            {
                if (m_SpawnObjects == null) return 0;

                int count = 0;

                for (var index = 0; index < m_SpawnObjects.Count; index++)
                {
                    SpawnObject so = m_SpawnObjects[index];
                    count += so.SpawnedObjects.Count;
                }

                return count;
            }
        }

        public int TotalSpawnedObjects
        {
            get
            {
                if (m_SpawnObjects == null)
                {
                    return 0;
                }

                // defrag so that accurately reflects currently active spawns
                Defrag(true);

                int count = 0;

                for (var index = 0; index < m_SpawnObjects.Count; index++)
                {
                    SpawnObject so = m_SpawnObjects[index];
                    count += so.SpawnedObjects.Count;
                }

                return count;
            }
        }

        public bool isEmpty()
        {
            if (m_SpawnObjects == null)
            {
                return true;
            }

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                if (so.SpawnedObjects != null && so.SpawnedObjects.Count > 0)
                {
                    if (so.SpawnedObjects[0] is Mobile)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int TotalSpawnObjectCount
        {
            get
            {
                int count = 0;

                for (var index = 0; index < m_SpawnObjects.Count; index++)
                {
                    SpawnObject so = m_SpawnObjects[index];
                    count += so.MaxCount;
                }

                return count;
            }
        }
        #endregion

        #region Command Properties

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GumpReset
        {

            set
            {
                if (value)
                {
                    m_SpawnerGump = null;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Region SpawnRegion
        {
            get => m_Region;
            set
            {
                // force a re-update of the smart spawning sector list the next time it is accessed
                ResetSectorList();

                m_Region = value;

                m_RegionName = m_Region?.Name;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string RegionName
        {
            get => m_RegionName;
            set
            {
                // force a re-update of the smart spawning sector list the next time it is accessed
                ResetSectorList();

                m_RegionName = value;

                if (string.IsNullOrEmpty(value))
                {
                    m_Region = null;
                    return;
                }

                if (Region.Regions.Count == 0)  // after world load, before region load
                    return;

                for (var index = 0; index < Region.Regions.Count; index++)
                {
                    Region region = Region.Regions[index];

                    if (string.Compare(region.Name, m_RegionName, true) == 0)
                    {
                        m_Region = region;
                        m_RegionName = region.Name;
                        //InvalidateProperties();
                        return;
                    }
                }

                status_str = "invalid region: " + value;
                m_Region = null;
            }
        }


        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D X1_Y1
        {
            get => new Point3D(m_X, m_Y, Z);
            set
            {
                // X1 and Y1 will initiate region specification
                m_Width = 0;
                m_Height = 0;
                m_X = value.X;
                m_Y = value.Y;

                // reset the sector list
                ResetSectorList();

                m_SpawnRange = 0;

                if (ShowBounds)
                {
                    ShowBounds = false;
                    ShowBounds = true;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D X2_Y2
        {
            get => new Point3D((m_X + m_Width), (m_Y + m_Height), Z);
            set
            {
                int X2;
                int Y2;

                int OriginalX2 = m_X + m_Width;
                int OriginalY2 = m_Y + m_Height;

                // reset the sector list
                ResetSectorList();

                // now determine based upon the entered coordinate values what the lower left corner is
                // lower left will be the min x and min y
                // upper right will be max x max y
                if (value.X < OriginalX2)
                {
                    // ok, this is the proper x value for the lower left
                    m_X = value.X;
                    X2 = OriginalX2;
                }
                else
                {
                    m_X = OriginalX2;
                    X2 = value.X;
                }

                if (value.Y < OriginalY2)
                {
                    // ok, this is the proper y value for the lower left
                    m_Y = value.Y;
                    Y2 = OriginalY2;
                }
                else
                {
                    m_Y = OriginalY2;
                    Y2 = value.Y;
                }

                m_Width = X2 - m_X;
                m_Height = Y2 - m_Y;

                if (m_Width == m_Height)
                    m_SpawnRange = m_Width / 2;
                else
                    m_SpawnRange = -1;

                if (m_HomeRangeIsRelative == false)
                {
                    int NewHomeRange = (m_Width > m_Height ? m_Height : m_Width);
                    m_HomeRange = (NewHomeRange > 0 ? NewHomeRange : 0);
                }

                // Stop the spawner if the width or height is less than 1
                if (m_Width < 0 || m_Height < 0)
                    Running = false;

                InvalidateProperties();

                if (ShowBounds)
                {
                    ShowBounds = false;
                    ShowBounds = true;
                }
            }
        }

        // added the spawnrange property.  It sets both the XY and width/height parameters automatically.
        // also doesnt mess with homerange like XY does
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange
        {
            get => m_SpawnRange;
            set
            {
                if (value < 0) return;

                // reset the sector list
                ResetSectorList();

                m_SpawnRange = value;
                m_Width = m_SpawnRange * 2;
                m_Height = m_SpawnRange * 2;

                // dont set the bounding box locations if the initial location is 0,0 since this occurs when the item is just being made
                // because m_X and m_Y are restored on loading, it creates problems with OnLocationChange which has to avoid applying translational
                // adjustments to newly placed spawners (because the actual m_X and m_Y is associated with the original location, not the 0,0 location)
                // basically, before placement, dont set m_X or m_Y to anything that needs to be adjusted later on

                if (Location.X == 0 && Location.Y == 0) return;

                m_X = Location.X - m_SpawnRange;
                m_Y = Location.Y - m_SpawnRange;

                if (ShowBounds)
                {
                    ShowBounds = false;
                    ShowBounds = true;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ShowBounds
        {
            get => m_ShowBoundsItems != null && m_ShowBoundsItems.Count > 0;
            set
            {
                if (value && !ShowBounds)
                {
                    if (m_ShowBoundsItems == null) m_ShowBoundsItems = new List<Static>();

                    // Boundary lines
                    int ValidX1 = m_X;
                    int ValidX2 = m_X + m_Width;
                    int ValidY1 = m_Y;
                    int ValidY2 = m_Y + m_Height;

                    for (int x = 0; x <= m_Width; x++)
                    {
                        int NewX = m_X + x;
                        for (int y = 0; y <= m_Height; y++)
                        {
                            int NewY = m_Y + y;

                            if (NewX == ValidX1 || NewX == ValidX2 || NewX == ValidY1 || NewX == ValidY2 || NewY == ValidX1 || NewY == ValidX2 || NewY == ValidY1 || NewY == ValidY2)
                            {
                                // Add an object to show the spawn area
                                Static s = new Static(ShowBoundsItemId)
                                {
                                    Visible = false
                                };
                                s.MoveToWorld(new Point3D(NewX, NewY, Z), Map);
                                m_ShowBoundsItems.Add(s);
                            }
                        }
                    }
                }

                if (value == false && m_ShowBoundsItems != null)
                {
                    // Remove all of the items from the array
                    for (var index = 0; index < m_ShowBoundsItems.Count; index++)
                    {
                        Static s = m_ShowBoundsItems[index];
                        s.Delete();
                    }

                    m_ShowBoundsItems.Clear();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxCount
        {
            get => m_Count;
            set
            {
                m_Count = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurrentCount => TotalSpawnedObjects;

        [CommandProperty(AccessLevel.GameMaster)]
        public WayPoint WayPoint
        {
            get => m_WayPoint;
            set => m_WayPoint = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ExternalTriggering
        {
            get => m_ExternalTriggering;
            set => m_ExternalTriggering = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ExtTrigState
        {
            get => m_ExternalTrigger;
            set => m_ExternalTrigger = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Running
        {
            get => m_Running;
            set
            {
                // Don't start the spawner unless the height and width are valid
                if (value && (m_Width >= 0) && (m_Height >= 0))
                {
                    Start();
                }
                else
                    Stop();

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HomeRange
        {
            get => m_HomeRange;
            set { m_HomeRange = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HomeRangeIsRelative
        {
            get => m_HomeRangeIsRelative;
            set => m_HomeRangeIsRelative = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Team
        {
            get => m_Team;
            set { m_Team = value; InvalidateProperties(); }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int StackAmount
        {
            get => m_StackAmount;
            set => m_StackAmount = value;
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MinDelay
        {
            get => m_MinDelay;
            set
            {
                m_MinDelay = value;
                // reset the spawn timer
                DoTimer();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan MaxDelay
        {
            get => m_MaxDelay;
            set
            {
                m_MaxDelay = value;
                // reset the spawn timer
                DoTimer();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int KillCount
        {
            get => m_killcount;
            set => m_killcount = value;
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int KillReset
        {
            get => m_KillReset;
            set => m_KillReset = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double TriggerProbability
        {
            get => m_TriggerProbability;
            set => m_TriggerProbability = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RefractMin
        {
            get => m_MinRefractory;
            set => m_MinRefractory = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RefractMax
        {
            get => m_MaxRefractory;
            set => m_MaxRefractory = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RefractoryOver
        {
            get
            {
                if (m_refractActivated)
                {
                    return m_RefractEnd - DateTime.UtcNow;
                }

                return TimeSpan.FromSeconds(0);
            }

            set => DoTimer3(value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string SetItemName
        {
            get
            {
                if (m_SetPropertyItem == null || m_SetPropertyItem.Deleted)
                    return null;

                return m_SetPropertyItem.Name;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Item SetItem
        {
            get => m_SetPropertyItem;
            set => m_SetPropertyItem = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string MobTriggerProp
        {
            get => m_MobPropertyName;
            set => m_MobPropertyName = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string MobTriggerName
        {
            get => m_MobTriggerName;
            set => m_MobTriggerName = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile MobTriggerId
        {
            get
            {
                if (m_MobTriggerName == null) return null;

                // try to parse out the type information if it has also been saved
                string[] typeargs = m_MobTriggerName.Split(",".ToCharArray(), 2);
                string typestr = null;
                string namestr = m_MobTriggerName;

                if (typeargs.Length > 1)
                {
                    namestr = typeargs[0];
                    typestr = typeargs[1];
                }
                return BaseXmlSpawner.FindMobileByName(this, namestr, typestr);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string PlayerTriggerProp
        {
            get => m_PlayerPropertyName;
            set => m_PlayerPropertyName = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan m_TODStart { get; set; } // time of day activation

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TODEnd
        {
            get => m_TODEnd;
            set => m_TODEnd = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TOD
        {
            get
            {
                if (m_TODMode == TODModeType.Gametime)
                {
                    int hours;
                    int minutes;
                    Clock.GetTime(Map, Location.X, Location.Y, out hours, out minutes);
                    return (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hours, minutes, 0).TimeOfDay);
                }

                return DateTime.UtcNow.TimeOfDay;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TODModeType TODMode
        {
            get => m_TODMode;
            set => m_TODMode = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool TODInRange
        {
            get
            {
                if (m_TODStart == m_TODEnd) return true;
                DateTime now;

                if (m_TODMode == TODModeType.Gametime)
                {
                    int hours;
                    int minutes;
                    Clock.GetTime(Map, Location.X, Location.Y, out hours, out minutes);
                    now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, hours, minutes, 0);
                }
                else
                {
                    // calculate the time window
                    now = DateTime.UtcNow;
                }
                var day_start = new DateTime(now.Year, now.Month, now.Day);
                // calculate the starting TOD window by adding the TODStart to day_start
                var TOD_start = day_start + m_TODStart;
                var TOD_end = day_start + m_TODEnd;

                // handle the case when TODstart is before midnight and end is after

                if (TOD_start > TOD_end)
                {
                    if (now > TOD_start || now < TOD_end)
                    {
                        return true;
                    }

                    return false;
                }

                if (now > TOD_start && now < TOD_end)
                {
                    return true;
                }

                return false;

            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DespawnTime
        {
            get => m_DespawnTime;
            set => m_DespawnTime = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan Duration
        {
            get => m_Duration;
            set
            {
                m_Duration = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DurationOver
        {
            get
            {
                if (m_durActivated)
                {
                    return m_DurEnd - DateTime.UtcNow;
                }

                return TimeSpan.FromSeconds(0);
            }
            set => DoTimer2(value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ProximityRange
        {
            get => m_ProximityRange;
            set
            {
                m_ProximityRange = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ProximityActivated
        {
            get => m_proximityActivated;
            set
            {
                if (AllowTriggering)
                {
                    ActivateTrigger();
                }

                m_proximityActivated = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int m_ProximityTriggerSound { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ProximityMsg 
        {
            get => m_ProximityTriggerMessage;
            set => m_ProximityTriggerMessage = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string SpeechTrigger
        {
            get => m_SpeechTrigger;
            set => m_SpeechTrigger = value;
        }

        public string SkillTrigger
        {
            get => m_SkillTrigger;
            set => m_SkillTrigger = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextSpawn
        {
            get
            {
                if (m_Running)
                {
                    return m_End - DateTime.UtcNow;
                }

                return TimeSpan.FromSeconds(0);
            }
            set
            {
                Start();
                DoTimer(value);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SpawnOnTrigger
        {
            get => m_SpawnOnTrigger;
            set => m_SpawnOnTrigger = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Group
        {
            get => m_Group;
            set { m_Group = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SequentialSpawn
        {
            get => m_SequentialSpawning;
            set => m_SequentialSpawning = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextSeqReset
        {
            get
            {
                if (m_Running && (m_SeqEnd - DateTime.UtcNow) > TimeSpan.Zero)
                {
                    return m_SeqEnd - DateTime.UtcNow;
                }

                return TimeSpan.FromSeconds(0);
            }

            set => m_SeqEnd = DateTime.UtcNow + value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AccessLevel TriggerAccessLevel
        {
            get => m_TriggerAccessLevel;
            set => m_TriggerAccessLevel = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DoRespawn
        {
            get => false;
            set
            {
                // need to determine whether this is being set by the spawner during processing of a respawn entry
                // if so then dont do it, otherwise you will infinitely recurse and crash with a stack overflow
                if (value && !inrespawn)
                {
                    Respawn();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DoReset
        {
            get => false;
            set { if (value) Reset(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowGhostTrig
        {
            get => m_AllowGhostTriggering;
            set => m_AllowGhostTriggering = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowNPCTrig
        {
            get => m_AllowNPCTriggering;
            set => m_AllowNPCTriggering = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile TriggerMob
        {
            get => m_mob_who_triggered;
            set => m_mob_who_triggered = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SmartSpawning
        {
            get => m_SmartSpawning;
            set
            {
                m_SmartSpawning = value;

                if (m_SmartSpawning)
                {
                    // if any spawner is smartspawning, then the smartspawning system is enabled
                    SmartSpawningSystemEnabled = true;
                    // check to see if the global sector timer is running
                    if (m_GlobalSectorTimer == null || !m_GlobalSectorTimer.Running)
                    {
                        // start the global smartspawning timer
                        DoGlobalSectorTimer(TimeSpan.FromSeconds(1));
                    }
                }

                //IsInactivated = false; 
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsEmpty => isEmpty();

        #endregion

        #region ISpawner interface support

        public bool UnlinkOnTaming => true;
        public Point3D HomeLocation => Location;
        public int Range => HomeRange;

        public virtual void GetSpawnProperties(ISpawnable spawn, ObjectPropertyList list)
        { }

        public virtual void GetSpawnContextEntries(ISpawnable spawn, Mobile user, List<ContextMenuEntry> list)
        { }

        public void Remove(ISpawnable spawn)
        {
            if (m_SpawnObjects == null) return;

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                {
                    if (so.SpawnedObjects[i] == spawn)
                    {
                        so.SpawnedObjects.Remove(spawn);
                        if (SequentialSpawn >= 0 && so.RestrictKillsToSubgroup)
                        {
                            if (so.SubGroup == SequentialSpawn)
                            {
                                m_killcount++;
                            }
                        }
                        else
                        {
                            m_killcount++;
                        }

                        return;
                    }
                }
            }
        }

        public void RestoreISpawner()
        {
            // restore the Spawner assignments to all spawned objects
            if (m_SpawnObjects == null) return;

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                {
                    object o = so.SpawnedObjects[i];
                    if (o is Item item)
                    {
                        item.Spawner = this;
                    }
                    else if (o is Mobile mobile)
                    {
                        mobile.Spawner = this;
                    }
                }
            }
        }

        #endregion

        #region Method Overrides

        public override void OnAfterDuped(Item newItem)
        {
            ((XmlSpawner)newItem).Running = false; // automatically turn off duped spawners

            base.OnAfterDuped(newItem);
        }

        public override void OnMapChange()
        {
            base.OnMapChange();

            currentmap = Map;

            ResetSectorList(); // reset the sector list for smart spawning
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from == null || from.Deleted || from.AccessLevel < AccessLevel.GameMaster || m_SpawnerGump != null && SomeOneHasGumpOpen)
                return;

            DeleteTextEntryBook(); // clear any text entry books that might still be around

            int x = 0;
            int y = 0;

            if (from.Account is Account acct)
            {
                XmlSpawnerDefaults.DefaultEntry defs = XmlSpawnerDefaults.GetDefaults(acct.ToString(), from.Name);
                if (defs != null)
                {
                    x = defs.SpawnerGumpX;
                    y = defs.SpawnerGumpY;
                }
            }

            XmlSpawnerGump g = new XmlSpawnerGump(this, x, y, 0, 0, 0);
            from.SendGump(g);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(m_Running ? 1060742 : 1060743); // Active - Inactive

            // add whitespace to the beginning to avoid any problem with names that begin with # and are interpreted as cliloc ids
            list.Add(1042971, " " + Name); // ~1_val~
            list.Add(1060656, m_Count.ToString()); // amount to make: ~1_val~
            list.Add(1061169, m_HomeRange.ToString()); // range ~1_val~

            int nlist_items = 6;

            if (m_Group)
            {
                list.Add(1060658 + 6 - nlist_items, $"group\t{m_Group}"); // ~1_val~: ~2_val~
                nlist_items--;
            }

            if (m_Team != 0)
            {
                list.Add(1060658 + 6 - nlist_items, $"team\t{m_Team}"); // ~1_val~: ~2_val~
                nlist_items--;
            }

            list.Add(1060658 + 6 - nlist_items, $"speed\t{m_MinDelay} to {m_MaxDelay}"); // ~1_val~: ~2_val~
            nlist_items--;

            // display the duration parameter in the prop gump if it is non-zero
            if (m_Duration > TimeSpan.FromMinutes(0))
            {
                list.Add(1060658 + 6 - nlist_items, $"Duration\t{m_Duration}");
                nlist_items--;
            }

            // display the proximity range parameter in the prop gump if it is active
            if (m_ProximityRange != -1)
            {
                list.Add(1060658 + 6 - nlist_items, $"ProximityRange\t{m_ProximityRange}");
                nlist_items--;
            }

            if (m_SpawnObjects != null)
                for (int i = 0; i < nlist_items && i < m_SpawnObjects.Count; ++i)
                {
                    string typename = m_SpawnObjects[i].TypeName;
                    if (typename != null && typename.Length > 20)
                    {
                        typename = typename.Substring(0, 20);
                    }

                    list.Add(1060658 + (6 - nlist_items) + i, " {0}\t{1}", typename, m_SpawnObjects[i].SpawnedObjects.Count);
                }
        }

        public override void OnDelete()
        {
            base.OnDelete();

            if (ShowBounds)
                ShowBounds = false;

            RemoveSpawnObjects();

            // remove any text entry books that might still be attached to the spawner
            DeleteTextEntryBook();

            if (m_Timer != null)
                m_Timer.Stop();

            if (m_DurTimer != null)
                m_DurTimer.Stop();

            if (m_RefractoryTimer != null)
                m_RefractoryTimer.Stop();

            // if statics were added for marking container held spawners, delete them
            if (m_ShowContainerStatic != null && !m_ShowContainerStatic.Deleted)
                m_ShowContainerStatic.Delete();
        }

        static bool IgnoreLocationChange = false;
        public override void OnLocationChange(Point3D oldLocation)
        {
            if (IgnoreLocationChange)
            {
                IgnoreLocationChange = false;
                return;
            }

            // calculate the positional shift
            if (oldLocation.X > 0 && oldLocation.Y > 0)
            {
                int diffx = X - oldLocation.X;
                int diffy = Y - oldLocation.Y;
                m_X += diffx;
                m_Y += diffy;
            }
            else
            {
                // Keep the original dimensions the same (Width, Height),
                // just recalculate the new top left corner
                m_X = X - (m_Width / 2);
                m_Y = Y - (m_Height / 2);
            }

            // reset the sector list for smart spawning
            ResetSectorList();

            // Check if the spawner is showing its bounds
            if (ShowBounds)
            {
                ShowBounds = false;
                ShowBounds = true;
            }
        }
        #endregion

        #region Gump support
        public bool SomeOneHasGumpOpen
        {
            get
            {
                // go through all online mobiles and see if any have xmlspawner gumps open
                List<NetState> states = NetState.Instances;

                for (int i = 0; i < states.Count; ++i)
                {
                    Mobile m = states[i].Mobile;

                    if (m != null && m.HasGump(typeof(XmlSpawnerGump)))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void DeleteTextEntryBook()
        {
            if (m_TextEntryBook != null)
            {
                for (var index = 0; index < m_TextEntryBook.Count; index++)
                {
                    XmlTextEntryBook s = m_TextEntryBook[index];
                    s.Delete();
                }

                m_TextEntryBook = null;
            }
        }

        #endregion

        #region Utility Methods
        private static bool IsConstructable(ConstructorInfo ctor)
        {
            return ctor.IsDefined(typeof(ConstructableAttribute), false);
        }

        private static void RemoveFromSectorTable(Sector s, XmlSpawner spawner)
        {
            if (s == null || s.Owner == null || s.Owner == Map.Internal || GlobalSectorTable[s.Owner.MapID] == null)
            {
                return;
            }

            // find the sector
            List<XmlSpawner> spawnerlist;
            if (GlobalSectorTable[s.Owner.MapID].TryGetValue(s, out spawnerlist) && spawnerlist != null)
            {
                spawnerlist.Remove(spawner);
            }
        }

        private void ResetSectorList()
        {
            // remove the global sector entries
            if (sectorList != null)
            {
                for (var index = 0; index < sectorList.Count; index++)
                {
                    Sector s = sectorList[index];
                    RemoveFromSectorTable(s, this);
                }
            }
            sectorList = null;
            UseSectorActivate = false;

            // force an update of the sector list
            bool sectorrefresh = HasActiveSectors;
        }

        public void ReportStatus()
        {
            if (PropertyInfoList != null)
            {
                Console.WriteLine("PropertyInfoList: {0}", PropertyInfoList.Count);

                for (var index = 0; index < PropertyInfoList.Count; index++)
                {
                    BaseXmlSpawner.TypeInfo to = PropertyInfoList[index];
                    Console.WriteLine("\t{0}", to.t);

                    for (var i = 0; i < to.plist.Count; i++)
                    {
                        PropertyInfo p = to.plist[i];
                        Console.WriteLine("\t\t{0}", p);
                    }
                }
            }
        }

        #endregion

        public static void _TraceStart(int index) { }
        public static void _TraceEnd(int index) { }

        #region Trigger Methods

        private bool ValidPlayerTrig(Mobile m)
        {
            if (m == null || m.Deleted) return false;
            return (m.Player || m_AllowNPCTriggering) && (m.AccessLevel <= TriggerAccessLevel) && ((!m.Body.IsGhost && !m_AllowGhostTriggering) || m.Body.IsGhost && m_AllowGhostTriggering);
        }

        private bool AllowTriggering => m_Running && !m_refractActivated && TODInRange && CanSpawn;

        private void ActivateTrigger()
        {
            DoTimer(); // reset the timer

            // start the refractory timer to set proximity activated to false, thus enabling another activation
            if (m_MaxRefractory > TimeSpan.FromMinutes(0))
            {
                int minSeconds = (int)m_MinRefractory.TotalSeconds;
                int maxSeconds = (int)m_MaxRefractory.TotalSeconds;

                DoTimer3(TimeSpan.FromSeconds(Utility.RandomMinMax(minSeconds, maxSeconds)));
            }

            // if the spawnontrigger flag is set, then spawn immediately
            if (m_SpawnOnTrigger)
            {
                NextSpawn = TimeSpan.Zero;
                ResetNextSpawnTimes();
            }

            // reset speech triggering if it was set
            m_speechTriggerActivated = false;
        }

        public void CheckTriggers(Mobile m, Skill s, bool hasproximity)
        {
            if (AllowTriggering && !m_proximityActivated) // only proximity trigger when no spawns have already been triggered
            {
                bool needs_speech_trigger = false;
                bool needs_player_trigger = false;
                bool has_player_trigger = false;

                m_skipped = false;

                // test for the various triggering options in the order of increasing computational demand.  No point checking a high demand test
                // if a low demand one has already failed.

                // check for external triggering
                if (m_ExternalTriggering && !m_ExternalTrigger)
                    return;

                // if speech triggering is set then test for successful activation
                if (!string.IsNullOrEmpty(m_SpeechTrigger))
                {
                    needs_speech_trigger = true;
                }
                // check to see if we have to continue
                if (needs_speech_trigger && !m_speechTriggerActivated)
                    return;

                // if player property triggering is set then look for the mob and test properties
                if (!string.IsNullOrEmpty(m_PlayerPropertyName))
                {
                    needs_player_trigger = true;
                    string status_str;

                    if (BaseXmlSpawner.TestMobProperty(this, m, m_PlayerPropertyName, out status_str))
                    {
                        has_player_trigger = true;
                    }

                    if (!string.IsNullOrEmpty(status_str))
                    {
                        this.status_str = status_str;
                    }
                }

                // check to see if we have to continue
                if (needs_player_trigger && !has_player_trigger)
                    return;

                // if this was called without being proximity triggered then check to see that the non-movement triggers were enabled.
                if (!hasproximity && !m_ExternalTriggering)
                    return;

                // all of the necessary trigger conditions have been met so go ahead and trigger
                // after you make the probability check

                if (Utility.RandomDouble() < m_TriggerProbability)
                {
                    // play a sound indicating the spawner has been triggered
                    if (m_ProximityTriggerSound > 0 && m != null && !m.Deleted)
                        m.PlaySound(m_ProximityTriggerSound);

                    // display the trigger message
                    if (!string.IsNullOrEmpty(m_ProximityTriggerMessage) && m != null && !m.Deleted)
                        m.PublicOverheadMessage(MessageType.Regular, 0x3B2, false, m_ProximityTriggerMessage);

                    // enable spawning at the next ontick
                    // this will also start the refractory timer and send the triggering indicators
                    ProximityActivated = true;

                    // keep track of who triggered this
                    m_mob_who_triggered = m;
                }
                else
                {
                    m_skipped = true;

                    // reset speech triggering if it was set
                    m_speechTriggerActivated = false;
                }
            }
        }

        public override bool HandlesOnSpeech => m_Running && !string.IsNullOrEmpty(m_SpeechTrigger);

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (m_Running && m_ProximityRange >= 0 && ValidPlayerTrig(e.Mobile) && CanSpawn && !m_refractActivated && TODInRange)
            {
                m_speechTriggerActivated = false;

                if (!Utility.InRange(e.Mobile.Location, Location, m_ProximityRange))
                    return;

                if (m_SpeechTrigger != null && e.Speech.ToLower().IndexOf(m_SpeechTrigger.ToLower()) >= 0)
                {
                    e.Handled = true;

                    // found the speech trigger so flag it for testing in the onmovement handler where the other proximity features are tested
                    m_speechTriggerActivated = true;

                    CheckTriggers(e.Mobile, null, true);
                }
            }
        }

        public override bool HandlesOnMovement => m_Running && m_ProximityRange >= 0;

        public void AddToMovementList(Mobile m)
        {
            // go through the list and check for redundancy
            if (m_MovementList == null)
            {
                m_MovementList = new List<MovementInfo>();
            }

            // check to see if the movement timer is running
            if (m_MovementTimer == null || !m_MovementTimer.Running)
            {
                DoMovementTimer(TimeSpan.FromSeconds(1));
            }

            bool add = true;

            for (var index = 0; index < m_MovementList.Count; index++)
            {
                MovementInfo moveinfo = m_MovementList[index];
                Mobile mtrig = moveinfo.trigMob;

                if (mtrig == m)
                {
                    add = false;
                    break;
                }
            }

            // wasnt on the list so add it
            if (add)
            {

                // is the list at max throttling length?
                if (m_MovementList.Count > MaxMoveCheck)
                {
                    // replace a random entry in the current list with this one
                    m_MovementList[Utility.Random(m_MovementList.Count)] = new MovementInfo(m);
                }
                else
                {

                    m_MovementList.Add(new MovementInfo(m));
                }
            }
        }

        public void DoMovementTimer(TimeSpan delay)
        {
            if (m_MovementTimer != null)
                m_MovementTimer.Stop();

            m_MovementTimer = new MovementTimer(this, delay);

            m_MovementTimer.Start();
        }

        private class MovementTimer : Timer
        {
            private readonly XmlSpawner m_Spawner;

            public MovementTimer(XmlSpawner spawner, TimeSpan delay)
                : base(delay)
            {
                m_Spawner = spawner;
            }

            protected override void OnTick()
            {
                // check everyone on the movement list then clear the list
                if (m_Spawner != null && !m_Spawner.Deleted)
                {
                    if (m_Spawner.m_Running && !m_Spawner.m_proximityActivated && !m_Spawner.m_refractActivated && m_Spawner.TODInRange && m_Spawner.CanSpawn)
                    {
                        int count = 0;
                        int maxspeed = 0;

                        for (var index = 0; index < m_Spawner.m_MovementList.Count; index++)
                        {
                            MovementInfo moveinfo = m_Spawner.m_MovementList[index];
                            Mobile m = moveinfo.trigMob;

                            if (m == null)
                            {
                                continue;
                            }

                            // additional throttling in here by limiting number of mobs that can be checked in a single ontick
                            count++;
                            if (count > MaxMoveCheck)
                            {
                                break;
                            }

                            int speed = (int) GetDistance(m.Location, moveinfo.trigLocation);

                            if (speed > maxspeed)
                            {
                                maxspeed = speed;
                            }

                            m_Spawner.CheckTriggers(m, null, true);
                        }

                        m_Spawner.MovingPlayerCount = m_Spawner.m_MovementList.Count;
                        m_Spawner.FastestPlayerSpeed = maxspeed;

                    }
                    m_Spawner.m_MovementList.Clear();
                }
            }
        }

        public static double GetDistance(Point3D p1, Point3D p2)
        {
            int xDelta = p1.X - p2.X;
            int yDelta = p1.Y - p2.Y;

            return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m_Running && m_ProximityRange >= 0 && ValidPlayerTrig(m) && CanSpawn)
            {
                // check to see if player is within range of the spawner
                if ((Parent == null) && Utility.InRange(m.Location, Location, m_ProximityRange))
                {
                    // add some throttling code here.
                    // add the player to a list that gets cleared every few seconds, checking for redundancy then trigger off of the list instead of off of
                    // the actual movement stream

                    AddToMovementList(m);
                }
                else
                {
                    // clear any speech triggering
                    m_speechTriggerActivated = false;
                }
            }
            base.OnMovement(m, oldLocation);
        }
        #endregion

        #region Initialization
        public delegate bool AssignSettingsHandler(string argname, string value);

        public static void Initialize()
        {
            // initialize the default waypoint name
            WayPoint tmpwaypoint = new WayPoint();
            defwaypointname = tmpwaypoint.Name;
            tmpwaypoint.Delete();

            int count = 0;
            int regional = 0;

            foreach (Item item in World.Items.Values)
            {
                if (item is XmlSpawner xmlSpawner)
                {
                    count++;
                    XmlSpawner spawner = xmlSpawner;

                    if (!string.IsNullOrEmpty(spawner.RegionName))
                    {
                        spawner.RegionName = spawner.RegionName;    // invoke set(RegionName)
                        regional++;
                    }

                    // check for smart spawning and restart timers after deser if needed
                    // note, HasActiveSectors will recalculate the sector list and UseSectorActivate property
                    bool recalc_sectors = spawner.HasActiveSectors;

                    spawner.RestoreISpawner();
                }
            }

            // start the global smartspawning timer
            if (SmartSpawningSystemEnabled)
            {
                DoGlobalSectorTimer(TimeSpan.FromSeconds(1));
            }

            // standard commands
            CommandSystem.Register("XmlSpawnerShowAll", AccessLevel.Administrator, ShowSpawnPoints_OnCommand);
            CommandSystem.Register("XmlSpawnerHideAll", AccessLevel.Administrator, HideSpawnPoints_OnCommand);
            CommandSystem.Register("XmlSpawnerWipe", AccessLevel.Administrator, Wipe_OnCommand);
            CommandSystem.Register("XmlSpawnerWipeAll", AccessLevel.Administrator, WipeAll_OnCommand);
            CommandSystem.Register("XmlSpawnerLoad", DiskAccessLevel, Load_OnCommand);
            CommandSystem.Register("XmlSpawnerSave", DiskAccessLevel, Save_OnCommand);
            CommandSystem.Register("XmlSpawnerSaveAll", DiskAccessLevel, SaveAll_OnCommand);
            CommandSystem.Register("XmlSpawnerRespawn", AccessLevel.Seer, Respawn_OnCommand);
            CommandSystem.Register("XmlSpawnerRespawnAll", AccessLevel.Seer, RespawnAll_OnCommand);

            CommandSystem.Register("XmlShow", AccessLevel.Administrator, ShowSpawnPoints_OnCommand);
            CommandSystem.Register("XmlHide", AccessLevel.Administrator, HideSpawnPoints_OnCommand);
            CommandSystem.Register("XmlHome", AccessLevel.GameMaster, XmlHome_OnCommand);
            CommandSystem.Register("XmlUnLoad", DiskAccessLevel, UnLoad_OnCommand);
            CommandSystem.Register("XmlSpawnerUnLoad", DiskAccessLevel, UnLoad_OnCommand);
            CommandSystem.Register("XmlLoad", DiskAccessLevel, Load_OnCommand);
            CommandSystem.Register("XmlSave", DiskAccessLevel, Save_OnCommand);
            CommandSystem.Register("XmlSaveAll", DiskAccessLevel, SaveAll_OnCommand);
            CommandSystem.Register("XmlDefaults", AccessLevel.Administrator, XmlDefaults_OnCommand);
            CommandSystem.Register("XmlGet", AccessLevel.GameMaster, XmlGetValue_OnCommand);
            
            TargetCommands.Register(new XmlSetCommand());
            TargetCommands.Register(new XmlSaveSingle());
        }
        #endregion

        #region Commands
        [Usage("XmlGet property")]
        [Description("Returns value of the property on the targeted object.")]
        public static void XmlGetValue_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new GetValueTarget(e);
        }
        private class GetValueTarget : Target
        {
            private readonly CommandEventArgs m_e;
            public GetValueTarget(CommandEventArgs e)
                : base(30, false, TargetFlags.None)
            {
                m_e = e;
            }
            protected override void OnTarget(Mobile from, object targeted)
            {
                string pname = m_e.GetString(0);
                Type ptype;
                string result = BaseXmlSpawner.GetPropertyValue(null, targeted, pname, out ptype);

                // see if it was successful
                if (ptype == null)
                {
                    return;
                }
                from.SendMessage("{0}", result);

            }
        }

        public class XmlSetCommand : BaseCommand
        {
            public XmlSetCommand()
            {
                AccessLevel = AccessLevel.Administrator;
                Supports = CommandSupport.All;
                Commands = new[] { "XmlSet" };
                ObjectTypes = ObjectTypes.Both;
                Usage = "XmlSet <propertyName> <value>";
                Description = "Sets a property value by name of a targeted object. Provides access to all public properties.";
            }

            public override void Execute(CommandEventArgs e, object obj)
            {
                if (e.Length >= 2)
                {
                    string result = BaseXmlSpawner.SetPropertyValue(null, obj, e.GetString(0), e.GetString(1));

                    if (result == "Property has been set.")
                        AddResponse(result);
                    else
                        LogFailure(result);
                }
                else
                {
                    LogFailure("Format: XmlSet <propertyName> <value>");
                }
            }
        }

        private class XmlHomeTarget : Target // added in targeting for the [xmlhome command
        {
            private readonly CommandEventArgs m_e;
            public XmlHomeTarget(CommandEventArgs e)
                : base(30, false, TargetFlags.None)
            {
                m_e = e;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                XmlSpawner spawner = null;

                if (targeted is XmlSpawner xSpawner)
                {
                    if (m_e.GetString(0) == "status")
                    {
                        xSpawner.ReportStatus();
                        return;
                    }
                }

                if (targeted is Mobile mob)
                {
                    spawner = mob.Spawner as XmlSpawner;
                }
                else if (targeted is Item item)
                {
                    spawner = item.Spawner as XmlSpawner;
                }

                if (spawner == null)
                {
                    from.SendMessage("Unable to find spawner for this object");
                    return;
                }

                // check to make sure it is still on the spawner
                for (var index = 0; index < spawner.m_SpawnObjects.Count; index++)
                {
                    SpawnObject so = spawner.m_SpawnObjects[index];

                    for (int x = 0; x < so.SpawnedObjects.Count; x++)
                    {
                        object o = so.SpawnedObjects[x];

                        if (o == targeted)
                        {
                            from.SendMessage("{0}, {1}, {2}", spawner.X, spawner.Y, spawner.Z);

                            if (m_e.GetString(0) == "go")
                            {
                                // make sure the spawner is not in a container.
                                if (spawner.Parent == null)
                                {
                                    from.Location = new Point3D(spawner.Location);
                                    from.Map = spawner.Map;
                                }
                                else
                                {
                                    from.SendMessage("Spawner is in a container");
                                }
                            }
                            else if (m_e.GetString(0) == "send")
                            {
                                // make sure the spawner is not in a container.
                                if (spawner.Parent == null)
                                {
                                    if (o is Item item)
                                    {
                                        item.Location = new Point3D(spawner.Location);
                                        item.Map = spawner.Map;
                                    }

                                    if (o is Mobile mobile)
                                    {
                                        mobile.Location = new Point3D(spawner.Location);
                                        mobile.Map = spawner.Map;
                                    }
                                }
                                else
                                {
                                    from.SendMessage("Spawner is in a container");
                                }
                            }
                            else if (m_e.GetString(0) == "gump")
                            {
                                spawner.OnDoubleClick(from);
                            }

                            return;
                        }
                    }
                }
            }
        }

        [Usage("XmlHome [go][gump][send]")]
        [Description("Returns the coordinates of the spawner for the targeted object. Args: 'go' teleports to spawner, 'gump' opens spawner gump, 'send' sends mob home")]
        public static void XmlHome_OnCommand(CommandEventArgs e)
        {
            e.Mobile.Target = new XmlHomeTarget(e);
        }

        private static void XmlSaveDefaults(string filePath, Mobile m)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            using (StreamWriter op = new StreamWriter(filePath))
            {
                XmlTextWriter xml = new XmlTextWriter(op)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

                xml.WriteStartDocument(true);

                xml.WriteStartElement("XmlDefaults");

                xml.WriteStartElement("defProximityRange");
                xml.WriteString(defProximityRange.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defTriggerProbability");
                xml.WriteString(defTriggerProbability.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defProximityTriggerSound");
                xml.WriteString(defProximityTriggerSound.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defMinRefractory");
                xml.WriteString(defMinRefractory.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defMaxRefractory");
                xml.WriteString(defMaxRefractory.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defTODStart");
                xml.WriteString(defTODStart.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defTODEnd");
                xml.WriteString(defTODEnd.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defStackAmount");
                xml.WriteString(defAmount.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defDuration");
                xml.WriteString(defDuration.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defIsGroup");
                xml.WriteString(defIsGroup.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defTeam");
                xml.WriteString(defTeam.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defRelativeHome");
                xml.WriteString(defRelativeHome.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defSpawnRange");
                xml.WriteString(defSpawnRange.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defHomeRange");
                xml.WriteString(defHomeRange.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defMinDelay");
                xml.WriteString(defMinDelay.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defMaxDelay");
                xml.WriteString(defMaxDelay.ToString());
                xml.WriteEndElement();
                xml.WriteStartElement("defTODMode");
                xml.WriteString(defTODMode.ToString());
                xml.WriteEndElement();

                xml.WriteEndElement();

                xml.Close();
            }
            m.SendMessage("defaults saved to {0}", filePath);
        }

        public static void XmlLoadDefaults(string filePath, Mobile m)
        {
            if (m == null || m.Deleted) return;
            if (filePath != null && filePath.Length >= 1)
            {

                if (File.Exists(filePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);

                    XmlElement root = doc["XmlDefaults"];
                    LoadDefaults(root);
                    m.SendMessage("defaults loaded successfully from {0}", filePath);
                }
                else
                {
                    m.SendMessage("File {0} does not exist.", filePath);
                }
            }
        }

        private static void LoadDefaults(XmlElement node)
        {
            try { defProximityRange = int.Parse(node["defProximityRange"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defTriggerProbability = double.Parse(node["defTriggerProbability"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defProximityTriggerSound = int.Parse(node["defProximityTriggerSound"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defMinRefractory = TimeSpan.Parse(node["defMinRefractory"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defMaxRefractory = TimeSpan.Parse(node["defMaxRefractory"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defTODStart = TimeSpan.Parse(node["defTODStart"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defTODEnd = TimeSpan.Parse(node["defTODEnd"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defAmount = int.Parse(node["defStackAmount"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defDuration = TimeSpan.Parse(node["defDuration"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defIsGroup = bool.Parse(node["defIsGroup"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defTeam = int.Parse(node["defTeam"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defRelativeHome = bool.Parse(node["defRelativeHome"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defSpawnRange = int.Parse(node["defSpawnRange"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defHomeRange = int.Parse(node["defHomeRange"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defMinDelay = TimeSpan.Parse(node["defMinDelay"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            try { defMaxDelay = TimeSpan.Parse(node["defMaxDelay"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            int todmode = 0;
            try { todmode = int.Parse(node["defTODMode"].InnerText); }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            switch (todmode)
            {
                case (int)TODModeType.Realtime:
                    defTODMode = TODModeType.Realtime;
                    break;
                case (int)TODModeType.Gametime:
                    defTODMode = TODModeType.Gametime;
                    break;
            }
        }

        [Usage("XmlDefaults [defaultpropertyname value]")]
        [Description("Returns or changes the default settings of the spawner.")]
        public static void XmlDefaults_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            if (m == null || m.Deleted) return;
            if (e.Arguments.Length >= 1)
            {
                // leave open the possibility of just requesting display of a single property
                if (e.Arguments.Length == 2)
                {
                    if (e.Arguments[0].ToLower() == "save")
                    {
                        XmlSaveDefaults(e.Arguments[1], m);
                    }
                    else if (e.Arguments[0].ToLower() == "load")
                    {
                        XmlLoadDefaults(e.Arguments[1], m);
                    }
                    else if (e.Arguments[0].ToLower() == "maxdelay")
                    {
                        try
                        {
                            defMaxDelay = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("MaxDelay = {0}", defMaxDelay);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "mindelay")
                    {
                        try
                        {
                            defMinDelay = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("MinDelay = {0}", defMinDelay);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "spawnrange")
                    {
                        try
                        {
                            defSpawnRange = Convert.ToInt32(e.Arguments[1]);
                            m.SendMessage("SpawnRange = {0}", defSpawnRange);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "homerange")
                    {
                        try
                        {
                            defHomeRange = Convert.ToInt32(e.Arguments[1]);
                            m.SendMessage("HomeRange = {0}", defHomeRange);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "relativehome")
                    {
                        try
                        {
                            defRelativeHome = Convert.ToBoolean(e.Arguments[1]);
                            m.SendMessage("RelativeHome = {0}", defRelativeHome);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "proximitytriggersound")
                    {
                        try
                        {
                            defProximityTriggerSound = Convert.ToInt32(e.Arguments[1]);
                            m.SendMessage("ProximityTriggerSound = {0}", defProximityTriggerSound);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "proximityrange")
                    {
                        try
                        {
                            defProximityRange = Convert.ToInt32(e.Arguments[1]);
                            m.SendMessage("ProximityRange = {0}", defProximityRange);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "triggerprobability")
                    {
                        try
                        {
                            defTriggerProbability = Convert.ToDouble(e.Arguments[1]);
                            m.SendMessage("TriggerProbability = {0}", defTriggerProbability);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "todstart")
                    {
                        try
                        {
                            defTODStart = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("TODStart = {0}", defTODStart);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "todend")
                    {
                        try
                        {
                            defTODEnd = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("TODEnd = {0}", defTODEnd);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "stackamount")
                    {
                        try
                        {
                            defAmount = Convert.ToInt32(e.Arguments[1]);
                            m.SendMessage("StackAmount = {0}", defAmount);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "duration")
                    {
                        try
                        {
                            defDuration = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("Duration = {0}", defDuration);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "group")
                    {
                        try
                        {
                            defIsGroup = Convert.ToBoolean(e.Arguments[1]);
                            m.SendMessage("Group = {0}", defIsGroup);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "team")
                    {
                        try
                        {
                            defTeam = Convert.ToInt32(e.Arguments[1]);
                            m.SendMessage("Team = {0}", defTeam);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "todmode")
                    {
                        try
                        {
                            int todmode = Convert.ToInt32(e.Arguments[1]);
                            switch (todmode)
                            {
                                case (int)TODModeType.Gametime:
                                    defTODMode = TODModeType.Gametime;
                                    break;
                                case (int)TODModeType.Realtime:
                                    defTODMode = TODModeType.Realtime;
                                    break;
                            }
                            m.SendMessage("TODMode = {0}", defTODMode);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "maxrefractory")
                    {
                        try
                        {
                            defMaxRefractory = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("MaxRefractory = {0}", defMaxRefractory);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else if (e.Arguments[0].ToLower() == "minrefractory")
                    {
                        try
                        {
                            defMinRefractory = TimeSpan.FromMinutes(Convert.ToDouble(e.Arguments[1]));
                            m.SendMessage("MinRefractory = {0}", defMinRefractory);
                        }
                        catch { m.SendMessage("invalid value : {0}", e.Arguments[1]); }
                    }
                    else
                    {
                        m.SendMessage("{0} : no such default value.", e.Arguments[0]);
                    }
                }

            }
            else
            {
                // just display the values
                m.SendMessage("TriggerProbability = {0}", defTriggerProbability);
                m.SendMessage("ProximityRange = {0}", defProximityRange);
                m.SendMessage("ProximityTriggerSound = {0}", defProximityTriggerSound);
                m.SendMessage("MinRefractory = {0}", defMinRefractory);
                m.SendMessage("MaxRefractory = {0}", defMaxRefractory);
                m.SendMessage("TODStart = {0}", defTODStart);
                m.SendMessage("TODEnd = {0}", defTODEnd);
                m.SendMessage("TODMode = {0}", defTODMode);
                m.SendMessage("StackAmount = {0}", defAmount);
                m.SendMessage("Duration = {0}", defDuration);
                m.SendMessage("Group = {0}", defIsGroup);
                m.SendMessage("Team = {0}", defTeam);
                m.SendMessage("RelativeHome = {0}", defRelativeHome);
                m.SendMessage("SpawnRange = {0}", defSpawnRange);
                m.SendMessage("HomeRange = {0}", defHomeRange);
                m.SendMessage("MinDelay = {0}", defMinDelay);
                m.SendMessage("MaxDelay = {0}", defMaxDelay);
            }
        }

        [Usage("XmlSpawnerShowAll")]
        [Aliases("XmlShow")]
        [Description("Makes all XmlSpawner objects movable and also changes the item id to a blue ships mast for easy identification.")]
        public static void ShowSpawnPoints_OnCommand(CommandEventArgs e)
        {
            List<Item> ToShow = new List<Item>();
            foreach (Item item in World.Items.Values)
            {
                if (item is XmlSpawner)
                {
                    //turned off visibility. Admins will still see masts but players will not.
                    item.Visible = false;    // set the spawn item visibility
                    item.Movable = false;    // Make the spawn item movable
                    item.Hue = 88;          // Bright blue colour so its easy to spot
                    item.ItemID = ShowItemId;   // Ship Mast (Very tall, easy to see if beneath other objects)

                    // find container-held spawners to be marked with an external static
                    if (item.Parent != null && item.RootParent is Container)
                    {
                        ToShow.Add(item);
                    }
                }
            }

            // place the statics
            for (var index = 0; index < ToShow.Count; index++)
            {
                var xml_item = (XmlSpawner) ToShow[index];
                // does the spawner already have a static attached to it? could happen if two showall commands are issued in a row.
                // if so then dont add another
                if ((xml_item.m_ShowContainerStatic == null || xml_item.m_ShowContainerStatic.Deleted) && xml_item.RootParent is Container rootItem)
                {
                    // calculate a world location for the static.  Position it just above the container
                    int x = rootItem.Location.X;
                    int y = rootItem.Location.Y;
                    int z = rootItem.Location.Z + 10;

                    Static s = new Static(ShowItemId)
                    {
                        Visible = false
                    };
                    s.MoveToWorld(new Point3D(x, y, z), rootItem.Map);

                    xml_item.m_ShowContainerStatic = s;
                }
            }
        }

        [Usage("XmlSpawnerHideAll")]
        [Aliases("XmlHide")]
        [Description("Makes all XmlSpawner objects invisible and unmovable returns the object id to the default.")]
        public static void HideSpawnPoints_OnCommand(CommandEventArgs e)
        {
            List<Item> ToDelete = new List<Item>();
            foreach (Item item in World.Items.Values)
            {
                if (item is XmlSpawner spawner)
                {
                    spawner.Visible = false;
                    spawner.Movable = false;
                    spawner.Hue = 0;
                    spawner.ItemID = BaseItemId;

                    // get rid of the external static marker for container-held spawners
                    // check anything that might have been tagged with a container static
                    if (spawner.m_ShowContainerStatic != null && !spawner.m_ShowContainerStatic.Deleted)
                    {
                        ToDelete.Add(spawner);
                    }
                }
            }

            for (var index = 0; index < ToDelete.Count; index++)
            {
                var xml_item = (XmlSpawner) ToDelete[index];

                if (xml_item.m_ShowContainerStatic != null && !xml_item.m_ShowContainerStatic.Deleted)
                {
                    xml_item.m_ShowContainerStatic.Delete();
                }
            }
        }

        [Usage("XmlSpawnerWipe [SpawnerPrefixFilter]")]
        [Description("Removes all XmlSpawner objects from the current map.")]
        public static void Wipe_OnCommand(CommandEventArgs e)
        {
            WipeSpawners(e, false);
        }

        [Usage("XmlSpawnerWipeAll [SpawnerPrefixFilter]")]
        [Description("Removes all XmlSpawner objects from the entire world.")]
        public static void WipeAll_OnCommand(CommandEventArgs e)
        {
            WipeSpawners(e, true);
        }

        public static void XmlUnLoadFromFile(string filename, string SpawnerPrefix, Mobile from, out int processedmaps, out int processedspawners)
        {

            processedmaps = 0;
            processedspawners = 0;
            if (filename == null || filename.Length <= 0) return;

            int total_processed_maps = 0;
            int total_processed_spawners = 0;

            // Check if the file exists
            if (File.Exists(filename))
            {
                FileStream fs = null;
                try
                {
                    fs = File.Open(filename, FileMode.Open, FileAccess.Read);
                }
                catch { }

                if (fs == null)
                {
                    if (from != null)
                        from.SendMessage("Unable to open {0} for unloading", filename);
                    return;
                }

                XmlUnLoadFromStream(fs, filename, SpawnerPrefix, from, out processedmaps, out processedspawners);

            }
            else
                // check to see if it is a directory
                if (Directory.Exists(filename))
            {
                // if so then import all of the .xml files in the directory
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(filename, "*.xml");
                }
                catch { }
                if (files != null && files.Length > 0)
                {
                    if (from != null)
                    {
                        from.SendMessage("UnLoading {0} .xml files from directory {1}", files.Length, filename);
                    }

                    for (var index = 0; index < files.Length; index++)
                    {
                        string file = files[index];

                        XmlUnLoadFromFile(file, SpawnerPrefix, from, out processedmaps, out processedspawners);
                        total_processed_maps += processedmaps;
                        total_processed_spawners += processedspawners;
                    }
                }
                // recursively search subdirectories for more .xml files
                string[] dirs = null;
                try
                {
                    dirs = Directory.GetDirectories(filename);
                }
                catch { }
                if (dirs != null && dirs.Length > 0)
                {
                    for (var index = 0; index < dirs.Length; index++)
                    {
                        string dir = dirs[index];

                        XmlUnLoadFromFile(dir, SpawnerPrefix, from, out processedmaps, out processedspawners);
                        total_processed_maps += processedmaps;
                        total_processed_spawners += processedspawners;
                    }
                }
                if (from != null)
                {
                    from.SendMessage("UnLoaded a total of {0} .xml files and {2} spawners from directory {1}", total_processed_maps, filename, total_processed_spawners);
                }

                processedmaps = total_processed_maps;
                processedspawners = total_processed_spawners;
            }
            else
            {
                if (from != null)
                {
                    from.SendMessage("{0} does not exist", filename);
                }
            }
        }

        public static void XmlUnLoadFromStream(Stream fs, string filename, string SpawnerPrefix, Mobile from, out int processedmaps, out int processedspawners)
        {
            processedmaps = 0;
            processedspawners = 0;

            if (fs == null)
                return;

            int TotalCount = 0;
            int TrammelCount = 0;
            int FeluccaCount = 0;
            int IlshenarCount = 0;
            int MalasCount = 0;
            int TokunoCount = 0;
            int OtherCount = 0;
            int bad_spawner_count = 0;
            int spawners_deleted = 0;

            if (from != null)
                from.SendMessage($"UnLoading {"XmlSpawner"} objects{(!string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty)} from file {filename}.");

            // Create the data set
            DataSet ds = new DataSet(SpawnDataSetName);

            // Read in the file
            //ds.ReadXml( e.Arguments[0].ToString() );
            bool fileerror = false;
            try
            {
                ds.ReadXml(fs);
            }
            catch
            {
                if (from != null)
                    from.SendMessage(33, "Error reading xml file {0}", filename);
                fileerror = true;
            }
            // close the file
            fs.Close();
            if (fileerror) return;

            // Check that at least a single table was loaded
            if (ds.Tables.Count > 0)
            {
                // Add each spawn point to the current map
                if (ds.Tables[SpawnTablePointName] != null && ds.Tables[SpawnTablePointName].Rows.Count > 0)
                {
                    for (var index = 0; index < ds.Tables[SpawnTablePointName].Rows.Count; index++)
                    {
                        DataRow dr = ds.Tables[SpawnTablePointName].Rows[index];
                        // load in the spawner info.  Certain fields are required and therefore cannot be ignored
                        // the exception handler for those will flag bad_spawner and the result will be logged

                        // Each row makes up a single spawner
                        string SpawnName = "Spawner";
                        try
                        {
                            SpawnName = (string) dr["Name"];
                        }
                        catch
                        {
                        }

                        // Check if there is any spawner name criteria specified on the unload
                        if (string.IsNullOrEmpty(SpawnerPrefix) || SpawnName.StartsWith(SpawnerPrefix))
                        {
                            bool bad_spawner = false;
                            // Try load the GUID (might not work so create a new GUID)
                            Guid SpawnId = Guid.NewGuid();
                            try
                            {
                                SpawnId = new Guid((string) dr["UniqueId"]);
                            }
                            catch
                            {
                                bad_spawner = true;
                            }

                            // have to have a GUID or no point in continuing
                            if (bad_spawner)
                            {
                                bad_spawner_count++;
                                continue;
                            }

                            // Get the map (default to the mobiles map)
                            Map SpawnMap = Map.Internal;
                            string XmlMapName = SpawnMap.Name;

                            // Try to get the "map" field, but in case it doesn't exist, catch and discard the exception
                            try
                            {
                                XmlMapName = (string) dr["Map"];
                            }
                            catch
                            {
                            }

                            // Convert the xml map value to a real map object
                            if (string.Compare(XmlMapName, Map.Trammel.Name, true) == 0 || XmlMapName == "Trammel")
                            {
                                SpawnMap = Map.Trammel;
                                TrammelCount++;
                            }
                            else if (string.Compare(XmlMapName, Map.Felucca.Name, true) == 0 || XmlMapName == "Felucca")
                            {
                                SpawnMap = Map.Felucca;
                                FeluccaCount++;
                            }
                            else if (string.Compare(XmlMapName, Map.Ilshenar.Name, true) == 0 ||
                                     XmlMapName == "Ilshenar")
                            {
                                SpawnMap = Map.Ilshenar;
                                IlshenarCount++;
                            }
                            else if (string.Compare(XmlMapName, Map.Malas.Name, true) == 0 || XmlMapName == "Malas")
                            {
                                SpawnMap = Map.Malas;
                                MalasCount++;
                            }
                            else if (string.Compare(XmlMapName, Map.Tokuno.Name, true) == 0 || XmlMapName == "Tokuno")
                            {
                                SpawnMap = Map.Tokuno;
                                TokunoCount++;
                            }
                            else
                            {
                                try
                                {
                                    SpawnMap = Map.Parse(XmlMapName);
                                }
                                catch
                                {
                                }

                                OtherCount++;
                            }

                            XmlSpawner OldSpawner = null; // Check if this spawner already exists

                            foreach (Item i in World.Items.Values)
                            {
                                // Check if the spawners GUID is the same as the one being unloaded
                                // and that the spawners map is the same as the one being unloaded
                                if (i is XmlSpawner checkSpawner && checkSpawner.UniqueId == SpawnId.ToString())
                                {
                                    OldSpawner = checkSpawner;

                                    if (OldSpawner != null)
                                    {
                                        spawners_deleted++;
                                        OldSpawner.Delete();
                                    }

                                    break;
                                }
                            }
                        }

                        TotalCount++;
                    }
                }
            }

            try
            {
                fs.Close();
            }
            catch { }

            if (from != null)
                from.SendMessage("{0}/{8} spawner(s) were unloaded using file {1} [Trammel={2}, Felucca={3}, Ilshenar={4}, Malas={5}, Tokuno={6}, Other={7}].",
                    spawners_deleted, filename, TrammelCount, FeluccaCount, IlshenarCount, MalasCount, TokunoCount, OtherCount, TotalCount);
            if (bad_spawner_count > 0)
            {
                if (from != null)
                    from.SendMessage(33, "{0} bad spawners detected.", bad_spawner_count);
            }

            processedmaps = 1;
            processedspawners = TotalCount;
        }

        [Usage("XmlSpawnerUnLoad <SpawnFile or directory> [SpawnerPrefixFilter]")]
        [Aliases("XmlUnload")]
        [Description("UnLoads XmlSpawner objects from the proper map as defined in the file supplied.")]
        public static void UnLoad_OnCommand(CommandEventArgs e)
        {
            if (e.Mobile.AccessLevel >= DiskAccessLevel)
            {
                if (e.Arguments.Length >= 1)
                {
                    // Spawner unload criteria (if any)
                    string SpawnerPrefix = string.Empty;

                    // Check if there is an argument provided (load criteria)
                    if (e.Arguments.Length > 1)
                        SpawnerPrefix = e.Arguments[1];

                    string filename = LocateFile(e.Arguments[0]);
                    int processedmaps;
                    int processedspawners;
                    XmlUnLoadFromFile(filename, SpawnerPrefix, e.Mobile, out processedmaps, out processedspawners);
                }
                else
                    e.Mobile.SendMessage("Usage:  {0} <SpawnFile or directory>", e.Command);
            }
            else
                e.Mobile.SendMessage("You do not have rights to perform this command.");
        }

        public static void XmlLoadFromFile(string filename, string SpawnerPrefix, Mobile from, Point3D fromloc, Map frommap, bool loadrelative, int maxrange, bool loadnew, out int processedmaps, out int processedspawners)
        {
            processedmaps = 0;
            processedspawners = 0;
            int total_processed_maps = 0;
            int total_processed_spawners = 0;

            if (filename == null || filename.Length <= 0)
                return;

            // Check if the file exists
            if (File.Exists(filename))
            {
                FileStream fs = null;
                try
                {
                    fs = File.Open(filename, FileMode.Open, FileAccess.Read);
                }
                catch { }

                if (fs == null)
                {
                    if (from != null)
                        from.SendMessage("Unable to open {0} for loading", filename);
                    return;
                }

                // load the file
                XmlLoadFromStream(fs, filename, SpawnerPrefix, from, fromloc, frommap, loadrelative, maxrange, loadnew, out processedmaps, out processedspawners);
            }
            else if (Directory.Exists(filename))
            {
                // if so then load all of the .xml files in the directory
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(filename, "*.xml");
                }
                catch { }
                if (files != null && files.Length > 0)
                {
                    if (from != null)
                    {
                        from.SendMessage("Loading {0} .xml files from directory {1}", files.Length, filename);
                    }

                    for (var index = 0; index < files.Length; index++)
                    {
                        string file = files[index];

                        XmlLoadFromFile(file, SpawnerPrefix, from, fromloc, frommap, loadrelative, maxrange, loadnew, out processedmaps, out processedspawners);

                        total_processed_maps += processedmaps;
                        total_processed_spawners += processedspawners;
                    }
                }
                // recursively search subdirectories for more .xml files
                string[] dirs = null;
                try
                {
                    dirs = Directory.GetDirectories(filename);
                }
                catch { }
                if (dirs != null && dirs.Length > 0)
                {
                    for (var index = 0; index < dirs.Length; index++)
                    {
                        string dir = dirs[index];

                        XmlLoadFromFile(dir, SpawnerPrefix, from, fromloc, frommap, loadrelative, maxrange, loadnew, out processedmaps, out processedspawners);

                        total_processed_maps += processedmaps;
                        total_processed_spawners += processedspawners;
                    }
                }
                if (from != null)
                {
                    from.SendMessage("Loaded a total of {0} .xml files and {2} spawners from directory {1}", total_processed_maps, filename, total_processed_spawners);
                }

                processedmaps = total_processed_maps;
                processedspawners = total_processed_spawners;
            }
            else
            {
                if (from != null)
                    from.SendMessage("{0} does not exist", filename);
            }
        }

        public static void XmlLoadFromFile(string filename, string SpawnerPrefix, Mobile from, bool loadrelative, int maxrange, bool loadnew, out int processedmaps, out int processedspawners)
        {
            processedmaps = 0;
            processedspawners = 0;

            if (from == null) return;

            XmlLoadFromFile(filename, SpawnerPrefix, from, from.Location, from.Map, loadrelative, maxrange, loadnew, out processedmaps, out processedspawners);
        }

        public static void XmlLoadFromStream(Stream fs, string filename, string SpawnerPrefix, Mobile from, Point3D fromloc, Map frommap, bool loadrelative, int maxrange, bool loadnew, out int processedmaps, out int processedspawners)
        {
            XmlLoadFromStream(fs, filename, SpawnerPrefix, from, fromloc, frommap, loadrelative, maxrange, loadnew, out processedmaps, out processedspawners, false);
        }

        public static void XmlLoadFromStream(Stream fs, string filename, string SpawnerPrefix, Mobile from, Point3D fromloc, Map frommap, bool loadrelative, int maxrange, bool loadnew, out int processedmaps, out int processedspawners, bool verbose)
        {
            processedmaps = 0;
            processedspawners = 0;

            if (fs == null) return;

            // assign an id that will be used to distinguish the newly loaded spawners by appending it to their name
            Guid newloadid = Guid.NewGuid();

            int TotalCount = 0;
            int TrammelCount = 0;
            int FeluccaCount = 0;
            int IlshenarCount = 0;
            int MalasCount = 0;
            int TokunoCount = 0;
            int OtherCount = 0;
            bool questionable_spawner = false;
            bool bad_spawner = false;
            int badcount = 0;
            int questionablecount = 0;

            int failedobjectitemcount = 0;
            int failedsetitemcount = 0;
            int relativex = -1;
            int relativey = -1;
            int relativez = 0;
            Map relativemap = null;

            if (from != null)
                from.SendMessage($"Loading {"XmlSpawner"} objects{(!string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty)} from file {filename}.");

            // Create the data set
            DataSet ds = new DataSet(SpawnDataSetName);

            // Read in the file
            bool fileerror = false;
            try
            {
                ds.ReadXml(fs);
            }
            catch
            {
                if (from != null)
                    from.SendMessage(33, "Error reading xml file {0}", filename);
                fileerror = true;
            }
            // close the file
            fs.Close();
            if (fileerror) return;

            // Check that at least a single table was loaded
            if (ds.Tables.Count > 0)
            {
                // Add each spawn point to the current map
                if (ds.Tables[SpawnTablePointName] != null && ds.Tables[SpawnTablePointName].Rows.Count > 0)
                {
                    for (var index = 0; index < ds.Tables[SpawnTablePointName].Rows.Count; index++)
                    {
                        DataRow dr = ds.Tables[SpawnTablePointName].Rows[index];
                        // load in the spawner info.  Certain fields are required and therefore cannot be ignored
                        // the exception handler for those will flag bad_spawner and the result will be logged

                        // Each row makes up a single spawner
                        string SpawnName = "Spawner";
                        try
                        {
                            SpawnName = (string) dr["Name"];
                        }
                        catch
                        {
                            questionable_spawner = true;
                        }

                        if (loadnew)
                        {
                            // append the new id to the name
                            SpawnName = $"{SpawnName}-{newloadid}";
                        }

                        // Check if there is any spawner name criteria specified on the load
                        if (string.IsNullOrEmpty(SpawnerPrefix) || SpawnName.StartsWith(SpawnerPrefix))
                        {
                            // Try load the GUID (might not work so create a new GUID)
                            Guid SpawnId = Guid.NewGuid();
                            if (!loadnew)
                            {
                                try
                                {
                                    SpawnId = new Guid((string) dr["UniqueId"]);
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                // change the dataset guid to the newly created one when new loading
                                try
                                {
                                    dr["UniqueId"] = SpawnId;
                                }
                                catch
                                {
                                    Console.WriteLine("unable to set UniqueId");
                                }
                            }

                            int SpawnCentreX = fromloc.X;
                            int SpawnCentreY = fromloc.Y;
                            int SpawnCentreZ = fromloc.Z;

                            try
                            {
                                SpawnCentreX = int.Parse((string) dr["CentreX"]);
                            }
                            catch
                            {
                                bad_spawner = true;
                            }

                            try
                            {
                                SpawnCentreY = int.Parse((string) dr["CentreY"]);
                            }
                            catch
                            {
                                bad_spawner = true;
                            }

                            try
                            {
                                SpawnCentreZ = int.Parse((string) dr["CentreZ"]);
                            }
                            catch
                            {
                                bad_spawner = true;
                            }

                            int SpawnX = SpawnCentreX;
                            int SpawnY = SpawnCentreY;
                            int SpawnWidth = 0;
                            int SpawnHeight = 0;
                            try
                            {
                                SpawnX = int.Parse((string) dr["X"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            try
                            {
                                SpawnY = int.Parse((string) dr["Y"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            try
                            {
                                SpawnWidth = int.Parse((string) dr["Width"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            try
                            {
                                SpawnHeight = int.Parse((string) dr["Height"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            // Try load the InContainer (default to false)
                            bool InContainer = false;
                            int ContainerX = 0;
                            int ContainerY = 0;
                            int ContainerZ = 0;
                            try
                            {
                                InContainer = bool.Parse((string) dr["InContainer"]);
                            }
                            catch
                            {
                            }

                            if (InContainer)
                            {
                                try
                                {
                                    ContainerX = int.Parse((string) dr["ContainerX"]);
                                }
                                catch
                                {
                                }

                                try
                                {
                                    ContainerY = int.Parse((string) dr["ContainerY"]);
                                }
                                catch
                                {
                                }

                                try
                                {
                                    ContainerZ = int.Parse((string) dr["ContainerZ"]);
                                }
                                catch
                                {
                                }
                            }

                            // Get the map (default to the mobiles map) if the relative distance is too great, then use the defined map

                            Map SpawnMap = frommap;

                            string XmlMapName = frommap.Name;

                            //if(!loadrelative && !loadnew)
                            {
                                // Try to get the "map" field, but in case it doesn't exist, catch and discard the exception
                                try
                                {
                                    XmlMapName = (string) dr["Map"];
                                }
                                catch
                                {
                                    questionable_spawner = true;
                                }

                                // Convert the xml map value to a real map object
                                if (string.Compare(XmlMapName, Map.Trammel.Name, true) == 0 || XmlMapName == "Trammel")
                                {
                                    SpawnMap = Map.Trammel;
                                    TrammelCount++;
                                }
                                else if (string.Compare(XmlMapName, Map.Felucca.Name, true) == 0 ||
                                         XmlMapName == "Felucca")
                                {
                                    SpawnMap = Map.Felucca;
                                    FeluccaCount++;
                                }
                                else if (string.Compare(XmlMapName, Map.Ilshenar.Name, true) == 0 ||
                                         XmlMapName == "Ilshenar")
                                {
                                    SpawnMap = Map.Ilshenar;
                                    IlshenarCount++;
                                }
                                else if (string.Compare(XmlMapName, Map.Malas.Name, true) == 0 || XmlMapName == "Malas")
                                {
                                    SpawnMap = Map.Malas;
                                    MalasCount++;
                                }
                                else if (string.Compare(XmlMapName, Map.Tokuno.Name, true) == 0 ||
                                         XmlMapName == "Tokuno")
                                {
                                    SpawnMap = Map.Tokuno;
                                    TokunoCount++;
                                }
                                else
                                {
                                    try
                                    {
                                        SpawnMap = Map.Parse(XmlMapName);
                                    }
                                    catch
                                    {
                                    }

                                    OtherCount++;
                                }
                            }

                            // test to see whether the distance between the relative center point and the spawner is too great.  If so then dont do relative
                            if (relativex == -1 && relativey == -1)
                            {
                                // the first xml entry in the file will determine the origin
                                relativex = SpawnCentreX;
                                relativey = SpawnCentreY;
                                relativez = SpawnCentreZ;

                                // and also the relative map to relocate from
                                relativemap = SpawnMap;
                            }

                            int SpawnRelZ = 0;
                            int OrigZ = SpawnCentreZ;

                            if (loadrelative && Math.Abs(relativex - SpawnCentreX) <= maxrange &&
                                Math.Abs(relativey - SpawnCentreY) <= maxrange && SpawnMap == relativemap)
                            {
                                // its within range so shift it
                                SpawnCentreX -= relativex - fromloc.X;
                                SpawnCentreY -= relativey - fromloc.Y;
                                SpawnX -= relativex - fromloc.X;
                                SpawnY -= relativey - fromloc.Y;
                                // force it to autosearch for Z when it places it but hold onto relative Z info just in case it can be placed there
                                SpawnRelZ = relativez - fromloc.Z;
                                SpawnCentreZ = short.MinValue;
                            }

                            // if relative loading has been specified, see if the loaded map is the same as the relativemap and relocate.
                            // if it doesnt match then just leave it
                            if (loadrelative && (relativemap == SpawnMap))
                            {
                                SpawnMap = frommap;
                            }

                            if (SpawnMap == Map.Internal)
                                bad_spawner = true;

                            // Try load the IsRelativeHomeRange (default to true)
                            bool SpawnIsRelativeHomeRange = true;
                            try
                            {
                                SpawnIsRelativeHomeRange = bool.Parse((string) dr["IsHomeRangeRelative"]);
                            }
                            catch
                            {
                            }

                            int SpawnHomeRange = 5;
                            try
                            {
                                SpawnHomeRange = int.Parse((string) dr["Range"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            int SpawnMaxCount = 1;
                            try
                            {
                                SpawnMaxCount = int.Parse((string) dr["MaxCount"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            //deal with double format for delay.  default is the old minute format
                            bool delay_in_sec = false;
                            try
                            {
                                delay_in_sec = bool.Parse((string) dr["DelayInSec"]);
                            }
                            catch
                            {
                            }

                            TimeSpan SpawnMinDelay = TimeSpan.FromMinutes(5);
                            TimeSpan SpawnMaxDelay = TimeSpan.FromMinutes(10);

                            if (delay_in_sec)
                            {
                                try
                                {
                                    SpawnMinDelay = TimeSpan.FromSeconds(int.Parse((string) dr["MinDelay"]));
                                }
                                catch
                                {
                                }

                                try
                                {
                                    SpawnMaxDelay = TimeSpan.FromSeconds(int.Parse((string) dr["MaxDelay"]));
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                try
                                {
                                    SpawnMinDelay = TimeSpan.FromMinutes(int.Parse((string) dr["MinDelay"]));
                                }
                                catch
                                {
                                }

                                try
                                {
                                    SpawnMaxDelay = TimeSpan.FromMinutes(int.Parse((string) dr["MaxDelay"]));
                                }
                                catch
                                {
                                }
                            }

                            TimeSpan SpawnMinRefractory = TimeSpan.FromMinutes(0);
                            try
                            {
                                SpawnMinRefractory = TimeSpan.FromMinutes(double.Parse((string) dr["MinRefractory"]));
                            }
                            catch
                            {
                            }

                            TimeSpan SpawnMaxRefractory = TimeSpan.FromMinutes(0);
                            try
                            {
                                SpawnMaxRefractory = TimeSpan.FromMinutes(double.Parse((string) dr["MaxRefractory"]));
                            }
                            catch
                            {
                            }

                            TimeSpan SpawnTODStart = TimeSpan.FromMinutes(0);
                            try
                            {
                                SpawnTODStart = TimeSpan.FromMinutes(double.Parse((string) dr["TODStart"]));
                            }
                            catch
                            {
                            }

                            TimeSpan SpawnTODEnd = TimeSpan.FromMinutes(0);
                            try
                            {
                                SpawnTODEnd = TimeSpan.FromMinutes(double.Parse((string) dr["TODEnd"]));
                            }
                            catch
                            {
                            }

                            int todmode = (int) TODModeType.Realtime;
                            TODModeType SpawnTODMode = TODModeType.Realtime;
                            try
                            {
                                todmode = int.Parse((string) dr["TODMode"]);
                            }
                            catch
                            {
                            }

                            switch (todmode)
                            {
                                case (int) TODModeType.Gametime:
                                    SpawnTODMode = TODModeType.Gametime;
                                    break;
                                case (int) TODModeType.Realtime:
                                    SpawnTODMode = TODModeType.Realtime;
                                    break;
                            }

                            int SpawnKillReset = defKillReset;
                            try
                            {
                                SpawnKillReset = int.Parse((string) dr["KillReset"]);
                            }
                            catch
                            {
                            }

                            string SpawnProximityMessage = null;
                            // proximity message
                            try
                            {
                                SpawnProximityMessage = (string) dr["ProximityTriggerMessage"];
                            }
                            catch
                            {
                            }

                            string SpawnItemTriggerName = null;
                            try
                            {
                                SpawnItemTriggerName = (string) dr["ItemTriggerName"];
                            }
                            catch
                            {
                            }

                            string SpawnNoItemTriggerName = null;
                            try
                            {
                                SpawnNoItemTriggerName = (string) dr["NoItemTriggerName"];
                            }
                            catch
                            {
                            }

                            string SpawnSpeechTrigger = null;
                            try
                            {
                                SpawnSpeechTrigger = (string) dr["SpeechTrigger"];
                            }
                            catch
                            {
                            }

                            string SpawnSkillTrigger = null;
                            try
                            {
                                SpawnSkillTrigger = (string) dr["SkillTrigger"];
                            }
                            catch
                            {
                            }

                            string SpawnMobTriggerName = null;
                            try
                            {
                                SpawnMobTriggerName = (string) dr["MobTriggerName"];
                            }
                            catch
                            {
                            }

                            string SpawnMobPropertyName = null;
                            try
                            {
                                SpawnMobPropertyName = (string) dr["MobPropertyName"];
                            }
                            catch
                            {
                            }

                            string SpawnPlayerPropertyName = null;
                            try
                            {
                                SpawnPlayerPropertyName = (string) dr["PlayerPropertyName"];
                            }
                            catch
                            {
                            }

                            double SpawnTriggerProbability = 1;
                            try
                            {
                                SpawnTriggerProbability = double.Parse((string) dr["TriggerProbability"]);
                            }
                            catch
                            {
                            }

                            int SpawnSequentialSpawning = -1;
                            try
                            {
                                SpawnSequentialSpawning = int.Parse((string) dr["SequentialSpawning"]);
                            }
                            catch
                            {
                            }

                            string SpawnRegionName = null;
                            try
                            {
                                SpawnRegionName = (string) dr["RegionName"];
                            }
                            catch
                            {
                            }

                            string SpawnConfigFile = null;
                            try
                            {
                                SpawnConfigFile = (string) dr["ConfigFile"];
                            }
                            catch
                            {
                            }

                            bool SpawnAllowGhost = false;
                            try
                            {
                                SpawnAllowGhost = bool.Parse((string) dr["AllowGhostTriggering"]);
                            }
                            catch
                            {
                            }

                            bool SpawnAllowNPC = false;
                            try
                            {
                                SpawnAllowNPC = bool.Parse((string) dr["AllowNPCTriggering"]);
                            }
                            catch
                            {
                            }

                            bool SpawnSpawnOnTrigger = false;
                            try
                            {
                                SpawnSpawnOnTrigger = bool.Parse((string) dr["SpawnOnTrigger"]);
                            }
                            catch
                            {
                            }

                            bool SpawnSmartSpawning = false;
                            try
                            {
                                SpawnSmartSpawning = bool.Parse((string) dr["SmartSpawning"]);
                            }
                            catch
                            {
                            }

                            bool TickReset = false;
                            try
                            {
                                TickReset = bool.Parse((string) dr["TickReset"]);
                            }
                            catch
                            {
                            }

                            string SpawnObjectPropertyName = null;
                            try
                            {
                                SpawnObjectPropertyName = (string) dr["ObjectPropertyName"];
                            }
                            catch
                            {
                            }

                            // we will assign this during the self-reference resolution pass
                            Item SpawnSetPropertyItem = null;

                            // we will assign this during the self-reference resolution pass
                            Item SpawnObjectPropertyItem = null;

                            // read the duration parameter from the xml file
                            // but older files wont have it so deal with that condition and set it to the default of "0", i.e. infinite duration
                            // Try to get the "Duration" field, but in case it doesn't exist, catch and discard the exception
                            TimeSpan SpawnDuration = TimeSpan.FromMinutes(0);
                            try
                            {
                                SpawnDuration = TimeSpan.FromMinutes(double.Parse((string) dr["Duration"]));
                            }
                            catch
                            {
                            }

                            TimeSpan SpawnDespawnTime = TimeSpan.FromHours(0);
                            try
                            {
                                SpawnDespawnTime = TimeSpan.FromHours(double.Parse((string) dr["DespawnTime"]));
                            }
                            catch
                            {
                            }

                            int SpawnProximityRange = -1;
                            // Try to get the "ProximityRange" field, but in case it doesn't exist, catch and discard the exception
                            try
                            {
                                SpawnProximityRange = int.Parse((string) dr["ProximityRange"]);
                            }
                            catch
                            {
                            }

                            int SpawnProximityTriggerSound = 0;
                            // Try to get the "ProximityTriggerSound" field, but in case it doesn't exist, catch and discard the exception
                            try
                            {
                                SpawnProximityTriggerSound = int.Parse((string) dr["ProximityTriggerSound"]);
                            }
                            catch
                            {
                            }

                            int SpawnAmount = 1;
                            try
                            {
                                SpawnAmount = int.Parse((string) dr["Amount"]);
                            }
                            catch
                            {
                            }

                            bool SpawnExternalTriggering = false;
                            try
                            {
                                SpawnExternalTriggering = bool.Parse((string) dr["ExternalTriggering"]);
                            }
                            catch
                            {
                            }

                            string waypointstr = null;
                            try
                            {
                                waypointstr = (string) dr["Waypoint"];
                            }
                            catch
                            {
                            }

                            WayPoint SpawnWaypoint = GetWaypoint(waypointstr);

                            int SpawnTeam = 0;
                            try
                            {
                                SpawnTeam = int.Parse((string) dr["Team"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            bool SpawnIsGroup = false;
                            try
                            {
                                SpawnIsGroup = bool.Parse((string) dr["IsGroup"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            bool SpawnIsRunning = false;
                            try
                            {
                                SpawnIsRunning = bool.Parse((string) dr["IsRunning"]);
                            }
                            catch
                            {
                                questionable_spawner = true;
                            }

                            // try loading the new spawn specifications first
                            SpawnObject[] Spawns = Array.Empty<SpawnObject>();
                            bool havenew = true;
                            try
                            {
                                Spawns = SpawnObject.LoadSpawnObjectsFromString2((string) dr["Objects2"]);
                            }
                            catch
                            {
                                havenew = false;
                            }

                            if (!havenew)
                            {
                                // try loading the new spawn specifications
                                try
                                {
                                    Spawns = SpawnObject.LoadSpawnObjectsFromString((string) dr["Objects"]);
                                }
                                catch
                                {
                                    questionable_spawner = true;
                                }

                                // can only have one of these defined
                            }

                            // do a check on the location of the spawner
                            if (!IsValidMapLocation(SpawnCentreX, SpawnCentreY, SpawnMap))
                            {
                                if (from != null)
                                    from.SendMessage(33, "Invalid location '{0}' at [{1} {2}] in {3}",
                                        SpawnName, SpawnCentreX, SpawnCentreY, XmlMapName);
                                bad_spawner = true;
                            }

                            XmlSpawner OldSpawner = null; // Check if this spawner already exists

                            bool found_container = false;
                            bool found_spawner = false;

                            Container spawn_container = null;

                            if (!bad_spawner)
                            {
                                foreach (Item i in World.Items.Values)
                                {
                                    // Check if the spawners GUID is the same as the one being loaded
                                    // and that the spawners map is the same as the one being loaded
                                    if (i is XmlSpawner checkSpawner && (checkSpawner.UniqueId == SpawnId.ToString()))
                                    {
                                        OldSpawner = checkSpawner;
                                        found_spawner = true;
                                    }

                                    //look for containers with the spawn coordinates if the incontainer flag is set
                                    if (InContainer && !found_container && (i is Container container) &&
                                        (SpawnCentreX == container.Location.X) &&
                                        (SpawnCentreY == container.Location.Y) &&
                                        (SpawnCentreZ == container.Location.Z || SpawnCentreZ == short.MinValue))
                                    {
                                        // assume this is the container that the spawner was in
                                        found_container = true;
                                        spawn_container = container;
                                    }

                                    // ok we can break if we have handled both the spawner and any containers
                                    if (found_spawner && (found_container || !InContainer))
                                        break;
                                }
                            }

                            // test to see whether the spawner specification was valid, bad, or questionable
                            if (bad_spawner)
                            {
                                badcount++;
                                if (from != null)
                                    from.SendMessage(33, "Invalid spawner");
                                // log it
                                long fileposition = -1;
                                try
                                {
                                    fileposition = fs.Position;
                                }
                                catch
                                {
                                }

                                try
                                {
                                    using (StreamWriter op = new StreamWriter("badxml.log", true))
                                    {
                                        op.WriteLine("# Invalid spawner : {0}: Fileposition {1} {2}", DateTime.UtcNow,
                                            fileposition, filename);
                                        op.WriteLine();
                                    }
                                }
                                catch
                                {
                                }
                            }
                            else if (questionable_spawner)
                            {
                                questionablecount++;
                                if (from != null)
                                    from.SendMessage(33, "Questionable spawner '{0}' at [{1} {2}] in {3}",
                                        SpawnName, SpawnCentreX, SpawnCentreY, XmlMapName);
                                // log it
                                long fileposition = -1;
                                try
                                {
                                    fileposition = fs.Position;
                                }
                                catch
                                {
                                }

                                try
                                {
                                    using (StreamWriter op = new StreamWriter("badxml.log", true))
                                    {
                                        op.WriteLine(
                                            "# Questionable spawner : {0}: Format: X Y Z Map SpawnerName Fileposition Xmlfile",
                                            DateTime.UtcNow);
                                        op.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", SpawnCentreX, SpawnCentreY,
                                            SpawnCentreZ, XmlMapName, SpawnName, fileposition, filename);
                                        op.WriteLine();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            if (!bad_spawner)
                            {
                                // Delete the old spawner if it exists
                                if (OldSpawner != null)
                                    OldSpawner.Delete();

                                // Create the new spawner
                                XmlSpawner TheSpawn = new XmlSpawner(SpawnId, SpawnX, SpawnY, SpawnWidth, SpawnHeight,
                                    SpawnName, SpawnMaxCount,
                                    SpawnMinDelay, SpawnMaxDelay, SpawnDuration, SpawnProximityRange,
                                    SpawnProximityTriggerSound, SpawnAmount,
                                    SpawnTeam, SpawnHomeRange, SpawnIsRelativeHomeRange, Spawns, SpawnMinRefractory,
                                    SpawnMaxRefractory, SpawnTODStart,
                                    SpawnTODEnd, SpawnObjectPropertyItem, SpawnObjectPropertyName,
                                    SpawnProximityMessage, SpawnItemTriggerName, SpawnNoItemTriggerName,
                                    SpawnSpeechTrigger, SpawnMobTriggerName, SpawnMobPropertyName,
                                    SpawnPlayerPropertyName, SpawnTriggerProbability,
                                    SpawnSetPropertyItem, SpawnIsGroup, SpawnTODMode, SpawnKillReset,
                                    SpawnExternalTriggering, SpawnSequentialSpawning,
                                    SpawnRegionName, SpawnAllowGhost, SpawnAllowNPC, SpawnSpawnOnTrigger,
                                    SpawnConfigFile, SpawnDespawnTime, SpawnSkillTrigger, SpawnSmartSpawning,
                                    SpawnWaypoint)
                                {
                                    m_DisableGlobalAutoReset = TickReset
                                };

                                // Try to find a valid Z height if required (SpawnCentreZ = short.MinValue)
                                int NewZ = 0;

                                if (loadrelative && HasTileSurface(SpawnMap, SpawnCentreX, SpawnCentreY, OrigZ - SpawnRelZ))
                                {
                                    NewZ = OrigZ - SpawnRelZ;
                                }
                                else if (SpawnCentreZ == short.MinValue)
                                {
                                    NewZ = SpawnMap.GetAverageZ(SpawnCentreX, SpawnCentreY);

                                    if (SpawnMap.CanFit(SpawnCentreX, SpawnCentreY, NewZ, SpawnFitSize) == false)
                                    {
                                        for (int x = 1; x <= 39; x++)
                                        {
                                            if (SpawnMap.CanFit(SpawnCentreX, SpawnCentreY, NewZ + x, SpawnFitSize))
                                            {
                                                NewZ += x;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    NewZ = SpawnCentreZ; // This spawn point already has a defined Z location, so use it
                                }

                                // if this is a container held spawner, drop it in the container
                                if (found_container && spawn_container != null && !spawn_container.Deleted)
                                {
                                    TheSpawn.Location = new Point3D(ContainerX, ContainerY, ContainerZ);
                                    spawn_container.AddItem(TheSpawn);
                                }
                                else
                                {
                                    // disable the X_Y adjustments in OnLocationChange
                                    IgnoreLocationChange = true;
                                    TheSpawn.MoveToWorld(new Point3D(SpawnCentreX, SpawnCentreY, NewZ), SpawnMap);
                                }

                                // reset the spawner
                                TheSpawn.Reset();
                                TheSpawn.Running = SpawnIsRunning;

                                // update subgroup-specific next spawn times
                                TheSpawn.NextSpawn = TimeSpan.Zero;
                                TheSpawn.ResetNextSpawnTimes();


                                // Send a message to the client that the spawner is created
                                if (from != null && verbose)
                                    from.SendMessage(188, "Created '{0}' in {1} at {2}", TheSpawn.Name,
                                        TheSpawn.Map.Name, TheSpawn.Location.ToString());

                                // Increment the count
                                TotalCount++;
                            }

                            bad_spawner = false;
                            questionable_spawner = false;
                        }
                    }
                }

                if (from != null)
                {
                    from.SendMessage("Resolving spawner self references");
                }

                if (ds.Tables[SpawnTablePointName] != null && ds.Tables[SpawnTablePointName].Rows.Count > 0)
                {
                    for (var index = 0; index < ds.Tables[SpawnTablePointName].Rows.Count; index++)
                    {
                        DataRow dr = ds.Tables[SpawnTablePointName].Rows[index];
                        // Try load the GUID
                        bool badid = false;
                        Guid SpawnId = Guid.NewGuid();
                        try
                        {
                            SpawnId = new Guid((string) dr["UniqueId"]);
                        }
                        catch
                        {
                            badid = true;
                        }

                        if (badid) continue;
                        // Get the map
                        Map SpawnMap = frommap;
                        string XmlMapName = frommap.Name;

                        if (!loadrelative)
                        {
                            try
                            {
                                XmlMapName = (string) dr["Map"];
                            }
                            catch
                            {
                            }

                            // Convert the xml map value to a real map object
                            try
                            {
                                SpawnMap = Map.Parse(XmlMapName);
                            }
                            catch
                            {
                            }
                        }

                        bool found_spawner = false;
                        XmlSpawner OldSpawner = null;

                        foreach (Item i in World.Items.Values)
                        {
                            // Check if the spawners GUID is the same as the one being loaded
                            // and that the spawners map is the same as the one being loaded
                            if (i is XmlSpawner spawner && spawner.UniqueId == SpawnId.ToString())
                            {
                                OldSpawner = spawner;
                                found_spawner = true;
                            }

                            if (found_spawner)
                                break;
                        }

                        if (found_spawner && OldSpawner != null && !OldSpawner.Deleted)
                        {
                            // resolve item name references since they may have referred to spawners that were just created
                            string setObjectName = null;
                            try
                            {
                                setObjectName = (string) dr["SetPropertyItemName"];
                            }
                            catch
                            {
                            }

                            if (!string.IsNullOrEmpty(setObjectName))
                            {
                                // try to parse out the type information if it has also been saved
                                string[] typeargs = setObjectName.Split(",".ToCharArray(), 2);
                                string typestr = null;
                                string namestr = setObjectName;

                                if (typeargs.Length > 1)
                                {
                                    namestr = typeargs[0];
                                    typestr = typeargs[1];
                                }

                                // if this is a new load then assume that it will be referring to another newly loaded object so append the newloadid
                                if (loadnew)
                                {
                                    string tmpsetObjectName = $"{namestr}-{newloadid}";
                                    OldSpawner.m_SetPropertyItem =
                                        BaseXmlSpawner.FindItemByName(null, tmpsetObjectName, typestr);
                                }

                                // if this fails then try the original
                                if (OldSpawner.m_SetPropertyItem == null)
                                {
                                    OldSpawner.m_SetPropertyItem =
                                        BaseXmlSpawner.FindItemByName(null, namestr, typestr);
                                }

                                if (OldSpawner.m_SetPropertyItem == null)
                                {
                                    failedsetitemcount++;
                                    if (from != null)
                                        from.SendMessage(33,
                                            "Failed to initialize SetItemProperty Object '{0}' on ' '{1}' at [{2} {3}] in {4}",
                                            setObjectName, OldSpawner.Name, OldSpawner.Location.X,
                                            OldSpawner.Location.Y, OldSpawner.Map);
                                    // log it
                                    try
                                    {
                                        using (StreamWriter op = new StreamWriter("badxml.log", true))
                                        {
                                            op.WriteLine(
                                                "# Failed SetItemProperty Object initialization : {0}: Format: ObjectName X Y Z Map SpawnerName Xmlfile",
                                                DateTime.UtcNow);
                                            op.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                setObjectName, OldSpawner.Location.X, OldSpawner.Location.Y,
                                                OldSpawner.Location.Z, OldSpawner.Map, OldSpawner.Name, filename);
                                            op.WriteLine();
                                        }
                                    }

                                    catch
                                    {
                                    }
                                }
                            }

                            string triggerObjectName = null;
                            try
                            {
                                triggerObjectName = (string) dr["ObjectPropertyItemName"];
                            }
                            catch
                            {
                            }

                            if (!string.IsNullOrEmpty(triggerObjectName))
                            {
                                string[] typeargs = triggerObjectName.Split(",".ToCharArray(), 2);
                                string typestr = null;
                                string namestr = triggerObjectName;

                                if (typeargs.Length > 1)
                                {
                                    namestr = typeargs[0];
                                    typestr = typeargs[1];
                                }

                                // if this is a new load then assume that it will be referring to another newly loaded object so append the newloadid
                                if (loadnew)
                                {
                                    string tmptriggerObjectName = $"{namestr}-{newloadid}";
                                    OldSpawner.m_ObjectPropertyItem =
                                        BaseXmlSpawner.FindItemByName(null, tmptriggerObjectName, typestr);
                                }

                                // if this fails then try the original
                                if (OldSpawner.m_ObjectPropertyItem == null)
                                {
                                    OldSpawner.m_ObjectPropertyItem =
                                        BaseXmlSpawner.FindItemByName(null, namestr, typestr);
                                }

                                if (OldSpawner.m_ObjectPropertyItem == null)
                                {
                                    failedobjectitemcount++;
                                    if (from != null)
                                        from.SendMessage(33,
                                            "Failed to initialize TriggerObject '{0}' on ' '{1}' at [{2} {3}] in {4}",
                                            triggerObjectName, OldSpawner.Name, OldSpawner.Location.X,
                                            OldSpawner.Location.Y, OldSpawner.Map);
                                    // log it
                                    try
                                    {
                                        using (StreamWriter op = new StreamWriter("badxml.log", true))
                                        {
                                            op.WriteLine(
                                                "# Failed TriggerObject initialization : {0}: Format: ObjectName X Y Z Map SpawnerName Xmlfile",
                                                DateTime.UtcNow);
                                            op.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                                triggerObjectName, OldSpawner.Location.X, OldSpawner.Location.Y,
                                                OldSpawner.Location.Z, OldSpawner.Map, OldSpawner.Name, filename);
                                            op.WriteLine();
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // close the file
            try
            {
                fs.Close();
            }
            catch { }

            if (from != null)
                from.SendMessage("{0} spawner(s) were created from file {1} [Trammel={2}, Felucca={3}, Ilshenar={4}, Malas={5}, Tokuno={6} Other={7}].",
                    TotalCount, filename, TrammelCount, FeluccaCount, IlshenarCount, MalasCount, TokunoCount, OtherCount);
            if (failedobjectitemcount > 0)
            {
                if (from != null)
                    from.SendMessage(33, "Failed to initialize TriggerObjects in {0} spawners. Saved to 'badxml.log'", failedobjectitemcount);
            }
            if (failedsetitemcount > 0)
            {
                if (from != null)
                    from.SendMessage(33, "Failed to initialize SetItemProperty Objects in {0} spawners. Saved to 'badxml.log'", failedsetitemcount);
            }
            if (badcount > 0)
            {
                if (from != null)
                    from.SendMessage(33, "{0} bad spawners detected. Saved to 'badxml.log'", badcount);
            }
            if (questionablecount > 0)
            {
                if (from != null)
                    from.SendMessage(33, "{0} questionable spawners detected. Saved to 'badxml.log'", questionablecount);
            }

            processedmaps = 1;
            processedspawners = TotalCount;
        }

        public static string LocateFile(string filename)
        {
            bool found = false;

            string dirname = null;

            if (Directory.Exists(XmlSpawnDir))
            {
                // get it from the defaults directory if it exists
                dirname = $"{XmlSpawnDir}/{filename}";
                found = File.Exists(dirname) || Directory.Exists(dirname);
            }

            if (!found)
            {
                // otherwise just get it from the main installation dir
                dirname = filename;
            }

            return dirname;
        }

        [Usage("XmlLoad <SpawnFile or directory> [SpawnerPrefixFilter]")]
        [Description("Loads XmlSpawner objects (replacing existing spawners with matching GUIDs) into the proper map as defined in the file supplied.")]
        public static void Load_OnCommand(CommandEventArgs e)
        {
            var m = e.Mobile;

            if (m == null || m.AccessLevel >= DiskAccessLevel)
            {
                if (e.Arguments.Length >= 1)
                {
                    string filename = LocateFile(e.Arguments[0]);

                    // Spawner load criteria (if any)
                    string SpawnerPrefix = string.Empty;

                    // Check if there is an argument provided (load criteria)
                    if (e.Arguments.Length > 1)
                        SpawnerPrefix = e.Arguments[1];
                    int processedmaps;
                    int processedspawners;

                    XmlLoadFromFile(filename, SpawnerPrefix, m, false, 0, false, out processedmaps, out processedspawners);
                }
                else if (m != null)
                {
                    e.Mobile.SendMessage("Usage:  {0} <SpawnFile or directory> [SpawnerPrefixFilter]", e.Command);
                }
            }
            else
            {
                e.Mobile.SendMessage("You do not have rights to perform this command.");
            }
        }

        [Usage("XmlSpawnerSave <SpawnFile> [SpawnerPrefixFilter]")]
        [Description("Saves all XmlSpawner objects from the current map into the file supplied.")]
        public static void Save_OnCommand(CommandEventArgs e)
        {
            SaveSpawns(e, false, false);
        }

        [Usage("XmlSpawnerSaveAll <SpawnFile> [SpawnerPrefixFilter]")]
        [Description("Saves ALL XmlSpawner objects from the entire world into the file supplied.")]
        public static void SaveAll_OnCommand(CommandEventArgs e)
        {
            SaveSpawns(e, true, false);
        }

        public class XmlSaveSingle : BaseCommand
        {
            public XmlSaveSingle()
            {
                AccessLevel = DiskAccessLevel;
                Supports = CommandSupport.Single;
                Commands = new[] { "XmlSaveSingle" };
                ObjectTypes = ObjectTypes.Items;
                Usage = "XmlSaveSingle <filename>";
                Description = "Saves single xmlspawner to specified file.";
            }

            public override void Execute(CommandEventArgs e, object obj)
            {
                if (e == null || e.Mobile == null || e.Arguments == null) return;

                if (e.Arguments.Length < 1)
                {
                    e.Mobile.SendMessage("Usage:  {0} <SpawnFile> (without spaces!!)", e.Command);
                    return;
                }

                string filename = e.Arguments[0];

                XmlSpawner xmlspawner = obj as XmlSpawner;

                if (xmlspawner == null)
                {
                    e.Mobile.SendMessage("You can select only XmlSpawner objects!");
                    return;
                }

                Mobile m = e.Mobile;

                CommandLogging.WriteLine(m, "{0} {1} Saving XmlSpawner {2} on file {3}", m.AccessLevel, CommandLogging.Format(m), CommandLogging.Format(xmlspawner), CommandLogging.Format(filename));
                SaveSpawns(m, xmlspawner, filename);
            }
        }

        private static void SaveSpawns(Mobile m, XmlSpawner xmlspawner, string filename)
        {
            if (m.AccessLevel < DiskAccessLevel)
            {
                m.SendMessage("You do not have rights to perform this command.");
                return;
            }

            string dirname;

            if (Directory.Exists(XmlSpawnDir) && filename != null && !filename.StartsWith("/") && !filename.StartsWith("\\"))
            {
                // put it in the defaults directory if it exists
                dirname = $"{XmlSpawnDir}/{filename}";
            }
            else
            {
                // otherwise just put it in the main installation dir
                dirname = filename;
            }

            m.SendMessage("Saving object in folder {0} - file {1} - spawner {2}.", dirname, filename, xmlspawner);

            List<XmlSpawner> saveslist = new List<XmlSpawner>(1);
            saveslist.Add(xmlspawner);
            SaveSpawnList(m, saveslist, dirname, false, true);
        }

        private static void SaveSpawns(CommandEventArgs e, bool SaveAllMaps, bool oldformat)
        {
            if (e == null || e.Mobile == null || e.Arguments == null || e.Arguments.Length < 1) return;

            if (e.Mobile.AccessLevel < DiskAccessLevel)
            {
                e.Mobile.SendMessage("You do not have rights to perform this command.");
                return;
            }

            if (e.Arguments != null && e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Usage:  {0} <SpawnFile> [SpawnerPrefixFilter]", e.Command);
                return;
            }

            // Spawner save criteria (if any)
            string SpawnerPrefix = string.Empty;

            // Check if there is an argument provided (save criteria)
            if (e.Arguments.Length > 1)
                SpawnerPrefix = e.Arguments[1];

            string filename = e.Arguments[0];

            string dirname;
            if (Directory.Exists(XmlSpawnDir) && filename != null && !filename.StartsWith("/") && !filename.StartsWith("\\"))
            {
                // put it in the defaults directory if it exists
                dirname = $"{XmlSpawnDir}/{filename}";
            }
            else
            {
                // otherwise just put it in the main installation dir
                dirname = filename;
            }

            if (SaveAllMaps)
                e.Mobile.SendMessage($"Saving {"XmlSpawner"} objects{(!string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty)} to file {dirname} from {e.Mobile.Map}.");
            else
                e.Mobile.SendMessage($"Saving {"XmlSpawner"} obejcts{(!string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty)} to file {dirname} from the entire world.");


            List<XmlSpawner> saveslist = new List<XmlSpawner>();

            // Add each spawn point to the list
            foreach (Item i in World.Items.Values)
            {
                if (i is XmlSpawner spawner && !spawner.Deleted && (SaveAllMaps || spawner.Map == e.Mobile.Map) && !(spawner.RootParent is Mobile) && (string.IsNullOrEmpty(SpawnerPrefix) || spawner.Name != null && spawner.Name.StartsWith(SpawnerPrefix)))
                {
                    saveslist.Add(spawner);
                }
            }

            // save the list
            SaveSpawnList(e.Mobile, saveslist, dirname, oldformat, true);
        }

        public static bool SaveSpawnList(Mobile from, List<XmlSpawner> savelist, string dirname, bool oldformat, bool verbose)
        {
            if (string.IsNullOrEmpty(dirname)) return false;


            bool save_ok = true;
            FileStream fs = null;

            try
            {
                // Create the FileStream to write with.
                fs = new FileStream(dirname, FileMode.Create);
            }
            catch
            {
                if (from != null)
                    from.SendMessage("Error creating file {0}", dirname);
                save_ok = false;
            }

            // so far so good
            if (save_ok)
            {
                save_ok = SaveSpawnList(from, savelist, dirname, fs, oldformat, verbose);
            }

            if (!save_ok && from != null)
            {
                from.SendMessage("Unable to complete save operation.");
            }

            return save_ok;
        }

        public static bool SaveSpawnList(Mobile from, List<XmlSpawner> savelist, string dirname, Stream stream, bool oldformat, bool verbose)
        {
            if (savelist == null || stream == null)
            {
                return false;
            }

            int TotalCount = 0;
            int TrammelCount = 0;
            int FeluccaCount = 0;
            int IlshenarCount = 0;
            int MalasCount = 0;
            int TokunoCount = 0;
            int OtherCount = 0;

            // Create the data set
            DataSet ds = new DataSet(SpawnDataSetName);

            // Load the data set up
            ds.Tables.Add(SpawnTablePointName);

            // Create spawn point schema
            ds.Tables[SpawnTablePointName].Columns.Add("Name");
            ds.Tables[SpawnTablePointName].Columns.Add("UniqueId");
            ds.Tables[SpawnTablePointName].Columns.Add("Map");
            ds.Tables[SpawnTablePointName].Columns.Add("X");
            ds.Tables[SpawnTablePointName].Columns.Add("Y");
            ds.Tables[SpawnTablePointName].Columns.Add("Width");
            ds.Tables[SpawnTablePointName].Columns.Add("Height");
            ds.Tables[SpawnTablePointName].Columns.Add("CentreX");
            ds.Tables[SpawnTablePointName].Columns.Add("CentreY");
            ds.Tables[SpawnTablePointName].Columns.Add("CentreZ");
            ds.Tables[SpawnTablePointName].Columns.Add("Range");
            ds.Tables[SpawnTablePointName].Columns.Add("MaxCount");
            ds.Tables[SpawnTablePointName].Columns.Add("MinDelay");
            ds.Tables[SpawnTablePointName].Columns.Add("MaxDelay");
            // deal with the double format for delay. old format stored them as minutes in int format. that meant that short delays were lost
            // proper solution would simply be to store as doubles, but older progs still assume int format (like spawneditor)
            // so this is the solution.  add a flag and do it both ways.
            ds.Tables[SpawnTablePointName].Columns.Add("DelayInSec");

            // add the duration and proximity range and sound parameters, and in container flag and coords inside the container
            ds.Tables[SpawnTablePointName].Columns.Add("Duration");
            ds.Tables[SpawnTablePointName].Columns.Add("DespawnTime");
            ds.Tables[SpawnTablePointName].Columns.Add("ProximityRange");
            ds.Tables[SpawnTablePointName].Columns.Add("ProximityTriggerSound");
            ds.Tables[SpawnTablePointName].Columns.Add("ProximityTriggerMessage");
            ds.Tables[SpawnTablePointName].Columns.Add("ObjectPropertyName");
            ds.Tables[SpawnTablePointName].Columns.Add("ObjectPropertyItemName");
            ds.Tables[SpawnTablePointName].Columns.Add("SetPropertyItemName");
            ds.Tables[SpawnTablePointName].Columns.Add("ItemTriggerName");
            ds.Tables[SpawnTablePointName].Columns.Add("NoItemTriggerName");
            ds.Tables[SpawnTablePointName].Columns.Add("MobTriggerName");
            ds.Tables[SpawnTablePointName].Columns.Add("MobPropertyName");
            ds.Tables[SpawnTablePointName].Columns.Add("PlayerPropertyName");
            ds.Tables[SpawnTablePointName].Columns.Add("TriggerProbability");
            ds.Tables[SpawnTablePointName].Columns.Add("SpeechTrigger");
            ds.Tables[SpawnTablePointName].Columns.Add("SkillTrigger");
            ds.Tables[SpawnTablePointName].Columns.Add("InContainer");
            ds.Tables[SpawnTablePointName].Columns.Add("ContainerX");
            ds.Tables[SpawnTablePointName].Columns.Add("ContainerY");
            ds.Tables[SpawnTablePointName].Columns.Add("ContainerZ");
            ds.Tables[SpawnTablePointName].Columns.Add("MinRefractory");
            ds.Tables[SpawnTablePointName].Columns.Add("MaxRefractory");
            ds.Tables[SpawnTablePointName].Columns.Add("TODStart");
            ds.Tables[SpawnTablePointName].Columns.Add("TODEnd");
            ds.Tables[SpawnTablePointName].Columns.Add("TODMode");
            ds.Tables[SpawnTablePointName].Columns.Add("KillReset");
            ds.Tables[SpawnTablePointName].Columns.Add("ExternalTriggering");
            ds.Tables[SpawnTablePointName].Columns.Add("SequentialSpawning");
            ds.Tables[SpawnTablePointName].Columns.Add("RegionName");
            ds.Tables[SpawnTablePointName].Columns.Add("AllowGhostTriggering");
            ds.Tables[SpawnTablePointName].Columns.Add("AllowNPCTriggering");
            ds.Tables[SpawnTablePointName].Columns.Add("SpawnOnTrigger");
            ds.Tables[SpawnTablePointName].Columns.Add("ConfigFile");
            ds.Tables[SpawnTablePointName].Columns.Add("SmartSpawning");
            ds.Tables[SpawnTablePointName].Columns.Add("TickReset");
            ds.Tables[SpawnTablePointName].Columns.Add("WayPoint");
            ds.Tables[SpawnTablePointName].Columns.Add("Team");
            // amount for stacked item spawns
            ds.Tables[SpawnTablePointName].Columns.Add("Amount");
            ds.Tables[SpawnTablePointName].Columns.Add("IsGroup");
            ds.Tables[SpawnTablePointName].Columns.Add("IsRunning");
            ds.Tables[SpawnTablePointName].Columns.Add("IsHomeRangeRelative");
            ds.Tables[SpawnTablePointName].Columns.Add(oldformat ? "Objects" : "Objects2");

            // Always export sorted by UUID to help diffs
            savelist.Sort((a, b) =>
            {
                return a.UniqueId.CompareTo(b.UniqueId);
            });

            // Add each spawn point to the new table
            for (var index = 0; index < savelist.Count; index++)
            {
                XmlSpawner sp = savelist[index];

                if (sp == null || sp.Map == null || sp.Deleted)
                    continue;

                if (verbose && from != null)
                    // Send a message to the client that the spawner is being saved
                    from.SendMessage(68, "Saving '{0}' in {1} at {2}", sp.Name, sp.Map.Name, sp.Location.ToString());

                // Create a new data row
                DataRow dr = ds.Tables[SpawnTablePointName].NewRow();

                // Populate the data
                dr["Name"] = sp.Name;

                // Set the unqiue id
                dr["UniqueId"] = sp.m_UniqueId;

                // Get the map name
                dr["Map"] = sp.Map.Name;

                // Convert the xml map value to a real map object
                if (string.Compare(sp.Map.Name, Map.Trammel.Name, true) == 0)
                    TrammelCount++;
                else if (string.Compare(sp.Map.Name, Map.Felucca.Name, true) == 0)
                    FeluccaCount++;
                else if (string.Compare(sp.Map.Name, Map.Ilshenar.Name, true) == 0)
                    IlshenarCount++;
                else if (string.Compare(sp.Map.Name, Map.Malas.Name, true) == 0)
                    MalasCount++;
                else if (string.Compare(sp.Map.Name, Map.Tokuno.Name, true) == 0)
                    TokunoCount++;
                else
                    OtherCount++;

                dr["X"] = sp.m_X;
                dr["Y"] = sp.m_Y;
                dr["Width"] = sp.m_Width;
                dr["Height"] = sp.m_Height;

                // check to see if this is in a container
                if (sp.RootParent is Container container)
                {
                    dr["CentreX"] = container.Location.X;
                    dr["CentreY"] = container.Location.Y;
                    dr["CentreZ"] = container.Location.Z;
                    dr["ContainerX"] = sp.Location.X;
                    dr["ContainerY"] = sp.Location.Y;
                    dr["ContainerZ"] = sp.Location.Z;
                    dr["InContainer"] = true;
                }
                else
                {
                    dr["CentreX"] = sp.Location.X;
                    dr["CentreY"] = sp.Location.Y;
                    dr["CentreZ"] = sp.Location.Z;
                    dr["InContainer"] = false;
                }

                dr["Range"] = sp.m_HomeRange;
                dr["MaxCount"] = sp.m_Count;

                // need to deal with the fact that the old xmlspawner xml format only saved delays in minutes as ints, so shorter spawn times
                // are lost
                // flag it then on reading it can be properly handled and still
                // maintain backward compatibility with older xml files
                if ((int) sp.m_MinDelay.TotalSeconds - 60 * (int) sp.m_MinDelay.TotalMinutes > 0 ||
                    (int) sp.m_MaxDelay.TotalSeconds - 60 * (int) sp.m_MaxDelay.TotalMinutes > 0)
                {
                    dr["DelayInSec"] = true;
                    dr["MinDelay"] = (int) sp.m_MinDelay.TotalSeconds;
                    dr["MaxDelay"] = (int) sp.m_MaxDelay.TotalSeconds;
                }
                else
                {
                    dr["DelayInSec"] = false;
                    dr["MinDelay"] = (int) sp.m_MinDelay.TotalMinutes;
                    dr["MaxDelay"] = (int) sp.m_MaxDelay.TotalMinutes;
                }

                // additional parameters
                dr["TODStart"] = sp.m_TODStart.TotalMinutes;
                dr["TODEnd"] = sp.m_TODEnd.TotalMinutes;
                dr["TODMode"] = (int) sp.m_TODMode;
                dr["KillReset"] = sp.m_KillReset;
                dr["MinRefractory"] = sp.m_MinRefractory.TotalMinutes;
                dr["MaxRefractory"] = sp.m_MaxRefractory.TotalMinutes;
                dr["Duration"] = sp.m_Duration.TotalMinutes;
                dr["DespawnTime"] = sp.m_DespawnTime.TotalHours;
                dr["ExternalTriggering"] = sp.m_ExternalTriggering;

                dr["ProximityRange"] = sp.m_ProximityRange;
                dr["ProximityTriggerSound"] = sp.m_ProximityTriggerSound;
                dr["ProximityTriggerMessage"] = sp.m_ProximityTriggerMessage;
                if (sp.m_ObjectPropertyItem != null && !sp.m_ObjectPropertyItem.Deleted)
                    dr["ObjectPropertyItemName"] = $"{sp.m_ObjectPropertyItem.Name},{sp.m_ObjectPropertyItem.GetType().Name}";
                else
                    dr["ObjectPropertyItemName"] = null;
                dr["ObjectPropertyName"] = sp.m_ObjectPropertyName;
                if (sp.m_SetPropertyItem != null && !sp.m_SetPropertyItem.Deleted)
                    dr["SetPropertyItemName"] = $"{sp.m_SetPropertyItem.Name},{sp.m_SetPropertyItem.GetType().Name}";
                else
                    dr["SetPropertyItemName"] = null;
                dr["ItemTriggerName"] = sp.m_ItemTriggerName;
                dr["NoItemTriggerName"] = sp.m_NoItemTriggerName;
                dr["MobTriggerName"] = sp.m_MobTriggerName;
                dr["MobPropertyName"] = sp.m_MobPropertyName;
                dr["PlayerPropertyName"] = sp.m_PlayerPropertyName;
                dr["TriggerProbability"] = sp.m_TriggerProbability;
                dr["SequentialSpawning"] = sp.m_SequentialSpawning;
                dr["RegionName"] = sp.m_RegionName;
                dr["AllowGhostTriggering"] = sp.m_AllowGhostTriggering;
                dr["AllowNPCTriggering"] = sp.m_AllowNPCTriggering;
                dr["SpawnOnTrigger"] = sp.m_SpawnOnTrigger;
                dr["SmartSpawning"] = sp.m_SmartSpawning;
                dr["TickReset"] = sp.m_DisableGlobalAutoReset;

                dr["SpeechTrigger"] = sp.m_SpeechTrigger;
                dr["SkillTrigger"] = sp.m_SkillTrigger;
                dr["Amount"] = sp.m_StackAmount;
                dr["Team"] = sp.m_Team;

                // assign the waypoint based on the waypoint name if it deviates from the default waypoint name, otherwise do it by serial
                string waystr = null;
                if (sp.m_WayPoint != null)
                {
                    if (sp.m_WayPoint.Name != defwaypointname && !string.IsNullOrEmpty(sp.m_WayPoint.Name))
                    {
                        waystr = sp.m_WayPoint.Name;
                    }
                    else
                    {
                        waystr = $"SERIAL,{sp.m_WayPoint.Serial}";
                    }
                }

                dr["WayPoint"] = waystr;

                dr["IsGroup"] = sp.m_Group;
                dr["IsRunning"] = sp.m_Running;
                dr["IsHomeRangeRelative"] = sp.m_HomeRangeIsRelative;
                if (oldformat)
                {
                    dr["Objects"] = sp.GetSerializedObjectList();
                }
                else
                {
                    dr["Objects2"] = sp.GetSerializedObjectList2();
                }

                // Add the row the the table
                ds.Tables[SpawnTablePointName].Rows.Add(dr);

                // Increment the count
                TotalCount++;
            }

            // Write out the file
            bool file_error = false;
            if (TotalCount > 0)
            {
                try
                {
                    ds.WriteXml(stream);
                }
                catch { file_error = true; }

                if (file_error)
                {
                    return false;
                }
            }

            try
            {
                stream.Close();
            }
            catch { }
            // Indicate how many spawners were written
            if (from != null)
            {
                from.SendMessage("{0} spawner(s) were saved to file {1} [Trammel={2}, Felucca={3}, Ilshenar={4}, Malas={5}, Tokuno={6}, Other={7}].",
                    TotalCount, dirname, TrammelCount, FeluccaCount, IlshenarCount, MalasCount, TokunoCount, OtherCount);
            }

            return true;
        }

        private static void WipeSpawners(CommandEventArgs e, bool WipeAll)
        {
            if (e == null || e.Mobile == null)
                return;

            if (e.Mobile.AccessLevel >= AccessLevel.Administrator)
            {
                // Spawner delete criteria (if any)
                string SpawnerPrefix = string.Empty;

                // Check if there is an argument provided (delete criteria)
                if (e.Arguments != null && e.Arguments.Length > 0)
                    SpawnerPrefix = e.Arguments[0];

                if (WipeAll)
                    e.Mobile.SendMessage("Removing ALL XmlSpawner objects from the world{0}.", !string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty);
                else
                    e.Mobile.SendMessage("Removing ALL XmlSpawner objects from {0}{1}.", e.Mobile.Map, !string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty);

                // Delete Xml spawner's in the world based on the mobiles current map
                int Count = 0;
                List<Item> ToDelete = new List<Item>();
                foreach (Item i in World.Items.Values)
                {
                    if (i is XmlSpawner && (WipeAll || i.Map == e.Mobile.Map) && !i.Deleted && (string.IsNullOrEmpty(SpawnerPrefix) || i.Name.StartsWith(SpawnerPrefix)))
                    {
                        ToDelete.Add(i);
                        Count++;
                    }
                }

                for (var index = 0; index < ToDelete.Count; index++)
                {
                    Item i = ToDelete[index];
                    i.Delete();
                }

                if (WipeAll)
                    e.Mobile.SendMessage("Removed {0} XmlSpawner objects from the world.", Count);
                else
                    e.Mobile.SendMessage("Removed {0} XmlSpawner objects from {1}.", Count, e.Mobile.Map);
            }

            else
                e.Mobile.SendMessage("You do not have rights to perform this command.");
        }

        [Usage("XmlSpawnerRespawn [SpawnerPrefixFilter]")]
        [Description("Respawns all XmlSpawner objects from the current map.")]
        public static void Respawn_OnCommand(CommandEventArgs e)
        {
            RespawnSpawners(e, false);
        }

        [Usage("XmlSpawnerRespawnAll [SpawnerPrefixFilter]")]
        [Description("Respawns all XmlSpawner objects from the entire world.")]
        public static void RespawnAll_OnCommand(CommandEventArgs e)
        {
            RespawnSpawners(e, true);
        }

        private static void RespawnSpawners(CommandEventArgs e, bool RespawnAll)
        {
            if (e == null || e.Mobile == null)
                return;

            if (e.Mobile.AccessLevel >= AccessLevel.Administrator)
            {
                // Spawner Respawn criteria (if any)
                string SpawnerPrefix = string.Empty;

                // Check if there is an argument provided (respawn criteria)
                if (e.Arguments != null && e.Arguments.Length > 0)
                    SpawnerPrefix = e.Arguments[0];

                if (RespawnAll)
                    e.Mobile.SendMessage("Respawning ALL XmlSpawner objects from the world{0}.", !string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty);
                else
                    e.Mobile.SendMessage("Respawning ALL XmlSpawner objects from {0}{1}.", e.Mobile.Map, !string.IsNullOrEmpty(SpawnerPrefix) ? " beginning with " + SpawnerPrefix : string.Empty);

                // Respawn Xml spawner's in the world based on the mobiles current map
                int Count = 0;
                List<Item> ToRespawn = new List<Item>();
                foreach (Item i in World.Items.Values)
                {
                    try
                    {
                        if (i is XmlSpawner && (RespawnAll || i.Map == e.Mobile.Map) && !i.Deleted && (string.IsNullOrEmpty(SpawnerPrefix) || i.Name != null && i.Name.StartsWith(SpawnerPrefix)))
                        {
                            // Check if there is a respawn condition
                            ToRespawn.Add(i);
                            Count++;
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error attempting to add {0}, {1}", i, ex.Message); }
                }
                // Respawn the items in the array list
                for (var index = 0; index < ToRespawn.Count; index++)
                {
                    Item i = ToRespawn[index];

                    e.Mobile.SendMessage(33, "Respawning '{0}' in {1} at {2}", i.Name, i.Map.Name, i.Location.ToString());

                    XmlSpawner CheckXmlSpawner = (XmlSpawner) i;
                    CheckXmlSpawner.Respawn();
                }

                if (RespawnAll)
                    e.Mobile.SendMessage("Respawned {0} XmlSpawner objects from the world.", Count);
                else
                    e.Mobile.SendMessage("Respawned {0} XmlSpawner objects from {1}.", Count, e.Mobile.Map);
            }

            else
                e.Mobile.SendMessage("You do not have rights to perform this command.");
        }
        #endregion

        #region Constructors

        [Constructable]
        public XmlSpawner()
            : base(BaseItemId)
        {
            m_UniqueId = Guid.NewGuid().ToString();
            SpawnRange = defSpawnRange;

            InitSpawn(0, 0, m_Width, m_Height, string.Empty, 0, defMinDelay, defMaxDelay, defDuration,
                defProximityRange, defProximityTriggerSound, defAmount, defTeam, defHomeRange, defRelativeHome, Array.Empty<SpawnObject>(), defMinRefractory, defMaxRefractory,
                defTODStart, defTODEnd, null, null, null, null, null, null, null, null, null, defTriggerProbability, null, defIsGroup, defTODMode,
                defKillReset, false, -1, null, false, false, false, null, defDespawnTime, null, false, null);
        }

        [Constructable]
        public XmlSpawner(int amount, int minDelay, int maxDelay, int team, int homeRange, string creatureName)
            : base(BaseItemId)
        {
            m_UniqueId = Guid.NewGuid().ToString();
            SpawnRange = homeRange;
            SpawnObject[] so = new SpawnObject[1];
            so[0] = new SpawnObject(creatureName, amount);

            InitSpawn(0, 0, m_Width, m_Height, string.Empty, amount, TimeSpan.FromMinutes(minDelay), TimeSpan.FromMinutes(maxDelay), defDuration,
                defProximityRange, defProximityTriggerSound, defAmount, team, homeRange, defRelativeHome, so, defMinRefractory, defMaxRefractory,
                defTODStart, defTODEnd, null, null, null, null, null, null, null, null, null, defTriggerProbability, null, defIsGroup, defTODMode,
                defKillReset, false, -1, null, false, false, false, null, defDespawnTime, null, false, null);
        }

        [Constructable]
        public XmlSpawner(int amount, int minDelay, int maxDelay, int team, int homeRange, int spawnRange, string creatureName)
            : base(BaseItemId)
        {
            m_UniqueId = Guid.NewGuid().ToString();
            SpawnRange = spawnRange;
            SpawnObject[] so = new SpawnObject[1];
            so[0] = new SpawnObject(creatureName, amount);

            InitSpawn(0, 0, m_Width, m_Height, string.Empty, amount, TimeSpan.FromMinutes(minDelay), TimeSpan.FromMinutes(maxDelay), defDuration,
                defProximityRange, defProximityTriggerSound, defAmount, team, homeRange, defRelativeHome, so, defMinRefractory, defMaxRefractory,
                defTODStart, defTODEnd, null, null, null, null, null, null, null, null, null, defTriggerProbability, null, defIsGroup, defTODMode,
                defKillReset, false, -1, null, false, false, false, null, defDespawnTime, null, false, null);
        }

        [Constructable]
        public XmlSpawner(string creatureName)
            : base(BaseItemId)
        {
            m_UniqueId = Guid.NewGuid().ToString();
            SpawnObject[] so = new SpawnObject[1];
            so[0] = new SpawnObject(creatureName, 1);
            SpawnRange = defSpawnRange;

            InitSpawn(0, 0, m_Width, m_Height, string.Empty, 1, defMinDelay, defMaxDelay, defDuration,
                defProximityRange, defProximityTriggerSound, defAmount, defTeam, defHomeRange, defRelativeHome, so, defMinRefractory, defMaxRefractory,
                defTODStart, defTODEnd, null, null, null, null, null, null, null, null, null, defTriggerProbability, null, defIsGroup, defTODMode,
                defKillReset, false, -1, null, false, false, false, null, defDespawnTime, null, false, null);
        }

        public XmlSpawner(Guid uniqueId, int x, int y, int width, int height, string name, int maxCount, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan duration,
            int proximityRange, int proximityTriggerSound, int amount, int team, int homeRange, bool isRelativeHomeRange, SpawnObject[] spawnObjects,
            TimeSpan minRefractory, TimeSpan maxRefractory, TimeSpan todstart, TimeSpan todend, Item objectPropertyItem, string objectPropertyName, string proximityMessage,
            string itemTriggerName, string noitemTriggerName, string speechTrigger, string mobTriggerName, string mobPropertyName, string playerPropertyName, double triggerProbability,
            Item setPropertyItem, bool isGroup, TODModeType todMode, int killReset, bool externalTriggering, int sequentialSpawning, string regionName,
            bool allowghost, bool allownpc, bool spawnontrigger, string configfile, TimeSpan despawnTime, string skillTrigger, bool smartSpawning, WayPoint wayPoint)
            : base(BaseItemId)
        {
            m_UniqueId = uniqueId.ToString();
            InitSpawn(x, y, width, height, name, maxCount, minDelay, maxDelay, duration,
                proximityRange, proximityTriggerSound, amount, team, homeRange, isRelativeHomeRange, spawnObjects, minRefractory, maxRefractory, todstart, todend,
                objectPropertyItem, objectPropertyName, proximityMessage, itemTriggerName, noitemTriggerName, speechTrigger, mobTriggerName, mobPropertyName, playerPropertyName,
                triggerProbability, setPropertyItem, isGroup, todMode, killReset, externalTriggering, sequentialSpawning, regionName, allowghost, allownpc, spawnontrigger, configfile,
                despawnTime, skillTrigger, smartSpawning, wayPoint);
        }

        public void InitSpawn(int x, int y, int width, int height, string name, int maxCount, TimeSpan minDelay, TimeSpan maxDelay, TimeSpan duration,
            int proximityRange, int proximityTriggerSound, int amount, int team, int homeRange, bool isRelativeHomeRange, SpawnObject[] objectsToSpawn,
            TimeSpan minRefractory, TimeSpan maxRefractory, TimeSpan todstart, TimeSpan todend, Item objectPropertyItem, string objectPropertyName, string proximityMessage,
            string itemTriggerName, string noitemTriggerName, string speechTrigger, string mobTriggerName, string mobPropertyName, string playerPropertyName, double triggerProbability,
            Item setPropertyItem, bool isGroup, TODModeType todMode, int killReset, bool externalTriggering, int sequentialSpawning, string regionName, bool allowghost, bool allownpc, bool spawnontrigger,
            string configfile, TimeSpan despawnTime, string skillTrigger, bool smartSpawning, WayPoint wayPoint)
        {

            Visible = false;
            Movable = false;
            m_X = x;
            m_Y = y;
            m_Width = width;
            m_Height = height;

            // init spawn range if compatible
            if (width == height)
                m_SpawnRange = width / 2;
            else
                m_SpawnRange = -1;
            m_Running = true;
            m_Group = isGroup;

            Name = !string.IsNullOrEmpty(name) ? name : "Spawner";

            m_MinDelay = minDelay;
            m_MaxDelay = maxDelay;

            // duration and proximity range parameter
            m_MinRefractory = minRefractory;
            m_MaxRefractory = maxRefractory;
            m_TODStart = todstart;
            m_TODEnd = todend;
            m_TODMode = todMode;
            m_KillReset = killReset;
            m_Duration = duration;
            m_DespawnTime = despawnTime;
            m_ProximityRange = proximityRange;
            m_ProximityTriggerSound = proximityTriggerSound;
            m_proximityActivated = false;
            m_durActivated = false;
            m_refractActivated = false;
            m_Count = maxCount;
            m_Team = team;
            m_StackAmount = amount;
            m_HomeRange = homeRange;
            m_HomeRangeIsRelative = isRelativeHomeRange;
            m_ObjectPropertyItem = objectPropertyItem;
            m_ObjectPropertyName = objectPropertyName;
            m_ProximityTriggerMessage = proximityMessage;
            m_ItemTriggerName = itemTriggerName;
            m_NoItemTriggerName = noitemTriggerName;
            m_SpeechTrigger = speechTrigger;
            SkillTrigger = skillTrigger;        // note this will register the skill as well
            m_MobTriggerName = mobTriggerName;
            m_MobPropertyName = mobPropertyName;
            m_PlayerPropertyName = playerPropertyName;
            m_TriggerProbability = triggerProbability;
            m_SetPropertyItem = setPropertyItem;
            m_ExternalTriggering = externalTriggering;
            m_ExternalTrigger = false;
            m_SequentialSpawning = sequentialSpawning;
            RegionName = regionName;
            m_AllowGhostTriggering = allowghost;
            m_AllowNPCTriggering = allownpc;
            m_SpawnOnTrigger = spawnontrigger;
            m_SmartSpawning = smartSpawning;
            m_WayPoint = wayPoint;

            // Create the array of spawned objects
            m_SpawnObjects = new List<SpawnObject>();

            // Assign the list of objects to spawn
            SpawnObjects = objectsToSpawn;

            // Kick off the process
            DoTimer(TimeSpan.FromSeconds(1));
        }

        public XmlSpawner(Serial serial)
            : base(serial)
        {
        }

        #endregion

        #region Defrag methods

        public void Defrag(bool killtest)
        {
            if (m_SpawnObjects == null)
            {
                return;
            }

            bool removed = false;
            int total_removed = 0;

            List<Item> deleteilist = new List<Item>();
            List<Mobile> deletemlist = new List<Mobile>();

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int x = 0; x < so.SpawnedObjects.Count; x++)
                {
                    object o = so.SpawnedObjects[x];

                    if (o is Item item)
                    {
                        bool despawned = false;
                        // check to see if the despawn time has elapsed.  If so, then delete it if it hasnt been picked up or stolen.
                        if (DespawnTime.TotalHours > 0 && !item.Deleted &&
                            item.LastMoved < DateTime.UtcNow - DespawnTime && item.Parent == Parent &&
                            (!ItemFlags.GetTaken(item) || item.Parent != null && item.Parent == Parent))
                        {
                            //item.Delete();
                            deleteilist.Add(item);
                            despawned = true;
                        }

                        // Check if the items has been deleted or
                        // if something else now owns the item (picked it up for example)
                        // also check the stolen/placed in container flag.  If any of those are true then the spawner doesnt own it any more so take it off the list.
                        // the stolen/container flag prevents spawns from being left on the list when players take them and lock them back down on the ground.
                        // If you have made the changes to stealing.cs and container.cs described in xmlspawner2.txt then just uncomment the line below to
                        // enable this check
                        if (item.Deleted || despawned || item.Parent != Parent // different container
                            || ItemFlags.GetTaken(item) && (item.Parent == null || (item.Parent != Parent))
                        ) // taken and in the world, or a different container
                        {
                            so.SpawnedObjects.Remove(item);
                            x--;
                            removed = true;
                            // if sequential spawning is active and the RestrictKillsToSubgroup flag is set, then check to see if
                            // the object is in the current subgroup before adding to the total
                            if (SequentialSpawn >= 0 && so.RestrictKillsToSubgroup)
                            {
                                if (so.SubGroup == SequentialSpawn)
                                    total_removed++;
                            }
                            else
                            {
                                // just add it
                                total_removed++;
                            }
                        }
                    }
                    else if (o is Mobile m)
                    {
                        bool despawned = false;
                        // check to see if the despawn time has elapsed.  If so, and the sector is not active then delete it.
                        if (DespawnTime.TotalHours > 0 && !m.Deleted &&
                            m.CreationTime < DateTime.UtcNow - DespawnTime && m.Map != null && m.Map != Map.Internal &&
                            !m.Map.GetSector(m.Location).Active)
                        {
                            deletemlist.Add(m);
                            despawned = true;
                        }

                        if (m.Deleted || despawned && (m is BaseCreature bc &&
                                                       (bc.Controlled || bc.IsStabled ||
                                                        bc.Owners != null && bc.Owners.Count > 0)))
                        {
                            // Remove the delete mobile from the list
                            so.SpawnedObjects.Remove(m);
                            x--;
                            removed = true;
                            // if sequential spawning is active and the RestrictKillsToSubgroup flag is set, then check to see if
                            // the object is in the current subgroup before adding to the total
                            if (SequentialSpawn >= 0 && so.RestrictKillsToSubgroup)
                            {
                                if (so.SubGroup == SequentialSpawn)
                                    total_removed++;
                            }
                            else
                            {
                                total_removed++; // just add it
                            }
                        }
                    }
                    else if (o is BaseXmlSpawner.KeywordTag tag)
                    {
                        if (tag.Deleted)
                        {
                            so.SpawnedObjects.Remove(o);
                            x--;
                            removed = true;
                        }
                    }
                    else
                    {
                        // Don't know what this is, so remove it
                        Console.WriteLine("removing unknown {0} from spawnlist", so);
                        so.SpawnedObjects.Remove(o);
                        x--;
                        removed = true;
                    }
                }
            }

            DeleteFromList(deleteilist, deletemlist);

            // Check if anything has been removed
            if (removed)
                InvalidateProperties();

            // increment the killcount based upon the number of items that were removed from the spawnlist (i.e. were spawned but now are gone, presumed killed)
            if (killtest)
                m_killcount += total_removed;
        }

        public void ClearGOTOTags() // special defrag pass to remove GOTO keyword tags
        {
            if (m_SpawnObjects == null) return;

            List<BaseXmlSpawner.KeywordTag> ToDelete = new List<BaseXmlSpawner.KeywordTag>();
            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int x = 0; x < so.SpawnedObjects.Count; x++)
                {
                    object o = so.SpawnedObjects[x];

                    if (o is BaseXmlSpawner.KeywordTag sot && sot.Type == 2) // clear the tags except for gump and delay tags
                    {
                        ToDelete.Add(sot);
                        so.SpawnedObjects.Remove(o);
                        x--;
                    }
                }
            }

            for (int x = ToDelete.Count - 1; x >= 0; --x) // BaseXmlSpawner.KeywordTag i in ToDelete)
            {
                BaseXmlSpawner.KeywordTag i = ToDelete[x];
                if (i != null && !i.Deleted)
                {
                    i.Delete();
                }
            }
        }
        #endregion

        #region SequentialSpawning methods

        private int SubGroupCount(int sgroup)
        {
            if (m_SpawnObjects == null)
                return 0;

            int nsub = 0;
            for (int i = 0; i < m_SpawnObjects.Count; i++)
            {
                SpawnObject s = m_SpawnObjects[i];

                if (s.SubGroup == sgroup)
                    nsub++;
            }

            return nsub;
        }

        private int RandomAvailableSpawnIndex()
        {
            return RandomAvailableSpawnIndex(-1); // get spawn indices randomly from all available spawns independent of group
        }

        private int RandomAvailableSpawnIndex(int sgroup) // get spawn indices randomly from all available spawns of a group
        {
            if (m_SpawnObjects == null)
                return -1;

            int maxrange = 0;
            List<int> sgrouplist = null;
            int totalcount = 0;
            // make a pass to determine which subgroups are available for spawning
            // by finding any subgroups that do not have available spawns
            for (int i = 0; i < m_SpawnObjects.Count; i++)
            {
                SpawnObject s = m_SpawnObjects[i];
                if (s.SubGroup > 0 && (s.Ignore || s.Disabled)) continue;

                totalcount += s.SpawnedObjects.Count;
                if (s.SubGroup > 0 && s.SpawnedObjects.Count >= s.MaxCount)
                {
                    // this subgroup is not available so add it to the list
                    if (sgrouplist == null)
                    {
                        sgrouplist = new List<int>();
                    }
                    sgrouplist.Add(s.SubGroup);
                }
            }

            for (int i = 0; i < m_SpawnObjects.Count; i++)
            {
                SpawnObject s = m_SpawnObjects[i];

                if (s.SubGroup > 0 && (s.Ignore || s.Disabled)) continue;

                if (s.MaxCount > s.SpawnedObjects.Count && (sgroup < 0 || sgroup == s.SubGroup)
                                                        && (sgrouplist == null || !sgrouplist.Contains(s.SubGroup)) && (s.SubGroup <= 0 || SubGroupCount(s.SubGroup) + totalcount <= MaxCount))
                {
                    // keep track of the number of spawn objects that are not at max (hence available for spawning)
                    // this will be used to compute the probabilistic weighting function based on the relative
                    // maxcounts of each entry
                    maxrange += s.MaxCount;
                    s.Available = true;
                }
                else
                {
                    s.Available = false;
                }
            }
            // now generate a random number over the available spawnobjects
            // but only if the entire subgroup is available for spawning
            // note, subgroup zero is exempt from this check.
            if (maxrange > 0)
            {
                int randindex = Utility.Random(maxrange);

                // and map it into the avail spawns
                int currentrange = 0;
                for (int i = 0; i < m_SpawnObjects.Count; i++)
                {
                    SpawnObject s = m_SpawnObjects[i];
                    if (s.SubGroup > 0 && (s.Ignore || s.Disabled)) continue;

                    // keep track of the number of spawn objects that are not at max (hence available for spawning)
                    if (s.Available)
                    {
                        // check to see if the random value maps into the range of the current index
                        if (randindex >= currentrange && randindex < currentrange + s.MaxCount)
                        {
                            return i;
                        }

                        currentrange += s.MaxCount;
                    }
                }

                return -1; // should never get here
            }

            return -1; // no spawns are available
        }

        // get spawn indices randomly from all available spawns of a group
        private int RandomSpawnIndex(int sgroup)
        {
            if (m_SpawnObjects == null)
                return -1;

            int avail = 0;
            int maxrange = 0;
            for (int i = 0; i < m_SpawnObjects.Count; i++)
            {
                SpawnObject s = m_SpawnObjects[i];

                // keep track of the number of spawn objects that are not at max (hence available for spawning)
                if (sgroup < 0 || (sgroup == s.SubGroup))
                {
                    avail++;
                    maxrange += s.MaxCount;
                }
            }
            // now generate a random number over the available spawnobjects
            if (avail > 0 && maxrange > 0)
            {
                int randindex = Utility.Random(maxrange);

                // and map it into the avail spawns
                int currentrange = 0;

                for (int i = 0; i < m_SpawnObjects.Count; i++)
                {
                    SpawnObject s = m_SpawnObjects[i];

                    // keep track of the number of spawn objects that are not at max (hence available for spawning)
                    if (sgroup < 0 || (sgroup == s.SubGroup))
                    {
                        if (randindex >= currentrange && randindex < currentrange + s.MaxCount)
                            return (i);

                        currentrange += s.MaxCount;
                    }
                }

                return -1; // should never get here
            }

            return -1; // no spawns are available
        }

        // return the next subgroup in the sequence.
        public int NextSequentialIndex(int sgroup)
        {
            while (true)
            {
                if (m_SpawnObjects == null || m_SpawnObjects.Count == 0) return 0;

                int finddirection = 1;
                int largergroup = -1;

                //find the next subgroup that is greater than the current one
                for (int j = 0; j < m_SpawnObjects.Count; j++)
                {
                    SpawnObject s = m_SpawnObjects[j];
                    if (s.SubGroup > 0 && (s.Ignore || s.Disabled)) continue;

                    int thisgroup = s.SubGroup;

                    // start off by finding a subgroup that is larger
                    if (finddirection == 1)
                    {
                        if (thisgroup > sgroup)
                        {
                            largergroup = thisgroup;

                            // then work backward to find the group that is less than this but still larger than the current
                            finddirection = -1;
                        }
                    }
                    else
                    {
                        if (thisgroup > sgroup && thisgroup < largergroup)
                        {
                            largergroup = thisgroup;

                            finddirection = -1;
                        }
                    }
                }

                if (largergroup < 0 && sgroup >= 0) // if couldnt find one larger, then it is time to wraparound
                {
                    sgroup = -1;
                    continue;
                }

                return largergroup;
            }
        }

        public int GetCurrentAvailableSequentialSpawnIndex(int sgroup) // returns the spawn index of a spawn entry in the current sequential subgroup
        {
            if (sgroup < 0)
                return -1;

            if (m_SpawnObjects == null)
                return -1;

            if (sgroup == 0)
                return (RandomAvailableSpawnIndex(0));

            for (int j = 0; j < m_SpawnObjects.Count; j++) //return the first instance of a spawn object that is an available member of the requested subgroup
            {
                SpawnObject s = m_SpawnObjects[j];

                if (s.SubGroup == sgroup && s.MaxCount > s.SpawnedObjects.Count)
                {
                    return j;
                }
            }
            
            return -1; // failed to find any spawn entry of the requested subgroup
        }

        public int GetCurrentSequentialSpawnIndex(int sgroup) // returns the spawn index of a spawn entry in the current sequential subgroup
        {
            if (sgroup < 0)
                return -1;

            if (m_SpawnObjects == null)
                return -1;

            if (sgroup == 0)
                return (RandomSpawnIndex(0));

            for (int j = 0; j < m_SpawnObjects.Count; j++) //return the first instance of a spawn object that is an available member of the requested subgroup
            {
                if (m_SpawnObjects[j].SubGroup == sgroup)
                {
                    return j;
                }
            }
            
            return -1; // failed to find any spawn entry of the requested subgroup
        }

        private void SeqResetTo(int sgroup)
        {
            // check the SequentialResetTo on the subgroup
            // cant do resets on subgroup 0
            if (sgroup == 0) return;

            // this will get the index of the first spawn entry in the subgroup
            // it will have the subgroup timer settings
            int spawnindex = GetCurrentSequentialSpawnIndex(sgroup);

            if (spawnindex >= 0)
            {
                // if it is greater than zero then initiate reset
                SpawnObject s = m_SpawnObjects[spawnindex];
                m_SequentialSpawning = s.SequentialResetTo;

                InitiateSequentialReset(sgroup);

                // clear the spawns
                //RemoveSpawnObjects();
                ClearSubgroup(s.SubGroup);

                // and reset the kill count
                KillCount = 0;
            }
        }

        private bool CheckForSequentialReset()
        {
            // check the SequentialResetTime on the subgroup
            // cant do resets on subgroup 0
            if (m_SequentialSpawning == 0) return false;

            // this will get the index of the first spawn entry in the subgroup
            // it will have the subgroup timer settings
            int spawnindex = GetCurrentSequentialSpawnIndex(m_SequentialSpawning);

            if (spawnindex >= 0)
            {
                // check the reset time on it
                SpawnObject s = m_SpawnObjects[spawnindex];
                // if it is greater than zero then resetting is possible
                if (s.SequentialResetTime > 0)
                {
                    // so check the reset timer
                    if (NextSeqReset <= TimeSpan.Zero)
                    {
                        // it has expired so time to reset
                        return true;
                    }
                }
            }
            return false;
        }

        private void InitiateSequentialReset(int sgroup)
        {
            // check the SequentialResetTime on the subgroup
            // cant do resets on subgroup 0
            if (sgroup == 0) return;

            // this will get the index of the first spawn entry in the subgroup
            // it will have the subgroup timer settings
            int spawnindex = GetCurrentSequentialSpawnIndex(sgroup);

            if (spawnindex >= 0)
            {
                // if it is greater than zero then initiate reset
                SpawnObject s = m_SpawnObjects[spawnindex];
                NextSeqReset = TimeSpan.FromMinutes(s.SequentialResetTime);
            }
        }

        public void ResetSequential()
        {
            // go back to the lowest level
            if (m_SequentialSpawning >= 0)
            {
                m_SequentialSpawning = NextSequentialIndex(-1);
            }

            // reset the nextspawn times
            ResetNextSpawnTimes();

            // and reset the kill count
            KillCount = 0;
        }

        public bool AdvanceSequential()
        {
            // check for a sequence hold

            if (HoldSequence) return false;

            // check for triggering
            if (!((m_proximityActivated || CanFreeSpawn) && TODInRange)) return false;

            // if kills needed is greater than zero then check the killcount as well
            int spawnindex = GetCurrentSequentialSpawnIndex(m_SequentialSpawning);

            int killsneeded = 0;
            int subgroup = -1;
            bool clearedobjects = false;

            if (spawnindex >= 0)
            {
                SpawnObject s = m_SpawnObjects[spawnindex];
                subgroup = s.SubGroup;
                killsneeded = s.KillsNeeded;
            }

            // advance the sequential spawn index if it is enabled and kills needed have been satisfied
            if (m_SequentialSpawning >= 0 && (killsneeded == 0 || KillCount >= killsneeded))
            {
                m_SequentialSpawning = NextSequentialIndex(m_SequentialSpawning);

                // set the sequential reset based on the current sequence state
                // this will be checked in the spawner OnTick to determine whether to Reset the sequential state
                InitiateSequentialReset(m_SequentialSpawning);

                // clear the spawns if there is a killcount on the level
                if (killsneeded >= 0)
                {
                    ClearSubgroup(subgroup);
                    clearedobjects = true;
                }

                // and reset the kill count
                KillCount = 0;
            }

            // returning true will indicate that all spawns have been cleared and therefore a new spawn can be initiated in the same OnTick
            return clearedobjects;
        }
        #endregion

        #region Spawn methods

        int killcount_held;

        public void OnTick()
        {
            _TraceStart(8);
            // start up the timer again for the next Ontick
            DoTimer();

            // reset the protection against runaway looping
            ClearSpawnedThisTick = true;

            // if regional spawning is enabled, update the region in case new regions were added after the initialization pass
            CheckRegionAssignment = true;

            // reset the killcount whenever a spawntick goes by in which it could have spawned, ie the spawner is full, or proximity triggered
            // spawns were not activated.  Note that killcount gets incremented within Defrag whenever a spawn is that had been generated is removed from the active list.
            // Check the count before and then after the spawn passes.
            // if the spawner is still refractory then dont do a reset of the killcount.
            //int startcount = this.m_killcount;
            int startcount = killcount_held;
            if (!m_skipped)
            {
                killcount_held = m_killcount;
            }

            // killcount will be updated in Defrag
            Defrag(true);

            if (!m_DisableGlobalAutoReset && startcount == m_killcount && !m_refractActivated && !m_skipped)
            {
                m_spawncheck--;
            }
            m_skipped = false;

            // allow for some slack in the killcount reset by resetting after a certain number of spawn ticks without kills pass
            if (m_spawncheck <= 0)
            {
                m_killcount = 0;
                m_spawncheck = m_KillReset;     // wait for 1 spawn ticks to pass before resetting.  This can be set to anything you like
            }

            // check for smart spawning
            if (SmartSpawning && IsFull && !HasActiveSectors && !HasDamagedOrDistantSpawns /*&& !HasHoldSmartSpawning */ )
            {
                IsInactivated = true;
                SmartRemoveSpawnObjects();

            }

            // dont process spawn ticks while inactivated if smart spawning is enabled
            if (SmartSpawning && IsInactivated)
            {
                _TraceEnd(8);
                return;
            }

            IsInactivated = false;

            // check to see if spawning is on hold due to a WAIT keyword
            if (!OnHold)
            {
                // look for  triggers that are not player activated.
                if (m_ProximityRange == -1 && CanSpawn)
                {
                    CheckTriggers(null, null, false);
                }

                // check for proximity triggers without movement activation
                if (m_ProximityRange >= 0 && CanSpawn)
                {
                    // check all nearby players
                    IPooledEnumerable eable = GetMobilesInRange(m_ProximityRange);
                    foreach (Mobile p in eable)
                    {
                        if (ValidPlayerTrig(p))
                            CheckTriggers(p, null, true);
                    }

                    eable.Free();
                }

                if (m_Group)
                {
                    // check the seq reset time on the current subgroup
                    // if the reset time is greater than zero then check the timer
                    // if it has expired then reset the sequential subgroup
                    // only do this if it can actually spawn
                    if (CheckForSequentialReset())
                    {
                        // it has expired so reset the sequential spawn level
                        SeqResetTo(m_SequentialSpawning);

                        bool triedtospawn = Respawn();

                        if (triedtospawn) ClearGOTOTags();

                        // dont advance if the spawn isnt triggered after resetting
                        if (!triedtospawn)
                            HoldSequence = true;
                    }
                    else if (TotalSpawnedObjects <= 0)
                    {
                        // advance the sequential spawn index if it is enabled
                        AdvanceSequential();

                        bool triedtospawn = Respawn();

                        if (triedtospawn)
                            ClearGOTOTags();
                    }
                }
                else
                {
                    if (CheckForSequentialReset())
                    {
                        // it has expired so reset the sequential spawn level
                        SeqResetTo(m_SequentialSpawning);

                        // dont advance if the spawn isnt triggered after resetting
                        HoldSequence = true;
                    }
                    else
                    {
                        // advance the sequence before spawning
                        AdvanceSequential();
                    }

                    // try to spawn.  If spawning conditions such as triggering or TOD are not met, then it returns false
                    bool triedtospawn = Spawn(false, 0);

                    if (triedtospawn)
                        ClearGOTOTags();
                    // this will maintain any sequential holds if spawning was suppressed due to triggering

                    if (!FreeRun)
                    {
                        m_mob_who_triggered = null;
                    }
                }

                // and clear triggering flags
                if (!OnHold && !FreeRun)
                {
                    m_proximityActivated = false;
                }
            }

            if (FreeRun && SpawnOnTrigger && m_proximityActivated)
            {
                // if it is in free run and was triggered, then just keep spawning as though it was triggered immediately
                NextSpawn = TimeSpan.Zero;
                ResetNextSpawnTimes();
            }

            // if it is out of the TOD range then delete the spawns
            if (!TODInRange)
            {
                RemoveSpawnObjects();

                ResetAllFlags();
            }

            _TraceEnd(8);
        }

        public bool ClearSpawnedThisTick
        {
            set
            {
                if (m_SpawnObjects == null || value == false) return;

                for (int i = 0; i < m_SpawnObjects.Count; i++)
                {
                    SpawnObject sobj = m_SpawnObjects[i];
                    if (sobj != null)
                    {
                        sobj.SpawnedThisTick = false;
                    }
                }
            }
        }

        // select and spawn something
        // return false if it cannot spawn, e.g. there is nothing to spawn or it is a triggerable spawner and has not been triggered
        public bool Spawn(bool smartspawn, byte loops)
        {
            if (m_SpawnObjects != null && m_SpawnObjects.Count > 0 && (m_proximityActivated || CanFreeSpawn) && TODInRange)
            {
                m_HoldSequence = false;

                // if the spawner is full then dont bother
                if (IsFull)
                {
                    ResetProximityActivated();
                    return true;
                }

                // Pick a spawn object to spawn
                int SpawnIndex;

                // see if sequential spawning has been selected
                SpawnIndex = m_SequentialSpawning >= 0 ? GetCurrentAvailableSequentialSpawnIndex(m_SequentialSpawning) : RandomAvailableSpawnIndex();

                // no spawns are available so no point in continuing
                if (SpawnIndex < 0)
                {
                    ResetProximityActivated();
                    return true;
                }

                SpawnObject sobj = m_SpawnObjects[SpawnIndex];
                int sgroup = sobj.SubGroup;

                // if this is part of a non-zero group, then spawn all of the group members as well
                if (sgroup != 0)
                {
                    SpawnSubGroup(sgroup, smartspawn, loops);
                }
                else
                {
                    // Found a valid spawn object so spawn it and see if it successful
                    if (Spawn(SpawnIndex, smartspawn, sobj.SpawnsPerTick, loops))
                    {
                        if (!smartspawn)
                            RefreshNextSpawnTime(sobj);
                    }
                }

                ResetProximityActivated();
                return true;
            }

            ResetProximityActivated();
            return false;
        }

        // spawn an individual entry by index up to count times
        public bool Spawn(int index, bool smartspawn, int count, int packrange, Point3D packcoord, bool ignoreloopprotection, byte loops)
        {
            if (m_SpawnObjects == null || index >= m_SpawnObjects.Count) return false;

            bool didspawn = false;

            SpawnObject so = m_SpawnObjects[index];

            if (so == null) return false;

            Defrag(false);

            // make sure you dont go over the individual entry maxcount
            int somax = so.MaxCount;
            int socnt = so.SpawnedObjects.Count;
            int nspawn = so.SpawnsPerTick;
            int scnt = SafeCurrentCount;

            for (int k = 0; k < nspawn && k + socnt < somax && k + scnt < MaxCount; k++)
            {
                if (packrange >= 0 && so.SubGroup > 0 && packcoord == Point3D.Zero)
                {
                    packcoord = GetPackCoord(so.SubGroup);
                }
                if (Spawn(index, smartspawn, packrange, packcoord, ignoreloopprotection, loops))
                {
                    // if any of the attempts were successful then flag it as having spawned
                    didspawn = true;
                }
            }

            return didspawn;
        }

        // spawn an individual entry by index up to count times
        public bool Spawn(int index, bool smartspawn, int count, byte loops)
        {
            return Spawn(index, smartspawn, count, false, loops);
        }

        // spawn an individual entry by index up to count times
        public bool Spawn(int index, bool smartspawn, int count, bool ignoreloopprotection, byte loops)
        {
            return Spawn(index, smartspawn, count, -1, Point3D.Zero, ignoreloopprotection, loops);
        }

        // spawn an individual entry by spawn object
        public void Spawn(string SpawnObjectTypeName, bool smartspawn, int packrange, Point3D packcoord, byte loops)
        {
            if (m_SpawnObjects == null) return;
            for (int i = 0; i < m_SpawnObjects.Count; i++)
            {
                if (m_SpawnObjects[i].TypeName.ToUpper() == SpawnObjectTypeName.ToUpper())
                {

                    if (Spawn(i, smartspawn, packrange, packcoord, loops))
                    {
                        RefreshNextSpawnTime(m_SpawnObjects[i]);
                    }
                    break;
                }
            }
        }

        // spawn an individual entry by index
        public void Spawn(string SpawnObjectTypeName, bool smartspawn, byte loops)
        {
            Spawn(SpawnObjectTypeName, smartspawn, -1, Point3D.Zero, loops);
        }

        // spawn an individual entry by index
        public bool Spawn(int index, bool smartspawn, int packrange, Point3D packcoord, byte loops)
        {
            return Spawn(index, smartspawn, packrange, packcoord, false, loops);
        }

        // spawn an individual entry by index
        public bool Spawn(int index, bool smartspawn, int packrange, Point3D packcoord, bool ignoreloopprotection, byte loops)
        {
            Map map = Map;

            // Make sure everything is ok to spawn an object
            if (map == null || map == Map.Internal || m_SpawnObjects == null || m_SpawnObjects.Count == 0 || index < 0 || index >= m_SpawnObjects.Count)
                return false;

            // Remove any spawns that don't belong to the spawner any more.
            Defrag(false);

            // Get the spawn object at the required index
            SpawnObject TheSpawn = m_SpawnObjects[index];

            // Check if the object retrieved is a valid SpawnObject
            if (TheSpawn != null)
            {
                // dont allow an entry to be spawned more than once per tick
                // this protects against runaway recursive looping
                if (TheSpawn.SpawnedThisTick && !ignoreloopprotection)
                    return false;

                // check the nextspawn time to see if it is available
                if (TheSpawn.NextSpawn > DateTime.UtcNow)
                    return false;

                int CurrentCreatureMax = TheSpawn.MaxCount;
                int CurrentCreatureCount = TheSpawn.SpawnedObjects.Count;

                // Check that the current object to be spawned has not reached its maximum allowed
                // and make sure that the maximum spawner count has not been exceeded as well
                if (CurrentCreatureCount >= CurrentCreatureMax || TotalSpawnedObjects >= m_Count)
                {
                    return false;
                }

                // check for string substitions
                string substitutedtypeName = BaseXmlSpawner.ApplySubstitution(this, this, TheSpawn.TypeName);

                // random positioning is the default
                List<SpawnPositionInfo> spawnpositioning = null;

                // require valid surfaces by default
                bool requiresurface = true;

                // parse the # function specification for the entry
                while (substitutedtypeName.StartsWith("#"))
                {
                    string[] args = BaseXmlSpawner.ParseSemicolonArgs(substitutedtypeName, 2);

                    if (args.Length > 0)
                    {
                        if (spawnpositioning == null)
                        {
                            spawnpositioning = new List<SpawnPositionInfo>();
                        }
                        // parse any comma args
                        string[] keyvalueargs = BaseXmlSpawner.ParseCommaArgs(args[0], 10);

                        if (keyvalueargs.Length > 0)
                        {

                            switch (keyvalueargs[0])
                            {
                                case "#NOITEMID":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.NoItemID, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#ITEMID":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.ItemID, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#NOTILES":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.NoTiles, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#TILES":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.Tiles, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#WET":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.Wet, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#XFILL":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.RowFill, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#YFILL":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.ColFill, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#EDGE":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.Perimeter, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#PLAYER":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.Player, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#WAYPOINT":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.Waypoint, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#RELXY":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.RelXY, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#DXY":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.DeltaLocation, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#XY":
                                    spawnpositioning.Add(new SpawnPositionInfo(SpawnPositionType.Location, m_mob_who_triggered, keyvalueargs));
                                    break;
                                case "#CONDITION":
                                    // test the specified condition string
                                    // syntax is #CONDITION,proptest
                                    // reparse with only one arg after the comma, this allows property tests that use commas as well
                                    string[] ckeyvalueargs = BaseXmlSpawner.ParseCommaArgs(args[0], 2);
                                    if (ckeyvalueargs.Length > 1)
                                    {
                                        // dont spawn if it fails the test
                                        if (!BaseXmlSpawner.CheckPropertyString(this, this, ckeyvalueargs[1], out status_str))
                                            return false;
                                    }
                                    else
                                    {
                                        status_str = "invalid #CONDITION specification: " + args[0];
                                    }
                                    break;
                                default:
                                    status_str = "invalid # specification: " + args[0];
                                    break;
                            }
                        }
                    }

                    // get the rest of the spawn entry
                    substitutedtypeName = args.Length > 1 ? args[1].Trim() : string.Empty;
                }

                if (substitutedtypeName.StartsWith("*"))
                {
                    requiresurface = false;
                    substitutedtypeName = substitutedtypeName.TrimStart('*');
                }

                TheSpawn.RequireSurface = requiresurface;

                string typeName = BaseXmlSpawner.ParseObjectType(substitutedtypeName);

                if (BaseXmlSpawner.IsTypeOrItemKeyword(typeName))
                {
                    string status_str = null;

                    bool completedtypespawn = BaseXmlSpawner.SpawnTypeKeyword(this, TheSpawn, typeName, substitutedtypeName, out status_str);

                    if (status_str != null)
                    {
                        this.status_str = status_str;
                    }

                    if (completedtypespawn)
                    {
                        // successfully spawned the keyword
                        // note that returning true means that Spawn will assume that it worked and will not try to respawn something else
                        // added the duration timer that begins on spawning
                        DoTimer2(m_Duration);

                        InvalidateProperties();

                        return true;
                    }

                    return false;
                }

                // its a regular type descriptor so find out what it is
                Type type = SpawnerType.GetType(typeName);

                // dont try to spawn invalid types, or Mobile type spawns in containers
                if (type != null && !(Parent != null && (type == typeof(Mobile) || type.IsSubclassOf(typeof(Mobile)))))
                {

                    string[] arglist = BaseXmlSpawner.ParseString(substitutedtypeName, 3, "/");

                    object o = CreateObject(type, arglist[0]);

                    if (o == null)
                    {
                        status_str = "invalid type specification: " + arglist[0];
                        return true;
                    }
                    try
                    {
                        if (o is Mobile m)
                        {
                            // if this is in any container such as a pack the xyz values are invalid as map coords so dont spawn the mob
                            if (Parent is Container)
                            {
                                m.Delete();

                                return true;
                            }

                            // add the mobile to the spawned list
                            TheSpawn.SpawnedObjects.Add(m);

                            m.Spawner = this;

                            var loc = GetSpawnPosition(requiresurface, packrange, packcoord, spawnpositioning, m);

                            if (!smartspawn)
                            {
                                m.OnBeforeSpawn(loc, map);
                            }

                            m.MoveToWorld(loc, map);

                            if (m is BaseCreature c)
                            {
                                c.RangeHome = m_HomeRange;
                                c.CurrentWayPoint = m_WayPoint;

                                if (m_Team > 0)
                                    c.Team = m_Team;

                                // Check if this spawner uses absolute (from spawnER location)
                                // or relative (from spawnED location) as the mobiles home point
                                c.Home = m_HomeRangeIsRelative ? c.Location : Location;
                            }

                            // if the object has an OnSpawned method, then invoke it
                            if (!smartspawn)
                            {
                                m.OnAfterSpawn();
                            }

                            // apply the parsed arguments from the typestring using setcommand
                            // be sure to do this after setting map and location so that errors dont place the mob on the internal map
                            string status_str;

                            BaseXmlSpawner.ApplyObjectStringProperties(this, substitutedtypeName, m, out status_str);

                            if (status_str != null)
                            {
                                this.status_str = status_str;
                            }

                            InvalidateProperties();

                            // added the duration timer that begins on spawning
                            DoTimer2(m_Duration);

                            return true;
                        }

                        if (o is Item item)
                        {
                            string status_str;

                            BaseXmlSpawner.AddSpawnItem(this, TheSpawn, item, Location, map, requiresurface, spawnpositioning, substitutedtypeName, smartspawn, out status_str);

                            if (status_str != null)
                            {
                                this.status_str = status_str;
                            }

                            InvalidateProperties();

                            // added the duration timer that begins on spawning
                            DoTimer2(m_Duration);

                            return true;
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("When spawning {0}, {1}", o, ex); }
                }
                else
                {
                    status_str = "invalid type specification: " + typeName;
                    return true;
                }
            }
            return false;
        }

        public bool SpawnSubGroup(int sgroup, byte loops)
        {
            return SpawnSubGroup(sgroup, false, loops);
        }

        public bool SpawnSubGroup(int sgroup, bool smartspawn, byte loops)
        {
            return SpawnSubGroup(sgroup, false, false, loops);
        }

        public bool SpawnSubGroup(int sgroup, bool smartspawn, bool ignoreloopprotection, byte loops)
        {
            if (m_SpawnObjects == null) return false;

            if (sgroup >= 0)
            {
                bool didspawn = false;
                Point3D packcoord = Point3D.Zero;

                for (int j = 0; j < m_SpawnObjects.Count; j++)
                {
                    SpawnObject so = m_SpawnObjects[j];

                    if (so != null && so.SubGroup == sgroup)
                    {
                        // find the first subgroup spawn to determine the packspawning reference coordinates
                        if (so.PackRange >= 0 && packcoord == Point3D.Zero)
                        {
                            packcoord = GetPackCoord(sgroup);
                        }

                        // get the SpawnsPerTick count and spawn up to that number
                        bool success = Spawn(j, smartspawn, so.SpawnsPerTick, so.PackRange, packcoord, ignoreloopprotection, loops);

                        if (success) didspawn = true;

                        if (success && !smartspawn)
                            RefreshNextSpawnTime(so);
                    }
                }

                if (didspawn) // success if any of the subgroup spawned
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Spawn support methods
        public Point3D GetPackCoord(int sgroup)
        {
            for (int j = 0; j < m_SpawnObjects.Count; j++)
            {
                SpawnObject so = m_SpawnObjects[j];

                if (so != null && so.SubGroup == sgroup && so.SpawnedObjects.Count > 0 && so.PackRange >= 0)
                {
                    // if pack spawning is enabled for this subgroup, then get the
                    // the origin for pack spawning using the first existing pack spawn
                    // in the subgroup

                    for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                    {
                        object o = so.SpawnedObjects[i];

                        if (o is Item item)
                        {
                            return item.Location;
                        }

                        if (o is Mobile mobile)
                        {
                            return mobile.Location;
                        }
                    }
                }
            }

            return Point3D.Zero;
        }

        public void ResetAllFlags() //used by the reset button in the gump
        {
            m_proximityActivated = false;
            m_ExternalTrigger = false;
            m_durActivated = false;
            m_refractActivated = false;
            m_mob_who_triggered = null;
            m_killcount = 0;
            FreeRun = false;
        }

        public bool BringHome
        {
            set { if (value) BringToHome(); }
        }

        public void BringToHome()
        {
            if (m_SpawnObjects == null)
            {
                return;
            }

            Defrag(false);

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                {
                    object o = so.SpawnedObjects[i];

                    if (o is Mobile m)
                    {
                        m.Map = Map;
                        m.Location = new Point3D(Location);
                    }
                    else if (o is Item item)
                    {
                        item.MoveToWorld(Location, Map);
                    }
                }
            }
        }

        public bool CheckRegionAssignment
        {
            get => false;
            set
            {
                if (value)
                {
                    // see if a region definition needs updating
                    if (m_Region == null && m_RegionName != null && RegionName != string.Empty)
                    {
                        RegionName = RegionName;

                        if (SpawnRegion != null)
                        {
                            status_str = null; // clear the status if successful
                        }
                    }
                }
            }
        }

        public void Start()
        {
            if (m_Running == false)
            {
                if (m_SpawnObjects != null && m_SpawnObjects.Count > 0)
                {
                    m_Running = true;
                    DoTimer();
                }
            }
        }

        public void Stop()
        {
            if (m_Running)
            {
                // turn off all timers
                if (m_Timer != null)
                    m_Timer.Stop();
                if (m_DurTimer != null)
                    m_DurTimer.Stop();
                if (m_RefractoryTimer != null)
                    m_RefractoryTimer.Stop();
                m_Running = false;
                m_proximityActivated = false;
                m_ExternalTrigger = false;
                m_mob_who_triggered = null;
            }
        }

        public void Reset()
        {
            Stop();
            // reset the protection against runaway looping
            ClearSpawnedThisTick = true;
            RemoveSpawnObjects();
            ResetAllFlags();
            status_str = "";
            m_killcount = 0;
            OnHold = false;
            mostRecentSpawnPosition = Point3D.Zero;
            spawnPositionWayTable = null;
            // dont advance before the next spawn
            HoldSequence = true;
            IsInactivated = false;
            ResetSequential();
        }

        public bool Respawn()
        {
            inrespawn = true;
            IsInactivated = false;

            // reset the protection against runaway looping
            ClearSpawnedThisTick = true;

            // Delete all currently spawned objects
            RemoveSpawnObjects();

            // added the explicit start.  Previously it relied on the automatic start that occurred when the spawnobject list was updated.
            Start();

            ResetNextSpawnTimes();

            // Respawn all objects up to the spawners current maximum allowed
            // note that by default, for proximity sensing, the spawner will only trigger once, but for respawns allow them all
            bool keepProximityActivated = m_proximityActivated;

            bool triedtospawn = false;

            // attempt to spawn up to the MaxCount of the spawner
            for (int x = 0; x < m_Count; x++)
            {
                triedtospawn = Spawn(false, 0);

                if (x < m_Count - 1 || OnHold)
                    m_proximityActivated = keepProximityActivated;

            }
            if (!FreeRun)
            {
                m_mob_who_triggered = null;
            }

            inrespawn = false;

            return triedtospawn;
        }

        // used to optimize smartspawning use of hasholdsmartspawning
        public void SmartRespawn()
        {
            inrespawn = true;
            IsInactivated = false;

            // reset the protection against runaway looping
            ClearSpawnedThisTick = true;

            // Delete all currently spawned objects
            SmartRemoveSpawnObjects();

            // added the explicit start.  Previously it relied on the automatic start that occurred when the spawnobject list was updated.
            Start();

            ResetNextSpawnTimes();

            // Respawn all objects up to the spawners current maximum allowed
            // note that by default, for proximity sensing, the spawner will only trigger once, but for respawns allow them all
            bool keepProximityActivated = m_proximityActivated;

            // attempt to spawn up to the MaxCount of the spawner
            for (int x = 0; x < m_Count; x++)
            {
                Spawn(true, 0);

                if (x < m_Count - 1 || OnHold) m_proximityActivated = keepProximityActivated;
            }

            if (!FreeRun)
            {
                m_mob_who_triggered = null;
            }

            inrespawn = false;
        }

        public void SortSpawns()
        {
            if (m_SpawnObjects == null)
                return;

            // establish the entry order
            int count = 0;

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                so.EntryOrder = count++;
            }

            m_SpawnObjects.Sort(new SubgroupSorter());
        }

        private class SubgroupSorter : IComparer<SpawnObject>
        {
            public int Compare(SpawnObject a, SpawnObject b)
            {
                if (a.SubGroup == b.SubGroup)
                {
                    // use the entry order as the secondary sort factor
                    return a.EntryOrder - b.EntryOrder;
                }

                return a.SubGroup - b.SubGroup;
            }
        }

        public bool HasSubGroups()
        {
            if (m_SpawnObjects == null)
                return false;

            for (int j = 0; j < m_SpawnObjects.Count; j++)
            {
                if (m_SpawnObjects[j].SubGroup > 0) return true;
            }

            return false;
        }

        private void ResetProximityActivated()
        {
            // dont reset triggering if free run mode has been selected
            if (!FreeRun)
            {
                m_proximityActivated = false;
            }
        }

        public bool HasIndividualSpawnTimes()
        {

            if (m_SpawnObjects != null && m_SpawnObjects.Count > 0)
            {
                for (int i = 0; i < m_SpawnObjects.Count; i++)
                {
                    SpawnObject so = m_SpawnObjects[i];

                    if (so.MinDelay != -1 || so.MaxDelay != -1)
                        return true;
                }
            }

            return false;
        }

        private void ResetNextSpawnTimes()
        {

            if (m_SpawnObjects != null && m_SpawnObjects.Count > 0)
            {
                for (int i = 0; i < m_SpawnObjects.Count; i++)
                {
                    SpawnObject so = m_SpawnObjects[i];

                    so.NextSpawn = DateTime.UtcNow;
                }
            }
        }

        public void RefreshNextSpawnTime(SpawnObject so)
        {
            if (so == null)
                return;

            int mind = (int)(so.MinDelay * 60);
            int maxd = (int)(so.MaxDelay * 60);

            if (mind < 0 || maxd < 0)
            {
                so.NextSpawn = DateTime.UtcNow;
            }
            else
            {
                TimeSpan delay = TimeSpan.FromSeconds(Utility.RandomMinMax(mind, maxd));

                so.NextSpawn = DateTime.UtcNow + delay;
            }
        }

        public static bool IsValidMapLocation(int X, int Y, Map map)
        {
            if (map == null || map == Map.Internal)
                return false;

            // check the location relative to the current map to make sure it is valid
            if (X < 0 || X > map.Width || Y < 0 || Y > map.Height)
            {
                return false;
            }

            return true;
        }
        
        private static WayPoint GetWaypoint(string waypointstr)
        {
            WayPoint waypoint = null;

            // try parsing the waypoint name to determine the waypoint. object syntax is "SERIAL,sernumber" or "waypointname"
            if (!string.IsNullOrEmpty(waypointstr))
            {
                string[] wayargs = BaseXmlSpawner.ParseString(waypointstr, 2, ",");
                if (wayargs != null && wayargs.Length > 0)
                {
                    // is this a SERIAL specification?
                    if (wayargs[0] == "SERIAL")
                    {
                        // look it up by serial
                        if (wayargs.Length > 1)
                        {
                            int sernum = -1;
                            try { sernum = (int)Convert.ToUInt64(wayargs[1].Substring(2), 16); }
                            catch { }

                            if (sernum > -1)
                            {
                                IEntity e = World.FindEntity(sernum);

                                if (e is WayPoint point)
                                {
                                    waypoint = point;
                                }
                            }
                        }
                    }
                    else
                    {
                        Item wayitem = BaseXmlSpawner.FindItemByName(null, wayargs[0], "WayPoint"); // just look it up by name

                        if (wayitem is WayPoint point)
                        {
                            waypoint = point;
                        }
                    }
                }
            }

            return waypoint;
        }

        private static bool HasTileSurface(Map map, int X, int Y, int Z)
        {
            if (map == null)
                return false;

            StaticTile[] tiles = map.Tiles.GetStaticTiles(X, Y, true);

            if (tiles == null)
                return false;

            for (var index = 0; index < tiles.Length; index++)
            {
                StaticTile o = tiles[index];
                StaticTile i = o;

                if (i.Z + i.Height == Z)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckHoldSmartSpawning(object o)
        {
            if (o == null)
                return false;

            // try looking this up in the lookup table
            if (holdSmartSpawningHash == null)
            {
                holdSmartSpawningHash = new Dictionary<Type, PropertyInfo>();
            }
            PropertyInfo prop;
            if (!holdSmartSpawningHash.TryGetValue(o.GetType(), out prop))
            {
                prop = o.GetType().GetProperty("HoldSmartSpawning");
                // check to make sure the HoldSmartSpawning property for this object has the right type
                if (prop != null && (!prop.CanRead || prop.PropertyType != typeof(bool)))
                {
                    prop = null;
                }

                holdSmartSpawningHash[o.GetType()] = prop;
            }

            if (prop != null)
            {
                try
                {
                    return (bool)prop.GetValue(o, null);
                }
                catch { }
            }

            return false;
        }

        public bool CanSpawnMobile(int x, int y, int z, Mobile mob)
        {
            if (DebugThis)
            {
                Console.WriteLine("CanSpawnMobile mob {0}", mob);
            }
            if (!Region.Find(new Point3D(x, y, z), Map).AllowSpawn())
                return false;

            return Map.CanFit(x, y, z, 16, false, true, true, mob);
        }

        public bool HasRegionPoints(Region r)
        {
            if (r != null && r.Area.Length > 0)
            {
                return true;
            }

            return false;
        }

        private void FindTileLocations(ref List<Point3D> locations, Map map, int startx, int starty, int width, int height, List<int> includetilelist, List<int> excludetilelist, TileFlag tileflag, bool checkitems, int spawnerZ)
        {
            if (width < 0 || height < 0 || map == null) return;

            if (locations == null)
                locations = new List<Point3D>();

            bool includetile;
            bool excludetile;

            for (int x = startx; x <= startx + width; x++)
            {
                for (int y = starty; y <= starty + height; y++)
                {
                    bool allok = false;
                    Point3D p = Point3D.Zero;
                    // go through all of the tiles at the location and find those that are in the allowed tiles list
                    LandTile ltile = map.Tiles.GetLandTile(x, y);
                    TileFlag lflags = TileData.LandTable[ltile.ID & TileData.MaxLandValue].Flags;

                    // check the land tile
                    if (includetilelist != null && includetilelist.Count > 0)
                    {
                        includetile = includetilelist.Contains(ltile.ID & TileData.MaxLandValue);
                    }
                    else
                    {
                        includetile = true;
                    }

                    // non-excluded tiles must also be passable
                    if (excludetilelist != null && excludetilelist.Count > 0)
                    {
                        // also require the tile to be passable
                        excludetile = (lflags & TileFlag.Impassable) != 0 || excludetilelist.Contains(ltile.ID & TileData.MaxLandValue);
                    }
                    else
                    {
                        excludetile = false;
                    }

                    if (includetile && !excludetile && (lflags & tileflag) == tileflag)
                    {
                        p = new Point3D(x, y, ltile.Z + ltile.Height);
                        allok = true;
                    }

                    StaticTile[] statictiles = map.Tiles.GetStaticTiles(x, y, true);

                    // check the static tiles
                    for (int i = 0; i < statictiles.Length; ++i)
                    {
                        StaticTile stile = statictiles[i];
                        TileFlag sflags = TileData.ItemTable[stile.ID & TileData.MaxItemValue].Flags;

                        if (includetilelist != null && includetilelist.Count > 0)
                        {
                            includetile = includetilelist.Contains(stile.ID & TileData.MaxItemValue);
                        }
                        else
                        {
                            includetile = true;
                        }

                        // non-excluded tiles must also be passable
                        if (excludetilelist != null && excludetilelist.Count > 0)
                        {
                            excludetile = (sflags & TileFlag.Impassable) != 0 || excludetilelist.Contains(stile.ID & TileData.MaxItemValue);
                        }
                        else
                        {
                            excludetile = false;
                        }

                        if (includetile && !excludetile && (sflags & tileflag) == tileflag)
                        {
                            //Console.WriteLine("found statictile {0}/{1} at {2},{3},{4}", stile.ID, stile.ID & 0x3fff, x, y, stile.Z + stile.Height);
                            if (p == Point3D.Zero)
                                p = new Point3D(x, y, stile.Z + stile.Height);
                            else if (!allok && p.Z - spawnerZ > Math.Abs(stile.Z - spawnerZ))
                                p = new Point3D(x, y, stile.Z + stile.Height);
                            else if (Math.Abs(ltile.Z - spawnerZ) > Math.Abs(stile.Z - spawnerZ)) 
                                p = new Point3D(x, y, stile.Z + stile.Height);
                            allok = true;
                        }
                    }

                    if (checkitems)
                    {
                        IPooledEnumerable itemslist = map.GetItemsInRange(new Point3D(x, y, 0), 0);

                        // check the itemsid
                        foreach (Item i in itemslist)
                        {
                            if (i.ItemData.Impassable)
                                excludetile = true;

                            TileFlag iflags = TileData.ItemTable[i.ItemID & TileData.MaxItemValue].Flags;
                            if (includetilelist != null && includetilelist.Count > 0)
                            {
                                includetile = includetilelist.Contains(i.ItemID & TileData.MaxItemValue);
                            }
                            else
                            {
                                includetile = true;
                            }

                            if (excludetilelist != null && excludetilelist.Count > 0)
                            {
                                excludetile = excludetilelist.Contains(i.ItemID & TileData.MaxItemValue);
                            }
                            else
                            {
                                excludetile = false;
                            }

                            if (includetile && !excludetile && (iflags & tileflag) == tileflag)
                            {
                                p = new Point3D(x, y, i.Z + i.ItemData.Height);
                                allok = true;
                            }
                        }

                        itemslist.Free();
                    }

                    if (allok && !excludetile)
                        locations.Add(p);
                }
            }
        }

        private void FindRegionTileLocations(ref List<Point3D> locations, Region r, List<int> includetilelist, List<int> excludetilelist, TileFlag tileflag, bool checkitems, int spawnerZ)
        {
            if (r == null || r.Area == null) return;

            int count = r.Area.Length;

            if (locations == null) locations = new List<Point3D>();

            // calculate fields of all rectangles (for probability calculating)
            for (int n = 0; n < count; n++)
            {
                Rectangle3D ra = r.Area[n];
                int sx = ra.Start.X;
                int sy = ra.Start.Y;
                int w = ra.Width;
                int h = ra.Height;

                // find all of the valid tile locations in the area
                FindTileLocations(ref locations, r.Map, sx, sy, w, h, includetilelist, excludetilelist, tileflag, checkitems, spawnerZ);
            }
        }

        public Point2D GetRandomRegionPoint(Region r)
        {
            int count = r.Area.Length;

            int[] FieldArray = new int[count];
            int total = 0;

            // calculate fields of all rectangles (for probability calculating)
            for (int i = 0; i < count; i++)
            {
                Rectangle3D ra = r.Area[i];
                total += FieldArray[i] = (ra.Width * ra.Height);
            }

            int sum = 0;
            int rnd = 0;
            if (total > 0)
                rnd = Utility.Random(total);
            int x = 0;
            int y = 0;
            for (int i = 0; i < count; i++)
            {
                sum += FieldArray[i];
                if (sum > rnd)
                {
                    Rectangle3D r3d = r.Area[i];
                    if (r3d.Width >= 0)
                        x = r3d.Start.X + Utility.Random(r3d.Width);
                    if (r3d.Height >= 0)
                        y = r3d.Start.Y + Utility.Random(r3d.Height);
                    break;
                }
            }

            return new Point2D(x, y);
        }

        // used for getting non-mobile spawn positions
        public Point3D GetSpawnPosition(bool requiresurface, int packrange, Point3D packcoord, List<SpawnPositionInfo> spawnpositioning)
        {
            return GetSpawnPosition(requiresurface, packrange, packcoord, spawnpositioning, null);
        }

        public Point3D GetSpawnPosition(bool requiresurface, int packrange, Point3D packcoord, List<SpawnPositionInfo> spawnpositioning, Mobile mob)
        {
            Map map = Map;

            if (map == null)
                return Location;

            // random positioning by default
            SpawnPositionType positioning = SpawnPositionType.Random;
            Mobile trigmob = null;
            List<int> includetilelist = null;
            List<int> excludetilelist = null;
            bool checkitems = false;
            // restrictions on tile flags
            TileFlag tileflag = TileFlag.None;
            List<Point3D> locations = null;

            int fillinc = 1;
            int positionrange = 0;
            string prefix = null;
            List<Item> WayList = null;
            int xinc = 0;
            int yinc = 0;
            int zinc = 0;
            if (spawnpositioning != null)
            {
                for (var index = 0; index < spawnpositioning.Count; index++)
                {
                    SpawnPositionInfo s = spawnpositioning[index];
                    if (s == null) continue;

                    trigmob = s.trigMob;
                    string[] positionargs = s.positionArgs;

                    // parse the possible args to the spawn position control keywords
                    switch (s.positionType)
                    {
                        case SpawnPositionType.Wet:
                            // syntax Wet
                            // find all of the wet tiles
                            tileflag |= TileFlag.Wet;
                            requiresurface = false;
                            break;
                        case SpawnPositionType.ItemID:
                            checkitems = true;
                            goto case SpawnPositionType.Tiles;
                        case SpawnPositionType.NoItemID:
                            checkitems = true;
                            goto case SpawnPositionType.NoTiles;
                        case SpawnPositionType.Tiles:
                        {
                            // syntax Tiles,start[,end]
                            // get the tiles in the range
                            requiresurface = false;
                            int start = -1;
                            int end = -1;
                            if (positionargs != null && positionargs.Length > 1)
                            {
                                try
                                {
                                    start = int.Parse(positionargs[1]);
                                }
                                catch
                                {
                                }
                            }

                            if (positionargs != null && positionargs.Length > 2)
                            {
                                try
                                {
                                    end = int.Parse(positionargs[2]);
                                }
                                catch
                                {
                                }
                            }

                            if (includetilelist == null)
                            {
                                includetilelist = new List<int>();
                            }

                            // add the tiles to the list
                            if (start > -1 && end < 0)
                            {
                                includetilelist.Add(start);
                            }
                            else if (start > -1 && end > -1)
                            {
                                for (int j = start; j <= end; j++)
                                {
                                    includetilelist.Add(j);
                                }
                            }

                            break;
                        }
                        case SpawnPositionType.NoTiles:
                        {
                            // syntax Tiles,start[,end]
                            // get the tiles in the range
                            requiresurface = false;
                            int start = -1;
                            int end = -1;
                            if (positionargs != null && positionargs.Length > 1)
                            {
                                try
                                {
                                    start = int.Parse(positionargs[1]);
                                }
                                catch
                                {
                                }
                            }

                            if (positionargs != null && positionargs.Length > 2)
                            {
                                try
                                {
                                    end = int.Parse(positionargs[2]);
                                }
                                catch
                                {
                                }
                            }

                            if (excludetilelist == null)
                            {
                                excludetilelist = new List<int>();
                            }

                            // add the tiles to the list
                            if (start > -1 && end < 0)
                            {
                                excludetilelist.Add(start);
                            }
                            else if (start > -1 && end > -1)
                            {
                                for (int j = start; j <= end; j++)
                                {
                                    excludetilelist.Add(j);
                                }
                            }

                            break;
                        }
                        case SpawnPositionType.RowFill:
                        case SpawnPositionType.ColFill:
                        case SpawnPositionType.Perimeter:
                            // syntax XFILL[,inc]
                            // syntax YFILL[,inc]
                            // syntax EDGE[,inc]
                            positioning = s.positionType;
                            if (positionargs != null && positionargs.Length > 1)
                            {
                                try
                                {
                                    fillinc = int.Parse(positionargs[1]);
                                }
                                catch
                                {
                                }
                            }

                            break;
                        case SpawnPositionType.RelXY:
                        case SpawnPositionType.DeltaLocation:
                        case SpawnPositionType.Location:
                            // syntax RELXY,xinc,yinc[,zinc]
                            // syntax XY,x,y[,z]
                            // syntax DXY,dx,dy[,dz]
                            positioning = s.positionType;
                            if (positionargs != null && positionargs.Length > 2)
                            {
                                try
                                {
                                    xinc = int.Parse(positionargs[1]);
                                    yinc = int.Parse(positionargs[2]);
                                }
                                catch
                                {
                                }
                            }

                            if (positionargs != null && positionargs.Length > 3)
                            {
                                try
                                {
                                    zinc = int.Parse(positionargs[3]);
                                }
                                catch
                                {
                                }
                            }

                            break;
                        case SpawnPositionType.Waypoint:
                            // syntax WAYPOINT,prefix[,range]
                            positioning = s.positionType;
                            if (positionargs != null && positionargs.Length > 1)
                            {
                                prefix = positionargs[1];
                            }

                            if (positionargs != null && positionargs.Length > 2)
                            {
                                try
                                {
                                    positionrange = int.Parse(positionargs[2]);
                                }
                                catch
                                {
                                }
                            }

                            // find a list of items that match the waypoint prefix
                            if (prefix != null)
                            {
                                // see if there is an existing hashtable for the waypoint lists
                                if (spawnPositionWayTable == null)
                                {
                                    spawnPositionWayTable = new Dictionary<string, List<Item>>();
                                }

                                // no existing list so create a new one
                                if (!spawnPositionWayTable.TryGetValue(prefix, out WayList) || WayList == null)
                                {
                                    WayList = new List<Item>();

                                    foreach (Item i in World.Items.Values)
                                    {
                                        if (i is WayPoint && !string.IsNullOrEmpty(i.Name) && i.Map == Map && i.Name == prefix)
                                        {
                                            // add it to the list of items
                                            WayList.Add(i);
                                        }
                                    }

                                    // add the new list to the local table
                                    spawnPositionWayTable[prefix] = WayList;
                                }
                            }

                            break;
                        case SpawnPositionType.Player:
                            // syntax PLAYER[,range]
                            positioning = s.positionType;
                            if (positionargs != null && positionargs.Length > 1)
                            {
                                try
                                {
                                    positionrange = int.Parse(positionargs[1]);
                                }
                                catch
                                {
                                }
                            }

                            break;
                    }
                }
            }

            // precalculate tile locations if they have been specified
            if (includetilelist != null || excludetilelist != null || tileflag != TileFlag.None)
            {
                if (m_Region != null && HasRegionPoints(m_Region))
                {
                    FindRegionTileLocations(ref locations, m_Region, includetilelist, excludetilelist, tileflag, checkitems, Z);
                }
                else if (positioning == SpawnPositionType.Random)
                {
                    FindTileLocations(ref locations, Map, m_X, m_Y, m_Width, m_Height, includetilelist, excludetilelist, tileflag, checkitems, Z);
                }
            }

            // Try 10 times to find a Spawnable location.
            // trace profiling indicates that this is a major bottleneck
            for (int i = 0; i < 10; i++)
            {
                int x = X;
                int y = Y;
                int z = Z;

                int defaultZ = Z;
                if (packrange >= 0 && packcoord != Point3D.Zero)
                {
                    defaultZ = packcoord.Z;
                }

                if (packrange >= 0 && packcoord != Point3D.Zero)
                {
                    // find a random coord relative to the packcoord
                    x = packcoord.X - packrange + Utility.Random(packrange * 2 + 1);
                    y = packcoord.Y - packrange + Utility.Random(packrange * 2 + 1);
                }
                else if (m_Region != null && HasRegionPoints(m_Region))  
                {
                    // if region spawning is selected then use that to find an x,y loc instead of the spawn box

                    if (includetilelist != null || excludetilelist != null || tileflag != TileFlag.None)
                    {
                        // use the precalculated tile locations
                        if (locations != null && locations.Count > 0)
                        {
                            Point3D p = locations[Utility.Random(locations.Count)];
                            x = p.X;
                            y = p.Y;
                            defaultZ = p.Z;
                        }
                    }
                    else
                    {
                        Point2D p = GetRandomRegionPoint(m_Region);
                        x = p.X;
                        y = p.Y;
                    }
                }
                else
                {
                    switch (positioning)
                    {
                        case SpawnPositionType.Random:
                            if (includetilelist != null || excludetilelist != null || tileflag != TileFlag.None)
                            {
                                if (locations != null && locations.Count > 0)
                                {
                                    Point3D p = locations[Utility.Random(locations.Count)];
                                    x = p.X;
                                    y = p.Y;
                                    defaultZ = p.Z;
                                }
                            }
                            else
                            {
                                if (m_Width > 0)
                                    x = m_X + Utility.Random(m_Width + 1);
                                if (m_Height > 0)
                                    y = m_Y + Utility.Random(m_Height + 1);
                            }
                            break;
                        case SpawnPositionType.RelXY:
                            x = mostRecentSpawnPosition.X + xinc;
                            y = mostRecentSpawnPosition.Y + yinc;
                            defaultZ = mostRecentSpawnPosition.Z + zinc;
                            break;
                        case SpawnPositionType.DeltaLocation:
                            x = X + xinc;
                            y = Y + yinc;
                            defaultZ = Z + zinc;
                            break;
                        case SpawnPositionType.Location:
                            x = xinc;
                            y = yinc;
                            defaultZ = zinc;
                            break;
                        case SpawnPositionType.RowFill:
                            x = mostRecentSpawnPosition.X + fillinc;
                            y = mostRecentSpawnPosition.Y;

                            if (x < m_X)
                            {
                                x = m_X;
                            }

                            if (y < m_Y)
                            {
                                y = m_Y;
                            }

                            if (x > m_X + m_Width)
                            {
                                x = m_X + (x - m_X - m_Width - 1);
                                y++;
                            }

                            if (y > m_Y + m_Height)
                            {
                                y = m_Y;
                            }

                            break;
                        case SpawnPositionType.ColFill:
                            x = mostRecentSpawnPosition.X;
                            y = mostRecentSpawnPosition.Y + fillinc;

                            if (x < m_X)
                            {
                                x = m_X;
                            }

                            if (y < m_Y)
                            {
                                y = m_Y;
                            }

                            if (y > m_Y + m_Height)
                            {
                                y = m_Y + (y - m_Y - m_Height - 1);
                                x++;
                            }

                            if (x > m_X + m_Width)
                            {
                                x = m_X;
                            }

                            break;
                        case SpawnPositionType.Perimeter:
                            x = mostRecentSpawnPosition.X;
                            y = mostRecentSpawnPosition.Y;

                            // if the point is not on the perimeter, reset it to the corner
                            if (x != m_X && x != m_X + m_Width && y != m_Y && y != m_Y + m_Height)
                            {
                                x = m_X;
                                y = m_Y;
                            }

                            if (y == m_Y && x < m_X + m_Width)
                                x += fillinc;
                            else if (y == m_Y + m_Height && x > m_X)
                                x -= fillinc;
                            else if (x == m_X && y > m_Y)
                                y -= fillinc;
                            else if (x == m_X + m_Width && y < m_Y + m_Height)
                                y += fillinc;

                            if (x > m_X + m_Width)
                            {
                                x = m_X + m_Width;
                            }

                            if (y > m_Y + m_Height)
                            {
                                y = m_Y + m_Height;
                            }

                            if (x < m_X)
                            {
                                x = m_X;
                            }

                            if (y < m_Y)
                            {
                                y = m_Y;
                            }

                            break;
                        case SpawnPositionType.Player:
                            if (trigmob != null)
                            {
                                x = trigmob.Location.X;
                                y = trigmob.Location.Y;
                                if (positionrange > 0)
                                {
                                    x += Utility.Random(positionrange * 2 + 1) - positionrange;
                                    y += Utility.Random(positionrange * 2 + 1) - positionrange;
                                }
                            }
                            break;
                        case SpawnPositionType.Waypoint:
                            // pick an item randomly from the waylist
                            if (WayList != null && WayList.Count > 0)
                            {
                                int index = Utility.Random(WayList.Count);
                                Item waypoint = WayList[index];
                                if (waypoint != null)
                                {
                                    x = waypoint.Location.X;
                                    y = waypoint.Location.Y;
                                    defaultZ = waypoint.Location.Z;
                                    if (positionrange > 0)
                                    {
                                        x += Utility.Random(positionrange * 2 + 1) - positionrange;
                                        y += Utility.Random(positionrange * 2 + 1) - positionrange;
                                    }
                                }
                            }

                            break;
                    }

                    mostRecentSpawnPosition = new Point3D(x, y, defaultZ);
                }

                // skip invalid points
                if (x < 0 || y < 0 || x == 0 && y == 0)
                    continue;

                // try to find a valid spawn location using the z coord of the spawner
                // relax the normal surface requirement for mobiles if the flag is set
                var fit = requiresurface ? CanSpawnMobile(x, y, defaultZ, mob) : Map.CanFit(x, y, defaultZ, SpawnFitSize, true, false, false);

                // if that fails then try to find a valid z coord
                if (fit)
                {
                    return new Point3D(x, y, defaultZ);
                }

                z = Map.GetAverageZ(x, y);

                fit = requiresurface ? CanSpawnMobile(x, y, z, mob) : Map.CanFit(x, y, z, SpawnFitSize, true, false, false);

                if (fit)
                {
                    return new Point3D(x, y, z);
                }
            }

            if (packrange >= 0 && packcoord != Point3D.Zero)
            {
                return packcoord;
            }

            return Location;
        }

        private void DeleteFromList(List<object> list)
        {
            if (list == null)
            {
                return;
            }

            for (var index = 0; index < list.Count; index++)
            {
                object o = list[index];

                if (o is Item item)
                {
                    item.Delete();
                }
                else if (o is Mobile mobile)
                {
                    mobile.Delete();
                }
            }
        }

        private void DeleteFromList(List<Item> listi, List<Mobile> listm)
        {
            if (listi != null)
            {
                int i = listi.Count;

                while (--i >= 0)
                {
                    if (i < listi.Count && listi[i] != null)
                    {
                        try
                        {
                            listi[i].Delete();
                        }
                        catch
                        { }
                    }
                }

                listi.Clear();
            }

            if (listm != null)
            {
                int i = listm.Count;

                while (--i >= 0)
                {
                    if (i < listm.Count && listm[i] != null)
                    {
                        try
                        {
                            listm[i].Delete();
                        }
                        catch
                        { }
                    }
                }

                listm.Clear();
            }
        }

        public void RemoveSpawnObjects()
        {
            if (m_SpawnObjects == null)
            {
                return;
            }

            Defrag(false);

            List<object> deletelist = new List<object>();

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                {
                    object o = so.SpawnedObjects[i];

                    if (o is Item || o is Mobile)
                    {
                        deletelist.Add(o);
                    }
                }
            }

            DeleteFromList(deletelist);

            Defrag(false); // Defrag again
        }

        public void RemoveSpawnObjects(SpawnObject so)
        {
            if (so == null) return;

            Defrag(false);

            List<object> deletelist = new List<object>();

            for (int i = 0; i < so.SpawnedObjects.Count; ++i)
            {
                object o = so.SpawnedObjects[i];

                if (o is Item || o is Mobile)
                    deletelist.Add(o);

            }

            DeleteFromList(deletelist);

            Defrag(false); // Defrag again
        }

        public void ClearSubgroup(int subgroup)
        {
            if (m_SpawnObjects == null)
            {
                return;
            }

            Defrag(false);

            List<object> deletelist = new List<object>();

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                if (so.SubGroup != subgroup || !so.ClearOnAdvance)
                {
                    continue;
                }

                for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                {
                    object o = so.SpawnedObjects[i];

                    if (o is Item || o is Mobile)
                    {
                        deletelist.Add(o);
                    }
                }
            }

            DeleteFromList(deletelist);

            Defrag(false); // Defrag again
        }

        // used to optimize smart spawning by removing all objects except those that have hold smartspawning
        public void SmartRemoveSpawnObjects()
        {
            if (m_SpawnObjects == null)
                return;

            Defrag(false);

            List<object> deletelist = new List<object>();

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                for (int i = 0; i < so.SpawnedObjects.Count; ++i)
                {
                    object o = so.SpawnedObjects[i];

                    // new optimization for smart spawning to remove all objects except those with hold smartspawning enabled
                    if (CheckHoldSmartSpawning(o))
                    {
                        continue;
                    }

                    if (o is Item || o is Mobile)
                        deletelist.Add(o);
                }
            }

            DeleteFromList(deletelist);

            Defrag(false); // Defrag again
        }

        public void AddSpawnObject(string SpawnObjectName)
        {
            if (m_SpawnObjects == null)
                return;

            Defrag(false);

            // Find the spawn object and increment its count by one
            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                if (so.TypeName.ToUpper() == SpawnObjectName.ToUpper())
                {
                    // Add one to the total count
                    m_Count++;

                    // Increment the max count for the current creature
                    so.ActualMaxCount++;

                    //only spawn them immediately if the spawner is running
                    if (Running)
                        Spawn(SpawnObjectName, false, 0);
                }
            }

            InvalidateProperties();
        }

        public void DeleteSpawnObject(Mobile from, string SpawnObjectName)
        {
            bool WasRunning = m_Running;

            try
            {
                // Stop spawning for a moment
                Stop();

                // Clean up any spawns marked as deleted
                Defrag(false);

                // Keep a reference to the spawn object
                SpawnObject TheSpawn = null;

                // Find the spawn object and increment its count by one
                for (var index = 0; index < m_SpawnObjects.Count; index++)
                {
                    SpawnObject so = m_SpawnObjects[index];

                    if (so.TypeName.ToUpper() == SpawnObjectName.ToUpper())
                    {
                        // Set the spawn
                        TheSpawn = so;
                        break;
                    }
                }

                // Was the spawn object found
                if (TheSpawn != null)
                {
                    bool delete_this_entry = false;

                    // Decrement the max count for the current creature
                    TheSpawn.ActualMaxCount--;

                    // Make sure the spawn count does not go negative
                    if (TheSpawn.MaxCount < 0)
                    {
                        TheSpawn.MaxCount = 0;
                        delete_this_entry = true;
                    }

                    if (!delete_this_entry)
                    {
                        // Subtract one to the total count
                        m_Count--;
                    }

                    // Make sure the count does not go negative
                    if (m_Count < 0)
                    {
                        m_Count = 0;

                    }

                    List<object> deletelist = new List<object>();

                    // Remove any spawns over the count
                    while (TheSpawn.SpawnedObjects != null && TheSpawn.SpawnedObjects.Count > 0 && TheSpawn.SpawnedObjects.Count > TheSpawn.MaxCount)
                    {
                        object o = TheSpawn.SpawnedObjects[0];

                        // Delete the object
                        if (o is Item || o is Mobile) deletelist.Add(o);

                        TheSpawn.SpawnedObjects.Remove(o);
                    }

                    DeleteFromList(deletelist);

                    // Check if the spawn object should be removed
                    if (delete_this_entry)
                    {
                        m_SpawnObjects.Remove(TheSpawn);
                        if (from != null)
                            CommandLogging.WriteLine(from, "{0} {1} removed from XmlSpawner {2} '{3}' [{4}, {5}] ({6}) : {7}", from.AccessLevel, CommandLogging.Format(from), Serial, Name, GetWorldLocation().X, GetWorldLocation().Y, Map, SpawnObjectName);
                    }
                }

                InvalidateProperties();
            }
            finally
            {
                if (WasRunning)
                    Start();
            }
        }

        public void RemoveSpawnObject(SpawnObject so)
        {
            m_SpawnObjects.Remove(so);
        }

        #endregion

        #region Object Creation

        public static object CreateObject(Type type, string itemtypestring)
        {
            return CreateObject(type, itemtypestring, true);
        }

        public static object CreateObject(Type type, string itemtypestring, bool requireconstructable)
        {
            // look for constructor arguments to be passed to it with the syntax type,arg1,arg2,.../
            string[] typewordargs = BaseXmlSpawner.ParseObjectArgs(itemtypestring);

            return CreateObject(type, typewordargs, requireconstructable);
        }

        public static object CreateObject(Type type, string[] typewordargs, bool requireconstructable)
        {
            if (type == null) return null;

            object o = null;

            int typearglen = 0;
            if (typewordargs != null)
                typearglen = typewordargs.Length;

            // ok, there are args in the typename, so we need to invoke the proper constructor
            ConstructorInfo[] ctors = type.GetConstructors();

            // go through all the constructors for this type
            for (int i = 0; i < ctors.Length; ++i)
            {
                ConstructorInfo ctor = ctors[i];

                if (!(requireconstructable && IsConstructable(ctor)))
                    continue;

                // check the parameter list of the constructor
                ParameterInfo[] paramList = ctor.GetParameters();

                // and compare with the argument list provided
                if (typearglen == paramList.Length)
                {
                    // this is a constructor that takes args and matches the number of args passed in to CreateObject
                    if (paramList.Length > 0)
                    {
                        object[] paramValues = null;

                        try
                        {
                            paramValues = Add.ParseValues(paramList, typewordargs);
                        }
                        catch { }

                        if (paramValues == null)
                            continue;

                        // ok, have a match on args, so try to construct it
                        try
                        {
                            o = Activator.CreateInstance(type, paramValues);
                        }
                        catch { }
                    }
                    else
                    {
                        // zero argument constructor
                        try
                        {
                            o = Activator.CreateInstance(type);
                        }
                        catch { }
                    }

                    // successfully constructed the object, otherwise try another matching constructor
                    if (o != null) break;
                }
            }

            return o;
        }

        #endregion

        #region Timers

        private static void DoGlobalSectorTimer(TimeSpan delay)
        {
            if (m_GlobalSectorTimer != null)
                m_GlobalSectorTimer.Stop();

            m_GlobalSectorTimer = new GlobalSectorTimer(delay);

            m_GlobalSectorTimer.Start();
        }

        private class GlobalSectorTimer : Timer
        {

            public GlobalSectorTimer(TimeSpan delay)
                : base(delay, delay)
            {
            }

            protected override void OnTick()
            {
                // check the sectors

                // check all active players
                if (NetState.Instances != null)
                {
                    for (var index = 0; index < NetState.Instances.Count; index++)
                    {
                        NetState state = NetState.Instances[index];
                        Mobile m = state.Mobile;

                        if (m != null && (m.AccessLevel <= SmartSpawnAccessLevel || !m.Hidden))
                        {
                            // activate any spawner in the sector they are in
                            if (m.Map != null && m.Map != Map.Internal)
                            {
                                Sector s = m.Map.GetSector(m.Location);

                                if (s != null && GlobalSectorTable[m.Map.MapID] != null)
                                {
                                    List<XmlSpawner> spawnerlist; // = GlobalSectorTable[m.Map.MapID][s];
                                    if (GlobalSectorTable[m.Map.MapID].TryGetValue(s, out spawnerlist) &&
                                        spawnerlist != null)
                                    {
                                        for (var i = 0; i < spawnerlist.Count; i++)
                                        {
                                            XmlSpawner spawner = spawnerlist[i];

                                            if (spawner != null && !spawner.Deleted && spawner.Running && spawner.SmartSpawning && spawner.IsInactivated)
                                            {
                                                spawner.SmartRespawn();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DoSectorTimer(TimeSpan delay)
        {
            if (m_SectorTimer != null)
                m_SectorTimer.Stop();

            m_SectorTimer = new SectorTimer(this, delay);

            m_SectorTimer.Start();
        }

        private class SectorTimer : Timer
        {
            private readonly XmlSpawner m_Spawner;

            public SectorTimer(XmlSpawner spawner, TimeSpan delay)
                : base(delay, delay)
            {
                m_Spawner = spawner;
            }

            protected override void OnTick()
            {
                // check the sectors
                if (m_Spawner != null && !m_Spawner.Deleted && m_Spawner.Running && m_Spawner.IsInactivated)
                {
                    if (m_Spawner.SmartSpawning)
                    {
                        if (m_Spawner.HasActiveSectors)
                        {
                            Stop();

                            m_Spawner.SmartRespawn();
                        }
                    }
                    else
                    {
                        Stop();

                        m_Spawner.IsInactivated = false;
                    }
                }
                else
                {
                    Stop();

                }
            }
        }

        private class WarnTimer2 : Timer
        {
            private readonly List<WarnEntry2> m_List;

            private class WarnEntry2
            {
                public Point3D m_Point;
                public Map m_Map;
                public string m_Name;

                public WarnEntry2(Point3D p, Map map, string name)
                {
                    m_Point = p;
                    m_Map = map;
                    m_Name = name;
                }
            }

            public WarnTimer2()
                : base(TimeSpan.FromSeconds(1.0))
            {
                m_List = new List<WarnEntry2>();
                Start();
            }

            public void Add(Point3D p, Map map, string name)
            {
                m_List.Add(new WarnEntry2(p, map, name));
            }

            protected override void OnTick()
            {
                try
                {
                    Console.WriteLine("Warning: {0} bad spawns detected, logged: 'badspawn.log'", m_List.Count);

                    using (StreamWriter op = new StreamWriter("badspawn.log", true))
                    {
                        op.WriteLine("# Bad spawns : {0}", DateTime.UtcNow);
                        op.WriteLine("# Format: X Y Z F Name");
                        op.WriteLine();

                        for (var index = 0; index < m_List.Count; index++)
                        {
                            WarnEntry2 e = m_List[index]; op.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", e.m_Point.X, e.m_Point.Y, e.m_Point.Z, e.m_Map, e.m_Name);
                        }

                        op.WriteLine();
                        op.WriteLine();
                    }
                }
                catch
                { }
            }
        }

        public void DoTimer()
        {
            if (!m_Running)
                return;

            int minSeconds = (int)m_MinDelay.TotalSeconds;
            int maxSeconds = (int)m_MaxDelay.TotalSeconds;

            TimeSpan delay = TimeSpan.FromSeconds(Utility.RandomMinMax(minSeconds, maxSeconds));
            DoTimer(delay);
        }

        public void DoTimer(TimeSpan delay)
        {
            if (!m_Running)
                return;

            m_End = DateTime.UtcNow + delay;

            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = new SpawnerTimer(this, delay);
            m_Timer.Start();
        }

        public void DoTimer2(TimeSpan delay)
        {
            m_DurEnd = DateTime.UtcNow + delay;
            if (m_Duration > TimeSpan.FromMinutes(0) || m_durActivated)
            {
                if (m_DurTimer != null)
                    m_DurTimer.Stop();
                m_DurTimer = new InternalTimer(this, delay);
                m_DurTimer.Start();
                m_durActivated = true;
            }
        }

        public void DoTimer3(TimeSpan delay)
        {
            m_RefractEnd = DateTime.UtcNow + delay;
            m_refractActivated = true;

            if (m_RefractoryTimer != null)
                m_RefractoryTimer.Stop();

            m_RefractoryTimer = new InternalTimer3(this, delay);
            m_RefractoryTimer.Start();
        }

        // added the duration timer that begins on spawning
        private class InternalTimer : Timer
        {
            private readonly XmlSpawner m_spawner;

            public InternalTimer(XmlSpawner spawner, TimeSpan delay)
                : base(delay)
            {
                m_spawner = spawner;
            }

            protected override void OnTick()
            {
                if (m_spawner != null && !m_spawner.Deleted)
                {
                    m_spawner.RemoveSpawnObjects();
                    m_spawner.m_durActivated = false;
                }

            }
        }

        private class SpawnerTimer : Timer
        {
            private readonly XmlSpawner m_Spawner;

            public SpawnerTimer(XmlSpawner spawner, TimeSpan delay)
                : base(delay)
            {
                m_Spawner = spawner;
            }

            protected override void OnTick()
            {
                if (m_Spawner != null && !m_Spawner.Deleted)
                {
                    m_Spawner.OnTick();
                }
            }
        }

        // added the refractory timer that begins on proximity triggering
        private class InternalTimer3 : Timer
        {
            private readonly XmlSpawner m_spawner;

            public InternalTimer3(XmlSpawner spawner, TimeSpan delay)
                : base(delay)
            {
                m_spawner = spawner;
            }

            protected override void OnTick()
            {
                if (m_spawner != null && !m_spawner.Deleted)
                {
                    // reenable triggering
                    m_spawner.m_refractActivated = false;
                }
            }
        }
        #endregion

        #region Serialization

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(33);

            writer.Write(m_DisableGlobalAutoReset);
            writer.Write(m_AllowNPCTriggering);

            if (m_SpawnObjects != null)
            {
                writer.Write(m_SpawnObjects.Count);
                for (int i = 0; i < m_SpawnObjects.Count; ++i)
                {
                    writer.Write(m_SpawnObjects[i].SpawnsPerTick);
                }
            }
            else
            {
                writer.Write(0);
            }

            if (m_SpawnObjects != null)
            {
                for (int i = 0; i < m_SpawnObjects.Count; ++i)
                {
                    writer.Write(m_SpawnObjects[i].PackRange);
                }
            }

            if (m_SpawnObjects != null)
            {
                for (int i = 0; i < m_SpawnObjects.Count; ++i)
                {
                    writer.Write(m_SpawnObjects[i].Disabled);
                }
            }

            writer.Write(m_SpawnOnTrigger);

            if (m_SpawnObjects != null)
            {
                for (int i = 0; i < m_SpawnObjects.Count; ++i)
                {
                    SpawnObject so = m_SpawnObjects[i];

                    writer.Write(so.RestrictKillsToSubgroup);
                    writer.Write(so.ClearOnAdvance);
                    writer.Write(so.MinDelay);
                    writer.Write(so.MaxDelay);
                    writer.WriteDeltaTime(so.NextSpawn);
                }
            }

            if (m_ShowBoundsItems != null && m_ShowBoundsItems.Count > 0)
            {
                writer.Write(true);
                writer.WriteItemList(m_ShowBoundsItems);
            }
            else
            {
                writer.Write(false);
            }

            writer.Write(IsInactivated);
            writer.Write(m_SmartSpawning);
            writer.Write(m_SkillTrigger);
            writer.Write((int)m_skill_that_triggered);
            writer.Write(m_FreeRun);
            writer.Write(m_mob_who_triggered);
            writer.Write(m_DespawnTime);
          
            if (m_SpawnObjects != null)
            {
                for (int i = 0; i < m_SpawnObjects.Count; ++i)
                {
                    writer.Write(m_SpawnObjects[i].RequireSurface);
                }
            }
            
            writer.Write(m_OnHold);
            writer.Write(m_HoldSequence);

            // compute the number of tags to save
            int tagcount = 0;
            for (int i = 0; i < m_KeywordTagList.Count; i++)
            {
                // only save WAIT type keywords or other keywords that have the save flag set
                if ((m_KeywordTagList[i].Flags & BaseXmlSpawner.KeywordFlags.Serialize) != 0)
                {
                    tagcount++;
                }
            }
            writer.Write(tagcount);
            // and write them out
            for (int i = 0; i < m_KeywordTagList.Count; i++)
            {
                if ((m_KeywordTagList[i].Flags & BaseXmlSpawner.KeywordFlags.Serialize) != 0)
                {
                    m_KeywordTagList[i].Serialize(writer);
                }
            }

            writer.Write(m_AllowGhostTriggering);
            writer.Write(m_SequentialSpawning);
            writer.Write(NextSeqReset);
          
            if (m_SpawnObjects != null)
            {
                for (int i = 0; i < m_SpawnObjects.Count; ++i)
                {
                    SpawnObject so = m_SpawnObjects[i];
                    // Write the subgroup and sequential reset time
                    writer.Write(so.SubGroup);
                    writer.Write(so.SequentialResetTime);
                    writer.Write(so.SequentialResetTo);
                    writer.Write(so.KillsNeeded);
                }
            }

            writer.Write(m_RegionName);
            writer.Write(m_ExternalTriggering);
            writer.Write(m_ExternalTrigger);
            writer.Write(m_NoItemTriggerName);

            int todtype = (int)m_TODMode;
            writer.Write(todtype);

            writer.Write(m_KillReset);
            writer.Write(m_skipped);
            writer.Write(m_spawncheck);
            writer.Write(m_SetPropertyItem);
            writer.Write(m_TriggerProbability);
            writer.Write(m_MobPropertyName);
            writer.Write(m_MobTriggerName);
            writer.Write(m_PlayerPropertyName);
            writer.Write(m_SpeechTrigger);
            writer.Write(m_ItemTriggerName);
            writer.Write(m_ProximityTriggerMessage);
            writer.Write(m_ObjectPropertyItem);
            writer.Write(m_ObjectPropertyName);
            writer.Write(m_killcount);
            writer.Write(m_ProximityRange);
            writer.Write(m_ProximityTriggerSound);
            writer.Write(m_proximityActivated);
            writer.Write(m_durActivated);
            writer.Write(m_refractActivated);
            writer.Write(m_StackAmount);
            writer.Write(m_TODStart);
            writer.Write(m_TODEnd);
            writer.Write(m_MinRefractory);
            writer.Write(m_MaxRefractory);

            if (m_refractActivated)
                writer.Write(m_RefractEnd - DateTime.UtcNow);
            if (m_durActivated)
                writer.Write(m_DurEnd - DateTime.UtcNow);

            writer.Write(m_ShowContainerStatic);
            writer.Write(m_Duration);
            writer.Write(m_UniqueId);
            writer.Write(m_HomeRangeIsRelative);
            writer.Write(m_Name);
            writer.Write(m_X);
            writer.Write(m_Y);
            writer.Write(m_Width);
            writer.Write(m_Height);
            writer.Write(m_WayPoint);
            writer.Write(m_Group);
            writer.Write(m_MinDelay);
            writer.Write(m_MaxDelay);
            writer.Write(m_Count);
            writer.Write(m_Team);
            writer.Write(m_HomeRange);
            writer.Write(m_Running);

            if (m_Running)
                writer.Write(m_End - DateTime.UtcNow);

            // Write the spawn object list
            int nso = 0;
            if (m_SpawnObjects != null) nso = m_SpawnObjects.Count;
            writer.Write(nso);
            for (int i = 0; i < nso; ++i)
            {
                SpawnObject so = m_SpawnObjects[i];

                writer.Write(so.TypeName); // Write the type and maximum count
                writer.Write(so.ActualMaxCount);

                writer.Write(so.SpawnedObjects.Count);// Write the spawned object information

                for (int x = 0; x < so.SpawnedObjects.Count; ++x)
                {
                    object o = so.SpawnedObjects[x];

                    if (o is Item item)
                        writer.Write(item);
                    else if (o is Mobile mobile)
                        writer.Write(mobile);
                    else
                    {
                        if (o is BaseXmlSpawner.KeywordTag tag) // if this is a keyword tag then add some more info
                        {
                            writer.Write(-1 * tag.Serial - 2);
                        }
                        else
                        {
                            writer.Write(Serial.MinusOne);
                        }
                    }
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            bool haveproximityrange = false;
            bool hasnewobjectinfo = false;
            int tmpSpawnListSize = 0;
            List<int> tmpSubGroup = null;
            List<double> tmpSequentialResetTime = null;
            List<int> tmpSequentialResetTo = null;
            List<int> tmpKillsNeeded = null;
            List<bool> tmpRequireSurface = null;
            List<bool> tmpRestrictKillsToSubgroup = null;
            List<bool> tmpClearOnAdvance = null;
            List<double> tmpMinDelay = null;
            List<double> tmpMaxDelay = null;
            List<DateTime> tmpNextSpawn = null;
            List<bool> tmpDisableSpawn = null;
            List<int> tmpPackRange = null;
            List<int> tmpSpawnsPer = null;

            switch (version)
            {
                case 33:
                case 32:
                    {
                        // 12/6/2023 - Start to clean up and eventually reset seri/deser
                        goto case 3;
                    }
                case 3:
                    {
                        m_DisableGlobalAutoReset = reader.ReadBool();
                        m_AllowNPCTriggering = reader.ReadBool();

                        tmpSpawnListSize = reader.ReadInt();
                        tmpSpawnsPer = new List<int>(tmpSpawnListSize);
                        for (int i = 0; i < tmpSpawnListSize; ++i)
                        {
                            int spawnsper = reader.ReadInt();

                            tmpSpawnsPer.Add(spawnsper);
                        }

                        tmpPackRange = new List<int>(tmpSpawnListSize);
                        for (int i = 0; i < tmpSpawnListSize; ++i)
                        {
                            int packrange = reader.ReadInt();

                            tmpPackRange.Add(packrange);
                        }

                        tmpDisableSpawn = new List<bool>(tmpSpawnListSize);
                        for (int i = 0; i < tmpSpawnListSize; ++i)
                        {
                            bool disablespawn = reader.ReadBool();

                            tmpDisableSpawn.Add(disablespawn);
                        }

                        m_SpawnOnTrigger = reader.ReadBool();

                        tmpRestrictKillsToSubgroup = new List<bool>(tmpSpawnListSize);
                        tmpClearOnAdvance = new List<bool>(tmpSpawnListSize);
                        tmpMinDelay = new List<double>(tmpSpawnListSize);
                        tmpMaxDelay = new List<double>(tmpSpawnListSize);
                        tmpNextSpawn = new List<DateTime>(tmpSpawnListSize);
                        for (int i = 0; i < tmpSpawnListSize; ++i)
                        {
                            bool restrictkills = reader.ReadBool();
                            bool clearadvance = reader.ReadBool();
                            double mind = reader.ReadDouble();
                            double maxd = reader.ReadDouble();
                            DateTime nextspawn = reader.ReadDeltaTime();

                            tmpRestrictKillsToSubgroup.Add(restrictkills);
                            tmpClearOnAdvance.Add(clearadvance);
                            tmpMinDelay.Add(mind);
                            tmpMaxDelay.Add(maxd);
                            tmpNextSpawn.Add(nextspawn);
                        }

                        bool hasitems = reader.ReadBool();

                        if (hasitems)
                        {
                            m_ShowBoundsItems = reader.ReadStrongItemList<Static>();
                        }

                        IsInactivated = reader.ReadBool();
                        SmartSpawning = reader.ReadBool();
                        SkillTrigger = reader.ReadString();
                        m_skill_that_triggered = (SkillName)reader.ReadInt();
                        m_FreeRun = reader.ReadBool();
                        m_mob_who_triggered = reader.ReadMobile();
                        m_DespawnTime = reader.ReadTimeSpan();

                        tmpRequireSurface = new List<bool>(tmpSpawnListSize);
                        for (int i = 0; i < tmpSpawnListSize; ++i)
                        {
                            bool requiresurface = reader.ReadBool();
                            tmpRequireSurface.Add(requiresurface);
                        }

                        if (version < 33)
                        {
                            reader.ReadString();
                        }

                        m_OnHold = reader.ReadBool();
                        m_HoldSequence = reader.ReadBool();

                        // deserialize the keyword tag list
                        int tagcount = reader.ReadInt();
                        m_KeywordTagList = new List<BaseXmlSpawner.KeywordTag>(tagcount);
                        for (int i = 0; i < tagcount; i++)
                        {
                            BaseXmlSpawner.KeywordTag tag = new BaseXmlSpawner.KeywordTag(null, this);
                            tag.Deserialize(reader);
                        }

                        m_AllowGhostTriggering = reader.ReadBool();
                        goto case 2;
                    }
                case 2:
                    {
                        // What is going on here with hasnewobjectinfo?
                        hasnewobjectinfo = true;
                        m_SequentialSpawning = reader.ReadInt();
                        TimeSpan seqdelay = reader.ReadTimeSpan();
                        m_SeqEnd = DateTime.UtcNow + seqdelay;

                        tmpSubGroup = new List<int>(tmpSpawnListSize);
                        tmpSequentialResetTime = new List<double>(tmpSpawnListSize);
                        tmpSequentialResetTo = new List<int>(tmpSpawnListSize);
                        tmpKillsNeeded = new List<int>(tmpSpawnListSize);
                        for (int i = 0; i < tmpSpawnListSize; ++i)
                        {
                            int subgroup = reader.ReadInt();
                            double resettime = reader.ReadDouble();
                            int resetto = reader.ReadInt();
                            int killsneeded = reader.ReadInt();
                            tmpSubGroup.Add(subgroup);
                            tmpSequentialResetTime.Add(resettime);
                            tmpSequentialResetTo.Add(resetto);
                            tmpKillsNeeded.Add(killsneeded);
                        }
                        m_RegionName = reader.ReadString();
                        goto case 1;
                    }
                case 1:
                    {
                        m_ExternalTriggering = reader.ReadBool();
                        m_ExternalTrigger = reader.ReadBool();
                        m_NoItemTriggerName = reader.ReadString();

                        if (version < 33)
                        {
                            reader.ReadString();
                        }

                        int todtype = reader.ReadInt();
                        switch (todtype)
                        {
                            case (int)TODModeType.Gametime:
                                m_TODMode = TODModeType.Gametime;
                                break;
                            case (int)TODModeType.Realtime:
                                m_TODMode = TODModeType.Realtime;
                                break;
                        }

                        m_KillReset = reader.ReadInt();
                        m_skipped = reader.ReadBool();
                        m_spawncheck = reader.ReadInt();
                        m_SetPropertyItem = reader.ReadItem();
                        m_TriggerProbability = reader.ReadDouble();
                        m_MobPropertyName = reader.ReadString();
                        m_MobTriggerName = reader.ReadString();
                        m_PlayerPropertyName = reader.ReadString();
                        m_SpeechTrigger = reader.ReadString();
                        m_ItemTriggerName = reader.ReadString();
                        m_ProximityTriggerMessage = reader.ReadString();
                        m_ObjectPropertyItem = reader.ReadItem();
                        m_ObjectPropertyName = reader.ReadString();
                        m_killcount = reader.ReadInt();

                        haveproximityrange = true;
                        m_ProximityRange = reader.ReadInt();
                        m_ProximityTriggerSound = reader.ReadInt();
                        m_proximityActivated = reader.ReadBool();
                        m_durActivated = reader.ReadBool();
                        m_refractActivated = reader.ReadBool();
                        m_StackAmount = reader.ReadInt();
                        m_TODStart = reader.ReadTimeSpan();
                        m_TODEnd = reader.ReadTimeSpan();
                        m_MinRefractory = reader.ReadTimeSpan();
                        m_MaxRefractory = reader.ReadTimeSpan();

                        if (m_refractActivated)
                        {
                            TimeSpan delay = reader.ReadTimeSpan();
                            DoTimer3(delay);
                        }
                        if (m_durActivated)
                        {
                            TimeSpan delay = reader.ReadTimeSpan();
                            DoTimer2(delay);
                        }

                        m_ShowContainerStatic = reader.ReadItem() as Static;
                        m_Duration = reader.ReadTimeSpan();
                        m_UniqueId = reader.ReadString();
                        m_HomeRangeIsRelative = reader.ReadBool();
                        goto case 0;
                    }
                case 0:
                    {
                        m_Name = reader.ReadString();
                        // backward compatibility with old name storage
                        if (!string.IsNullOrEmpty(m_Name)) Name = m_Name;
                        m_X = reader.ReadInt();
                        m_Y = reader.ReadInt();
                        m_Width = reader.ReadInt();
                        m_Height = reader.ReadInt();

                        //we HAVE to check if the area is even or if coordinates point to the original spawner, otherwise it's custom area!
                        if (m_Width == m_Height && (m_Width % 2) == 0 && (m_X + m_Width / 2) == X && (m_Y + m_Height / 2) == Y)
                            m_SpawnRange = m_Width / 2;
                        else
                            m_SpawnRange = -1;

                        if (!haveproximityrange)
                        {
                            m_ProximityRange = -1;
                        }

                        m_WayPoint = reader.ReadItem() as WayPoint;
                        m_Group = reader.ReadBool();
                        m_MinDelay = reader.ReadTimeSpan();
                        m_MaxDelay = reader.ReadTimeSpan();
                        m_Count = reader.ReadInt();
                        m_Team = reader.ReadInt();
                        m_HomeRange = reader.ReadInt();
                        m_Running = reader.ReadBool();

                        if (m_Running)
                        {
                            TimeSpan delay = reader.ReadTimeSpan();
                            DoTimer(delay);
                        }

                        // Read in the size of the spawn object list
                        int SpawnListSize = reader.ReadInt();
                        m_SpawnObjects = new List<SpawnObject>(SpawnListSize);
                        for (int i = 0; i < SpawnListSize; ++i)
                        {
                            string TypeName = reader.ReadString();
                            int TypeMaxCount = reader.ReadInt();

                            SpawnObject TheSpawnObject = new SpawnObject(TypeName, TypeMaxCount);

                            m_SpawnObjects.Add(TheSpawnObject);

                            string typeName = BaseXmlSpawner.ParseObjectType(TypeName);

                            if (typeName == null || SpawnerType.GetType(typeName) == null && !BaseXmlSpawner.IsTypeOrItemKeyword(typeName) && typeName.IndexOf('{') == -1 && !typeName.StartsWith("*") && !typeName.StartsWith("#"))
                            {
                                if (m_WarnTimer == null)
                                    m_WarnTimer = new WarnTimer2();

                                m_WarnTimer.Add(Location, Map, TypeName);

                                status_str = "invalid type: " + typeName;
                            }

                            // Read in the number of spawns already
                            int SpawnedCount = reader.ReadInt();

                            TheSpawnObject.SpawnedObjects = new List<object>(SpawnedCount);

                            for (int x = 0; x < SpawnedCount; ++x)
                            {
                                int serial = reader.ReadInt();
                                if (serial < -1)
                                {
                                    // minusone is reserved for unknown types by default
                                    //  minustwo on is used for referencing keyword tags
                                    int tagserial = -1 * (serial + 2);
                                    // get the tag with that serial and add it
                                    BaseXmlSpawner.KeywordTag t = BaseXmlSpawner.GetFromTagList(this, tagserial);
                                    if (t != null)
                                    {
                                        TheSpawnObject.SpawnedObjects.Add(t);
                                    }
                                }
                                else
                                {
                                    IEntity e = World.FindEntity(serial);

                                    if (e != null)
                                        TheSpawnObject.SpawnedObjects.Add(e);
                                }
                            }
                        }

                        // now have to reintegrate the later version spawnobject information into the earlier version desered objects
                        if (hasnewobjectinfo && tmpSpawnListSize == SpawnListSize)
                        {
                            for (int i = 0; i < SpawnListSize; ++i)
                            {
                                SpawnObject so = m_SpawnObjects[i];
                                so.SubGroup = tmpSubGroup[i];
                                so.SequentialResetTime = tmpSequentialResetTime[i];
                                so.SequentialResetTo = tmpSequentialResetTo[i];
                                so.KillsNeeded = tmpKillsNeeded[i];
                                so.RequireSurface = tmpRequireSurface[i];

                                bool restrictkills = false;
                                bool clearadvance = true;
                                double mind = -1;
                                double maxd = -1;
                                DateTime nextspawn = DateTime.MinValue;

                                restrictkills = tmpRestrictKillsToSubgroup[i];
                                clearadvance = tmpClearOnAdvance[i];
                                mind = tmpMinDelay[i];
                                maxd = tmpMaxDelay[i];
                                nextspawn = tmpNextSpawn[i];

                                so.RestrictKillsToSubgroup = restrictkills;
                                so.ClearOnAdvance = clearadvance;
                                so.MinDelay = mind;
                                so.MaxDelay = maxd;
                                so.NextSpawn = nextspawn;

                                bool disablespawn = false;
                                disablespawn = tmpDisableSpawn[i];
                                so.Disabled = disablespawn;

                                int packrange = -1;
                                packrange = tmpPackRange[i];
                                so.PackRange = packrange;

                                int spawnsper = 1;
                                spawnsper = tmpSpawnsPer[i];
                                so.SpawnsPerTick = spawnsper;
                            }
                        }

                        break;
                    }
            }

            if (m_RegionName != null)
            {
                Timer.DelayCall(delegate { if (!Deleted && m_RegionName != null) RegionName = m_RegionName; });
            }
        }

        internal string GetSerializedObjectList()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                if (sb.Length > 0)
                {
                    sb.Append(':'); // ':' Separates multiple object types
                }

                sb.Append($"{so.TypeName}={so.ActualMaxCount}"); // '=' separates object name from maximum amount
            }

            return sb.ToString();
        }

        internal string GetSerializedObjectList2()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (var index = 0; index < m_SpawnObjects.Count; index++)
            {
                SpawnObject so = m_SpawnObjects[index];

                if (sb.Length > 0)
                {
                    sb.Append(":OBJ="); // Separates multiple object types
                }

                sb.Append($"{so.TypeName}:MX={so.ActualMaxCount}:SB={so.SubGroup}:RT={so.SequentialResetTime}:TO={so.SequentialResetTo}:KL={so.KillsNeeded}:RK={(so.RestrictKillsToSubgroup ? 1 : 0)}:CA={(so.ClearOnAdvance ? 1 : 0)}:DN={so.MinDelay}:DX={so.MaxDelay}:SP={so.SpawnsPerTick}:PR={so.PackRange}");
            }

            return sb.ToString();
        }
        #endregion

        #region Spawn classes
        public class SpawnObject
        {
            public bool Available; // temporary variable used to calculate weighted spawn probabilities

            public List<object> SpawnedObjects;
            public string[] PropertyArgs;
            public double SequentialResetTime;
            public int EntryOrder;  // used for sorting
            public bool RequireSurface = true;
            public DateTime NextSpawn;
            public bool SpawnedThisTick;

            // these are externally accessible to the SETONSPAWNENTRY keyword
            public string TypeName { get; set; }

            public int MaxCount
            {
                get
                {
                    if (Disabled)
                    {
                        return 0;
                    }

                    return ActualMaxCount;
                }

                set => ActualMaxCount = value;
            }

            public int ActualMaxCount { get; set; }
            public int SubGroup { get; set; }
            public int SpawnsPerTick { get; set; } = 1;
            public int SequentialResetTo { get; set; }
            public int KillsNeeded { get; set; }
            public bool RestrictKillsToSubgroup { get; set; } 
            public bool ClearOnAdvance { get; set; } = true;
            public double MinDelay { get; set; } = -1;
            public double MaxDelay { get; set; } = -1;
            public bool Disabled { get; set; } = false;
            public bool Ignore { get; set; } = false;
            public int PackRange { get; set; } = -1;

            // command loggable constructor
            public SpawnObject(Mobile from, XmlSpawner spawner, string name, int maxamount)
            {
                if (from != null && spawner != null)
                {
                    bool found = false;
                    // go through the current spawner objects and see if this is a new entry
                    if (spawner.m_SpawnObjects != null)
                    {
                        for (int i = 0; i < spawner.m_SpawnObjects.Count; i++)
                        {
                            SpawnObject s = spawner.m_SpawnObjects[i];
                            if (s != null && s.TypeName == name)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        CommandLogging.WriteLine(from, "{0} {1} added to XmlSpawner {2} '{3}' [{4}, {5}] ({6}) : {7}", from.AccessLevel, CommandLogging.Format(from), spawner.Serial, spawner.Name, spawner.GetWorldLocation().X, spawner.GetWorldLocation().Y, spawner.Map, name);
                    }
                }

                TypeName = name;
                MaxCount = maxamount;
                SubGroup = 0;
                SequentialResetTime = 0;
                SequentialResetTo = 0;
                KillsNeeded = 0;
                RestrictKillsToSubgroup = false;
                ClearOnAdvance = true;
                SpawnedObjects = new List<object>();
            }

            public SpawnObject(string name, int maxamount)
            {
                TypeName = name;
                MaxCount = maxamount;
                SubGroup = 0;
                SequentialResetTime = 0;
                SequentialResetTo = 0;
                KillsNeeded = 0;
                RestrictKillsToSubgroup = false;
                ClearOnAdvance = true;
                SpawnedObjects = new List<object>();
            }

            public SpawnObject(string name, int maxamount, int subgroup, double sequentialresettime, int sequentialresetto, int killsneeded,
                bool restrictkills, bool clearadvance, double mindelay, double maxdelay, int spawnsper, int packrange)
            {
                TypeName = name;
                MaxCount = maxamount;
                SubGroup = subgroup;
                SequentialResetTime = sequentialresettime;
                SequentialResetTo = sequentialresetto;
                KillsNeeded = killsneeded;
                RestrictKillsToSubgroup = restrictkills;
                ClearOnAdvance = clearadvance;
                MinDelay = mindelay;
                MaxDelay = maxdelay;
                SpawnsPerTick = spawnsper;
                PackRange = packrange;
                SpawnedObjects = new List<object>();
            }

            internal static string GetParm(string str, string separator)
            {
                // find the parm separator in the string
                // then look for the termination at the ':'  or end of string
                // and return the stuff between
                string[] arg = BaseXmlSpawner.SplitString(str, separator);
                //should be 2 args
                if (arg.Length > 1)
                {
                    string[] parm = arg[1].Split(':'); // look for the end of parm terminator (could also be eol)
                    if (parm.Length > 0)
                    {
                        return parm[0];
                    }
                }

                return null;
            }

            internal static SpawnObject[] LoadSpawnObjectsFromString(string ObjectList)
            {
                // Clear the spawn object list
                List<SpawnObject> NewSpawnObjects = new List<SpawnObject>();

                if (!string.IsNullOrEmpty(ObjectList))
                {
                    // Split the string based on the object separator first ':'
                    string[] SpawnObjectList = ObjectList.Split(':');

                    // Parse each item in the array
                    for (var index = 0; index < SpawnObjectList.Length; index++)
                    {
                        string s = SpawnObjectList[index];
                        // Split the single spawn object item by the max count '='
                        string[] SpawnObjectDetails = s.Split('=');

                        // Should be two entries
                        if (SpawnObjectDetails.Length == 2)
                        {
                            // Validate the information

                            // Make sure the spawn object name part has a valid length
                            if (SpawnObjectDetails[0].Length > 0)
                            {
                                // Make sure the max count part has a valid length
                                if (SpawnObjectDetails[1].Length > 0)
                                {
                                    int maxCount = 1;

                                    try
                                    {
                                        maxCount = int.Parse(SpawnObjectDetails[1]);
                                    }
                                    catch (Exception)
                                    {
                                        // Something went wrong, leave the default amount }
                                    }

                                    // Create the spawn object and store it in the array list
                                    SpawnObject so = new SpawnObject(SpawnObjectDetails[0], maxCount);
                                    NewSpawnObjects.Add(so);
                                }
                            }
                        }
                    }
                }

                return NewSpawnObjects.ToArray();
            }

            internal static SpawnObject[] LoadSpawnObjectsFromString2(string ObjectList)
            {
                // Clear the spawn object list
                List<SpawnObject> NewSpawnObjects = new List<SpawnObject>();

                // spawn object definitions will take the form typestring:MX=int:SB=int:RT=double:TO=int:KL=int
                // or typestring:MX=int:SB=int:RT=double:TO=int:KL=int:OBJ=typestring...
                if (!string.IsNullOrEmpty(ObjectList))
                {
                    string[] SpawnObjectList = BaseXmlSpawner.SplitString(ObjectList, ":OBJ=");

                    // Parse each item in the array
                    for (var index = 0; index < SpawnObjectList.Length; index++)
                    {
                        string s = SpawnObjectList[index];
                        // at this point each spawn string will take the form typestring:MX=int:SB=int:RT=double:TO=int:KL=int
                        // Split the single spawn object item by the max count to get the typename and the remaining parms
                        string[] SpawnObjectDetails = BaseXmlSpawner.SplitString(s, ":MX=");

                        // Should be two entries
                        if (SpawnObjectDetails.Length == 2)
                        {
                            // Validate the information

                            // Make sure the spawn object name part has a valid length
                            if (SpawnObjectDetails[0].Length > 0)
                            {
                                // Make sure the parm part has a valid length
                                if (SpawnObjectDetails[1].Length > 0)
                                {
                                    // now parse out the parms
                                    // MaxCount
                                    string parmstr = GetParm(s, ":MX=");
                                    int maxCount = 1;
                                    try
                                    {
                                        maxCount = int.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // SubGroup
                                    parmstr = GetParm(s, ":SB=");

                                    int subGroup = 0;
                                    try
                                    {
                                        subGroup = int.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // SequentialSpawnResetTime
                                    parmstr = GetParm(s, ":RT=");
                                    double resetTime = 0;
                                    try
                                    {
                                        resetTime = double.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // SequentialSpawnResetTo
                                    parmstr = GetParm(s, ":TO=");
                                    int resetTo = 0;
                                    try
                                    {
                                        resetTo = int.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // KillsNeeded
                                    parmstr = GetParm(s, ":KL=");
                                    int killsNeeded = 0;
                                    try
                                    {
                                        killsNeeded = int.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // RestrictKills
                                    parmstr = GetParm(s, ":RK=");
                                    bool restrictKills = false;
                                    if (parmstr != null)
                                        try
                                        {
                                            restrictKills = int.Parse(parmstr) == 1;
                                        }
                                        catch
                                        {
                                        }

                                    // ClearOnAdvance
                                    parmstr = GetParm(s, ":CA=");
                                    var clearAdvance = true;
                                    // if kills needed is zero, then set CA to false by default.  This maintains consistency with the
                                    // previous default behavior for old spawn specs that havent specified CA
                                    if (killsNeeded == 0)
                                        clearAdvance = false;
                                    if (parmstr != null)
                                        try
                                        {
                                            clearAdvance = int.Parse(parmstr) == 1;
                                        }
                                        catch
                                        {
                                        }

                                    // MinDelay
                                    parmstr = GetParm(s, ":DN=");
                                    double minD = -1;
                                    try
                                    {
                                        minD = double.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // MaxDelay
                                    parmstr = GetParm(s, ":DX=");
                                    double maxD = -1;
                                    try
                                    {
                                        maxD = double.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // SpawnsPerTick
                                    parmstr = GetParm(s, ":SP=");
                                    int spawnsPer = 1;
                                    try
                                    {
                                        spawnsPer = int.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // PackRange
                                    parmstr = GetParm(s, ":PR=");
                                    int packRange = -1;
                                    try
                                    {
                                        packRange = int.Parse(parmstr);
                                    }
                                    catch
                                    {
                                    }

                                    // Create the spawn object and store it in the array list
                                    SpawnObject so = new SpawnObject(SpawnObjectDetails[0], maxCount, subGroup,
                                        resetTime, resetTo, killsNeeded, restrictKills, clearAdvance, minD, maxD,
                                        spawnsPer, packRange);

                                    NewSpawnObjects.Add(so);
                                }
                            }
                        }
                    }
                }

                return NewSpawnObjects.ToArray();
            }
        }

        #endregion
    }
}
