using System;
using System.Collections.Generic;
using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.ArenaSystem;
using Server.Engines.BulkOrders;
using Server.Engines.CannedEvil;
using Server.Engines.Chat;
using Server.Engines.CityLoyalty;
using Server.Engines.Craft;
using Server.Engines.Despise;
using Server.Engines.Doom;
using Server.Engines.Events;
using Server.Engines.Help;
using Server.Engines.InstancedPeerless;
using Server.Engines.NewMagincia;
using Server.Engines.PartySystem;
using Server.Engines.Plants;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Engines.Shadowguard;
using Server.Engines.SphynxFortune;
using Server.Engines.VendorSearching;
using Server.Engines.VeteranRewards;
using Server.Engines.VoidPool;
using Server.Engines.VvV;
using Server.Events.Halloween;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Multis;
using Server.Network;
using Server.Network.Packets;
using Server.Regions;
using Server.Services.TownCryer;
using Server.Services.Virtues;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.SkillMasteries;
using Server.Spells.Spellweaving;
using Server.Targeting;
using Aggression = Server.Misc.Aggression;
using RankDefinition = Server.Guilds.RankDefinition;

namespace Server.Mobiles
{
    #region Enums
    [Flags]
    public enum PlayerFlag
    {
        None = 0x00000000,
        Glassblowing = 0x00000001,
        Masonry = 0x00000002,
        SandMining = 0x00000004,
        StoneMining = 0x00000008,
        ToggleMiningStone = 0x00000010,
        KarmaLocked = 0x00000020,
        AutoRenewInsurance = 0x00000040,
        UseOwnFilter = 0x00000080,
        PublicMyRunUO = 0x00000100,
        PagingSquelched = 0x00000200,
        Young = 0x00000400,
        AcceptGuildInvites = 0x00000800,
        DisplayChampionTitle = 0x00001000,
        HasStatReward = 0x00002000,
        Bedlam = 0x00010000,
        LibraryFriend = 0x00020000,
        Spellweaving = 0x00040000,
        GemMining = 0x00080000,
        ToggleMiningGem = 0x00100000,
        BasketWeaving = 0x00200000,
        AbyssEntry = 0x00400000,
        ToggleClippings = 0x00800000,
        ToggleCutClippings = 0x01000000,
        ToggleCutReeds = 0x02000000,
        MechanicalLife = 0x04000000,
        Unused = 0x08000000,
        ToggleCutTopiaries = 0x10000000,
        HasValiantStatReward = 0x20000000,
        RefuseTrades = 0x40000000
    }

    [Flags]
    public enum ExtendedPlayerFlag
    {
        Unused = 0x00000001,
        ToggleStoneOnly = 0x00000002,
        CanBuyCarpets = 0x00000004,
        VoidPool = 0x00000008,
        DisabledPvpWarning = 0x00000010
    }

    public enum NpcGuild
    {
        None,
        MagesGuild,
        WarriorsGuild,
        ThievesGuild,
        RangersGuild,
        HealersGuild,
        MinersGuild,
        MerchantsGuild,
        TinkersGuild,
        TailorsGuild,
        FishermensGuild,
        BardsGuild,
        BlacksmithsGuild
    }

    public enum SolenFriendship
    {
        None,
        Red,
        Black
    }
    #endregion

    public partial class PlayerMobile : Mobile, IHonorTarget
    {
        public static List<PlayerMobile> Instances { get; }

        static PlayerMobile()
        {
            Instances = new List<PlayerMobile>(0x1000);
        }

        #region Mount Blocking
        public void SetMountBlock(BlockMountType type, TimeSpan duration, bool dismount)
        {
            if (dismount)
            {
                BaseMount.Dismount(this, this, type, duration, false);
            }
            else
            {
                BaseMount.SetMountPrevention(this, type, duration);
            }
        }
        #endregion

        #region Stygian Abyss
        public override void ToggleFlying()
        {
            if (Race != Race.Gargoyle)
                return;

            if (Frozen)
            {
                SendLocalizedMessage(1060170); // You cannot use this ability while frozen.
                return;
            }

            if (!Flying)
            {
                if (BeginAction(typeof(FlySpell)))
                {
                    if (Spell is Spell flySpell)
                    {
                        flySpell.Disturb(DisturbType.Unspecified, false, false);
                    }

                    Spell spell = new FlySpell(this);
                    spell.Cast();

                    Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
                }
                else
                {
                    LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
                }
            }
            else if (IsValidLandLocation(Location, Map))
            {
                if (BeginAction(typeof(FlySpell)))
                {
                    if (Spell is Spell flySpell)
                        flySpell.Disturb(DisturbType.Unspecified, false, false);

                    Animate(AnimationType.Land, 0);
                    Flying = false;
                    BuffInfo.RemoveBuff(this, BuffIcon.Fly);

                    Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
                }
                else
                {
                    LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
                }
            }
            else
                LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113081); // You may not land here.
        }

        public static bool IsValidLandLocation(Point3D p, Map map)
        {
            return map.CanFit(p.X, p.Y, p.Z, 16, false, false);
        }
        #endregion

        private DesignContext m_DesignContext;

        private NpcGuild m_NpcGuild;
        private DateTime m_NpcGuildJoinTime;
        private TimeSpan m_NpcGuildGameTime;
        private PlayerFlag m_Flags;
        private ExtendedPlayerFlag m_ExtendedFlags;
        private int m_Profession;

        private int m_NonAutoreinsuredItems;
        // number of items that could not be automaitically reinsured because gold in bank was not enough

        /*
		* a value of zero means, that the mobile is not executing the spell. Otherwise,
		* the value should match the BaseMana required
		*/
        private int m_ExecutesLightningStrike; // move to Server.Mobiles??

        private DateTime m_LastOnline;
        private RankDefinition m_GuildRank;
        private bool m_NextEnhanceSuccess;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool NextEnhanceSuccess { get => m_NextEnhanceSuccess; set => m_NextEnhanceSuccess = value; }

        private int m_GuildMessageHue, m_AllianceMessageHue;

        private List<Mobile> m_AutoStabled;
        private List<Mobile> m_AllFollowers;
        private List<Mobile> m_RecentlyReported;

        public bool UseSummoningRite { get; set; }

        #region Points System
        private PointsSystemProps _PointsSystemProps;
        private BODProps _BODProps;
        private AccountGoldProps _AccountGold;

