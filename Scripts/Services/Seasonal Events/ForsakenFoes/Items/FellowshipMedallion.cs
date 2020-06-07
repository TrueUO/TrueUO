using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class FellowshipMedallion : Item
    {
        public override int LabelNumber => 1159248;  // Fellowship Medallion

        [Constructable]
        public FellowshipMedallion()
            : base(0xA429) // 0xa42a gargoyle id
        {
            Weight = 1.0;
            Layer = Layer.Neck;
        }

        public FellowshipMedallion(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.HasGump(typeof(FellowshipMedallionGump)))
            {
                from.SendGump(new FellowshipMedallionGump(this));
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1157722, "its origin"); // *Your proficiency in ~1_SKILL~ reveals more about the item*
                from.PlaySound(1050);
            }
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

    public class FellowshipMedallionGump : Gump
    {
        public FellowshipMedallionGump(Item item)
            : base(100, 100)
        {
            AddPage(0);

            AddBackground(0, 0, 454, 400, 0x24A4);
            AddItem(75, 120, item.ItemID, item.Hue);
            AddHtmlLocalized(177, 50, 250, 18, 1114513, "#1159248", 0x3442, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            AddHtmlLocalized(177, 77, 250, 36, 1114513, "#1159033", 0x3442, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            AddHtmlLocalized(177, 122, 250, 228, 1159247, 0xC63, true, true); // This is an otherwise unassuming metal medallion in the shape of a triangle.  The letters T, W, and U are engraved on it. It is almost immediately recognizable as a sign of the Fellowship.
        }
    }

    public class GargishFellowshipMedallion : GargishNecklace
    {
        public override int LabelNumber => 1159248;  // Fellowship Medallion

        public override bool IsArtifact => true;

        [Constructable]
        public GargishFellowshipMedallion()
            : base(0xA42A)
        {
            Weight = 1.0;
            Layer = Layer.Neck;
        }

        public GargishFellowshipMedallion(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.HasGump(typeof(FellowshipMedallionGump)))
            {
                from.SendGump(new FellowshipMedallionGump(this));
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1157722, "its origin"); // *Your proficiency in ~1_SKILL~ reveals more about the item*
                from.PlaySound(1050);
            }
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
