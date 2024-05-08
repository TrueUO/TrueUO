
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.First
{
    public class WeakenSpell : MagerySpell
    {
        private static readonly SpellInfo _Info = new SpellInfo(
            "Weaken", "Des Mani",
            212,
            9031,
            Reagent.Garlic,
            Reagent.Nightshade);

        private static Dictionary<Mobile, Timer> _Table = new Dictionary<Mobile, Timer>();

        public static bool IsUnderEffects(Mobile m)
        {
            return _Table.ContainsKey(m);
        }

        public static void RemoveEffects(Mobile m, bool removeMod = true)
        {
            if (_Table.TryGetValue(m, out Timer value))
            {
                if (value != null && value.Running)
                {
                    value.Stop();
                }

                BuffInfo.RemoveBuff(m, BuffIcon.Weaken);

                if (removeMod)
                {
                    m.RemoveStatMod("[Magic] Str Curse");
                }

                _Table.Remove(m);
            }
        }

        public WeakenSpell(Mobile caster, Item scroll)
            : base(caster, scroll, _Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.First;

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

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);
                SpellHelper.CheckReflect(this, Caster, ref m);

                if (Mysticism.StoneFormSpell.CheckImmunity(m))
                {
                    Caster.SendLocalizedMessage(1080192); // Your target resists your ability reduction magic.
                    return;
                }

                int oldOffset = SpellHelper.GetCurseOffset(m, StatType.Str);
                int newOffset = SpellHelper.GetOffset(Caster, m, StatType.Str, true, false);

                if (-newOffset > oldOffset || newOffset == 0)
                {
                    DoHurtFizzle();
                }
                else
                {
                    m.Spell?.OnCasterHurt();

                    m.Paralyzed = false;

                    m.FixedParticles(0x3779, 10, 15, 5002, EffectLayer.Head);
                    m.PlaySound(0x1DF);

                    HarmfulSpell(m);

                    if (-newOffset < oldOffset)
                    {
                        SpellHelper.AddStatCurse(Caster, m, StatType.Str, false, newOffset);

                        int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
                        TimeSpan length = SpellHelper.GetDuration(Caster, m);
                        BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Weaken, 1075837, length, m, percentage.ToString()));

                        if (_Table.TryGetValue(m, out Timer value))
                        {
                            value.Stop();
                        }

                        _Table[m] = Timer.DelayCall(length, () =>
                        {
                            RemoveEffects(m);
                        });
                    }
                }
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private readonly WeakenSpell _Owner;

            public InternalTarget(WeakenSpell owner)
                : base(10, false, TargetFlags.Harmful)
            {
                _Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile mobile)
                {
                    _Owner.Target(mobile);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                _Owner.FinishSequence();
            }
        }
    }
}
