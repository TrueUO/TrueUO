using Server.Engines.Craft;

namespace Server.Items
{
    public class Hammer : BaseTool
    {
        [Constructable]
        public Hammer()
            : base(0x102A)
        {
        }

        [Constructable]
        public Hammer(int uses)
            : base(uses, 0x102A)
        {
        }

        public Hammer(Serial serial)
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
