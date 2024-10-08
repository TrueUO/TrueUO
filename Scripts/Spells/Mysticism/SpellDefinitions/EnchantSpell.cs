using Server.Gumps;
using Server.Items;
using Server.Spells.Spellweaving;
using System;
using System.Collections.Generic;

namespace Server.Spells.Mysticism
{
    public class EnchantSpell : MysticSpell
    {
        private const string _ModName = "EnchantAttribute";

        public override SpellCircle Circle => SpellCircle.Second;
        public override bool ClearHandsOnCast => false;

        public BaseWeapon Weapon { get; }
        public AosWeaponAttribute Attribute { get; }

        private static readonly SpellInfo m_Info = new(
                "Enchant", "In Ort Ylem",
                230,
                9022,
                Reagent.SpidersSilk,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public EnchantSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        public EnchantSpell(Mobile caster, Item scroll, BaseWeapon weapon, AosWeaponAttribute attribute) : base(caster, scroll, m_Info)
        {
            Weapon = weapon;
            Attribute = attribute;
        }

        public override bool CheckCast()
        {
            if (Weapon == null)
            {
                BaseWeapon wep = Caster.Weapon as BaseWeapon;

                if (wep == null)
                {
                    Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
                }
                else
                {
                    if (Caster.HasGump(typeof(EnchantSpellGump)))
                    {
                        Caster.CloseGump(typeof(EnchantSpellGump));
                    }

                    EnchantSpellGump gump = new EnchantSpellGump(Caster, Scroll, wep);
                    int serial = gump.Serial;

                    Caster.SendGump(gump);

                    Timer.DelayCall(TimeSpan.FromSeconds(30), () =>
                    {
                        Gump current = Caster.FindGump(typeof(EnchantSpellGump));

                        if (current != null && current.Serial == serial)
                        {
                            Caster.CloseGump(typeof(EnchantSpellGump));
                            FinishSequence();
                        }
                    });
                }

                return false;
            }

            if (IsUnderSpellEffects(Caster, Weapon))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
                return false;
            }

            if (ImmolatingWeaponSpell.IsImmolating(Caster, Weapon) || Weapon.ConsecratedContext != null)
            {
                Caster.SendLocalizedMessage(1080128); //You cannot use this ability while your weapon is enchanted.
                return false;
            }

            if (Weapon.FocusWeilder != null)
            {
                Caster.SendLocalizedMessage(1080446); // You cannot enchant an item that is under the effects of the ninjitsu focus attack ability.
                return false;
            }

            if (Weapon.WeaponAttributes.HitLightning > 0 || Weapon.WeaponAttributes.HitFireball > 0 || Weapon.WeaponAttributes.HitHarm > 0 || Weapon.WeaponAttributes.HitMagicArrow > 0 || Weapon.WeaponAttributes.HitDispel > 0)
            {
                Caster.SendLocalizedMessage(1080127); // This weapon already has a hit spell effect and cannot be enchanted.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            BaseWeapon wep = Caster.Weapon as BaseWeapon;

            if (wep == null || wep != Weapon)
            {
                Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
            }
            else if (IsUnderSpellEffects(Caster, Weapon))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
            }
            else if (ImmolatingWeaponSpell.IsImmolating(Caster, Weapon) || Weapon.ConsecratedContext != null)
            {
                Caster.SendLocalizedMessage(1080128); //You cannot use this ability while your weapon is enchanted.
            }
            else if (Weapon.FocusWeilder != null)
            {
                Caster.SendLocalizedMessage(1080446); // You cannot enchant an item that is under the effects of the ninjitsu focus attack ability.
            }
            else if (Weapon.WeaponAttributes.HitLightning > 0 || Weapon.WeaponAttributes.HitFireball > 0 || Weapon.WeaponAttributes.HitHarm > 0 || Weapon.WeaponAttributes.HitMagicArrow > 0 || Weapon.WeaponAttributes.HitDispel > 0)
            {
                Caster.SendLocalizedMessage(1080127); // This weapon already has a hit spell effect and cannot be enchanted.
            }
            else if (CheckSequence() && Caster.Weapon == Weapon)
            {
                Caster.PlaySound(0x64E);
                Caster.FixedEffect(0x36CB, 1, 9, 1915, 0);

                int prim = (int)Caster.Skills[CastSkill].Value;
                int sec = (int)Caster.Skills[DamageSkill].Value;

                int value = (60 * (prim + sec)) / 240;
                double duration = ((prim + sec) / 2.0) + 30.0;
                int malus = 0;

                if (Table == null)
                    Table = new Dictionary<Mobile, EnchantmentTimer>();

                Enhancement.SetValue(Caster, Attribute, value, _ModName);

                if (prim >= 80 && sec >= 80 && Weapon.Attributes.SpellChanneling == 0)
                {
                    Enhancement.SetValue(Caster, AosAttribute.SpellChanneling, 1, _ModName);
                    Enhancement.SetValue(Caster, AosAttribute.CastSpeed, -1, _ModName);
                    malus = 1;
                }

                Table[Caster] = new EnchantmentTimer(Caster, Weapon, Attribute, value, malus, duration);

                int loc;

                switch (Attribute)
                {
                    default:
                    case AosWeaponAttribute.HitLightning: loc = 1060423; break;
                    case AosWeaponAttribute.HitFireball: loc = 1060420; break;
                    case AosWeaponAttribute.HitHarm: loc = 1060421; break;
                    case AosWeaponAttribute.HitMagicArrow: loc = 1060426; break;
                    case AosWeaponAttribute.HitDispel: loc = 1060417; break;
                }

                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.Enchant, 1080126, loc, TimeSpan.FromSeconds(duration), Caster, value.ToString()));

