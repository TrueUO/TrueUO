namespace Server.Network.Packets
{
    public sealed class HeritagePacket : Packet
    {
        public static readonly Packet Close = SetStatic(new HeritagePacket(false, 0xFF));

        public HeritagePacket(bool female, short type)
            : base(0xBF)
        {
            EnsureCapacity(7);

            m_Stream.Write((short)0x2A);
            m_Stream.Write((byte)(female ? 1 : 0));
            m_Stream.Write((byte)type);
        }
    }
}
