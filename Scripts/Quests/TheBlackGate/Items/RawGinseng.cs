using Server.Network;
using System.Collections.Generic;

namespace Server.Items
{
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

        public RawGinsengDecoration(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this, 3))
            {
                from.SendLocalizedMessage(1019045); // I can't reach that.
                return;
            }

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
        public override int Lifespan => 259200;
        public override bool UseSeconds => false;

        [Constructable]
        public RawGinseng()
            : base(0x18EB)
        {
            Name = "Raw Ginseng";
        }

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
