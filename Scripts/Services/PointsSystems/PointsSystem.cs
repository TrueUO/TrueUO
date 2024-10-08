using Server.Engines.ArenaSystem;
using Server.Engines.CityLoyalty;
using Server.Engines.SorcerersDungeon;
using Server.Engines.Fellowship;
using Server.Engines.JollyRoger;
using Server.Engines.VvV;
using Server.Misc;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.Points
{
    public enum PointsType
    {
        None = -1,

        QueensLoyalty,
        VoidPool,
        DespiseCrystals,
        ShameCrystals,
        CasinoData,
        CityTrading,

        // City Loyalty System
        Moonglow,
        Britain,
        Jhelom,
        Yew,
        Minoc,
        Trinsic,
        SkaraBrae,
        NewMagincia,
        Vesper,
        // End City Loyalty System

        Blackthorn,
        CleanUpBritannia,
        ViceVsVirtue,
        TreasuresOfKotlCity,
        PVPArena,
        Khaldun,
        Doom,
        SorcerersDungeon,
        RisingTide,
        GauntletPoints,
        TOT,
        VAS,
        FellowshipData,
        JollyRogerData
    }

    public abstract class PointsSystem
    {
        public static string FilePath = Path.Combine("Saves/PointsSystem", "Persistence.bin");

        public List<PointsEntry> PlayerTable { get; set; }

        public abstract TextDefinition Name { get; }
        public abstract PointsType Loyalty { get; }
        public abstract bool AutoAdd { get; }
        public abstract double MaxPoints { get; }

        public virtual bool ShowOnLoyaltyGump => true;

        public PointsSystem()
        {
            PlayerTable = new List<PointsEntry>();

            AddSystem(this);
        }

        private static void AddSystem(PointsSystem system)
        {
            PointsSystem first = null;

            for (var index = 0; index < Systems.Count; index++)
            {
                var s = Systems[index];

                if (s.Loyalty == system.Loyalty)
                {
                    first = s;
                    break;
                }
            }

            if (first != null)
            {
                return;
            }

            Systems.Add(system);
        }

        public virtual void ProcessKill(Mobile victim, Mobile damager)
        {
        }

        public virtual void ProcessQuest(Mobile from, Type quest)
        {
        }

        public virtual void ConvertFromOldSystem(PlayerMobile from, double points)
        {
            PointsEntry entry = GetEntry(from);

            if (entry == null)
            {
                if (points > MaxPoints)
                {
                    points = MaxPoints;
                }

                AddEntry(from, true);
                GetEntry(from).Points = points;

                Utility.PushColor(ConsoleColor.Green);
                Console.WriteLine("Converted {0} points for {1} to {2}!", (int)points, from.Name, GetType().Name);
                Utility.PopColor();
            }
        }

        public virtual void AwardPoints(Mobile from, double points, bool quest = false, bool message = true)
        {
            if (!(from is PlayerMobile) || points <= 0)
            {
                return;
            }

            PointsEntry entry = GetEntry(from);

            if (entry != null)
            {
                double old = entry.Points;

                SetPoints((PlayerMobile)from, Math.Min(MaxPoints, entry.Points + points));
                SendMessage((PlayerMobile)from, old, points, quest);
            }
        }

        public void SetPoints(PlayerMobile pm, double points)
        {
            PointsEntry entry = GetEntry(pm);

            if (entry != null)
                entry.Points = points;
        }

        public virtual void SendMessage(PlayerMobile from, double old, double points, bool quest)
        {
            if (quest)
                from.SendLocalizedMessage(1113719, ((int)points).ToString(), 0x26); //You have received ~1_val~ loyalty points as a reward for completing the quest. 
            else
                from.SendLocalizedMessage(1115920, $"{Name}\t{((int) points).ToString()}");  // Your loyalty to ~1_GROUP~ has increased by ~2_AMOUNT~;Original
        }

        public virtual bool DeductPoints(Mobile from, double points, bool message = false)
        {
            PointsEntry entry = GetEntry(from);

            if (entry == null || entry.Points < points)
            {
                return false;
            }

            entry.Points -= points;

            if (message)
            {
                from.SendLocalizedMessage(1115921, $"{Name}\t{((int) points).ToString()}"); // Your loyalty to ~1_GROUP~ has decreased by ~2_AMOUNT~;Original
            }

            return true;
        }

        public virtual void OnPlayerAdded(PlayerMobile pm)
        {
        }

        public virtual PointsEntry AddEntry(PlayerMobile pm, bool existed = false)
        {
            PointsEntry entry = GetSystemEntry(pm);

            if (!PlayerTable.Contains(entry))
            {
                PlayerTable.Add(entry);

                if (!existed)
                    OnPlayerAdded(pm);
            }

            return entry;
        }

        public double GetPoints(Mobile from)
        {
            PointsEntry entry = GetEntry(from);

            if (entry != null)
                return entry.Points;

            return 0.0;
        }

        public virtual TextDefinition GetTitle(PlayerMobile from)
        {
            return null;
        }

        public PointsEntry GetEntry(Mobile from, bool create = false)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
            {
                return null;
            }

            PointsEntry entry = null;

            for (var index = 0; index < PlayerTable.Count; index++)
            {
                var e = PlayerTable[index];

                if (e.Player == pm)
                {
                    entry = e;
                    break;
                }
            }

            if (entry == null && (create || AutoAdd))
            {
                entry = AddEntry(pm);
            }

            return entry;
        }

        public TEntry GetPlayerEntry<TEntry>(Mobile mobile, bool create = false) where TEntry : PointsEntry
        {
            PlayerMobile pm = mobile as PlayerMobile;

            if (pm == null)
            {
                return null;
            }

            PointsEntry first = null;

            for (var index = 0; index < PlayerTable.Count; index++)
            {
                var p = PlayerTable[index];

                if (p.Player == pm)
                {
                    first = p;
                    break;
                }
            }

            TEntry e = first as TEntry;

            if (e == null && (AutoAdd || create))
            {
                e = AddEntry(pm) as TEntry;
            }

            return e;
        }

        /// <summary>
        /// Override this if you are going to derive Points Entry into a bigger and badder class!
        /// </summary>
        /// <param name="pm"></param>
        /// <returns></returns>
        public virtual PointsEntry GetSystemEntry(PlayerMobile pm)
        {
            return new PointsEntry(pm);
        }

        public int Version { get; set; }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(2);

            writer.Write(PlayerTable.Count);

            for (var index = 0; index < PlayerTable.Count; index++)
            {
                var entry = PlayerTable[index];

                writer.Write(entry.Player);
                entry.Serialize(writer);
            }
        }

        public virtual void Deserialize(GenericReader reader)
        {
            Version = reader.ReadInt();

            switch (Version)
            {
                case 2: // added serialize/deserialize in all base classes. Poor implementation on my part, should have had from the get-go
                case 1:
                case 0:
                    {
                        int count = reader.ReadInt();

                        for (int i = 0; i < count; i++)
                        {
                            PlayerMobile player = reader.ReadMobile() as PlayerMobile;
                            PointsEntry entry = GetSystemEntry(player);

                            if (Version > 0)
                                entry.Deserialize(reader);
                            else
                                entry.Points = reader.ReadDouble();

                            if (player != null)
                            {
                                if (!PlayerTable.Contains(entry))
                                {
                                    PlayerTable.Add(entry);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        #region Static Methods and Accessors
        public static PointsSystem GetSystemInstance(PointsType t)
        {
            for (var index = 0; index < Systems.Count; index++)
            {
                var s = Systems[index];

                if (s.Loyalty == t)
                {
                    return s;
                }
            }

            return null;
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(2);

                    writer.Write(Systems.Count);
                    for (var index = 0; index < Systems.Count; index++)
                    {
                        var s = Systems[index];

                        writer.Write((int) s.Loyalty);
                        s.Serialize(writer);
                    }
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                FilePath,
                reader =>
                {
                    int version = reader.ReadInt();

                    if (version < 2)
                        reader.ReadBool();

                    PointsType loaded = PointsType.None;

                    int count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            PointsType type = (PointsType)reader.ReadInt();
                            loaded = type;
                            PointsSystem s = GetSystemInstance(type);

                            s.Deserialize(reader);
                        }
                        catch
                        {
                            throw new Exception($"Points System Failed Load: {loaded.ToString()} Last Loaded...");
                        }
                    }
                });
        }

        public static List<PointsSystem> Systems { get; set; }

        public static QueensLoyalty QueensLoyalty { get; set; }
        public static VoidPool VoidPool { get; set; }
        public static DespiseCrystals DespiseCrystals { get; set; }
        public static ShameCrystals ShameCrystals { get; set; }
        public static CasinoData CasinoData { get; set; }
        public static BlackthornData Blackthorn { get; set; }
        public static CleanUpBritanniaData CleanUpBritannia { get; set; }
        public static ViceVsVirtueSystem ViceVsVirtue { get; set; }
        public static KotlCityData TreasuresOfKotlCity { get; set; }
        public static PVPArenaSystem ArenaSystem { get; set; }
        public static KhaldunData Khaldun { get; set; }
        public static DoomData TreasuresOfDoom { get; set; }
        public static SorcerersDungeonData SorcerersDungeon { get; set; }
        public static RisingTide RisingTide { get; set; }
        public static DoomGauntlet DoomGauntlet { get; set; }
        public static TreasuresOfTokuno TreasuresOfTokuno { get; set; }
        public static VirtueArtifactsSystem VirtueArtifacts { get; set; }
        public static FellowshipData FellowshipData { get; set; }
        public static JollyRogerData JollyRogerData { get; set; }

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;

            Systems = new List<PointsSystem>();

            QueensLoyalty = new QueensLoyalty();
            VoidPool = new VoidPool();
            DespiseCrystals = new DespiseCrystals();
            ShameCrystals = new ShameCrystals();
            CasinoData = new CasinoData();
            Blackthorn = new BlackthornData();
            CleanUpBritannia = new CleanUpBritanniaData();
            ViceVsVirtue = new ViceVsVirtueSystem();
            TreasuresOfKotlCity = new KotlCityData();

            CityLoyaltySystem.ConstructSystems();
            ArenaSystem = new PVPArenaSystem();
            Khaldun = new KhaldunData();
            TreasuresOfDoom = new DoomData();
            SorcerersDungeon = new SorcerersDungeonData();
            RisingTide = new RisingTide();
            DoomGauntlet = new DoomGauntlet();
            TreasuresOfTokuno = new TreasuresOfTokuno();
            VirtueArtifacts = new VirtueArtifactsSystem();
            FellowshipData = new FellowshipData();
            JollyRogerData = new JollyRogerData();
        }

        public static void OnKilledBy(BaseCreature killed, Mobile killer)
        {
            for (var index = 0; index < Systems.Count; index++)
            {
                var s = Systems[index];

                s.ProcessKill(killed, killer);
            }
        }

        public static void CompleteQuest(PlayerMobile pm, Type type)
        {
            for (var index = 0; index < Systems.Count; index++)
            {
                var s = Systems[index];

                s.ProcessQuest(pm, type);
            }
        }
        #endregion
    }

    public class PointsEntry
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public PlayerMobile Player { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Points { get; set; }

        public PointsEntry(PlayerMobile pm)
        {
            Player = pm;
        }

        public PointsEntry(PlayerMobile pm, double points)
        {
            Player = pm;
            Points = points;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is PointsEntry))
            {
                return false;
            }

            return ((PointsEntry)o).Player == Player;
        }

        public override int GetHashCode()
        {
            if (Player != null)
            {
                return Player.GetHashCode();
            }

            return base.GetHashCode();
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(0);
            writer.Write(Player);
            writer.Write(Points);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadInt();
            Player = reader.ReadMobile() as PlayerMobile;
            Points = reader.ReadDouble();
        }
    }
}
