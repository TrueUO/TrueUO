using Server.Gumps;

namespace Server.Items
{
    public class AnkhNecklace : BaseNecklace
    {
        public override string DefaultName => "Ankh Necklace";

        [Constructable]
        public AnkhNecklace()
            : base(0x3BB5)
        {
            Hue = 2498;
        }

        public AnkhNecklace(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "An Ankh Necklace",
                Description = "Recovered from a Fallen Ranger in Hythloth",
                Line1 = "You recovered the spirit of the fallen ranger in Hytloth for Shamino.",
                Line2 = "You have proven your own Spirituality in performing this solemn deed, and for that Shamino has given you the necklace."
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
