using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Server.Accounting;
using Server.Commands;
using Server.Guilds;
using Server.Items;
using Server.Network;

namespace Server
{
	public delegate void SpeechEventHandler(SpeechEventArgs e);

	public delegate void ServerListEventHandler(ServerListEventArgs e);

	public delegate void MovementEventHandler(MovementEventArgs e);

	public delegate void CrashedEventHandler(CrashedEventArgs e);

	public delegate void ShutdownEventHandler(ShutdownEventArgs e);

	public delegate void SocketConnectEventHandler(SocketConnectEventArgs e);

	public delegate void AccountLoginEventHandler(AccountLoginEventArgs e);

	public delegate void PaperdollRequestEventHandler(PaperdollRequestEventArgs e);

	public delegate void AggressiveActionEventHandler(AggressiveActionEventArgs e);

	public delegate void WorldLoadEventHandler();

	public delegate void WorldSaveEventHandler(WorldSaveEventArgs e);

	public delegate void BeforeWorldSaveEventHandler(BeforeWorldSaveEventArgs e);

	public delegate void AfterWorldSaveEventHandler(AfterWorldSaveEventArgs e);

	public delegate void FastWalkEventHandler(FastWalkEventArgs e);

	public delegate void CreateGuildHandler(CreateGuildEventArgs e);

	public delegate void OnEnterRegionEventHandler(OnEnterRegionEventArgs e);

	public delegate void ItemDeletedEventHandler(ItemDeletedEventArgs e);

	public delegate void ContainerDroppedToEventHandler(ContainerDroppedToEventArgs e);

	public delegate void MultiDesignQueryHandler(MultiDesignQueryEventArgs e);

	public sealed class CreateGuildEventArgs : EventArgs
	{
		public int Id { get; set; }

		public BaseGuild Guild { get; set; }

		public CreateGuildEventArgs(int id)
		{
			Id = id;
		}
	}

	public sealed class AggressiveActionEventArgs : EventArgs
	{
		private Mobile m_Aggressed;
		private Mobile m_Aggressor;
		private bool m_Criminal;

		public Mobile Aggressed => m_Aggressed;
		public Mobile Aggressor => m_Aggressor;
		public bool Criminal => m_Criminal;

		private static readonly Queue<AggressiveActionEventArgs> m_Pool = new Queue<AggressiveActionEventArgs>();

		public static AggressiveActionEventArgs Create(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			AggressiveActionEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.m_Aggressed = aggressed;
				args.m_Aggressor = aggressor;
				args.m_Criminal = criminal;
			}
			else
			{
				args = new AggressiveActionEventArgs(aggressed, aggressor, criminal);
			}

			return args;
		}

