namespace Server.Network.Packets
{
    public sealed class SpellbookContentPacket : Packet
    {
        public SpellbookContentPacket(IEntity item, int graphic, int offset, ulong content)
            : base(0xBF)
        {
            EnsureCapacity(23);

            m_Stream.Write((short)0x1B);
            m_Stream.Write((short)0x01);

            m_Stream.Write(item.Serial);
            m_Stream.Write((short)graphic);
            m_Stream.Write((short)offset);

            for (int i = 0; i < 8; ++i)
            {
                m_Stream.Write((byte)(content >> (i * 8)));
            }
        }
    }
}
