namespace Server.Network.Packets
{
    public sealed class MovementRejPacket : Packet
    {
        public MovementRejPacket(int seq, IEntity m)
            : base(0x21, 8)
        {
            m_Stream.Write((byte)seq);
            m_Stream.Write((short)m.X);
            m_Stream.Write((short)m.Y);
            m_Stream.Write((byte)m.Direction);
            m_Stream.Write((sbyte)m.Z);
        }
    }
}
