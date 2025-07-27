using Server.Engines.Craft;

namespace Server.Items
{
    [Flipable(0x102C, 0x102D)]
    public class MouldingPlane : BaseTool
    {
        [Constructable]
        public MouldingPlane()
            : base(0x102C)
        {
        }

        [Constructable]
        public MouldingPlane(int uses)
            : base(uses, 0x102C)
        {
        }

        public MouldingPlane(Serial serial)
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
