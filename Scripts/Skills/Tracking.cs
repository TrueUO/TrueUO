using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;
using Server.Spells.Seventh;
using System;
using System.Collections.Generic;

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

        public static TimeSpan OnUse(Mobile m)
        {
            m.SendLocalizedMessage(1011350); // What do you wish to track?

            m.CloseGump(typeof(TrackWhatGump));
            m.CloseGump(typeof(TrackWhoGump));
            m.SendGump(new TrackWhatGump(m));

            return TimeSpan.FromSeconds(10.0); // 10 second delay before beign able to re-use a skill
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
                return 0.0;

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

        public class TrackingInfo
        {
            public Mobile m_Target;
            public Point2D m_Location;
            public Map m_Map;
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
                TrackWhoGump.DisplayTo(m_Success, m_From, info.ButtonID - 1);
        }
    }

    public class TrackWhoGump : Gump
    {
        private Dictionary<Body, string> bodyNames = new Dictionary<Body, string>(){
                                  {747, "a wraith"},
                                  {748, "a wraith"},
                                    {749, "a lich"},
                                    {746, "a lich"},
 {0x7A, "a unicorn"},
 {0xF6, "a bake-kitsune"},
 {0x90E, "a wolf"},
 {0xDC, "a llama"},
 {0x8B0, "a ostard"},
 {0x7D6, "a bullfrog"},
 {0x7E2, "a giant serpent"},
 {0x90E, "a dog"},
 {0x90E, "a cat"},
 {0x90E, "a rat"},
 {0x90E, "a rabbit"},
 {0x116, "a squirrel"},
 {0x117, "a ferret"},
 {0x115, "a cu sidhe"},
 {0x114, "a reptalon"},
 {0x4E7, "a white tiger"},
  {0xD0, "a chicken"},
 {0xD9, "a dog"},
 {0xE1, "a wolf"},
 {0xD6, "a panther"},
 {0x1D, "a gorilla"},
 {0xD3, "a black bear"},
 {0xD4, "a grizzly bear"},
 {0xD5, "a polar bear"},
 {0x33, "a slime"},
 {0x11, "a orc"},
 {0x21, "a lizardMan"},
 {0x04, "a gargoyle"},
 {0x01, "a ogre"},
 {0x36, "a troll"},
 {0x02, "a ettin"},
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

                if (list.Count > 8)
                {
                    AddBackground(0, 310, 440, 155, 5054);

                    AddBackground(10, 320, 420, 75, 2620);
                    AddBackground(10, 395, 420, 45, 3000);
                }
            }

            for (int i = 0; i < list.Count && i < 12; ++i)
            {
                Mobile m = list[i];

                AddItem(20 + i % 4 * 100, 20 + i / 4 * 155, ShrinkTable.Lookup(m));
                AddButton(20 + i % 4 * 100, 130 + i / 4 * 155, 4005, 4007, i + 1, GumpButtonType.Reply, 0);

                string name = m.Name;
                if (TransformationSpellHelper.UnderTransformation(m, typeof(VampiricEmbraceSpell)) || m.Player && m.Body.IsHuman && m.IsBodyMod)
                {
                    name = m.Body.IsFemale ? NameList.RandomName("female") : NameList.RandomName("male");
                }
                else if (m.Player && (m.Body.IsAnimal || m.Body.IsMonster) && bodyNames.ContainsKey(m.Body))
                {
                    bodyNames.TryGetValue(m.Body, out name);
                }
                if (name != null)
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
                return;

            TrackTypeDelegate check = m_Delegates[type];

            from.CheckSkill(SkillName.Tracking, 21.1, 100.0); // Passive gain

            int range = 10 + (int)(from.Skills[SkillName.Tracking].Value / 10);

            List<Mobile> list = new List<Mobile>();
            IPooledEnumerable eable = from.GetMobilesInRange(range);

            foreach (Mobile m in eable)
            {
                // Ghosts can no longer be tracked 
                if (m != from && m.Alive && (!m.Hidden || m.IsPlayer() || from.AccessLevel > m.AccessLevel) && check(m) && CheckDifficulty(from, m))
                    list.Add(m);
            }
            eable.Free();

            if (list.Count > 0)
            {
                list.Sort(new InternalSorter(from));

                from.SendGump(new TrackWhoGump(from, list, range));
                from.SendLocalizedMessage(1018093); // Select the one you would like to track.
            }
            else
            {
                if (type == 0)
                    from.SendLocalizedMessage(502991); // You see no evidence of animals in the area.
                else if (type == 1)
                    from.SendLocalizedMessage(502993); // You see no evidence of creatures in the area.
                else
                    from.SendLocalizedMessage(502995); // You see no evidence of people in the area.
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            int index = info.ButtonID - 1;

            if (index >= 0 && index < m_List.Count && index < 12)
            {
                Mobile m = m_List[index];

                m_From.QuestArrow = new TrackArrow(m_From, m, m_Range * 2);

                Tracking.AddInfo(m_From, m);
            }
        }

        // Tracking players uses tracking and detect hidden vs. hiding and stealth 
        private static bool CheckDifficulty(Mobile from, Mobile m)
        {
            if (!m.IsPlayer() && (IsAnimal(m) || IsMonster(m)))
                return from.Skills[SkillName.Tracking].Fixed > Math.Min(m.Fame, 18000) / 1800-10+ Utility.Random(20);
            if (!m.IsPlayer() && IsHumanNPC(m))
                return from.Skills[SkillName.Tracking].Fixed > 200;

            int tracking = from.Skills[SkillName.Tracking].Fixed;
            int detectHidden = from.Skills[SkillName.DetectHidden].Fixed;

            if (m.Race == Race.Elf)
                tracking /= 2; //The 'Guide' says that it requires twice as Much tracking SKILL to track an elf.  Not the total difficulty to track.

            int hiding = m.Skills[SkillName.Hiding].Fixed;
            int stealth = m.Skills[SkillName.Stealth].Fixed;
            int divisor = hiding + stealth;

            // Necromancy forms affect tracking difficulty 
            if (TransformationSpellHelper.UnderTransformation(m, typeof(HorrificBeastSpell)))
                divisor -= 200;
            else if (TransformationSpellHelper.UnderTransformation(m, typeof(VampiricEmbraceSpell)) && divisor < 500)
                divisor = 500;
            else if (TransformationSpellHelper.UnderTransformation(m, typeof(WraithFormSpell)))
                divisor += 200;
            if (divisor >= 2200)
                divisor = 2200;
            int chance;

            if (divisor > 0)
            {
                chance = 50 * (tracking + detectHidden + Math.Max(1, Utility.Random(200))) / divisor;
            }
            else
                chance = 100;

            return chance > Utility.Random(100);
        }

        private static bool IsAnimal(Mobile m)
        {
            return m.Body.IsAnimal;
        }

        private static bool IsMonster(Mobile m)
        {
            return m.Body.IsHuman && m.Murderer || m.Body.IsMonster || TransformationSpellHelper.UnderTransformation(m, typeof(VampiricEmbraceSpell));
        }

        private static bool IsHumanNPC(Mobile m)
        {
            return (!m.Player && m.Body.IsHuman) || m.Player && !m.CanBeginAction(typeof(PolymorphSpell)) && m.Body.IsHuman;
        }

        private static bool IsPlayer(Mobile m)
        {
            return m.Player && !m.Body.IsMonster && !m.Body.IsAnimal;
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
                    return 0;
                if (x == null)
                    return -1;
                if (y == null)
                    return 1;

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

                m_From.SendLocalizedMessage(503177); // You have lost your quarry.
            }
        }
    }

    public class TrackTimer : Timer
    {
        private readonly Mobile m_From;
        private readonly IEntity m_Target;
        private readonly int m_Range;
        private readonly QuestArrow m_Arrow;
        private int m_LastX, m_LastY;
        public TrackTimer(Mobile from, IEntity target, int range, QuestArrow arrow)
            : base(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(2.5))
        {
            m_From = from;
            m_Target = target;
            m_Range = range;

            m_Arrow = arrow;
        }

        protected override void OnTick()
        {
            if (!m_Arrow.Running)
            {
                Stop();
                return;
            }

            if (m_From.NetState == null || m_From.Deleted || m_Target.Deleted || m_From.Map != m_Target.Map || !m_From.InRange(m_Target, m_Range) || m_Target is Mobile m && m.Hidden && m.AccessLevel > m_From.AccessLevel)
            {
                m_Arrow.Stop();
                Stop();
                return;
            }

            if (m_LastX != m_Target.Location.X || m_LastY != m_Target.Location.Y)
            {
                m_LastX = m_Target.Location.X;
                m_LastY = m_Target.Location.Y;

                m_Arrow.Update();
            }
        }
    }
}
