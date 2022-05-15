namespace Server.Mobiles
{
    [CorpseName("a boura corpse")]
    public class HighPlainsBoura : BaseCreature
    {
        [Constructable]
        public HighPlainsBoura()
            : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a high plains boura";
            Body = 715;

            SetStr(400, 435);
            SetDex(90, 96);
            SetInt(25, 30);

            SetHits(555, 618);

            SetDamage(20, 25);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 50, 60);
            SetResistance(ResistanceType.Fire, 35, 40);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.Anatomy, 95.2, 105.4);
            SetSkill(SkillName.MagicResist, 60.7, 70.0);
            SetSkill(SkillName.Tactics, 95.4, 105.7);
            SetSkill(SkillName.Wrestling, 105.1, 115.3);

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 47.1;

            Fame = 5000;
            Karma = -5000;

            SetSpecialAbility(SpecialAbility.TailSwipe);
            SetSpecialAbility(SpecialAbility.ColossalBlow);
        }

        public HighPlainsBoura(Serial serial) : base(serial)
        {
        }

        public override int Meat => 10;

        public override int Hides => 22;

        public override int DragonBlood => 8;

        public override HideType HideType => HideType.Horned;

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
