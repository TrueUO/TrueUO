namespace Server.Network.Packets
{
    public sealed class DisplayPaperdollPacket : Packet
    {
        public DisplayPaperdollPacket(Mobile m, string text, bool canLift)
            : base(0x88, 66)
        {
            byte flags = 0x00;

            if (m.Warmode)
            {
                flags |= 0x01;
            }

            if (canLift)
            {
                flags |= 0x02;
            }

            m_Stream.Write(m.Serial);
            m_Stream.WriteAsciiFixed(text, 60);
            m_Stream.Write(flags);
        }
    }
}
