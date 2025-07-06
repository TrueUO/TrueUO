using System.Collections.Generic;

namespace Server.Network
{
	public enum MessageType
	{
		Regular = 0x00,
		System = 0x01,
		Emote = 0x02,
		Label = 0x06,
		Focus = 0x07,
		Whisper = 0x08,
		Yell = 0x09,
		Spell = 0x0A,

		Guild = 0x0D,
		Alliance = 0x0E,
		Command = 0x0F,

		Encoded = 0xC0
	}

	public static class PacketHandlers
	{
		private static readonly PacketHandler[] m_Handlers;

		private static readonly PacketHandler[] m_ExtendedHandlersLow;
		private static readonly Dictionary<int, PacketHandler> m_ExtendedHandlersHigh;

		private static readonly EncodedPacketHandler[] m_EncodedHandlersLow;
		private static readonly Dictionary<int, EncodedPacketHandler> m_EncodedHandlersHigh;

		static PacketHandlers()
		{
			m_Handlers = new PacketHandler[0x100];

			m_ExtendedHandlersLow = new PacketHandler[0x100];
			m_ExtendedHandlersHigh = new Dictionary<int, PacketHandler>();

			m_EncodedHandlersLow = new EncodedPacketHandler[0x100];
			m_EncodedHandlersHigh = new Dictionary<int, EncodedPacketHandler>();
		}

		public static void Register(int packetID, int length, bool ingame, OnPacketReceive onReceive)
		{
			m_Handlers[packetID] = new PacketHandler(packetID, length, ingame, onReceive);
		}

		public static PacketHandler GetHandler(int packetID)
		{
			return m_Handlers[packetID];
		}

		public static void RegisterExtended(int packetID, bool ingame, OnPacketReceive onReceive)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				m_ExtendedHandlersLow[packetID] = new PacketHandler(packetID, 0, ingame, onReceive);
			}
			else
			{
				m_ExtendedHandlersHigh[packetID] = new PacketHandler(packetID, 0, ingame, onReceive);
			}
		}

		public static PacketHandler GetExtendedHandler(int packetID)
        {
            if (packetID >= 0 && packetID < 0x100)
			{
				return m_ExtendedHandlersLow[packetID];
			}

            m_ExtendedHandlersHigh.TryGetValue(packetID, out PacketHandler handler);
            return handler;
        }

        public static void RegisterEncoded(int packetID, bool ingame, OnEncodedPacketReceive onReceive)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				m_EncodedHandlersLow[packetID] = new EncodedPacketHandler(packetID, ingame, onReceive);
			}
			else
			{
				m_EncodedHandlersHigh[packetID] = new EncodedPacketHandler(packetID, ingame, onReceive);
			}
		}

		public static EncodedPacketHandler GetEncodedHandler(int packetID)
        {
            if (packetID >= 0 && packetID < 0x100)
			{
				return m_EncodedHandlersLow[packetID];
			}

            m_EncodedHandlersHigh.TryGetValue(packetID, out EncodedPacketHandler handler);
            return handler;
        }

        public static void RegisterThrottler(int packetID, ThrottlePacketCallback t)
		{
			PacketHandler ph = GetHandler(packetID);

			if (ph != null)
			{
				ph.ThrottleCallback = t;
			}
		}
	}
}
