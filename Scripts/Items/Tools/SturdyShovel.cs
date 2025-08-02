using Server.Engines.Harvest;

namespace Server.Items
{
    public class SturdyShovel : BaseHarvestTool
    {
        [Constructable]
        public SturdyShovel()
            : this(180)
        {
        }

        [Constructable]
        public SturdyShovel(int uses)
            : base(uses, 0xF39)
        {
            Hue = 0x973;
        }

        public SturdyShovel(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 5.0;

        public override int LabelNumber => 1045125;// sturdy shovel
        public override HarvestSystem HarvestSystem => Mining.System;

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
