using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class EvidenceQuest : BaseQuest
    {
        public EvidenceQuest()
            : base()
        {
            AddObjective(new ObtainObjective(typeof(OrdersFromMinax), "orders from minax", 1));

            AddReward(new BaseReward(typeof(RewardBox), 1072584));
        }

        /* Evidence */
        public override object Title => 1072906;
        /* We believe the Black Order has fallen under the sway of Minax, somehow.  Seek evidence that proves our theory 
        by piercing the secrets of the Citadel. */
        public override object Description => 1072964;
        /* Many fear to tangle with the wicked sorceress.  I understand and appreciate your concerns. */
        public override object Refuse => 1072975;
        /* I don't know where inside The Citadel such evidence could be found.  Perhaps the most guarded sanctum is 
        the place to look. */
        public override object Uncomplete => 1072976;
        public override bool CanOffer()
        {
            return MondainsLegacy.Citadel;
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

    public class Sarakki : MondainQuester
    {
        [Constructable]
        public Sarakki()
            : base("Sarakki", "the notary")
        {
        }

        public Sarakki(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(EvidenceQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            Race = Race.Human;

            Hue = 0x841E;
            HairItemID = 0x2049;
            HairHue = 0x1BB;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Shoes(0x740));
            AddItem(new FancyShirt(0x72C));
            AddItem(new Skirt(0x53C));
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
}
