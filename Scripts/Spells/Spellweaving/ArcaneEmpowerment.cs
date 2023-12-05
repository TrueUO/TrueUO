using System;
using System.Collections;

namespace Server.Spells.Spellweaving
{
    public class ArcaneEmpowermentSpell : ArcanistSpell
    {
        private static readonly SpellInfo _Info = new SpellInfo(
            "Arcane Empowerment", "Aslavdra",
            -1);

        private static readonly Hashtable _Table = new Hashtable();

        public ArcaneEmpowermentSpell(Mobile caster, Item scroll)
            : base(caster, scroll, _Info)
        {
        }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(3);
        public override double RequiredSkill => 24.0;
        public override int RequiredMana => 50;

        public static double GetDispellBonus(Mobile m)
        {
            if (_Table[m] is EmpowermentInfo info)
            {
                return 10.0 * info.Focus;
            }

            return 0.0;
        }

        public static int GetSpellBonus(Mobile m, bool playerVsPlayer)
        {
            if (_Table[m] is EmpowermentInfo info)
            {
                return info.Bonus + (playerVsPlayer ? info.Focus : 0);
            }

            return 0;
        }

        public static void AddHealBonus(Mobile m, ref int toHeal)
        {
            if (_Table[m] is EmpowermentInfo info)
            {
                toHeal = (int)Math.Floor((1 + (10 + info.Bonus) / 100.0) * toHeal);
            }
        }

        public static bool IsUnderEffects(Mobile m)
        {
            return _Table.ContainsKey(m);
        }

        public override void OnCast()
        {
            if (_Table.ContainsKey(Caster))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
            }
            else if (CheckSequence())
            {
                Caster.PlaySound(0x5C1);

                int level = GetFocusLevel(Caster);
                double skill = Caster.Skills[SkillName.Spellweaving].Value;

                TimeSpan duration = TimeSpan.FromSeconds(15 + (int)(skill / 24) + level * 2);
                int bonus = (int)Math.Floor(skill / 12) + level * 5;

                _Table[Caster] = new EmpowermentInfo(Caster, duration, bonus, level);

                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.ArcaneEmpowerment, 1031616, 1075808, duration, Caster, new TextDefinition($"{bonus}\t10")));

                Caster.Delta(MobileDelta.WeaponDamage);
            }

            FinishSequence();
        }

        private class EmpowermentInfo
        {
            public readonly int Bonus;
            public readonly int Focus;
            public readonly ExpireTimer Timer;

            public EmpowermentInfo(Mobile caster, TimeSpan duration, int bonus, int focus)
            {
                Bonus = bonus;
                Focus = focus;

                Timer = new ExpireTimer(caster, duration);
                Timer.Start();
            }
        }

        private class ExpireTimer : Timer
        {
            private readonly Mobile _Mobile;

            public ExpireTimer(Mobile m, TimeSpan delay)
                : base(delay)
            {
                _Mobile = m;
            }

            protected override void OnTick()
            {
                _Mobile.PlaySound(0x5C2);
                _Table.Remove(_Mobile);

                _Mobile.Delta(MobileDelta.WeaponDamage);
            }
        }
    }
}
