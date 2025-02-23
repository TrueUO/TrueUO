using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Network.Packets
{
    public sealed class VendorBuyListPacket : Packet
    {
        public VendorBuyListPacket(Mobile vendor, IReadOnlyList<BuyItemState> list)
            : base(0x74)
        {
            EnsureCapacity(256);

            Container BuyPack = vendor.FindItemOnLayer(Layer.ShopBuy) as Container;
            m_Stream.Write(BuyPack == null ? Serial.MinusOne : BuyPack.Serial);

            m_Stream.Write((byte)list.Count);

            for (int i = 0; i < list.Count; ++i)
            {
                BuyItemState bis = list[i];

                m_Stream.Write(bis.Price);

                string desc = bis.Description;

                if (desc == null)
                {
                    desc = "";
                }

                m_Stream.Write((byte)(desc.Length + 1));
                m_Stream.WriteAsciiNull(desc);
            }
        }
    }
}
