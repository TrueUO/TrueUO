namespace Server.Mobiles
{
    // Things Unknown:
    // 1. Starting skills other than wrestling
    // 2. AI Type - Melee vs Magic?
    // 3. Fame and Karma values
    // 4. Sounds - currently just using dragon

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
            //SetSkill(SkillName.Tactics, 90.3, 99.3);
            //SetSkill(SkillName.MagicResist, 75.3, 90.0);
            //SetSkill(SkillName.Anatomy, 65.5, 69.4);
            //SetSkill(SkillName.Healing, 72.2, 98.9);

            //Fame = 5000;  //Guessing here
            //Karma = 5000;  //Guessing here

            Tamable = true;
            ControlSlots = 3;
            MinTameSkill = 94.0;
        }

        public JuvenileUmbrascale(Serial serial)
            : base(serial)
        {
        }

        public override FoodType FavoriteFood => FoodType.Meat;
        public override bool CanAngerOnTame => true;
     
        public override int Hides => 10;
        public override int Meat => 3;

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
