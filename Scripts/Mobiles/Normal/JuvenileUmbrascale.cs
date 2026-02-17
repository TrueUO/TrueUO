using Server.Gumps;
using System;

namespace Server.Mobiles
{
    public class HatchingUmbrascaleEgg : Item, ICreatureStatuette
    {
        public Type CreatureType => typeof(JuvenileUmbrascale);

        [Constructable]
        public HatchingUmbrascaleEgg()
            : base(45778)
        {
            Name = "Hatching Umbrascale Egg";
        }

        public HatchingUmbrascaleEgg(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                if (from.Skills[SkillName.AnimalTaming].Value >= 100)
                {
                    from.SendGump(new ConfirmMountStatuetteGump(this));
                }
                else
                {
                    from.SendLocalizedMessage(1158959, "100"); // ~1_SKILL~ Animal Taming skill is required to redeem this pet.
                }
            }
            else
            {
                SendLocalizedMessageTo(from, 1010095); // This must be on your person to use.
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1158954); // *Redeemable for a pet*<br>*Requires Grandmaster Taming to Claim Pet*
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    // Things Unknown:
    // 1. Sounds - currently just using dragon

    [CorpseName("a juvenile umbrascale corpse")]
    public class JuvenileUmbrascale : BaseMount
    {
        public override double HealChance => 1.0;

        [Constructable]
        public JuvenileUmbrascale()
            : this("juvenile umbrascale")
        {
        }

        [Constructable]
        public JuvenileUmbrascale(string name)
            : base(name, 1409, 0x3EDD, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            SetStr(760);
            SetDex(165);
            SetInt(300);

            SetHits(450);
            SetStam(150);
            SetMana(300);

            SetDamage(18, 24);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Fire, 50);
            SetDamageType(ResistanceType.Energy, 50);

            SetResistance(ResistanceType.Physical, 60);
            SetResistance(ResistanceType.Fire, 60);
            SetResistance(ResistanceType.Cold, 50);
            SetResistance(ResistanceType.Poison, 50);
            SetResistance(ResistanceType.Energy, 50);

            SetSkill(SkillName.Wrestling, 110.0, 130.0);
            SetSkill(SkillName.Tactics, 81.6);
            SetSkill(SkillName.MagicResist, 57.3);
            SetSkill(SkillName.Healing, 92.2);
            SetSkill(SkillName.DetectHidden, 51.3);
            SetSkill(SkillName.Parry, 63.5);
            SetSkill(SkillName.Focus, 26.0);

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 94.0;
        }

        public JuvenileUmbrascale(Serial serial)
            : base(serial)
        {
        }

        public override bool DeleteOnRelease => true;

        public override FoodType FavoriteFood => FoodType.Meat;
        
        public override void GenerateLoot()
        {
        }

        public override int GetIdleSound()
        {
            return 0x2C4;
        }

        public override int GetAttackSound()
        {
            return 0x2C0;
        }

        public override int GetDeathSound()
        {
            return 0x2C1;
        }

        public override int GetAngerSound()
        {
            return 0x2C4;
        }

        public override int GetHurtSound()
        {
            return 0x2C3;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
