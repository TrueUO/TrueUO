namespace Server.Network.Packets
{
    public sealed class PersonalLightLevelPacket : Packet
    {
        public PersonalLightLevelPacket(Mobile m)
            : this(m, m.LightLevel)
        { }

        public PersonalLightLevelPacket(IEntity m, int level)
            : base(0x4E, 6)
        {
            m_Stream.Write(m.Serial);
            m_Stream.Write((byte)level);
        }
    }
}
