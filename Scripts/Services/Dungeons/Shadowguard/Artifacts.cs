using Server.Engines.Craft;

namespace Server.Items
{
    public class AnonsBoots : Boots
    {
        public override int LabelNumber => 1156295;  // Anon's Boots

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public AnonsBoots()
        {
            Hue = 1325;

            Attributes.AttackChance = -5;
            Attributes.DefendChance = 10;
        }

        public AnonsBoots(Serial serial) : base(serial)
        {
        }

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

    public class AnonsSpellbook : Spellbook
    {
        public override int LabelNumber => 1156344;

        public override bool IsArtifact => true;

        [Constructable]
        public AnonsSpellbook()
        {
            LootType = LootType.Blessed;
            SkillBonuses.SetValues(0, SkillName.Magery, 15.0);
            Attributes.BonusInt = 8;
            Attributes.SpellDamage = 15;
            Attributes.LowerManaCost = 10;
            Attributes.LowerRegCost = 10;

            Slayer = Utility.RandomBool() ? SlayerName.Dinosaur : SlayerName.Myrmidex;
        }

        public AnonsSpellbook(Serial serial) : base(serial)
        {
        }

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

    public class BalakaisShamanStaff : WildStaff
    {
        public override int LabelNumber => 1156125;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public BalakaisShamanStaff()
        {
            SkillBonuses.SetValues(0, SkillName.Meditation, 10.0);
            WeaponAttributes.MageWeapon = 30;
            Attributes.SpellChanneling = 1;
            Attributes.EnhancePotions = 25;
        }

        public BalakaisShamanStaff(Serial serial) : base(serial)
        {
        }

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

    public class BalakaisShamanStaffGargoyle : BaseWand
    {
        public override int LabelNumber => 1156125;
        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public BalakaisShamanStaffGargoyle() : base(WandEffect.None, 0, 0)
        {
            SkillBonuses.SetValues(0, SkillName.Meditation, 10.0);
            WeaponAttributes.MageWeapon = 30;
            Attributes.SpellChanneling = 1;
            Attributes.EnhancePotions = 25;
        }

        public BalakaisShamanStaffGargoyle(Serial serial) : base(serial)
        {
        }

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

    public class EnchantressCameo : BaseTalisman
    {
        public override int LabelNumber => 1156301;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public EnchantressCameo() : base(0x2F5B)
        {
            Hue = 1645;
            Attributes.BonusStr = 1;
            Attributes.RegenHits = 2;
            Attributes.AttackChance = 10;
            Attributes.WeaponSpeed = 5;
            Attributes.WeaponDamage = 20;

            Slayer = (TalismanSlayerName)Utility.RandomList(11, 13, 14, 15, 16, 17);
        }

        public EnchantressCameo(Serial serial) : base(serial)
        {
        }

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

    public class GrugorsShield : WoodenShield
    {
        public override int LabelNumber => 1156129;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GrugorsShield()
        {
            SkillBonuses.SetValues(0, SkillName.Parry, 10.0);
            Attributes.BonusStr = 10;
            Attributes.BonusStam = 10;
            Attributes.RegenHits = 5;
            Attributes.WeaponSpeed = 10;

            PhysicalBonus = 4;
            FireBonus = 4;
            ColdBonus = 4;
            PoisonBonus = 4;
            EnergyBonus = 3;
        }

        public GrugorsShield(Serial serial) : base(serial)
        {
        }

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

    public class HalawasHuntingBow : Yumi
    {
        public override int LabelNumber => 1156127;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public HalawasHuntingBow()
        {
            Slayer = SlayerName.Eodon;
            WeaponAttributes.HitLeechMana = 20;
            Velocity = 60;
            Attributes.AttackChance = 20;
            Attributes.WeaponSpeed = 45;
        }

        public HalawasHuntingBow(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0 && WeaponAttributes.HitLeechMana != 50)
                WeaponAttributes.HitLeechMana = 50;
        }
    }

    public class HawkwindsRobe : BaseOuterTorso, IRepairable
    {
        public CraftSystem RepairSystem => DefTailoring.CraftSystem;

        public override int LabelNumber => 1156299;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public HawkwindsRobe() : base(0x7816, 0)
        {
            Attributes.RegenMana = 2;
            Attributes.SpellDamage = 5;
            Attributes.LowerManaCost = 10;
            Attributes.LowerRegCost = 10;
        }

        public HawkwindsRobe(Serial serial) : base(serial)
        {
        }

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

    public class JumusSacredHide : FurCape
    {
        public override int LabelNumber => 1156130;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public JumusSacredHide()
        {
            Attributes.SpellDamage = 5;
            Attributes.CastRecovery = 1;
            Attributes.WeaponDamage = 20;

            SAAbsorptionAttributes.EaterPoison = 15;
            Resistances.Fire = 5;
        }

