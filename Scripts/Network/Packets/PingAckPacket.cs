namespace Server.Network.Packets
{
    public sealed class PingAckPacket : Packet
    {
        private static readonly PingAckPacket[] _Cache = new PingAckPacket[0x100];

        public static PingAckPacket Instantiate(byte ping)
        {
            PingAckPacket p = _Cache[ping];

            if (p == null)
            {
                _Cache[ping] = p = new PingAckPacket(ping);
                p.SetStatic();
            }

            return p;
        }

        private PingAckPacket(byte ping)
            : base(0x73, 2)
        {
            m_Stream.Write(ping);
        }
    }
}
