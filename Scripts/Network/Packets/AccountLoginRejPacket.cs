namespace Server.Network.Packets
{
    public sealed class AccountLoginRejPacket : Packet
    {
        public AccountLoginRejPacket(ALRReason reason)
            : base(0x82, 2)
        {
            m_Stream.Write((byte)reason);
        }
    }
}
