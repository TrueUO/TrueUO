using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class Geoffrey : MondainQuester
    {
        [Constructable]
        public Geoffrey()
            : base("Geoffrey")
        {
        }

        public Geoffrey(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(InTheFaceOfDragonsQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            Race = Race.Human;
            Body = 0x190;

            Hue = 33770;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            SetWearable(new ChainChest());
            SetWearable(new ChainLegs());
            SetWearable(new BodySash(), 33);
            SetWearable(new Boots(), 2012);
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

    public class InTheFaceOfDragonsQuest : BaseQuest
    {
        public InTheFaceOfDragonsQuest()
        {
            AddObjective(new CollectionsObtainObjective(typeof(ShepherdsCrookOfHumility), "Shepherd's Crook of Humility (Replica)", 1));
            AddReward(new BaseReward(1075852)); // A better understanding of Britannia's people
        }

        public override object Title => "In The Face of Dragons";

        public override object Description => "We've got those Fellowship killers on the run! Ha! I hope you are here to join in the fight because we can't relent for even a moment! Reports suggest the Fellowship may be attempting to train dragons down in Destard to launch a full scale attack against Britannia! We've got to stop them! Head down to Destard and thin the herd to keep the Fellowship from building a dragon army we won't be able to stop!";

        public override object Refuse => "Then rue got no time for you! If you can't summon the courage to join in this fight be gone with you!";

        public override object Uncomplete => "What are you waiting for? We don't haue time to lose! The Fellowship is there trying to recruit a dragon army to destroy Britannia! Go to Destard and thin the herd!";

        public override object Complete => "You summoned your inner courage and headed deep inside the dreaded Destard to encounter the dragon herd. As your eyes awoke with the power of nightsight you could see deep into the dungeon. Hundreds of lizard-eyes looked back at you and attacked at once! One by one you dispatched countless drakes and dragons. You are tired from battle but invigorated with your victory! Sir Geoffrey is quite pleased and proud of your progress. He gives you a damaged Fellowship sword as a memento of your valiant efforts!";

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