                Weapon.EnchantedWeilder = Caster;
                Weapon.InvalidateProperties();
            }

            FinishSequence();
        }

        public static Dictionary<Mobile, EnchantmentTimer> Table { get; set; }

        public static bool IsUnderSpellEffects(Mobile mobile, BaseWeapon wep)
        {
            if (Table == null)
            {
                return false;
            }

            return Table.ContainsKey(mobile) && Table[mobile].Weapon == wep;
        }

        public static AosWeaponAttribute BonusAttribute(Mobile from)
        {
            if (Table.TryGetValue(from, out EnchantmentTimer value))
            {
                return value.WeaponAttribute;
            }

            return AosWeaponAttribute.HitColdArea;
        }

        public static int BonusValue(Mobile from)
        {
            if (Table.TryGetValue(from, out EnchantmentTimer value))
            {
                return value.AttributeValue;
            }

            return 0;
        }

        public static bool CastingMalus(Mobile from, BaseWeapon weapon)
        {
            if (Table.TryGetValue(from, out EnchantmentTimer value))
            {
                return value.CastingMalus > 0;
            }

            return false;
        }

        public static void RemoveEnchantment(Mobile caster)
        {
            if (Table != null && Table.TryGetValue(caster, out EnchantmentTimer value))
            {
                BaseWeapon weapon = value.Weapon;

                value.Stop();
                Table[caster] = null;
                Table.Remove(caster);

                caster.SendLocalizedMessage(1115273); // The enchantment on your weapon has expired.
                caster.PlaySound(0x1E6);

                Enhancement.RemoveMobile(caster);

                weapon?.InvalidateProperties();

                BuffInfo.RemoveBuff(caster, BuffIcon.Enchant);
            }
        }

        public static void OnWeaponRemoved(BaseWeapon wep, Mobile from)
        {
            if (IsUnderSpellEffects(from, wep))
                RemoveEnchantment(from);

            if (wep.EnchantedWeilder != null)
                wep.EnchantedWeilder = null;
        }
    }

    public class EnchantmentTimer : Timer
    {
        public Mobile Owner { get; }
        public BaseWeapon Weapon { get; }
        public AosWeaponAttribute WeaponAttribute { get; }
        public int AttributeValue { get; }
        public int CastingMalus { get; }

        public EnchantmentTimer(Mobile owner, BaseWeapon wep, AosWeaponAttribute attribute, int value, int malus, double duration) : base(TimeSpan.FromSeconds(duration))
        {
            Owner = owner;
            Weapon = wep;
            WeaponAttribute = attribute;
            AttributeValue = value;
            CastingMalus = malus;

            Start();
        }

        protected override void OnTick()
        {
            if (Weapon != null)
                Weapon.EnchantedWeilder = null;

            EnchantSpell.RemoveEnchantment(Owner);
        }
    }
}
