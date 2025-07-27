using Server.Engines.Craft;

namespace Server.Items
{
    [Flipable(0x102E, 0x102F)]
    public class Nails : BaseTool
    {
        [Constructable]
        public Nails()
            : base(0x102E)
        {
        }

        [Constructable]
        public Nails(int uses)
            : base(uses, 0x102C)
        {
        }

        public Nails(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

        public override CraftSystem CraftSystem => DefCarpentry.CraftSystem;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
