using Server.Commands;
using Server.ContextMenus;
using Server.Engines.PartySystem;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Engines.Shadowguard
{
    [Flags]
    public enum EncounterType
    {
        Bar = 0x00000001,
        Orchard = 0x00000002,
        Armory = 0x00000004,
        Fountain = 0x00000008,
        Belfry = 0x00000010,
        Roof = 0x00000020,

        Required = Bar | Orchard | Armory | Fountain | Belfry
    }

    [DeleteConfirm("Are you sure you want to delete this? Deleting this will delete any saved encounter data your players have.")]
    public class ShadowguardController : Item
    {
        public static readonly TimeSpan ReadyDuration = TimeSpan.FromSeconds(Config.Get("Shadowguard.ReadyDuration", 30));
        public static bool RandomInstances = Config.Get("Shadowguard.RandomizeInstances", false);

        public static ShadowguardController Instance { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D KickLocation { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Lobby { get; set; }

        public Dictionary<Mobile, EncounterType> Table { get; set; }
        public List<ShadowguardEncounter> Encounters { get; set; }
        public Dictionary<Mobile, EncounterType> Queue { get; set; }
        public List<BaseAddon> Addons { get; set; }
        public List<ShadowguardInstance> Instances { get; set; }

        public Timer Timer { get; set; }

        public override int LabelNumber => 1156235;  // An Enchanting Crystal Ball

        public static void Initialize()
        {
            CommandSystem.Register("AddController", AccessLevel.Administrator, e =>
                {
                    if (Instance == null)
                    {
                        ShadowguardController controller = new ShadowguardController();
                        controller.MoveToWorld(new Point3D(501, 2192, 50), Map.TerMur);

                        e.Mobile.SendMessage("Shadowguard controller setup!");
                    }
                    else
                    {
                        e.Mobile.SendMessage("A Shadowguard controller already exists!");
                    }
                });

            CommandSystem.Register("CompleteAllRooms", AccessLevel.GameMaster, e =>
                {
                    if (Instance.Table == null)
                        Instance.Table = new Dictionary<Mobile, EncounterType>();

                    Instance.Table[e.Mobile] = EncounterType.Bar | EncounterType.Orchard | EncounterType.Armory | EncounterType.Fountain | EncounterType.Belfry;
                });
        }

        public void InitializeInstances()
        {
            Instances = new List<ShadowguardInstance>();

            for (int i = 0; i < _CenterPoints.Length; i++)
            {
                Instances.Add(new ShadowguardInstance(this, _CenterPoints[i], _EncounterBounds[i], i));
            }
        }

        public ShadowguardController()
            : base(0x468B)
        {
            Instance = this;

            Weight = 0;

            KickLocation = new Point3D(505, 2192, 25);
            Lobby = new Rectangle2D(497, 2153, 50, 80);

            Encounters = new List<ShadowguardEncounter>();
            Addons = new List<BaseAddon>();
            Queue = new Dictionary<Mobile, EncounterType>();

            InitializeInstances();

            Movable = false;

            StartTimer();
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            if (from.Alive)
            {
                list.Add(new ExitQueueEntry(from, this));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from is PlayerMobile mobile && mobile.InRange(Location, 3))
            {
                mobile.SendGump(new ShadowguardGump(mobile));
            }
        }

        public void OnTick()
        {
            if (Encounters == null)
            {
                return;
            }

            for (int index = 0; index < Encounters.Count; index++)
            {
                ShadowguardEncounter e = Encounters[index];

                if (e != null)
                {
                    if (e.EncounterDuration != TimeSpan.MaxValue)
                    {
                        DateTime end = e.StartTime + e.EncounterDuration;

                        if (!e.DoneWarning && DateTime.UtcNow > end - TimeSpan.FromMinutes(5))
                        {
                            e.DoWarning();
                        }
                        else if (DateTime.UtcNow >= end)
                        {
                            e.Expire();
                        }
                        else
                        {
                            e.OnTick();
                        }
                    }
                    else
                    {
                        e.OnTick();
                    }
                }
            }
        }

        public void CompleteRoof(Mobile m)
        {
            if (Table == null)
                return;

            Table.Remove(m);

            if (Table.Count == 0)
                Table = null;
        }

        public void OnEncounterComplete(ShadowguardEncounter encounter, bool expired)
        {
            Encounters.Remove(encounter);
            CheckQueue();

            if (!expired)
            {
                foreach (Mobile mobile in encounter.Region.GetEnumeratedMobiles())
                {
                    if (mobile is PlayerMobile pm)
                    {
                        AddToTable(pm, encounter.Encounter);
                    }
                }
            }
        }

        public void AddToTable(Mobile m, EncounterType encounter)
        {
            if (encounter == EncounterType.Roof)
            {
                return;
            }

            if (Table != null && Table.TryGetValue(m, out EncounterType value))
            {
                if ((value & encounter) == 0)
                {
                    Table[m] |= encounter;
                }
            }
            else
            {
                if (Table == null)
                {
                    Table = new Dictionary<Mobile, EncounterType>();
                }

                Table[m] = encounter;
            }
        }

        public void AddEncounter(ShadowguardEncounter encounter)
        {
            Encounters.Add(encounter);
        }

        public bool HasCompletedEncounter(Mobile m, EncounterType encounter)
        {
            return Table != null && Table.ContainsKey(m) && (Table[m] & encounter) != 0;
        }

        public bool CanTryEncounter(Mobile m, EncounterType encounter)
        {
            Party p = Party.Get(m);

            if (p != null && p.Leader != m)
            {
                m.SendLocalizedMessage(1156184); // You may not start a Shadowguard encounter while in a party unless you are the party leader.
                return false;
            }

            if (encounter == EncounterType.Roof)
            {
                if (p != null)
                {
                    for (int index = 0; index < p.Members.Count; index++)
                    {
                        PartyMemberInfo info = p.Members[index];

                        if (Table == null || !Table.TryGetValue(info.Mobile, out EncounterType value) || (value & EncounterType.Required) != EncounterType.Required)
                        {
                            m.SendLocalizedMessage(1156249); // All members of your party must complete each of the Shadowguard Towers before attempting the finale. 
                            return false;
                        }
                    }
                }
                else if (Table == null || !Table.TryGetValue(m, out EncounterType value) || (value & EncounterType.Required) != EncounterType.Required)
                {
                    m.SendLocalizedMessage(1156196); // You must complete each level of Shadowguard before attempting the Roof.
                    return false;
                }
            }

            if (p != null)
            {
                for (int index = 0; index < p.Members.Count; index++)
                {
                    PartyMemberInfo info = p.Members[index];

                    for (int i = 0; i < Encounters.Count; i++)
                    {
                        ShadowguardEncounter enc = Encounters[i];

                        if (enc.PartyLeader != null)
                        {
                            Party party = Party.Get(enc.PartyLeader);

                            if (enc.PartyLeader == info.Mobile || party != null && party.Contains(info.Mobile))
                            {
                                m.SendLocalizedMessage(1156189, info.Mobile.Name); // ~1_NAME~ in your party is already attempting to join a Shadowguard encounter.  Start a new party without them or wait until they are finished and try again.
                                return false;
                            }
                        }

                        foreach (Mobile mob in Queue.Keys)
                        {
                            if (mob != null)
                            {
                                Party party = Party.Get(mob);

                                if (mob == info.Mobile || party != null && party.Contains(info.Mobile))
                                {
                                    m.SendLocalizedMessage(1156189, info.Mobile.Name); // ~1_NAME~ in your party is already attempting to join a Shadowguard encounter.  Start a new party without them or wait until they are finished and try again.
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            for (int index = 0; index < Encounters.Count; index++)
            {
                ShadowguardEncounter instance = Encounters[index];

                if (instance.PartyLeader == m)
                {
                    return false;
                }
            }

            return true;
        }

        public void StartTimer()
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

        public static int GetLocalization(EncounterType encounter)
        {
            switch (encounter)
            {
                default:
                case EncounterType.Bar: return 1156165;
                case EncounterType.Orchard: return 1156166;
                case EncounterType.Armory: return 1156167;
                case EncounterType.Fountain: return 1156168;
                case EncounterType.Belfry: return 1156169;
                case EncounterType.Roof: return 1156170;
            }
        }

        public ShadowguardInstance GetAvailableInstance(EncounterType type)
        {
            if (RandomInstances)
            {
                List<ShadowguardInstance> instances;

                if (type == EncounterType.Roof)
                {
                    instances = new List<ShadowguardInstance>();

                    for (int index = 0; index < Instances.Count; index++)
                    {
                        ShadowguardInstance e = Instances[index];

                        if (e.IsRoof && !e.InUse)
                        {
                            instances.Add(e);
                        }
                    }
                }
                else
                {
                    instances = new List<ShadowguardInstance>();

                    for (int index = 0; index < Instances.Count; index++)
                    {
                        ShadowguardInstance e = Instances[index];

                        if (!e.IsRoof && !e.InUse)
                        {
                            instances.Add(e);
                        }
                    }
                }

                ShadowguardInstance inst = null;

                if (instances.Count > 0)
                {
                    inst = instances[Utility.Random(instances.Count)];
                }

                ColUtility.Free(instances);
                return inst;
            }

            if (type == EncounterType.Roof)
            {
                for (int index = 0; index < Instances.Count; index++)
                {
                    ShadowguardInstance e = Instances[index];

                    if (e.IsRoof && !e.InUse)
                    {
                        return e;
                    }
                }

                return null;
            }

            for (int index = 0; index < Instances.Count; index++)
            {
                ShadowguardInstance e = Instances[index];

                if (!e.IsRoof && !e.InUse)
                {
                    return e;
                }
            }

            return null;
        }

        public void AddToQueue(Mobile m, EncounterType encounter)
        {
            if (Queue.ContainsKey(m))
            {
                if (encounter == EncounterType.Roof)
                {
                    m.SendLocalizedMessage(1156245); // You are currently already in the queue for the finale. You cannot join this queue unless you leave the other queue. Use the context menu option on the crystal ball to exit that queue.
                }
                else
                {
                    m.SendLocalizedMessage(1156246); // You are currently already in the queue for one of the tower encounters. You cannot join this queue unless you leave the other queue. Use the context menu option on the crystal ball to exit that queue.
                }

                return;
            }

            Queue.Add(m, encounter);

            List<Mobile> list = new List<Mobile>();

            foreach (Mobile key in Queue.Keys)
            {
                list.Add(key);
            }

            int order = Array.IndexOf(list.ToArray(), m) + 1;

            m.SendLocalizedMessage(1156182, order > 1 ? order.ToString() : "next");
            /* The fortress is currently full right now. You are currently ~1_NUM~ in the queue.  
            You will be messaged when an encounter is available.  You must remain in the lobby in
            order to be able to join.*/
        }

        public bool IsInQueue(Mobile m)
        {
            return Queue.ContainsKey(m);
        }

        public bool RemoveFromQueue(Mobile m)
        {
            if (Queue.ContainsKey(m))
            {
                Queue.Remove(m);

                return false;
            }

            return false;
        }

        public void CheckQueue()
        {
            if (Queue.Count == 0)
                return;

            bool message = false;

            List<Mobile> copy = new List<Mobile>(Queue.Keys);

            for (int i = 0; i < copy.Count; i++)
            {
                Mobile m = copy[i];

                if (m.Map != Map.TerMur || m.NetState == null)
                {
                    RemoveFromQueue(m);

                    if (i == 0)
                    {
                        message = true;
                    }

                    continue;
                }

                for (int index = 0; index < Encounters.Count; index++)
                {
                    ShadowguardEncounter inst = Encounters[index];

                    if (inst.PartyLeader == m)
                    {
                        if (i == 0)
                        {
                            message = true;
                        }

                        RemoveFromQueue(m);
                    }
                }

                if (Queue.Count > 0)
                {
                    message = true;

                    Timer.DelayCall(TimeSpan.FromMinutes(2), mobile =>
                    {
                        if (Queue.TryGetValue(m, out EncounterType value))
                        {
                            ShadowguardInstance instance = GetAvailableInstance(value);

                            if (instance != null && instance.TryBeginEncounter(m, true, value))
                            {
                                RemoveFromQueue(m);
                            }
                        }
                    }, m);
                }

                break;
            }

            ColUtility.Free(copy);

            if (message && Queue.Count > 0)
            {
                ColUtility.For(Queue.Keys, (i, mob) =>
                {
                    Party p = Party.Get(mob);

                    if (p != null)
                    {
                        for (int index = 0; index < p.Members.Count; index++)
                        {
                            PartyMemberInfo info = p.Members[index];

                            info.Mobile.SendLocalizedMessage(1156190, i + 1 > 1 ? i.ToString() : "next");
                        }
                    }
                    //A Shadowguard encounter has opened. You are currently ~1_NUM~ in the 
                    //queue. If you are next, you may proceed to the entry stone to join.
                    else
                    {
                        mob.SendLocalizedMessage(1156190, i + 1 > 1 ? i.ToString() : "next");
                    }
                });
            }
        }

        private static readonly Rectangle2D[] _EncounterBounds =
        {
            new Rectangle2D(70, 1990, 51, 51),
            new Rectangle2D(198, 1990, 51, 51),
            new Rectangle2D(326, 1990, 51, 51),
            new Rectangle2D(454, 1990, 51, 51),

            new Rectangle2D(134, 2054, 51, 51),
            new Rectangle2D(262, 2054, 51, 51),
            new Rectangle2D(390, 2054, 51, 51),

            new Rectangle2D(70, 2118, 51, 51),
            new Rectangle2D(198, 2118, 51, 51),
            new Rectangle2D(326, 2118, 51, 51),

            new Rectangle2D(134, 2182, 51, 51),
            new Rectangle2D(262, 2182, 51, 51),
            new Rectangle2D(390, 2182, 51, 51),

            new Rectangle2D(31, 2303, 64, 64),
            new Rectangle2D(127, 2303, 64, 64),
            new Rectangle2D(31, 2399, 64, 64),
            new Rectangle2D(127, 2399, 64, 64)
        };

        private static readonly Point3D[] _CenterPoints =
        {
            new Point3D(96, 2016, -20),  new Point3D(224, 2016, -20),  new Point3D(352, 2016, -20), new Point3D(480, 2016, -20),
            new Point3D(160, 2080, -20), new Point3D(288, 2080, -20),  new Point3D(416, 2080, -20),
            new Point3D(96, 2144, -20),  new Point3D(224, 2144, -20),  new Point3D(352, 2144, -20),
            new Point3D(160, 2208, -20), new Point3D(288, 2208, -20),  new Point3D(416, 2208, -20),

            new Point3D(64, 2336, 0),    new Point3D(160, 2336, 0),    new Point3D(64, 2432, 0),   new Point3D(160, 2432, 0)
        };

        public static ShadowguardEncounter GetEncounter(Point3D p, Map map)
        {
            if (Region.Find(p, map) is ShadowguardRegion r)
            {
                return r.Instance.Encounter;
            }

            return null;
        }

        public static ShadowguardInstance GetInstance(Point3D p, Map map)
        {
            if (Region.Find(p, map) is ShadowguardRegion r)
            {
                return r.Instance;
            }

            return null;
        }

        public override void Delete()
        {
            base.Delete();

            EndTimer();

            if (Encounters != null)
            {
                for (int index = 0; index < Encounters.Count; index++)
                {
                    ShadowguardEncounter e = Encounters[index];

                    e.Reset();
                }

                ColUtility.Free(Encounters);
                Encounters = null;
            }

            if (Addons != null)
            {
                Addons.IterateReverse(addon =>
                {
                    addon.Delete();
                });

                ColUtility.Free(Addons);
                Addons = null;
            }

            if (Instances != null)
            {
                for (int index = 0; index < Instances.Count; index++)
                {
                    ShadowguardInstance inst = Instances[index];

                    if (inst.Region != null)
                    {
                        inst.ClearRegion();
                        inst.Region.Unregister();
                    }
                }

                ColUtility.Free(Instances);
                Instances = null;
            }

            if (Queue != null)
            {
                Queue.Clear();
                Queue = null;
            }

            if (Table != null)
            {
                Table.Clear();
                Table = null;
            }

            Instance = null;
        }

        public ShadowguardController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(KickLocation);
            writer.Write(Lobby);

            writer.Write(Encounters.Count);

            for (int index = 0; index < Encounters.Count; index++)
            {
                ShadowguardEncounter encounter = Encounters[index];

                writer.Write((int) encounter.Encounter);
                encounter.Serialize(writer);
            }

            writer.Write(Table == null ? 0 : Table.Count);

            if (Table != null)
            {
                ColUtility.ForEach(Table, (m, encounter) =>
                {
                    writer.Write(m);
                    writer.Write((int)encounter);
                });
            }

            writer.Write(Addons.Count);

            for (int index = 0; index < Addons.Count; index++)
            {
                BaseAddon addon = Addons[index];

                writer.Write(addon);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            Instance = this;

            base.Deserialize(reader);
            reader.ReadInt();

            InitializeInstances();

            Encounters = new List<ShadowguardEncounter>();
            Addons = new List<BaseAddon>();
            Queue = new Dictionary<Mobile, EncounterType>();

            KickLocation = reader.ReadPoint3D();
            Lobby = reader.ReadRect2D();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                ShadowguardEncounter encounter = ShadowguardEncounter.ConstructEncounter((EncounterType)reader.ReadInt());
                encounter.Deserialize(reader);

                AddEncounter(encounter);
            }

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                if (Table == null)
                    Table = new Dictionary<Mobile, EncounterType>();

                Mobile m = reader.ReadMobile();
                if (m != null)
                    Table[m] = (EncounterType)reader.ReadInt();
            }

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                if (reader.ReadItem() is BaseAddon addon)
                {
                    Addons.Add(addon);
                }
            }

            StartTimer();
        }

        public static void OnDisconnected(Mobile m)
        {
            ShadowguardEncounter encounter = GetEncounter(m.Location, m.Map);

            if (encounter != null)
            {
                encounter.CheckPlayerStatus(m);
            }
        }

        public static void OnLogin(Mobile m)
        {
            if (m.AccessLevel > AccessLevel.GameMaster)
                return;

            ShadowguardInstance inst = GetInstance(m.Location, m.Map);

            if (inst != null)
            {
                ShadowguardEncounter encounter = inst.Encounter;

                if (encounter == null)
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(1), mob =>
                    {
                        ShadowguardEncounter.MovePlayer(mob, Instance.KickLocation, true);
                    }, m);
                }
                else if (m != encounter.PartyLeader && m is PlayerMobile mobile && !encounter.Participants.Contains(mobile))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(1), mob =>
                    {
                        ShadowguardEncounter.MovePlayer(mob, Instance.KickLocation, true);
                    }, m);
                }
            }
        }

        public static void SetupShadowguard(Mobile from)
        {
            if (Instance != null)
                return;

            ShadowguardController controller = new ShadowguardController();
            controller.MoveToWorld(new Point3D(501, 2192, 50), Map.TerMur);

            MetalDoor door = new MetalDoor(DoorFacing.NorthCCW)
            {
                Hue = 1779
            };
            door.MoveToWorld(new Point3D(519, 2188, 25), Map.TerMur);

            door = new MetalDoor(DoorFacing.SouthCW)
            {
                Hue = 1779
            };
            door.MoveToWorld(new Point3D(519, 2189, 25), Map.TerMur);

            door = new MetalDoor(DoorFacing.NorthCCW)
            {
                Hue = 1779
            };
            door.MoveToWorld(new Point3D(519, 2192, 25), Map.TerMur);

            door = new MetalDoor(DoorFacing.SouthCW)
            {
                Hue = 1779
            };
            door.MoveToWorld(new Point3D(519, 2193, 25), Map.TerMur);

            AnkhWest ankh = new AnkhWest();
            ankh.MoveToWorld(new Point3D(503, 2191, 25), Map.TerMur);

            Item item = new Static(19343);
            item.MoveToWorld(new Point3D(64, 2336, 29), Map.TerMur);

            item = new Static(19343);
            item.MoveToWorld(new Point3D(160, 2336, 29), Map.TerMur);

            item = new Static(19343);
            item.MoveToWorld(new Point3D(64, 2432, 29), Map.TerMur);

            item = new Static(19343);
            item.MoveToWorld(new Point3D(160, 2432, 29), Map.TerMur);

            from.SendMessage("Shadowguard has been setup!");
            Console.WriteLine("Shadowguard setup!");
        }
    }

    public class ShadowguardGump : Gump
    {
        public static readonly int Red = 0x7E10;
        public static readonly int Green = 0x43F0;

        public PlayerMobile User { get; }

        public ShadowguardGump(PlayerMobile user)
            : base(100, 100)
        {
            User = user;

            AddPage(0);

            AddBackground(0, 0, 370, 320, 83);
            AddHtmlLocalized(10, 10, 350, 18, 1154645, "#1156164", 0x7FFF, false, false); // Shadowguard
            AddHtmlLocalized(10, 41, 350, 18, 1154645, "#1156181", 0x7FFF, false, false); // Select the area of Shadowguard you wish to explore...

            ShadowguardController controller = ShadowguardController.Instance;
            int index = 0;

            for (int i = 0; i < _Encounters.Length; i++)
            {
                EncounterType encounter = _Encounters[i];

                int hue = controller.HasCompletedEncounter(User, encounter) ? Green : Red;

                AddHtmlLocalized(50, 72 + (index * 20), 150, 20, ShadowguardController.GetLocalization(encounter), hue, false, false);
                AddButton(10, 72 + (index * 20), 1209, 1210, i + 1, GumpButtonType.Reply, 0);

                index++;
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID > 0)
            {
                int id = info.ButtonID - 1;

                if (id >= 0 && id < _Encounters.Length)
                {
                    ShadowguardController controller = ShadowguardController.Instance;

                    EncounterType type = _Encounters[id];
                    ShadowguardInstance inst = controller.GetAvailableInstance(type);

                    if (controller.CanTryEncounter(User, type))
                    {
                        if (inst == null)
                        {
                            controller.AddToQueue(User, type);
                        }
                        else
                        {
                            inst.TryBeginEncounter(User, false, type);
                            controller.RemoveFromQueue(User);
                        }
                    }
                }
            }
        }

        private readonly EncounterType[] _Encounters =
        {
            EncounterType.Bar,
            EncounterType.Orchard,
            EncounterType.Armory,
            EncounterType.Fountain,
            EncounterType.Belfry,
            EncounterType.Roof
        };
    }

    public class ShadowguardRegion : BaseRegion
    {
        public ShadowguardInstance Instance { get; }

        public ShadowguardRegion(Rectangle2D bounds, string regionName, ShadowguardInstance instance)
            : base($"Shadowguard_{regionName}", Map.TerMur, DefaultPriority, bounds)
        {
            Instance = instance;
        }

        public override bool CheckTravel(Mobile m, Point3D newlocation, TravelCheckType travelType)
        {
            if (Instance.InUse)
                return travelType >= (TravelCheckType)5;

            return true;
        }

        public override void OnDeath(Mobile m)
        {
            if (!Instance.InUse)
                return;

            if (m is PlayerMobile)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
                {
                    if (Instance.Encounter != null)
                        Instance.Encounter.CheckPlayerStatus(m);
                });
            }
            else if (m is BaseCreature creature && Instance.Encounter != null)
            {
                Instance.Encounter.OnCreatureKilled(creature);
            }
        }

        public override bool OnTarget(Mobile m, Target t, object o)
        {
            if (m.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (o is AddonComponent component && component.ItemData.Height + component.Z > m.Z + 3)
                return false;

            if (o is StaticTarget target && target.Z > m.Z + 3)
                return false;

            if (t.Flags == TargetFlags.Harmful && (o is LadyMinax || o is ShadowguardGreaterDragon dragon && dragon.Z > m.Z))
            {
                return false;
            }

            return base.OnTarget(m, t, o);
        }

        public override void OnSpeech(SpeechEventArgs args)
        {
            Mobile m = args.Mobile;

            if (m.AccessLevel >= AccessLevel.GameMaster && args.Speech != null && args.Speech.ToLower().Trim() == "getprops")
            {
                if (Instance.Encounter != null)
                    m.SendGump(new PropertiesGump(m, Instance.Encounter));
                else
                    m.SendMessage("There is no encounter for this instance at this time.");
            }
        }
    }
}
