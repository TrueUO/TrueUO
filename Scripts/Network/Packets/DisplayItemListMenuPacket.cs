using Server.Menus;

namespace Server.Network.Packets
{
    public sealed class DisplayItemListMenuPacket : Packet
    {
        public DisplayItemListMenuPacket(ItemListMenu menu)
            : base(0x7C)
        {
            EnsureCapacity(256);

            m_Stream.Write(((IMenu)menu).Serial);
            m_Stream.Write((short)0);

            string question = menu.Question;

            if (question == null)
            {
                m_Stream.Write((byte)0);
            }
            else
            {
                int questionLength = question.Length;
                m_Stream.Write((byte)questionLength);
                m_Stream.WriteAsciiFixed(question, questionLength);
            }

            ItemListEntry[] entries = menu.Entries;

            int entriesLength = (byte)entries.Length;

            m_Stream.Write((byte)entriesLength);

            for (int i = 0; i < entriesLength; ++i)
            {
                ItemListEntry e = entries[i];

                m_Stream.Write((ushort)e.ItemID);
                m_Stream.Write((short)e.Hue);

                string name = e.Name;

                if (name == null)
                {
                    m_Stream.Write((byte)0);
                }
                else
                {
                    int nameLength = name.Length;
                    m_Stream.Write((byte)nameLength);
                    m_Stream.WriteAsciiFixed(name, nameLength);
                }
            }
        }
    }
}
