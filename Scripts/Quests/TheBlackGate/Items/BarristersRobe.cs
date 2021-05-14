using Server.Gumps;

namespace Server.Items
{
    public class BarristersRobe : Robe
    {
        public override string DefaultName => "a Barrister's Robe";

        [Constructable]
        public BarristersRobe()
        {
            Hue = 1367;
        }

        public BarristersRobe(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "A Common Barrister's Robe",
                Description = "Worn by Jaana at the Court of Truth in Yew",
                Line1 = "The robe commonly worn by those standing as barristers before the Court of Truth in Yew.",
                Line2 = "In delivering evidence against the Fellowship, you have brought Justice to those who seek to harm Britannia."
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
