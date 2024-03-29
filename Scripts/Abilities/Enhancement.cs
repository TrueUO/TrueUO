using System;
using System.Collections.Generic;

namespace Server
{
    public class EnhancementAttributes
    {
        public string Title { get; }

        public AosAttributes Attributes { get; }
        public AosWeaponAttributes WeaponAttributes { get; }
        public AosArmorAttributes ArmorAttributes { get; }
        public SAAbsorptionAttributes AbsorptionAttributes { get; }
        public ExtendedWeaponAttributes ExtendedWeaponAttributes { get; }

        public EnhancementAttributes(string title)
        {
            Title = title;
            Attributes = new AosAttributes(null);
            WeaponAttributes = new AosWeaponAttributes(null);
            ArmorAttributes = new AosArmorAttributes(null);
            AbsorptionAttributes = new SAAbsorptionAttributes(null);
            ExtendedWeaponAttributes = new ExtendedWeaponAttributes(null);
        }
    }

    public class Enhancement
    {
        private static Dictionary<Mobile, List<EnhancementAttributes>> _EnhancementList = new Dictionary<Mobile, List<EnhancementAttributes>>();

        public static bool AddMobile(Mobile m)
        {
            if (!_EnhancementList.ContainsKey(m))
            {
                _EnhancementList.Add(m, new List<EnhancementAttributes>());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the mobile and/or attributes from the dictionary
        /// </summary>
        /// <param name="m"></param>
        /// <param name="title">null or default value will remove the entire entry. Add the title arg to remove only that element from the list.</param>
        /// <returns></returns>
        public static bool RemoveMobile(Mobile m, string title = null)
        {
            if (_EnhancementList.TryGetValue(m, out List<EnhancementAttributes> value))
            {
                if (title != null)
                {
                    EnhancementAttributes match = null;

                    for (int index = 0; index < value.Count; index++)
                    {
                        EnhancementAttributes attrs = value[index];

                        if (attrs.Title == title)
                        {
                            match = attrs;
                            break;
                        }
                    }

                    if (match != null && value.Contains(match))
                    {
                        if (match.Attributes.BonusStr > 0)
                        {
                            m.RemoveStatMod("MagicalEnhancementStr");
                        }

                        if (match.Attributes.BonusDex > 0)
                        {
                            m.RemoveStatMod("MagicalEnhancementDex");
                        }

                        if (match.Attributes.BonusInt > 0)
                        {
                            m.RemoveStatMod("MagicalEnhancementInt");
                        }

                        value.Remove(match);
                    }
                }

                if (value.Count == 0 || title == null)
                {
                    _EnhancementList.Remove(m);
                }

                m.CheckStatTimers();
                m.UpdateResistances();
                m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);

                for (int index = 0; index < m.Items.Count; index++)
                {
                    Item i = m.Items[index];

                    i.InvalidateProperties();
                }

                return true;
            }

            return false;
        }

        public static int GetValue(Mobile m, AosAttribute att)
        {
            if (m == null)
            {
                return 0;
            }

            if (_EnhancementList.TryGetValue(m, out List<EnhancementAttributes> enhancmentValue))
            {
                int value = 0;

                for (int index = 0; index < enhancmentValue.Count; index++)
                {
                    EnhancementAttributes attrs = enhancmentValue[index];

                    value += attrs.Attributes[att];
                }

                return value;
            }

            return 0;
        }

        public static void SetValue(Mobile m, AosAttribute att, int value, string title)
        {
            if (!_EnhancementList.ContainsKey(m))
            {
                AddMobile(m);
            }

            if (att == AosAttribute.BonusStr)
            {
                m.RemoveStatMod("MagicalEnhancementStr");
                m.AddStatMod(new StatMod(StatType.Str, "MagicalEnhancementStr", value, TimeSpan.Zero));
            }
            else if (att == AosAttribute.BonusDex)
            {
                m.RemoveStatMod("MagicalEnhancementDex");
                m.AddStatMod(new StatMod(StatType.Dex, "MagicalEnhancementDex", value, TimeSpan.Zero));
            }
            else if (att == AosAttribute.BonusInt)
            {
                m.RemoveStatMod("MagicalEnhancementInt");
                m.AddStatMod(new StatMod(StatType.Int, "MagicalEnhancementInt", value, TimeSpan.Zero));
            }

            EnhancementAttributes match = null;

            for (int index = 0; index < _EnhancementList[m].Count; index++)
            {
                EnhancementAttributes attrs = _EnhancementList[m][index];

                if (attrs.Title == title)
                {
                    match = attrs;
                    break;
                }
            }

            if (match != null)
            {
                match.Attributes[att] = value;
            }
            else
            {
                match = new EnhancementAttributes(title);
                match.Attributes[att] = value;

                _EnhancementList[m].Add(match);
            }

            m.CheckStatTimers();
            m.UpdateResistances();
            m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
        }

        public static int GetValue(Mobile m, AosWeaponAttribute att)
        {
            if (m == null)
            {
                return 0;
            }

            if (_EnhancementList.TryGetValue(m, out List<EnhancementAttributes> enhancementValue))
            {
                int value = 0;

                for (int index = 0; index < enhancementValue.Count; index++)
                {
                    EnhancementAttributes attrs = enhancementValue[index];

                    value += attrs.WeaponAttributes[att];
                }

                return value;
            }

            return 0;
        }

        public static void SetValue(Mobile m, AosWeaponAttribute att, int value, string title)
        {
            if (!_EnhancementList.ContainsKey(m))
            {
                AddMobile(m);
            }

            EnhancementAttributes match = null;

            for (int index = 0; index < _EnhancementList[m].Count; index++)
            {
                EnhancementAttributes attrs = _EnhancementList[m][index];

                if (attrs.Title == title)
                {
                    match = attrs;
                    break;
                }
            }

            if (match != null)
            {
                match.WeaponAttributes[att] = value;
            }
            else
            {
                match = new EnhancementAttributes(title);
                match.WeaponAttributes[att] = value;

                _EnhancementList[m].Add(match);
            }

            m.CheckStatTimers();
            m.UpdateResistances();
            m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
        }

        public static int GetValue(Mobile m, SAAbsorptionAttribute att)
        {
            if (m == null)
            {
                return 0;
            }

            if (_EnhancementList.TryGetValue(m, out List<EnhancementAttributes> enhancementValue))
            {
                int value = 0;

                for (int index = 0; index < enhancementValue.Count; index++)
                {
                    EnhancementAttributes attrs = enhancementValue[index];

                    value += attrs.AbsorptionAttributes[att];
                }

                return value;
            }

            return 0;
        }

        public static int GetValue(Mobile m, ExtendedWeaponAttribute att)
        {
            if (m == null)
            {
                return 0;
            }

            if (_EnhancementList.TryGetValue(m, out List<EnhancementAttributes> enhancementValue))
            {
                int value = 0;

                for (int index = 0; index < enhancementValue.Count; index++)
                {
                    EnhancementAttributes attrs = enhancementValue[index];

                    value += attrs.ExtendedWeaponAttributes[att];
                }

                return value;
            }

            return 0;
        }

        public static void SetValue(Mobile m, ExtendedWeaponAttribute att, int value, string title)
        {
            if (!_EnhancementList.ContainsKey(m))
            {
                AddMobile(m);
            }

            EnhancementAttributes match = null;

            for (int index = 0; index < _EnhancementList[m].Count; index++)
            {
                EnhancementAttributes attrs = _EnhancementList[m][index];

                if (attrs.Title == title)
                {
                    match = attrs;
                    break;
                }
            }

            if (match != null)
            {
                match.ExtendedWeaponAttributes[att] = value;
            }
            else
            {
                match = new EnhancementAttributes(title);
                match.ExtendedWeaponAttributes[att] = value;

                _EnhancementList[m].Add(match);
            }

            m.CheckStatTimers();
            m.UpdateResistances();
            m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
        }
    }
}
