using Server.Mobiles;
using Server.Network;
using System;

namespace Server
{
    public class BuffInfo
    {
        public static int Blank => 1114057;  // ~1_val~

        #region Properties
        private readonly BuffIcon m_ID;
        public BuffIcon ID => m_ID;

        private readonly int m_TitleCliloc;
        public int TitleCliloc => m_TitleCliloc;

        private readonly int m_SecondaryCliloc;
        public int SecondaryCliloc => m_SecondaryCliloc;

        private readonly int m_ThirdCliloc;
        public int ThirdCliloc => m_ThirdCliloc;

        private readonly bool m_NoTimer;
        public bool NoTimer => m_NoTimer;

        private readonly TimeSpan m_TimeLength;
        public TimeSpan TimeLength => m_TimeLength;

        private readonly DateTime m_TimeStart;
        public DateTime TimeStart => m_TimeStart;

        private readonly Timer m_Timer;
        public Timer Timer => m_Timer;

        private readonly bool m_RetainThroughDeath;
        public bool RetainThroughDeath => m_RetainThroughDeath;

        private readonly TextDefinition m_TitleArguments;
        public TextDefinition TitleArguments => m_TitleArguments;

        private readonly TextDefinition m_SecondaryArguments;
        public TextDefinition SecondaryArguments => m_SecondaryArguments;

        private readonly TextDefinition m_ThirdArguments;
        public TextDefinition ThirdArguments => m_ThirdArguments;

        #endregion

