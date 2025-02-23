namespace Server.Network.Packets
{
    public sealed class WeatherPacket : Packet
    {
        public WeatherPacket(int v1, int v2, int v3)
            : base(0x65, 4)
        {
            m_Stream.Write((byte)v1);
            m_Stream.Write((byte)v2);
            m_Stream.Write((byte)v3);
        }
    }
}
