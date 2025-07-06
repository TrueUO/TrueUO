using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Network.Packets
{
    public sealed class VendorBuyContentPacket : Packet
    {
        public VendorBuyContentPacket(IReadOnlyList<BuyItemState> list)
            : base(0x3C)
        {
            EnsureCapacity(list.Count * 20 + 5);

            m_Stream.Write((short)list.Count);

            //The client sorts these by their X/Y value.
            //OSI sends these in wierd order.  X/Y highest to lowest and serial loest to highest
            //These are already sorted by serial (done by the vendor class) but we have to send them by x/y
            //(the x74 packet is sent in 'correct' order.)
            for (int i = list.Count - 1; i >= 0; --i)
            {
                BuyItemState bis = list[i];

                m_Stream.Write(bis.MySerial);
                m_Stream.Write((ushort)bis.ItemID);
                m_Stream.Write((byte)0); //itemid offset
                m_Stream.Write((ushort)bis.Amount);
                m_Stream.Write((short)(i + 1)); //x
                m_Stream.Write((short)1); //y
                m_Stream.Write((byte)0); // Grid Location?
                m_Stream.Write(bis.ContainerSerial);
                m_Stream.Write((ushort)bis.Hue);
            }
        }
    }
}
