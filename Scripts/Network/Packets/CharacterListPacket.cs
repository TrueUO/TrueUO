using System;
using System.Collections.Generic;
using Server.Accounting;

namespace Server.Network.Packets
{
    public sealed class CharacterListPacket : Packet
    {
        public CharacterListPacket(IAccount a, IReadOnlyList<CityInfo> info, bool IsEnhancedClient)
            : base(0xA9)
        {
            EnsureCapacity(11 + (a.Length * 60) + (info.Count * 89));

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
                if (a[i] != null)
                {
                    m_Stream.WriteAsciiFixed(a[i].Name, 30);
                    m_Stream.Fill(30); // password
                }
                else
                {
                    m_Stream.Fill(60);
                }
            }

            m_Stream.Write((byte)info.Count);

            for (int i = 0; i < info.Count; ++i)
            {
                CityInfo ci = info[i];

                m_Stream.Write((byte)i);
                m_Stream.WriteAsciiFixed(ci.City, 32);
                m_Stream.WriteAsciiFixed(ci.Building, 32);
                m_Stream.Write(ci.X);
                m_Stream.Write(ci.Y);
                m_Stream.Write(ci.Z);
                m_Stream.Write(ci.Map.MapID);
                m_Stream.Write(ci.Description);
                m_Stream.Write(0);
            }

            CharacterListFlags flags = ExpansionInfo.CoreExpansion.CharacterListFlags;

            if (count > 6)
            {
                flags |= CharacterListFlags.SeventhCharacterSlot | CharacterListFlags.SixthCharacterSlot;
            }
            // 7th Character Slot - TODO: Is SixthCharacterSlot Required?
            else if (count == 6)
            {
                flags |= CharacterListFlags.SixthCharacterSlot; // 6th Character Slot
            }
            else if (a.Limit == 1)
            {
                flags |= CharacterListFlags.SlotLimit | CharacterListFlags.OneCharacterSlot; // Limit Characters & One Character
            }

            if (IsEnhancedClient)
            {
                flags |= CharacterListFlags.KR; // Suppport Enhanced Client / KR flag 1 and 2 (0x200 + 0x400)
            }

            flags |= AdditionalFlags;

            Console.WriteLine("{0}: {1} / {2} [{3}]", a.Username, a.Count, a.Limit, flags);

            m_Stream.Write((int)flags);

            m_Stream.Write((short)-1);
        }

        public static CharacterListFlags AdditionalFlags { get; set; }
    }
}
