using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    /// <summary>
    /// A quick attack to all enemies in range of your weapon that causes damage over time. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public class FrenziedWhirlwind : WeaponAbility
    {
        public override SkillName GetSecondarySkill(Mobile from)
        {
            return from.Skills[SkillName.Ninjitsu].Base > from.Skills[SkillName.Bushido].Base ? SkillName.Ninjitsu : SkillName.Bushido;
        }

        public override int BaseMana => 20;

        private static readonly Dictionary<Mobile, Timer> _Registry = new Dictionary<Mobile, Timer>();
        public static Dictionary<Mobile, Timer> Registry => _Registry;

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker)) // Mana check after check that there are targets
            {
                return;
            }

            ClearCurrentAbility(attacker);

            Map map = attacker.Map;

            if (map == null)
            {
                return;
            }

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            if (weapon == null)
            {
                return;
            }

            List<Mobile> targets = new List<Mobile>();

            foreach (IDamageable target in SpellHelper.AcquireIndirectTargets(attacker, attacker.Location, attacker.Map, 2))
            {
                if (target is Mobile mobile)
                {
                    targets.Add(mobile);
                }
            }

            if (targets.Count > 0)
            {
                if (!CheckMana(attacker, true))
                {
                    return;
                }

                attacker.FixedEffect(0x3728, 10, 15);
                attacker.PlaySound(0x2A1);

                if (_Registry.ContainsKey(attacker))
                {
                    RemoveFromRegistry(attacker);
                }

                _Registry[attacker] = new InternalTimer(attacker, targets);

                for (int index = 0; index < targets.Count; index++)
                {
                    Mobile target = targets[index];

                    if (target is PlayerMobile pm)
                    {
                        BuffInfo.AddBuff(pm, new BuffInfo(BuffIcon.SplinteringEffect, 1153804, 1028852, TimeSpan.FromSeconds(2.0), pm));
                    }
                }

                if (defender is PlayerMobile && attacker is PlayerMobile)
                {
                    defender.SendSpeedControl(SpeedControlType.WalkSpeed);
                    BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.SplinteringEffect, 1153804, 1152144, TimeSpan.FromSeconds(2.0), defender));
                    Timer.DelayCall(TimeSpan.FromSeconds(2), mob => mob.SendSpeedControl(SpeedControlType.Disable), defender);
                }

                if (attacker is BaseCreature bc)
                {
                    PetTrainingHelper.OnWeaponAbilityUsed(bc, SkillName.Ninjitsu);
                }
            }
        }

        public static void RemoveFromRegistry(Mobile from)
        {
            if (_Registry.TryGetValue(from, out Timer value))
            {
                value.Stop();

                _Registry.Remove(from);
            }
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile _Attacker;
            private readonly List<Mobile> _List;
            private readonly long _Start;

            public InternalTimer(Mobile attacker, List<Mobile> list)
                : base(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500))
            {
                _Attacker = attacker;
                _List = list;

                _Start = Core.TickCount;

                DoHit();

                Start();
            }

            protected override void OnTick()
            {
                if (_Attacker.Alive)
                {
                    DoHit();
                }

                if (!_Attacker.Alive || _Start + 2000 < Core.TickCount)
                {
                    ColUtility.Free(_List);
                    RemoveFromRegistry(_Attacker);
                }
            }

            private void DoHit()
            {
                if (_List == null)
                {
                    return;
                }

                for (int index = 0; index < _List.Count; index++)
                {
                    Mobile m = _List[index];

                    if (_Attacker.InRange(m.Location, 2) && m.Alive && m.Map == _Attacker.Map)
                    {
                        _Attacker.FixedEffect(0x3728, 10, 15);
                        _Attacker.PlaySound(0x2A1);

                        int skill = _Attacker is BaseCreature ? (int) _Attacker.Skills[SkillName.Ninjitsu].Value : (int) Math.Max(_Attacker.Skills[SkillName.Bushido].Value, _Attacker.Skills[SkillName.Ninjitsu].Value);

                        int baseMin = Math.Max(5, (skill / 50) * 5);
                        AOS.Damage(m, _Attacker, Utility.RandomMinMax(baseMin, (baseMin * 3) + 2), 100, 0, 0, 0, 0);
                    }
                }
            }
        }
    }
}
