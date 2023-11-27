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
	public delegate void CheckEquipItemEventHandler(CheckEquipItemEventArgs e);

	public delegate void WorldBroadcastEventHandler(WorldBroadcastEventArgs e);

	public delegate void CharacterCreatedEventHandler(CharacterCreatedEventArgs e);

	public delegate void OpenDoorMacroEventHandler(OpenDoorMacroEventArgs e);

	public delegate void SpeechEventHandler(SpeechEventArgs e);

	public delegate void LoginEventHandler(LoginEventArgs e);

	public delegate void ServerListEventHandler(ServerListEventArgs e);

	public delegate void MovementEventHandler(MovementEventArgs e);

	public delegate void CrashedEventHandler(CrashedEventArgs e);

	public delegate void ShutdownEventHandler(ShutdownEventArgs e);

	public delegate void HelpRequestEventHandler(HelpRequestEventArgs e);

	public delegate void OpenSpellbookRequestEventHandler(OpenSpellbookRequestEventArgs e);

	public delegate void CastSpellRequestEventHandler(CastSpellRequestEventArgs e);

	public delegate void BandageTargetRequestEventHandler(BandageTargetRequestEventArgs e);

	public delegate void AnimateRequestEventHandler(AnimateRequestEventArgs e);

	public delegate void LogoutEventHandler(LogoutEventArgs e);

	public delegate void SocketConnectEventHandler(SocketConnectEventArgs e);

	public delegate void ConnectedEventHandler(ConnectedEventArgs e);

	public delegate void DisconnectedEventHandler(DisconnectedEventArgs e);

	public delegate void RenameRequestEventHandler(RenameRequestEventArgs e);

	public delegate void PlayerDeathEventHandler(PlayerDeathEventArgs e);

	public delegate void CreatureDeathEventHandler(CreatureDeathEventArgs e);

	public delegate void VirtueGumpRequestEventHandler(VirtueGumpRequestEventArgs e);

	public delegate void VirtueItemRequestEventHandler(VirtueItemRequestEventArgs e);

	public delegate void VirtueMacroRequestEventHandler(VirtueMacroRequestEventArgs e);

	public delegate void AccountLoginEventHandler(AccountLoginEventArgs e);

	public delegate void PaperdollRequestEventHandler(PaperdollRequestEventArgs e);

	public delegate void ProfileRequestEventHandler(ProfileRequestEventArgs e);

	public delegate void ChangeProfileRequestEventHandler(ChangeProfileRequestEventArgs e);

	public delegate void AggressiveActionEventHandler(AggressiveActionEventArgs e);

	public delegate void GameLoginEventHandler(GameLoginEventArgs e);

	public delegate void DeleteRequestEventHandler(DeleteRequestEventArgs e);

	public delegate void WorldLoadEventHandler();

	public delegate void WorldSaveEventHandler(WorldSaveEventArgs e);

	public delegate void BeforeWorldSaveEventHandler(BeforeWorldSaveEventArgs e);

	public delegate void AfterWorldSaveEventHandler(AfterWorldSaveEventArgs e);

	public delegate void SetAbilityEventHandler(SetAbilityEventArgs e);

	public delegate void FastWalkEventHandler(FastWalkEventArgs e);

	public delegate void ServerStartedEventHandler();

	public delegate void CreateGuildHandler(CreateGuildEventArgs e);

	public delegate void GuildGumpRequestHandler(GuildGumpRequestArgs e);

	public delegate void ClientVersionReceivedHandler(ClientVersionReceivedArgs e);

	public delegate void ClientTypeReceivedHandler(ClientTypeReceivedArgs e);

	public delegate void OnKilledByEventHandler(OnKilledByEventArgs e);

	public delegate void OnEnterRegionEventHandler(OnEnterRegionEventArgs e);

	public delegate void QuestCompleteEventHandler(QuestCompleteEventArgs e);

	public delegate void ItemDeletedEventHandler(ItemDeletedEventArgs e);

	public delegate void TargetedSpellEventHandler(TargetedSpellEventArgs e);

	public delegate void TargetedSkillEventHandler(TargetedSkillEventArgs e);

	public delegate void EquipMacroEventHandler(EquipMacroEventArgs e);

	public delegate void UnequipMacroEventHandler(UnequipMacroEventArgs e);

	public delegate void TargetByResourceMacroEventHandler(TargetByResourceMacroEventArgs e);

	public delegate void ContainerDroppedToEventHandler(ContainerDroppedToEventArgs e);

	public delegate void MultiDesignQueryHandler(MultiDesignQueryEventArgs e);

	public class CheckEquipItemEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item Item { get; }
		public bool Message { get; }

		public bool Block { get; set; }

		public CheckEquipItemEventArgs(Mobile m, Item item, bool message)
		{
			Mobile = m;
			Item = item;
			Message = message;
		}
	}

	public class WorldBroadcastEventArgs : EventArgs
	{
		public int Hue { get; }
		public bool Ascii { get; }
		public AccessLevel Access { get; }
		public string Text { get; }

		public WorldBroadcastEventArgs(int hue, bool ascii, AccessLevel access, string text)
		{
			Hue = hue;
			Ascii = ascii;
			Access = access;
			Text = text;
		}
	}

	public class ClientVersionReceivedArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly ClientVersion m_Version;

		public NetState State => m_State;
		public ClientVersion Version => m_Version;

		public ClientVersionReceivedArgs(NetState state, ClientVersion cv)
		{
			m_State = state;
			m_Version = cv;
		}
	}

	public class ClientTypeReceivedArgs : EventArgs
	{
		private readonly NetState m_State;

		public NetState State => m_State;

		public ClientTypeReceivedArgs(NetState state)
		{
			m_State = state;
		}
	}

	public class CreateGuildEventArgs : EventArgs
	{
		public int Id { get; set; }

		public BaseGuild Guild { get; set; }

		public CreateGuildEventArgs(int id)
		{
			Id = id;
		}
	}

	public class GuildGumpRequestArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public GuildGumpRequestArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class SetAbilityEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly int m_Index;

		public Mobile Mobile => m_Mobile;
		public int Index => m_Index;

		public SetAbilityEventArgs(Mobile mobile, int index)
		{
			m_Mobile = mobile;
			m_Index = index;
		}
	}

	public class DeleteRequestEventArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly int m_Index;

		public NetState State => m_State;
		public int Index => m_Index;

		public DeleteRequestEventArgs(NetState state, int index)
		{
			m_State = state;
			m_Index = index;
		}
	}

	public class GameLoginEventArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly string m_Username;
		private readonly string m_Password;

		public NetState State => m_State;
		public string Username => m_Username;
		public string Password => m_Password;
		public bool Accepted { get; set; }
		public CityInfo[] CityInfo { get; set; }

		public GameLoginEventArgs(NetState state, string un, string pw)
		{
			m_State = state;
			m_Username = un;
			m_Password = pw;
		}
	}

	public class AggressiveActionEventArgs : EventArgs
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

	public class ProfileRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Beholder;
		private readonly Mobile m_Beheld;

		public Mobile Beholder => m_Beholder;
		public Mobile Beheld => m_Beheld;

		public ProfileRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
		}
	}

	public class ChangeProfileRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Beholder;
		private readonly Mobile m_Beheld;
		private readonly string m_Text;

		public Mobile Beholder => m_Beholder;
		public Mobile Beheld => m_Beheld;
		public string Text => m_Text;

		public ChangeProfileRequestEventArgs(Mobile beholder, Mobile beheld, string text)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
			m_Text = text;
		}
	}

	public class PaperdollRequestEventArgs : EventArgs
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

	public class AccountLoginEventArgs : EventArgs
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

	public class VirtueItemRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Beholder;
		private readonly Mobile m_Beheld;
		private readonly int m_GumpID;

		public Mobile Beholder => m_Beholder;
		public Mobile Beheld => m_Beheld;
		public int GumpID => m_GumpID;

		public VirtueItemRequestEventArgs(Mobile beholder, Mobile beheld, int gumpID)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
			m_GumpID = gumpID;
		}
	}

	public class VirtueGumpRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Beholder;
		private readonly Mobile m_Beheld;

		public Mobile Beholder => m_Beholder;
		public Mobile Beheld => m_Beheld;

		public VirtueGumpRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
		}
	}

	public class VirtueMacroRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly int m_VirtueID;

		public Mobile Mobile => m_Mobile;
		public int VirtueID => m_VirtueID;

		public VirtueMacroRequestEventArgs(Mobile mobile, int virtueID)
		{
			m_Mobile = mobile;
			m_VirtueID = virtueID;
		}
	}

	public class PlayerDeathEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Mobile Killer { get; }
		public Container Corpse { get; }

		public PlayerDeathEventArgs(Mobile mobile)
			: this(mobile, mobile.LastKiller, mobile.Corpse)
		{ }

		public PlayerDeathEventArgs(Mobile mobile, Mobile killer, Container corpse)
		{
			Mobile = mobile;
			Killer = killer;
			Corpse = corpse;
		}
	}

	public class CreatureDeathEventArgs : EventArgs
	{
		public Mobile Creature { get; }
		public Mobile Killer { get; }
		public Container Corpse { get; }

		public List<Item> ForcedLoot { get; private set; }

		public bool PreventDefault { get; set; }
		public bool PreventDelete { get; set; }
		public bool ClearCorpse { get; set; }

		public CreatureDeathEventArgs(Mobile creature)
			: this(creature, creature.LastKiller, creature.Corpse)
		{ }

		public CreatureDeathEventArgs(Mobile creature, Mobile killer, Container corpse)
		{
			Creature = creature;
			Killer = killer;
			Corpse = corpse;

			ForcedLoot = new List<Item>();
		}

		public void ClearLoot(bool free)
		{
			if (free)
			{
				ForcedLoot.Clear();
				ForcedLoot.TrimExcess();
			}
			else
			{
				ForcedLoot = new List<Item>();
			}
		}
	}

	public class RenameRequestEventArgs : EventArgs
	{
		private readonly Mobile m_From;
		private readonly Mobile m_Target;
		private readonly string m_Name;

		public Mobile From => m_From;
		public Mobile Target => m_Target;
		public string Name => m_Name;

		public RenameRequestEventArgs(Mobile from, Mobile target, string name)
		{
			m_From = from;
			m_Target = target;
			m_Name = name;
		}
	}

	public class LogoutEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public LogoutEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class SocketConnectEventArgs : EventArgs
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

	public class ConnectedEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public ConnectedEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class DisconnectedEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public DisconnectedEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class AnimateRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly string m_Action;

		public Mobile Mobile => m_Mobile;
		public string Action => m_Action;

		public AnimateRequestEventArgs(Mobile m, string action)
		{
			m_Mobile = m;
			m_Action = action;
		}
	}

	public class CastSpellRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly Item m_Spellbook;
		private readonly int m_SpellID;

		public Mobile Mobile => m_Mobile;
		public Item Spellbook => m_Spellbook;
		public int SpellID => m_SpellID;

		public CastSpellRequestEventArgs(Mobile m, int spellID, Item book)
		{
			m_Mobile = m;
			m_Spellbook = book;
			m_SpellID = spellID;
		}
	}

	public class BandageTargetRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly Item m_Bandage;
		private readonly Mobile m_Target;

		public Mobile Mobile => m_Mobile;
		public Item Bandage => m_Bandage;
		public Mobile Target => m_Target;

		public BandageTargetRequestEventArgs(Mobile m, Item bandage, Mobile target)
		{
			m_Mobile = m;
			m_Bandage = bandage;
			m_Target = target;
		}
	}

	public class OpenSpellbookRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly int m_Type;

		public Mobile Mobile => m_Mobile;
		public int Type => m_Type;

		public OpenSpellbookRequestEventArgs(Mobile m, int type)
		{
			m_Mobile = m;
			m_Type = type;
		}
	}

	public class HelpRequestEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public HelpRequestEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class ShutdownEventArgs : EventArgs
	{ }

	public class CrashedEventArgs : EventArgs
	{
		private readonly Exception m_Exception;

		public Exception Exception => m_Exception;
		public bool Close { get; set; }

		public CrashedEventArgs(Exception e)
		{
			m_Exception = e;
		}
	}

	public class MovementEventArgs : EventArgs
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

	public class ServerListEventArgs : EventArgs
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

	public class CharacterCreatedEventArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly IAccount m_Account;
		private readonly CityInfo m_City;
		private readonly SkillNameValue[] m_Skills;
		private readonly int m_ShirtHue;
		private readonly int m_PantsHue;
		private readonly int m_HairID;
		private readonly int m_HairHue;
		private readonly int m_BeardID;
		private readonly int m_BeardHue;
		private readonly string m_Name;
		private readonly bool m_Female;
		private readonly int m_Hue;
		private readonly int m_Str;
		private readonly int m_Dex;
		private readonly int m_Int;
		private readonly Race m_Race;
		private readonly int m_Face;
		private readonly int m_FaceHue;

		public NetState State => m_State;
		public IAccount Account => m_Account;
		public Mobile Mobile { get; set; }
		public string Name => m_Name;
		public bool Female => m_Female;
		public int Hue => m_Hue;
		public int Str => m_Str;
		public int Dex => m_Dex;
		public int Int => m_Int;
		public CityInfo City => m_City;
		public SkillNameValue[] Skills => m_Skills;
		public int ShirtHue => m_ShirtHue;
		public int PantsHue => m_PantsHue;
		public int HairID => m_HairID;
		public int HairHue => m_HairHue;
		public int BeardID => m_BeardID;
		public int BeardHue => m_BeardHue;
		public int Profession { get; set; }
		public Race Race => m_Race;
		public int FaceID => m_Face;
		public int FaceHue => m_FaceHue;

		public CharacterCreatedEventArgs(
			NetState state,
			IAccount a,
			string name,
			bool female,
			int hue,
			int str,
			int dex,
			int intel,
			CityInfo city,
			SkillNameValue[] skills,
			int shirtHue,
			int pantsHue,
			int hairID,
			int hairHue,
			int beardID,
			int beardHue,
			int profession,
			Race race)
			: this(state, a, name, female, hue, str, dex, intel, city, skills, shirtHue, pantsHue, hairID, hairHue, beardID, beardHue, profession, race, 0, 0)
		{
		}

		public CharacterCreatedEventArgs(
			NetState state,
			IAccount a,
			string name,
			bool female,
			int hue,
			int str,
			int dex,
			int intel,
			CityInfo city,
			SkillNameValue[] skills,
			int shirtHue,
			int pantsHue,
			int hairID,
			int hairHue,
			int beardID,
			int beardHue,
			int profession,
			Race race,
			int faceID,
			int faceHue)
		{
			m_State = state;
			m_Account = a;
			m_Name = name;
			m_Female = female;
			m_Hue = hue;
			m_Str = str;
			m_Dex = dex;
			m_Int = intel;
			m_City = city;
			m_Skills = skills;
			m_ShirtHue = shirtHue;
			m_PantsHue = pantsHue;
			m_HairID = hairID;
			m_HairHue = hairHue;
			m_BeardID = beardID;
			m_BeardHue = beardHue;
			Profession = profession;
			m_Race = race;
			m_Face = faceID;
			m_FaceHue = faceHue;
		}
	}

	public class OpenDoorMacroEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public OpenDoorMacroEventArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class SpeechEventArgs : EventArgs
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

	public class LoginEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public Mobile Mobile => m_Mobile;

		public LoginEventArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class WorldSaveEventArgs : EventArgs
	{
        public WorldSaveEventArgs()
		{
        }
	}

	public class BeforeWorldSaveEventArgs : EventArgs
	{
		public BeforeWorldSaveEventArgs()
		{
		}
	}


	public class AfterWorldSaveEventArgs : EventArgs
	{
		public AfterWorldSaveEventArgs()
		{
		}
	}


	public class FastWalkEventArgs : EventArgs
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

	public class OnKilledByEventArgs : EventArgs
	{
		private readonly Mobile m_Killed;
		private readonly Mobile m_KilledBy;

		public OnKilledByEventArgs(Mobile killed, Mobile killedBy)
		{
			m_Killed = killed;
			m_KilledBy = killedBy;
		}

		public Mobile Killed => m_Killed;
		public Mobile KilledBy => m_KilledBy;
	}

	public class OnEnterRegionEventArgs : EventArgs
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

	public class QuestCompleteEventArgs : EventArgs
	{
		public Type QuestType { get; }
		public Mobile Mobile { get; }

		public QuestCompleteEventArgs(Mobile from, Type type)
		{
			Mobile = from;
			QuestType = type;
		}
	}

	public class ItemDeletedEventArgs : EventArgs
	{
		public Item Item { get; set; }

		public ItemDeletedEventArgs(Item item)
		{
			Item = item;
		}
	}

	public class TargetedSpellEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly IEntity m_Target;
		private readonly short m_SpellID;

		public Mobile Mobile => m_Mobile;
		public IEntity Target => m_Target;
		public short SpellID => m_SpellID;

		public TargetedSpellEventArgs(Mobile m, IEntity target, short spellID)
		{
			m_Mobile = m;
			m_Target = target;
			m_SpellID = spellID;
		}
	}

	public class TargetedSkillEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly IEntity m_Target;
		private readonly short m_SkillID;

		public Mobile Mobile => m_Mobile;
		public IEntity Target => m_Target;
		public short SkillID => m_SkillID;

		public TargetedSkillEventArgs(Mobile m, IEntity target, short skillID)
		{
			m_Mobile = m;
			m_Target = target;
			m_SkillID = skillID;
		}
	}

	public class TargetByResourceMacroEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly Item m_Tool;
		private readonly int m_ResourceType;

		public Mobile Mobile => m_Mobile;
		public Item Tool => m_Tool;
		public int ResourceType => m_ResourceType;

		public TargetByResourceMacroEventArgs(Mobile mobile, Item tool, int type)
		{
			m_Mobile = mobile;
			m_Tool = tool;
			m_ResourceType = type;
		}
	}

	public class EquipMacroEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly List<int> m_List;

		public Mobile Mobile => m_Mobile;
		public List<int> List => m_List;

		public EquipMacroEventArgs(Mobile mobile, List<int> list)
		{
			m_Mobile = mobile;
			m_List = list;
		}
	}

	public class UnequipMacroEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly List<int> m_List;

		public Mobile Mobile => m_Mobile;
		public List<int> List => m_List;

		public UnequipMacroEventArgs(Mobile mobile, List<int> list)
		{
			m_Mobile = mobile;
			m_List = list;
		}
	}

	public class ContainerDroppedToEventArgs : EventArgs
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

	public class MultiDesignQueryEventArgs : EventArgs
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
		public static event CheckEquipItemEventHandler CheckEquipItem;
		public static event WorldBroadcastEventHandler WorldBroadcast;
		public static event CharacterCreatedEventHandler CharacterCreated;
		public static event OpenDoorMacroEventHandler OpenDoorMacroUsed;
		public static event SpeechEventHandler Speech;
		public static event LoginEventHandler Login;
		public static event ServerListEventHandler ServerList;
		public static event MovementEventHandler Movement;
		public static event CrashedEventHandler Crashed;
		public static event ShutdownEventHandler Shutdown;
		public static event HelpRequestEventHandler HelpRequest;
		public static event OpenSpellbookRequestEventHandler OpenSpellbookRequest;
		public static event CastSpellRequestEventHandler CastSpellRequest;
		public static event BandageTargetRequestEventHandler BandageTargetRequest;
		public static event AnimateRequestEventHandler AnimateRequest;
		public static event LogoutEventHandler Logout;
		public static event SocketConnectEventHandler SocketConnect;
		public static event ConnectedEventHandler Connected;
		public static event DisconnectedEventHandler Disconnected;
		public static event RenameRequestEventHandler RenameRequest;
		public static event PlayerDeathEventHandler PlayerDeath;
		public static event CreatureDeathEventHandler CreatureDeath;
		public static event VirtueGumpRequestEventHandler VirtueGumpRequest;
		public static event VirtueItemRequestEventHandler VirtueItemRequest;
		public static event VirtueMacroRequestEventHandler VirtueMacroRequest;
		public static event AccountLoginEventHandler AccountLogin;
		public static event PaperdollRequestEventHandler PaperdollRequest;
		public static event ProfileRequestEventHandler ProfileRequest;
		public static event ChangeProfileRequestEventHandler ChangeProfileRequest;
		public static event AggressiveActionEventHandler AggressiveAction;
		public static event CommandEventHandler Command;
		public static event GameLoginEventHandler GameLogin;
		public static event DeleteRequestEventHandler DeleteRequest;
		public static event WorldLoadEventHandler WorldLoad;
		public static event WorldSaveEventHandler WorldSave;
		public static event BeforeWorldSaveEventHandler BeforeWorldSave;
		public static event AfterWorldSaveEventHandler AfterWorldSave;
		public static event SetAbilityEventHandler SetAbility;
		public static event FastWalkEventHandler FastWalk;
		public static event CreateGuildHandler CreateGuild;
		public static event ServerStartedEventHandler ServerStarted;
		public static event GuildGumpRequestHandler GuildGumpRequest;
		public static event ClientVersionReceivedHandler ClientVersionReceived;
		public static event ClientTypeReceivedHandler ClientTypeReceived;
		public static event OnKilledByEventHandler OnKilledBy;
		public static event OnEnterRegionEventHandler OnEnterRegion;
		public static event QuestCompleteEventHandler QuestComplete;
		public static event ItemDeletedEventHandler ItemDeleted;
		public static event TargetedSpellEventHandler TargetedSpell;
		public static event TargetedSkillEventHandler TargetedSkill;
		public static event EquipMacroEventHandler EquipMacro;
		public static event UnequipMacroEventHandler UnequipMacro;
		public static event TargetByResourceMacroEventHandler TargetByResourceMacro;
		public static event ContainerDroppedToEventHandler ContainerDroppedTo;
		public static event MultiDesignQueryHandler MultiDesign;

		public static void InvokeCheckEquipItem(CheckEquipItemEventArgs e)
        {
            CheckEquipItem?.Invoke(e);
        }

		public static void InvokeWorldBroadcast(WorldBroadcastEventArgs e)
        {
            WorldBroadcast?.Invoke(e);
        }

		public static void InvokeClientVersionReceived(ClientVersionReceivedArgs e)
        {
            ClientVersionReceived?.Invoke(e);
        }

		public static void InvokeClientTypeReceived(ClientTypeReceivedArgs e)
        {
            ClientTypeReceived?.Invoke(e);
        }

		public static void InvokeServerStarted()
        {
            ServerStarted?.Invoke();
        }

		public static void InvokeCreateGuild(CreateGuildEventArgs e)
        {
            CreateGuild?.Invoke(e);
        }

		public static void InvokeSetAbility(SetAbilityEventArgs e)
        {
            SetAbility?.Invoke(e);
        }

		public static void InvokeGuildGumpRequest(GuildGumpRequestArgs e)
        {
            GuildGumpRequest?.Invoke(e);
        }

		public static void InvokeFastWalk(FastWalkEventArgs e)
        {
            FastWalk?.Invoke(e);
        }

		public static void InvokeDeleteRequest(DeleteRequestEventArgs e)
        {
            DeleteRequest?.Invoke(e);
        }

		public static void InvokeGameLogin(GameLoginEventArgs e)
        {
            GameLogin?.Invoke(e);
        }

		public static void InvokeCommand(CommandEventArgs e)
        {
            Command?.Invoke(e);
        }

		public static void InvokeAggressiveAction(AggressiveActionEventArgs e)
        {
            AggressiveAction?.Invoke(e);
        }

		public static void InvokeProfileRequest(ProfileRequestEventArgs e)
        {
            ProfileRequest?.Invoke(e);
        }

		public static void InvokeChangeProfileRequest(ChangeProfileRequestEventArgs e)
        {
            ChangeProfileRequest?.Invoke(e);
        }

		public static void InvokePaperdollRequest(PaperdollRequestEventArgs e)
        {
            PaperdollRequest?.Invoke(e);
        }

		public static void InvokeAccountLogin(AccountLoginEventArgs e)
        {
            AccountLogin?.Invoke(e);
        }

		public static void InvokeVirtueItemRequest(VirtueItemRequestEventArgs e)
        {
            VirtueItemRequest?.Invoke(e);
        }

		public static void InvokeVirtueGumpRequest(VirtueGumpRequestEventArgs e)
        {
            VirtueGumpRequest?.Invoke(e);
        }

		public static void InvokeVirtueMacroRequest(VirtueMacroRequestEventArgs e)
        {
            VirtueMacroRequest?.Invoke(e);
        }

		public static void InvokePlayerDeath(PlayerDeathEventArgs e)
        {
            PlayerDeath?.Invoke(e);
        }

		public static void InvokeCreatureDeath(CreatureDeathEventArgs e)
        {
            CreatureDeath?.Invoke(e);
        }

		public static void InvokeRenameRequest(RenameRequestEventArgs e)
        {
            RenameRequest?.Invoke(e);
        }

		public static void InvokeLogout(LogoutEventArgs e)
        {
            Logout?.Invoke(e);
        }

		public static void InvokeSocketConnect(SocketConnectEventArgs e)
        {
            SocketConnect?.Invoke(e);
        }

		public static void InvokeConnected(ConnectedEventArgs e)
        {
            Connected?.Invoke(e);
        }

		public static void InvokeDisconnected(DisconnectedEventArgs e)
        {
            Disconnected?.Invoke(e);
        }

		public static void InvokeAnimateRequest(AnimateRequestEventArgs e)
        {
            AnimateRequest?.Invoke(e);
        }

		public static void InvokeCastSpellRequest(CastSpellRequestEventArgs e)
        {
            CastSpellRequest?.Invoke(e);
        }

		public static void InvokeBandageTargetRequest(BandageTargetRequestEventArgs e)
        {
            BandageTargetRequest?.Invoke(e);
        }

		public static void InvokeOpenSpellbookRequest(OpenSpellbookRequestEventArgs e)
        {
            OpenSpellbookRequest?.Invoke(e);
        }

		public static void InvokeHelpRequest(HelpRequestEventArgs e)
        {
            HelpRequest?.Invoke(e);
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

		public static void InvokeLogin(LoginEventArgs e)
        {
            Login?.Invoke(e);
        }

		public static void InvokeSpeech(SpeechEventArgs e)
        {
            Speech?.Invoke(e);
        }

		public static void InvokeCharacterCreated(CharacterCreatedEventArgs e)
        {
            CharacterCreated?.Invoke(e);
        }

		public static void InvokeOpenDoorMacroUsed(OpenDoorMacroEventArgs e)
        {
            OpenDoorMacroUsed?.Invoke(e);
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

		public static void InvokeOnKilledBy(OnKilledByEventArgs e)
        {
            OnKilledBy?.Invoke(e);
        }

		public static void InvokeOnEnterRegion(OnEnterRegionEventArgs e)
        {
            OnEnterRegion?.Invoke(e);
        }

		public static void InvokeQuestComplete(QuestCompleteEventArgs e)
        {
            QuestComplete?.Invoke(e);
        }

		public static void InvokeItemDeleted(ItemDeletedEventArgs e)
        {
            ItemDeleted?.Invoke(e);
        }

		public static void InvokeTargetedSpell(TargetedSpellEventArgs e)
        {
            TargetedSpell?.Invoke(e);
        }

		public static void InvokeTargetedSkill(TargetedSkillEventArgs e)
        {
            TargetedSkill?.Invoke(e);
        }

		public static void InvokeTargetByResourceMacro(TargetByResourceMacroEventArgs e)
        {
            TargetByResourceMacro?.Invoke(e);
        }

		public static void InvokeEquipMacro(EquipMacroEventArgs e)
        {
            EquipMacro?.Invoke(e);
        }

		public static void InvokeUnequipMacro(UnequipMacroEventArgs e)
        {
            UnequipMacro?.Invoke(e);
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
