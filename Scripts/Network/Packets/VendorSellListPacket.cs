using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Network.Packets
{
    public sealed class VendorSellListPacket : Packet
    {
        public VendorSellListPacket(IEntity shopkeeper, ICollection<SellItemState> sis)
            : base(0x9E)
        {
            EnsureCapacity(256);

            m_Stream.Write(shopkeeper.Serial);

            m_Stream.Write((ushort)sis.Count);

            foreach (SellItemState state in sis)
            {
                m_Stream.Write(state.Item.Serial);
                m_Stream.Write((ushort)state.Item.ItemID);
                m_Stream.Write((ushort)state.Item.Hue);
                m_Stream.Write((ushort)state.Item.Amount);
                m_Stream.Write((ushort)state.Price);

                string name = state.Item.Name;

                if (name == null || (name = name.Trim()).Length <= 0)
                {
                    name = state.Name;
                }

                if (name == null)
                {
                    name = "";
                }

                m_Stream.Write((ushort)name.Length);
                m_Stream.WriteAsciiFixed(name, (ushort)name.Length);
            }
        }
    }
}
