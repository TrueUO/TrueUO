using Server.Gumps;

namespace Server.Items
{
    public class MugOfPurpleAle : BaseBeverage
    {
        public override int MaxQuantity => 5;
        public override int ComputeItemID() { return ItemID; }

        [Constructable]
        public MugOfPurpleAle()
        {
            ItemID = 0x9EF;
            Name = "Mug Of Purple Ale";
            Hue = 1158;
            Quantity = 5;
        }

        public MugOfPurpleAle(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "A Frothy Mug of Purple Ale",
                Description = "Shared by Dupre",
                Line1 = "A specialty of the Keg and Anchor, Dupre is known to enjoy a mug or two of Purple Ale.",
                Line2 = "In proving your Honor to Dupre in battle against the daemon Arcadion, he has shared this mug of ale with you."
            };

            g.RenderString(from);
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
