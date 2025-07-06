using System;

namespace Server.Network.Packets
{
    public sealed class TripTimeResponsePacket : Packet
    {
        public TripTimeResponsePacket(int unk)
            : base(0xC9, 6)
        {
            m_Stream.Write((byte)unk);
            m_Stream.Write(Environment.TickCount);
        }
    }
}
