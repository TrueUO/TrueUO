using System;

namespace Server.Network.Packets
{
    public sealed class CurrentTimePacket : Packet
    {
        public CurrentTimePacket()
            : base(0x5B, 4)
        {
            DateTime now = DateTime.UtcNow;

            m_Stream.Write((byte)now.Hour);
            m_Stream.Write((byte)now.Minute);
            m_Stream.Write((byte)now.Second);
        }
    }
}
