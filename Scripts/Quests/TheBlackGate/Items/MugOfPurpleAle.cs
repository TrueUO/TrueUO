using Server.Gumps;
using Server.Network;

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
            Gump g = new Gump(100, 100);
            g.AddPage(0);

            g.AddBackground(0, 0, 480, 320, 0x6DB);
            g.AddSpriteImage(24, 24, 0x474, 60, 60, 108, 108);
            g.AddImage(15, 15, 0xA9F);
            g.AddImageTiledButton(22, 22, 0x176F, 0x176F, 0x0, GumpButtonType.Page, 0, ItemID, Hue, 33, 38);
            g.AddHtml(150, 15, 320, 22, "<BASEFONT COLOR=#D5D52A><DIV ALIGN=CENTER>A Frothy Mug of Purple Ale</DIV>", false, false);
            g.AddHtml(150, 46, 320, 44, "<BASEFONT COLOR=#AABFD4><DIV ALIGN=CENTER>Shared by Dupre</DIV>", false, false);
            g.AddHtml(150, 99, 320, 98, "<BASEFONT COLOR=#DFDFDF>A specialty of the Keg and Anchor, Dupre is known to enjoy a mug or two of Purple Ale.", false, false);
            g.AddHtml(150, 197, 320, 98, "<BASEFONT COLOR=#DFDFDF>In proving your Honor to Dupre in battle against the daemon Arcadion, he has shared this mug of ale with you.", false, false);

            from.CloseGump(typeof(Gump));
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
