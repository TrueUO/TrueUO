using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
    public class Focus
    {
        private static readonly Dictionary<Mobile, FocusInfo> _Table = new Dictionary<Mobile, FocusInfo>();
        private const int _DefaultDamageBonus = -40;

        public class FocusInfo
        {
            public Mobile Target { get; set; }
            public int DamageBonus { get; set; }

            public FocusInfo(Mobile defender, int bonus)
            {
                Target = defender;
                DamageBonus = bonus;
            }
        }

        public static void OnLogin(Mobile m)
        {
            if (m is PlayerMobile pm)
            {
                UpdateBuff(pm);
            }
        }

        public static void UpdateBuff(Mobile from, Mobile target = null)
        {
            Item item = from.FindItemOnLayer(Layer.TwoHanded);

            if (item == null)
            {
                item = from.FindItemOnLayer(Layer.OneHanded);
            }

            if (item == null)
            {
                if (_Table.Remove(from))
                {
                    BuffInfo.RemoveBuff(from, BuffIcon.RageFocusingBuff);
                }
            }
            else if (item is BaseWeapon weapon && weapon.ExtendedWeaponAttributes.Focus > 0)
            {
                if (_Table.TryGetValue(from, out FocusInfo value))
                {
                    BuffInfo.AddBuff(from, new BuffInfo(BuffIcon.RageFocusingBuff, 1151393, 1151394, $"{(value.Target == null ? "NONE" : value.Target.Name)}\t{value.DamageBonus}"));
                }

                _Table[from] = new FocusInfo(target, _DefaultDamageBonus);
            }
        }

        public static int GetBonus(Mobile from, Mobile target)
        {
            if (_Table.TryGetValue(from, out FocusInfo value) && value.Target == target)
            {
                return value.DamageBonus;
            }

            return 0;
        }

        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (_Table.TryGetValue(attacker, out FocusInfo value))
            {
                if (value.Target == null)
                {
                    value.DamageBonus -= 10;
                }
                else if (value.Target == defender)
                {
                    if (value.DamageBonus < -40)
                    {
                        value.DamageBonus += 10;
                    }
                    else
                    {
                        value.DamageBonus += 8;
                    }
                }
                else
                {
                    if (value.DamageBonus >= -50)
                    {
                        value.DamageBonus = _DefaultDamageBonus;
                    }
                }

                if (value.Target != defender)
                {
                    value.Target = defender;
                }

                UpdateBuff(attacker, defender);
            }
        }
    }
}
