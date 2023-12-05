using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    /// <summary>
    /// Make your opponent bleed profusely with this wicked use of your weapon.
    /// When successful, the target will bleed for several seconds, taking damage as time passes for up to ten seconds.
    /// The rate of damage slows down as time passes, and the blood loss can be completely staunched with the use of bandages. 
    /// </summary>
    public class BleedAttack : WeaponAbility
    {
        private static readonly Dictionary<Mobile, BleedTimer> _BleedTable = new Dictionary<Mobile, BleedTimer>();

        public override int BaseMana => 30;

        public static bool IsBleeding(Mobile m)
        {
            return _BleedTable.ContainsKey(m);
        }

        public static void BeginBleed(Mobile m, Mobile from, bool splintering = false)
        {
            BleedTimer timer;

            if (_BleedTable.TryGetValue(m, out BleedTimer value))
            {
                if (splintering)
                {
                    timer = value;
                    timer.Stop();
                }
                else
                {
                    return;
                }
            }

            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Bleed, 1075829, 1075830, TimeSpan.FromSeconds(10), m, "1\t10\t2"));

            timer = new BleedTimer(from, m, CheckBloodDrink(from));
            _BleedTable[m] = timer;
            timer.Start();

            from.SendLocalizedMessage(1060159); // Your target is bleeding!
            m.SendLocalizedMessage(1060160); // You are bleeding!

            if (m is PlayerMobile)
            {
                m.LocalOverheadMessage(MessageType.Regular, 0x21, 1060757); // You are bleeding profusely
                m.NonlocalOverheadMessage(MessageType.Regular, 0x21, 1060758, m.Name); // ~1_NAME~ is bleeding profusely
            }

            m.PlaySound(0x133);
            m.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);
        }

        public static void DoBleed(Mobile m, Mobile from, int damage, bool blooddrinker)
        {
            if (m.Alive && !m.IsDeadBondedPet)
            {
                if (!m.Player)
                {
                    damage *= 2;
                }

                m.PlaySound(0x133);
                AOS.Damage(m, from, damage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);

                if (blooddrinker && from.Hits < from.HitsMax)
                {
                    from.SendLocalizedMessage(1113606); //The blood drinker effect heals you.
                    from.Heal(damage);
                }

                Blood blood = new Blood
                {
                    ItemID = Utility.Random(0x122A, 5)
                };
                blood.MoveToWorld(m.Location, m.Map);
            }
            else
            {
                EndBleed(m, false);
            }
        }

        public static void EndBleed(Mobile m, bool message)
        {
            Timer t = null;

            if (_BleedTable.TryGetValue(m, out BleedTimer value))
            {
                t = value;

                _BleedTable.Remove(m);
            }

            if (t == null)
            {
                return;
            }

            t.Stop();
            BuffInfo.RemoveBuff(m, BuffIcon.Bleed);

            if (message)
            {
                m.SendLocalizedMessage(1060167); // The bleeding wounds have healed, you are no longer bleeding!
            }
        }

        public static bool CheckBloodDrink(Mobile attacker)
        {
            return attacker.Weapon is BaseWeapon weapon && weapon.WeaponAttributes.BloodDrinker > 0;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
            {
                return;
            }

            ClearCurrentAbility(attacker);

            // Necromancers under Lich or Wraith Form are immune to Bleed Attacks.
            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if (context != null && (context.Type == typeof(LichFormSpell) || context.Type == typeof(WraithFormSpell)) || defender is BaseCreature bc && bc.BleedImmune || Spells.Mysticism.StoneFormSpell.CheckImmunity(defender))
            {
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
                return;
            }

            BeginBleed(defender, attacker);
        }

        private class BleedTimer : Timer
        {
            private readonly Mobile _From;
            private readonly Mobile _Mobile;
            private int _Count;
            private readonly int _MaxCount;
            private readonly bool _BloodDrinker;

            public BleedTimer(Mobile from, Mobile m, bool bloodDrinker)
                : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                _From = from;
                _Mobile = m;
                _BloodDrinker = bloodDrinker;

                _MaxCount = Spells.SkillMasteries.ResilienceSpell.UnderEffects(m) ? 3 : 5;
            }

            protected override void OnTick()
            {
                if (!_Mobile.Alive || _Mobile.Deleted)
                {
                    EndBleed(_Mobile, true);
                }
                else
                {
                    if (!Spells.SkillMasteries.WhiteTigerFormSpell.HasBleedMod(_From, out int damage))
                    {
                        damage = Math.Max(1, Utility.RandomMinMax(5 - _Count, (5 - _Count) * 2));
                    }

                    DoBleed(_Mobile, _From, damage, _BloodDrinker);

                    if (++_Count == _MaxCount)
                    {
                        EndBleed(_Mobile, true);
                    }
                }
            }
        }
    }
}
