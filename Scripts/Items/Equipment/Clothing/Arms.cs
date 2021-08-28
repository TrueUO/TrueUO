namespace Server.Items
{
    public class GargishClothArms : BaseClothing
    {
        [Constructable]
        public GargishClothArms()
            : this(0)
        {
        }

        [Constructable]
        public GargishClothArms(int hue)
            : base(0x0404, Layer.Arms, hue)
        {
            Weight = 2.0;
        }

        public GargishClothArms(Serial serial)
            : base(serial)
        {
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            if (parent is Mobile mobile)
            {
                if (mobile.Female)
                {
                    ItemID = 0x0403;
                }
                else
                {
                    ItemID = 0x0404;
                }
            }
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
