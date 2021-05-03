using System;
using Server;

namespace Server.Items
{
    [TypeAlias("Server.Items.GoldShield")]
    public class ShieldOfRecognition : OrderShield
    {
        public override int LabelNumber { get { return 1075851; } } // Shield of Recognition

        [Constructable]
        public ShieldOfRecognition()
        {
            ItemID = 0x1BC5;
            Hue = 50;
            ArmorAttributes.LowerStatReq = 80;
        }

        public ShieldOfRecognition(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
