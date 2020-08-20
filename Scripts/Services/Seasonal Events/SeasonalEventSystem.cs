using Server.Commands;
using Server.Engines.Fellowship;
using Server.Engines.JollyRoge;
using Server.Engines.Khaldun;
using Server.Engines.RisingTide;
using Server.Engines.SorcerersDungeon;
using Server.Engines.TreasuresOfDoom;
using Server.Engines.ArtisanFestival;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Engines.SeasonalEvents
{
    public enum EventType
    {
        TreasuresOfTokuno,
        VirtueArtifacts,
        TreasuresOfKotlCity,
        SorcerersDungeon,
        TreasuresOfDoom,
        TreasuresOfKhaldun,
        KrampusEncounter,
        RisingTide,
        Fellowship,
        JollyRoger,
        ArtisanFestival
    }

    public enum EventStatus
    {
        Inactive,
        Active,
        Seasonal,
    }

    public interface ISeasonalEventObject
    {
        EventType EventType { get; }
        bool EventActive { get; }
    }

    public class SeasonalEventSystem
    {
        public static string FilePath = Path.Combine("Saves/Misc", "SeasonalEvents.bin");

        public static List<SeasonalEvent> Entries { get; set; } = new List<SeasonalEvent>();

        public static void Configure()
        {
            LoadEntries();

            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
            EventSink.AfterWorldSave += AfterSafe;

            CommandSystem.Register("SeasonSystemGump", AccessLevel.Administrator, SendGump);
        }

        public static void LoadEntries()
        {
            Entries.Add(new SeasonalEvent(EventType.TreasuresOfTokuno, "Treasures of Tokuno", EventStatus.Inactive));
            Entries.Add(new SeasonalEvent(EventType.VirtueArtifacts, "Virtue Artifacts", EventStatus.Active));
            Entries.Add(new SeasonalEvent(EventType.TreasuresOfKotlCity, "Treasures of Kotl", EventStatus.Inactive, 10, 1, 60));
            Entries.Add(new SorcerersDungeonEvent(EventType.SorcerersDungeon, "Sorcerer's Dungeon", EventStatus.Seasonal, 10, 1, 60));
            Entries.Add(new SeasonalEvent(EventType.TreasuresOfDoom, "Treasures of Doom", EventStatus.Seasonal, 10, 1, 60));
            Entries.Add(new SeasonalEvent(EventType.TreasuresOfKhaldun, "Treasures of Khaldun", EventStatus.Seasonal, 10, 1, 60));
            Entries.Add(new SeasonalEvent(EventType.KrampusEncounter, "Krampus Encounter", EventStatus.Seasonal, 12, 1, 60));
            Entries.Add(new SeasonalEvent(EventType.RisingTide, "Rising Tide", EventStatus.Active));
            Entries.Add(new SeasonalEvent(EventType.Fellowship, "Fellowship", EventStatus.Inactive));
            Entries.Add(new SeasonalEvent(EventType.JollyRoger, "Jolly Roger", EventStatus.Inactive));
            Entries.Add(new ArtisanFestivalEvent(EventType.ArtisanFestival, "Artisan Festival", EventStatus.Seasonal, 12, 1, 30));
        }

        [Usage("SeasonSystemGump")]
        [Description("Displays a menu to configure various seasonal systems.")]
        public static void SendGump(CommandEventArgs e)
        {
            if (e.Mobile is PlayerMobile)
            {
                BaseGump.SendGump(new SeasonalEventGump((PlayerMobile)e.Mobile));
            }
        }

        public static bool IsActive(EventType type)
        {
            SeasonalEvent entry = GetEvent(type);

            if (entry != null)
            {
                return entry.IsActive();
            }

            return false;
        }

        public static bool IsRunning(EventType type)
        {
            SeasonalEvent entry = GetEvent(type);

            if (entry != null)
            {
                return entry.Running;
            }

            return false;
        }

        public static SeasonalEvent GetEvent(EventType type)
        {
            return Entries.FirstOrDefault(e => e.EventType == type);
        }

        public static void OnToTDeactivated(Mobile from)
        {
            SeasonalEvent entry = GetEvent(EventType.TreasuresOfTokuno);

            if (entry != null)
            {
                entry.Status = EventStatus.Inactive;

                if (from is PlayerMobile)
                {
                    BaseGump.SendGump(new SeasonalEventGump((PlayerMobile)from));
                }
            }
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(0);

                    writer.Write(Entries.Count);

                    for (int i = 0; i < Entries.Count; i++)
                    {
                        writer.Write((int)Entries[i].EventType);
                        Entries[i].Serialize(writer);
                    }
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                FilePath,
                reader =>
                {
                    reader.ReadInt(); // version

                    int count = reader.ReadInt();

                    for (int i = 0; i < count; i++)
                    {
                        SeasonalEvent entry = GetEvent((EventType)reader.ReadInt());
                        entry.Deserialize(reader);
                    }
                });
        }

        public static void AfterSafe(AfterWorldSaveEventArgs e)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].CheckEnabled();
            }
        }
    }

    [PropertyObject]
    public class SeasonalEvent
    {
        private EventStatus _Status;

        [CommandProperty(AccessLevel.Administrator)]
        public EventStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                EventStatus old = _Status;

                _Status = value;

                if (old != _Status)
                {
                    CheckEnabled();
                }
            }
        }

        [CommandProperty(AccessLevel.Administrator)]
        public string Name { get; private set; }

        [CommandProperty(AccessLevel.Administrator)]
        public EventType EventType { get; private set; }

        [CommandProperty(AccessLevel.Administrator)]
        public int MonthStart { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public int DayStart { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public int Duration { get; set; }

        public bool Running { get; private set; }

        public SeasonalEvent(EventType type, string name, EventStatus status)
        {
            EventType = type;
            Name = name;
            _Status = status;
            MonthStart = 1;
            DayStart = 1;
            Duration = 365;
        }

        public SeasonalEvent(EventType type, string name, EventStatus status, int month, int day, int duration)
        {
            EventType = type;
            Name = name;
            _Status = status;
            MonthStart = month;
            DayStart = day;
            Duration = duration;
        }

        /// <summary>
        /// Dynamically checks if this event is active or not, based on time of year/override
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            // ToT uses its own system, this just reads it
            if (EventType == EventType.TreasuresOfTokuno)
            {
                return TreasuresOfTokuno.DropEra != TreasuresOfTokunoEra.None;
            }

            switch (Status)
            {
                default:
                    {
                        return false;
                    }
                case EventStatus.Active:
                    {
                        return true;
                    }
                case EventStatus.Seasonal:
                    {
                        if (Duration >= 365)
                            return true;

                        DateTime now = DateTime.Now;
                        DateTime starts = new DateTime(now.Year, MonthStart, DayStart, 0, 0, 0);

                        return now > starts && now < starts + TimeSpan.FromDays(Duration);
                    }
            }
        }

        /*public void OnStatusChange()
        {
            switch (EventType)
            {
                case EventType.TreasuresOfDoom:
                    TreasuresOfDoomGeneration.CheckEnabled();
                    break;
                case EventType.TreasuresOfKhaldun:
                    TreasuresOfKhaldunGeneration.CheckEnabled();
                    break;
                case EventType.SorcerersDungeon:
                    SorcerersDungeonGenerate.CheckEnabled();
                    break;
                case EventType.KrampusEncounter:
                    KrampusEncounter.CheckEnabled();
                    break;
                case EventType.RisingTide:
                    RisingTideGeneration.CheckEnabled();
                    break;
                case EventType.Fellowship:
                    ForsakenFoesGeneration.CheckEnabled();
                    break;
                case EventType.JollyRoger:
                    JollyRogerGeneration.CheckEnabled();
                    break;
                case EventType.ArtisanFestival:
                    ArtisanFestivalGeneration.CheckEnabled();
                    break;
            }

            Running = IsActive();
        }*/

        public virtual void CheckEnabled()
        {
            if (Running && !IsActive())
            {
                Utility.WriteConsoleColor(ConsoleColor.Green, string.Format("Disabling {0}", Name));

                Remove();
            }
            else if (!Running && IsActive())
            {
                Utility.WriteConsoleColor(ConsoleColor.Green, string.Format("Enabling {1}", Name));

                Generate();
            }

            Running = IsActive();
        }

        protected virtual void Generate()
        {
        }

        protected virtual void Remove()
        {
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(Running);

            writer.Write((int)_Status);

            writer.Write(MonthStart);
            writer.Write(DayStart);
            writer.Write(Duration);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            var v = reader.ReadInt(); // version

            switch (v)
            {
                case 1:
                    Running = reader.ReadBool();
                    goto case 0;
                case 0:
                    _Status = (EventStatus)reader.ReadInt();

                    MonthStart = reader.ReadInt();
                    DayStart = reader.ReadInt();
                    Duration = reader.ReadInt();
                    break;
            }

            if (v == 0)
            {
                Running = IsActive();
                InheritInsertion = true;
            }

            // TODO: Remvove this
            if (v == 1)
            {
                InheritInsertion = true;
            }
        }

        protected bool InheritInsertion = false;
    }
}
