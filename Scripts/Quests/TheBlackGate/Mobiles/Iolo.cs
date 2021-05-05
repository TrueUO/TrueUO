using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class Iolo : MondainQuester
    {
        [Constructable]
        public Iolo()
            : base("Iolo")
        {
        }

        public Iolo(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(InServiceOfThePoorhouseQuest) };

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
            SetWearable(new FancyShirt());
            SetWearable(new JinBaori(), 692);
            SetWearable(new Boots());
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

    public class InServiceOfThePoorhouseQuest : BaseQuest
    {
        public InServiceOfThePoorhouseQuest()
        {
            AddObjective(new CollectionsObtainObjective(typeof(ShepherdsCrookOfHumility), "Shepherd's Crook of Humility (Replica)", 1));
            AddReward(new BaseReward(1075852)); // A better understanding of Britannia's people
        }

        public override object Title => "In Service of the Poorhouse";

        public override object Description => "Greetings friend. Welcome to the poorhouse. Here we offer what we can to Britannia's most needy. We don't have much, but what we do comes from the compassionate donations of citizens like yourself. Can you help us? Any supplies would go a long way to making those who need our help more comfortable. Even simple things like bread, milk, and shoes can go a long way to easing the suffering of those who have nothing.";

        public override object Refuse => "There are those who are too preoccupied to be compassionate to others. We will find our solace elsewhere, friend.";

        public override object Uncomplete => "Your generosity will go a long way. friend. Most taverns sell a round loaf of bread and a pitcher of milk for a small amount of gold. Cobblers throughout the realm sell simple shoes for a small sum as well. If you can help the poorhouse, it would go such a long way.";

        public override object Complete => "You are truly compassionate! lolo takes the items you have provided the poorhouse and distributes them to the overjoyed faces of those in need. The poorhouse welcomes more and more Fellowship refugees each day. Your heart warms knowing you did right by the people of Britannia. Gazing about the poorhouse in satisfaction a tug at your boots draws your attention down. A small child hands you a pillow.";

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
