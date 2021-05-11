using Server.Engines.Quests;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class ScrollOfAlacrity : SpecialScroll
    {        
        public override int LabelNumber => 1078604; // Scroll of Alacrity

        public override int Message => 1078602;
        /*Using a Scroll of Transcendence for a given skill will permanently increase your current 
        *level in that skill by the amount of points displayed on the scroll.
        *As you may not gain skills beyond your maximum skill cap, any excess points will be lost.*/

        public override int Title => 1078604; // Scroll of Alacrity
        public override string DefaultTitle => "<basefont color=#FFFFFF>Scroll of Alacrity:</basefont>";        

        public ScrollOfAlacrity()
            : this(SkillName.Alchemy)
        {
        }

        [Constructable]
        public ScrollOfAlacrity(SkillName skill)
            : base(skill, 0.0)
        {
            ItemID = 0x14EF;
            Hue = 1195;
        }

        public ScrollOfAlacrity(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1071345, "{0} 15 Minutes", GetName()); // Skill: ~1_val~
        }

        public override bool CanUse(Mobile from)
        {
            if (!base.CanUse(from))
                return false;

            if (!(from is PlayerMobile pm))
                return false;

            for (int i = pm.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = pm.Quests[i];

                for (int j = quest.Objectives.Count - 1; j >= 0; j--)
                {
                    BaseObjective objective = quest.Objectives[j];

                    if (objective is ApprenticeObjective)
                    {
                        from.SendLocalizedMessage(1079254); // You may not use your Scroll of Alacrity while your character is on a new player skill quest.
                        return false;
                    }
                }
            }

            if (pm.AcceleratedStart > DateTime.UtcNow)
            {
                from.SendLocalizedMessage(1077951); // You are already under the effect of an accelerated skillgain scroll.
                return false;
            }

            return true;
        }

        public override void Use(Mobile from)
        {
            if (!CanUse(from))
                return;

            if (!(from is PlayerMobile pm))
                return;

            double tskill = from.Skills[Skill].Base;
            double tcap = from.Skills[Skill].Cap;

            if (tskill >= tcap || from.Skills[Skill].Lock != SkillLock.Up)
            {
                from.SendLocalizedMessage(1094935);	/*You cannot increase this skill at this time. The skill may be locked or set to lower in your skill menu.
                *If you are at your total skill cap, you must use a Powerscroll to increase your current skill cap.*/
                return;
            }            

            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.FixedFrom, from.Serial, Serial.Zero, 0, from.Location, from.Location, 0, 0, false, false, 0, 0, 0, 5060, 1, from.Serial, 59, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.Moving, Serial.Zero, from.Serial, 0x36D4, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Location, 7, 0, false, false, 1178, 0, 0, 0, 1, from.Serial, 91, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.Moving, Serial.Zero, from.Serial, 0x36D4, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Location, 7, 0, false, false, 1178, 0, 0, 0, 1, from.Serial, 91, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.Moving, Serial.Zero, from.Serial, 0x36D4, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Location, 7, 0, false, false, 1178, 0, 0, 0, 1, from.Serial, 91, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.FixedFrom, from.Serial, Serial.Zero, 0x375A, from.Location, from.Location, 35, 90, false, false, 0, 0, 0, 0, 1, from.Serial, 91, 0));

            Effects.PlaySound(from.Location, from.Map, 0x100);            

            CreateTimer(pm, 900, Skill);
            StartTimer(pm);
            
            Delete();
        }

        public class AlacrityArray
        {
            public Mobile Mobile { get; set; }
            public ExpireTimer Timer { get; set; }
            public SkillName Skill { get; set; }
        }

        private static readonly List<AlacrityArray> Table = new List<AlacrityArray>();

        public static void Configure()
        {
            EventSink.Login += OnLogin;
            EventSink.Logout += OnLogout;
        }

        private static void OnLogin(LoginEventArgs e)
        {
            Timer.DelayCall(TimeSpan.FromSeconds(1), () => StartTimer(e.Mobile));
        }

        private static void OnLogout(LogoutEventArgs e)
        {
            StopTimer(e.Mobile);
        }

        public class ExpireTimer : Timer
        {
            private readonly Mobile m_Mobile;
            public int Tick;

            public ExpireTimer(Mobile m, int tick)
                : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            {
                m_Mobile = m;
                Tick = tick;
            }

            protected override void OnTick()
            {
                if (Tick <= 0)
                {
                    RemoveTimer(m_Mobile);
                }
                else
                {
                    Tick--;
                }
            }
        }

        public static AlacrityArray Contains(Mobile m)
        {
            AlacrityArray arr = null;

            for (var index = 0; index < Table.Count; index++)
            {
                var t = Table[index];

                if (t.Mobile == m)
                {
                    arr = t;
                    break;
                }
            }

            return arr;            
        }

        public static void CreateTimer(Mobile m, int tick, SkillName skillname)
        {
            if (m != null)
            {
                var contains = Contains(m);

                if (contains == null)
                {
                    Table.Add(new AlacrityArray { Mobile = m, Timer = new ExpireTimer(m, tick), Skill = skillname });
                }
            }
        }

        public static void RemoveTimer(Mobile m)
        {
            if (m == null)
                return;

            var contains = Contains(m);

            if (contains != null)
            {
                ExpireTimer t = contains.Timer;

                if (t != null)
                {
                    t.Stop();
                    Table.Remove(contains);

                    if (((PlayerMobile)m).AcceleratedStart > DateTime.UtcNow)
                    {
                        ((PlayerMobile)m).AcceleratedStart = DateTime.UtcNow;
                    }

                    m.PlaySound(0x100);
                    BuffInfo.RemoveBuff(m, BuffIcon.ArcaneEmpowerment);
                    m.SendLocalizedMessage(1077957);// The intense energy dissipates. You are no longer under the effects of an accelerated skillgain scroll.
                }
            }
        }

        public static void StartTimer(Mobile m)
        {
            if (m == null)
                return;

            var contains = Contains(m);

            if (contains != null)
            {
                ExpireTimer t = contains.Timer;

                if (t != null)
                {
                    t.Start();

                    m.SendLocalizedMessage(1077956); // You are infused with intense energy. You are under the effects of an accelerated skill gain scroll.

                    m.PlaySound(0x0FF);

                    ((PlayerMobile)m).AcceleratedStart = DateTime.UtcNow + TimeSpan.FromSeconds(t.Tick);
                    ((PlayerMobile)m).AcceleratedSkill = contains.Skill;

                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.ArcaneEmpowerment, 1078511, 1078512, TimeSpan.FromSeconds(t.Tick), m, SkillInfo.Table[(int)contains.Skill].Name));
                }
            }
            else
            {
                if (((PlayerMobile)m).AcceleratedStart > DateTime.UtcNow)
                {
                    ((PlayerMobile)m).AcceleratedStart = DateTime.UtcNow;
                }
            }
        }

        public static void StopTimer(Mobile m)
        {
            if (m == null)
                return;

            var contains = Contains(m);

            if (contains != null)
            {
                ExpireTimer t = contains.Timer;

                if (t != null)
                {
                    t.Stop();

                    if (((PlayerMobile)m).AcceleratedStart > DateTime.UtcNow)
                    {
                        ((PlayerMobile)m).AcceleratedStart = DateTime.UtcNow;
                    }
                }
            }
        }        

        public static ScrollOfAlacrity CreateRandom()
        {
            return new ScrollOfAlacrity((SkillName)SkillInfo.Table[Utility.Random(SkillInfo.Table.Length)].SkillID);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
