using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    public class Dupre : MondainQuester
    {
        [Constructable]
        public Dupre()
            : base("Dupre")
        {
        }

        public Dupre(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(ThePurpleAleOfHonorQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            CantWalk = true;
            Female = false;
            Race = Race.Human;
            Body = 0x190;

            Hue = 33770;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            SetWearable(new PlateArms());
            SetWearable(new PlateChest());
            SetWearable(new PlateLegs());
            SetWearable(new PlateGloves());
            SetWearable(new BodySash(), 1158);
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

    public class ThePurpleAleOfHonorQuest : BaseQuest
    {
        public ThePurpleAleOfHonorQuest()
        {
            AddObjective(new SlayObjective(typeof(UnboundEnergyVortex), "unbound energy vortex", 1));

            AddReward(new BaseReward(typeof(MugOfPurpleAle), "Mug of Purple Ale", 0x09EF, 1158));
        }

        public override object Title => "The Purple Ale of Honor";

        public override object Description => "You've met Dupre at the Keg and Anchor. He smiles as he leans on the bar. \"No doubt the realm is in turmoil...with the Shrines destroyed virtue is sapped each day. Still Honorable combat exists between those courageous and truthful to the art to practice it. A daemon called Arcadion has challenged anyone brave enough to venture into Dungeon Shame and prove their worth in combat. Are ye up to the task? Upsetting a daemon is the last thing we need at a time like this...\"";

        public override object Refuse => "You're right. it's not going to be easy. Those unbound energy vortexes pack a nasty punch. If you don't have it in you. I understand. Doubt Arcadion will though. Gather the resources, train your skills, and recruit some comrades to help you dispatch that unbound energy vortex in Shame!";

        public override object Uncomplete => "You're right. it's not going to be easy. Those unbound energy vortexes pack a nasty punch. If you don't have it in you. I understand. Doubt Arcadion will though. Gather the resources, train your skills, and recruit some comrades to help you dispatch that unbound energy vortex in Shame!";

        public override object Complete => "Dupre looks elated as you relive the tale of your great victory with him and all those assembled in the pub! The crowd hangs on your every word and erupts in raucous cheer as you describe in great detail the killing blow to the mighty unbound energy vortex! Dupre smiles as he slides a mug of Purple Ale your way with a salutatory, Cheers!";

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
