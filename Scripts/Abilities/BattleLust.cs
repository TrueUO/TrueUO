using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server
{
    public class BattleLust
    {
        private static readonly Dictionary<Mobile, BattleLustTimer> _Table = new Dictionary<Mobile, BattleLustTimer>();

        public static int GetBonus(Mobile attacker, Mobile defender)
        {
            if (!_Table.TryGetValue(attacker, out BattleLustTimer value))
            {
                return 0;
            }

            int bonus = value.Bonus * attacker.Aggressed.Count;

            if (defender is PlayerMobile && bonus > 45)
            {
                bonus = 45;
            }
            else if (bonus > 90)
            {
                bonus = 90;
            }

            return bonus;
        }

        public static void IncreaseBattleLust(Mobile m, int damage)
        {
            if (damage < 30)
            {
                return;
            }

            if (AosWeaponAttributes.GetValue(m, AosWeaponAttribute.BattleLust) == 0)
            {
                return;
            }

            if (_Table.TryGetValue(m, out BattleLustTimer value))
            {
                if (value.CanGain)
                {
                    if (value.Bonus < 16)
                    {
                        value.Bonus++;
                    }

                    value.CanGain = false;
                }
            }
            else
            {
                BattleLustTimer blt = new BattleLustTimer(m, 1);
                blt.Start();
                _Table.Add(m, blt);
                m.SendLocalizedMessage(1113748); // The damage you received fuels your battle fury.
            }
        }

        public static bool DecreaseBattleLust(Mobile m)
        {
            if (_Table.TryGetValue(m, out BattleLustTimer value))
            {
                value.Bonus--;

                if (value.Bonus <= 0)
                {
                    _Table.Remove(m);

                    // No Message?
                    //m.SendLocalizedMessage( 0 ); //

                    return false;
                }
            }

            return true;
        }

        public class BattleLustTimer : Timer
        {
            public int Bonus;
            public bool CanGain;

            private readonly Mobile _Mobile;
            private int _Count;

            public BattleLustTimer(Mobile m, int bonus)
                : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                _Mobile = m;
                Bonus = bonus;
                _Count = 1;
            }

            protected override void OnTick()
            {
                _Count %= 3;

                if (_Count == 0)
                {
                    if (!DecreaseBattleLust(_Mobile))
                    {
                        Stop();
                    }
                }
                else
                {
                    CanGain = true;
                }

                _Count++;
            }
        }
    }
}
