using Server.Network;
using System.Collections.Generic;

namespace Server.Items
{
    public class SpilledBlood : Item
    {
        public override string DefaultName => "Spilled Blood From Julia's Final Battle";

        private readonly Dictionary<Mobile, int> list = new Dictionary<Mobile, int>();

        [Constructable]
        public SpilledBlood()
            : base(0x122A)
        {
            Weight = 0.0;
            Movable = false;
        }

        public SpilledBlood(Serial serial)
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

            PrivateOverheadMessage(MessageType.Regular, 1150, false, "*The blood is still wet despite the event happening some time ago...you collect a vial and tuck it away*", from.NetState);

            if (list.ContainsKey(from))
            {
                if (list[from] >= 1)
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

            from.AddToBackpack(new VialOfBlood());
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
