using Server.Engines.Craft;

namespace Server.Items
{
    [Flipable(0x1030, 0x1031)]
    public class JointingPlane : BaseTool
    {
		public override CraftSystem CraftSystem => DefCarpentry.CraftSystem;
		
        [Constructable]
        public JointingPlane()
            : base(0x1030)
        {
        }

        [Constructable]
        public JointingPlane(int uses)
            : base(uses, 0x1030)
        {
        }

        public JointingPlane(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

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
