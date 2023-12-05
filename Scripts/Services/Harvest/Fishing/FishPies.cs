using Server.Engines.Craft;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public enum FishPieEffect
    {
        None,
        MedBoost,
        FocusBoost,
        ColdSoak,
        EnergySoak,
        FireSoak,
        PoisonSoak,
        PhysicalSoak,
        WeaponDam,
        HitChance,
        DefChance,
        SpellDamage,
        ManaRegen,
        HitsRegen,
        StamRegen,
        SoulCharge,
        CastFocus
    }

    public class BaseFishPie : Item, IQuality
    {
        private ItemQuality _Quality;

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemQuality Quality { get => _Quality; set { _Quality = value; InvalidateProperties(); } }

        public bool PlayerConstructed => true;

        public virtual TimeSpan Duration => TimeSpan.FromMinutes(5);
        public virtual int Buff => 0;
        public virtual int BuffName => 0;
        public virtual int BuffAmount => 0;
        public virtual int BuffDescription => 0;
        public virtual FishPieEffect Effect => FishPieEffect.None;

        private static readonly Dictionary<Mobile, FishPieEffect> _EffectsList = new Dictionary<Mobile, FishPieEffect>();

        public BaseFishPie() : base(4161)
        {
            Stackable = true;
        }

        public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            Quality = (ItemQuality)quality;

            return quality;
        }

        public static bool IsUnderEffects(Mobile from, FishPieEffect type)
        {
            if (!_EffectsList.TryGetValue(from, out FishPieEffect value))
            {
                return false;
            }

            return value == type;
        }

        public static void RemoveEffects(Mobile from)
        {
            if (!_EffectsList.TryGetValue(from, out FishPieEffect value))
            {
                return;
            }

            if (value == FishPieEffect.WeaponDam)
            {
                from.Delta(MobileDelta.WeaponDamage);
            }

            _EffectsList.Remove(from);

            BuffInfo.RemoveBuff(from, BuffIcon.FishPie);            
        }

        public static void ScaleDamage(Mobile from, Mobile to, ref int totalDamage, int phys, int fire, int cold, int pois, int nrgy, int direct)
        {
            if (from is PlayerMobile && to is PlayerMobile)
            {
                return;
            }

            if (IsUnderEffects(to, FishPieEffect.PhysicalSoak) && phys > 0)
            {
                totalDamage -= (int)Math.Min(5.0, totalDamage * (phys / 100.0));
            }

            if (IsUnderEffects(to, FishPieEffect.FireSoak) && fire > 0)
            {
                totalDamage -= (int)Math.Min(5.0, totalDamage * (fire / 100.0));
            }

            if (IsUnderEffects(to, FishPieEffect.ColdSoak) && cold > 0)
            {
                totalDamage -= (int)Math.Min(5.0, totalDamage * (cold / 100.0));
            }

            if (IsUnderEffects(to, FishPieEffect.PoisonSoak) && pois > 0)
            {
                totalDamage -= (int)Math.Min(5.0, totalDamage * (pois / 100.0));
            }

            if (IsUnderEffects(to, FishPieEffect.EnergySoak) && nrgy > 0)
            {
                totalDamage -= (int)Math.Min(5.0, totalDamage * (nrgy / 100.0));
            }
        }

        public virtual void Apply(Mobile from)
        {
            RemoveEffects(from);

            _EffectsList[from] = Effect;

            switch (Effect)
            {
                default:
                case FishPieEffect.None:
                {
                    break;
                }
                case FishPieEffect.MedBoost:
                {
                    TimedSkillMod mod1 = new TimedSkillMod(SkillName.Meditation, true, 10.0, Duration)
                    {
                        ObeyCap = true
                    };
                    from.AddSkillMod(mod1);
                    break;
                }
                case FishPieEffect.FocusBoost:
                {
                    TimedSkillMod mod2 = new TimedSkillMod(SkillName.Focus, true, 10.0, Duration)
                    {
                        ObeyCap = true
                    };
                    from.AddSkillMod(mod2);
                    break;
                }
                case FishPieEffect.ColdSoak:
                {
                    break;
                }
                case FishPieEffect.EnergySoak:
                {
                    break;
                }
                case FishPieEffect.PoisonSoak:
                {
                    break;
                }
                case FishPieEffect.FireSoak:
                {
                    break;
                }
                case FishPieEffect.PhysicalSoak:
                {
                    break;
                }
                case FishPieEffect.WeaponDam:
                {
                    break;
                }
                case FishPieEffect.HitChance:
                {
                    break;
                }
                case FishPieEffect.DefChance:
                {
                    break;
                }
                case FishPieEffect.SpellDamage:
                {
                    break;
                }
                case FishPieEffect.ManaRegen:
                {
                    break;
                }
                case FishPieEffect.StamRegen:
                {
                    break;
                }
                case FishPieEffect.HitsRegen:
                {
                    break;
                }
                case FishPieEffect.SoulCharge:
                {
                    break;
                }
                case FishPieEffect.CastFocus:
                {
                    break;
                }
            }

            if (Effect != FishPieEffect.None)
            {
                Timer t = new InternalTimer(Duration, from);
                t.Start();

                BuffInfo.AddBuff(from, new BuffInfo(BuffIcon.FishPie, 1116559, $"#{Buff}", 1116560, $"+{BuffAmount}\t#{BuffDescription}", Duration, from)); // Magic Fish Buff<br>~1_val~
            }
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile _From;

            public InternalTimer(TimeSpan duration, Mobile from)
                : base(duration)
            {
                _From = from;
            }

            protected override void OnTick()
            {
                RemoveEffects(_From);
            }
        }

        public override void AddCraftedProperties(ObjectPropertyList list)
        {
            if (_Quality == ItemQuality.Exceptional)
            {
                list.Add(1060636); // Exceptional
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(BuffName, BuffAmount.ToString());
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Movable)
            {
                return;
            }

            if (!from.InRange(GetWorldLocation(), 1))
            {
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else if (Food.FillHunger(from, 4))
            {
                Apply(from);

                from.Animate(AnimationType.Eat, 0);

                from.SendLocalizedMessage(1116285, $"#{LabelNumber}"); //You eat the ~1_val~.  Mmm, tasty!

                Consume();
            }
        }

        public BaseFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write((int)_Quality);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _Quality = (ItemQuality)reader.ReadInt();
        }
    }

    public class AutumnDragonfishPie : BaseFishPie
    {
        public override int LabelNumber => 1116224; // autumn dragonfish pie
        public override int Buff => 1116553; // Autumn Dragonfish Serenity
        public override int BuffName => 1116280; // (Eat to increase meditation skill: ~1_val~)
        public override int BuffAmount => 10;
        public override int BuffDescription => 1116537; // Meditation Skill
        public override FishPieEffect Effect => FishPieEffect.MedBoost;

        [Constructable]
        public AutumnDragonfishPie()
        {
            Hue = 544;
        }

        public AutumnDragonfishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class BullFishPie : BaseFishPie
    {
        public override int LabelNumber => 1116220; // bull fish pie
        public override int Buff => 1116549; // Bull Fish Rage
        public override int BuffName => 1116276; // (Eat to increase weapon damage: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116533; // Melee Damage
        public override FishPieEffect Effect => FishPieEffect.WeaponDam;

        [Constructable]
        public BullFishPie()
        {
            Hue = 1175;
        }

        public BullFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class CrystalFishPie : BaseFishPie
    {
        public override int LabelNumber => 1116219; // crystal fish pie
        public override int Buff => 1116548; // Crystal Fish Kindred
        public override int BuffName => 1116275; // (Eat to soak energy damage: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116532; // Soak Energy
        public override FishPieEffect Effect => FishPieEffect.EnergySoak;

        [Constructable]
        public CrystalFishPie()
        {
            Hue = FishInfo.GetFishHue(typeof(CrystalFish));
        }

        public CrystalFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class FairySalmonPie : BaseFishPie
    {
        public override int LabelNumber => 1116222; // fairy salmon pie
        public override int Buff => 1116551; // Fairy Salmon Clarity
        public override int BuffName => 1116278; // (Eat to increase casting focus: ~1_val~)
        public override int BuffAmount => 2;
        public override int BuffDescription => 1116535; // Casting Focus
        public override FishPieEffect Effect => FishPieEffect.CastFocus;

        [Constructable]
        public FairySalmonPie()
        {
            Hue = FishInfo.GetFishHue(typeof(FairySalmon));
        }

        public FairySalmonPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class FireFishPie : BaseFishPie
    {
        public override int LabelNumber => 1116217; // fire fish pie
        public override int Buff => 1116546; // Fire Fish Kindred
        public override int BuffName => 1116271; // (Eat to soak fire damage: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116528; // Soak Fire
        public override FishPieEffect Effect => FishPieEffect.FireSoak;

        [Constructable]
        public FireFishPie()
        {
            Hue = 2117;
        }

        public FireFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GiantKoiPie : BaseFishPie
    {
        public override int LabelNumber => 1116216; // giant koi pie
        public override int Buff => 1116545; // Giant Koi Evasion
        public override int BuffName => 1116270; // (Eat to increase defense chance: ~1_TOKEN~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116527; // Defense Chance
        public override FishPieEffect Effect => FishPieEffect.DefChance;

        [Constructable]
        public GiantKoiPie()
        {
            Hue = FishInfo.GetFishHue(typeof(GiantKoi));
        }

        public GiantKoiPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GreatBarracudaPie : BaseFishPie
    {
        public override int LabelNumber => 1116214; // great barracuda pie
        public override int Buff => 1116543; // Great Barracuda Strike
        public override int BuffName => 1116269; // (Eat to increase hit chance: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116526; // Hit Chance
        public override FishPieEffect Effect => FishPieEffect.HitChance;

        [Constructable]
        public GreatBarracudaPie()
        {
            Hue = 1287;
        }

        public GreatBarracudaPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class HolyMackerelPie : BaseFishPie
    {
        public override int LabelNumber => 1116225; // holy mackerel pie
        public override int Buff => 1116554; // Holy Mackerel Spirit
        public override int BuffName => 1116283; // (Eat to increase mana regeneration: ~1_val~)
        public override int BuffAmount => 3;
        public override int BuffDescription => 1116540; // Mana Regeneration
        public override FishPieEffect Effect => FishPieEffect.ManaRegen;

        [Constructable]
        public HolyMackerelPie()
        {
            Hue = FishInfo.GetFishHue(typeof(HolyMackerel));
        }

        public HolyMackerelPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class LavaFishPie : BaseFishPie
    {
        public override int LabelNumber => 1116223; // lava fish pie
        public override int Buff => 1116552; // Lava Fish Soul
        public override int BuffName => 1116279; // (Eat to increase soul charge ability: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116536; // Soul Charge
        public override FishPieEffect Effect => FishPieEffect.SoulCharge;

        [Constructable]
        public LavaFishPie()
        {
            Hue = 1779;
        }

        public LavaFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class ReaperFishPie : BaseFishPie
    {
        public override int LabelNumber => 1116218; // reaper fish pie
        public override int Buff => 1116547; // Reaper Fish Kindred
        public override int BuffName => 1116274; // (Eat to soak poison damage: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116531; // Soak Poison
        public override FishPieEffect Effect => FishPieEffect.PoisonSoak;

        [Constructable]
        public ReaperFishPie()
        {
            Hue = 1152;
        }

        public ReaperFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class SummerDragonfishPie : BaseFishPie
    {
        public override int LabelNumber => 1116221; // summer dragonfish pie
        public override int Buff => 1116550; // Summer Dragonfish Rage
        public override int BuffName => 1116277; // (Eat to increase spell damage: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116534; // Spell Damage
        public override FishPieEffect Effect => FishPieEffect.SpellDamage;

        [Constructable]
        public SummerDragonfishPie()
        {
            Hue = FishInfo.GetFishHue(typeof(SummerDragonfish));
        }

        public SummerDragonfishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class UnicornFishPie : BaseFishPie
    {
        public override int LabelNumber => 1116226; // unicorn fish pie
        public override int Buff => 1116555; // Unicorn Fish Endurance
        public override int BuffName => 1116284; // (Eat to increase stamina regeneration: ~1_val~)
        public override int BuffAmount => 3;
        public override int BuffDescription => 1116541; // Staminia Regeneration
        public override FishPieEffect Effect => FishPieEffect.StamRegen;

        [Constructable]
        public UnicornFishPie()
        {
            Hue = FishInfo.GetFishHue(typeof(UnicornFish));
        }

        public UnicornFishPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class YellowtailBarracudaPie : BaseFishPie
    {
        public override int LabelNumber => 1116215; // yellowtail barracuda pie
        public override int Buff => 1116544; // Yellowtail Barracuda Vitality
        public override int BuffName => 1116282; // (Eat to increase hp regeneration: ~1_val~)
        public override int BuffAmount => 3;
        public override int BuffDescription => 1116539; // HP Regeneration
        public override FishPieEffect Effect => FishPieEffect.HitsRegen;

        [Constructable]
        public YellowtailBarracudaPie()
        {
            Hue = FishInfo.GetFishHue(typeof(YellowtailBarracuda));
        }

        public YellowtailBarracudaPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class StoneCrabPie : BaseFishPie
    {
        public override int LabelNumber => 1116227; // stone crab pie
        public override int Buff => 1116556; // Stone Crab Kindred
        public override int BuffName => 1116272; // (Eat to soak physical damage: ~1_val~)
        public override int BuffAmount => 3;
        public override int BuffDescription => 1116529; // Soak Physical
        public override FishPieEffect Effect => FishPieEffect.PhysicalSoak;

        [Constructable]
        public StoneCrabPie()
        {
            Hue = FishInfo.GetFishHue(typeof(StoneCrab));
        }

        public StoneCrabPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class SpiderCrabPie : BaseFishPie
    {
        public override int LabelNumber => 1116229; // spider crab pie
        public override int Buff => 1116558; // Spider Crab Focus
        public override int BuffName => 1116281; // (Eat to increase focus skill: ~1_val~)
        public override int BuffAmount => 10;
        public override int BuffDescription => 1116538; // Focus Skill
        public override FishPieEffect Effect => FishPieEffect.FocusBoost;

        [Constructable]
        public SpiderCrabPie()
        {
            Hue = FishInfo.GetFishHue(typeof(SpiderCrab));
        }

        public SpiderCrabPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class BlueLobsterPie : BaseFishPie
    {
        public override int LabelNumber => 1116228; // blue lobster pie
        public override int Buff => 1116557; // Blue Lobster Kindred
        public override int BuffName => 1116273; // (Eat to soak cold damage: ~1_val~)
        public override int BuffAmount => 5;
        public override int BuffDescription => 1116530; // Soak Cold
        public override FishPieEffect Effect => FishPieEffect.ColdSoak;

        [Constructable]
        public BlueLobsterPie()
        {
            Hue = FishInfo.GetFishHue(typeof(BlueLobster));
        }

        public BlueLobsterPie(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
