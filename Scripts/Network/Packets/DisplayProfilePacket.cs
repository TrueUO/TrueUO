namespace Server.Network.Packets
{
    public sealed class DisplayProfilePacket : Packet
    {
        public DisplayProfilePacket(bool realSerial, IEntity m, string header, string body, string footer)
            : base(0xB8)
        {
            if (header == null)
            {
                header = "";
            }

            if (body == null)
            {
                body = "";
            }

            if (footer == null)
            {
                footer = "";
            }

            EnsureCapacity(12 + header.Length + (footer.Length * 2) + (body.Length * 2));

            m_Stream.Write(realSerial ? m.Serial : Serial.Zero);
            m_Stream.WriteAsciiNull(header);
            m_Stream.WriteBigUniNull(footer);
            m_Stream.WriteBigUniNull(body);
        }
    }
}
