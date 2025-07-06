using System;

namespace Server.Items
{
    public class Blood : Item
    {
        [Constructable]
        public Blood() // Different blood graphics.
            : this(Utility.RandomList(0x1645, 0x122A, 0x122B, 0x122C, 0x122D, 0x122E, 0x122F))
        {
        }

        [Constructable]
        public Blood(int itemId)
            : base(itemId)
        {
            Movable = false;

            // Delete after 3 seconds.
            Timer.DelayCall(TimeSpan.FromSeconds(3.0), Delete);
        }

        public Blood(Serial serial)
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

            Delete();
        }
    }
}
