using Server.Misc;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public interface IEpiphanyArmor
    {
        Alignment Alignment { get; }
        SurgeType Type { get; }
        int Frequency { get; }
        int Bonus { get; }
    }

    public static class EpiphanyHelper
    {
        public static Dictionary<Mobile, Dictionary<SurgeType, int>> Table { get; set; }

        public static readonly int MinTriggerDamage = 15; // TODO: Amount?

        public static int GetFrequency(Mobile m, IEpiphanyArmor armor)
        {
            if (m == null)
            {
                return 1;
            }

            int count = 0;

            for (int index = 0; index < m.Items.Count; index++)
            {
                Item i = m.Items[index];

                if (i is IEpiphanyArmor eArmor && eArmor.Alignment == armor.Alignment && eArmor.Type == armor.Type)
                {
                    count++;
                }
            }

            return Math.Max(1, Math.Min(5, count));
        }

        public static int GetBonus(Mobile m, IEpiphanyArmor armor)
        {
            if (m == null)
            {
                return 0;
            }

            switch (armor.Alignment)
            {
                default:
                {
                    return 0;
                }
                case Alignment.Good:
                {
                    if (m.Karma <= 0)
                    {
                        return 0;
                    }

                    return Math.Min(20, m.Karma / (Titles.MaxKarma / 20));
                }
                case Alignment.Evil:
                {
                    if (m.Karma >= 0)
                    {
                        return 0;
                    }

                    return Math.Min(20, -m.Karma / (Titles.MaxKarma / 20));
                }
            }
        }

        public static void OnHit(Mobile m, int damage)
        {
            if (damage > MinTriggerDamage)
            {
                CheckHit(m, damage, SurgeType.Hits);
                CheckHit(m, damage, SurgeType.Stam);
                CheckHit(m, damage, SurgeType.Mana);
            }
        }

        public static void CheckHit(Mobile m, int damage, SurgeType type)
        {
            IEpiphanyArmor item = null;

            for (int index = 0; index < m.Items.Count; index++)
            {
                Item mItem = m.Items[index];

                if (mItem is IEpiphanyArmor i && i.Type == type)
                {
                    item = i;
                    break;
                }
            }

            if (item == null)
            {
                return;
            }

            if (Table == null)
            {
                Table = new Dictionary<Mobile, Dictionary<SurgeType, int>>();
            }

            if (!Table.TryGetValue(m, out Dictionary<SurgeType, int> value))
            {
                value = new Dictionary<SurgeType, int>();

                Table[m] = value;
            }

            if (!value.TryGetValue(type, out int typeValue))
            {
                value[type] = damage;
            }
            else
            {
                damage += typeValue;
            }

            int freq = GetFrequency(m, item);
            int bonus = GetBonus(m, item);

            if (freq > 0 && bonus > 0 && damage > Utility.Random(10000 / freq))
            {
                value.Remove(type);

                if (value.Count == 0)
                {
                    Table.Remove(m);
                }

                switch (type)
                {
                    case SurgeType.Hits:
                    {
                        m.Hits = Math.Min(m.HitsMax, m.Hits + bonus); break;
                    }
                    case SurgeType.Stam:
                    {
                        m.Hits = Math.Min(m.HitsMax, m.Hits + bonus); break;
                    }
                    default:
                    case SurgeType.Mana:
                    {
                        m.Hits = Math.Min(m.HitsMax, m.Hits + bonus); break;
                    }
                }
            }
            else
            {
                Table[m][type] = damage;
            }
        }

        public static void OnKarmaChange(Mobile m)
        {
            for (int index = 0; index < m.Items.Count; index++)
            {
                Item item = m.Items[index];

                if (item is IEpiphanyArmor)
                {
                    item.InvalidateProperties();
                }
            }
        }

        public static void AddProperties(IEpiphanyArmor item, ObjectPropertyList list)
        {
            if (item == null)
            {
                return;
            }

            switch (item.Type)
            {
                case SurgeType.Hits:
                {
                    list.Add(1150829 + (int)item.Alignment); // Set Ability: good healing burst
                    break;
                }
                case SurgeType.Stam: // NOTE: This doesn't exist on EA, but put it in here anyways!
                {
                    list.Add(1149953, $"Set Ability\t{(item.Alignment == Alignment.Evil ? "evil stamina burst" : "good stamina burst")}");
                    break;
                }
                default:
                case SurgeType.Mana:
                {
                    list.Add(1150240 + (int)item.Alignment); // Set Ability: evil mana burst
                    break;
                }
            }

            if (item is Item eItem)
            {
                list.Add(1150240, GetFrequency(eItem.Parent as Mobile, item).ToString()); // Set Bonus: Frequency ~1_val~
                list.Add(1150243, GetBonus(eItem.Parent as Mobile, item).ToString()); // Karma Bonus: Burst level ~1_val~
            }
        }
    }
}
