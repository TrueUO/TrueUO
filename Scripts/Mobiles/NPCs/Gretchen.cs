using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class Curiosities : BaseQuest
    {
        /* Curiosities */
        public override object Title => "Curiosities";


        public override object Description => 1094978;
        public override object Refuse => "You are Scared from this Task !! Muahahah";

        public override object Uncomplete => "I am sorry that you have not accepted!";

        public override object Complete => 1094981;

        public Curiosities() : base()
        {
            AddObjective(new ObtainObjective(typeof(FertileDirt), "Fertil Dirt", 3, 0xF81));
            AddObjective(new ObtainObjective(typeof(Bone), "Bone", 3, 0xF7e));

            AddReward(new BaseReward(typeof(ExplodingTarPotion), "Exploding Tar Potion"));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class Gretchen : MondainQuester
    {
        public override Type[] Quests => new Type[]
{
            typeof( Curiosities )
};

        [Constructable]
        public Gretchen() : base("Gretchen", "the Alchemist")
        {
        }

        public Gretchen(Serial serial) : base(serial)
        {
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;

            CantWalk = true;
            Direction = Direction.East;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
