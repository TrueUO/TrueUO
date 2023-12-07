using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class HitLower
    {
        private static readonly TimeSpan _AttackEffectDuration = TimeSpan.FromSeconds(10.0);
        private static readonly TimeSpan _DefenseEffectDuration = TimeSpan.FromSeconds(8.0);

        private static readonly Dictionary<Mobile, AttackTimer> _AttackTable = new Dictionary<Mobile, AttackTimer>();
        private static readonly Dictionary<Mobile, DefenseTimer> _DefenseTable = new Dictionary<Mobile, DefenseTimer>();

        public static bool IsUnderAttackEffect(Mobile m)
        {
            return _AttackTable.ContainsKey(m);
        }

        public static bool ApplyAttack(Mobile m)
        {
            if (IsUnderAttackEffect(m))
            {
                return false;
            }

            _AttackTable[m] = new AttackTimer(m);
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.HitLowerAttack, 1151315, 1151314, _AttackEffectDuration, m, "25"));
            m.SendLocalizedMessage(1062319); // Your attack chance has been reduced!

            m.Delta(MobileDelta.WeaponDamage);

            return true;
        }

        public static bool IsUnderDefenseEffect(Mobile m)
        {
            return _DefenseTable.ContainsKey(m);
        }

        public static bool ApplyDefense(Mobile m)
        {
            if (_DefenseTable.TryGetValue(m, out DefenseTimer value) && value != null)
            {
                value.Stop();
                value.DefenseMalus = 0;
            }

            int malus;

            if (m is PlayerMobile)
            {
                malus = 45 + BaseArmor.GetRefinedDefenseChance(m);
                malus = malus - (int)(malus * .35);
            }
            else
            {
                malus = 25;
            }

            _DefenseTable[m] = new DefenseTimer(m, malus);
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.HitLowerDefense, 1151313, 1151286, _DefenseEffectDuration, m, malus.ToString()));
            m.SendLocalizedMessage(1062318); // Your defense chance has been reduced!

            m.Delta(MobileDelta.WeaponDamage);

            return true;
        }

        private static void RemoveAttack(Mobile m)
        {
            if (_AttackTable.Remove(m))
            {
                m.SendLocalizedMessage(1062320); // Your attack chance has returned to normal.
            }
        }

        private static void RemoveDefense(Mobile m)
        {
            if (_DefenseTable.Remove(m))
            {
                m.SendLocalizedMessage(1062321); // Your defense chance has returned to normal.

                m.Delta(MobileDelta.WeaponDamage);
            }
        }

        public static int GetDefenseMalus(Mobile m)
        {
            if (_DefenseTable.TryGetValue(m, out DefenseTimer value))
            {
                return value.DefenseMalus;
            }

            return 0;
        }

        private class AttackTimer : Timer
        {
            private readonly Mobile _Player;

            public AttackTimer(Mobile player)
                : base(_AttackEffectDuration)
            {
                _Player = player;

                Start();
            }

            protected override void OnTick()
            {
                RemoveAttack(_Player);
            }
        }

        private class DefenseTimer : Timer
        {
            private readonly Mobile _Player;

            public int DefenseMalus { get; set; }

            public DefenseTimer(Mobile player, int malus)
                : base(_DefenseEffectDuration)
            {
                _Player = player;
                DefenseMalus = malus;

                Start();
            }

            protected override void OnTick()
            {
                RemoveDefense(_Player);
            }
        }
    }
}
