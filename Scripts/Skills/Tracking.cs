using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;
using System;
using System.Collections.Generic;
using Server.Items;
using System.Linq;
namespace Server.SkillHandlers
{
    public delegate bool TrackTypeDelegate(Mobile m);

    public class Tracking
    {
        private static readonly Dictionary<Mobile, TrackingInfo> m_Table = new Dictionary<Mobile, TrackingInfo>();

        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Tracking].Callback = OnUse;
        }

        private static TimeSpan OnUse(Mobile m)
        {
            m.SendLocalizedMessage(1011350); // What do you wish to track?

            m.CloseGump(typeof(TrackWhatGump));
            m.CloseGump(typeof(TrackWhoGump));
            m.SendGump(new TrackWhatGump(m));

            return m.AccessLevel == AccessLevel.Player ? TimeSpan.FromSeconds(10.0) : TimeSpan.FromSeconds(1.0);
        }

        public static void AddInfo(Mobile tracker, Mobile target)
        {
            TrackingInfo info = new TrackingInfo(target);

            m_Table[tracker] = info;
        }

        public static double GetStalkingBonus(Mobile tracker, Mobile target)
        {
            TrackingInfo info;

            m_Table.TryGetValue(tracker, out info);

            if (info == null || info.m_Target != target || info.m_Map != target.Map)
            {
                return 0.0;
            }

            int xDelta = info.m_Location.X - target.X;
            int yDelta = info.m_Location.Y - target.Y;

            double bonus = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);

            m_Table.Remove(tracker);

            return Math.Min(bonus, 10 + tracker.Skills.Tracking.Value / 10);
        }

        public static void ClearTrackingInfo(Mobile tracker)
        {
            m_Table.Remove(tracker);
        }

        private class TrackingInfo
        {
            public readonly Mobile m_Target;
            public readonly Map m_Map;

            public Point2D m_Location;

            public TrackingInfo(Mobile target)
            {
                m_Target = target;
                m_Location = new Point2D(target.X, target.Y);
                m_Map = target.Map;
            }
        }
    }

    public class TrackWhatGump : Gump
    {
        private readonly Mobile m_From;
        private readonly bool m_Success;

        public TrackWhatGump(Mobile from)
            : base(20, 30)
        {
            m_From = from;
            m_Success = from.CheckSkill(SkillName.Tracking, 0.0, 21.1);

            AddPage(0);

            AddBackground(0, 0, 440, 135, 5054);

            AddBackground(10, 10, 420, 75, 2620);
            AddBackground(10, 85, 420, 25, 3000);

            AddItem(20, 20, 9682);
            AddButton(20, 110, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(20, 90, 100, 20, 1018087, false, false); // Animals

            AddItem(120, 20, 9607);
            AddButton(120, 110, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtmlLocalized(120, 90, 100, 20, 1018088, false, false); // Monsters

            AddItem(220, 20, 8454);
            AddButton(220, 110, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddHtmlLocalized(220, 90, 100, 20, 1018089, false, false); // Human NPCs

            AddItem(320, 20, 8455);
            AddButton(320, 110, 4005, 4007, 4, GumpButtonType.Reply, 0);
            AddHtmlLocalized(320, 90, 100, 20, 1018090, false, false); // Players
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID >= 1 && info.ButtonID <= 4)
            {
                TrackWhoGump.DisplayTo(m_Success, m_From, info.ButtonID - 1);
            }
        }
    }

    public class TrackWhoGump : Gump
    {
        private static readonly bool TrackNecroAsMonster = Config.Get("Tracking.NecroTransformsShowAsMonsters", false);
        private static readonly bool TrackThiefAsNPC = Config.Get("Tracking.ThiefDisguiseShowsAsNPC", false);
        private static readonly int BaseTrackingDetectionRange = Config.Get("Tracking.BaseTrackingDetectionRange", 10);
        private static readonly int TrackDistanceMultiplier = Config.Get("Tracking.TrackDistanceMultiplier", 5);
        private static readonly int NonPlayerRangeMultiplier = Config.Get("Tracking.NonPlayerRangeMultiplier", 1);
        private static readonly bool RegionTracking = Config.Get("Tracking.RegionTracking", false);
        private static readonly bool CustomTargetNumbers = Config.Get("Tracking.CustomTargetNumbers", false);
        private static readonly bool NotifyPlayer = Config.Get("Tracking.NotifyPlayer", false);

        private readonly Dictionary<Body, string> bodyNames = new Dictionary<Body, string>
        {
            {0x2EB, "wraith"},
            {0x2EC, "wraith"},
            {0x2ED, "lich"},
            {0x2EA, "moloch"},
            {0x84, "ki-rin"},
            {0x7A, "unicorn"},
            {0xF6, "bake kitsune"},
            {0x19, "wolf"},
            {0xDC, "llama"},
            {0xDB, "ostard"},
            {0x51, "bullfrog"},
            {0x15, "giant serpent"},
            {0xD9, "dog"},
            {0xC9, "cat"},
            {0xEE, "rat"},
            {0xCD, "rabbit"},
            {0x116, "squirrel"},
            {0x117, "ferret"},
            {0x115, "cu sidhe"},
            {0x114, "reptalon"},
            {0x4E7, "white tiger"},
            {0xD0, "chicken"},
            {0xE1, "wolf"},
            {0xD6, "panther"},
            {0x1D, "gorilla"},
            {0xD3, "black bear"},
            {0xD4, "grizzly bear"},
            {0xD5, "polar bear"},
            {0x33, "slime"},
            {0x11, "orc"},
            {0x21, "lizardMan"},
            {0x04, "gargoyle"},
            {0x01, "ogre"},
            {0x36, "troll"},
            {0x02, "ettin"},
            {0x09, NameList.RandomName("daemon")}
        };

        private static readonly TrackTypeDelegate[] m_Delegates =
        {
            IsAnimal,
            IsMonster,
            IsHumanNPC,
            IsPlayer
        };

        private readonly Mobile m_From;
        private readonly int m_Range;
        private readonly List<Mobile> m_List;

        private TrackWhoGump(Mobile from, List<Mobile> list, int range)
            : base(20, 30)
        {
            m_From = from;
            m_List = list;
            m_Range = range;

            AddPage(0);

            AddBackground(0, 0, 440, 155, 5054);

            AddBackground(10, 10, 420, 75, 2620);
            AddBackground(10, 85, 420, 45, 3000);

            if (list.Count > 4)
            {
                AddBackground(0, 155, 440, 155, 5054);

                AddBackground(10, 165, 420, 75, 2620);
                AddBackground(10, 240, 420, 45, 3000);
            }

            if (list.Count > 8)
            {
                AddBackground(0, 310, 440, 155, 5054);

                AddBackground(10, 320, 420, 75, 2620);
                AddBackground(10, 395, 420, 45, 3000);
            }

            if (list.Count > 12)
            {
                AddBackground(0, 465, 440, 155, 5054);

                AddBackground(10, 475, 420, 75, 2620);
                AddBackground(10, 550, 420, 45, 3000);
            }

            if (list.Count > 16)
            {
                AddBackground(0, 620, 440, 155, 5054);

                AddBackground(10, 630, 420, 75, 2620);
                AddBackground(10, 705, 420, 45, 3000);
            }

            for (int i = 0; i < list.Count && i < TotalTargetsBySkill(from); ++i)
            {
                Mobile m = list[i];

                int displayHue;

                displayHue = m.Hue > 0x8000 ? 0 : m.Hue;

                AddItem(20 + i % 4 * 100, 20 + i / 4 * 155, ShrinkTable.Lookup(m), displayHue);
                AddButton(20 + i % 4 * 100, 130 + i / 4 * 155, 4005, 4007, i + 1, GumpButtonType.Reply, 0);

                string name = m.Name;

                if (m.Player && (m.Body.IsAnimal || m.Body.IsMonster) && bodyNames.ContainsKey(m.Body))
                {
                    bodyNames.TryGetValue(m.Body, out name);
                }

                if (name != null && name.StartsWith("a "))
                {
                    name = name.Substring(2);
                }

                AddHtml(20 + i % 4 * 100, 90 + i / 4 * 155, 90, 40, name, false, false);
            }
        }

        public static void DisplayTo(bool success, Mobile from, int type)
        {
            if (!success)
            {
                from.SendLocalizedMessage(1018092); // You see no evidence of those in the area.
                return;
            }

            Map map = from.Map;

            if (map == null)
            {
                return;
            }

            TrackTypeDelegate check = m_Delegates[type];

            from.CheckSkill(SkillName.Tracking, 21.1, 100.0); // Passive gain

            int range = (BaseTrackingDetectionRange + (int)(from.Skills[SkillName.Tracking].Value / 10)) * NonPlayerRangeMultiplier;

            List<Mobile> list = new List<Mobile>();

            if (RegionTracking)
            {
                if (type == 3)
                {
                    list = NetState.Instances.AsParallel().Select(m => m.Mobile).Where(m => m != null
                            && m != from
                            && m.Map == from.Map
                            && m.Alive
                            && (!m.Hidden || m.IsPlayer() || from.AccessLevel > m.AccessLevel)
                            && check(m)
                            && CheckDifficulty(from, m)
                            && ReachableTarget(from, m, range))
                        .OrderBy(x => x.GetDistanceToSqrt(from)).Select(x => x).Take(TotalTargetsBySkill(from)).ToList();
                }
                else
                {
                    IEnumerable<Mobile> mobiles = FilterRegionMobs(from, range);

                    list = mobiles.AsParallel().Where(m => m != null
                            && m != from
                            && m.Alive
                            && (!m.Hidden || m.IsPlayer() || from.AccessLevel > m.AccessLevel)
                            && check(m)
                            && CheckDifficulty(from, m)
                            && ReachableTarget(from, m, range))
                        .OrderBy(x => x.GetDistanceToSqrt(from)).Select(x => x).Take(TotalTargetsBySkill(from)).ToList();
                }
            }
            else
            {
                IPooledEnumerable eable = from.GetMobilesInRange(range);

                foreach (Mobile m in eable)
                {
                    if (list.Count <= TotalTargetsBySkill(from)
                        && m != from
                        && m.Alive
                        && (!m.Hidden || m.IsPlayer() || from.AccessLevel > m.AccessLevel)
                        && check(m)
                        && CheckDifficulty(from, m)
                        && (m.IsPlayer() && NonPlayerRangeMultiplier == 1 ? m.InRange(from, range / NonPlayerRangeMultiplier) : m.InRange(from, range)))
                    {
                        list.Add(m);
                    }

                    if (list.Count >= TotalTargetsBySkill(from))
                    {
                        break;
                    }
                }

                eable.Free();
            }

            if (list.Count > 0)
            {
                list.Sort(new InternalSorter(from));

                from.SendGump(new TrackWhoGump(from, list, range));
                from.SendLocalizedMessage(1018093); // Select the one you would like to track.
            }
            else
            {
                if (type == 0)
                {
                    from.SendLocalizedMessage(502991); // You see no evidence of animals in the area.
                }
                else if (type == 1)
                {
                    from.SendLocalizedMessage(502993); // You see no evidence of creatures in the area.
                }
                else
                {
                    from.SendLocalizedMessage(502995); // You see no evidence of people in the area.
                }
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            int index = info.ButtonID - 1;

            if (index >= 0 && index < m_List.Count && index < TotalTargetsBySkill(m_From))
            {
                Mobile m = m_List[index];

                if (RegionTracking)
                {
                    m_From.QuestArrow = new TrackArrow(m_From, m, m_Range);
                }
                else
                {
                    m_From.QuestArrow = new TrackArrow(m_From, m, m_Range * (TrackDistanceMultiplier == 0 ? 1000 : TrackDistanceMultiplier));
                }

                if (NotifyPlayer && m.Player)
                {
                    m.SendLocalizedMessage(1042971, "Your presence has been detected in this area."); // ~1_NOTHING~
                }

                Tracking.AddInfo(m_From, m);
            }
        }

        private static bool ReachableTarget(Mobile from, Mobile m, int range)
        {
            if (RegionTracking)
            {
                return true;
            }

            if (NonPlayerRangeMultiplier == 1 || !m.Player)
            {
                return m.InRange(from, range);
            }

            return m.InRange(from, range / NonPlayerRangeMultiplier);
        }

        private static int TotalTargetsBySkill(Mobile m)
        {
            if (!CustomTargetNumbers)
            {
                return 12;
            }

            int totalTargets = (int) (m.Skills[SkillName.Tracking].Value / 5.0); // 20 targets at 100 skill.

            if (totalTargets > 20)
            {
                return 20;
            }

            if (totalTargets < 1)
            {
                return 1;
            }

            return totalTargets;
        }

        private static List<Mobile> ConvertToList(IEnumerable<Mobile> ienum)
        {
            List<Mobile> list = new List<Mobile>();

            foreach (var mobile in ienum)
            {
                list.Add(mobile);
            }
            
            return list;
        }

        private static readonly Dictionary<Map, List<Rectangle2D[]>> mapAreas = new Dictionary<Map, List<Rectangle2D[]>>
        {
            {Map.Trammel, new List<Rectangle2D[]> {
            //Tram
            new[]{new Rectangle2D(new Point2D(0,0), new Point2D(5120,4096)) },
            //T2A
            new[]{new Rectangle2D(new Point2D(5120,2300), new Point2D(6152,4096)) }
            }
            },

            {Map.Felucca, new List<Rectangle2D[]> {
            //Fel
            new[]{new Rectangle2D(new Point2D(0,0), new Point2D(5120,4096)) },
            //T2A
            new[]{new Rectangle2D(new Point2D(5120,2300), new Point2D(6152,4096)) }
            }
            },

            {Map.Ilshenar, new List<Rectangle2D[]> {
            //Ilsh land
            new[]{
                new Rectangle2D(new Point2D(185,265), new Point2D(1878,943)),
                new Rectangle2D(new Point2D(185,944), new Point2D(1752,1000)),
                new Rectangle2D(new Point2D(185,1001), new Point2D(1878,1421)),
                new Rectangle2D(new Point2D(185,1422), new Point2D(580,1480))
            },
            //MeerRoom
            new[]{new Rectangle2D(new Point2D(1748,35), new Point2D(1824,97)) }
            }
            },

            {Map.Tokuno, new List<Rectangle2D[]> {
            //TokunoLand
            new[]{new Rectangle2D(new Point2D(0,0), new Point2D(1448, 1448)) }
            }
            },

            {Map.Malas, new List<Rectangle2D[]> {
            //Malas
            new[]{new Rectangle2D(new Point2D(512, 0), new Point2D(2560, 2048)) }
            }
            }
        };

        private static List<Mobile> GetMobsFromArrayBounds(Mobile from)
        {
            List<Mobile> mobiles = null;

            for (var i = 0; i < mapAreas[from.Map].Count; i++)
            {
                Rectangle2D[] areas = mapAreas[from.Map][i];

                foreach (var defined in areas)
                {
                    if (from.X > defined.X && from.Y > defined.Y && from.X < defined.X + defined.Width && from.Y < defined.Y + defined.Height)
                    {
                        mobiles = new List<Mobile>();

                        for (var index = 0; index < areas.Length; index++)
                        {
                            Rectangle2D area = areas[index];
                            mobiles.AddRange(ConvertToList(from.Map.GetMobilesInBounds(area)));
                        }

                        break;
                    }
                }
            }

            return mobiles;
        }

        private static List<Mobile> FilterRegionMobs(Mobile from, int range)
        {
            List<Mobile> mobiles = null;

            if (mapAreas.ContainsKey(from.Map))
            {
                mobiles = GetMobsFromArrayBounds(from);
            }

            if (mobiles == null && from.TopRegion.Area.Length != 0)
            {
                mobiles = from.Region.GetMobiles();
            }

            if (mobiles == null)
            {
                mobiles = ConvertToList(from.Map.GetMobilesInRange(from.Location, range));
            }

            return mobiles;
        }

        // Tracking players uses tracking and detect hidden vs. hiding and stealth 
        private static bool CheckDifficulty(Mobile from, Mobile m)
        {
            if (!m.Player && (IsAnimal(m) || IsMonster(m)))
            {
                return from.Skills[SkillName.Tracking].Fixed > Math.Min(m.Fame, 18000) / 18 - 10 + Utility.Random(20);
            }

            if (!m.Player && IsHumanNPC(m))
            {
                return from.Skills[SkillName.Tracking].Fixed >= 200;
            }

            int tracking = from.Skills[SkillName.Tracking].Fixed;
            int detectHidden = from.Skills[SkillName.DetectHidden].Fixed;
            int hiding = m.Skills[SkillName.Hiding].Fixed;
            int stealth = m.Skills[SkillName.Stealth].Fixed;
            int divisor = hiding + stealth;

            if (m.Race == Race.Elf)
            {
                divisor /= 2; // From testing OSI Humans track at ~70%, elves at ~35%, which is half the total chance
            }

            // Necromancy forms affect tracking difficulty 
            if (TransformationSpellHelper.UnderTransformation(m, typeof(HorrificBeastSpell)))
            {
                divisor -= 200;
            }
            else if (TransformationSpellHelper.UnderTransformation(m, typeof(VampiricEmbraceSpell)) && divisor < 500)
            {
                divisor = 500;
            }
            else if (TransformationSpellHelper.UnderTransformation(m, typeof(WraithFormSpell)))
            {
                divisor += 200;
            }
            else if (TransformationSpellHelper.UnderTransformation(m, typeof(LichFormSpell)))
            {
                divisor -= 200;
            }

            if (divisor > 2200)
            {
                divisor = 2200;
            }

            int chance = divisor > 0 ? 70 * (tracking + detectHidden) / divisor : 100;

            return chance > Utility.Random(100);
        }

        private static bool IsAnimal(Mobile m)
        {
            return m.Body.IsAnimal && !(m.Region.IsPartOf<Regions.HouseRegion>() && m.Blessed);
        }

        private static bool IsMonster(Mobile m)
        {
            return (!m.Player && m.Body.IsHuman && m is BaseCreature bc && bc.IsAggressiveMonster || m.Body.IsMonster || TrackedNecro(m)) && !(m.Region.IsPartOf<Regions.HouseRegion>() && m.Blessed);
        }

        private static bool IsHumanNPC(Mobile m)
        {
            return !m.Player && m.Body.IsHuman && !(m.Region.IsPartOf<Regions.HouseRegion>() && m.Blessed) && m is BaseCreature bc && !bc.IsAggressiveMonster || TrackedThief(m);
        }

        private static bool IsPlayer(Mobile m)
        {
            return m.Player && !m.Body.IsMonster && !m.Body.IsAnimal && !TrackedNecro(m) && !TrackedThief(m);
        }

        private static bool TrackedNecro(Mobile m)
        {
            return TrackNecroAsMonster && TransformationSpellHelper.UnderTransformation(m);
        }

        private static bool TrackedThief(Mobile m)
        {
            return TrackThiefAsNPC && m.Player && DisguiseTimers.IsDisguised(m) && m.Body.IsHuman;
        }

        private class InternalSorter : IComparer<Mobile>
        {
            private readonly Mobile m_From;

            public InternalSorter(Mobile from)
            {
                m_From = from;
            }

            public int Compare(Mobile x, Mobile y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                return m_From.GetDistanceToSqrt(x).CompareTo(m_From.GetDistanceToSqrt(y));
            }
        }
    }

    public class TrackArrow : QuestArrow
    {
        private readonly Timer m_Timer;
        private Mobile m_From;

        public TrackArrow(Mobile from, IEntity target, int range)
            : base(from, target)
        {
            m_From = from;
            m_Timer = new TrackTimer(from, target, range, this);
            m_Timer.Start();
        }

        public override void OnClick(bool rightClick)
        {
            if (rightClick)
            {
                Tracking.ClearTrackingInfo(m_From);

                m_From = null;

                Stop();
            }
        }

        public override void OnStop()
        {
            m_Timer.Stop();

            if (m_From != null)
            {
                Tracking.ClearTrackingInfo(m_From);
            }
        }
    }

    public class TrackTimer : Timer
    {
        private static readonly bool RegionTracking = Config.Get("Tracking.RegionTracking", false);
        private static readonly bool KeepMarkerOnRangeLost = Config.Get("Tracking.KeepMarkerOnRangeLost", true);

        private readonly Mobile m_From;
        private readonly IEntity m_Target;
        private readonly int m_Range;

        private readonly QuestArrow m_Arrow;
        private int p_LastX, p_LastY, m_LastX, m_LastY, m_LastDistance, m_newDistance;

        public TrackTimer(Mobile from, IEntity target, int range, QuestArrow arrow)
            : base(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(2.5))
        {
            m_From = from;
            m_Target = target;
            m_Range = range;
            m_Arrow = arrow;
            p_LastX = m_From.Location.X;
            p_LastY = m_From.Location.Y;

            if (RegionTracking)
            {
                m_LastDistance = Math.Max(Math.Abs(m_Target.Location.Y - m_From.Location.Y), Math.Abs(m_Target.Location.X - m_From.Location.X));
            }
        }

        protected override void OnTick()
        {
            if (RegionTracking)
            {
                m_newDistance = Math.Max(Math.Abs(m_Target.Location.Y - m_From.Location.Y), Math.Abs(m_Target.Location.X - m_From.Location.X));
            }

            if (!m_Arrow.Running)
            {
                Stop();
                return;
            }

            if (m_From.Deleted
                || m_Target.Deleted
                || m_From.Map != m_Target.Map
                || RegionTracking && m_Target is Mobile mt && m_From.TopRegion != mt.TopRegion && Math.Abs(m_LastDistance - m_newDistance) > 20
                || !RegionTracking && !m_From.InRange(m_Target, m_Range)
                || m_Target is Mobile m && m.Hidden && m.AccessLevel > AccessLevel.Player)
            {
                if (!KeepMarkerOnRangeLost)
                {
                    m_Arrow.Stop();
                }

                Stop();

                m_From.SendLocalizedMessage(503177); // You have lost your quarry.
                return;
            }

            if (p_LastX != m_From.Location.X || p_LastY != m_From.Location.Y || m_LastX != m_Target.Location.X || m_LastY != m_Target.Location.Y)
            {
                m_LastX = m_Target.Location.X;
                m_LastY = m_Target.Location.Y;
                p_LastX = m_From.Location.X;
                p_LastY = m_From.Location.Y;

                if (RegionTracking)
                {
                    m_LastDistance = m_newDistance;
                }

                m_Arrow.Update();
            }
        }
    }
}
