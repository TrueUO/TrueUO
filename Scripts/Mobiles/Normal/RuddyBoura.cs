namespace Server.Mobiles
{
    [CorpseName("a boura corpse")]
    public class RuddyBoura : BaseCreature
    {
        [Constructable]
        public RuddyBoura() : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a ruddy boura";
            Body = 715;

            SetStr(396, 480);
            SetDex(68, 82);
            SetInt(16, 20);

            SetHits(435, 509);
            SetStam(68, 82);
            SetMana(16, 20);

            SetDamage(16, 20);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 50, 60);
            SetResistance(ResistanceType.Fire, 35, 40);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.Anatomy, 86.6, 88.8);
            SetSkill(SkillName.MagicResist, 69.7, 87.7);
            SetSkill(SkillName.Tactics, 83.3, 88.8);
            SetSkill(SkillName.Wrestling, 86.6, 87.9);

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 19.1;

            Fame = 5000;
            Karma = -2500;

            SetSpecialAbility(SpecialAbility.ColossalBlow);
        }

        public RuddyBoura(Serial serial) : base(serial)
        {
        }

        public override int Meat => 10;

        public override int Hides => 20;

        public override int DragonBlood => 8;

        public override HideType HideType => HideType.Spined;

        public override FoodType FavoriteFood => FoodType.FruitsAndVegies;

        public override int GetIdleSound()
        {
            return 1507;
        }

        public override int GetAngerSound()
        {
            return 1504;
        }

        public override int GetHurtSound()
        {
            return 1506;
        }

        public override int GetDeathSound()
        {
            return 1505;
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
