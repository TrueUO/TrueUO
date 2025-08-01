using Server.Engines.Craft;

namespace Server.Items
{
    public class Skillet : BaseTool
    {
        [Constructable]
        public Skillet()
            : base(0x97F)
        {
        }

        [Constructable]
        public Skillet(int uses)
            : base(uses, 0x97F)
        {
        }

        public Skillet(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

        public override int LabelNumber => 1044567;// skillet
        public override CraftSystem CraftSystem => DefCooking.CraftSystem;

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
