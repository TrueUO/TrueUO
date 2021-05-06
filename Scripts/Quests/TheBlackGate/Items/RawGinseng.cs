using Server.Network;
using System.Collections.Generic;

namespace Server.Items
{ // 3386, 319, 4 region
    public class RawGinsengDecoration : Item
    {
        private readonly Dictionary<Mobile, int> list = new Dictionary<Mobile, int>();

        [Constructable]
        public RawGinsengDecoration()
            : base(0x18EA)
        {
            Name = "Raw Ginseng";
            Weight = 0.0;
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            base.OnDoubleClick(from);

            PrivateOverheadMessage(MessageType.Regular, 1150, false, "*You harvest some fresh ginseng. It does not have a long shelf life and will spoil in about 3 days!*", from.NetState);

            if (list.ContainsKey(from))
            {
                if (list[from] >= 2)
                {
                    from.SendLocalizedMessage(1071539); // Sorry. You cannot receive another item at this time.
                    return;
                }

                list[from]++;
            }
            else
            {
                list.Add(from, 0);
            }

            from.AddToBackpack(new RawGinseng());            
        }

        public RawGinsengDecoration(Serial serial)
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

    public class RawGinseng : BaseDecayingItem
    {
        [Constructable]
        public RawGinseng()
            : base(0x18EB)
        {
            Name = "Raw Ginseng";
            Weight = 1.0;
        }

        public override int Lifespan => 4320;

        public RawGinseng(Serial serial)
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
