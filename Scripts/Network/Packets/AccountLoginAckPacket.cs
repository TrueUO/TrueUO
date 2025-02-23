using System.Collections.Generic;

namespace Server.Network.Packets
{
    public sealed class AccountLoginAckPacket : Packet
    {
        public AccountLoginAckPacket(IReadOnlyList<ServerInfo> info)
            : base(0xA8)
        {
            EnsureCapacity(6 + info.Count * 40);

            m_Stream.Write((byte)0x5D); // Unknown

            m_Stream.Write((ushort)info.Count);

            for (int i = 0; i < info.Count; ++i)
            {
                ServerInfo si = info[i];

                m_Stream.Write((ushort)i);
                m_Stream.WriteAsciiFixed(si.Name, 32);
                m_Stream.Write((byte)si.FullPercent);
                m_Stream.Write((sbyte)si.TimeZone);
                m_Stream.Write(Utility.GetAddressValue(si.Address.Address));
            }
        }
    }
}
