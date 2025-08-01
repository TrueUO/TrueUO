using Server.Engines.Harvest;

namespace Server.Items
{
    public class Shovel : BaseHarvestTool
    {
        public override HarvestSystem HarvestSystem => Mining.System;

        [Constructable]
        public Shovel()
            : this(50)
        {
        }

        [Constructable]
        public Shovel(int uses)
            : base(uses, 0xF39)
        {
        }

        public Shovel(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 5.0;

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
