namespace Server.Network.Packets
{
    /// <summary>
    ///     Asks the client for it's version
    /// </summary>
    public sealed class ClientVersionReqPacket : Packet
    {
        public ClientVersionReqPacket()
            : base(0xBD)
        {
            EnsureCapacity(3);
        }
    }
}
