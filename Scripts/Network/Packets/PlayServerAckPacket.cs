namespace Server.Network.Packets
{
    public sealed class PlayServerAckPacket : Packet
    {
        public PlayServerAckPacket(ServerInfo si, uint auth)
            : base(0x8C, 11)
        {
            int addr = Utility.GetAddressValue(si.Address.Address);

            m_Stream.Write((byte)addr);
            m_Stream.Write((byte)(addr >> 8));
            m_Stream.Write((byte)(addr >> 16));
            m_Stream.Write((byte)(addr >> 24));

            m_Stream.Write((short)si.Address.Port);
            m_Stream.Write(auth);
        }
    }
}
