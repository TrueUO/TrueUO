using Server.Network;

using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Mysticism
{
    public class SleepSpell : MysticSpell
    {
        public override SpellCircle Circle => SpellCircle.Third;

        private static readonly SpellInfo _Info = new SpellInfo(
                "Sleep", "In Zu",
                230,
                9022,
                Reagent.Nightshade,
                Reagent.SpidersSilk,
                Reagent.BlackPearl
            );

        public SleepSpell(Mobile caster, Item scroll)
            : base(caster, scroll, _Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool OnInstantCast(IEntity target)
        {
            Target t = new InternalTarget(this);
            if (Caster.InRange(target, t.Range) && Caster.InLOS(target))
            {
                t.Invoke(Caster, target);
                return true;
            }
            else
                return false;
        }

        public void OnTarget(object o)
        {
            Mobile target = o as Mobile;

            if (target == null)
            {
                return;
            }

            if (target.Paralyzed)
            {
                Caster.SendLocalizedMessage(1080134); //Your target is already immobilized and cannot be slept.
            }
            else if (_ImmunityList.Contains(target))
            {
                Caster.SendLocalizedMessage(1080135); //Your target cannot be put to sleep.
            }
            else if (CheckHSequence(target))
            {
                SpellHelper.CheckReflect(this, Caster, ref target);

                double duration = ((Caster.Skills[CastSkill].Value + Caster.Skills[DamageSkill].Value) / 20) + 2;
                duration -= GetResistSkill(target) / 10;

                if (duration <= 0 || StoneFormSpell.CheckImmunity(target))
                {
                    Caster.SendLocalizedMessage(1080136); //Your target resists sleep.
                    target.SendLocalizedMessage(1080137); //You resist sleep.
                }
                else
                {
                    DoSleep(Caster, target, TimeSpan.FromSeconds(duration));
                }
            }

            FinishSequence();
        }

        private static readonly Dictionary<Mobile, SleepTimer> _Table = new Dictionary<Mobile, SleepTimer>();
        private static readonly List<Mobile> _ImmunityList = new List<Mobile>();

        public static void DoSleep(Mobile caster, Mobile target, TimeSpan duration)
        {
            target.Combatant = null;
            target.SendSpeedControl(SpeedControlType.WalkSpeed);

            if (_Table.TryGetValue(target, out SleepTimer value))
            {
                value.Stop();
            }

            _Table[target] = new SleepTimer(target, duration);

            BuffInfo.AddBuff(target, new BuffInfo(BuffIcon.Sleep, 1080139, 1080140, duration, target));

            target.Delta(MobileDelta.WeaponDamage);
        }

        public static void AddToSleepTable(Mobile from, TimeSpan duration)
        {
            _Table.Add(from, new SleepTimer(from, duration));
        }

        public static bool IsUnderSleepEffects(Mobile from)
        {
            return _Table.ContainsKey(from);
        }

        public static void OnDamage(Mobile from)
        {
            if (_Table.ContainsKey(from))
            {
                EndSleep(from);
            }
        }

        public class SleepTimer : Timer
        {
            private readonly Mobile _Target;
            private readonly DateTime _EndTime;

            public SleepTimer(Mobile target, TimeSpan duration)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(0.5))
            {
                _EndTime = DateTime.UtcNow + duration;
                _Target = target;

                Start();
            }

            protected override void OnTick()
            {
                if (_EndTime < DateTime.UtcNow)
                {
                    EndSleep(_Target);
                    Stop();
                }
                else
                {
                    Effects.SendTargetParticles(_Target, 0x3779, 1, 32, 0x13BA, EffectLayer.Head);
                }
            }
        }

        public static void EndSleep(Mobile target)
        {
            if (_Table.TryGetValue(target, out SleepTimer value))
            {
                target.SendSpeedControl(SpeedControlType.Disable);

                value.Stop();

                _Table.Remove(target);

                BuffInfo.RemoveBuff(target, BuffIcon.Sleep);

                double immuneDuration = target.Skills[SkillName.MagicResist].Value / 10;

                _ImmunityList.Add(target);
                Timer.DelayCall(TimeSpan.FromSeconds(immuneDuration), RemoveImmunity_Callback, target);

                target.Delta(MobileDelta.WeaponDamage);
            }
        }

        public static void RemoveImmunity_Callback(object state)
        {
            Mobile m = (Mobile)state;

            _ImmunityList.Remove(m);
        }

        public class InternalTarget : Target
        {
            public SleepSpell Owner { get; }

            public InternalTarget(SleepSpell owner)
                : this(owner, false)
            {
            }

            public InternalTarget(SleepSpell owner, bool allowland)
                : base(12, allowland, TargetFlags.Harmful)
            {
                Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o == null)
                {
                    return;
                }

                if (!from.CanSee(o))
                {
                    from.SendLocalizedMessage(500237); // Target can not be seen.
                }
                else
                {
                    SpellHelper.Turn(from, o);
                    Owner.OnTarget(o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                Owner.FinishSequence();
            }
        }
    }
}
