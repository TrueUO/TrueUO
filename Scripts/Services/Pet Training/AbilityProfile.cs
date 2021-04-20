using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    [PropertyObject]
    public class AbilityProfile
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public MagicalAbility MagicalAbility { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public AreaEffect[] AreaEffects { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public SpecialAbility[] SpecialAbilities { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAbility[] WeaponAbilities { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool TokunoTame { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenHits { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenStam { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RegenMana { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DamageIndex { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseCreature Creature { get; }

        public List<object> Advancements { get; private set; }

        public AbilityProfile(BaseCreature bc)
        {
            Creature = bc;
            DamageIndex = -1;
        }

        public void OnTame()
        {
            if (Creature.ControlMaster is PlayerMobile mobile)
            {
                Engines.Quests.TamingPetQuest.CheckTame(mobile);
            }

            if (Creature.Map == Map.Tokuno)
            {
                TokunoTame = true;
            }
        }

        public bool AddAbility(MagicalAbility ability, bool advancement = true)
        {
            if (Creature.Controlled)
            {
                MagicalAbility oldAbility = MagicalAbility;

                if (IsSpecialMagicalAbility(oldAbility))
                {
                    RemoveSpecialMagicalAbility(oldAbility);
                }

                OnRemoveMagicalAbility(oldAbility, ability);

                MagicalAbility = ability;
            }
            else
            {
                MagicalAbility |= ability;
            }

            OnAddAbility(ability, advancement);
            return true;
        }

        public bool AddAbility(SpecialAbility ability, bool advancement = true)
        {
            if (SpecialAbilities == null)
            {
                SpecialAbilities = new[] { ability };
            }
            else
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

                if (!any)
                {
                    SpecialAbility[] temp = SpecialAbilities;

                    SpecialAbilities = new SpecialAbility[temp.Length + 1];

                    for (int i = 0; i < temp.Length; i++)
                    {
                        SpecialAbilities[i] = temp[i];
                    }

                    SpecialAbilities[temp.Length] = ability;
                }
            }

            OnAddAbility(ability, advancement);

            return true;
        }

        public bool AddAbility(AreaEffect ability, bool advancement = true)
        {
            if (AreaEffects == null)
            {
                AreaEffects = new[] { ability };
            }
            else
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

                if (!any)
                {
                    AreaEffect[] temp = AreaEffects;

                    AreaEffects = new AreaEffect[temp.Length + 1];

                    for (int i = 0; i < temp.Length; i++)
                    {
                        AreaEffects[i] = temp[i];
                    }

                    AreaEffects[temp.Length] = ability;
                }
            }

            OnAddAbility(ability, advancement);

            return true;
        }

        public bool AddAbility(WeaponAbility ability, bool advancement = true)
        {
            if (WeaponAbilities == null)
            {
                WeaponAbilities = new[] { ability };
            }
            else
            {
                bool any = false;

                for (var index = 0; index < WeaponAbilities.Length; index++)
                {
                    var a = WeaponAbilities[index];

                    if (a == ability)
                    {
                        any = true;
                        break;
                    }
                }

                if (!any)
                {
                    WeaponAbility[] temp = WeaponAbilities;

                    WeaponAbilities = new WeaponAbility[temp.Length + 1];

                    for (int i = 0; i < temp.Length; i++)
                    {
                        WeaponAbilities[i] = temp[i];
                    }

                    WeaponAbilities[temp.Length] = ability;
                }
            }

            OnAddAbility(ability, advancement);

            return true;
        }

        public void RemoveAbility(MagicalAbility ability)
        {
            if ((MagicalAbility & ability) != 0)
            {
                MagicalAbility ^= ability;
                RemovePetAdvancement(ability);
            }
        }

        public void RemoveAbility(SpecialAbility ability)
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

            if (SpecialAbilities == null || !any)
            {
                return;
            }

            List<SpecialAbility> list = new List<SpecialAbility>();

            for (var index = 0; index < SpecialAbilities.Length; index++)
            {
                var specialAbility = SpecialAbilities[index];

                list.Add(specialAbility);
            }

            list.Remove(ability);
            RemovePetAdvancement(ability);

            SpecialAbilities = list.ToArray();

            ColUtility.Free(list);
        }

        public void RemoveAbility(WeaponAbility ability)
        {
            bool any = false;

            for (var index = 0; index < WeaponAbilities.Length; index++)
            {
                var a = WeaponAbilities[index];

                if (a == ability)
                {
                    any = true;
                    break;
                }
            }

            if (WeaponAbilities == null || !any)
            {
                return;
            }

            List<WeaponAbility> list = new List<WeaponAbility>();

            for (var index = 0; index < WeaponAbilities.Length; index++)
            {
                var weaponAbility = WeaponAbilities[index];

                list.Add(weaponAbility);
            }

            list.Remove(ability);
            RemovePetAdvancement(ability);

            WeaponAbilities = list.ToArray();

            ColUtility.Free(list);
        }

        public void RemoveAbility(AreaEffect ability)
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

            if (AreaEffects == null || !any)
            {
                return;
            }

            List<AreaEffect> list = new List<AreaEffect>();

            for (var index = 0; index < AreaEffects.Length; index++)
            {
                var effect = AreaEffects[index];

                list.Add(effect);
            }

            list.Remove(ability);
            RemovePetAdvancement(ability);

            AreaEffects = list.ToArray();

            ColUtility.Free(list);
        }

        public bool AddAbility(SkillName skill, bool advancement = true)
        {
            OnAddAbility(skill, advancement);
            return true;
        }

        public bool CanAddAbility(object o)
        {
            if (!Creature.Controlled)
                return true;

            if (o is MagicalAbility)
                return true;

            if (o is SpecialAbility && (SpecialAbilities == null || SpecialAbilities.Length == 0))
                return true;

            if (o is AreaEffect && (AreaEffects == null || AreaEffects.Length == 0))
                return true;

            if (o is WeaponAbility && (WeaponAbilities == null || WeaponAbilities.Length < 2))
                return true;

            return false;
        }

        public void OnAddAbility(object newAbility, bool advancement)
        {
            if (advancement)
            {
                AddPetAdvancement(newAbility);
            }

            if (newAbility is MagicalAbility ability)
            {
                AddMagicalAbility(ability);
            }

            TrainingPoint trainPoint = PetTrainingHelper.GetTrainingPoint(newAbility);

            if (trainPoint != null && trainPoint.Requirements != null)
            {
                for (var index = 0; index < trainPoint.Requirements.Length; index++)
                {
                    TrainingPointRequirement req = trainPoint.Requirements[index];

                    if (req != null)
                    {
                        if (req.Requirement is SkillName name)
                        {
                            double skill = Creature.Skills[name].Base;
                            double toAdd = req.Cost == 100 ? 20 : 40;

                            if (name == SkillName.Hiding)
                            {
                                toAdd = 100;
                            }

                            if (skill < toAdd)
                            {
                                Creature.Skills[name].Base = toAdd;
                            }
                        }
                        else if (req.Requirement is WeaponAbility requirement)
                        {
                            AddAbility(requirement);
                        }
                    }
                }
            }
        }

        public void AddPetAdvancement(object o)
        {
            if (Creature.Controlled)
            {
                if (Advancements == null)
                    Advancements = new List<object>();

                if (!Advancements.Contains(o))
                {
                    Advancements.Add(o);
                }
            }
        }

        public void RemovePetAdvancement(object o)
        {
            if (Creature.Controlled && Advancements != null && Advancements.Contains(o))
            {
                Advancements.Remove(o);
            }
        }

        public bool HasAbility(object o)
        {
            if (o is MagicalAbility ability)
            {
                return (MagicalAbility & ability) != 0;
            }

            if (o is SpecialAbility specialAbility && SpecialAbilities != null)
            {
                for (var index = 0; index < SpecialAbilities.Length; index++)
                {
                    var a = SpecialAbilities[index];

                    if (a == specialAbility)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (o is AreaEffect effect && AreaEffects != null)
            {
                for (var index = 0; index < AreaEffects.Length; index++)
                {
                    var a = AreaEffects[index];

                    if (a == effect)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (o is WeaponAbility weaponAbility && WeaponAbilities != null)
            {
                for (var index = 0; index < WeaponAbilities.Length; index++)
                {
                    var a = WeaponAbilities[index];

                    if (a == weaponAbility)
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        public int AbilityCount()
        {
            int count = 0;

            if (MagicalAbility != MagicalAbility.None)
            {
                count++;
            }

            if (SpecialAbilities != null)
            {
                int sac = 0;

                for (var index = 0; index < SpecialAbilities.Length; index++)
                {
                    var a = SpecialAbilities[index];

                    if (!a.NaturalAbility)
                    {
                        sac++;
                    }
                }

                count += sac;
            }

            if (AreaEffects != null)
            {
                count += AreaEffects.Length;
            }

            if (WeaponAbilities != null)
            {
                count += WeaponAbilities.Length;
            }

            return count;
        }

        public bool CanChooseMagicalAbility(MagicalAbility ability)
        {
            if (!Creature.Controlled)
            {
                return true;
            }

            bool any = false;

            for (var index = 0; index < SpecialAbilities.Length; index++)
            {
                var a = SpecialAbilities[index];

                if (!a.NaturalAbility)
                {
                    any = true;
                    break;
                }
            }

            if (HasSpecialMagicalAbility() && IsSpecialMagicalAbility(ability) && SpecialAbilities != null && SpecialAbilities.Length > 0 && any)
            {
                return false;
            }

            return true;
        }

        public bool CanChooseSpecialAbility(SpecialAbility[] list)
        {
            if (!Creature.Controlled)
            {
                return true;
            }

            bool all = true;

            for (var index = 0; index < SpecialAbilities.Length; index++)
            {
                var a = SpecialAbilities[index];

                if (!a.NaturalAbility)
                {
                    all = false;
                    break;
                }
            }

            bool any = false;

            for (var index = 0; index < list.Length; index++)
            {
                var abil = list[index];

                if (IsRuleBreaker(abil))
                {
                    any = true;
                    break;
                }
            }

            if (HasSpecialMagicalAbility() && any && (AreaEffects == null || AreaEffects.Length == 0) && (SpecialAbilities == null || SpecialAbilities.Length == 0 || all))
            {
                return true;
            }

            int count = 0;

            for (var index = 0; index < SpecialAbilities.Length; index++)
            {
                var a = SpecialAbilities[index];

                if (!a.NaturalAbility)
                {
                    count++;
                }
            }

            return !HasSpecialMagicalAbility() && (SpecialAbilities == null || count == 0) && AbilityCount() < 3;
        }

        public static bool IsRuleBreaker(SpecialAbility ability)
        {
            for (var index = 0; index < PetTrainingHelper.RuleBreakers.Length; index++)
            {
                var abil = PetTrainingHelper.RuleBreakers[index];

                if (abil == ability)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanChooseAreaEffect()
        {
            if (!Creature.Controlled)
                return true;

            if (HasSpecialMagicalAbility() && (AreaEffects == null || AreaEffects.Length == 0) && (SpecialAbilities == null || SpecialAbilities.Length == 0))
                return true;

            return !HasSpecialMagicalAbility() && (AreaEffects == null || AreaEffects.Length == 0) && AbilityCount() < 3;
        }

        public bool CanChooseWeaponAbility()
        {
            if (!Creature.Controlled)
                return true;

            return !HasSpecialMagicalAbility() && (WeaponAbilities == null || WeaponAbilities.Length < 2) && AbilityCount() < 3;
        }

        public bool HasSpecialMagicalAbility()
        {
            return (MagicalAbility & MagicalAbility.Piercing) != 0 ||
                (MagicalAbility & MagicalAbility.Bashing) != 0 ||
                (MagicalAbility & MagicalAbility.Slashing) != 0 ||
                (MagicalAbility & MagicalAbility.BattleDefense) != 0 ||
                (MagicalAbility & MagicalAbility.WrestlingMastery) != 0;
        }

        public static bool IsSpecialMagicalAbility(MagicalAbility ability)
        {
            return ability != MagicalAbility.None && ability <= MagicalAbility.WrestlingMastery;
        }

        public void AddMagicalAbility(MagicalAbility ability)
        {
            if (IsSpecialMagicalAbility(ability))
            {
                //SpecialAbilities = null;
                WeaponAbilities = null;
                Creature.AI = AIType.AI_Melee;
            }

            switch (ability)
            {
                case MagicalAbility.Piercing:
                    Creature.Mastery = SkillName.Fencing;
                    break;
                case MagicalAbility.Bashing:
                    Creature.Mastery = SkillName.Macing;
                    break;
                case MagicalAbility.Slashing:
                    Creature.Mastery = SkillName.Swords;
                    break;
                case MagicalAbility.BattleDefense:
                    Creature.Mastery = SkillName.Parry;
                    break;
                case MagicalAbility.WrestlingMastery:
                    Creature.Mastery = SkillName.Wrestling;
                    break;
                case MagicalAbility.Poisoning:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Melee)
                        Creature.AI = AIType.AI_Melee;
                    break;
                case MagicalAbility.Bushido:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Samurai)
                        Creature.AI = AIType.AI_Samurai;
                    if (!HasAbility(WeaponAbility.WhirlwindAttack))
                    {
                        AddAbility(WeaponAbility.WhirlwindAttack, false);
                    }
                    break;
                case MagicalAbility.Ninjitsu:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Ninja)
                        Creature.AI = AIType.AI_Ninja;
                    if (!HasAbility(WeaponAbility.FrenziedWhirlwind))
                    {
                        AddAbility(WeaponAbility.FrenziedWhirlwind, false);
                    }
                    break;
                case MagicalAbility.Discordance:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Melee)
                        Creature.AI = AIType.AI_Melee;
                    break;
                case MagicalAbility.Magery:
                case MagicalAbility.MageryMastery:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Mage)
                        Creature.AI = AIType.AI_Mage;
                    break;
                case MagicalAbility.Mysticism:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Mystic)
                        Creature.AI = AIType.AI_Mystic;
                    break;
                case MagicalAbility.Spellweaving:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Spellweaving)
                        Creature.AI = AIType.AI_Spellweaving;
                    break;
                case MagicalAbility.Chivalry:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Paladin)
                        Creature.AI = AIType.AI_Paladin;
                    break;
                case MagicalAbility.Necromage:
                    if (Creature.Controlled && Creature.AI != AIType.AI_NecroMage)
                        Creature.AI = AIType.AI_NecroMage;
                    break;
                case MagicalAbility.Necromancy:
                    if (Creature.Controlled && Creature.AI != AIType.AI_Necro)
                        Creature.AI = AIType.AI_Necro;
                    break;
            }
        }

        public void OnRemoveMagicalAbility(MagicalAbility oldAbility, MagicalAbility newAbility)
        {
            if ((oldAbility & MagicalAbility.Bushido) != 0)
            {
                if (HasAbility(WeaponAbility.WhirlwindAttack))
                {
                    RemoveAbility(WeaponAbility.WhirlwindAttack);
                }
            }

            if ((oldAbility & MagicalAbility.Ninjitsu) != 0)
            {
                if (HasAbility(WeaponAbility.FrenziedWhirlwind))
                {
                    RemoveAbility(WeaponAbility.FrenziedWhirlwind);
                }
            }
        }

        public void RemoveSpecialMagicalAbility(MagicalAbility ability)
        {
            //SpecialAbilities = null;
            WeaponAbilities = null;

            Creature.Mastery = SkillName.Alchemy; // default
        }

        public bool HasCustomized()
        {
            return Advancements != null && Advancements.Count > 0;
        }

        public bool IsNaturalAbility(object o)
        {
            if (Advancements == null)
                return true;

            if (o is SpecialAbility specialAbility)
            {
                bool any = false;

                for (var index = 0; index < Advancements.Count; index++)
                {
                    var s = Advancements[index];

                    if (s is SpecialAbility ability && ability == specialAbility)
                    {
                        any = true;
                        break;
                    }
                }

                return SpecialAbilities != null && !any;
            }

            if (o is WeaponAbility weaponAbility)
            {
                bool any = false;

                for (var index = 0; index < Advancements.Count; index++)
                {
                    var s = Advancements[index];

                    if (s is WeaponAbility ability && ability == weaponAbility)
                    {
                        any = true;
                        break;
                    }
                }

                return WeaponAbilities != null && !any;
            }

            return false;
        }

        public IEnumerable<object> EnumerateAllAbilities()
        {
            if (MagicalAbility != MagicalAbility.None)
            {
                for (var index = 0; index < PetTrainingHelper.MagicalAbilities.Length; index++)
                {
                    MagicalAbility abil = PetTrainingHelper.MagicalAbilities[index];

                    if ((MagicalAbility & abil) != 0)
                    {
                        yield return abil;
                    }
                }
            }

            if (SpecialAbilities != null)
            {
                for (var index = 0; index < SpecialAbilities.Length; index++)
                {
                    SpecialAbility abil = SpecialAbilities[index];

                    yield return abil;
                }
            }

            if (AreaEffects != null)
            {
                for (var index = 0; index < AreaEffects.Length; index++)
                {
                    AreaEffect effect = AreaEffects[index];

                    yield return effect;
                }
            }

            if (WeaponAbilities != null)
            {
                for (var index = 0; index < WeaponAbilities.Length; index++)
                {
                    WeaponAbility abil = WeaponAbilities[index];

                    yield return abil;
                }
            }
        }

        public IEnumerable<SpecialAbility> EnumerateSpecialAbilities()
        {
            if (SpecialAbilities == null)
            {
                yield break;
            }

            for (var index = 0; index < SpecialAbilities.Length; index++)
            {
                SpecialAbility ability = SpecialAbilities[index];

                yield return ability;
            }
        }

        public SpecialAbility[] GetSpecialAbilities()
        {
            List<SpecialAbility> list = new List<SpecialAbility>();

            foreach (var ability in EnumerateSpecialAbilities())
            {
                list.Add(ability);
            }

            return list.ToArray();
        }

        public IEnumerable<AreaEffect> EnumerateAreaEffects()
        {
            if (AreaEffects == null)
            {
                yield break;
            }

            for (var index = 0; index < AreaEffects.Length; index++)
            {
                AreaEffect ability = AreaEffects[index];

                yield return ability;
            }
        }

        public AreaEffect[] GetAreaEffects()
        {
            List<AreaEffect> list = new List<AreaEffect>();

            foreach (var effect in EnumerateAreaEffects())
            {
                list.Add(effect);
            }

            return list.ToArray();
        }

        public override string ToString()
        {
            return "...";
        }

        public AbilityProfile(BaseCreature bc, GenericReader reader)
        {
            int version = reader.ReadInt();

            Creature = bc;

            switch (version)
            {
                case 0:
                    DamageIndex = -1;
                    break;
                case 1:
                    DamageIndex = reader.ReadInt();
                    break;
            }

            MagicalAbility = (MagicalAbility)reader.ReadInt();
            TokunoTame = reader.ReadBool();

            RegenHits = reader.ReadInt();
            RegenStam = reader.ReadInt();
            RegenMana = reader.ReadInt();

            int count = reader.ReadInt();
            SpecialAbilities = new SpecialAbility[count];

            for (int i = 0; i < count; i++)
            {
                SpecialAbilities[i] = SpecialAbility.Abilities[reader.ReadInt()];
            }

            count = reader.ReadInt();
            AreaEffects = new AreaEffect[count];

            for (int i = 0; i < count; i++)
            {
                AreaEffects[i] = AreaEffect.Effects[reader.ReadInt()];
            }

            count = reader.ReadInt();
            WeaponAbilities = new WeaponAbility[count];

            for (int i = 0; i < count; i++)
            {
                WeaponAbilities[i] = WeaponAbility.Abilities[reader.ReadInt()];
            }

            count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                if (Advancements == null)
                    Advancements = new List<object>();

                switch (reader.ReadInt())
                {
                    case 1: Advancements.Add((MagicalAbility)reader.ReadInt()); break;
                    case 2: Advancements.Add(SpecialAbility.Abilities[reader.ReadInt()]); break;
                    case 3: Advancements.Add(AreaEffect.Effects[reader.ReadInt()]); break;
                    case 4: Advancements.Add(WeaponAbility.Abilities[reader.ReadInt()]); break;
                    case 5: Advancements.Add((SkillName)reader.ReadInt()); break;
                }
            }
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.Write(1);

            writer.Write(DamageIndex);

            writer.Write((int)MagicalAbility);
            writer.Write(TokunoTame);

            writer.Write(RegenHits);
            writer.Write(RegenStam);
            writer.Write(RegenMana);

            writer.Write(SpecialAbilities != null ? SpecialAbilities.Length : 0);

            if (SpecialAbilities != null)
            {
                for (var index = 0; index < SpecialAbilities.Length; index++)
                {
                    SpecialAbility abil = SpecialAbilities[index];

                    writer.Write(Array.IndexOf(SpecialAbility.Abilities, abil));
                }
            }

            writer.Write(AreaEffects != null ? AreaEffects.Length : 0);

            if (AreaEffects != null)
            {
                for (var index = 0; index < AreaEffects.Length; index++)
                {
                    AreaEffect abil = AreaEffects[index];

                    writer.Write(Array.IndexOf(AreaEffect.Effects, abil));
                }
            }

            writer.Write(WeaponAbilities != null ? WeaponAbilities.Length : 0);

            if (WeaponAbilities != null)
            {
                for (var index = 0; index < WeaponAbilities.Length; index++)
                {
                    WeaponAbility abil = WeaponAbilities[index];

                    writer.Write(Array.IndexOf(WeaponAbility.Abilities, abil));
                }
            }

            writer.Write(Advancements != null ? Advancements.Count : 0);

            if (Advancements != null)
            {
                for (var index = 0; index < Advancements.Count; index++)
                {
                    object o = Advancements[index];

                    if (o is MagicalAbility ability)
                    {
                        writer.Write(1);
                        writer.Write((int) ability);
                    }
                    else if (o is SpecialAbility specialAbility)
                    {
                        writer.Write(2);
                        writer.Write(Array.IndexOf(SpecialAbility.Abilities, specialAbility));
                    }
                    else if (o is AreaEffect effect)
                    {
                        writer.Write(3);
                        writer.Write(Array.IndexOf(AreaEffect.Effects, effect));
                    }
                    else if (o is WeaponAbility weaponAbility)
                    {
                        writer.Write(4);
                        writer.Write(Array.IndexOf(WeaponAbility.Abilities, weaponAbility));
                    }
                    else if (o is SkillName skillName)
                    {
                        writer.Write(5);
                        writer.Write((int) skillName);
                    }
                    else
                    {
                        writer.Write(0);
                    }
                }
            }
        }
    }
}
