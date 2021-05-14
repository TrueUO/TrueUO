using Server.Gumps;

namespace Server.Items
{
    public class ShornWool : Item
    {
        public override string DefaultName => "a Shorn Wool";

        [Constructable]
        public ShornWool()
            : base(0xDFE)
        {
            Hue = 2051;
        }

        public ShornWool(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "Premium Shorn Wool",
                Description = "Given to you by Katrina",
                Line1 = "You have killed the trogdolytes that were tormenting the sheep farmers.",
                Line2 = "In performing such a task for those who cannot defend themselves you have humbled yourself in their service."
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
