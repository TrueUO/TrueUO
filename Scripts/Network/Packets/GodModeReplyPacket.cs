namespace Server.Network.Packets
{
    public sealed class GodModeReplyPacket : Packet
    {
        public GodModeReplyPacket(bool reply)
            : base(0x2B, 2)
        {
            m_Stream.Write(reply);
        }
    }
}
