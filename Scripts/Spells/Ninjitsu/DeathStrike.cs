using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Ninjitsu
{
    public class DeathStrike : NinjaMove
    {
        private static readonly TimeSpan _DamageDelay = TimeSpan.FromSeconds(3.0);
        private static Dictionary<Mobile, DeathStrikeInfo> _Table = new Dictionary<Mobile, DeathStrikeInfo>();

        public override int BaseMana => 30;
        public override double RequiredSkill => 85.0;
        public override TextDefinition AbilityMessage => new TextDefinition(1063091);// You prepare to hit your opponent with a Death Strike.

        public static void AddStep(Mobile m)
        {
            if (!_Table.TryGetValue(m, out DeathStrikeInfo value))
            {
                return;
            }

            if (++value.Steps >= 5)
            {
                ProcessDeathStrike(value);
            }
        }

        public override double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            return 0.5;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
            {
                return;
            }

            ClearCurrentMove(attacker);            

            if (!attacker.CheckSkill(MoveSkill, RequiredSkill - 12.5, RequiredSkill + 37.5))
            {
                attacker.SendLocalizedMessage(1070779); // You missed your opponent with a Death Strike.
                return;
            }

            DeathStrikeInfo info;

            if (_Table.TryGetValue(defender, out DeathStrikeInfo value))
            {
                defender.SendLocalizedMessage(1063092); // Your opponent lands another Death Strike!

                info = value;

                info.Timer?.Stop();

                _Table.Remove(defender);
            }
            else
            {
                defender.SendLocalizedMessage(1063093); // You have been hit by a Death Strike!  Move with caution!
            }

            attacker.SendLocalizedMessage(1063094); // You inflict a Death Strike upon your opponent!

            defender.FixedParticles(0x374A, 1, 17, 0x26BC, EffectLayer.Waist);
            attacker.PlaySound(attacker.Female ? 0x50D : 0x50E);

            double ninjitsu = attacker.Skills[SkillName.Ninjitsu].Base;
            double hiding = attacker.Skills[SkillName.Hiding].Base;
            double stealth = attacker.Skills[SkillName.Stealth].Base;

            double average = (hiding + stealth) / 100;

            double scalar = ninjitsu / 9;

            double baseDamage = ninjitsu / 5.7;

            int totalDamage = (int)(baseDamage + average * scalar);

            if (totalDamage > 50 && attacker is PlayerMobile && defender is PlayerMobile)
            {
                totalDamage = 50;
            }

            info = new DeathStrikeInfo(defender, attacker, totalDamage, attacker.Weapon is BaseRanged);
            info.Timer = Timer.DelayCall(_DamageDelay, () => ProcessDeathStrike(info));

            _Table[defender] = info;

            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.DeathStrike, 1075645, _DamageDelay, defender, $"{totalDamage}"));
        }

        private static void ProcessDeathStrike(DeathStrikeInfo info)
        {
            int damage = info.Damage;

            if (info.IsRanged)
            {
                damage /= 2;
            }

            if (info.Steps < 5)
            {
                damage /= 3;
            }

            AOS.Damage(info.Target, info.Attacker, damage, 0, 0, 0, 0, 0, 0, 100); // Damage is direct.

            info.Timer?.Stop();

            _Table.Remove(info.Target);
        }

        private class DeathStrikeInfo
        {
            public readonly Mobile Target;
            public readonly Mobile Attacker;
            public readonly int Damage;
            public readonly bool IsRanged;
            public int Steps;
            public Timer Timer;

            public DeathStrikeInfo(Mobile target, Mobile attacker, int damageBonus, bool isRanged)
            {
                Target = target;
                Attacker = attacker;
                Damage = damageBonus;
                IsRanged = isRanged;
            }
        }

        public static void Initialize()
        {
            EventSink.Movement += EventSink_Movement;
        }

        public static void EventSink_Movement(MovementEventArgs e)
        {
            AddStep(e.Mobile);
        }
    }
}
