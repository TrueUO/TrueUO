namespace Server.Network.Packets
{
    public sealed class GlobalLightLevelPacket : Packet
    {
        private static readonly GlobalLightLevelPacket[] _Cache = new GlobalLightLevelPacket[0x100];

        public static GlobalLightLevelPacket Instantiate(int level)
        {
            byte lvl = (byte)level;
            GlobalLightLevelPacket p = _Cache[lvl];

            if (p == null)
            {
                _Cache[lvl] = p = new GlobalLightLevelPacket(level);
                p.SetStatic();
            }

            return p;
        }

        public GlobalLightLevelPacket(int level)
            : base(0x4F, 2)
        {
            m_Stream.Write((byte)level);
        }
    }
}
