namespace Server.Network.Packets
{
    public sealed class SwingPacket : Packet
    {
        public SwingPacket(int flag, IEntity attacker, IEntity defender)
            : base(0x2F, 10)
        {
            m_Stream.Write((byte)flag);
            m_Stream.Write(attacker.Serial);
            m_Stream.Write(defender.Serial);
        }
    }
}
