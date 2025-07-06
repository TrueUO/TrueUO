using System;
using System.Collections.Generic;

namespace Server.Items
{
    public sealed class PsychicAttack : WeaponAbility
    {
        public override int BaseMana => 30;

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
            {
                return;
            }

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1074383); // Your shot sends forth a wave of psychic energy.
            defender.SendLocalizedMessage(1074384); // Your mind is attacked by psychic force!

            defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
            defender.PlaySound(0x1F8);

            if (_Registry.TryGetValue(defender, out PsychicAttackTimer value))
            {
                if (!value.DoneIncrease)
                {
                    value.SpellDamageMalus *= 2;
                    value.ManaCostMalus *= 2;
                }
            }
            else
            {
                _Registry[defender] = new PsychicAttackTimer(defender);
            }

            BuffInfo.RemoveBuff(defender, BuffIcon.PsychicAttack);

            string args = $"{_Registry[defender].SpellDamageMalus}\t{_Registry[defender].ManaCostMalus}";
            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.PsychicAttack, 1151296, 1151297, args));
        }

        private static readonly Dictionary<Mobile, PsychicAttackTimer> _Registry = new Dictionary<Mobile, PsychicAttackTimer>();
        public static Dictionary<Mobile, PsychicAttackTimer> Registry => _Registry;

        public static void RemoveEffects(Mobile defender)
        {
            if (defender == null)
            {
                return;
            }

            BuffInfo.RemoveBuff(defender, BuffIcon.PsychicAttack);

            _Registry.Remove(defender);

            defender.SendLocalizedMessage(1150292); // You recover from the effects of the psychic attack.
        }

        public class PsychicAttackTimer : Timer
        {
            private readonly Mobile _Defender;
            private int _SpellDamageMalus;
            private int _ManaCostMalus;
            private bool _DoneIncrease;

            public int SpellDamageMalus { get => _SpellDamageMalus; set { _SpellDamageMalus = value; _DoneIncrease = true; } }
            public int ManaCostMalus { get => _ManaCostMalus; set { _ManaCostMalus = value; _DoneIncrease = true; } }
            public bool DoneIncrease => _DoneIncrease;

            public PsychicAttackTimer(Mobile defender)
                : base(TimeSpan.FromSeconds(10))
            {
                _Defender = defender;
                _SpellDamageMalus = 15;
                _ManaCostMalus = 15;
                _DoneIncrease = false;

                Start();
            }

            protected override void OnTick()
            {
                RemoveEffects(_Defender);
                Stop();
            }
        }
    }
}
