using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class EmbroideredPillow : Item
    {
        [Constructable]
        public EmbroideredPillow()
            : base(0x9E1D)
        {
            Name = "an Embroidered Pillow";
            Hue = 2125;
        }

        public EmbroideredPillow(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            Gump g = new Gump(100, 100);
            g.AddPage(0);

            g.AddBackground(0, 0, 480, 320, 0x6DB);
            g.AddSpriteImage(24, 24, 0x474, 60, 60, 108, 108);
            g.AddImage(15, 15, 0xA9F);
            g.AddImageTiledButton(22, 22, 0x176F, 0x176F, 0x0, GumpButtonType.Page, 0, ItemID, Hue, 33, 44);
            g.AddHtml(150, 15, 320, 22, "<BASEFONT COLOR=#D5D52A><DIV ALIGN=CENTER>A Large Heart Embroidered on a Fluffy Pillow</DIV>", false, false);
            g.AddHtml(150, 46, 320, 44, "<BASEFONT COLOR=#AABFD4><DIV ALIGN=CENTER>Given by Iolo for Providing Provisions to the Children of the Poorhouse</DIV>", false, false);
            g.AddHtml(150, 99, 320, 98, "<BASEFONT COLOR=#DFDFDF>Despite the artisan being a mere child, the embroidery wor is impressive.", false, false);
            g.AddHtml(150, 197, 320, 98, "<BASEFONT COLOR=#DFDFDF>Provisions provided to the poorhouse webt a long way to ease the suffering of those in need, a compassionate gift indeed.", false, false);

            from.SendGump(g);

            from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1157722, "its origin", from.NetState); // *Your proficiency in ~1_SKILL~ reveals more about the item*
            from.SendSound(from.Female ? 0x30B : 0x41A);
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
