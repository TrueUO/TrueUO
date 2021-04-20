using Server.Items;
using System;

namespace Server.Mobiles
{
    public class TrainingDefinition
    {
        public Type CreatureType { get; }
        public Class Class { get; }
        public MagicalAbility MagicalAbilities { get; }
        public SpecialAbility[] SpecialAbilities { get; }
        public WeaponAbility[] WeaponAbilities { get; }
        public AreaEffect[] AreaEffects { get; }

        public int ControlSlotsMin { get; }
        public int ControlSlotsMax { get; }

        public TrainingDefinition(
            Type type,
            Class classificaion,
            MagicalAbility magicalAbility,
            SpecialAbility[] specialAbility,
            WeaponAbility[] weaponAbility,
            AreaEffect[] areaEffect,
            int controlmin,
            int controlmax)
        {
            CreatureType = type;
            Class = classificaion;
            MagicalAbilities = magicalAbility;
            SpecialAbilities = specialAbility;
            WeaponAbilities = weaponAbility;
            AreaEffects = areaEffect;

            ControlSlotsMin = controlmin;
            ControlSlotsMax = controlmax;
        }

        public bool HasSpecialAbility(SpecialAbility ability)
        {
            bool any = false;

            for (var index = 0; index < SpecialAbilities.Length; index++)
            {
                var a = SpecialAbilities[index];

                if (a == ability)
                {
                    any = true;
                    break;
                }
            }

            return SpecialAbilities != null && any;
        }

        public bool HasAreaEffect(AreaEffect ability)
        {
            bool any = false;

            for (var index = 0; index < AreaEffects.Length; index++)
            {
                var a = AreaEffects[index];

                if (a == ability)
                {
                    any = true;
                    break;
                }
            }

            return AreaEffects != null && any;
        }
    }
}
