using System;

namespace Server.Network.Packets
{
    public sealed class UTripTimeResponsePacket : Packet
    {
        public UTripTimeResponsePacket(int unk)
            : base(0xCA, 6)
        {
            m_Stream.Write((byte)unk);
            m_Stream.Write(Environment.TickCount);
        }
    }
}
