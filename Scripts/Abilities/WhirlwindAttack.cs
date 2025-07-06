using Server.Spells;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    /// <summary>
    /// A godsend to a warrior surrounded, the Whirlwind Attack allows the fighter to strike at all nearby targets in one mighty spinning swing.
    /// </summary>
    public sealed class WhirlwindAttack : WeaponAbility
    {
        public override int BaseMana => 15;

        public static List<WhirlwindAttackContext> Contexts { get; set; } 

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            if (attacker.Weapon is BaseWeapon wep)
            {
                wep.ProcessingMultipleHits = true;
            }

            return true;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            bool any = false;

            if (Contexts != null)
            {
                for (var index = 0; index < Contexts.Count; index++)
                {
                    var c = Contexts[index];

                    if (Contexts != null && c.Attacker == attacker)
                    {
                        any = true;
                        break;
                    }
                }
            }

            if (!Validate(attacker) || Contexts != null && any)
            {
                return;
            }

            ClearCurrentAbility(attacker);

            Map map = attacker.Map;

            if (map == null)
                return;

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            if (weapon == null)
                return;

            if (!CheckMana(attacker, true))
                return;

            attacker.FixedEffect(0x3728, 10, 15);
            attacker.PlaySound(0x2A1);

            List<Mobile> list = new List<Mobile>();

            foreach (IDamageable target in SpellHelper.AcquireIndirectTargets(attacker, attacker, attacker.Map, 1))
            {
                if (target is Mobile m && (attacker.InRange(m, weapon.MaxRange) && m != defender))
                {
                    list.Add(m);
                }
            }

            int count = list.Count;

            if (count > 0)
            {
                double bushido = attacker.Skills.Bushido.Value;

                int bonus = 0;

                if (bushido > 0)
                {
                    bonus = (int)Math.Min(100, ((list.Count * bushido) * (list.Count * bushido)) / 3600);
                }

                var context = new WhirlwindAttackContext(attacker, list, bonus);
                AddContext(context);

                attacker.RevealingAction();

                for (var index = 0; index < list.Count; index++)
                {
                    Mobile m = list[index];

                    attacker.SendLocalizedMessage(1060161); // The whirling attack strikes a target!
                    m.SendLocalizedMessage(1060162); // You are struck by the whirling attack and take damage!

                    weapon.OnHit(attacker, m);
                }

                RemoveContext(context);
            }

            ColUtility.Free(list);

            weapon.ProcessingMultipleHits = false;
        }

        private static void AddContext(WhirlwindAttackContext context)
        {
            if (Contexts == null)
            {
                Contexts = new List<WhirlwindAttackContext>();
            }

            Contexts.Add(context);
        }

        private static void RemoveContext(WhirlwindAttackContext context)
        {
            if (Contexts != null)
            {
                Contexts.Remove(context);
            }
        }

        public static int DamageBonus(Mobile attacker, Mobile defender)
        {
            if (Contexts == null)
            {
                return 0;
            }

            WhirlwindAttackContext context = null;

            for (var index = 0; index < Contexts.Count; index++)
            {
                var c = Contexts[index];

                if (c.Attacker == attacker && c.Victims.Contains(defender))
                {
                    context = c;
                    break;
                }
            }

            if (context != null)
            {
                return context.DamageBonus;
            }

            return 0;
        }

        public class WhirlwindAttackContext
        {
            public Mobile Attacker { get; }
            public List<Mobile> Victims { get; }
            public int DamageBonus { get; }

            public WhirlwindAttackContext(Mobile attacker, List<Mobile> list, int bonus)
            {
                Attacker = attacker;
                Victims = list;
                DamageBonus = bonus;
            }
        }
    }
}