        public JumusSacredHide(Serial serial) : base(serial)
        {
        }

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

    public class JuonarsGrimoire : NecromancerSpellbook
    {
        public override int LabelNumber => 1156300;

        public override bool IsArtifact => true;

        [Constructable]
        public JuonarsGrimoire()
        {
            Hue = 2500;

            SkillBonuses.SetValues(0, SkillName.Necromancy, 15.0);
            Slayer = SlayerGroup.RandomSuperSlayerTOL();

            Attributes.BonusInt = 8;
            Attributes.SpellDamage = 15;
            Attributes.LowerManaCost = 10;
            Attributes.LowerRegCost = 10;
        }

        public JuonarsGrimoire(Serial serial) : base(serial)
        {
        }

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

    public class LereisHuntingSpear : Spear
    {
        public override int LabelNumber => 1156128;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public LereisHuntingSpear()
        {
            WeaponAttributes.HitCurse = 10;
            Slayer = SlayerName.ReptilianDeath;
            WeaponAttributes.HitLeechMana = 80;
            Attributes.AttackChance = 20;
            Attributes.WeaponSpeed = 30;
            Attributes.WeaponDamage = 60;

            AosElementDamages.Poison = 100;
        }

        public LereisHuntingSpear(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0 && WeaponAttributes.HitLeechMana != 50)
                WeaponAttributes.HitLeechMana = 50;
        }
    }

    public class MinaxsSandles : Sandals
    {
        public override int LabelNumber => 1156297;  // Minax's Sandles

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public MinaxsSandles()
        {
            Hue = 1645;
            Attributes.Luck = 150;
            Attributes.LowerManaCost = 5;
            Attributes.LowerRegCost = 10;

            switch (Utility.Random(5))
            {
                case 0: Resistances.Physical = -3; break;
                case 1: Resistances.Fire = -3; break;
                case 2: Resistances.Cold = -3; break;
                case 3: Resistances.Poison = -3; break;
                case 4: Resistances.Energy = -3; break;
            }
        }

        public MinaxsSandles(Serial serial) : base(serial)
        {
        }

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

    public class OzymandiasObi : Obi
    {
        public override int LabelNumber => 1156298;

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public OzymandiasObi()
        {
            Hue = 2105;
            Attributes.BonusStr = 10;
            Attributes.BonusStam = 10;
            Attributes.RegenStam = 2;
        }

        public OzymandiasObi(Serial serial) : base(serial)
        {
        }

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

    public class ShantysWaders : ThighBoots
    {
        public override int LabelNumber => 1156296;  // Shanty's Waders

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public ShantysWaders()
        {
            Attributes.AttackChance = 10;
            Attributes.DefendChance = -5;
        }

        public ShantysWaders(Serial serial) : base(serial)
        {
        }

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

    public class TotemOfTheTribe : BaseTalisman
    {
        public override int LabelNumber => 1156294;

        public override bool IsArtifact => true;

        [Constructable]
        public TotemOfTheTribe() : base(0x2F5A)
        {
            SAAbsorptionAttributes.EaterDamage = 5;
            Attributes.RegenHits = 2;
            Attributes.AttackChance = 5;
            Attributes.DefendChance = 5;
        }

        public TotemOfTheTribe(Serial serial) : base(serial)
        {
        }

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

    public class UnstableTimeRift : Item
    {
        public override int LabelNumber => 1156320;  // An Unstable Time Rift

        [Constructable]
        public UnstableTimeRift()
            : base(14068)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m.InRange(GetWorldLocation(), 3))
                LabelTo(m, 1156321); // *You peer into the Time Rift and see back to the very beginning of Time...*
        }

        public UnstableTimeRift(Serial serial)
            : base(serial)
        {
        }

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

    [TypeAlias("Server.Items.MocapotilsObsidianSword")]
    public class MocapotlsObsidianSword : PaladinSword
    {
        public override int LabelNumber => 1156131;  // Moctapotl's Obsidian Sword

        public override bool IsArtifact => true;

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public MocapotlsObsidianSword()
        {
            WeaponAttributes.HitHarm = 50;
            WeaponAttributes.HitPhysicalArea = 50;
            WeaponAttributes.HitLeechStam = 100;
            WeaponAttributes.SplinteringWeapon = 20;
            Attributes.WeaponSpeed = 40;
            Attributes.WeaponDamage = 75;

            Hue = 1932;
        }

        public MocapotlsObsidianSword(Serial serial)
            : base(serial)
        {
        }

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
