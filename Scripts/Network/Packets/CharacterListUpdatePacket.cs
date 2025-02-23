using System;
using Server.Accounting;

namespace Server.Network.Packets
{
    public sealed class CharacterListUpdatePacket : Packet
    {
        public CharacterListUpdatePacket(IAccount a)
            : base(0x86)
        {
            EnsureCapacity(4 + (a.Length * 60));

            int highSlot = -1;

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != null)
                {
                    highSlot = i;
                }
            }

            int count = Math.Max(Math.Max(highSlot + 1, a.Limit), 5);

            m_Stream.Write((byte)count);

            for (int i = 0; i < count; ++i)
            {
                Mobile m = a[i];

                if (m != null)
                {
                    m_Stream.WriteAsciiFixed(m.Name, 30);
                    m_Stream.Fill(30); // password
                }
                else
                {
                    m_Stream.Fill(60);
                }
            }
        }
    }
}
