using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a human corpse")]
    public class DupresSquire : BaseCreature
    {
        [Constructable]
        public DupresSquire() : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("male");
            Title = "the Squire";
            Body = 0x190;
            Hue = Utility.RandomSkinHue();
            Female = false;

            SetStr(190, 200);
            SetDex(50, 75);
            SetInt(150, 250);
            SetHits(3900, 4100);
            SetDamage(22, 28);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 50, 70);
            SetResistance(ResistanceType.Fire, 50, 70);
            SetResistance(ResistanceType.Cold, 50, 70);
            SetResistance(ResistanceType.Poison, 50, 70);
            SetResistance(ResistanceType.Energy, 50, 70);

            SetSkill(SkillName.EvalInt, 195.0, 220.0);
            SetSkill(SkillName.Magery, 195.0, 220.0);
            SetSkill(SkillName.Meditation, 195.0, 200.0);
            SetSkill(SkillName.MagicResist, 100.0, 120.0);
            SetSkill(SkillName.Tactics, 195.0, 220.0);
            SetSkill(SkillName.Wrestling, 195.0, 220.0);

            Item vikingsword = new VikingSword
            {
                Movable = false
            };
            SetWearable(vikingsword);

            Item cc = new ChainChest
            {
                Movable = false
            };
            SetWearable(cc);

            Item cl = new ChainLegs
            {
                Movable = false
            };
            SetWearable(cl);

            Item ch = new CloseHelm
            {
                Movable = false
            };
            SetWearable(ch);

            Item boots = new Boots(1)
            {
                Movable = false
            };
            SetWearable(boots);

            Item pgl = new PlateGloves
            {
                Movable = false
            };
            SetWearable(pgl);

            Item mks = new MetalKiteShield
            {
                Movable = false,
                Hue = 0x776
            };
            SetWearable(mks);

            Item bs = new BodySash(0x794)
            {
                Movable = false
            }; // dark purple
            SetWearable(bs);
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LootGold(400, 600));
        }

        public override bool CanBeParagon => false;

        public override bool InitialInnocent => true;

        public override Poison PoisonImmune => Poison.Lethal;

        public override int TreasureMapLevel => 5;

        public DupresSquire(Serial serial)
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
