using Server.Gumps;
using Server.Network;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class VirtueRune : Item
    {
        public VirtueRune()
            : this(virtueList.ElementAt(Utility.Random(virtueList.Count)))
        {
        }

        public VirtueRune(KeyValuePair<string, int> random)
           : this(random.Key, random.Value)
        {
        }

        public VirtueRune(string name, int id)
            : base(id)
        {
            Name = name;
        }

        public VirtueRune(Serial serial)
            : base(serial)
        {
        }

        public static int GetRandomVirtueID()
        {
            return virtueList.ElementAt(Utility.Random(virtueList.Count)).Value;
        }

        public static readonly Dictionary<string, int> virtueList = new Dictionary<string, int> 
        {
            {"Compassion", 0xA51B },
            {"Honesty", 0xA519},
            {"Honor", 0xA51D},
            {"Humility", 0xA521},
            {"Justice", 0xA51F},
            {"Sacrifice", 0xA523},
            {"Spirituality", 0xA517},
            {"Valor", 0xA525}
        };

        public override void OnDoubleClick(Mobile from)
        {
            Gump g = new Gump(100, 100);
            g.AddPage(0);

            g.AddBackground(0, 0, 480, 320, 0x6DB);
            g.AddSpriteImage(24, 24, 0x474, 60, 60, 108, 108);
            g.AddImage(15, 15, 0xA9F);
            g.AddImageTiledButton(22, 22, 0x176F, 0x176F, 0x0, GumpButtonType.Page, 0, ItemID, Hue, 33, 36);
            g.AddHtml(150, 15, 320, 22, string.Format("<BASEFONT COLOR=#D5D52A><DIV ALIGN=CENTER>{0}</DIV>", Name), false, false);
            g.AddHtml(150, 46, 320, 44, "<BASEFONT COLOR=#AABFD4><DIV ALIGN=CENTER>Given to you by Julia as a thanks for restoring protection to the Shrines of Britannia</DIV>", false, false);
            g.AddHtml(150, 99, 320, 98, "<BASEFONT COLOR=#DFDFDF>The artisanship of the heavy stone rune is like nothing you have ever seen. The sigil is carved and adorned with exacting precision.", false, false);
            g.AddHtml(150, 197, 320, 98, "<BASEFONT COLOR=#DFDFDF>Despite the the size and heft of the stone, it is uncharacteristically easy to move.", false, false);

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
