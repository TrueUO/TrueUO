namespace Server.Network.Packets
{
    public sealed class EndVendorSellPacket : Packet
    {
        public EndVendorSellPacket(IEntity vendor)
            : base(0x3B, 8)
        {
            m_Stream.Write((ushort)8); //length
            m_Stream.Write(vendor.Serial);
            m_Stream.Write((byte)0);
        }
    }
}
