using Server.Gumps;

namespace Server.Items
{
    public class EmbroideredPillow : Item
    {
        public override string DefaultName => "an Embroidered Pillow";

        [Constructable]
        public EmbroideredPillow()
            : base(0x9E1D)
        {
            Hue = 2125;
        }

        public EmbroideredPillow(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "A Large Heart Embroidered on a Fluffy Pillow",
                Description = "Given by Iolo for Providing Provisions to the Children of the Poorhouse",
                Line1 = "Despite the artisan being a mere child, the embroidery wor is impressive.",
                Line2 = "Provisions provided to the poorhouse webt a long way to ease the suffering of those in need, a compassionate gift indeed."
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
