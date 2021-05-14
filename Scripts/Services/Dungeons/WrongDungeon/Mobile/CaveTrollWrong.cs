namespace Server.Mobiles
{
    [CorpseName("a cave troll corpse")]
    public class CaveTrollWrong : BaseCreature
    {
        [Constructable]
        public CaveTrollWrong()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Cave Troll";
            Body = Utility.RandomList(53, 54);
            BaseSoundID = 461;
            Hue = 674;

            SetStr(116, 130);
            SetDex(47, 63);
            SetInt(46, 70);

            SetHits(2143, 2204);

            SetDamage(8, 14);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 70, 80);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 70, 80);
            SetResistance(ResistanceType.Poison, 70, 80);
            SetResistance(ResistanceType.Energy, 50, 60);

            SetSkill(SkillName.MagicResist, 86.2, 92.4);
            SetSkill(SkillName.Tactics, 124.7, 138.6);
            SetSkill(SkillName.Wrestling, 125.5, 135.6);

            Fame = 3500;
            Karma = -3500;
        }

        public CaveTrollWrong(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses => true;
        public override bool AllureImmune => true;
        public override int TreasureMapLevel => 3;

        public override int Meat => 2;

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
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
