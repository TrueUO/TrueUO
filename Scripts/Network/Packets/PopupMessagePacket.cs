namespace Server.Network.Packets
{
    public enum PMMessage : byte
    {
        CharNoExist = 1,
        CharExists = 2,
        CharInWorld = 5,
        LoginSyncError = 6,
        IdleWarning = 7
    }

    public sealed class PopupMessagePacket : Packet
    {
        public PopupMessagePacket(PMMessage msg)
            : base(0x53, 2)
        {
            m_Stream.Write((byte)msg);
        }
    }
}