        #region Constructors
        public BuffInfo(BuffIcon iconID, int titleCliloc)
            : this(iconID, titleCliloc, titleCliloc + 1)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc)
            : this(iconID, titleCliloc, secondaryCliloc, 0)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, int thirdCliloc)
        {
            m_ID = iconID;
            m_TitleCliloc = titleCliloc;
            m_SecondaryCliloc = secondaryCliloc;
            m_ThirdCliloc = thirdCliloc;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TimeSpan length, Mobile m)
            : this(iconID, titleCliloc, titleCliloc + 1, length, m)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, TimeSpan length, Mobile m)
            : this(iconID, titleCliloc, secondaryCliloc, 0, length, m)
        {
        }

        //Only the timed one needs to Mobile to know when to automagically remove it.
        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, int thirdCliloc, TimeSpan length, Mobile m)
            : this(iconID, titleCliloc, secondaryCliloc, thirdCliloc)
        {
            m_TimeLength = length;
            m_TimeStart = DateTime.UtcNow;

            m_Timer = Timer.DelayCall(length, delegate
            {
                PlayerMobile pm = m as PlayerMobile;

                if (pm == null)
                    return;

                pm.RemoveBuff(this);
            });
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, TimeSpan length, Mobile m, bool notimer)
            : this(iconID, titleCliloc, secondaryCliloc, length, m)
        {
            m_NoTimer = notimer;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TextDefinition secargs)
            : this(iconID, titleCliloc, titleCliloc + 1, secargs)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, TextDefinition secargs)
            : this(iconID, titleCliloc, secondaryCliloc)
        {
            m_SecondaryArguments = secargs;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, bool retainThroughDeath)
            : this(iconID, titleCliloc, titleCliloc + 1, retainThroughDeath)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, bool retainThroughDeath)
            : this(iconID, titleCliloc, secondaryCliloc)
        {
            m_RetainThroughDeath = retainThroughDeath;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TextDefinition secargs, bool retainThroughDeath)
            : this(iconID, titleCliloc, titleCliloc + 1, secargs, retainThroughDeath)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, TextDefinition secargs, bool retainThroughDeath)
            : this(iconID, titleCliloc, secondaryCliloc, secargs)
        {
            m_RetainThroughDeath = retainThroughDeath;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TimeSpan length, Mobile m, TextDefinition secargs)
            : this(iconID, titleCliloc, titleCliloc + 1, length, m, secargs)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, TimeSpan length, Mobile m, TextDefinition secargs)
            : this(iconID, titleCliloc, secondaryCliloc, length, m)
        {
            m_SecondaryArguments = secargs;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TextDefinition titleargs, int secondaryCliloc, TextDefinition secargs, TimeSpan length, Mobile m)
            : this(iconID, titleCliloc, secondaryCliloc, length, m)
        {
            m_TitleArguments = titleargs;
            m_SecondaryArguments = secargs;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TextDefinition titleargs, int secondaryCliloc, TextDefinition secargs, int thirdCliloc, TextDefinition thirdargs, TimeSpan length, Mobile m)
            : this(iconID, titleCliloc, secondaryCliloc, thirdCliloc, length, m)
        {
            m_TitleArguments = titleargs;
            m_SecondaryArguments = secargs;
            m_ThirdArguments = thirdargs;
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, TimeSpan length, Mobile m, TextDefinition secargs, bool retainThroughDeath)
            : this(iconID, titleCliloc, titleCliloc + 1, length, m, secargs, retainThroughDeath)
        {
        }

        public BuffInfo(BuffIcon iconID, int titleCliloc, int secondaryCliloc, TimeSpan length, Mobile m, TextDefinition secargs, bool retainThroughDeath)
            : this(iconID, titleCliloc, secondaryCliloc, length, m)
        {
            m_SecondaryArguments = secargs;
            m_RetainThroughDeath = retainThroughDeath;
        }

        #endregion

        #region Convenience Methods
        public static void AddBuff(Mobile m, BuffInfo b)
        {
            if (m is PlayerMobile pm)
            {
                pm.AddBuff(b);
            }
        }

        public static void RemoveBuff(Mobile m, BuffInfo b)
        {
            if (m is PlayerMobile pm)
            {
                pm.RemoveBuff(b);
            }
        }

        public static void RemoveBuff(Mobile m, BuffIcon b)
        {
            if (m is PlayerMobile pm)
            {
                pm.RemoveBuff(b);
            }
        }
        #endregion
    }

    public enum BuffIcon : short
    {
        DismountPrevention = 0x3E9,
        NoRearm = 0x3EA,
        //Currently, no 0x3EB or 0x3EC
        NightSight = 0x3ED,	//*
        DeathStrike,
        EvilOmen,
        HonoredDebuff,
        AchievePerfection,
        DivineFury,			//*
        EnemyOfOne,			//*
        HidingAndOrStealth,	//*
        ActiveMeditation,	//*
        BloodOathCaster,	//*
        BloodOathCurse,		//*
        CorpseSkin,			//*
        Mindrot,			//*
        PainSpike,			//*
        Strangle,
        GiftOfRenewal,		//*
        AttuneWeapon,		//*
        Thunderstorm,		//*
        EssenceOfWind,		//*
        EtherealVoyage,		//*
        GiftOfLife,			//*
        ArcaneEmpowerment,	//*
        MortalStrike,
        ReactiveArmor,		//*
        Protection,			//*
        ArchProtection,
        MagicReflection,	//*
        Incognito,			//*
        Disguised,
        AnimalForm,
        Polymorph,
        Invisibility,		//*
        Paralyze,			//*
        Poison,
        Bleed,
        Clumsy,				//*
        FeebleMind,			//*
        Weaken,				//*
        Curse,				//*
        MassCurse,
        Agility,			//*
        Cunning,			//*
        Strength,			//*
        Bless,				//*
        Sleep,
        StoneForm,
        SpellPlague,
        Berserk,
        MassSleep,
        Fly,
        Inspire,
        Invigorate,
        Resilience,
        Perseverance,
        TribulationTarget,
        DespairTarget,
        FishPie = 0x426,
        HitLowerAttack,
        HitLowerDefense,
        DualWield,
        Block,
        DefenseMastery,
        DespairCaster,
        Healing,
        SpellFocusingBuff,
        SpellFocusingDebuff,
        RageFocusingDebuff,
        RageFocusingBuff,
        Warding,
        TribulationCaster,
        ForceArrow,
        Disarm,
        Surge,
        Feint,
        TalonStrike,
        PsychicAttack,
        ConsecrateWeapon,
        GrapesOfWrath,
        EnemyOfOneDebuff,
        HorrificBeast,
        LichForm,
        VampiricEmbrace,
        CurseWeapon,
        ReaperForm,
        ImmolatingWeapon,
        Enchant,
        HonorableExecution,
        Confidence,
        Evasion,
        CounterAttack,
        LightningStrike,
        MomentumStrike,
        OrangePetals,
        RoseOfTrinsic,
        PoisonImmunity,
        Veterinary,
        Perfection,
        Honored,
        ManaPhase,
        FanDancerFanFire,
        Rage,
        Webbing,
        MedusaStone,
        TrueFear,
        AuraOfNausea,
        HowlOfCacophony,
        GazeDespair,
        HiryuPhysicalResistance,
        RuneBeetleCorruption,
        BloodwormAnemia,
        RotwormBloodDisease,
        SkillUseDelay,
        FactionStatLoss,
        HeatOfBattleStatus,
        CriminalStatus,
        ArmorPierce,
        SplinteringEffect,
        SwingSpeedDebuff,
        WraithForm,
        CityTradeDeal = 0x466,
        HumilityDebuff = 0x467,
        Spirituality,
        Humility,
        // Skill Masteries
        Rampage,
        Stagger, // Debuff
        Toughness,
        Thrust,
        Pierce,   // Debuff
        PlayingTheOdds,
        FocusedEye,
        Onslaught, // Debuff
        ElementalFury,
        ElementalFuryDebuff, // Debuff
        CalledShot,
        Knockout,
        SavingThrow,
        Conduit,
        EtherealBurst,
        MysticWeapon,
        ManaShield,
        AnticipateHit,
        Warcry,
        Shadow,
        WhiteTigerForm,
        Bodyguard,
        HeightenedSenses,
        Tolerance,
        DeathRay,
        DeathRayDebuff,
        Intuition,
        EnchantedSummoning,
        ShieldBash,
        Whispering,
        CombatTraining,
        InjectedStrikeDebuff,
        InjectedStrike,
        UnknownTomato,
        PlayingTheOddsDebuff,
        DragonTurtleDebuff,
        Boarding,
        Potency,
        ThrustDebuff,
        FistsOfFury, // 1169
        BarrabHemolymphConcentrate,
        JukariBurnPoiltice,
        KurakAmbushersEssence,
        BarakoDraftOfMight,
        UraliTranceTonic,
        SakkhraProphylaxis, // 1175
        Sparks,
        Swarm,
        BoneBreaker,
        Unknown2,
        SwarmImmune,
        BoneBreakerImmune,
        UnknownGoblin,
        UnknownRedDrop,
        UnknownStar,
        FeintDebuff,
        CaddelliteInfused,
        PotionGloriousFortune,
        MysticalPolymorphTotem,
        UnknownDebuff
    }

    public sealed class AddBuffPacket : Packet
    {
        public AddBuffPacket(Mobile m, BuffInfo info)
            : this(m, info.ID, info.TitleCliloc, info.TitleArguments, info.SecondaryCliloc, info.SecondaryArguments, info.ThirdCliloc, info.ThirdArguments, info.NoTimer ? TimeSpan.Zero : (info.TimeStart != DateTime.MinValue) ? ((info.TimeStart + info.TimeLength) - DateTime.UtcNow) : TimeSpan.Zero)
        {
        }

        public AddBuffPacket(Mobile mob, BuffIcon iconID, int titleCliloc, TextDefinition titleargs, int secondaryCliloc, TextDefinition secargs, int thirdCliloc, TextDefinition thirdargs, TimeSpan length)
            : base(0xDF)
        {
            bool args = titleargs != null || secargs != null || thirdargs != null;

            string title = $"{titleargs ?? ""}{(args ? "\0" : "")}";
            string secondary = $"{secargs ?? ""}{(args ? "\0" : "")}";
            string third = $"{thirdargs ?? ""}{(args ? "\0" : "")}";

            EnsureCapacity(46 + (title.Length * 2) + (secondary.Length * 2) + (third.Length * 2));

            m_Stream.Write(mob.Serial); // Serial

            m_Stream.Write((short)iconID);	// BuffIconType
            m_Stream.Write((short)0x1); // Buffs Count

            m_Stream.Write((short)0x0); // Source Type
            m_Stream.Write((short)0x0);

            m_Stream.Write((short)iconID); // Buff Icon ID
            m_Stream.Write((short)0x1); // Buff Queue Index

            m_Stream.Write(0x0);

            if (length < TimeSpan.Zero)
                length = TimeSpan.Zero;

            m_Stream.Write((short)Math.Round(length.TotalSeconds)); // Need this in TotalSeconds (rounded)

            m_Stream.Fill(3); // byte[3] 0x00

            m_Stream.Write(titleCliloc); // Buff Title Cliloc
            m_Stream.Write(secondaryCliloc); // Buff Secondary Cliloc
            m_Stream.Write(thirdCliloc); // Buff Third Cliloc

            m_Stream.Write((short)(title.Length)); // Primary Cliloc Arguments Length

            if (title.Length > 0)
            {                
                m_Stream.WriteLittleUniFixed(title, title.Length); // Primary Cliloc Arguments
            }

            m_Stream.Write((short)(secondary.Length)); // Secondary Cliloc Arguments

            if (secondary.Length > 0)
            {                
                m_Stream.WriteLittleUniFixed(secondary, secondary.Length); // Secondary Cliloc Arguments
            }

            m_Stream.Write((short)(third.Length)); // Third Cliloc Arguments Length

            if (third.Length > 0)
            {                
                m_Stream.WriteLittleUniFixed(third, third.Length); // Third Cliloc Arguments
            }
        }
    }

    public sealed class RemoveBuffPacket : Packet
    {
        public RemoveBuffPacket(Mobile mob, BuffInfo info)
            : this(mob, info.ID)
        {
        }

        public RemoveBuffPacket(Mobile mob, BuffIcon iconID)
            : base(0xDF)
        {
            EnsureCapacity(13);
            m_Stream.Write(mob.Serial);

            m_Stream.Write((short)iconID);	//ID
            m_Stream.Write((short)0x0);	//Type 0 for removal. 1 for add 2 for Data

            m_Stream.Fill(4);
        }
    }
}
