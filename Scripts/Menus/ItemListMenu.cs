using Server.Network;
using Server.Network.Packets;

namespace Server.Menus
{
	public class ItemListEntry
	{
		private readonly string _Name;
		private readonly int _ItemID;
		private readonly int _Hue;

		public string Name => _Name;
		public int ItemID => _ItemID;
		public int Hue => _Hue;

		public ItemListEntry(string name, int itemID)
			: this(name, itemID, 0)
		{ }

		public ItemListEntry(string name, int itemID, int hue)
		{
			_Name = name;
			_ItemID = itemID;
			_Hue = hue;
		}
	}

	public class ItemListMenu : IMenu
	{
		private readonly string m_Question;
		private ItemListEntry[] m_Entries;

		private readonly int m_Serial;
		private static int m_NextSerial;

		int IMenu.Serial => m_Serial;

		int IMenu.EntryLength => m_Entries.Length;

		public string Question => m_Question;

		public ItemListEntry[] Entries { get => m_Entries; set => m_Entries = value; }

		public ItemListMenu(string question, ItemListEntry[] entries)
		{
			m_Question = question;
			m_Entries = entries;

			do
			{
				m_Serial = m_NextSerial++;
				m_Serial &= 0x7FFFFFFF;
			}
			while (m_Serial == 0);

			m_Serial = (int)((uint)m_Serial | 0x80000000);
		}

		public virtual void OnCancel(NetState state)
		{ }

		public virtual void OnResponse(NetState state, int index)
		{ }

		public void SendTo(NetState state)
		{
			state.AddMenu(this);
			state.Send(new DisplayItemListMenuPacket(this));
		}
	}
}
