namespace Server.Network.Packets
{
    public sealed class DisplaySpellbookPacket : Packet
    {
        public DisplaySpellbookPacket(IEntity book)
            : base(0x24, 9)
        {
            m_Stream.Write(book.Serial);
            m_Stream.Write((short)-1);
            m_Stream.Write((short)0x7D);
        }
    }
}
