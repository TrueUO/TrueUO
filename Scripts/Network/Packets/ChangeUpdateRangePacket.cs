namespace Server.Network.Packets
{
    public sealed class ChangeUpdateRangePacket : Packet
    {
        private static readonly ChangeUpdateRangePacket[] _Cache = new ChangeUpdateRangePacket[0x100];

        public static ChangeUpdateRangePacket Instantiate(int range)
        {
            byte idx = (byte)range;
            ChangeUpdateRangePacket p = _Cache[idx];

            if (p == null)
            {
                _Cache[idx] = p = new ChangeUpdateRangePacket(range);
                p.SetStatic();
            }

            return p;
        }

        public ChangeUpdateRangePacket(int range)
            : base(0xC8, 2)
        {
            m_Stream.Write((byte)range);
        }
    }
}