		private AggressiveActionEventArgs(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			m_Aggressed = aggressed;
			m_Aggressor = aggressor;
			m_Criminal = criminal;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public sealed class PaperdollRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Beholder;
		private readonly Mobile m_Beheld;

		public Mobile Beholder => m_Beholder;
		public Mobile Beheld => m_Beheld;

		public PaperdollRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
		}
	}

	public sealed class AccountLoginEventArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly string m_Username;
		private readonly string m_Password;

		public NetState State => m_State;
		public string Username => m_Username;
		public string Password => m_Password;
		public bool Accepted { get; set; }
		public ALRReason RejectReason { get; set; }

		public AccountLoginEventArgs(NetState state, string username, string password)
		{
			m_State = state;
			m_Username = username;
			m_Password = password;
			Accepted = true;
		}
	}

	public sealed class SocketConnectEventArgs : EventArgs
	{
		private readonly Socket m_Socket;

		public Socket Socket => m_Socket;
		public bool AllowConnection { get; set; }

		public SocketConnectEventArgs(Socket s)
		{
			m_Socket = s;
			AllowConnection = true;
		}
	}

    public sealed class ShutdownEventArgs : EventArgs
    {
    }

	public sealed class CrashedEventArgs : EventArgs
	{
		private readonly Exception m_Exception;

		public Exception Exception => m_Exception;
		public bool Close { get; set; }

		public CrashedEventArgs(Exception e)
		{
			m_Exception = e;
		}
	}

	public sealed class MovementEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private Direction m_Direction;
		private bool m_Blocked;

		public Mobile Mobile => m_Mobile;
		public Direction Direction => m_Direction;
		public bool Blocked { get => m_Blocked; set => m_Blocked = value; }

		private static readonly Queue<MovementEventArgs> m_Pool = new Queue<MovementEventArgs>();

		public static MovementEventArgs Create(Mobile mobile, Direction dir)
		{
			MovementEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.m_Mobile = mobile;
				args.m_Direction = dir;
				args.m_Blocked = false;
			}
			else
			{
				args = new MovementEventArgs(mobile, dir);
			}

			return args;
		}

		public MovementEventArgs(Mobile mobile, Direction dir)
		{
			m_Mobile = mobile;
			m_Direction = dir;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public sealed class ServerListEventArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly IAccount m_Account;
		private readonly List<ServerInfo> m_Servers;

		public NetState State => m_State;
		public IAccount Account => m_Account;
		public bool Rejected { get; set; }
		public List<ServerInfo> Servers => m_Servers;

		public void AddServer(string name, IPEndPoint address)
		{
			AddServer(name, 0, TimeZoneInfo.Local, address);
		}

		public void AddServer(string name, int fullPercent, TimeZoneInfo tZi, IPEndPoint address)
		{
			m_Servers.Add(new ServerInfo(name, fullPercent, tZi, address));
		}

		public ServerListEventArgs(NetState state, IAccount account)
		{
			m_State = state;
			m_Account = account;
			m_Servers = new List<ServerInfo>();
		}
	}

	public readonly struct SkillNameValue
	{
		private readonly SkillName m_Name;
		private readonly int m_Value;

		public SkillName Name => m_Name;
		public int Value => m_Value;

		public SkillNameValue(SkillName name, int value)
		{
			m_Name = name;
			m_Value = value;
		}
	}

	public sealed class SpeechEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly MessageType m_Type;
		private readonly int m_Hue;
		private readonly int[] m_Keywords;

		public Mobile Mobile => m_Mobile;
		public string Speech { get; set; }
		public MessageType Type => m_Type;
		public int Hue => m_Hue;
		public int[] Keywords => m_Keywords;
		public bool Handled { get; set; }
		public bool Blocked { get; set; }

		public bool HasKeyword(int keyword)
		{
			for (int i = 0; i < m_Keywords.Length; ++i)
			{
				if (m_Keywords[i] == keyword)
				{
					return true;
				}
			}

			return false;
		}

		public SpeechEventArgs(Mobile mobile, string speech, MessageType type, int hue, int[] keywords)
		{
			m_Mobile = mobile;
			Speech = speech;
			m_Type = type;
			m_Hue = hue;
			m_Keywords = keywords;
		}
	}

	public sealed class WorldSaveEventArgs : EventArgs
	{
        public WorldSaveEventArgs()
		{
        }
	}

	public sealed class BeforeWorldSaveEventArgs : EventArgs
	{
		public BeforeWorldSaveEventArgs()
		{
		}
	}

	public sealed class AfterWorldSaveEventArgs : EventArgs
	{
		public AfterWorldSaveEventArgs()
		{
		}
	}

	public sealed class FastWalkEventArgs : EventArgs
	{
		private readonly NetState m_State;

		public FastWalkEventArgs(NetState state)
		{
			m_State = state;
			Blocked = false;
		}

		public NetState NetState => m_State;
		public bool Blocked { get; set; }
	}

	public sealed class OnEnterRegionEventArgs : EventArgs
	{
		private readonly Mobile m_From;
		private readonly Region m_OldRegion;
		private readonly Region m_NewRegion;

		public OnEnterRegionEventArgs(Mobile from, Region oldRegion, Region newRegion)
		{
			m_From = from;
			m_OldRegion = oldRegion;
			m_NewRegion = newRegion;
		}

		public Mobile From => m_From;
		public Region OldRegion => m_OldRegion;
		public Region NewRegion => m_NewRegion;
	}

	public sealed class ItemDeletedEventArgs : EventArgs
	{
		public Item Item { get; set; }

		public ItemDeletedEventArgs(Item item)
		{
			Item = item;
		}
	}

	public sealed class ContainerDroppedToEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public Container Container { get; set; }
		public Item Dropped { get; set; }

		public ContainerDroppedToEventArgs(Mobile m, Container container, Item dropped)
		{
			Mobile = m;
			Container = container;
			Dropped = dropped;
		}
	}

	public sealed class MultiDesignQueryEventArgs : EventArgs
	{
		public NetState State { get; set; }
		public BaseMulti Multi { get; set; }

		public MultiDesignQueryEventArgs(NetState state, BaseMulti multi)
		{
			State = state;
			Multi = multi;
		}
	}

	public static class EventSink
	{
		public static event SpeechEventHandler Speech;
		public static event ServerListEventHandler ServerList;
		public static event MovementEventHandler Movement;
		public static event CrashedEventHandler Crashed;
		public static event ShutdownEventHandler Shutdown;
		public static event SocketConnectEventHandler SocketConnect;
		public static event AccountLoginEventHandler AccountLogin;
		public static event PaperdollRequestEventHandler PaperdollRequest;
		public static event AggressiveActionEventHandler AggressiveAction;
		public static event CommandEventHandler Command;
		public static event WorldLoadEventHandler WorldLoad;
		public static event WorldSaveEventHandler WorldSave;
		public static event BeforeWorldSaveEventHandler BeforeWorldSave;
		public static event AfterWorldSaveEventHandler AfterWorldSave;
		public static event FastWalkEventHandler FastWalk;
		public static event CreateGuildHandler CreateGuild;
		public static event OnEnterRegionEventHandler OnEnterRegion;
		public static event ItemDeletedEventHandler ItemDeleted;
		public static event ContainerDroppedToEventHandler ContainerDroppedTo;
		public static event MultiDesignQueryHandler MultiDesign;

		public static void InvokeCreateGuild(CreateGuildEventArgs e)
        {
            CreateGuild?.Invoke(e);
        }

		public static void InvokeFastWalk(FastWalkEventArgs e)
        {
            FastWalk?.Invoke(e);
        }

		public static void InvokeCommand(CommandEventArgs e)
        {
            Command?.Invoke(e);
        }

		public static void InvokeAggressiveAction(AggressiveActionEventArgs e)
        {
            AggressiveAction?.Invoke(e);
        }

		public static void InvokePaperdollRequest(PaperdollRequestEventArgs e)
        {
            PaperdollRequest?.Invoke(e);
        }

		public static void InvokeAccountLogin(AccountLoginEventArgs e)
        {
            AccountLogin?.Invoke(e);
        }

		public static void InvokeSocketConnect(SocketConnectEventArgs e)
        {
            SocketConnect?.Invoke(e);
        }

		public static void InvokeShutdown(ShutdownEventArgs e)
        {
            Shutdown?.Invoke(e);
        }

		public static void InvokeCrashed(CrashedEventArgs e)
        {
            Crashed?.Invoke(e);
        }

		public static void InvokeMovement(MovementEventArgs e)
        {
            Movement?.Invoke(e);
        }

		public static void InvokeServerList(ServerListEventArgs e)
        {
            ServerList?.Invoke(e);
        }

		public static void InvokeSpeech(SpeechEventArgs e)
        {
            Speech?.Invoke(e);
        }

		public static void InvokeWorldLoad()
        {
            WorldLoad?.Invoke();
        }

		public static void InvokeWorldSave(WorldSaveEventArgs e)
        {
            WorldSave?.Invoke(e);
        }

		public static void InvokeBeforeWorldSave(BeforeWorldSaveEventArgs e)
        {
            BeforeWorldSave?.Invoke(e);
        }

		public static void InvokeAfterWorldSave(AfterWorldSaveEventArgs e)
        {
            AfterWorldSave?.Invoke(e);
        }

		public static void InvokeOnEnterRegion(OnEnterRegionEventArgs e)
        {
            OnEnterRegion?.Invoke(e);
        }

		public static void InvokeItemDeleted(ItemDeletedEventArgs e)
        {
            ItemDeleted?.Invoke(e);
        }

		public static void InvokeContainerDroppedTo(ContainerDroppedToEventArgs e)
        {
            ContainerDroppedTo?.Invoke(e);
        }

		public static void InvokeMultiDesignQuery(MultiDesignQueryEventArgs e)
        {
            MultiDesign?.Invoke(e);
        }
    }
}
