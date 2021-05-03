using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class RumorsAboundQuest : BaseQuest
    {
        public RumorsAboundQuest()
        {
            AddObjective(new DeliverObjective(typeof(EgwexemWrit), "Egwexem's Writ", 1, typeof(Naxatilor), "Naxatilor"));

            AddReward(new BaseReward(1112731)); // Knowing that you did the right thing.
        }

        public override TimeSpan RestartDelay => TimeSpan.FromHours(12);

        public override Type NextQuest { get { return typeof(TheArisenQuest); } }
        public override bool DoneOnce => true;

        /* Rumors Abound */
        public override object Title => 1112514;

        /* I know not the details, but from what little truth that can be separated
		 * from rumor, it seems that the Holy City is being savaged repeatedly by the
		 * Arisen. Diligence demands that you make your way with haste to the Holy
		 * City, which lies some distance to the south-east. Please take this writ
		 * and deliver it to Naxatilor so he will know that I sent you. */
        public override object Description => 1112515;

        /* The safety of the Holy City and the Elders is at stake. Surely you cannot
		 * be refusing to help? */
        public override object Refuse => 1112516;

        /* Make haste to the Holy City! */
        public override object Uncomplete => 1112518;

        /* I am sorry, I am too busy to...
		 * 
		 * *You hand Naxatilor the writ*
		 * 
		 * I see that Egwexem has sent you. It is good that you have come, we could
		 * use your help. */
        public override object Complete => 1112519;

        public override bool CanOffer()
        {
            return !Owner.AbyssEntry;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    } 

    public class Egwexem : MondainQuester
    {
        [Constructable]
        public Egwexem()
            : base("Egwexem", "the Noble")
        {
        }

        public Egwexem(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(RumorsAboundQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            CantWalk = true;
            Body = 666;
            HairItemID = 16987;
            HairHue = 1801;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new GargishClothChest());
            AddItem(new GargishClothKilt());
            AddItem(new GargishClothLegs(Utility.RandomNeutralHue()));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class EgwexemWrit : Item
    {
        public override int LabelNumber { get { return 1112520; } } // Egwexem's Writ

        [Constructable]
        public EgwexemWrit()
            : base(0x14EF)
        {
            LootType = LootType.Blessed;
            Hue = 556;
        }

        public EgwexemWrit(Serial serial)
            : base(serial)
        {
        }

        public override bool HiddenQuestItemHue => true;
        public override bool Nontransferable { get { return true; } }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1072351); // Quest Item
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
