using System;
using System.Collections.Generic;
using System.IO;
using Server.Accounting;
using Server.ContextMenus;
using Server.HuePickers;
using Server.Mobiles;
using Server.Prompts;
using CV = Server.ClientVersion;

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

			Register(0x91, 65, false, GameLogin);
			Register(0x95, 9, true, HuePickerResponse);
			Register(0x96, 0, true, GameCentralMoniter);
			Register(0x98, 0, true, MobileNameRequest);
			Register(0x9D, 51, true, GMSingle);
			Register(0x9F, 0, true, VendorSellReply);
			Register(0xA0, 3, false, PlayServer);
			Register(0xA4, 149, false, SystemInfo);
			Register(0xA7, 4, true, RequestScrollWindow);
			Register(0xB6, 9, true, ObjectHelpRequest);
			Register(0xBB, 9, false, AccountID);
			Register(0xBD, 0, true, ClientVersion);
			Register(0xBE, 0, true, AssistVersion);
			Register(0xBF, 0, true, ExtendedCommand);
			Register(0xC2, 0, true, UnicodePromptResponse);
			Register(0xC8, 2, true, SetUpdateRange);
			Register(0xC9, 6, true, TripTime);
			Register(0xCA, 6, true, UTripTime);
			Register(0xD0, 0, true, ConfigurationFile);
			Register(0xD1, 2, true, LogoutReq);
			Register(0xD6, 0, true, BatchQueryProperties);
			Register(0xD7, 0, true, EncodedCommand);
			Register(0xE1, 0, false, ClientType);
			Register(0xEF, 21, false, LoginServerSeed);
			Register(0xF4, 0, false, CrashReport);
			Register(0xFB, 2, false, PublicHouseContent);

			RegisterExtended(0x05, false, ScreenSize);
			RegisterExtended(0x06, true, PartyMessage);
			RegisterExtended(0x07, true, QuestArrow);
			RegisterExtended(0x09, true, DisarmRequest);
			RegisterExtended(0x0A, true, StunRequest);
			RegisterExtended(0x0B, false, Language);
			RegisterExtended(0x0C, true, CloseStatus);
			RegisterExtended(0x0E, true, Animate);
			RegisterExtended(0x0F, false, Empty); // What's this?
			RegisterExtended(0x10, true, QueryProperties);
			RegisterExtended(0x13, true, ContextMenuRequest);
			RegisterExtended(0x15, true, ContextMenuResponse);
			RegisterExtended(0x1A, true, StatLockChange);
			RegisterExtended(0x24, false, UnhandledBF);
            RegisterExtended(0x32, true, ToggleFlying);
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

		private static void UnhandledBF(NetState state, PacketReader pvSrc)
		{ }

        private static void Empty(NetState state, PacketReader pvSrc)
		{ }

		public static void EncodedCommand(NetState state, PacketReader pvSrc)
		{
			IEntity e = World.FindEntity(pvSrc.ReadInt32());
			int packetID = pvSrc.ReadUInt16();

			EncodedPacketHandler ph = GetEncodedHandler(packetID);

			if (ph != null)
			{
				if (ph.Ingame && state.Mobile == null)
				{
					Console.WriteLine(
						"Client: {0}: Sent ingame packet (0xD7x{1:X2}) before having been attached to a mobile", state, packetID);
					state.Dispose();
				}
				else if (ph.Ingame && state.Mobile.Deleted)
				{
					state.Dispose();
				}
				else
				{
					ph.OnReceive(state, e, new EncodedReader(pvSrc));
				}
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		public static void VendorSellReply(NetState state, PacketReader pvSrc)
		{
			Serial serial = pvSrc.ReadInt32();
			Mobile vendor = World.FindMobile(serial);

			if (vendor == null)
			{
				return;
			}

            if (vendor.Deleted || !Utility.RangeCheck(vendor.Location, state.Mobile.Location, 10))
            {
                state.Send(new EndVendorSell(vendor));
                return;
            }

            int count = pvSrc.ReadUInt16();
			if (count < 100 && pvSrc.Size == (1 + 2 + 4 + 2 + (count * 6)))
			{
				List<SellItemResponse> sellList = new List<SellItemResponse>(count);

				for (int i = 0; i < count; i++)
				{
					Item item = World.FindItem(pvSrc.ReadInt32());
					int Amount = pvSrc.ReadInt16();

					if (item != null && Amount > 0)
					{
						sellList.Add(new SellItemResponse(item, Amount));
					}
				}

				if (sellList.Count > 0 && vendor is IVendor v && v.OnSellItems(state.Mobile, sellList))
				{
                    state.Send(new EndVendorSell(vendor));
                }
			}
		}

		public static void GameCentralMoniter(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int type = pvSrc.ReadByte();
				int num1 = pvSrc.ReadInt32();

				Console.WriteLine("God Client: {0}: Game central moniter", state);
				Console.WriteLine(" - Type: {0}", type);
				Console.WriteLine(" - Number: {0}", num1);

				pvSrc.Trace(state);
			}
		}

		public static void GMSingle(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				pvSrc.Trace(state);
			}
		}

		public static void ObjectHelpRequest(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			Serial serial = pvSrc.ReadInt32();
			int unk = pvSrc.ReadByte();
			string lang = pvSrc.ReadString(3);

			if (serial.IsItem)
			{
				Item item = World.FindItem(serial);

				if (item != null && from.Map == item.Map && Utility.InUpdateRange(from, item) && from.CanSee(item))
				{
					item.OnHelpRequest(from);
				}
			}
			else if (serial.IsMobile)
			{
				Mobile m = World.FindMobile(serial);

				if (m != null && from.Map == m.Map && Utility.InUpdateRange(from, m) && from.CanSee(m))
				{
					m.OnHelpRequest(m);
				}
			}
		}

		public static void MobileNameRequest(NetState state, PacketReader pvSrc)
		{
			Mobile m = World.FindMobile(pvSrc.ReadInt32());

			if (m != null && Utility.InUpdateRange(state.Mobile, m) && state.Mobile.CanSee(m))
			{
				state.Send(new MobileName(m));
			}
		}

		public static void RequestScrollWindow(NetState state, PacketReader pvSrc)
		{
			int lastTip = pvSrc.ReadInt16();
			int type = pvSrc.ReadByte();
		}

		public static void HuePickerResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int value = pvSrc.ReadInt16();
			int hue = pvSrc.ReadInt16() & 0x3FFF;

			hue = Utility.ClipDyedHue(hue);

            for (var index = 0; index < state.HuePickers.Count; index++)
            {
                HuePicker huePicker = state.HuePickers[index];

                if (huePicker.Serial == serial)
                {
                    state.RemoveHuePicker(huePicker);

                    hue = Math.Max(0, hue);

                    if (state.Mobile == null || state.Mobile.AccessLevel < AccessLevel.GameMaster)
                    {
                        huePicker.Clip(ref hue);
                    }

                    huePicker.OnResponse(hue);

                    break;
                }
            }
        }

		public static void TripTime(NetState state, PacketReader pvSrc)
		{
			int unk1 = pvSrc.ReadByte();
			int unk2 = pvSrc.ReadInt32();

			state.Send(new TripTimeResponse(unk1));
		}

		public static void UTripTime(NetState state, PacketReader pvSrc)
		{
			int unk1 = pvSrc.ReadByte();
			int unk2 = pvSrc.ReadInt32();

			state.Send(new UTripTimeResponse(unk1));
		}

		public static void SystemInfo(NetState state, PacketReader pvSrc)
		{
			int v1 = pvSrc.ReadByte();
			int v2 = pvSrc.ReadUInt16();
			int v3 = pvSrc.ReadByte();
			string s1 = pvSrc.ReadString(32);
			string s2 = pvSrc.ReadString(32);
			string s3 = pvSrc.ReadString(32);
			string s4 = pvSrc.ReadString(32);
			int v4 = pvSrc.ReadUInt16();
			int v5 = pvSrc.ReadUInt16();
			int v6 = pvSrc.ReadInt32();
			int v7 = pvSrc.ReadInt32();
			int v8 = pvSrc.ReadInt32();
		}

        private static void AccountID(NetState state, PacketReader pvSrc)
		{ }

		public static bool VerifyGC(NetState state)
        {
            if (state.Mobile == null || state.Mobile.IsPlayer())
			{
				if (state.Running)
				{
					Console.WriteLine("Warning: {0}: Player using godclient, disconnecting", state);
				}

				state.Dispose();
				return false;
			}

            return true;
        }

		public static void UnicodePromptResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int prompt = pvSrc.ReadInt32();
			int type = pvSrc.ReadInt32();
			string lang = pvSrc.ReadString(4);
			string text = pvSrc.ReadUnicodeStringLESafe();

			if (text.Length > 128)
			{
				return;
			}

			Mobile from = state.Mobile;
			Prompt p = from.Prompt;

			int promptSerial = (p != null && p.Sender != null) ? p.Sender.Serial.Value : from.Serial.Value;

			if (p != null && promptSerial == serial && p.TypeId == prompt)
			{
				from.Prompt = null;

				if (type == 0)
				{
					p.OnCancel(from);
				}
				else
				{
					p.OnResponse(from, text);
				}
			}
		}

		public static void ConfigurationFile(NetState state, PacketReader pvSrc)
		{ }

		public static void LogoutReq(NetState state, PacketReader pvSrc)
		{
			state.Send(new LogoutAck());
		}

		public static void SetUpdateRange(NetState state, PacketReader pvSrc)
		{
			//            min   max  default
			/* 640x480    5     18   15
             * 800x600    5     18   18
             * 1024x768   5     24   24
             * 1152x864   5     24   24 
             * 1280x720   5     24   24
             */

			int range = pvSrc.ReadByte();

			// Don't let range drop below the minimum standard.
			range = Math.Max(Core.GlobalUpdateRange, range);

			int old = state.UpdateRange;

			if (old == range)
			{
				return;
			}

			state.UpdateRange = range;

			state.Send(ChangeUpdateRange.Instantiate(state.UpdateRange));

			if (state.Mobile != null)
			{
				state.Mobile.OnUpdateRangeChanged(old, state.UpdateRange);
			}
		}

        private static int[] _ValidAnimations =
        {
			6, 21, 32, 33, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
			120, 121, 123, 124, 125, 126, 127, 128
		};

        public static void Animate(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;
			int action = pvSrc.ReadInt32();

			bool ok = false;

			for (int i = 0; !ok && i < _ValidAnimations.Length; ++i)
			{
				ok = action == _ValidAnimations[i];
			}

			if (from != null && ok && from.Alive && from.Body.IsHuman && !from.Mounted)
			{
				from.Animate(action, 7, 1, true, false, 0);
			}
		}

		public static void QuestArrow(NetState state, PacketReader pvSrc)
		{
			bool rightClick = pvSrc.ReadBoolean();
			Mobile from = state.Mobile;

			if (from != null && from.QuestArrow != null)
			{
				from.QuestArrow.OnClick(rightClick);
			}
		}

		public static void ExtendedCommand(NetState state, PacketReader pvSrc)
		{
			int packetID = pvSrc.ReadUInt16();

			PacketHandler ph = GetExtendedHandler(packetID);

			if (ph != null)
			{
				if (ph.Ingame && state.Mobile == null)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("Client: {0}: Packet (0xBF.0x{1:X2}) Requires State Mobile", state, packetID);
					Utility.PopColor();

					state.Dispose();
				}
				else if (ph.Ingame && state.Mobile.Deleted)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("Client: {0}: Packet (0xBF.0x{1:X2}) Ivalid State Mobile", state, packetID);
					Utility.PopColor();

					state.Dispose();
				}
				else
				{
					ph.OnReceive(state, pvSrc);
				}
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		#region Stygain Abyss
		public static void ToggleFlying(NetState state, PacketReader pvSrc)
		{
			state.Mobile.ToggleFlying();
		}
		#endregion

		public static void BatchQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (state == null || state.Mobile == null)
			{
				return;
			}

			Mobile from = state.Mobile;
			int length = pvSrc.Size - 3;

			if (length < 0 || (length % 4) != 0)
			{
				return;
			}

			int count = length / 4;

			for (int i = 0; i < count; ++i)
			{
				Serial s = pvSrc.ReadInt32();

				if (s.IsMobile)
				{
					Mobile m = World.FindMobile(s);

					if (m != null && from.CanSee(m) && from.InUpdateRange(m))
					{
						m.SendPropertiesTo(from);
					}
				}
				else if (s.IsItem)
				{
					Item item = World.FindItem(s);

					if (item != null && !item.Deleted && from.CanSee(item) &&
						from.InUpdateRange(item.GetWorldLocation()))
					{
						item.SendPropertiesTo(from);
					}
				}
			}
		}

		public static void QueryProperties(NetState state, PacketReader pvSrc)
		{
			if (state == null || state.Mobile == null)
			{
				return;
			}

			Mobile from = state.Mobile;

			Serial s = pvSrc.ReadInt32();

			if (s.IsMobile)
			{
				Mobile m = World.FindMobile(s);

				if (m != null && from.CanSee(m) && from.InUpdateRange(m))
				{
					m.SendPropertiesTo(from);
				}
			}
			else if (s.IsItem)
			{
				Item item = World.FindItem(s);

				if (item != null && !item.Deleted && from.CanSee(item) &&
					from.InUpdateRange(item.GetWorldLocation()))
				{
					item.SendPropertiesTo(from);
				}
			}
		}

		public static void PartyMessage(NetState state, PacketReader pvSrc)
		{
			if (state.Mobile == null)
			{
				return;
			}

			switch (pvSrc.ReadByte())
			{
				case 0x01:
				PartyMessage_AddMember(state, pvSrc);
				break;
				case 0x02:
				PartyMessage_RemoveMember(state, pvSrc);
				break;
				case 0x03:
				PartyMessage_PrivateMessage(state, pvSrc);
				break;
				case 0x04:
				PartyMessage_PublicMessage(state, pvSrc);
				break;
				case 0x06:
				PartyMessage_SetCanLoot(state, pvSrc);
				break;
				case 0x08:
				PartyMessage_Accept(state, pvSrc);
				break;
				case 0x09:
				PartyMessage_Decline(state, pvSrc);
				break;
				default:
				pvSrc.Trace(state);
				break;
			}
		}

		public static void PartyMessage_AddMember(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnAdd(state.Mobile);
			}
		}

		public static void PartyMessage_RemoveMember(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnRemove(state.Mobile, World.FindMobile(pvSrc.ReadInt32()));
			}
		}

		public static void PartyMessage_PrivateMessage(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnPrivateMessage(
					state.Mobile, World.FindMobile(pvSrc.ReadInt32()), pvSrc.ReadUnicodeStringSafe());
			}
		}

		public static void PartyMessage_PublicMessage(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnPublicMessage(state.Mobile, pvSrc.ReadUnicodeStringSafe());
			}
		}

		public static void PartyMessage_SetCanLoot(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnSetCanLoot(state.Mobile, pvSrc.ReadBoolean());
			}
		}

		public static void PartyMessage_Accept(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnAccept(state.Mobile, World.FindMobile(pvSrc.ReadInt32()));
			}
		}

		public static void PartyMessage_Decline(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnDecline(state.Mobile, World.FindMobile(pvSrc.ReadInt32()));
			}
		}

		public static void StunRequest(NetState state, PacketReader pvSrc)
		{ }

		public static void DisarmRequest(NetState state, PacketReader pvSrc)
		{ }

		public static void StatLockChange(NetState state, PacketReader pvSrc)
		{
			int stat = pvSrc.ReadByte();
			int lockValue = pvSrc.ReadByte();

			if (lockValue > 2)
			{
				lockValue = 0;
			}

			Mobile m = state.Mobile;

			if (m != null)
			{
				switch (stat)
				{
					case 0:
					m.StrLock = (StatLockType)lockValue;
					break;
					case 1:
					m.DexLock = (StatLockType)lockValue;
					break;
					case 2:
					m.IntLock = (StatLockType)lockValue;
					break;
				}
			}
		}

		public static void ScreenSize(NetState state, PacketReader pvSrc)
		{
			int width = pvSrc.ReadInt32();
			int unk = pvSrc.ReadInt32();
		}

		public static void ContextMenuResponse(NetState state, PacketReader pvSrc)
		{
			Mobile user = state.Mobile;

			if (user == null)
			{
				return;
			}

			using (ContextMenu menu = user.ContextMenu)
			{
				user.ContextMenu = null;

				if (menu != null && user == menu.From)
				{
					IEntity entity = World.FindEntity(pvSrc.ReadInt32());

					if (entity != null && entity == menu.Target && user.CanSee(entity))
					{
						Point3D p;

						if (entity is Mobile)
						{
							p = entity.Location;
						}
						else if (entity is Item item)
						{
							p = item.GetWorldLocation();
						}
						else
						{
							return;
						}

						int index = pvSrc.ReadUInt16();

						if (state.IsEnhancedClient && index > 0x64)
						{
							index = menu.GetIndexEC(index);
						}

						if (index >= 0 && index < menu.Entries.Length)
						{
							using (ContextMenuEntry e = menu.Entries[index])
							{
								int range = e.Range;

								if (range == -1)
								{
									if (user.NetState != null && user.NetState.UpdateRange > 0)
									{
										range = user.NetState.UpdateRange;
									}
									else
									{
										range = Core.GlobalUpdateRange;
									}
								}

								if (user.InRange(p, range))
								{
									if (e.Enabled)
									{
										e.OnClick();
									}
									else
									{
										e.OnClickDisabled();
									}
								}
							}
						}
					}
				}
			}
		}

		public static void ContextMenuRequest(NetState state, PacketReader pvSrc)
		{
			IEntity target = World.FindEntity(pvSrc.ReadInt32());

			if (target != null)
			{
				ContextMenu.Display(state.Mobile, target);
			}
		}

		public static void CloseStatus(NetState state, PacketReader pvSrc)
		{
			Serial serial = pvSrc.ReadInt32();
		}

		public static void Language(NetState state, PacketReader pvSrc)
		{
			string lang = pvSrc.ReadString(4);

			if (state.Mobile != null)
			{
				state.Mobile.Language = lang;
			}
		}

		public static void AssistVersion(NetState state, PacketReader pvSrc)
		{
			int unk = pvSrc.ReadInt32();
			string av = pvSrc.ReadString();
		}

		public static void ClientVersion(NetState state, PacketReader pvSrc)
		{
			CV version = state.Version = new CV(pvSrc.ReadString());

			EventSink.InvokeClientVersionReceived(new ClientVersionReceivedArgs(state, version));
		}

		public static void ClientType(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadUInt16(); // 0x1
			pvSrc.ReadUInt32(); // 0x2 for KR, 0x3 for EC

			EventSink.InvokeClientTypeReceived(new ClientTypeReceivedArgs(state));
		}

		public static void PublicHouseContent(NetState state, PacketReader pvSrc)
		{
			int value = pvSrc.ReadByte();
			state.Mobile.PublicHouseContent = Convert.ToBoolean(value);
		}

		private static bool m_ClientVerification = true;

		public static bool ClientVerification { get => m_ClientVerification; set => m_ClientVerification = value; }

		internal struct AuthIDPersistence
		{
			public DateTime Age;
			public ClientVersion Version;

			public AuthIDPersistence(ClientVersion v)
			{
				Age = DateTime.UtcNow;
				Version = v;
			}
		}

		private const int m_AuthIDWindowSize = 128;

		private static readonly Dictionary<uint, AuthIDPersistence> m_AuthIDWindow = new Dictionary<uint, AuthIDPersistence>(m_AuthIDWindowSize);

		private static uint GenerateAuthID(NetState state)
		{
			if (m_AuthIDWindow.Count == m_AuthIDWindowSize)
			{
				uint oldestID = 0;
				DateTime oldest = DateTime.MaxValue;

				foreach (KeyValuePair<uint, AuthIDPersistence> kvp in m_AuthIDWindow)
				{
					if (kvp.Value.Age < oldest)
					{
						oldestID = kvp.Key;
						oldest = kvp.Value.Age;
					}
				}

				m_AuthIDWindow.Remove(oldestID);
			}

			uint authID;

			do
			{
				authID = (uint)Utility.RandomMinMax(1, uint.MaxValue - 1);

				if (Utility.RandomBool())
				{
					authID |= 1U << 31;
				}
			}
			while (m_AuthIDWindow.ContainsKey(authID));

			m_AuthIDWindow[authID] = new AuthIDPersistence(state.Version);

			return authID;
		}

        public static void GameLogin(NetState state, PacketReader pvSrc)
		{
			if (state.SentFirstPacket)
			{
				state.Dispose();
				return;
			}

			state.SentFirstPacket = true;

			uint authID = pvSrc.ReadUInt32();

			if (m_AuthIDWindow.TryGetValue(authID, out AuthIDPersistence value))
			{
                m_AuthIDWindow.Remove(authID);

				state.Version = value.Version;
			}
			else if (m_ClientVerification)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("Login: {0}: Invalid Client", state);
				Utility.PopColor();

				state.Dispose();
				return;
			}

			if (state.AuthID != 0 && authID != state.AuthID)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("Login: {0}: Invalid Client", state);
				Utility.PopColor();

				state.Dispose();
				return;
			}

			if (state.AuthID == 0 && authID != state.Seed)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("Login: {0}: Invalid Client", state);
				Utility.PopColor();

				state.Dispose();
				return;
			}

			string username = pvSrc.ReadString(30);
			string password = pvSrc.ReadString(30);

			GameLoginEventArgs e = new GameLoginEventArgs(state, username, password);

			EventSink.InvokeGameLogin(e);

			if (e.Accepted)
			{
				state.CityInfo = e.CityInfo;
				state.CompressionEnabled = true;

				state.Send(SupportedFeatures.Instantiate(state));

				state.Send(new CharacterList(state.Account, state.CityInfo, state.IsEnhancedClient));
			}
			else
			{
				state.Dispose();
			}
		}

		public static void PlayServer(NetState state, PacketReader pvSrc)
		{
			int index = pvSrc.ReadInt16();
			ServerInfo[] info = state.ServerInfo;
			IAccount a = state.Account;

			if (info == null || a == null || index < 0 || index >= info.Length)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("Client: {0}: Invalid Server ({1})", state, index);
				Utility.PopColor();

				state.Dispose();
			}
			else
			{
				state.AuthID = GenerateAuthID(state);

				state.SentFirstPacket = false;
				state.Send(new PlayServerAck(info[index], state.AuthID));
			}
		}

		public static void LoginServerSeed(NetState state, PacketReader pvSrc)
		{
			state.Seed = pvSrc.ReadUInt32();
			state.Seeded = true;

			if (state.Seed == 0)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("Login: {0}: Invalid Client", state);
				Utility.PopColor();

				state.Dispose();
				return;
			}

			int clientMaj = pvSrc.ReadInt32();
			int clientMin = pvSrc.ReadInt32();
			int clientRev = pvSrc.ReadInt32();
			int clientPat = pvSrc.ReadInt32();

			state.Version = new ClientVersion(clientMaj, clientMin, clientRev, clientPat);
		}

		public static void CrashReport(NetState state, PacketReader pvSrc)
		{
			byte clientMaj = pvSrc.ReadByte();
			byte clientMin = pvSrc.ReadByte();
			byte clientRev = pvSrc.ReadByte();
			byte clientPat = pvSrc.ReadByte();

			ushort x = pvSrc.ReadUInt16();
			ushort y = pvSrc.ReadUInt16();
			sbyte z = pvSrc.ReadSByte();
			byte map = pvSrc.ReadByte();

			string account = pvSrc.ReadString(32);
			string character = pvSrc.ReadString(32);
			string ip = pvSrc.ReadString(15);

			int unk1 = pvSrc.ReadInt32();
			int exception = pvSrc.ReadInt32();

			string process = pvSrc.ReadString(100);
			string report = pvSrc.ReadString(100);

			pvSrc.ReadByte(); // 0x00

			int offset = pvSrc.ReadInt32();

			int count = pvSrc.ReadByte();

			for (int i = 0; i < count; i++)
			{
				int address = pvSrc.ReadInt32();
			}
		}
	}
}
