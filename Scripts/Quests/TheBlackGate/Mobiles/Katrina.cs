using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    public class Katrina : MondainQuester
    {
        [Constructable]
        public Katrina()
            : base("Katrina")
        {
        }

        public Katrina(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(TheShepherdsWayQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            CantWalk = true;
            Female = true;
            Race = Race.Human;

            Hue = 33770;
            HairItemID = 0x203C;
            HairHue = 1141;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Skirt(1308));
            AddItem(new Shirt());
            AddItem(new Boots());
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

    public class TheShepherdsWayQuest : BaseQuest
    {
        public TheShepherdsWayQuest()
        {
            AddObjective(new SlayObjective(typeof(Troglodyte), "troglodytes", 50));

            AddReward(new BaseReward(typeof(ShornWool), "Shorn Wool", 0xDFE, 2051));
        }

        public override object Title => "The Shepherd's Way";

        public override object Description => "You have been approached by Katrina who tells you of a group of sheep farmers who are being tormented by Troglodytes from the Painted Caves. She has had trouble getting the guards to take her requests for assistance seriously, especially in the face of the greater perils that threaten Britannia. She asks if you will humble yourself before the farmers and assist them without promise of payment so their livelihood will not be at stake.";

        public override object Refuse => "Katrina is dismayed at your refusal, but understanding. There are much greater riches to be found elsewhere in the realm.";

        public override object Uncomplete => "You haue agreed to assist Katrina and the Farmers. Venture to the Painted Caves and slay the Troglodytes who haue been eating the farmer's sheep!";

        public override object Complete => "You've cleared the painted eaves and dealt with the troglodyte threat. The herd is already rebounding now that they aren't being eaten on a daily basis! The farmers appreciate your humility in dealing with a task far beneath the tactical acclaim of someone such as yourself. Katrina and the farmers are quite thankful and present you with some special wool as a keepsake.";

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
