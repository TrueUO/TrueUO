namespace Server.Network.Packets
{
    public sealed class LoginConfirmPacket : Packet
    {
        public LoginConfirmPacket(Mobile m)
            : base(0x1B, 37)
        {
            m_Stream.Write(m.Serial);
            m_Stream.Write(0);
            m_Stream.Write((short)m.Body);
            m_Stream.Write((short)m.X);
            m_Stream.Write((short)m.Y);
            m_Stream.Write((short)m.Z);
            m_Stream.Write((byte)m.Direction);
            m_Stream.Write((byte)0);
            m_Stream.Write(-1);

            Map map = m.Map;

            if (map == null || map == Map.Internal)
            {
                map = m.LogoutMap;
            }

            m_Stream.Write((short)0);
            m_Stream.Write((short)0);
            m_Stream.Write((short)(map == null ? 6144 : map.Width));
            m_Stream.Write((short)(map == null ? 4096 : map.Height));

            m_Stream.Fill();
        }
    }
}
