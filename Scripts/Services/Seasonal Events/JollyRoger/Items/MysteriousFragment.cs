using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class MysteriousFragment : Item
    {
        public override int LabelNumber => 1159025;  // mysterious fragment

        [Constructable]
        public MysteriousFragment()
            : base(0x1F13)
        {
            Hue = Utility.RandomList(1918, 1910, 1916, 2500, 1912, 1914, 1920, 1922);
        }
        public override void OnDoubleClick(Mobile from)
        {
            Gump g = new Gump(100, 100);
            g.AddBackground(0, 0, 454, 400, 0x24A4);
            g.AddItem(75, 120, ItemID, Hue);
            g.AddHtmlLocalized(177, 50, 250, 18, 1114513, "#1159025", 0x3442, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            g.AddHtmlLocalized(177, 77, 250, 36, 1114513, "#1159026", 0x3442, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            g.AddHtmlLocalized(177, 122, 250, 228, 1159027, 0xC63, true, true); // The item appears to be the jagged fragment of a larger piece.  While you cannot quite discern the origins or purpose of such a piece, it is no doubt fascinating.  The color shimmers with a strange brilliance that you feel you have seen before, yet cannot quite place.  Whatever created this fragment did so with awesome force.


            from.SendGump(g);

            from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1157722, "its origin", from.NetState); // *Your proficiency in ~1_SKILL~ reveals more about the item*
            from.SendSound(from.Female ? 0x30B : 0x41A);
        }

        public MysteriousFragment(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