        [CommandProperty(AccessLevel.GameMaster)]
        public PointsSystemProps PointSystems
        {
            get
            {
                if (_PointsSystemProps == null)
                    _PointsSystemProps = new PointsSystemProps(this);

                return _PointsSystemProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BODProps BODData
        {
            get
            {
                if (_BODProps == null)
                {
                    _BODProps = new BODProps(this);
                }

                return _BODProps;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AccountGoldProps AccountGold
        {
            get
            {
                if (_AccountGold == null)
                {
                    _AccountGold = new AccountGoldProps(this);
                }

                return _AccountGold;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AccountSovereigns
        {
            get
            {
                if (Account is Account acct)
                {
                    return acct.Sovereigns;
                }

                return 0;
            }
            set
            {
                Account acct = Account as Account;

                acct?.SetSovereigns(value);
            }
        }

        public bool DepositSovereigns(int amount)
        {
            if (Account is Account acct)
            {
                return acct.DepositSovereigns(amount);
            }

            return false;
        }

        public bool WithdrawSovereigns(int amount)
        {
            if (Account is Account acct)
            {
                return acct.WithdrawSovereigns(amount);
            }

            return false;
        }
        #endregion

        #region Getters & Setters
        public List<Mobile> RecentlyReported { get => m_RecentlyReported; set => m_RecentlyReported = value; }

        public List<Mobile> AutoStabled => m_AutoStabled;

        public bool NinjaWepCooldown { get; set; }

        public List<Mobile> AllFollowers
        {
            get
            {
                if (m_AllFollowers == null)
                {
                    m_AllFollowers = new List<Mobile>();
                }

                return m_AllFollowers;
            }
        }

        [CommandProperty(AccessLevel.GameMaster, true)]
        public RankDefinition GuildRank
        {
            get
            {
                if (AccessLevel >= AccessLevel.GameMaster)
                {
                    return RankDefinition.Leader;
                }

                return m_GuildRank;
            }
            set => m_GuildRank = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GuildMessageHue { get => m_GuildMessageHue; set => m_GuildMessageHue = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AllianceMessageHue { get => m_AllianceMessageHue; set => m_AllianceMessageHue = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Profession { get => m_Profession; set => m_Profession = value; }

        public int StepsTaken { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public NpcGuild NpcGuild { get => m_NpcGuild; set => m_NpcGuild = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NpcGuildJoinTime { get => m_NpcGuildJoinTime; set => m_NpcGuildJoinTime = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextBODTurnInTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastOnline { get => m_LastOnline; set => m_LastOnline = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NpcGuildGameTime { get => m_NpcGuildGameTime; set => m_NpcGuildGameTime = value; }

        public int ExecutesLightningStrike { get => m_ExecutesLightningStrike; set => m_ExecutesLightningStrike = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ToothAche { get => BaseSweet.GetToothAche(this); set => BaseSweet.SetToothAche(this, value, true); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool MechanicalLife { get => GetFlag(PlayerFlag.MechanicalLife); set => SetFlag(PlayerFlag.MechanicalLife, value); }
        #endregion

        #region PlayerFlags
        public PlayerFlag Flags { get => m_Flags; set => m_Flags = value; }
        public ExtendedPlayerFlag ExtendedFlags { get => m_ExtendedFlags; set => m_ExtendedFlags = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PagingSquelched { get => GetFlag(PlayerFlag.PagingSquelched); set => SetFlag(PlayerFlag.PagingSquelched, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Glassblowing { get => GetFlag(PlayerFlag.Glassblowing); set => SetFlag(PlayerFlag.Glassblowing, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Masonry { get => GetFlag(PlayerFlag.Masonry); set => SetFlag(PlayerFlag.Masonry, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SandMining { get => GetFlag(PlayerFlag.SandMining); set => SetFlag(PlayerFlag.SandMining, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool StoneMining { get => GetFlag(PlayerFlag.StoneMining); set => SetFlag(PlayerFlag.StoneMining, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool GemMining { get => GetFlag(PlayerFlag.GemMining); set => SetFlag(PlayerFlag.GemMining, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool BasketWeaving { get => GetFlag(PlayerFlag.BasketWeaving); set => SetFlag(PlayerFlag.BasketWeaving, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningStone { get => GetFlag(PlayerFlag.ToggleMiningStone); set => SetFlag(PlayerFlag.ToggleMiningStone, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AbyssEntry { get => GetFlag(PlayerFlag.AbyssEntry); set => SetFlag(PlayerFlag.AbyssEntry, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleMiningGem { get => GetFlag(PlayerFlag.ToggleMiningGem); set => SetFlag(PlayerFlag.ToggleMiningGem, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool KarmaLocked { get => GetFlag(PlayerFlag.KarmaLocked); set => SetFlag(PlayerFlag.KarmaLocked, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AutoRenewInsurance { get => GetFlag(PlayerFlag.AutoRenewInsurance); set => SetFlag(PlayerFlag.AutoRenewInsurance, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool UseOwnFilter { get => GetFlag(PlayerFlag.UseOwnFilter); set => SetFlag(PlayerFlag.UseOwnFilter, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AcceptGuildInvites { get => GetFlag(PlayerFlag.AcceptGuildInvites); set => SetFlag(PlayerFlag.AcceptGuildInvites, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasStatReward { get => GetFlag(PlayerFlag.HasStatReward); set => SetFlag(PlayerFlag.HasStatReward, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasValiantStatReward { get => GetFlag(PlayerFlag.HasValiantStatReward); set => SetFlag(PlayerFlag.HasValiantStatReward, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RefuseTrades
        {
            get => GetFlag(PlayerFlag.RefuseTrades);
            set => SetFlag(PlayerFlag.RefuseTrades, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisabledPvpWarning
        {
            get => GetFlag(ExtendedPlayerFlag.DisabledPvpWarning);
            set => SetFlag(ExtendedPlayerFlag.DisabledPvpWarning, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CanBuyCarpets
        {
            get => GetFlag(ExtendedPlayerFlag.CanBuyCarpets);
            set => SetFlag(ExtendedPlayerFlag.CanBuyCarpets, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool VoidPool
        {
            get => GetFlag(ExtendedPlayerFlag.VoidPool);
            set => SetFlag(ExtendedPlayerFlag.VoidPool, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleStoneOnly
        {
            get => GetFlag(ExtendedPlayerFlag.ToggleStoneOnly);
            set => SetFlag(ExtendedPlayerFlag.ToggleStoneOnly, value);
        }

        #region Plant system
        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleClippings { get => GetFlag(PlayerFlag.ToggleClippings); set => SetFlag(PlayerFlag.ToggleClippings, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutReeds { get => GetFlag(PlayerFlag.ToggleCutReeds); set => SetFlag(PlayerFlag.ToggleCutReeds, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutClippings { get => GetFlag(PlayerFlag.ToggleCutClippings); set => SetFlag(PlayerFlag.ToggleCutClippings, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ToggleCutTopiaries { get => GetFlag(PlayerFlag.ToggleCutTopiaries); set => SetFlag(PlayerFlag.ToggleCutTopiaries, value); }

        private DateTime m_SSNextSeed;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SSNextSeed { get => m_SSNextSeed; set => m_SSNextSeed = value; }

        private DateTime m_SSSeedExpire;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SSSeedExpire { get => m_SSSeedExpire; set => m_SSSeedExpire = value; }

        private Point3D m_SSSeedLocation;

        public Point3D SSSeedLocation { get => m_SSSeedLocation; set => m_SSSeedLocation = value; }

        private Map m_SSSeedMap;

        public Map SSSeedMap { get => m_SSSeedMap; set => m_SSSeedMap = value; }
        #endregion

        #endregion

        #region Auto Arrow Recovery
        private readonly Dictionary<Type, int> m_RecoverableAmmo = new Dictionary<Type, int>();

        public Dictionary<Type, int> RecoverableAmmo => m_RecoverableAmmo;

        public void RecoverAmmo()
        {
            if (Alive)
            {
                foreach (KeyValuePair<Type, int> kvp in m_RecoverableAmmo)
                {
                    if (kvp.Value > 0)
                    {
                        Item ammo = null;

                        try
                        {
                            ammo = Activator.CreateInstance(kvp.Key) as Item;
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }

                        if (ammo != null)
                        {
                            string name = ammo.Name;
                            ammo.Amount = kvp.Value;

                            if (name == null)
                            {
                                if (ammo is Arrow)
                                {
                                    name = "arrow";
                                }
                                else if (ammo is Bolt)
                                {
                                    name = "bolt";
                                }
                            }

                            if (name != null && ammo.Amount > 1)
                            {
                                name = $"{name}s";
                            }

                            if (name == null)
                            {
                                name = $"#{ammo.LabelNumber}";
                            }

                            PlaceInBackpack(ammo);
                            SendLocalizedMessage(1073504, $"{ammo.Amount}\t{name}"); // You recover ~1_NUM~ ~2_AMMO~.
                        }
                    }
                }

                m_RecoverableAmmo.Clear();
            }
        }
        #endregion

        #region Reward Stable Slots
        [CommandProperty(AccessLevel.GameMaster)]
        public int RewardStableSlots { get; set; }
        #endregion

        private DateTime m_AnkhNextUse;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AnkhNextUse { get => m_AnkhNextUse; set => m_AnkhNextUse = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextGemOfSalvationUse { get; set; }

        #region Mondain's Legacy
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Bedlam { get => GetFlag(PlayerFlag.Bedlam); set => SetFlag(PlayerFlag.Bedlam, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool LibraryFriend { get => GetFlag(PlayerFlag.LibraryFriend); set => SetFlag(PlayerFlag.LibraryFriend, value); }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Spellweaving { get => GetFlag(PlayerFlag.Spellweaving); set => SetFlag(PlayerFlag.Spellweaving, value); }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan DisguiseTimeLeft => DisguiseTimers.TimeRemaining(this);

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime PeacedUntil { get; set; }

        [CommandProperty(AccessLevel.Decorator)]
        public override string TitleName
        {
            get
            {
                string name;

                name = Fame >= 10000 ? $"{(Female ? "Lady" : "Lord")} {RawName}" : RawName;

                return name;
            }
        }

        #region Scroll of Alacrity
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime AcceleratedStart { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName AcceleratedSkill { get; set; }
        #endregion

        public static Direction GetDirection4(Point3D from, Point3D to)
        {
            int dx = from.X - to.X;
            int dy = from.Y - to.Y;

            int rx = dx - dy;
            int ry = dx + dy;

            Direction ret;

            if (rx >= 0 && ry >= 0)
            {
                ret = Direction.West;
            }
            else if (rx >= 0 && ry < 0)
            {
                ret = Direction.South;
            }
            else if (rx < 0 && ry < 0)
            {
                ret = Direction.East;
            }
            else
            {
                ret = Direction.North;
            }

            return ret;
        }

        public override bool OnDroppedItemToWorld(Item item, Point3D location)
        {
            if (!base.OnDroppedItemToWorld(item, location))
            {
                return false;
            }

            IPooledEnumerable mobiles = Map.GetMobilesInRange(location, 0);

            foreach (Mobile m in mobiles)
            {
                if (m.Z >= location.Z && m.Z < location.Z + 16)
                {
                    mobiles.Free();
                    return false;
                }
            }

            mobiles.Free();

            BounceInfo bi = item.GetBounce();

            if (bi != null && AccessLevel > AccessLevel.Counselor)
            {
                Type type = item.GetType();

                if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
                {
                    object[] objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

                    if (objs.Length > 0)
                    {
                        if (objs[0] is FlipableAttribute fp)
                        {
                            int[] itemIDs = fp.ItemIDs;

                            Point3D oldWorldLoc = bi.m_WorldLoc;
                            Point3D newWorldLoc = location;

                            if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
                            {
                                Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

                                if (itemIDs.Length == 2)
                                {
                                    switch (dir)
                                    {
                                        case Direction.North:
                                        case Direction.South:
                                            item.ItemID = itemIDs[0];
                                            break;
                                        case Direction.East:
                                        case Direction.West:
                                            item.ItemID = itemIDs[1];
                                            break;
                                    }
                                }
                                else if (itemIDs.Length == 4)
                                {
                                    switch (dir)
                                    {
                                        case Direction.South:
                                            item.ItemID = itemIDs[0];
                                            break;
                                        case Direction.East:
                                            item.ItemID = itemIDs[1];
                                            break;
                                        case Direction.North:
                                            item.ItemID = itemIDs[2];
                                            break;
                                        case Direction.West:
                                            item.ItemID = itemIDs[3];
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override int GetPacketFlags()
        {
            int flags = base.GetPacketFlags();

            return flags;
        }

        public override int GetOldPacketFlags()
        {
            int flags = base.GetOldPacketFlags();

            return flags;
        }

        public bool GetFlag(PlayerFlag flag)
        {
            return (m_Flags & flag) != 0;
        }

        public void SetFlag(PlayerFlag flag, bool value)
        {
            if (value)
            {
                m_Flags |= flag;
            }
            else
            {
                m_Flags &= ~flag;
            }
        }

        public bool GetFlag(ExtendedPlayerFlag flag)
        {
            return (m_ExtendedFlags & flag) != 0;
        }

        public void SetFlag(ExtendedPlayerFlag flag, bool value)
        {
            if (value)
            {
                m_ExtendedFlags |= flag;
            }
            else
            {
                m_ExtendedFlags &= ~flag;
            }
        }

        public DesignContext DesignContext { get => m_DesignContext; set => m_DesignContext = value; }

        public static void Initialize()
        {
            if (FastwalkPrevention)
            {
                PacketHandlers.RegisterThrottler(0x02, MovementThrottle_Callback);
            }

            Timer.DelayCall(TimeSpan.Zero, CheckPets);
        }

        public static void TargetedSkillUse(Mobile from, IEntity target, int skillId)
        {
            if (from == null || target == null)
                return;

            from.TargetLocked = true;

            if (skillId == (short)SkillName.Provocation)
            {
                Provocation.DeferredSecondaryTarget = true;
                InvokeTarget(from, target, skillId);
                Provocation.DeferredSecondaryTarget = false;
            }

            else if (skillId == (short)SkillName.AnimalTaming)
            {
                AnimalTaming.DisableMessage = true;
                AnimalTaming.DeferredTarget = false;
                InvokeTarget(from, target, skillId);
                AnimalTaming.DeferredTarget = true;
                AnimalTaming.DisableMessage = false;

            }
            else
            {
                InvokeTarget(from, target, skillId);
            }


            from.TargetLocked = false;
        }

        private static void InvokeTarget(Mobile from, IEntity target, int skillId)
        {
            if (from.UseSkill(skillId))
            {
                from.Target?.Invoke(from, target);
            }
        }

        public static void EquipMacro(Mobile m, List<Serial> list)
        {
            if (m is PlayerMobile pm && pm.Backpack != null && pm.Alive && list != null && list.Count > 0 && !pm.HasTrade)
            {
                if (pm.IsStaff() || Core.TickCount - pm.NextActionTime >= 0)
                {
                    Container pack = pm.Backpack;

                    for (int itemSerial = 0; itemSerial < list.Count; itemSerial++)
                    {
                        Serial serial = list[itemSerial];

                        Item item = null;

                        for (int index = 0; index < pack.Items.Count; index++)
                        {
                            Item i = pack.Items[index];

                            if (i.Serial == serial)
                            {
                                item = i;
                                break;
                            }
                        }

                        if (item != null)
                        {
                            Item toMove = pm.FindItemOnLayer(item.Layer);

                            if (toMove != null)
                            {
                                toMove.Internalize();

                                if (!pm.EquipItem(item))
                                {
                                    pm.EquipItem(toMove);
                                }
                                else
                                {
                                    pack.DropItem(toMove);
                                }
                            }
                            else
                            {
                                pm.EquipItem(item);
                            }
                        }
                    }

                    pm.NextActionTime = Core.TickCount + ActionDelay * list.Count;
                }
	            else
	            {
	                pm.SendActionMessage();
	            }
	        }
        }

        public static void UnequipMacro(Mobile m, List<Layer> layers)
        {
            if (m is PlayerMobile pm && pm.Backpack != null && pm.Alive && layers != null && layers.Count > 0 && !pm.HasTrade)
            {
                if (pm.IsStaff() || Core.TickCount - pm.NextActionTime >= 0)
                {
                    Container pack = pm.Backpack;

                    List<Item> worn = new List<Item>(pm.Items);

                    for (int index = 0; index < worn.Count; index++)
                    {
                        Item item = worn[index];

                        if (layers.Contains(item.Layer))
                        {
                            pack.TryDropItem(pm, item, false);
                        }
                    }

                    pm.NextActionTime = Core.TickCount + ActionDelay;
                    ColUtility.Free(worn);
                }
	            else
	            {
	                pm.SendActionMessage();
	            }
	        }
        }

        private static void CheckPets()
        {
            foreach (Mobile value in World.Mobiles.Values)
            {
                if (value is PlayerMobile pm && ((!pm.Mounted || pm.Mount is EtherealMount) && pm.AllFollowers.Count > pm.AutoStabled.Count || pm.Mounted && pm.AllFollowers.Count > pm.AutoStabled.Count + 1))
                {
                    pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
                }
            }
        }

        public override void OnSkillInvalidated(Skill skill)
        {
            if (skill.SkillName == SkillName.MagicResist)
            {
                UpdateResistances();
            }
        }

        public override int GetMaxResistance(ResistanceType type)
        {
            if (IsStaff())
            {
                return 100;
            }

            int max = base.GetMaxResistance(type);
            int refineBonus = BaseArmor.GetRefinedResist(this, type);

            if (refineBonus != 0)
            {
                max += refineBonus;
            }
            else
            {
                max += Spells.Mysticism.StoneFormSpell.GetMaxResistBonus(this);
            }

            if (Race == Race.Elf && type == ResistanceType.Energy)
            {
                max += 5; //Intended to go after the 60 max from curse
            }

            if (type != ResistanceType.Physical && 60 < max && CurseSpell.UnderEffect(this))
            {
                max -= 10;
            }

            if ((type == ResistanceType.Fire || type == ResistanceType.Poison) && CorpseSkinSpell.IsUnderEffects(this))
            {
                max = CorpseSkinSpell.GetResistMalus(this);
            }

            if (type == ResistanceType.Physical && MagicReflectSpell.HasReflect(this))
            {
                max -= 5;
            }

            return max;
        }

        public override void ComputeResistances()
        {
            base.ComputeResistances();

            for (int i = 0; i < Resistances.Length; ++i)
            {
                Resistances[i] = 0;
            }

            Resistances[0] += BasePhysicalResistance;
            Resistances[1] += BaseFireResistance;
            Resistances[2] += BaseColdResistance;
            Resistances[3] += BasePoisonResistance;
            Resistances[4] += BaseEnergyResistance;

            for (int i = 0; ResistanceMods != null && i < ResistanceMods.Count; ++i)
            {
                ResistanceMod mod = ResistanceMods[i];
                int v = (int)mod.Type;

                if (v >= 0 && v < Resistances.Length)
                {
                    Resistances[v] += mod.Offset;
                }
            }

            for (int i = 0; i < Items.Count; ++i)
            {
                Item item = Items[i];

                if (item.CheckPropertyConfliction(this))
                {
                    continue;
                }

                ISetItem setItem = item as ISetItem;

                Resistances[0] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Physical) : item.PhysicalResistance;
                Resistances[1] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Fire) : item.FireResistance;
                Resistances[2] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Cold) : item.ColdResistance;
                Resistances[3] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Poison) : item.PoisonResistance;
                Resistances[4] += setItem != null && setItem.SetEquipped ? setItem.SetResistBonus(ResistanceType.Energy) : item.EnergyResistance;
            }

            for (int i = 0; i < Resistances.Length; ++i)
            {
                int min = GetMinResistance((ResistanceType)i);
                int max = GetMaxResistance((ResistanceType)i);

                if (max < min)
                {
                    max = min;
                }

                if (Resistances[i] > max)
                {
                    Resistances[i] = max;
                }
                else if (Resistances[i] < min)
                {
                    Resistances[i] = min;
                }
            }
        }

        protected override void OnRaceChange(Race oldRace)
        {
            if (oldRace == Race.Gargoyle && Flying)
            {
                Flying = false;
                SendSpeedControl(SpeedControlType.Disable);
                BuffInfo.RemoveBuff(this, BuffIcon.Fly);
            }
            else if (oldRace != Race.Gargoyle && Race == Race.Gargoyle && Mounted)
            {
                Mount.Rider = null;
            }

            ValidateEquipment();
            UpdateResistances();
        }

        public override int MaxWeight => (Race == Race.Human ? 100 : 40) + (int)(3.5 * Str);

        private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

        public override void OnNetStateChanged()
        {
            m_LastGlobalLight = -1;
            m_LastPersonalLight = -1;
        }

        public override void ComputeBaseLightLevels(out int global, out int personal)
        {
            global = LightCycle.ComputeLevelFor(this);

            bool racialNightSight = Race == Race.Elf;

            if (LightLevel < 21 && (AosAttributes.GetValue(this, AosAttribute.NightSight) > 0 || racialNightSight))
            {
                personal = 21;
            }
            else
            {
                personal = LightLevel;
            }
        }

        public override void CheckLightLevels(bool forceResend)
        {
            NetState ns = NetState;

            if (ns == null)
            {
                return;
            }

            int global, personal;

            ComputeLightLevels(out global, out personal);

            if (!forceResend)
            {
                forceResend = global != m_LastGlobalLight || personal != m_LastPersonalLight;
            }

            if (!forceResend)
            {
                return;
            }

            m_LastGlobalLight = global;
            m_LastPersonalLight = personal;

            ns.Send(GlobalLightLevelPacket.Instantiate(global));
            ns.Send(new PersonalLightLevelPacket(this, personal));
        }

        public override bool SendSpeedControl(SpeedControlType type)
        {
            AnimalFormContext context = AnimalForm.GetContext(this);

            if (context != null && context.SpeedBoost)
            {
                switch (type)
                {
                    case SpeedControlType.WalkSpeed: return base.SendSpeedControl(SpeedControlType.WalkSpeedFast);
                    case SpeedControlType.Disable: return base.SendSpeedControl(SpeedControlType.MountSpeed);
                }
            }

            return base.SendSpeedControl(type);
        }

        public override int GetMinResistance(ResistanceType type)
        {
            int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
            int min = int.MinValue;

            if (magicResist >= 1000)
            {
                min = 40 + (magicResist - 1000) / 50;
            }
            else if (magicResist >= 400)
            {
                min = (magicResist - 400) / 15;
            }

            return Math.Max(MinPlayerResistance, Math.Min(MaxPlayerResistance, min));
        }

        #region City Loyalty
        public override int GetResistance(ResistanceType type)
        {
            int resistance = base.GetResistance(type) + SphynxFortune.GetResistanceBonus(this, type);

            if (CityLoyaltySystem.HasTradeDeal(this, TradeDeal.SocietyOfClothiers))
            {
                resistance++;
                return Math.Min(resistance, GetMaxResistance(type));
            }

            return resistance;
        }
        #endregion

        public override void OnManaChange(int oldValue)
        {
            base.OnManaChange(oldValue);
            if (m_ExecutesLightningStrike > 0)
            {
                if (Mana < m_ExecutesLightningStrike)
                {
                    SpecialMove.ClearCurrentMove(this);
                }
            }
        }

        public override void OnLogin()
        {
            CheckAtrophies(this);

            if (AccountHandler.LockdownLevel > AccessLevel.VIP)
            {
                string notice;

                Account acct = Account as Account;

                if (acct == null || !acct.HasAccess(NetState))
                {
                    notice = IsPlayer() ? "The server is currently under lockdown. No players are allowed to log in at this time." : "The server is currently under lockdown. You do not have sufficient access level to connect.";

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), Disconnect, this);
                }
                else if (AccessLevel >= AccessLevel.Administrator)
                {
                    notice = "The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
                }
                else
                {
                    notice = "The server is currently under lockdown. You have sufficient access level to connect.";
                }

                SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
                return;
            }

            ClaimAutoStabledPets();
            ValidateEquipment();

            ReportMurdererGump.CheckMurderer(this);

            QuestHelper.QuestionQuestCheck(this);

            if (Siege.SiegeShard && Map == Map.Trammel && AccessLevel == AccessLevel.Player)
            {
                Map = Map.Felucca;
            }

            if (NetState != null && NetState.IsEnhancedClient && Mount is EtherealMount fromMount)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), mount =>
                {
                    if (mount.IsChildOf(Backpack))
                    {
                        mount.Rider = this;
                    }
                },
                fromMount);
            }

            CheckStatTimers();

            Accounting.Account.OnLogin(this);
            AnimalForm.OnLogin(this);
            BaseBeverage.OnLogin(this);
            Caddellite.OnLogin(this);
            ChampionSpawnRegion.OnLogin(this);
            CityLoyaltySystem.OnLogin(this);
            CorgulRegion.OnLogin(this);
            DespiseController.OnLogin(this);
            FellowshipMedallion.OnLogin(this);
            Focus.OnLogin(this);
            GiftGiving.OnLogin(this);
            GiftOfLifeSpell.OnLogin(this);
            HouseRegion.OnLogin(this);
            HumilityProofGump.OnLogin(this);
            KhaldunTastyTreat.OnLogin(this);
            LampRoomRegion.OnLogin(this);
            LightCycle.OnLogin(this);
            LoginStats.OnLogin(this);
            MaginciaLottoSystem.OnLogin(this);
            MasteryInfo.OnLogin(this);
            PlantSystem.OnLogin(this);
            PotionOfGloriousFortune.OnLogin(this);
            QuestSystem.OnLogin(this);
            ResponseEntry.OnLogin(this);
            ScrollOfAlacrity.OnLogin(this);
            ShadowguardController.OnLogin(this);
            ShardPoller.OnLogin(this);
            StormLevelGump.OnLogin(this);
            Strandedness.OnLogin(this);
            TwistedWealdDesert.OnLogin(this);

            Engines.PartySystem.Party.OnLogin(this);

            if (PreventInaccess.Enabled)
            {
                PreventInaccess.OnLogin(this);
            }

            if (RewardSystem.Enabled)
            {
                RewardSystem.OnLogin(this);
            }

            if (TownCryerSystem.Enabled)
            {
                TownCryerSystem.OnLogin(this);
            }

            if (ViceVsVirtueSystem.Enabled)
            {
                ViceVsVirtueSystem.OnLogin(this);
            }

            if (Siege.SiegeShard)
            {
                Siege.OnLogin(this);
            }

            ResendBuffs();
        }

        private bool m_NoDeltaRecursion;

        public void ValidateEquipment()
        {
            if (m_NoDeltaRecursion || Map == null || Map == Map.Internal)
            {
                return;
            }

            if (Items == null)
            {
                return;
            }

            m_NoDeltaRecursion = true;
            Timer.DelayCall(TimeSpan.Zero, ValidateEquipment_Sandbox);
        }

        private void ValidateEquipment_Sandbox()
        {
            try
            {
                if (Map == null || Map == Map.Internal)
                {
                    return;
                }

                List<Item> items = Items;

                if (items == null)
                {
                    return;
                }

                bool moved = false;

                int str = Str;
                int dex = Dex;
                int intel = Int;

                Mobile from = this;

                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                    {
                        continue;
                    }

                    Item item = items[i];

                    bool drop = !RaceDefinitions.ValidateEquipment(from, item, false);

                    if (item is BaseWeapon weapon)
                    {
                        if (!drop)
                        {
                            if (dex < weapon.DexRequirement)
                            {
                                drop = true;
                            }
                            else if (str < AOS.Scale(weapon.StrRequirement, 100 - weapon.GetLowerStatReq()))
                            {
                                drop = true;
                            }
                            else if (intel < weapon.IntRequirement)
                            {
                                drop = true;
                            }
                        }

                        if (drop)
                        {
                            string name = weapon.Name;

                            if (name == null)
                            {
                                name = $"#{weapon.LabelNumber}";
                            }

                            from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
                            from.AddToBackpack(weapon);
                            moved = true;
                        }
                    }
                    else if (item is BaseArmor armor)
                    {
                        if (!drop)
                        {
                            if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else
                            {
                                int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
                                int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
                                int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

                                if (dex < dexReq || dex + dexBonus < 1)
                                {
                                    drop = true;
                                }
                                else if (str < strReq || str + strBonus < 1)
                                {
                                    drop = true;
                                }
                                else if (intel < intReq || intel + intBonus < 1)
                                {
                                    drop = true;
                                }
                            }
                        }

                        if (drop)
                        {
                            string name = armor.Name;

                            if (name == null)
                            {
                                name = $"#{armor.LabelNumber}";
                            }

                            if (armor is BaseShield)
                            {
                                from.SendLocalizedMessage(1062003, name); // You can no longer equip your ~1_SHIELD~
                            }
                            else
                            {
                                from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~
                            }

                            from.AddToBackpack(armor);
                            moved = true;
                        }
                    }
                    else if (item is BaseClothing clothing)
                    {
                        if (!drop)
                        {
                            if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
                            {
                                drop = true;
                            }
                            else
                            {
                                int strBonus = clothing.ComputeStatBonus(StatType.Str);
                                int strReq = clothing.ComputeStatReq(StatType.Str);

                                if (str < strReq || str + strBonus < 1)
                                {
                                    drop = true;
                                }
                            }
                        }

                        if (drop)
                        {
                            string name = clothing.Name;

                            if (name == null)
                            {
                                name = $"#{clothing.LabelNumber}";
                            }

                            from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

                            from.AddToBackpack(clothing);
                            moved = true;
                        }
                    }
                    else if (item is BaseQuiver && drop)
                    {
                        from.AddToBackpack(item);

                        from.SendLocalizedMessage(1062002, "quiver"); // You can no longer wear your ~1_ARMOR~
                        moved = true;
                    }

                    #region Vice Vs Virtue
                    if (item is IVvVItem vvvItem && vvvItem.IsVvVItem && !ViceVsVirtueSystem.IsVvV(from))
                    {
                        from.AddToBackpack(item);
                        moved = true;
                    }
                    #endregion
                }

                if (from.Mount is VvVMount && !ViceVsVirtueSystem.IsVvV(from))
                {
                    from.Mount.Rider = null;
                }

                if (moved)
                {
                    from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
                }
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
            finally
            {
                m_NoDeltaRecursion = false;
            }
        }

        public override void Delta(MobileDelta flag)
        {
            base.Delta(flag);

            if ((flag & MobileDelta.Stat) != 0)
            {
                ValidateEquipment();
            }

            InvalidateProperties();
        }

        public override void OnConnected(Mobile m)
        {
            base.OnConnected(m);

            // Checks young timer.
            Accounting.Account.OnConnected(m);

            if (m is PlayerMobile pm)
            {
                pm.m_SessionStart = DateTime.UtcNow;

                pm.m_Quest?.StartTimer();
                QuestHelper.StartTimer(pm);

                pm.BedrollLogout = false;

                pm.LastOnline = DateTime.UtcNow;
            }

            DisguiseTimers.StartTimer(m);

            Timer.DelayCall(TimeSpan.Zero, ClearSpecialMovesCallback, m);
        }

        private static void ClearSpecialMovesCallback(object state)
        {
            Mobile from = (Mobile)state;

            SpecialMove.ClearAllMoves(from);
        }

        public override void OnDisconnected(Mobile m)
        {
            base.OnDisconnected(m);

            // Young timer
            Accounting.Account.OnDisconnected(m);

            DesignContext context = DesignContext.Find(m);
            if (context != null)
            {
                /* Client disconnected
                 *  - Remove design context
                 *  - Eject all from house
                 *  - Restore relocated entities
                 */
                // Remove design context
                DesignContext.Remove(m);

                // Eject all from house
                m.RevealingAction();

                List<Item> list = context.Foundation.GetItems();
                for (int index = 0; index < list.Count; index++)
                {
                    Item item = list[index];
                    item.Location = context.Foundation.BanLocation;
                }

                List<Mobile> mobiles = context.Foundation.GetMobiles();
                for (int index = 0; index < mobiles.Count; index++)
                {
                    Mobile mobile = mobiles[index];
                    mobile.Location = context.Foundation.BanLocation;
                }

                // Restore relocated entities
                context.Foundation.RestoreRelocatedEntities();
            }

            if (m is PlayerMobile pm)
            {
                pm.m_GameTime += DateTime.UtcNow - pm.m_SessionStart;

                pm.m_Quest?.StopTimer();

                QuestHelper.StopTimer(pm);

                pm.m_SpeechLog = null;
                pm.LastOnline = DateTime.UtcNow;

                pm.AutoStablePets();
            }

            DisguiseTimers.StopTimer(m);

            BaseBoat.OnDisconnected(m);

            // Remove from public chat
            Channel.OnDisconnected(m);

            ShadowguardController.OnDisconnected(m);
        }

        private static void Disconnect(object state)
        {
            NetState ns = ((Mobile)state).NetState;

            ns?.Dispose();
        }

        public override void OnLogout(Mobile m)
        {
            base.OnLogout(m);

            PlayerMobile pm = m as PlayerMobile;

            if (pm == null)
            {
                return;
            }

            ChampionSpawnRegion.OnLogout(pm);
            BaseEscort.DeleteEscort(pm);
            BaseFamiliar.OnLogout(pm);
            BasketOfHerbs.CheckBonus(pm);
            InstanceRegion.OnLogout(pm);
            ScrollOfAlacrity.OnLogout(pm);
            Engines.PartySystem.Party.OnLogout(pm);
        }

        public override void RevealingAction()
        {
            if (m_DesignContext != null)
            {
                return;
            }

            InvisibilitySpell.RemoveTimer(this);

            base.RevealingAction();
        }

        public override void OnHiddenChanged()
        {
            base.OnHiddenChanged();

            RemoveBuff(BuffIcon.Invisibility);
            //Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

            if (!Hidden)
            {
                RemoveBuff(BuffIcon.HidingAndOrStealth);
            }
            else // if( !InvisibilitySpell.HasTimer( this ) )
            {
                BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655)); //Hidden/Stealthing & You Are Hidden
            }
        }

        public override void OnSubItemAdded(Item item)
        {
            if (AccessLevel < AccessLevel.GameMaster && item.IsChildOf(Backpack))
            {
                int curWeight = BodyWeight + TotalWeight;

                if (curWeight > MaxWeight)
                {
                    SendLocalizedMessage(1019035, true, $" : {curWeight} / {MaxWeight}");
                }
            }
        }

        public override void OnSubItemRemoved(Item item)
        {
            if (Engines.UOStore.UltimaStore.HasPendingItem(this))
                Timer.DelayCall(TimeSpan.FromSeconds(1.5), Engines.UOStore.UltimaStore.CheckPendingItem, this);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            // This will update aggressor for the aggressors master
            if (aggressor is BaseCreature creature && creature.ControlMaster != null && creature.ControlMaster != this)
            {
                Mobile aggressiveMaster = creature.ControlMaster;

                // First lets find out if the creatures master is in our aggressor list
                AggressorInfo info = null;

                for (var index = 0; index < Aggressors.Count; index++)
                {
                    var i = Aggressors[index];

                    if (i.Attacker == aggressiveMaster)
                    {
                        info = i;
                        break;
                    }
                }

                if (info != null)
                {
                    // already in the list, so we're refreshing it
                    info.Refresh();
                    info.CriminalAggression = criminal;
                }
                else
                {
                    // not in the list, so we're adding it
                    Aggressors.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));

                    if (CanSee(aggressiveMaster))
                    {
                        NetState?.Send(MobileIncoming.Create(NetState, this, aggressiveMaster));
                    }

                    UpdateAggrExpire();
                }

                // Now, if I am in the creatures master aggressor list, it needs to be refreshed
                info = null;

                for (var index = 0; index < aggressiveMaster.Aggressors.Count; index++)
                {
                    var i = aggressiveMaster.Aggressors[index];

                    if (i.Attacker == this)
                    {
                        info = i;
                        break;
                    }
                }

                info?.Refresh();

                info = null;

                for (var index = 0; index < Aggressed.Count; index++)
                {
                    var i = Aggressed[index];

                    if (i.Defender == aggressiveMaster)
                    {
                        info = i;
                        break;
                    }
                }

                info?.Refresh();

                // next lets find out if we're on the creatures master aggressed list
                info = null;

                for (var index = 0; index < aggressiveMaster.Aggressed.Count; index++)
                {
                    var i = aggressiveMaster.Aggressed[index];

                    if (i.Defender == this)
                    {
                        info = i;
                        break;
                    }
                }

                if (info != null)
                {
                    // already in the list, so we're refreshing it
                    info.Refresh();
                    info.CriminalAggression = criminal;
                }
                else
                {
                    // not in the list, so we're adding it
                    creature.Aggressed.Add(AggressorInfo.Create(aggressiveMaster, this, criminal));

                    if (CanSee(aggressiveMaster))
                    {
                        NetState?.Send(MobileIncoming.Create(NetState, this, aggressiveMaster));
                    }

                    UpdateAggrExpire();
                }

                if (aggressiveMaster is PlayerMobile || aggressiveMaster is BaseCreature bc && !bc.IsMonster)
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, this, true));
                    BuffInfo.AddBuff(aggressiveMaster, new BuffInfo(BuffIcon.HeatOfBattleStatus, 1153801, 1153827, Aggression.CombatHeatDelay, aggressiveMaster, true));
                }
            }

            base.AggressiveAction(aggressor, criminal);
        }

        public override void DoHarmful(IDamageable damageable, bool indirect)
        {
            base.DoHarmful(damageable, indirect);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == ViceVsVirtueSystem.Facet) && damageable is Mobile mobile)
            {
                ViceVsVirtueSystem.CheckHarmful(this, mobile);
            }
        }

        public override void DoBeneficial(Mobile target)
        {
            base.DoBeneficial(target);

            if (ViceVsVirtueSystem.Enabled && (ViceVsVirtueSystem.EnhancedRules || Map == ViceVsVirtueSystem.Facet) && target != null)
            {
                ViceVsVirtueSystem.CheckBeneficial(this, target);
            }
        }

        public override bool CanBeHarmful(IDamageable damageable, bool message, bool ignoreOurBlessedness, bool ignorePeaceCheck)
        {
            Mobile target = damageable as Mobile;

            if (m_DesignContext != null || target is PlayerMobile mobile && mobile.m_DesignContext != null)
            {
                return false;
            }

            if (target is BaseVendor vendor && vendor.IsInvulnerable || target is PlayerVendor || target is TownCrier)
            {
                if (message)
                {
                    if (target.Title == null)
                    {
                        SendMessage($"{target.Name} the vendor cannot be harmed.");
                    }
                    else
                    {
                        SendMessage($"{target.Name} {target.Title} cannot be harmed.");
                    }
                }

                return false;
            }

            if (damageable is IDamageableItem item && !item.CanDamage)
            {
                if (message)
                    SendMessage("That cannot be harmed.");

                return false;
            }

            return base.CanBeHarmful(damageable, message, ignoreOurBlessedness, ignorePeaceCheck);
        }

        public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
        {
            if (m_DesignContext != null || target is PlayerMobile pm && pm.m_DesignContext != null)
            {
                return false;
            }

            return base.CanBeBeneficial(target, message, allowDead);
        }

        public override bool CheckContextMenuDisplay(IEntity target)
        {
            return m_DesignContext == null;
        }

        public override void OnItemAdded(Item item)
        {
            base.OnItemAdded(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                Hits = Hits;
                Stam = Stam;
                Mana = Mana;
            }

            if (NetState != null)
            {
                CheckLightLevels(false);
            }
        }

        private BaseWeapon m_LastWeapon;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseWeapon LastWeapon { get => m_LastWeapon; set => m_LastWeapon = value; }

        public override void OnItemRemoved(Item item)
        {
            base.OnItemRemoved(item);

            if (item is BaseArmor || item is BaseWeapon)
            {
                Hits = Hits;
                Stam = Stam;
                Mana = Mana;
            }

            if (item is BaseWeapon weapon)
            {
                m_LastWeapon = weapon;
            }

            if (NetState != null)
            {
                CheckLightLevels(false);
            }
        }

        #region [Stats]Max
        [CommandProperty(AccessLevel.GameMaster)]
        public override int HitsMax
        {
            get
            {
                int strBase;
                int strOffs = GetStatOffset(StatType.Str);

                strBase = Str; //Str already includes GetStatOffset/str
                strOffs = AosAttributes.GetValue(this, AosAttribute.BonusHits);

                if (strOffs > 25 && IsPlayer())
                {
                    strOffs = 25;
                }

                if (AnimalForm.UnderTransformation(this, typeof(BakeKitsune)) ||
                    AnimalForm.UnderTransformation(this, typeof(GreyWolf)))
                {
                    strOffs += 20;
                }

                // Skill Masteries
                strOffs += ToughnessSpell.GetHPBonus(this);
                strOffs += InvigorateSpell.GetHPBonus(this);

                return strBase / 2 + 50 + strOffs;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int StamMax => base.StamMax + AosAttributes.GetValue(this, AosAttribute.BonusStam);

        [CommandProperty(AccessLevel.GameMaster)]
        public override int ManaMax => base.ManaMax + AosAttributes.GetValue(this, AosAttribute.BonusMana) + (Race == Race.Elf ? 20 : 0) + MasteryInfo.IntuitionBonus(this) + UraliTranceTonic.GetManaBuff(this);
        #endregion

        #region Stat Getters/Setters
        [CommandProperty(AccessLevel.GameMaster)]
        public override int Str
        {
            get
            {
                if (IsPlayer())
                {
                    return Math.Min(base.Str, StrMaxCap);
                }

                return base.Str;
            }
            set => base.Str = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Int
        {
            get
            {
                if (IsPlayer())
                {
                    return Math.Min(base.Int, IntMaxCap);
                }

                return base.Int;
            }
            set => base.Int = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override int Dex
        {
            get
            {
                if (IsPlayer())
                {
                    int dex = base.Dex;

                    return Math.Min(dex, DexMaxCap);
                }

                return base.Dex;
            }
            set => base.Dex = value;
        }
        #endregion

        public long NextPassiveDetectHidden { get; set; }

        public override bool Move(Direction d)
        {
            NetState ns = NetState;

            if (ns != null)
            {
                if (HasGump(typeof(ResurrectGump)))
                {
                    if (Alive)
                    {
                        CloseGump(typeof(ResurrectGump));
                    }
                    else
                    {
                        SendLocalizedMessage(500111); // You are frozen and cannot move.
                        return false;
                    }
                }
            }

            int speed = ComputeMovementSpeed(d);

            bool result = base.Move(d);

            if (result && !Siege.SiegeShard && Core.TickCount - NextPassiveDetectHidden >= 0)
            {
                DetectHidden.DoPassiveDetect(this);
                NextPassiveDetectHidden = Core.TickCount + (int)TimeSpan.FromSeconds(2).TotalMilliseconds;
            }

            m_NextMovementTime += speed;

            return result;
        }

        public override bool CheckMovement(Direction d, out int newZ)
        {
            DesignContext context = m_DesignContext;

            if (context == null)
            {
                bool check = base.CheckMovement(d, out newZ);

                if (check && VvVSigil.ExistsOn(this, true) && !VvVSigil.CheckMovement(this, d))
                {
                    SendLocalizedMessage(1155414); // You may not remove the sigil from the battle region!
                    return false;
                }

                return check;
            }

            HouseFoundation foundation = context.Foundation;

            newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int newX = X, newY = Y;
            Movement.Movement.Offset(d, ref newX, ref newY);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            return newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map;
        }

        public override void OnHitsChange(int oldValue)
        {
            if (Race == Race.Gargoyle)
            {
                if (Hits <= HitsMax / 2)
                {
                    BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.Berserk, 1080449, 1115021, $"{GetRacialBerserkBuff(false)}\t{GetRacialBerserkBuff(true)}", false));
                    Delta(MobileDelta.WeaponDamage);
                }
                else if (oldValue < Hits && Hits > HitsMax / 2)
                {
                    BuffInfo.RemoveBuff(this, BuffIcon.Berserk);
                    Delta(MobileDelta.WeaponDamage);
                }
            }

            base.OnHitsChange(oldValue);
        }

        /// <summary>
        /// Returns Racial Berserk value, for spell or melee
        /// </summary>
        /// <param name="spell">true for spell damage, false for damage increase (melee)</param>
        /// <returns></returns>
        public virtual int GetRacialBerserkBuff(bool spell)
        {
            if (Race != Race.Gargoyle || Hits > HitsMax / 2)
            {
                return 0;
            }

            double perc = Hits / (double)HitsMax * 100;

            int value = 0;

            perc = (100 - perc) / 20;

            if (perc > 4)
                value += spell ? 12 : 60;
            else if (perc >= 3)
                value += spell ? 9 : 45;
            else if (perc >= 2)
                value += spell ? 6 : 30;
            else if (perc >= 1)
                value += spell ? 3 : 15;

            return value;
        }

        public override void OnHeal(ref int amount, Mobile from)
        {
            base.OnHeal(ref amount, from);

            if (from == null)
                return;

            BestialSetHelper.OnHeal(this, from, ref amount);

            if (amount > 0 && from != this)
            {
                for (int i = Aggressed.Count - 1; i >= 0; i--)
                {
                    AggressorInfo info = Aggressed[i];

                    bool any = false;

                    for (var index = 0; index < info.Defender.DamageEntries.Count; index++)
                    {
                        var de = info.Defender.DamageEntries[index];

                        if (de.Damager == this)
                        {
                            any = true;
                            break;
                        }
                    }

                    if (info.Defender.InRange(Location, Core.GlobalMaxUpdateRange) && any)
                    {
                        info.Defender.RegisterDamage(amount, from);
                    }

                    if (info.Defender.Player && from.CanBeHarmful(info.Defender, false))
                    {
                        from.DoHarmful(info.Defender, true);
                    }
                }

                for (int i = Aggressors.Count - 1; i >= 0; i--)
                {
                    AggressorInfo info = Aggressors[i];

                    bool any = false;

                    for (var index = 0; index < info.Attacker.DamageEntries.Count; index++)
                    {
                        var de = info.Attacker.DamageEntries[index];

                        if (de.Damager == this)
                        {
                            any = true;
                            break;
                        }
                    }

                    if (info.Attacker.InRange(Location, Core.GlobalMaxUpdateRange) && any)
                    {
                        info.Attacker.RegisterDamage(amount, from);
                    }

                    if (info.Attacker.Player && from.CanBeHarmful(info.Attacker, false))
                    {
                        from.DoHarmful(info.Attacker, true);
                    }
                }
            }
        }

        public override bool AllowItemUse(Item item)
        {
            return DesignContext.Check(this);
        }

        public SkillName[] AnimalFormRestrictedSkills => m_AnimalFormRestrictedSkills;

        private readonly SkillName[] m_AnimalFormRestrictedSkills =
        {
            SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics, SkillName.Inscribe,
            SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking, SkillName.Provocation, SkillName.RemoveTrap,
            SkillName.SpiritSpeak, SkillName.Stealing, SkillName.TasteID
        };

        public override bool AllowSkillUse(SkillName skill)
        {
            if (AnimalForm.UnderTransformation(this))
            {
                for (int i = 0; i < m_AnimalFormRestrictedSkills.Length; i++)
                {
                    if (m_AnimalFormRestrictedSkills[i] == skill)
                    {
                        AnimalFormContext context = AnimalForm.GetContext(this);

                        if (skill == SkillName.Stealing && context.StealingMod != null && context.StealingMod.Value > 0)
                        {
                            continue;
                        }

                        SendLocalizedMessage(1070771); // You cannot use that skill in this form.
                        return false;
                    }
                }
            }

            return DesignContext.Check(this);
        }

        private bool m_LastProtectedMessage;
        private int m_NextProtectionCheck = 10;

        public virtual void RecheckTownProtection()
        {
            m_NextProtectionCheck = 10;

            GuardedRegion reg = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));

            bool isProtected = reg != null && !reg.IsDisabled();

            if (isProtected != m_LastProtectedMessage)
            {
                if (isProtected)
                {
                    SendLocalizedMessage(500112); // You are now under the protection of the town guards.
                }
                else
                {
                    SendLocalizedMessage(500113); // You have left the protection of the town guards.
                }

                m_LastProtectedMessage = isProtected;
            }
        }

        public override void MoveToWorld(Point3D loc, Map map)
        {
            base.MoveToWorld(loc, map);

            RecheckTownProtection();
        }

        public override void SetLocation(Point3D loc, bool isTeleport)
        {
            if (!isTeleport && IsPlayer() && !Flying)
            {
                // moving, not teleporting
                int zDrop = Location.Z - loc.Z;

                if (zDrop > 20) // we fell more than one story
                {
                    Hits -= zDrop / 20 * 10 - 5; // deal some damage; does not kill, disrupt, etc
                    SendMessage("Ouch!");
                }
            }

            base.SetLocation(loc, isTeleport);

            if (isTeleport || --m_NextProtectionCheck == 0)
            {
                RecheckTownProtection();
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            list.Add(new PaperdollEntry(this));

            if (from == this)
            {
                #region TOL Shadowguard
                if (ShadowguardController.GetInstance(Location, Map) != null)
                {
                    list.Add(new ExitEntry(this));
                }
                #endregion

                if (Alive)
                {
                    list.Add(new SearchVendors(this));
                }

                BaseHouse house = BaseHouse.FindHouseAt(this);

                if (house != null)
                {
                    if (house.IsCoOwner(this))
                    {
                        list.Add(new CallbackEntry(6205, ReleaseCoOwnership));
                    }
                }

                list.Add(new TitlesMenuEntry(this));

                if (Alive)
                {
                    list.Add(new Engines.Points.LoyaltyRating(this));
                }

                list.Add(new OpenBackpackEntry(this));

                if (Alive && InsuranceEnabled)
                {
                    list.Add(new CallbackEntry(1114299, OpenItemInsuranceMenu));
                    list.Add(new CallbackEntry(6201, ToggleItemInsurance));
                }
                else if (Siege.SiegeShard)
                {
                    list.Add(new CallbackEntry(3006168, SiegeBlessItem));
                }

                if (Alive)
                {
                    QuestHelper.GetContextMenuEntries(list);
                }

                m_Quest?.GetContextMenuEntries(list);

                if (house != null)
                {
                    if (Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
                    {
                        list.Add(new CallbackEntry(6204, GetVendor));
                    }

                    list.Add(new CallbackEntry(6207, LeaveHouse));
                }

                list.Add(new CallbackEntry(RefuseTrades ? 1154112 : 1154113, ToggleTrades)); // Allow Trades / Refuse Trades				

                if (m_JusticeProtectors.Count > 0)
                {
                    list.Add(new CallbackEntry(6157, CancelProtection));
                }

                #region Void Pool
                if (VoidPool || Region.IsPartOf<VoidPoolRegion>())
                {
                    VoidPoolController controller = Map == Map.Felucca ? VoidPoolController.InstanceFel : VoidPoolController.InstanceTram;

                    if (controller != null)
                    {
                        if (!VoidPool)
                        {
                            VoidPool = true;
                        }

                        list.Add(new VoidPoolInfo(this, controller));
                    }
                }
                #endregion

                if (DisabledPvpWarning)
                {
                    list.Add(new CallbackEntry(1113797, EnablePvpWarning));
                }
            }
            else
            {
                BaseGalleon galleon = BaseGalleon.FindGalleonAt(from.Location, from.Map);

                if (galleon != null && galleon.IsOwner(from))
                    list.Add(new ShipAccessEntry(this, from, galleon));

                if (Alive)
                {
                    Party theirParty = from.Party as Party;
                    Party ourParty = Party as Party;

                    if (theirParty == null && ourParty == null)
                    {
                        list.Add(new AddToPartyEntry(from, this));
                    }
                    else if (theirParty != null && theirParty.Leader == from)
                    {
                        if (ourParty == null)
                        {
                            list.Add(new AddToPartyEntry(from, this));
                        }
                        else if (ourParty == theirParty)
                        {
                            list.Add(new RemoveFromPartyEntry(from, this));
                        }
                    }
                }

                if (from.InRange(this, 10))
                {
                    list.Add(new CallbackEntry(1077728, () => OpenTrade(from))); // Trade
                }

                if (Alive && EjectPlayerEntry.CheckAccessible(from, this))
                {
                    list.Add(new EjectPlayerEntry(from, this));
                }
            }
        }

        private void CancelProtection()
        {
            for (int i = 0; i < m_JusticeProtectors.Count; ++i)
            {
                Mobile prot = m_JusticeProtectors[i];

                string args = $"{Name}\t{prot.Name}";

                prot.SendLocalizedMessage(1049371, args);
                // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
                SendLocalizedMessage(1049371, args);
                // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
            }

            m_JusticeProtectors.Clear();
        }

        #region Insurance
        private void ToggleItemInsurance()
        {
            if (!CheckAlive())
            {
                return;
            }

            BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
            SendLocalizedMessage(1060868); // Target the item you wish to toggle insurance status on <ESC> to cancel
        }

        private static bool CanInsure(Item item)
        {
            if (item is BaseQuiver && item.LootType == LootType.Regular)
            {
                return true;
            }

            if (item is Container && !(item is BaseQuiver) || item is BagOfSending || item is KeyRing || item is MountItem)
            {
                return false;
            }

            if (item is Spellbook && item.LootType == LootType.Blessed || item is Runebook || item is PotionKeg || item is VvVSigil)
            {
                return false;
            }

            if (item is BaseBalmOrLotion || item is GemOfSalvation || item is SeedOfLife || item is ManaDraught)
            {
                return false;
            }

            if (item.Stackable)
            {
                return false;
            }

            if (item.LootType == LootType.Cursed)
            {
                return false;
            }

            if (item.ItemID == 0x204E) // death shroud
            {
                return false;
            }

            if (item.LootType == LootType.Blessed)
            {
                return false;
            }

            return true;
        }

        private void ToggleItemInsurance_Callback(Mobile from, object obj)
        {
            if (!CheckAlive())
                return;

            ToggleItemInsurance_Callback(from, obj as Item, true);
        }

        private void ToggleItemInsurance_Callback(Mobile from, Item item, bool target)
        {
            if (item == null || !item.IsChildOf(this))
            {
                if (target)
                    BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);

                SendLocalizedMessage(1060871, "", 0x23); // You can only insure items that you have equipped or that are in your backpack
            }
            else if (item.Insured)
            {
                item.Insured = false;

                SendLocalizedMessage(1060874, "", 0x35); // You cancel the insurance on the item

                if (target)
                {
                    BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
                    SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
                }
            }
            else if (!CanInsure(item))
            {
                if (target)
                    BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);

                SendLocalizedMessage(1060869, "", 0x23); // You cannot insure that
            }
            else
            {
                if (!item.PayedInsurance)
                {
                    int cost = GetInsuranceCost(item);

                    if (Banker.Withdraw(from, cost))
                    {
                        SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                        item.PayedInsurance = true;
                    }
                    else
                    {
                        SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        return;
                    }
                }

                item.Insured = true;

                SendLocalizedMessage(1060873, "", 0x23); // You have insured the item

                if (target)
                {
                    BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
                    SendLocalizedMessage(1060868, "", 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
                }
            }
        }

        public int GetInsuranceCost(Item item)
        {
            int imbueWeight = Imbuing.GetTotalWeight(item, -1, false, false);
            int cost = 600; // this handles old items, set items, etc

            if (item is IVvVItem vItem && vItem.IsVvVItem)
                cost = 800;
            else if (imbueWeight > 0)
                cost = Math.Min(800, Math.Max(10, imbueWeight));
            else if (GenericBuyInfo.BuyPrices.ContainsKey(item.GetType()))
                cost = Math.Min(800, Math.Max(10, GenericBuyInfo.BuyPrices[item.GetType()]));
            else if (item.LootType == LootType.Newbied)
                cost = 10;

            NegativeAttributes negAttrs = RunicReforging.GetNegativeAttributes(item);

            if (negAttrs != null && negAttrs.Prized > 0)
                cost *= 2;

            if (Region != null)
                cost = (int)(cost * Region.InsuranceMultiplier);

            return cost;
        }

        private void AutoRenewInventoryInsurance()
        {
            if (!CheckAlive())
            {
                return;
            }

            SendLocalizedMessage(1060881, "", 0x23); // You have selected to automatically reinsure all insured items upon death
            AutoRenewInsurance = true;
        }

        #region Siege Bless Item
        private Item _BlessedItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public Item BlessedItem { get => _BlessedItem; set => _BlessedItem = value; }

        private void SiegeBlessItem()
        {
            if (_BlessedItem != null && _BlessedItem.Deleted)
                _BlessedItem = null;

            BeginTarget(2, false, TargetFlags.None, (from, targeted) =>
            {
                Siege.TryBlessItem(this, targeted);
            });
        }

        public override bool Drop(Point3D loc)
        {
            if (!Siege.SiegeShard || _BlessedItem == null)
                return base.Drop(loc);

            Item item = Holding;
            bool drop = base.Drop(loc);

            if (item != null && drop && item.Parent == null && _BlessedItem != null && _BlessedItem == item)
            {
                _BlessedItem = null;
                item.LootType = LootType.Regular;

                SendLocalizedMessage(1075292, item.Name != null ? item.Name : "#" + item.LabelNumber); // ~1_NAME~ has been unblessed.
            }

            return drop;
        }
        #endregion

        private class CancelRenewInventoryInsuranceGump : Gump
        {
            private readonly PlayerMobile m_Player;
            private readonly ItemInsuranceMenuGump m_InsuranceGump;

            public CancelRenewInventoryInsuranceGump(PlayerMobile player, ItemInsuranceMenuGump insuranceGump)
                : base(250, 200)
            {
                m_Player = player;
                m_InsuranceGump = insuranceGump;

                AddBackground(0, 0, 240, 142, 0x13BE);
                AddImageTiled(6, 6, 228, 100, 0xA40);
                AddImageTiled(6, 116, 228, 20, 0xA40);
                AddAlphaRegion(6, 6, 228, 142);

                AddHtmlLocalized(8, 8, 228, 100, 1071021, 0x7FFF, false, false);
                // You are about to disable inventory insurance auto-renewal.

                AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

                AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(148, 118, 450, 20, 1071022, 0x7FFF, false, false); // DISABLE IT!
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (!m_Player.CheckAlive())
                {
                    return;
                }

                if (info.ButtonID == 1)
                {
                    m_Player.SendLocalizedMessage(1061075, "", 0x23);
                    // You have cancelled automatically reinsuring all insured items upon death
                    m_Player.AutoRenewInsurance = false;
                }
                else
                {
                    m_Player.SendLocalizedMessage(1042021); // Cancelled.
                }

                if (m_InsuranceGump != null)
                    m_Player.SendGump(m_InsuranceGump.NewInstance());
            }
        }

        private void OpenItemInsuranceMenu()
        {
            if (!CheckAlive())
            {
                return;
            }

            List<Item> items = new List<Item>();

            for (var index = 0; index < Items.Count; index++)
            {
                Item item = Items[index];

                if (DisplayInItemInsuranceGump(item))
                {
                    items.Add(item);
                }
            }

            Container pack = Backpack;

            if (pack != null)
            {
                items.AddRange(pack.FindItemsByType<Item>(true, DisplayInItemInsuranceGump));
            }

            CloseGump(typeof(ItemInsuranceMenuGump));

            if (items.Count == 0)
            {
                SendLocalizedMessage(1114915, "", 0x35); // None of your current items meet the requirements for insurance.
            }
            else
            {
                SendGump(new ItemInsuranceMenuGump(this, items.ToArray()));
            }
        }

        private bool DisplayInItemInsuranceGump(Item item)
        {
            if (item.Parent is LockableContainer container && container.Locked)
            {
                return false;
            }

            return (item.Visible || AccessLevel >= AccessLevel.GameMaster) && (item.Insured || CanInsure(item));
        }

        private class ItemInsuranceMenuGump : Gump
        {
            private readonly PlayerMobile m_From;
            private readonly Item[] m_Items;
            private readonly bool[] m_Insure;
            private readonly int m_Page;

            public ItemInsuranceMenuGump(PlayerMobile from, Item[] items)
                : this(from, items, null, 0)
            {
            }

            public ItemInsuranceMenuGump(PlayerMobile from, Item[] items, bool[] insure, int page)
                : base(25, 50)
            {
                m_From = from;
                m_Items = items;

                if (insure == null)
                {
                    insure = new bool[items.Length];

                    for (int i = 0; i < items.Length; ++i)
                        insure[i] = items[i].Insured;
                }

                m_Insure = insure;
                m_Page = page;

                AddPage(0);

                AddBackground(0, 0, 520, 510, 0x13BE);
                AddImageTiled(10, 10, 500, 30, 0xA40);
                AddImageTiled(10, 50, 500, 355, 0xA40);
                AddImageTiled(10, 415, 500, 80, 0xA40);
                AddAlphaRegion(10, 10, 500, 485);

                AddButton(15, 470, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, 472, 80, 20, 1011012, 0x7FFF, false, false); // CANCEL

                if (from.AutoRenewInsurance)
                    AddButton(360, 10, 9723, 9724, 1, GumpButtonType.Reply, 0);
                else
                    AddButton(360, 10, 9720, 9722, 1, GumpButtonType.Reply, 0);

                AddHtmlLocalized(395, 14, 105, 20, 1114122, 0x7FFF, false, false); // AUTO REINSURE

                AddButton(395, 470, 0xFA5, 0xFA6, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(430, 472, 50, 20, 1006044, 0x7FFF, false, false); // OK

                AddHtmlLocalized(10, 14, 150, 20, 1114121, 0x7FFF, false, false); // <CENTER>ITEM INSURANCE MENU</CENTER>

                AddHtmlLocalized(45, 54, 70, 20, 1062214, 0x7FFF, false, false); // Item
                AddHtmlLocalized(250, 54, 70, 20, 1061038, 0x7FFF, false, false); // Cost
                AddHtmlLocalized(400, 54, 70, 20, 1114311, 0x7FFF, false, false); // Insured

                int balance = Banker.GetBalance(from);
                int cost = 0;

                for (int i = 0; i < items.Length; ++i)
                {
                    if (insure[i])
                        cost += m_From.GetInsuranceCost(items[i]);
                }

                AddHtmlLocalized(15, 420, 300, 20, 1114310, 0x7FFF, false, false); // GOLD AVAILABLE:
                AddLabel(215, 420, 0x481, balance.ToString());
                AddHtmlLocalized(15, 435, 300, 20, 1114123, 0x7FFF, false, false); // TOTAL COST OF INSURANCE:
                AddLabel(215, 435, 0x481, cost.ToString());

                if (cost != 0)
                {
                    AddHtmlLocalized(15, 450, 300, 20, 1114125, 0x7FFF, false, false); // NUMBER OF DEATHS PAYABLE:
                    AddLabel(215, 450, 0x481, (balance / cost).ToString());
                }

                for (int i = page * 4, y = 72; i < (page + 1) * 4 && i < items.Length; ++i, y += 75)
                {
                    Item item = items[i];
                    Rectangle2D b = ItemBounds.Table[item.ItemID];

                    AddImageTiledButton(40, y, 0x918, 0x918, 0, GumpButtonType.Page, 0, item.ItemID, item.Hue, 40 - b.Width / 2 - b.X, 30 - b.Height / 2 - b.Y);
                    AddItemProperty(item.Serial);

                    if (insure[i])
                    {
                        AddButton(400, y, 9723, 9724, 100 + i, GumpButtonType.Reply, 0);
                        AddLabel(250, y, 0x481, m_From.GetInsuranceCost(item).ToString());
                    }
                    else
                    {
                        AddButton(400, y, 9720, 9722, 100 + i, GumpButtonType.Reply, 0);
                        AddLabel(250, y, 0x66C, m_From.GetInsuranceCost(item).ToString());
                    }
                }

                if (page >= 1)
                {
                    AddButton(15, 380, 0xFAE, 0xFAF, 3, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(50, 380, 450, 20, 1044044, 0x7FFF, false, false); // PREV PAGE
                }

                if ((page + 1) * 4 < items.Length)
                {
                    AddButton(400, 380, 0xFA5, 0xFA7, 4, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(435, 380, 70, 20, 1044045, 0x7FFF, false, false); // NEXT PAGE
                }
            }

            public ItemInsuranceMenuGump NewInstance()
            {
                return new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page);
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (info.ButtonID == 0 || !m_From.CheckAlive())
                    return;

                switch (info.ButtonID)
                {
                    case 1: // Auto Reinsure
                        {
                            if (m_From.AutoRenewInsurance)
                            {
                                if (!m_From.HasGump(typeof(CancelRenewInventoryInsuranceGump)))
                                    m_From.SendGump(new CancelRenewInventoryInsuranceGump(m_From, this));
                            }
                            else
                            {
                                m_From.AutoRenewInventoryInsurance();
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
                            }

                            break;
                        }
                    case 2: // OK
                        {
                            m_From.SendGump(new ItemInsuranceMenuConfirmGump(m_From, m_Items, m_Insure, m_Page));

                            break;
                        }
                    case 3: // Prev
                        {
                            if (m_Page >= 1)
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page - 1));

                            break;
                        }
                    case 4: // Next
                        {
                            if ((m_Page + 1) * 4 < m_Items.Length)
                                m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page + 1));

                            break;
                        }
                    default:
                        {
                            int idx = info.ButtonID - 100;

                            if (idx >= 0 && idx < m_Items.Length)
                                m_Insure[idx] = !m_Insure[idx];

                            m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));

                            break;
                        }
                }
            }
        }

        private class ItemInsuranceMenuConfirmGump : Gump
        {
            private readonly PlayerMobile m_From;
            private readonly Item[] m_Items;
            private readonly bool[] m_Insure;
            private readonly int m_Page;

            public ItemInsuranceMenuConfirmGump(PlayerMobile from, Item[] items, bool[] insure, int page)
                : base(250, 200)
            {
                m_From = from;
                m_Items = items;
                m_Insure = insure;
                m_Page = page;

                AddBackground(0, 0, 240, 142, 0x13BE);
                AddImageTiled(6, 6, 228, 100, 0xA40);
                AddImageTiled(6, 116, 228, 20, 0xA40);
                AddAlphaRegion(6, 6, 228, 142);

                AddHtmlLocalized(8, 8, 228, 100, 1114300, 0x7FFF, false, false); // Do you wish to insure all newly selected items?

                AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

                AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(148, 118, 450, 20, 1073996, 0x7FFF, false, false); // ACCEPT
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (!m_From.CheckAlive())
                    return;

                if (info.ButtonID == 1)
                {
                    for (int i = 0; i < m_Items.Length; ++i)
                    {
                        Item item = m_Items[i];

                        if (item.Insured != m_Insure[i])
                            m_From.ToggleItemInsurance_Callback(m_From, item, false);
                    }
                }
                else
                {
                    m_From.SendLocalizedMessage(1042021); // Cancelled.
                    m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
                }
            }
        }

        #endregion

        private void ToggleTrades()
        {
            RefuseTrades = !RefuseTrades;
        }

        private void GetVendor()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (CheckAlive() && house != null && house.IsOwner(this) && house.InternalizedVendors.Count > 0)
            {
                CloseGump(typeof(ReclaimVendorGump));
                SendGump(new ReclaimVendorGump(house));
            }
        }

        private void LeaveHouse()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null)
            {
                Location = house.BanLocation;
            }
        }

        private void ReleaseCoOwnership()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsCoOwner(this))
            {
                SendGump(new WarningGump(1060635, 30720, 1062006, 32512, 420, 280, ClearCoOwners_Callback, house));
            }
        }

        public static void ClearCoOwners_Callback(Mobile from, bool okay, object state)
        {
            BaseHouse house = (BaseHouse)state;

            if (house.Deleted)
            {
                return;
            }

            if (okay && house.IsCoOwner(from))
            {
                house.CoOwners?.Remove(from);
                from.SendLocalizedMessage(501300); // You have been removed as a house co-owner.
            }
        }

        private void EnablePvpWarning()
        {
            DisabledPvpWarning = false;
            SendLocalizedMessage(1113798); // Your PvP warning query has been re-enabled.
        }

        private delegate void ContextCallback();

        private class CallbackEntry : ContextMenuEntry
        {
            private readonly ContextCallback m_Callback;

            public CallbackEntry(int number, ContextCallback callback)
                : this(number, -1, callback)
            { }

            public CallbackEntry(int number, int range, ContextCallback callback)
                : base(number, range)
            {
                m_Callback = callback;
            }

            public override void OnClick()
            {
                m_Callback?.Invoke();
            }
        }

        public override void DisruptiveAction()
        {
            if (Meditating)
            {
                RemoveBuff(BuffIcon.ActiveMeditation);
            }

            base.DisruptiveAction();
        }

        public override bool Meditating
        {
            set
            {
                base.Meditating = value;
                if (value == false)
                {
                    RemoveBuff(BuffIcon.ActiveMeditation);
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (this == from && !Warmode)
            {
                IMount mount = Mount;

                if (mount != null && !DesignContext.Check(this))
                {
                    return;
                }
            }

            base.OnDoubleClick(from);
        }

        public override void DisplayPaperdollTo(Mobile to)
        {
            if (DesignContext.Check(this))
            {
                base.DisplayPaperdollTo(to);
            }
        }

        private static bool m_NoRecursion;

        public override bool CheckEquip(Item item)
        {
            if (!base.CheckEquip(item))
            {
                return false;
            }

            Region r = Region.Find(Location, Map);

            if (r is ArenaRegion region && !region.AllowItemEquip(this, item))
            {
                return false;
            }

            #region Vice Vs Virtue
            if (item is IVvVItem vvvItem && vvvItem.IsVvVItem && !ViceVsVirtueSystem.IsVvV(this))
            {
                return false;
            }
            #endregion

            if (AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && HasTrade)
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null)
                {
                    if (bounce.m_Parent is Item parent)
                    {
                        if (parent == Backpack || parent.IsChildOf(Backpack))
                        {
                            return true;
                        }
                    }
                    else if (bounce.m_Parent == this)
                    {
                        return true;
                    }
                }

                SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
                return false;
            }

            return true;
        }

        public override bool OnDragLift(Item item)
        {
            if (item is IPromotionalToken token && token.GumpType != null)
            {
                Type t = token.GumpType;

                if (HasGump(t))
                    CloseGump(t);
            }

            return base.OnDragLift(item);
        }

        public override bool CheckTrade(
            Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            int msgNum = 0;

            if (_BlessedItem != null && _BlessedItem == item)
            {
                msgNum = 1075282; // You cannot trade a blessed item.
            }

            if (msgNum == 0 && cont == null)
            {
                if (to.Holding != null)
                {
                    msgNum = 1062727; // You cannot trade with someone who is dragging something.
                }
                else if (HasTrade)
                {
                    msgNum = 1062781; // You are already trading with someone else!
                }
                else if (to.HasTrade)
                {
                    msgNum = 1062779; // That person is already involved in a trade
                }
                else if (to is PlayerMobile pm && pm.RefuseTrades)
                {
                    msgNum = 1154111; // ~1_NAME~ is refusing all trades.
                }
            }

            if (msgNum == 0 && item != null)
            {
                if (cont != null)
                {
                    plusItems += cont.TotalItems;
                    plusWeight += cont.TotalWeight;
                }

                if (Backpack == null || !Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
                {
                    msgNum = 1004040; // You would not be able to hold this if the trade failed.
                }
                else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
                {
                    msgNum = 1004039; // The recipient of this trade would not be able to carry 
                }
                else
                {
                    msgNum = CheckContentForTrade(item);
                }
            }

            if (msgNum == 0)
            {
                return true;
            }

            if (!message)
            {
                return false;
            }

            if (msgNum == 1154111)
            {
                if (to != null)
                {
                    SendLocalizedMessage(msgNum, to.Name);
                }
            }
            else
            {
                SendLocalizedMessage(msgNum);
            }

            return false;
        }

        private static int CheckContentForTrade(Item item)
        {
            if (item is TrapableContainer container && container.TrapType != TrapType.None)
            {
                return 1004044; // You may not trade trapped items.
            }

            if (StolenItem.IsStolen(item))
            {
                return 1004043; // You may not trade recently stolen items.
            }

            if (item is Container)
            {
                for (var index = 0; index < item.Items.Count; index++)
                {
                    Item subItem = item.Items[index];

                    int msg = CheckContentForTrade(subItem);

                    if (msg != 0)
                    {
                        return msg;
                    }
                }
            }

            return 0;
        }

        public override bool CheckHasTradeDrop(Mobile from, Item item, Item target)
        {
            if (!base.CheckHasTradeDrop(from, item, target))
            {
                return false;
            }

            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                return true;
            }

            Container pack = Backpack;
            if (from == this && HasTrade && (target == pack || target.IsChildOf(pack)))
            {
                BounceInfo bounce = item.GetBounce();

                if (bounce != null && bounce.m_Parent is Item pItem && pItem == pack)
                {
                    if (pItem.IsChildOf(pack))
                    {
                        return true;
                    }
                }

                SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
                return false;
            }

            return true;
        }

        protected override void OnLocationChange(Point3D oldLocation)
        {
            CheckLightLevels(false);

            DesignContext context = m_DesignContext;

            if (context == null || m_NoRecursion)
            {
                return;
            }

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            int newX = X, newY = Y;
            int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

            int startX = foundation.X + foundation.Components.Min.X + 1;
            int startY = foundation.Y + foundation.Components.Min.Y + 1;
            int endX = startX + foundation.Components.Width - 1;
            int endY = startY + foundation.Components.Height - 2;

            if (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map)
            {
                if (Z != newZ)
                {
                    Location = new Point3D(X, Y, newZ);
                }

                m_NoRecursion = false;
                return;
            }

            Location = new Point3D(foundation.X, foundation.Y, newZ);
            Map = foundation.Map;

            m_NoRecursion = false;
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (m is BaseCreature bc && !bc.Controlled)
            {
                return !Alive || !bc.Alive || IsDeadBondedPet || bc.IsDeadBondedPet || Hidden && IsStaff();
            }

            return base.OnMoveOver(m);
        }

        public override bool CheckShove(Mobile shoved)
        {
            if (TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
            {
                return true;
            }

            return base.CheckShove(shoved);
        }

        protected override void OnMapChange(Map oldMap)
        {
            ViceVsVirtueSystem.OnMapChange(this);

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.OnMapChange(this, oldMap);
            }

            if (Map != ViceVsVirtueSystem.Facet && oldMap == ViceVsVirtueSystem.Facet || Map == ViceVsVirtueSystem.Facet && oldMap != ViceVsVirtueSystem.Facet)
            {
                InvalidateProperties();
            }

            BaseGump.CheckCloseGumps(this);

            DesignContext context = m_DesignContext;

            if (context == null || m_NoRecursion)
            {
                return;
            }

            m_NoRecursion = true;

            HouseFoundation foundation = context.Foundation;

            if (Map != foundation.Map)
            {
                Map = foundation.Map;
            }

            m_NoRecursion = false;
        }

        public override void OnBeneficialAction(Mobile target, bool isCriminal)
        {
            m_SentHonorContext?.OnSourceBeneficialAction(target);

            if (Siege.SiegeShard && isCriminal)
            {
                Criminal = true;
                return;
            }

            base.OnBeneficialAction(target, isCriminal);
        }

        public override bool IsBeneficialCriminal(Mobile target)
        {
            if (!target.Criminal && target is BaseCreature bc && bc.GetMaster() == this)
                return false;

            return base.IsBeneficialCriminal(target);
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            int disruptThreshold;

            if (from != null && from.Player)
            {
                disruptThreshold = 19;
            }
            else
            {
                disruptThreshold = 26;
            }

            disruptThreshold += Dex / 12;

            if (amount > disruptThreshold)
            {
                BandageContext c = BandageContext.GetContext(this);

                c?.Slip();
            }

            if (Confidence.IsRegenerating(this))
            {
                Confidence.StopRegenerating(this);
            }

            m_ReceivedHonorContext?.OnTargetDamaged(from, amount);

            m_SentHonorContext?.OnSourceDamaged(from, amount);

            if (willKill && from is PlayerMobile pm)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), pm.RecoverAmmo);
            }

            if (InvisibilityPotion.HasTimer(this))
            {
                InvisibilityPotion.Iterrupt(this);
            }

            UndertakersStaff.TryRemoveTimer(this);

            base.OnDamage(amount, from, willKill);
        }

        public override void Resurrect()
        {
            bool wasAlive = Alive;

            base.Resurrect();

            if (Alive && !wasAlive)
            {
                Item deathRobe = new DeathRobe();

                if (!EquipItem(deathRobe))
                {
                    deathRobe.Delete();
                }

                if (NetState != null)
                {
                    Waypoints.RemoveHealers(this, Map);
                }

                ScrollOfAlacrity.StartTimer(this);
            }
        }

        public override double RacialSkillBonus
        {
            get
            {
                if (Race == Race.Human)
                {
                    return 20.0;
                }

                return 0;
            }
        }

        public override double GetRacialSkillBonus(SkillName skill)
        {
            if (Race == Race.Human)
                return 20.0;

            if (Race == Race.Gargoyle)
            {
                if (skill == SkillName.Imbuing)
                    return 30.0;

                if (skill == SkillName.Throwing)
                    return 20.0;
            }

            return RacialSkillBonus;
        }

        public override void OnWarmodeChanged()
        {
            if (!Warmode)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), RecoverAmmo);
            }
        }

        private Mobile m_InsuranceAward;
        private int m_InsuranceCost;
        private int m_InsuranceBonus;

        private List<Item> m_EquipSnapshot;

        public List<Item> EquipSnapshot => m_EquipSnapshot;

        private bool FindItems_Callback(Item item)
        {
            if (item is Runebook && item.Parent is RunebookStrap || item is Spellbook && item.Parent is SpellbookStrap)
            {
                return false;
            }

            if (!item.Deleted && (item.LootType == LootType.Blessed || item.Insured))
            {
                if (Backpack != item.Parent)
                {
                    return true;
                }
            }

            return false;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Criminal
        {
            get => base.Criminal;
            set
            {
                bool crim = base.Criminal;
                base.Criminal = value;

                if (value != crim)
                {
                    if (value)
                        BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.CriminalStatus, 1153802, 1153828));
                    else
                        BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);
                }
            }
        }

        public override bool OnBeforeDeath()
        {
            NetState state = NetState;

            state?.CancelAllTrades();

            if (Criminal)
            {
                BuffInfo.RemoveBuff(this, BuffIcon.CriminalStatus);
            }

            DropHolding();

            if (Backpack != null && !Backpack.Deleted)
            {
                List<Item> ilist = Backpack.FindItemsByType<Item>(FindItems_Callback);

                for (int i = 0; i < ilist.Count; i++)
                {
                    Backpack.AddItem(ilist[i]);
                }
            }

            m_EquipSnapshot = new List<Item>(Items);

            m_NonAutoreinsuredItems = 0;
            m_InsuranceCost = 0;
            m_InsuranceAward = FindMostRecentDamager(false);

            if (m_InsuranceAward is BaseCreature bc)
            {
                Mobile master = bc.GetMaster();

                if (master != null)
                {
                    m_InsuranceAward = master;
                }
            }

            if (m_InsuranceAward != null && (!m_InsuranceAward.Player || m_InsuranceAward == this))
            {
                m_InsuranceAward = null;
            }

            if (m_InsuranceAward is PlayerMobile pm)
            {
                pm.m_InsuranceBonus = 0;
            }

            m_ReceivedHonorContext?.OnTargetKilled();

            m_SentHonorContext?.OnSourceKilled();

            RecoverAmmo();

            if (NetState != null && NetState.IsEnhancedClient)
            {
                Waypoints.AddCorpse(this);
            }

            return base.OnBeforeDeath();
        }

        private bool CheckInsuranceOnDeath(Item item)
        {
            if (Young)
                return false;

            if (InsuranceEnabled && item.Insured)
            {
                int insuredAmount = GetInsuranceCost(item);

                if (AutoRenewInsurance)
                {
                    int cost = m_InsuranceAward == null ? insuredAmount : insuredAmount / 2;

                    if (Banker.Withdraw(this, cost))
                    {
                        m_InsuranceCost += cost;
                        item.PayedInsurance = true;
                        SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                    }
                    else
                    {
                        SendLocalizedMessage(1061079, "", 0x23); // You lack the funds to purchase the insurance
                        item.PayedInsurance = false;
                        item.Insured = false;
                        m_NonAutoreinsuredItems++;
                    }
                }
                else
                {
                    item.PayedInsurance = false;
                    item.Insured = false;
                }

                if (m_InsuranceAward != null)
                {
                    if (Banker.Deposit(m_InsuranceAward, insuredAmount / 2) && m_InsuranceAward is PlayerMobile pm)
                    {
                        pm.m_InsuranceBonus += insuredAmount / 2;
                    }
                }

                return true;
            }

            return false;
        }

        public override DeathMoveResult GetParentMoveResultFor(Item item)
        {
            if (CheckInsuranceOnDeath(item) && !Young)
            {
                return DeathMoveResult.MoveToBackpack;
            }

            DeathMoveResult res = base.GetParentMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
            {
                res = DeathMoveResult.MoveToBackpack;
            }

            return res;
        }

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            if (CheckInsuranceOnDeath(item) && !Young)
            {
                return DeathMoveResult.MoveToBackpack;
            }

            DeathMoveResult res = base.GetInventoryMoveResultFor(item);

            if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
            {
                res = DeathMoveResult.MoveToBackpack;
            }

            return res;
        }

        public override void OnDeath(Container c)
        {
            if (NetState != null)
            {
                Waypoints.OnDeath(this);
            }

            Mobile m = FindMostRecentDamager(false);
            PlayerMobile killer = m as PlayerMobile;

            if (killer == null && m is BaseCreature bc)
            {
                killer = bc.GetMaster() as PlayerMobile;
            }

            if (m_NonAutoreinsuredItems > 0)
            {
                SendLocalizedMessage(1061115);
            }

            base.OnDeath(c);

            m_EquipSnapshot = null;

            HueMod = -1;
            NameMod = null;
            SavagePaintExpiration = TimeSpan.Zero;

            SetHairMods(-1, -1);

            PolymorphSpell.StopTimer(this);
            IncognitoSpell.StopTimer(this);
            ScrollOfAlacrity.StopTimer(this);
            DisguiseTimers.RemoveTimer(this);

            WeakenSpell.RemoveEffects(this);
            ClumsySpell.RemoveEffects(this);
            FeeblemindSpell.RemoveEffects(this);
            CurseSpell.RemoveEffect(this);
            StrangleSpell.RemoveCurse(this);
            Spells.Second.ProtectionSpell.EndProtection(this);

            BaseFishPie.RemoveEffects(this);

            EndAction(typeof(PolymorphSpell));
            EndAction(typeof(IncognitoSpell));

            MeerMage.StopEffect(this, false);

            BaseEscort.DeleteEscort(this);

            if (Flying)
            {
                Flying = false;
                BuffInfo.RemoveBuff(this, BuffIcon.Fly);
            }

            StolenItem.ReturnOnDeath(this, c);

            if (m_PermaFlags.Count > 0)
            {
                m_PermaFlags.Clear();

                if (c is Corpse corpse)
                {
                    corpse.Criminal = true;
                }

                if (Stealing.ClassicMode)
                {
                    Criminal = true;
                }
            }

            Aggression.OnPlayerDeath(this);
            BaseBoat.OnPlayerDeath(this);
            EodonianPotion.OnPlayerDeath(this);
            GemOfSalvation.OnPlayerDeath(this);
            GiftOfLifeSpell.HandleDeath(this);
            KhaldunRevenant.OnPlayerDeath(this);
            ReportMurdererGump.OnPlayerDeath(this);
            ViceVsVirtueSystem.OnPlayerDeath(this);

            if (DateTime.UtcNow >= HalloweenSettings.StartHalloween && DateTime.UtcNow <= HalloweenSettings.FinishHalloween)
            {
                HalloweenHauntings.OnPlayerDeath(this);
            }

            Engines.PartySystem.Party.OnPlayerDeath(this);

            if (killer != null && Murderer && DateTime.UtcNow >= killer.m_NextJustAward)
            {
                // This scales 700.0 skill points to 1000 valor points
                int pointsToGain = SkillsTotal / 7;

                // This scales 700.0 skill points to 7 minutes wait
                int minutesToWait = Math.Max(1, SkillsTotal / 1000);

                bool gainedPath = false;

                if (VirtueHelper.Award(m, VirtueName.Justice, pointsToGain, ref gainedPath))
                {
                    if (gainedPath)
                    {
                        m.SendLocalizedMessage(1049367); // You have gained a path in Justice!
                    }
                    else
                    {
                        m.SendLocalizedMessage(1049363); // You have gained in Justice.
                    }

                    m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
                    m.PlaySound(0x1F7);

                    killer.m_NextJustAward = DateTime.UtcNow + TimeSpan.FromMinutes(minutesToWait);
                }
            }

            if (m_InsuranceAward is PlayerMobile pm && pm.m_InsuranceBonus > 0)
            {
                pm.SendLocalizedMessage(1060397, pm.m_InsuranceBonus.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
            }

            if (Young)
            {
                if (YoungDeathTeleport())
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(2.5), SendYoungDeathNotice);
                }
            }

            Guilds.Guild.HandleDeath(this, killer);

            if (m_BuffTable != null)
            {
                List<BuffInfo> list = new List<BuffInfo>();

                foreach (BuffInfo buff in m_BuffTable.Values)
                {
                    if (!buff.RetainThroughDeath)
                    {
                        list.Add(buff);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    RemoveBuff(list[i]);
                }
            }

            if (Region.IsPartOf("Abyss") && SSSeedExpire > DateTime.UtcNow)
            {
                SendGump(new ResurrectGump(this, ResurrectMessage.SilverSapling));
            }

            if (LastKiller is BaseVoidCreature vc)
            {
                vc.Mutate(VoidEvolution.Killing);
            }
        }

        private List<Mobile> m_PermaFlags;
        private readonly List<Mobile> m_VisList;
        private TimeSpan m_GameTime;
        private TimeSpan m_ShortTermElapse;
        private TimeSpan m_LongTermElapse;
        private DateTime m_SessionStart;
        private DateTime m_SavagePaintExpiration;
        private SkillName m_Learning = (SkillName)(-1);

        public SkillName Learning { get => m_Learning; set => m_Learning = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan SavagePaintExpiration
        {
            get
            {
                TimeSpan ts = m_SavagePaintExpiration - DateTime.UtcNow;

                if (ts < TimeSpan.Zero)
                {
                    ts = TimeSpan.Zero;
                }

                return ts;
            }
            set => m_SavagePaintExpiration = DateTime.UtcNow + value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextSmithBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Smith, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Smith, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextTailorBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Tailor, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Tailor, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextAlchemyBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Alchemy, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Alchemy, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextInscriptionBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Inscription, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Inscription, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextTinkeringBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Tinkering, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Tinkering, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextFletchingBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Fletching, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Fletching, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextCarpentryBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Carpentry, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Carpentry, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan NextCookingBulkOrder
        {
            get => BulkOrderSystem.GetNextBulkOrder(BODType.Cooking, this);
            set => BulkOrderSystem.SetNextBulkOrder(BODType.Cooking, this, value);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastEscortTime { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime LastPetBallTime { get; set; }

        public PlayerMobile()
        {
            Instances.Add(this);

            m_AutoStabled = new List<Mobile>();

            m_DoneQuests = new List<QuestRestartInfo>();
            m_Collections = new Dictionary<Collection, int>();
            m_RewardTitles = new List<object>();

            m_VisList = new List<Mobile>();
            m_PermaFlags = new List<Mobile>();
            m_RecentlyReported = new List<Mobile>();

            m_GameTime = TimeSpan.Zero;
            m_ShortTermElapse = TimeSpan.FromHours(8.0);
            m_LongTermElapse = TimeSpan.FromHours(40.0);

            m_JusticeProtectors = new List<Mobile>();
            m_GuildRank = RankDefinition.Lowest;

            m_ChampionTitles = new ChampionTitleInfo();
        }

        public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
        {
            if (Alive)
            {
                return false;
            }

            if (Skills[SkillName.SpiritSpeak].Value >= 100.0)
            {
                return false;
            }

            for (int i = 0; i < hears.Count; ++i)
            {
                Mobile m = hears[i];

                if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
                {
                    return false;
                }
            }

            return base.MutateSpeech(hears, ref text, ref context);
        }

        public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
        {
            if (type == MessageType.Guild || type == MessageType.Alliance)
            {
                Guild g = Guild as Guild;
                if (g == null)
                {
                    SendLocalizedMessage(1063142); // You are not in a guild!
                }
                else if (type == MessageType.Alliance)
                {
                    if (g.Alliance != null && g.Alliance.IsMember(g))
                    {
                        g.Alliance.AllianceChat(this, text);
                        SendToStaffMessage(this, $"[Alliance]: {text}");

                        m_AllianceMessageHue = hue;
                    }
                    else
                    {
                        SendLocalizedMessage(1071020); // You are not in an alliance!
                    }
                }
                else //Type == MessageType.Guild
                {
                    m_GuildMessageHue = hue;

                    g.GuildChat(this, text);
                    SendToStaffMessage(this, $"[Guild]: {text}");
                }
            }
            else
            {
                base.DoSpeech(text, keywords, type, hue);
            }
        }

        private static void SendToStaffMessage(Mobile from, string text)
        {
            Packet p = null;

            foreach (NetState ns in from.GetClientsInRange(8))
            {
                Mobile mob = ns.Mobile;

                if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > from.AccessLevel)
                {
                    if (p == null)
                    {
                        p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language, from.Name, text));
                    }

                    ns.Send(p);
                }
            }

            Packet.Release(p);
        }

        private static void SendToStaffMessage(Mobile from, string format, params object[] args)
        {
            SendToStaffMessage(from, string.Format(format, args));
        }

        #region Poison
        public override void OnCured(Mobile from, Poison oldPoison)
        {
            BuffInfo.RemoveBuff(this, BuffIcon.Poison);
        }

        public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
        {
            if (!Alive || poison == null)
            {
                return ApplyPoisonResult.Immune;
            }

            //Skill Masteries
            if (ResilienceSpell.UnderEffects(this) && 0.25 > Utility.RandomDouble())
            {
                return ApplyPoisonResult.Immune;
            }

            if (EvilOmenSpell.TryEndEffect(this))
            {
                poison = PoisonImpl.IncreaseLevel(poison);
            }

            //Skill Masteries
            if ((Poison == null || Poison.Level < poison.Level) && ToleranceSpell.OnPoisonApplied(this))
            {
                poison = PoisonImpl.DecreaseLevel(poison);

                if (poison == null || poison.Level <= 0)
                {
                    PrivateOverheadMessage(MessageType.Regular, 0x3F, 1053092, NetState); // * You feel yourself resisting the effects of the poison *
                    return ApplyPoisonResult.Immune;
                }
            }

            ApplyPoisonResult result = base.ApplyPoison(from, poison);

            if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer timer)
            {
                timer.From = from;
            }

            return result;
        }

        public override bool CheckPoisonImmunity(Mobile from, Poison poison)
        {
            if (Young)
            {
                return true;
            }

            return base.CheckPoisonImmunity(from, poison);
        }

        public override void OnPoisonImmunity(Mobile from, Poison poison)
        {
            if (Young)
            {
                SendLocalizedMessage(502808);
                // You would have been poisoned, were you not new to the land of Britannia. Be careful in the future.
            }
            else
            {
                base.OnPoisonImmunity(from, poison);
            }
        }
        #endregion

        public PlayerMobile(Serial s)
            : base(s)
        {
            Instances.Add(this);

            m_VisList = new List<Mobile>();
        }

        public List<Mobile> VisibilityList => m_VisList;

        public List<Mobile> PermaFlags => m_PermaFlags;

        public override int Luck => AosAttributes.GetValue(this, AosAttribute.Luck) + TenthAnniversarySculpture.GetLuckBonus(this) + FountainOfFortune.GetLuckBonus(this);

        public int RealLuck
        {
            get
            {
                int facetBonus = !Siege.SiegeShard && Map == Map.Felucca ? RandomItemGenerator.FeluccaLuckBonus : 0;

                return Luck + FountainOfFortune.GetLuckBonus(this) + facetBonus;
            }
        }

        public override bool IsHarmfulCriminal(IDamageable damageable)
        {
            Mobile target = damageable as Mobile;

            if (Stealing.ClassicMode && target is PlayerMobile pm && pm.m_PermaFlags.Count > 0)
            {
                int noto = Notoriety.Compute(this, target);

                if (noto == Notoriety.Innocent)
                {
                    pm.Delta(MobileDelta.Noto);
                }

                return false;
            }

            if (target is BaseCreature bc && bc.InitialInnocent && !bc.Controlled)
            {
                return false;
            }

            if (target is BaseCreature creature && creature.Controlled && creature.ControlMaster == this)
            {
                return false;
            }

            if (target is BaseCreature baseCreature && baseCreature.Summoned && baseCreature.SummonMaster == this)
            {
                return false;
            }

            return base.IsHarmfulCriminal(damageable);
        }

        public BOBFilter BOBFilter => BulkOrderSystem.GetBOBFilter(this);

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 42: // upgraded quest serialization
                case 41: // removed PeacedUntil - no need to serialize this
                case 40: // Version 40, moved gauntlet points, virtua artys and TOT convert to PointsSystem
                case 39: // Version 39, removed ML quest save/load
                case 38:
                    NextGemOfSalvationUse = reader.ReadDateTime();
                    goto case 37;
                case 37:
                    m_ExtendedFlags = (ExtendedPlayerFlag)reader.ReadInt();
                    goto case 36;
                case 36:
                    RewardStableSlots = reader.ReadInt();
                    goto case 35;
                case 35: // Siege Blessed Item
                    _BlessedItem = reader.ReadItem();
                    goto case 34;
                // Version 34 - new BOD System
                case 34:
                case 33:
                    {
                        ExploringTheDeepQuest = (ExploringTheDeepQuestChain)reader.ReadInt();
                        goto case 31;
                    }
                case 32:
                case 31:
                    {
                        DisplayGuildTitle = version > 31 && reader.ReadBool();
                        m_FameKarmaTitle = reader.ReadString();
                        m_PaperdollSkillTitle = reader.ReadString();
                        m_OverheadTitle = reader.ReadString();
                        m_SubtitleSkillTitle = reader.ReadString();

                        m_CurrentChampTitle = reader.ReadString();
                        m_CurrentVeteranTitle = reader.ReadInt();
                        goto case 30;
                    }
                case 30: goto case 29;
                case 29:
                    {
                        m_SSNextSeed = reader.ReadDateTime();
                        m_SSSeedExpire = reader.ReadDateTime();
                        m_SSSeedLocation = reader.ReadPoint3D();
                        m_SSSeedMap = reader.ReadMap();

                        m_Collections = new Dictionary<Collection, int>();
                        m_RewardTitles = new List<object>();

                        for (int i = reader.ReadInt(); i > 0; i--)
                        {
                            m_Collections.Add((Collection)reader.ReadInt(), reader.ReadInt());
                        }

                        for (int i = reader.ReadInt(); i > 0; i--)
                        {
                            m_RewardTitles.Add(QuestReader.Object(reader));
                        }

                        m_SelectedTitle = reader.ReadInt();

                        goto case 28;
                    }
                case 28:
                    {
                        goto case 27;
                    }
                case 27:
                    {
                        m_AnkhNextUse = reader.ReadDateTime();

                        goto case 26;
                    }
                case 26:
                    {
                        m_AutoStabled = reader.ReadStrongMobileList();

                        goto case 25;
                    }
                case 25:
                    {
                        int recipeCount = reader.ReadInt();

                        if (recipeCount > 0)
                        {
                            m_AcquiredRecipes = new Dictionary<int, bool>();

                            for (int i = 0; i < recipeCount; i++)
                            {
                                int r = reader.ReadInt();
                                if (reader.ReadBool()) //Don't add in recipies which we haven't gotten or have been removed
                                {
                                    m_AcquiredRecipes.Add(r, true);
                                }
                            }
                        }
                        goto case 24;
                    }
                case 24:
                    {
                        m_LastHonorLoss = reader.ReadDeltaTime();
                        goto case 23;
                    }
                case 23:
                    {
                        m_ChampionTitles = new ChampionTitleInfo(reader);
                        goto case 22;
                    }
                case 22:
                    {
                        m_LastValorLoss = reader.ReadDateTime();
                        goto case 21;
                    }
                case 21:
                    {
                        goto case 20;
                    }
                case 20:
                    {
                        m_AllianceMessageHue = reader.ReadEncodedInt();
                        m_GuildMessageHue = reader.ReadEncodedInt();

                        goto case 19;
                    }
                case 19:
                    {
                        int rank = reader.ReadEncodedInt();
                        int maxRank = RankDefinition.Ranks.Length - 1;
                        if (rank > maxRank)
                        {
                            rank = maxRank;
                        }

                        m_GuildRank = RankDefinition.Ranks[rank];
                        m_LastOnline = reader.ReadDateTime();
                        goto case 18;
                    }
                case 18:
                    {
                        m_SolenFriendship = (SolenFriendship)reader.ReadEncodedInt();

                        goto case 17;
                    }
                case 17: 
                case 16:
                    {
                        m_Quest = QuestSerializer.DeserializeQuest(reader);

                        if (m_Quest != null)
                        {
                            m_Quest.From = this;
                        }

                        int count = reader.ReadEncodedInt();

                        if (count > 0)
                        {
                            m_DoneQuests = new List<QuestRestartInfo>();

                            for (int i = 0; i < count; ++i)
                            {
                                Type questType;

                                if (version >= 42)
                                    questType = reader.ReadObjectType();
                                else
                                    questType = QuestSerializer.ReadQuestType(reader);

                                DateTime restartTime;

                                restartTime = reader.ReadDateTime();

                                m_DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
                            }
                        }

                        m_Profession = reader.ReadEncodedInt();
                        goto case 15;
                    }
                case 15:
                    {
                        m_LastCompassionLoss = reader.ReadDeltaTime();
                        goto case 14;
                    }
                case 14:
                    {
                        m_CompassionGains = reader.ReadEncodedInt();

                        if (m_CompassionGains > 0)
                        {
                            m_NextCompassionDay = reader.ReadDeltaTime();
                        }

                        goto case 13;
                    }
                case 13: 
                case 12:
                    {
                        goto case 11;
                    }
                case 11:
                case 10:
                    {
                        if (reader.ReadBool())
                        {
                            m_HairModID = reader.ReadInt();
                            m_HairModHue = reader.ReadInt();
                            m_BeardModID = reader.ReadInt();
                            m_BeardModHue = reader.ReadInt();
                        }

                        goto case 9;
                    }
                case 9:
                    {
                        SavagePaintExpiration = reader.ReadTimeSpan();

                        if (SavagePaintExpiration > TimeSpan.Zero)
                        {
                            BodyMod = Female ? 184 : 183;
                            HueMod = 0;
                        }

                        goto case 8;
                    }
                case 8:
                    {
                        m_NpcGuild = (NpcGuild)reader.ReadInt();
                        m_NpcGuildJoinTime = reader.ReadDateTime();
                        m_NpcGuildGameTime = reader.ReadTimeSpan();
                        goto case 7;
                    }
                case 7:
                    {
                        m_PermaFlags = reader.ReadStrongMobileList();
                        goto case 6;
                    }
                case 6:                   
                case 5:                   
                case 4:
                    {
                        m_LastJusticeLoss = reader.ReadDeltaTime();
                        m_JusticeProtectors = reader.ReadStrongMobileList();
                        goto case 3;
                    }
                case 3:
                    {
                        m_LastSacrificeGain = reader.ReadDeltaTime();
                        m_LastSacrificeLoss = reader.ReadDeltaTime();
                        m_AvailableResurrects = reader.ReadInt();
                        goto case 2;
                    }
                case 2:
                    {
                        m_Flags = (PlayerFlag)reader.ReadInt();
                        goto case 1;
                    }
                case 1:
                    {
                        m_LongTermElapse = reader.ReadTimeSpan();
                        m_ShortTermElapse = reader.ReadTimeSpan();
                        m_GameTime = reader.ReadTimeSpan();
                        goto case 0;
                    }
                case 0:
                    {                      
                        break;
                    }
            }

            if (m_RecentlyReported == null)
            {
                m_RecentlyReported = new List<Mobile>();
            }

            if (m_DoneQuests == null)
            {
                m_DoneQuests = new List<QuestRestartInfo>();
            }

            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            if (m_RewardTitles == null)
            {
                m_RewardTitles = new List<object>();
            }

            // Professions weren't verified on 1.0 RC0
            if (!CharacterCreation.VerifyProfession(m_Profession))
            {
                m_Profession = 0;
            }

            if (m_PermaFlags == null)
            {
                m_PermaFlags = new List<Mobile>();
            }

            if (m_JusticeProtectors == null)
            {
                m_JusticeProtectors = new List<Mobile>();
            }

            if (m_GuildRank == null)
            {
                m_GuildRank = RankDefinition.Member;
                //Default to member if going from older version to new version (only time it should be null)
            }

            if (m_LastOnline == DateTime.MinValue && Account != null)
            {
                m_LastOnline = ((Account)Account).LastLogin;
            }

            if (m_ChampionTitles == null)
            {
                m_ChampionTitles = new ChampionTitleInfo();
            }

            List<Mobile> list = Stabled;

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] is BaseCreature bc)
                {
                    bc.IsStabled = true;
                    bc.StabledBy = this;
                }
            }

            CheckAtrophies(this);

            if (Hidden) //Hiding is the only buff where it has an effect that's serialized.
            {
                AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
            }

            if (_BlessedItem != null)
            {
                Timer.DelayCall(
                b =>
                {
                    if (_BlessedItem == b && b.RootParent != this)
                    {
                        _BlessedItem = null;
                    }
                },
                _BlessedItem);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            CheckKillDecay();
            CheckAtrophies(this);

            base.Serialize(writer);
            writer.Write(42); // version

            writer.Write(NextGemOfSalvationUse);

            writer.Write((int)m_ExtendedFlags);

            writer.Write(RewardStableSlots);

            if (_BlessedItem != null && _BlessedItem.RootParent != this)
            {
                _BlessedItem = null;
            }

            writer.Write(_BlessedItem);

            writer.Write((int)ExploringTheDeepQuest);

            // Version 31/32 Titles
            writer.Write(DisplayGuildTitle);
            writer.Write(m_FameKarmaTitle);
            writer.Write(m_PaperdollSkillTitle);
            writer.Write(m_OverheadTitle);
            writer.Write(m_SubtitleSkillTitle);
            writer.Write(m_CurrentChampTitle);
            writer.Write(m_CurrentVeteranTitle);

            // Version 30 open to take out old Queens Loyalty Info

            #region Plant System
            writer.Write(m_SSNextSeed);
            writer.Write(m_SSSeedExpire);
            writer.Write(m_SSSeedLocation);
            writer.Write(m_SSSeedMap);
            #endregion

            #region Mondain's Legacy

            if (m_Collections == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_Collections.Count);

                foreach (KeyValuePair<Collection, int> pair in m_Collections)
                {
                    writer.Write((int)pair.Key);
                    writer.Write(pair.Value);
                }
            }

            if (m_RewardTitles == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_RewardTitles.Count);

                for (int i = 0; i < m_RewardTitles.Count; i++)
                {
                    QuestWriter.Object(writer, m_RewardTitles[i]);
                }
            }

            writer.Write(m_SelectedTitle);
            #endregion

            // Version 28
            writer.Write(m_AnkhNextUse);
            writer.Write(m_AutoStabled, true);

            if (m_AcquiredRecipes == null)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(m_AcquiredRecipes.Count);

                foreach (KeyValuePair<int, bool> kvp in m_AcquiredRecipes)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
            }

            writer.WriteDeltaTime(m_LastHonorLoss);

            ChampionTitleInfo.Serialize(writer, m_ChampionTitles);

            writer.Write(m_LastValorLoss);

            writer.WriteEncodedInt(m_AllianceMessageHue);
            writer.WriteEncodedInt(m_GuildMessageHue);

            writer.WriteEncodedInt(m_GuildRank.Rank);
            writer.Write(m_LastOnline);

            writer.WriteEncodedInt((int)m_SolenFriendship);

            QuestSerializer.Serialize(m_Quest, writer);

            if (m_DoneQuests == null)
            {
                writer.WriteEncodedInt(0);
            }
            else
            {
                writer.WriteEncodedInt(m_DoneQuests.Count);

                for (int i = 0; i < m_DoneQuests.Count; ++i)
                {
                    QuestRestartInfo restartInfo = m_DoneQuests[i];

                    writer.WriteObjectType(restartInfo.QuestType);
                    writer.Write(restartInfo.RestartTime);
                }
            }

            writer.WriteEncodedInt(m_Profession);

            writer.WriteDeltaTime(m_LastCompassionLoss);

            writer.WriteEncodedInt(m_CompassionGains);

            if (m_CompassionGains > 0)
            {
                writer.WriteDeltaTime(m_NextCompassionDay);
            }

            bool useMods = m_HairModID != -1 || m_BeardModID != -1;

            writer.Write(useMods);

            if (useMods)
            {
                writer.Write(m_HairModID);
                writer.Write(m_HairModHue);
                writer.Write(m_BeardModID);
                writer.Write(m_BeardModHue);
            }

            writer.Write(SavagePaintExpiration);

            writer.Write((int)m_NpcGuild);
            writer.Write(m_NpcGuildJoinTime);
            writer.Write(m_NpcGuildGameTime);

            writer.Write(m_PermaFlags, true);

            writer.WriteDeltaTime(m_LastJusticeLoss);
            writer.Write(m_JusticeProtectors, true);

            writer.WriteDeltaTime(m_LastSacrificeGain);
            writer.WriteDeltaTime(m_LastSacrificeLoss);
            writer.Write(m_AvailableResurrects);

            writer.Write((int)m_Flags);

            writer.Write(m_LongTermElapse);
            writer.Write(m_ShortTermElapse);
            writer.Write(GameTime);
        }

        public static void CheckAtrophies(Mobile m)
        {
            SacrificeVirtue.CheckAtrophy(m);
            JusticeVirtue.CheckAtrophy(m);
            CompassionVirtue.CheckAtrophy(m);
            ValorVirtue.CheckAtrophy(m);

            if (m is PlayerMobile pm)
            {
                ChampionTitleInfo.CheckAtrophy(pm);
            }
        }

        public void CheckKillDecay()
        {
            if (m_ShortTermElapse < GameTime)
            {
                m_ShortTermElapse += TimeSpan.FromHours(8);
                if (ShortTermMurders > 0)
                {
                    --ShortTermMurders;
                }
            }

            if (m_LongTermElapse < GameTime)
            {
                m_LongTermElapse += TimeSpan.FromHours(40);
                if (Kills > 0)
                {
                    --Kills;
                }
            }
        }

        public void ResetKillTime()
        {
            m_ShortTermElapse = GameTime + TimeSpan.FromHours(8);
            m_LongTermElapse = GameTime + TimeSpan.FromHours(40);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SessionStart => m_SessionStart;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan GameTime
        {
            get
            {
                if (NetState != null)
                {
                    return m_GameTime + (DateTime.UtcNow - m_SessionStart);
                }

                return m_GameTime;
            }
        }

        public override bool CanSee(Mobile m)
        {
            if (m is IConditionalVisibility && !((IConditionalVisibility)m).CanBeSeenBy(this))
                return false;

            if (m is CharacterStatue statue)
            {
                statue.OnRequestedAnimation(this);
            }

            if (m is PlayerMobile pm && pm.m_VisList.Contains(this))
            {
                return true;
            }

            return base.CanSee(m);
        }

        public override bool CanSee(Item item)
        {
            if (item is IConditionalVisibility vis && !vis.CanBeSeenBy(this))
                return false;

            if (m_DesignContext != null && m_DesignContext.Foundation.IsHiddenToCustomizer(this, item))
            {
                return false;
            }

            if (AccessLevel == AccessLevel.Player)
            {
                Region r = item.GetRegion();

                if (r is BaseRegion region && !region.CanSee(this, item))
                {
                    return false;
                }
            }

            return base.CanSee(item);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            Instances.Remove(this);

            BaseHouse.HandleDeletion(this);

            DisguiseTimers.RemoveTimer(this);
            ScrollOfAlacrity.RemoveTimer(this);
        }

        public delegate void PlayerPropertiesEventHandler(PlayerPropertiesEventArgs e);

        public static event PlayerPropertiesEventHandler PlayerProperties;

        public sealed class PlayerPropertiesEventArgs(PlayerMobile player, ObjectPropertyList list) : EventArgs
        {
            public PlayerMobile Player = player;
            public ObjectPropertyList PropertyList = list;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            Engines.JollyRoger.JollyRogerData.DisplayTitle(this, list);

            if (m_SubtitleSkillTitle != null)
                list.Add(1042971, m_SubtitleSkillTitle);

            if (m_CurrentVeteranTitle > 0)
                list.Add(m_CurrentVeteranTitle);

            if (m_RewardTitles != null && m_SelectedTitle > -1)
            {
                if (m_SelectedTitle < m_RewardTitles.Count)
                {
                    if (m_RewardTitles[m_SelectedTitle] is int)
                    {
                        string customTitle;

                        if ((int)m_RewardTitles[m_SelectedTitle] == 1154017 && CityLoyaltySystem.HasCustomTitle(this, out customTitle))
                        {
                            list.Add(1154017, customTitle); // ~1_TITLE~ of ~2_CITY~
                        }
                        else
                        {
                            list.Add((int)m_RewardTitles[m_SelectedTitle]);
                        }
                    }
                    else if (m_RewardTitles[m_SelectedTitle] is string)
                    {
                        list.Add(1070722, (string)m_RewardTitles[m_SelectedTitle]);
                    }
                }
            }

            for (int i = AllFollowers.Count - 1; i >= 0; i--)
            {
                if (AllFollowers[i] is BaseCreature c && c.GuardMode == GuardType.Active)
                {
                    list.Add(501129); // guarded
                    break;
                }
            }

            if (TestCenter.Enabled)
            {
                VvVPlayerEntry entry = PointsSystem.ViceVsVirtue.GetPlayerEntry<VvVPlayerEntry>(this);

                list.Add($"Kills: {(entry == null ? "0" : entry.Kills.ToString())} / Deaths: {(entry == null ? "0" : entry.Deaths.ToString())} / Assists: {(entry == null ? "0" : entry.Assists.ToString())}");

                list.Add(1060415, AosAttributes.GetValue(this, AosAttribute.AttackChance).ToString()); // hit chance increase ~1_val~%
                list.Add(1060408, AosAttributes.GetValue(this, AosAttribute.DefendChance).ToString()); // defense chance increase ~1_val~%
                list.Add(1060486, AosAttributes.GetValue(this, AosAttribute.WeaponSpeed).ToString()); // swing speed increase ~1_val~%
                list.Add(1060401, AosAttributes.GetValue(this, AosAttribute.WeaponDamage).ToString()); // damage increase ~1_val~%
                list.Add(1060483, AosAttributes.GetValue(this, AosAttribute.SpellDamage).ToString()); // spell damage increase ~1_val~%
                list.Add(1060433, AosAttributes.GetValue(this, AosAttribute.LowerManaCost).ToString()); // lower mana cost
            }

            PlayerProperties?.Invoke(new PlayerPropertiesEventArgs(this, list));
        }

        protected override bool OnMove(Direction d)
        {
            if (Party != null && NetState != null)
            {
                Waypoints.UpdateToParty(this);
            }

            if (IsStaff())
            {
                return true;
            }

            if (Hidden && DesignContext.Find(this) == null) //Hidden & NOT customizing a house
            {
                if (!Mounted && Skills.Stealth.Value >= 25.0)
                {
                    bool running = (d & Direction.Running) != 0;

                    if (running)
                    {
                        if ((AllowedStealthSteps -= 2) <= 0)
                        {
                            RevealingAction();
                        }
                    }
                    else if (AllowedStealthSteps-- <= 0)
                    {
                        Stealth.OnUse(this);
                    }
                }
                else
                {
                    RevealingAction();
                }
            }

            if (InvisibilityPotion.HasTimer(this))
            {
                InvisibilityPotion.Iterrupt(this);
            }

            return true;
        }

        public bool BedrollLogout { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public override bool Paralyzed
        {
            get => base.Paralyzed;
            set
            {
                base.Paralyzed = value;

                if (value)
                {
                    AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827)); //Paralyze/You are frozen and can not move
                }
                else
                {
                    RemoveBuff(BuffIcon.Paralyze);
                }
            }
        }

        #region Quests
        private QuestSystem m_Quest;
        private List<QuestRestartInfo> m_DoneQuests;
        private SolenFriendship m_SolenFriendship;

        public QuestSystem Quest { get => m_Quest; set => m_Quest = value; }

        public List<QuestRestartInfo> DoneQuests { get => m_DoneQuests; set => m_DoneQuests = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public SolenFriendship SolenFriendship { get => m_SolenFriendship; set => m_SolenFriendship = value; }
        #endregion

        #region Mondain's Legacy
        public List<BaseQuest> Quests => MondainQuestData.GetQuests(this);
        public Dictionary<QuestChain, BaseChain> Chains => MondainQuestData.GetChains(this);

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Peaced => PeacedUntil > DateTime.UtcNow;

        private Dictionary<Collection, int> m_Collections;
        private List<object> m_RewardTitles;
        private int m_SelectedTitle;

        public Dictionary<Collection, int> Collections => m_Collections;

        public List<object> RewardTitles => m_RewardTitles;

        public int SelectedTitle => m_SelectedTitle;

        public bool RemoveRewardTitle(object o, bool silent)
        {
            if (m_RewardTitles.Contains(o))
            {
                int i = m_RewardTitles.IndexOf(o);

                if (i == m_SelectedTitle)
                    SelectRewardTitle(-1, silent);
                else if (i > m_SelectedTitle)
                    SelectRewardTitle(m_SelectedTitle - 1, silent);

                m_RewardTitles.Remove(o);

                return true;
            }

            return false;
        }

        public int GetCollectionPoints(Collection collection)
        {
            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            int points = 0;

            if (m_Collections.ContainsKey(collection))
            {
                m_Collections.TryGetValue(collection, out points);
            }

            return points;
        }

        public void AddCollectionPoints(Collection collection, int points)
        {
            if (m_Collections == null)
            {
                m_Collections = new Dictionary<Collection, int>();
            }

            if (!m_Collections.TryAdd(collection, points))
            {
                m_Collections[collection] += points;
            }
        }

        public void SelectRewardTitle(int num, bool silent = false)
        {
            if (num == -1)
            {
                m_SelectedTitle = num;

                if (!silent)
                    SendLocalizedMessage(1074010); // You elect to hide your Reward Title.
            }
            else if (num < m_RewardTitles.Count && num >= -1)
            {
                if (m_SelectedTitle != num)
                {
                    m_SelectedTitle = num;

                    if (m_RewardTitles[num] is int && !silent)
                    {
                        SendLocalizedMessage(1074008, "#" + (int)m_RewardTitles[num]);
                        // You change your Reward Title to "~1_TITLE~".
                    }
                    else if (m_RewardTitles[num] is string && !silent)
                    {
                        SendLocalizedMessage(1074008, (string)m_RewardTitles[num]); // You change your Reward Title to "~1_TITLE~".
                    }
                }
                else if (!silent)
                {
                    SendLocalizedMessage(1074009); // You decide to leave your title as it is.
                }
            }

            InvalidateProperties();
        }

        public bool AddRewardTitle(object title)
        {
            if (m_RewardTitles == null)
            {
                m_RewardTitles = new List<object>();
            }

            if (title != null && !m_RewardTitles.Contains(title))
            {
                m_RewardTitles.Add(title);

                InvalidateProperties();
                return true;
            }

            return false;
        }

        public void ShowChangeTitle()
        {
            SendGump(new SelectTitleGump(this, m_SelectedTitle));
        }
        #endregion

        #region Titles
        private string m_FameKarmaTitle;
        private string m_PaperdollSkillTitle;
        private string m_SubtitleSkillTitle;
        private string m_CurrentChampTitle;
        private string m_OverheadTitle;
        private int m_CurrentVeteranTitle;

        public string FameKarmaTitle
        {
            get => m_FameKarmaTitle;
            set { m_FameKarmaTitle = value; InvalidateProperties(); }
        }

        public string PaperdollSkillTitle
        {
            get => m_PaperdollSkillTitle;
            set { m_PaperdollSkillTitle = value; InvalidateProperties(); }
        }

        public string SubtitleSkillTitle
        {
            get => m_SubtitleSkillTitle;
            set { m_SubtitleSkillTitle = value; InvalidateProperties(); }
        }

        public string CurrentChampTitle
        {
            get => m_CurrentChampTitle;
            set { m_CurrentChampTitle = value; InvalidateProperties(); }
        }

        public string OverheadTitle
        {
            get => m_OverheadTitle;
            set { m_OverheadTitle = value; InvalidateProperties(); }
        }

        public int CurrentVeteranTitle
        {
            get => m_CurrentVeteranTitle;
            set { m_CurrentVeteranTitle = value; InvalidateProperties(); }
        }

        public override bool ShowAccessTitle
        {
            get
            {
                switch (AccessLevel)
                {
                    case AccessLevel.VIP:
                    case AccessLevel.Counselor:
                    case AccessLevel.GameMaster:
                    case AccessLevel.Seer:
                        return true;
                }

                return false;
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            string prefix = "";

            if (ShowFameTitle && Fame >= 10000)
            {
                prefix = Female ? "Lady" : "Lord";
            }

            string suffix = "";

            if (PropertyTitle && !string.IsNullOrEmpty(Title))
            {
                suffix = Title;
            }

            BaseGuild guild = Guild;
            bool vvv = ViceVsVirtueSystem.IsVvV(this) && (ViceVsVirtueSystem.EnhancedRules || Map == ViceVsVirtueSystem.Facet);

            if (m_OverheadTitle != null)
            {
                if (vvv)
                {
                    suffix = "[VvV]";
                }
                else
                {
                    int loc = Utility.ToInt32(m_OverheadTitle);

                    if (loc > 0)
                    {
                        if (CityLoyaltySystem.ApplyCityTitle(this, list, prefix, loc))
                            return;
                    }
                    else if (suffix.Length > 0)
                    {
                        suffix = $"{suffix} {m_OverheadTitle}";
                    }
                    else
                    {
                        suffix = $"{m_OverheadTitle}";
                    }
                }
            }
            else if (guild != null && DisplayGuildAbbr)
            {
                if (vvv)
                {
                    suffix = $"[{Utility.FixHtml(guild.Abbreviation)}] [VvV]";
                }
                else if (suffix.Length > 0)
                {
                    suffix = $"{suffix} [{Utility.FixHtml(guild.Abbreviation)}]";
                }
                else
                {
                    suffix = $"[{Utility.FixHtml(guild.Abbreviation)}]";
                }
            }
            else if (vvv)
            {
                suffix = "[VvV]";
            }

            suffix = ApplyNameSuffix(suffix);
            string name = Name;

            list.Add(1050045, $"{prefix} \t{name}\t {suffix}"); // ~1_PREFIX~~2_NAME~~3_SUFFIX~

            if (guild != null && DisplayGuildTitle)
            {
                string title = GuildTitle;

                title = title == null ? "" : title.Trim();

                if (title.Length > 0)
                {
                    list.Add(1060776, $"{Utility.FixHtml(title)}\t{Utility.FixHtml(guild.Name)}"); // ~1_val~, ~2_val~
                }
            }
        }

        public override void OnAfterNameChange(string oldName, string newName)
        {
            if (m_FameKarmaTitle != null)
            {
                FameKarmaTitle = FameKarmaTitle.Replace(oldName, newName);
            }
        }
        #endregion

        public override void OnKillsChange(int oldValue)
        {
            if (Young && Kills > oldValue)
            {
                Account acc = Account as Account;

                acc?.RemoveYoungStatus(0);
            }
        }

        public override void OnKarmaChange(int oldValue)
        {
            EpiphanyHelper.OnKarmaChange(this);
        }

        public override void OnSkillChange(SkillName skill, double oldBase)
        {
            if (skill != SkillName.Alchemy && Skills.CurrentMastery == skill && Skills[skill].Value < MasteryInfo.MinSkillRequirement)
            {
                SkillName mastery = Skills.CurrentMastery;
                Skills.CurrentMastery = SkillName.Alchemy;

                MasteryInfo.OnMasteryChanged(this, mastery);
            }

            TransformContext context = TransformationSpellHelper.GetContext(this);

            if (context != null)
            {
                TransformationSpellHelper.CheckCastSkill(this, context);
            }
        }

        public override void OnAccessLevelChanged(AccessLevel oldLevel)
        {
            IgnoreMobiles = !IsPlayer() || TransformationSpellHelper.UnderTransformation(this, typeof(WraithFormSpell));
        }

        public override void OnDelete()
        {
            Instances.Remove(this);

            m_ReceivedHonorContext?.Cancel();

            m_SentHonorContext?.Cancel();
        }

        #region Fastwalk Prevention
        private const bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
        private const int FastwalkThreshold = 400; // Fastwalk prevention will become active after 0.4 seconds

        private long m_NextMovementTime;
        private bool m_HasMoved;

        public long NextMovementTime => m_NextMovementTime;

        public virtual bool UsesFastwalkPrevention => IsPlayer();

        public override int ComputeMovementSpeed(Direction dir, bool checkTurning)
        {
            if (checkTurning && (dir & Direction.Mask) != (Direction & Direction.Mask))
            {
                return RunMount; // We are NOT actually moving (just a direction change)
            }

            bool running = (dir & Direction.Running) != 0;

            bool onHorse = Mount != null || Flying;

            AnimalFormContext animalContext = AnimalForm.GetContext(this);

            if (onHorse || animalContext != null && animalContext.SpeedBoost)
            {
                return running ? RunMount : WalkMount;
            }

            return running ? RunFoot : WalkFoot;
        }

        public static bool MovementThrottle_Callback(byte packetID, NetState ns, out bool drop)
        {
            drop = false;

            PlayerMobile pm = ns.Mobile as PlayerMobile;

            if (pm == null || !pm.UsesFastwalkPrevention)
            {
                return true;
            }

            if (!pm.m_HasMoved)
            {
                // has not yet moved
                pm.m_NextMovementTime = Core.TickCount;
                pm.m_HasMoved = true;
                return true;
            }

            long ts = pm.m_NextMovementTime - Core.TickCount;

            if (ts < 0)
            {
                // been a while since we've last moved
                pm.m_NextMovementTime = Core.TickCount;
                return true;
            }

            return ts < FastwalkThreshold;
        }
        #endregion

        #region Hair and beard mods
        private int m_HairModID = -1, m_HairModHue;
        private int m_BeardModID = -1, m_BeardModHue;

        public void SetHairMods(int hairID, int beardID)
        {
            if (hairID == -1)
            {
                InternalRestoreHair(true, ref m_HairModID, ref m_HairModHue);
            }
            else if (hairID != -2)
            {
                InternalChangeHair(true, hairID, ref m_HairModID, ref m_HairModHue);
            }

            if (beardID == -1)
            {
                InternalRestoreHair(false, ref m_BeardModID, ref m_BeardModHue);
            }
            else if (beardID != -2)
            {
                InternalChangeHair(false, beardID, ref m_BeardModID, ref m_BeardModHue);
            }
        }

        private void CreateHair(bool hair, int id, int hue)
        {
            if (hair)
            {
                //TODO Verification?
                HairItemID = id;
                HairHue = hue;
            }
            else
            {
                FacialHairItemID = id;
                FacialHairHue = hue;
            }
        }

        private void InternalRestoreHair(bool hair, ref int id, ref int hue)
        {
            if (id == -1)
            {
                return;
            }

            if (hair)
            {
                HairItemID = 0;
            }
            else
            {
                FacialHairItemID = 0;
            }

            //if( id != 0 )
            CreateHair(hair, id, hue);

            id = -1;
            hue = 0;
        }

        private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
        {
            if (storeID == -1)
            {
                storeID = hair ? HairItemID : FacialHairItemID;
                storeHue = hair ? HairHue : FacialHairHue;
            }
            CreateHair(hair, id, 0);
        }
        #endregion

        #region Virtues
        private DateTime m_LastSacrificeGain;
        private DateTime m_LastSacrificeLoss;
        private int m_AvailableResurrects;

        public DateTime LastSacrificeGain { get => m_LastSacrificeGain; set => m_LastSacrificeGain = value; }
        public DateTime LastSacrificeLoss { get => m_LastSacrificeLoss; set => m_LastSacrificeLoss = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AvailableResurrects { get => m_AvailableResurrects; set => m_AvailableResurrects = value; }

        private DateTime m_NextJustAward;
        private DateTime m_LastJusticeLoss;
        private List<Mobile> m_JusticeProtectors;

        public DateTime LastJusticeLoss { get => m_LastJusticeLoss; set => m_LastJusticeLoss = value; }
        public List<Mobile> JusticeProtectors { get => m_JusticeProtectors; set => m_JusticeProtectors = value; }

        private DateTime m_LastCompassionLoss;
        private DateTime m_NextCompassionDay;
        private int m_CompassionGains;

        public DateTime LastCompassionLoss { get => m_LastCompassionLoss; set => m_LastCompassionLoss = value; }
        public DateTime NextCompassionDay { get => m_NextCompassionDay; set => m_NextCompassionDay = value; }
        public int CompassionGains { get => m_CompassionGains; set => m_CompassionGains = value; }

        private DateTime m_LastValorLoss;

        public DateTime LastValorLoss { get => m_LastValorLoss; set => m_LastValorLoss = value; }

        private DateTime m_LastHonorLoss;
        private HonorContext m_ReceivedHonorContext;
        private HonorContext m_SentHonorContext;
        public DateTime m_hontime;

        public DateTime LastHonorLoss { get => m_LastHonorLoss; set => m_LastHonorLoss = value; }

        public DateTime LastHonorUse { get; set; }

        public bool HonorActive { get; set; }

        public HonorContext ReceivedHonorContext { get => m_ReceivedHonorContext; set => m_ReceivedHonorContext = value; }
        public HonorContext SentHonorContext { get => m_SentHonorContext; set => m_SentHonorContext = value; }
        #endregion

        #region Young system
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Young
        {
            get => GetFlag(PlayerFlag.Young);
            set
            {
                SetFlag(PlayerFlag.Young, value);
                InvalidateProperties();
            }
        }

        public override string ApplyNameSuffix(string suffix)
        {
            if (Young)
            {
                suffix = suffix.Length == 0 ? "(Young)" : string.Concat(suffix, " (Young)");
            }

            return base.ApplyNameSuffix(suffix);
        }

        public override TimeSpan GetLogoutDelay()
        {
            if (Young || BedrollLogout || TestCenter.Enabled)
            {
                return TimeSpan.Zero;
            }

            return base.GetLogoutDelay();
        }

        private DateTime m_LastYoungMessage = DateTime.MinValue;

        public bool CheckYoungProtection(Mobile from)
        {
            if (!Young)
            {
                return false;
            }

            if (Region is BaseRegion region && !region.YoungProtected)
            {
                return false;
            }

            if (from is BaseCreature creature && creature.IgnoreYoungProtection)
            {
                return false;
            }

            if (Quest != null && Quest.IgnoreYoungProtection(from))
            {
                return false;
            }

            if (DateTime.UtcNow - m_LastYoungMessage > TimeSpan.FromMinutes(1.0))
            {
                m_LastYoungMessage = DateTime.UtcNow;
                SendLocalizedMessage(1019067);
                // A monster looks at you menacingly but does not attack.  You would be under attack now if not for your status as a new citizen of Britannia.
            }

            return true;
        }

        private DateTime m_LastYoungHeal = DateTime.MinValue;

        public bool CheckYoungHealTime()
        {
            if (DateTime.UtcNow - m_LastYoungHeal > TimeSpan.FromMinutes(5.0))
            {
                m_LastYoungHeal = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        private static readonly Point3D[] m_TrammelDeathDestinations =
        {
            new Point3D(1481, 1612, 20), new Point3D(2708, 2153, 0), new Point3D(2249, 1230, 0), new Point3D(5197, 3994, 37),
            new Point3D(1412, 3793, 0), new Point3D(3688, 2232, 20), new Point3D(2578, 604, 0), new Point3D(4397, 1089, 0),
            new Point3D(5741, 3218, -2), new Point3D(2996, 3441, 15), new Point3D(624, 2225, 0), new Point3D(1916, 2814, 0),
            new Point3D(2929, 854, 0), new Point3D(545, 967, 0), new Point3D(3469, 2559, 36)
        };

        private static readonly Point3D[] m_IlshenarDeathDestinations =
        {
            new Point3D(1216, 468, -13), new Point3D(723, 1367, -60), new Point3D(745, 725, -28), new Point3D(281, 1017, 0),
            new Point3D(986, 1011, -32), new Point3D(1175, 1287, -30), new Point3D(1533, 1341, -3), new Point3D(529, 217, -44),
            new Point3D(1722, 219, 96)
        };

        private static readonly Point3D[] m_MalasDeathDestinations =
        {
            new Point3D(2079, 1376, -70), new Point3D(944, 519, -71)
        };

        private static readonly Point3D[] m_TokunoDeathDestinations =
        {
            new Point3D(1166, 801, 27), new Point3D(782, 1228, 25), new Point3D(268, 624, 15)
        };

        public bool YoungDeathTeleport()
        {
            if (Region.IsPartOf<Jail>() || Region.IsPartOf("Samurai start location") ||
                Region.IsPartOf("Ninja start location") || Region.IsPartOf("Ninja cave"))
            {
                return false;
            }

            Point3D loc;
            Map map;

            DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));
            if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
            {
                loc = dungeon.EntranceLocation;
                map = dungeon.EntranceMap;
            }
            else
            {
                loc = Location;
                map = Map;
            }

            Point3D[] list;

            if (map == Map.Trammel)
            {
                list = m_TrammelDeathDestinations;
            }
            else if (map == Map.Ilshenar)
            {
                list = m_IlshenarDeathDestinations;
            }
            else if (map == Map.Malas)
            {
                list = m_MalasDeathDestinations;
            }
            else if (map == Map.Tokuno)
            {
                list = m_TokunoDeathDestinations;
            }
            else
            {
                return false;
            }

            Point3D dest = Point3D.Zero;
            int sqDistance = int.MaxValue;

            for (int i = 0; i < list.Length; i++)
            {
                Point3D curDest = list[i];

                int width = loc.X - curDest.X;
                int height = loc.Y - curDest.Y;
                int curSqDistance = width * width + height * height;

                if (curSqDistance < sqDistance)
                {
                    dest = curDest;
                    sqDistance = curSqDistance;
                }
            }

            MoveToWorld(dest, map);
            return true;
        }

        private void SendYoungDeathNotice()
        {
            SendGump(new YoungDeathNotice());
        }
        #endregion

        #region Speech
        private SpeechLog m_SpeechLog;
        private bool m_TempSquelched;

        public SpeechLog SpeechLog => m_SpeechLog;

        [CommandProperty(AccessLevel.Administrator)]
        public bool TempSquelched { get => m_TempSquelched; set => m_TempSquelched = value; }

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (SpeechLog.Enabled && NetState != null)
            {
                if (m_SpeechLog == null)
                {
                    m_SpeechLog = new SpeechLog();
                }

                m_SpeechLog.Add(e.Mobile, e.Speech);
            }
        }

        public override void OnSaid(SpeechEventArgs e)
        {
            if (m_TempSquelched)
            {
                SendLocalizedMessage(500168); // You can not say anything, you have been muted.
                e.Blocked = true;
            }
            else
            {
                base.OnSaid(e);
            }
        }
        #endregion

        #region Champion Titles
        [CommandProperty(AccessLevel.GameMaster)]
        public bool DisplayChampionTitle { get => GetFlag(PlayerFlag.DisplayChampionTitle); set => SetFlag(PlayerFlag.DisplayChampionTitle, value); }

        private ChampionTitleInfo m_ChampionTitles;

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionTitleInfo ChampionTitles { get => m_ChampionTitles; set { } }

        [PropertyObject]
        public class ChampionTitleInfo
        {
            public static TimeSpan LossDelay = TimeSpan.FromDays(1.0);
            public const int LossAmount = 90;

            private class TitleInfo
            {
                private int m_Value;
                private DateTime m_LastDecay;

                public int Value { get => m_Value; set => m_Value = value; }
                public DateTime LastDecay { get => m_LastDecay; set => m_LastDecay = value; }

                public TitleInfo()
                { }

                public TitleInfo(GenericReader reader)
                {
                    int version = reader.ReadEncodedInt();

                    switch (version)
                    {
                        case 0:
                            {
                                m_Value = reader.ReadEncodedInt();
                                m_LastDecay = reader.ReadDateTime();
                                break;
                            }
                    }
                }

                public static void Serialize(GenericWriter writer, TitleInfo info)
                {
                    writer.WriteEncodedInt(0); // version

                    writer.WriteEncodedInt(info.m_Value);
                    writer.Write(info.m_LastDecay);
                }
            }

            private TitleInfo[] m_Values;

            private int m_Harrower; //Harrower titles do NOT decay

            public int GetValue(ChampionSpawnType type)
            {
                return GetValue((int)type);
            }

            public void SetValue(ChampionSpawnType type, int value)
            {
                SetValue((int)type, value);
            }

            public void Award(ChampionSpawnType type, int value)
            {
                Award((int)type, value);
            }

            public int GetValue(int index)
            {
                if (m_Values == null || index < 0 || index >= m_Values.Length)
                {
                    return 0;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                return m_Values[index].Value;
            }

            public DateTime GetLastDecay(int index)
            {
                if (m_Values == null || index < 0 || index >= m_Values.Length)
                {
                    return DateTime.MinValue;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                return m_Values[index].LastDecay;
            }

            public void SetValue(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (value < 0)
                {
                    value = 0;
                }

                if (index < 0 || index >= m_Values.Length)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                m_Values[index].Value = value;
            }

            public void Award(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (index < 0 || index >= m_Values.Length || value <= 0)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                m_Values[index].Value += value;
            }

            public void Atrophy(int index, int value)
            {
                if (m_Values == null)
                {
                    m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                if (index < 0 || index >= m_Values.Length || value <= 0)
                {
                    return;
                }

                if (m_Values[index] == null)
                {
                    m_Values[index] = new TitleInfo();
                }

                int before = m_Values[index].Value;

                if (m_Values[index].Value - value < 0)
                {
                    m_Values[index].Value = 0;
                }
                else
                {
                    m_Values[index].Value -= value;
                }

                if (before != m_Values[index].Value)
                {
                    m_Values[index].LastDecay = DateTime.UtcNow;
                }
            }

            public override string ToString()
            {
                return "...";
            }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Abyss { get => GetValue(ChampionSpawnType.Abyss); set => SetValue(ChampionSpawnType.Abyss, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Arachnid { get => GetValue(ChampionSpawnType.Arachnid); set => SetValue(ChampionSpawnType.Arachnid, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ColdBlood { get => GetValue(ChampionSpawnType.ColdBlood); set => SetValue(ChampionSpawnType.ColdBlood, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int ForestLord { get => GetValue(ChampionSpawnType.ForestLord); set => SetValue(ChampionSpawnType.ForestLord, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int SleepingDragon { get => GetValue(ChampionSpawnType.SleepingDragon); set => SetValue(ChampionSpawnType.SleepingDragon, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int UnholyTerror { get => GetValue(ChampionSpawnType.UnholyTerror); set => SetValue(ChampionSpawnType.UnholyTerror, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int VerminHorde { get => GetValue(ChampionSpawnType.VerminHorde); set => SetValue(ChampionSpawnType.VerminHorde, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Harrower { get => m_Harrower; set => m_Harrower = value; }

            #region Mondain's Legacy Peerless Champion
            [CommandProperty(AccessLevel.GameMaster)]
            public int Glade { get => GetValue(ChampionSpawnType.Glade); set => SetValue(ChampionSpawnType.Glade, value); }

            [CommandProperty(AccessLevel.GameMaster)]
            public int Corrupt { get => GetValue(ChampionSpawnType.Corrupt); set => SetValue(ChampionSpawnType.Corrupt, value); }
            #endregion

            public ChampionTitleInfo()
            { }

            public ChampionTitleInfo(GenericReader reader)
            {
                int version = reader.ReadEncodedInt();

                switch (version)
                {
                    case 0:
                        {
                            m_Harrower = reader.ReadEncodedInt();

                            int length = reader.ReadEncodedInt();
                            m_Values = new TitleInfo[length];

                            for (int i = 0; i < length; i++)
                            {
                                m_Values[i] = new TitleInfo(reader);
                            }

                            if (m_Values.Length != ChampionSpawnInfo.Table.Length)
                            {
                                TitleInfo[] oldValues = m_Values;
                                m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

                                for (int i = 0; i < m_Values.Length && i < oldValues.Length; i++)
                                {
                                    m_Values[i] = oldValues[i];
                                }
                            }
                            break;
                        }
                }
            }

            public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
            {
                writer.WriteEncodedInt(0); // version

                writer.WriteEncodedInt(titles.m_Harrower);

                int length = titles.m_Values.Length;
                writer.WriteEncodedInt(length);

                for (int i = 0; i < length; i++)
                {
                    if (titles.m_Values[i] == null)
                    {
                        titles.m_Values[i] = new TitleInfo();
                    }

                    TitleInfo.Serialize(writer, titles.m_Values[i]);
                }
            }

            public static void CheckAtrophy(PlayerMobile pm)
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                {
                    return;
                }

                if (t.m_Values == null)
                {
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if (t.GetLastDecay(i) + LossDelay < DateTime.UtcNow)
                    {
                        t.Atrophy(i, LossAmount);
                    }
                }
            }

            public static void AwardHarrowerTitle(PlayerMobile pm)
            //Called when killing a harrower.  Will give a minimum of 1 point.
            {
                ChampionTitleInfo t = pm.m_ChampionTitles;
                if (t == null)
                {
                    return;
                }

                if (t.m_Values == null)
                {
                    t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];
                }

                int count = 1;

                for (int i = 0; i < t.m_Values.Length; i++)
                {
                    if (t.m_Values[i].Value > 900)
                    {
                        count++;
                    }
                }

                t.m_Harrower = Math.Max(count, t.m_Harrower); //Harrower titles never decay.
            }

            public bool HasChampionTitle(PlayerMobile pm)
            {
                if (m_Harrower > 0)
                {
                    return true;
                }

                if (m_Values == null)
                {
                    return false;
                }

                for (var index = 0; index < m_Values.Length; index++)
                {
                    TitleInfo info = m_Values[index];

                    if (info.Value > 300)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion

        #region Recipes
        private Dictionary<int, bool> m_AcquiredRecipes;

        public virtual bool HasRecipe(Recipe r)
        {
            if (r == null)
            {
                return false;
            }

            return HasRecipe(r.ID);
        }

        public virtual bool HasRecipe(int recipeID)
        {
            if (m_AcquiredRecipes != null && m_AcquiredRecipes.TryGetValue(recipeID, out bool value))
            {
                return value;
            }

            return false;
        }

        public virtual void AcquireRecipe(Recipe r)
        {
            if (r != null)
            {
                AcquireRecipe(r.ID);
            }
        }

        public virtual void AcquireRecipe(int recipeID)
        {
            if (m_AcquiredRecipes == null)
            {
                m_AcquiredRecipes = new Dictionary<int, bool>();
            }

            m_AcquiredRecipes[recipeID] = true;
        }

        public virtual void ResetRecipes()
        {
            m_AcquiredRecipes = null;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int KnownRecipes
        {
            get
            {
                if (m_AcquiredRecipes == null)
                {
                    return 0;
                }

                return m_AcquiredRecipes.Count;
            }
        }
        #endregion

        #region Buff Icons
        public void ResendBuffs()
        {
            if (m_BuffTable == null)
            {
                return;
            }

            NetState state = NetState;

            if (state != null)
            {
                foreach (BuffInfo info in m_BuffTable.Values)
                {
                    state.Send(new AddBuffPacket(this, info));
                }
            }
        }

        private Dictionary<BuffIcon, BuffInfo> m_BuffTable;

        public void AddBuff(BuffInfo b)
        {
            if (b == null)
            {
                return;
            }

            RemoveBuff(b); //Check & subsequently remove the old one.

            if (m_BuffTable == null)
            {
                m_BuffTable = new Dictionary<BuffIcon, BuffInfo>();
            }

            m_BuffTable.Add(b.ID, b);

            NetState state = NetState;

            state?.Send(new AddBuffPacket(this, b));
        }

        public void RemoveBuff(BuffInfo b)
        {
            if (b == null)
            {
                return;
            }

            RemoveBuff(b.ID);
        }

        public void RemoveBuff(BuffIcon b)
        {
            if (m_BuffTable == null || !m_BuffTable.TryGetValue(b, out BuffInfo value))
            {
                return;
            }

            if (value.Timer != null && value.Timer.Running)
            {
                value.Timer.Stop();
            }

            m_BuffTable.Remove(b);

            NetState state = NetState;

            state?.Send(new RemoveBuffPacket(this, b));

            if (m_BuffTable.Count <= 0)
            {
                m_BuffTable = null;
            }
        }
        #endregion

        [CommandProperty(AccessLevel.GameMaster)]
        public ExploringTheDeepQuestChain ExploringTheDeepQuest { get; set; }

        public void AutoStablePets()
        {
            if (AllFollowers.Count > 0)
            {
                for (int i = m_AllFollowers.Count - 1; i >= 0; --i)
                {
                    BaseCreature pet = AllFollowers[i] as BaseCreature;

                    if (pet == null)
                    {
                        continue;
                    }

                    if (pet.Summoned && pet.Map != Map)
                    {
                        pet.PlaySound(pet.GetAngerSound());

                        Timer.DelayCall(pet.Delete);

                        continue;
                    }

                    if (!pet.CanAutoStable || Stabled.Count >= AnimalTrainer.GetMaxStabled(this))
                    {
                        continue;
                    }

                    pet.ControlTarget = null;
                    pet.ControlOrder = LastOrderType.Stay;
                    pet.Internalize();

                    pet.SetControlMaster(null);
                    pet.SummonMaster = null;

                    pet.IsStabled = true;
                    pet.StabledBy = this;

                    Stabled.Add(pet);
                    m_AutoStabled.Add(pet);
                }
            }
        }

        public void ClaimAutoStabledPets()
        {
            if (!Region.AllowAutoClaim(this) || m_AutoStabled.Count <= 0)
            {
                return;
            }

            if (!Alive)
            {
                SendGump(new ReLoginClaimGump());
                return;
            }

            for (int i = m_AutoStabled.Count - 1; i >= 0; --i)
            {
                BaseCreature pet = m_AutoStabled[i] as BaseCreature;

                if (pet == null || pet.Deleted)
                {
                    if (pet != null)
                    {
                        pet.IsStabled = false;
                        pet.StabledBy = null;

                        if (Stabled.Contains(pet))
                        {
                            Stabled.Remove(pet);
                        }
                    }

                    continue;
                }

                if (Followers + pet.ControlSlots <= FollowersMax)
                {
                    pet.SetControlMaster(this);

                    if (pet.Summoned)
                    {
                        pet.SummonMaster = this;
                    }

                    pet.FollowTarget = this;
                    pet.ControlOrder = LastOrderType.Follow;

                    pet.MoveToWorld(Location, Map);

                    pet.IsStabled = false;
                    pet.StabledBy = null;

                    if (Stabled.Contains(pet))
                    {
                        Stabled.Remove(pet);
                    }
                }
                else
                {
                    SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
                }
            }

            m_AutoStabled.Clear();
        }
    }
}
