using Server.Items;
using Server.Mobiles;
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

            CantWalk = true;
            Female = false;
            Race = Race.Human;

            Hue = 33770;
            HairItemID = 0x203B;
            HairHue = 1117;
            FacialHairItemID = 0x2041;
            FacialHairHue = 1117;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new ChainChest());
            AddItem(new ChainLegs());
            AddItem(new BodySash(33));
            AddItem(new Boots(2012));
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
            AddObjective(new SlayObjective(typeof(Drake), "drakes", 40));
            AddObjective(new SlayObjective(typeof(Dragon), "dragons", 20));
            AddObjective(new SlayObjective(typeof(GreaterDragon), "greater dragons", 5));

            AddReward(new BaseReward(typeof(BrokenFellowshipSword), "A Broken Fellowship Sword", 0xA33F, 2117));
        }

        public override object Title => "In The Face of Dragons";

        public override object Description => "We've got those Fellowship killers on the run! Ha! I hope you are here to join in the fight because we can't relent for even a moment! Reports suggest the Fellowship may be attempting to train dragons down in Destard to launch a full scale attack against Britannia! We've got to stop them! Head down to Destard and thin the herd to keep the Fellowship from building a dragon army we won't be able to stop!";

        public override object Refuse => "Then rue got no time for you! If you can't summon the courage to join in this fight be gone with you!";

        public override object Uncomplete => "What are you waiting for? We don't have time to lose! The Fellowship is there trying to recruit a dragon army to destroy Britannia! Go to Destard and thin the herd!";

        public override object Complete => "You summoned your inner courage and headed deep inside the dreaded Destard to encounter the dragon herd. As your eyes awoke with the power of night sight you could see deep into the dungeon. Hundreds of lizard-eyes looked back at you and attacked at once! One by one you dispatched countless drakes and dragons. You are tired from battle but invigorated with your victory! Sir Geoffrey is quite pleased and proud of your progress. He gives you a damaged Fellowship sword as a memento of your valiant efforts!";

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
