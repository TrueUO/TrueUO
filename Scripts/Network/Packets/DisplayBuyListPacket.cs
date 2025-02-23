namespace Server.Network.Packets
{
    public sealed class DisplayBuyListPacket : Packet
    {
        public DisplayBuyListPacket(IEntity vendor)
            : base(0x24, 9)
        {
            m_Stream.Write(vendor.Serial);
            m_Stream.Write((short)0x30); // buy window id?
            m_Stream.Write((short)0x00);
        }
    }
}
