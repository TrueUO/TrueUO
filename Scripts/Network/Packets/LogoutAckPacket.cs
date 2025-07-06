namespace Server.Network.Packets
{
    public sealed class LogoutAckPacket : Packet
    {
        public LogoutAckPacket()
            : base(0xD1, 2)
        {
            m_Stream.Write((byte)0x01);
        }
    }
}
