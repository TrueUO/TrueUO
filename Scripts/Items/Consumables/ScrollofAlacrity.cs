using Server.Engines.Quests;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    [TypeAlias("Server.Items.ScrollofAlacrity")]
    public class ScrollOfAlacrity : SpecialScroll
    {
        private static readonly Dictionary<Mobile, Timer> Table = new Dictionary<Mobile, Timer>();

        public override int LabelNumber => 1078604;// Scroll of Alacrity

        public override int Message => 1078602;/*Using a Scroll of Transcendence for a given skill will permanently increase your current 
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

            from.SendLocalizedMessage(1077956); // You are infused with intense energy. You are under the effects of an accelerated skillgain scroll.

            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.FixedFrom, from.Serial, Serial.Zero, 0, from.Location, from.Location, 0, 0, false, false, 0, 0, 0, 5060, 1, from.Serial, 59, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.Moving, Serial.Zero, from.Serial, 0x36D4, new Point3D(from.X - 6, from.Y - 6, from.Z + 15), from.Location, 7, 0, false, false, 1178, 0, 0, 0, 1, from.Serial, 91, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.Moving, Serial.Zero, from.Serial, 0x36D4, new Point3D(from.X - 4, from.Y - 6, from.Z + 15), from.Location, 7, 0, false, false, 1178, 0, 0, 0, 1, from.Serial, 91, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.Moving, Serial.Zero, from.Serial, 0x36D4, new Point3D(from.X - 6, from.Y - 4, from.Z + 15), from.Location, 7, 0, false, false, 1178, 0, 0, 0, 1, from.Serial, 91, 0));
            Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.FixedFrom, from.Serial, Serial.Zero, 0x375A, from.Location, from.Location, 35, 90, false, false, 0, 0, 0, 0, 1, from.Serial, 91, 0));

            Effects.PlaySound(from.Location, from.Map, 0x100);
            Effects.PlaySound(from.Location, from.Map, 0x0FF);

            pm.AcceleratedStart = DateTime.UtcNow + TimeSpan.FromMinutes(15);

            Table[from] = Timer.DelayCall(TimeSpan.FromMinutes(15), new TimerStateCallback(Expire_Callback), from);

            pm.AcceleratedSkill = Skill;

            BuffInfo.AddBuff(pm, new BuffInfo(BuffIcon.ArcaneEmpowerment, 1078511, 1078512, TimeSpan.FromMinutes(15), pm, GetName(), true));

            Delete();
        }        

        private static void Expire_Callback(object state)
        {
            AlacrityEnd((Mobile)state);
        }

        public static bool AlacrityEnd(Mobile m)
        {
            Table.Remove(m);

            m.PlaySound(0x100);

            BuffInfo.RemoveBuff(m, BuffIcon.ArcaneEmpowerment);

            m.SendLocalizedMessage(1077957);// The intense energy dissipates. You are no longer under the effects of an accelerated skillgain scroll.

            return true;
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
            int version = InheritsItem ? 0 : reader.ReadInt(); //Required for SpecialScroll insertion
        }
    }
}
