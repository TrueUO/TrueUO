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

            CantWalk = true;
            Female = false;
            Race = Race.Human;

            Hue = 33770;
            HairItemID = 0x2048;
            HairHue = 1150;
            FacialHairItemID = 0x204C;
            FacialHairHue = 1150;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new FancyShirt());
            AddItem(new JinBaori(692));
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

    public class InServiceOfThePoorhouseQuest : BaseQuest
    {
        public override bool DoneOnce => true;

        public InServiceOfThePoorhouseQuest()
        {
            AddObjective(new InternalObjective());
            AddObjective(new ObtainObjective(typeof(BreadLoaf), "bread loaf", 1, 0x103B));
            AddObjective(new ObtainObjective(typeof(Shoes), "shoes", 1, 0x170F));

            AddReward(new BaseReward(typeof(EmbroideredPillow), "An Embroidered Pillow", 0x9E1D, 2125));
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

        private class InternalObjective : ObtainObjective
        {
            public InternalObjective()
                : base(typeof(Pitcher), "milk", 1)
            {
            }

            public override bool IsObjective(Item item)
            {
                if (base.IsObjective(item))
                {
                    Pitcher pitcher = (Pitcher)item;

                    if (pitcher.Content == BeverageType.Milk && !pitcher.IsEmpty)
                        return true;
                }

                return false;
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.WriteEncodedInt(0); // version
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadEncodedInt();
            }
        }
    }
}
