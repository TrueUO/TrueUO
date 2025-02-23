namespace Server.Network.Packets
{
    public sealed class LoginCompletePacket : Packet
    {
        public static readonly Packet Instance = SetStatic(new LoginCompletePacket());

        public LoginCompletePacket()
            : base(0x55, 1)
        { }
    }
}
