using Server.Network;

namespace Server.Gumps
{
    public class QuestRewardGump : Gump
    {
        public string Title;
        public string Description;
        public string Line1;
        public string Line2;

        public QuestRewardGump(Item item, Mobile from)
            : base(100, 100)
        {
            Rectangle2D b = ItemBounds.Table[item.ItemID];

            AddPage(0);
            AddBackground(0, 0, 480, 320, 0x6DB);
            AddSpriteImage(24, 24, 0x474, 60, 60, 108, 108);
            AddImage(15, 15, 0xA9F);
            AddImageTiledButton(22, 22, 0x176F, 0x176F, 0x0, GumpButtonType.Page, 0, item.ItemID, item.Hue, 58 - b.Width / 2 - b.X, 58 - b.Height / 2 - b.Y);            
        }

        public void RenderString(Mobile from)
        {
            AddHtml(150, 15, 320, 22, $"<BASEFONT COLOR=#D5D52A><DIV ALIGN=CENTER>{Title}</DIV>", false, false);
            AddHtml(150, 46, 320, 44, $"<BASEFONT COLOR=#AABFD4><DIV ALIGN=CENTER>{Description}</DIV>", false, false);
            AddHtml(150, 99, 320, 98, $"<BASEFONT COLOR=#DFDFDF>{Line1}", false, false);
            AddHtml(150, 197, 320, 98, $"<BASEFONT COLOR=#DFDFDF>{Line2}", false, false);

            from.CloseGump(typeof(QuestRewardGump));
            from.SendGump(this);
            from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1157722, "its origin", from.NetState); // *Your proficiency in ~1_SKILL~ reveals more about the item*
            from.SendSound(from.Female ? 0x30B : 0x41A);
        }
    }
}
