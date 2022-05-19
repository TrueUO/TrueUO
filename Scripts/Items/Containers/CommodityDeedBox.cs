namespace Server.Items
{
    [Flipable(0x9AA, 0xE7D)]
    public class CommodityDeedBox : BaseContainer
    {
        [Constructable]
        public CommodityDeedBox()
            : base(0x9AA)
        {
            Hue = 0x47;
            Weight = 4.0;
        }

        public CommodityDeedBox(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1080523;// Commodity Deed Box

        public override int DefaultGumpID => 0x43;

        public static CommodityDeedBox Find(Item deed)
        {
            Item parent = deed;

            while (parent != null && !(parent is CommodityDeedBox))
            {
                parent = parent.Parent as Item;
            }

            return parent as CommodityDeedBox;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
