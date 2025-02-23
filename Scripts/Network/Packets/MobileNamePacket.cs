namespace Server.Network.Packets
{
    public sealed class MobileNamePacket : Packet
    {
        public MobileNamePacket(IEntity m)
            : base(0x98)
        {
            string name = m.Name;

            if (name == null)
            {
                name = "";
            }

            EnsureCapacity(37);

            m_Stream.Write(m.Serial);
            m_Stream.WriteAsciiFixed(name, 30);
        }
    }
}
