
using Server.Network;

namespace Server.Items
{
    public class SandalwoodBox : Container
    {
        public override int LabelNumber => 1159355;  // Sandalwood Box

        [Constructable]
        public SandalwoodBox()
            : base(0x9AA)
        {
            Movable = false;
            Hue = 1111;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendLocalizedMessage(1005420); // You cannot use this.
            from.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1154226); // *It's an unassuming strong box. You examine the lock more closely and determine there is no way to pick it. You'll need to find a key.*

            base.OnDoubleClick(from);
        }

        public SandalwoodBox(Serial serial)
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
            int version = reader.ReadInt();
        }
    }
}
