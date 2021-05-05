using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class VialOfBlood : Item
    {
        [Constructable]
        public VialOfBlood()
            : base(0xE24)
        {
            Name = "a vial of blood";
            Hue = 44;
        }

        public override void OnDoubleClick(Mobile from)
        {
            Gump g = new Gump(100, 100);
            g.AddPage(0);

            g.AddBackground(0, 0, 480, 320, 0x6DB);
            g.AddSpriteImage(24, 24, 0x474, 60, 60, 108, 108);
            g.AddImage(15, 15, 0xA9F);
            g.AddImageTiledButton(22, 22, 0x176F, 0x176F, 0x0, GumpButtonType.Page, 0, ItemID, Hue, 33, 44);
            g.AddHtml(150, 15, 320, 22, "<BASEFONT COLOR=#D5D52A><DIV ALIGN=CENTER>A Vial of Julia's Blood</DIV>", false, false);
            g.AddHtml(150, 46, 320, 44, "<BASEFONT COLOR=#AABFD4><DIV ALIGN=CENTER>Recovered from the Site of her Death near Minoc</DIV>", false, false);
            g.AddHtml(150, 99, 320, 98, "<BASEFONT COLOR=#DFDFDF>Julia was killed some time back by the vengeful troll G'thunk. The ground is still wet with her Sacrifice.", false, false);
            g.AddHtml(150, 197, 320, 98, "<BASEFONT COLOR=#DFDFDF>Her blood stands as a reminder of the sacrifice you must ultimately make to save virtue and rebuild the shrines...", false, false);

            from.SendGump(g);

            from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1157722, "its origin", from.NetState); // *Your proficiency in ~1_SKILL~ reveals more about the item*
            from.SendSound(from.Female ? 0x30B : 0x41A);
        }

        public VialOfBlood(Serial serial)
            : base(serial)
        {
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
