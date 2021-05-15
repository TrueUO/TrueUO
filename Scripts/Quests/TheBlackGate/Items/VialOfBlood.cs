using Server.Gumps;

namespace Server.Items
{
    public class VialOfBlood : Item
    {
        public override string DefaultName => "a vial of blood";

        [Constructable]
        public VialOfBlood()
            : base(0xE24)
        {
            Hue = 44;
        }

        public VialOfBlood(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "A Vial of Julia's Blood",
                Description = "Recovered from the Site of her Death near Minoc",
                Line1 = "Julia was killed some time back by the vengeful troll G'thunk. The ground is still wet with her Sacrifice.",
                Line2 = "Her blood stands as a reminder of the sacrifice you must ultimately make to save virtue and rebuild the shrines..."
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
