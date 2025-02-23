using Server.Misc;

namespace Server.Network.Packets
{
    public sealed class PopupMessagePacket : Packet
    {
        public PopupMessagePacket(PMMessage msg)
            : base(0x53, 2)
        {
            m_Stream.Write((byte)msg);
        }
    }
}
