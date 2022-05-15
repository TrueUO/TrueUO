using Server.Gumps;

namespace Server.Items
{
    public class StasisChamberPowerCore : Item
    {
        public override int LabelNumber => 1156623;

        [Constructable]
        public StasisChamberPowerCore()
            : base(40155)
        {
        }

        public StasisChamberPowerCore(Serial serial) : base(serial)
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

    public class UnabridgedAtlasOfEodon : Item
    {
        public override int LabelNumber => 1156721;  // Unabridged Atlas of Eodon

        [Constructable]
        public UnabridgedAtlasOfEodon()
            : base(7185)
        {
            Hue = 2007;
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (IsChildOf(m.Backpack))
            {
                Gump g = new Gump(100, 50);
                g.AddImage(0, 0, 30236);
                g.AddHtmlLocalized(115, 35, 350, 600, 1156723, 1, false, true);

                m.SendGump(g);
            }
        }

        public UnabridgedAtlasOfEodon(Serial serial)
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
