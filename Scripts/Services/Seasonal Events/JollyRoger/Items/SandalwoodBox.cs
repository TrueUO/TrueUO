
using System.Collections.Generic;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class SandalwoodBox : Item
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
