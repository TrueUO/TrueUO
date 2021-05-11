using Server.Gumps;

namespace Server.Items
{
    public class Illumination : Item
    {
        [Constructable]
        public Illumination()
            : base(0x1C13)
        {
            Name = "an Illumination";
            Hue = 2747;
        }

        public Illumination(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "Mariah's Illumination from the Book of Truth",
                Description = "Given for Helping Mariah Recover from the Madness of the Tetrahedron",
                Line1 = "The ornate illumination was painstakingly copied from the book of truth by Mariah.",
                Line2 = "Great care was taken to make an honest recreation from the famed Book of Truth."
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
