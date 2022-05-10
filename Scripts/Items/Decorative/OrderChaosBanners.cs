namespace Server.Items
{
    [Flipable(0x42CD, 0x42CE)]
    public class OrderBanner : Item
    {
        [Constructable]
        public OrderBanner()
            : base(0x42CD)
        {
            Name = "Order Banner";
            Weight = 1.0;
        }

        public OrderBanner(Serial serial)
            : base(serial)
        {
        }

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

    [Flipable(0x42CB, 0x42CC)]
    public class ChaosBanner : Item
    {
        [Constructable]
        public ChaosBanner()
            : base(0x42CB)
        {
            Name = "Chaos Banner";
            Weight = 1.0;
        }

        public ChaosBanner(Serial serial)
            : base(serial)
        {
        }

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
