using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    [Flipable(0x44CF, 0x44D0)]
    public class LobsterTrap : Item, IBaitable
    {
        private Type m_BaitType;

        [CommandProperty(AccessLevel.GameMaster)]
        public Type BaitType
        {
            get => m_BaitType;
            set
            {
                m_BaitType = value;

                if (m_BaitType == null)
                {
                    m_EnhancedBait = false;
                }

                InvalidateProperties();
            }
        }

        private bool m_EnhancedBait;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool EnhancedBait { get => m_EnhancedBait; set { m_EnhancedBait = value; InvalidateProperties(); } }

        public List<Type> Caught;

        public override int LabelNumber => IsFull ? 1149599 : 1116389; // full lobster trap || empty lobster trap

        public bool IsFull => Caught != null && Caught.Count > 0;

        [Constructable]
        public LobsterTrap()
            : this(1)
        {
        }

        [Constructable]
        public LobsterTrap(int amount)
            : base(0x44CF)
        {
            Weight = 5.0;
            Stackable = true;
            Amount = amount;
        }

        public override void OnAfterDuped(Item newItem)
        {
            if (!(newItem is LobsterTrap trap))
                return;

            trap.BaitType = m_BaitType;
            trap.EnhancedBait = m_EnhancedBait;

            base.OnAfterDuped(newItem);
        }

        public override bool WillStack(Mobile from, Item item)
        {
            if (IsFull || item is LobsterTrap trap && (trap.BaitType != BaitType || trap.EnhancedBait != EnhancedBait))
            {
                return false;
            }

            return base.WillStack(from, item);
        }

        public void CheckTrap()
        {
            if (IsFull)
            {
                ItemID = 0x44D0;
            }
            else
            {
                ItemID = 0x44CF;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!IsFull && m_BaitType != null)
            {
                object label = FishInfo.GetFishLabel(m_BaitType);

                if (label is int i)
                    list.Add(1116468, $"#{i}"); // baited to attract: ~1_val~
                else if (label is string s)
                    list.Add(1116468, s);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Caught != null && Caught.Count > 0)
            {
                DumpContents(from);
                InvalidateProperties();
                return;
            }

            if (from.Mounted || from.Flying)
            {
                if (IsChildOf(from.Backpack))
                {
                    from.SendLocalizedMessage(500971); // You can't fish while riding!
                }
                else
                {
                    PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500971, from.NetState); // You can't fish while riding!
                }

                return;
            }

            from.SendLocalizedMessage(500974); // What water do you want to fish in?
            from.BeginTarget(-1, true, TargetFlags.None, OnTarget);
        }

        private void DumpContents(Mobile from)
        {
            ItemID = 0x44CF;

            Container pack = from.Backpack;

            foreach (var t in Caught)
            {
                Item item = Loot.Construct(t);

                if (item is RareCrabAndLobster fish)
                {
                    fish.Fisher = from;
                    fish.DateCaught = DateTime.UtcNow;
                    fish.Weight = Utility.RandomMinMax(10, 200);
                    fish.Stackable = false;
                }

                if (item != null)
                {
                    if (!pack.TryDropItem(from, item, false))
                    {
                        item.MoveToWorld(from.Location, from.Map);
                    }

                    from.SendLocalizedMessage(1116386, $"#{item.LabelNumber}"); // You remove ~1_ITEM~from the trap and put it in your pack.
                }
            }

            Caught = null;
            CheckTrap();
        }

        public void OnTarget(Mobile from, object targeted)
        {
            if (Deleted)
                return;

            IPoint3D pnt = (IPoint3D)targeted;
            Map map = from.Map;

            if (map == null || map == Map.Internal)
                return;

            if (!from.InLOS(targeted))
            {
                from.SendLocalizedMessage(500979); // You cannot see that location.
            }
            else if (!from.InRange(pnt, 6))
            {
                from.SendLocalizedMessage(1116388); // The trap is too cumbersome to deploy that far away.
            }
            else if (!IsValidLocation(pnt.X, pnt.Y, pnt.Z, map))
            {
                from.SendLocalizedMessage(1116393); // The location is too close to another trap.
            }
            else
            {
                var lava = this is LavaLobsterTrap;

                if (IsValidTile(from, targeted, lava))
                {
                    from.RevealingAction();

                    var mechanism = new LobsterTrapMechanism(from, BaitType, EnhancedBait);

                    if (lava)
                    {
                        mechanism.Hue = 2515;
                    }

                    mechanism.MoveToWorld(new Point3D(pnt.X, pnt.Y, pnt.Z), map);

                    Consume();
                }
            }
        }

        public virtual bool IsValidTile(Mobile from, object targeted, bool lava)
        {
            bool isWater = false;
            bool isLava = false;

            IPoint3D pnt = (IPoint3D)targeted;

            if (targeted is LandTarget landTarget)
            {
                isWater = IsNotShallowWaterLand(from.Map, pnt);
                isLava = LaveTileValidate(landTarget.TileID);
            }
            else if (targeted is StaticTarget staticTarget)
            {
                isLava = LaveTileValidate(staticTarget.ItemID);
                isWater = IsNotShallowWaterStaticTile(from.Map, pnt);
            }
            else
            {
                from.SendLocalizedMessage(500977); // You can't reach the water there.
                return false;
            }

            if (!isLava && isWater && lava)
            {
                from.SendLocalizedMessage(1149622); // You need lava to fish in!
                return false;
            }

            if (!isWater && isLava && !lava)
            {
                from.SendLocalizedMessage(500978); // You need water to fish in!
                return false;
            }

            if (!isWater && !lava || !isLava && lava || lava && !from.Region.IsPartOf("Abyss"))
            {
                from.SendLocalizedMessage(1116695); // The water there is too shallow for the trap.
                return false;
            }

            return true;
        }

        public static bool IsNotShallowWaterStaticTile(Map map, IPoint3D pnt)
        {
            bool water = true;

            Misc.Geometry.Circle2D(new Point3D(pnt.X, pnt.Y, pnt.Z), map, 5, (p, m) =>
            {
                StaticTile[] stile = map.Tiles.GetStaticTiles(p.X, p.Y, false);

                if (stile.Length > 0)
                {
                    for (var index = 0; index < stile.Length; index++)
                    {
                        StaticTile st = stile[index];

                        if (st.Z == pnt.Z)
                        {
                            int id = (st.ID & 0x3FFF) | 0x4000;

                            if (WaterTileValidate(id) && water)
                            {
                                water = true;
                            }
                            else
                            {
                                water = false;
                            }
                        }
                    }
                }
            });

            return water;
        }

        public static bool IsNotShallowWaterLand(Map map, IPoint3D pnt)
        {
            bool water = true;

            Misc.Geometry.Circle2D(new Point3D(pnt.X, pnt.Y, pnt.Z), map, 5, (p, m) =>
            {
                LandTile ltile = map.Tiles.GetLandTile(p.X, p.Y);

                if (WaterTileValidate(ltile.ID) && water)
                {
                    water = true;
                }
                else
                {
                    water = false;
                }
            });

            return water;
        }

        public static bool WaterTileValidate(int tileID)
        {
            var tiles = Engines.Harvest.Fishing.WaterTiles;

            bool contains = false;

            for (int i = 0; !contains && i < tiles.Length; i += 2)
                contains = tileID >= tiles[i] && tileID <= tiles[i + 1];

            return contains;
        }

        public bool LaveTileValidate(int tileID)
        {
            var tiles = Engines.Harvest.Fishing.LavaTiles;

            for (var index = 0; index < tiles.Length; index++)
            {
                int id = tiles[index];

                if (tileID == id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsValidLocation(int x, int y, int z, Map map)
        {
            IPooledEnumerable eable = map.GetItemsInRange(new Point3D(x, y, z), 1);

            foreach (Item item in eable)
            {
                if (item is LobsterTrapMechanism)
                {
                    eable.Free();
                    return false;
                }
            }

            eable.Free();

            return true;
        }

        public LobsterTrap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            int index = FishInfo.GetIndexFromType(m_BaitType);
            writer.Write(index);
            writer.Write(m_EnhancedBait);
            writer.Write(Caught == null ? 0 : Caught.Count);

            if (Caught != null)
            {
                foreach (var c in Caught)
                {
                    writer.Write(c.Name);
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            int index = reader.ReadInt();
            m_BaitType = index == -1 ? null : FishInfo.GetTypeFromIndex(index);
            m_EnhancedBait = reader.ReadBool();

            int count = reader.ReadInt();

            if (count > 0)
            {
                Caught = new List<Type>();
            }

            for (int i = 0; i < count; i++)
            {
                Type t = ScriptCompiler.FindTypeByName(reader.ReadString());

                if (t != null)
                    Caught.Add(t);
            }
        }
    }

    public class LobsterTrapMechanism : Item, IBaitable
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public Type BaitType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool EnhancedBait { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool InUse { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsLava => Hue == 2515;

        [CommandProperty(AccessLevel.GameMaster)]
        public int TickCount { get; set; }

        private List<Type> Caught;

        [Constructable]
        public LobsterTrapMechanism(Mobile owner, Type bait, bool enhanced)
            : base(0x44CC)
        {
            Owner = owner;
            BaitType = bait;
            EnhancedBait = enhanced;

            Weight = 0.0;
            Movable = false;

            Caught = new List<Type>();

            StartTimer();
        }

        public override bool IsVirtualItem => true;

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Owner == null)
                list.Add(1096487); // lobster trap
            else
                list.Add(1116390, Owner.Name);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (BaitType != null)
            {
                object label = FishInfo.GetFishLabel(BaitType);

                if (label is int i)
                    list.Add(1116468, $"#{i}"); // baited to attract: ~1_val~
                else if (label is string s)
                    list.Add(1116468, s);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (CanUseTrap(from))
                EndTimer(from);
        }

        private Timer m_Timer;

        public void StartTimer()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            InUse = true;

            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), OnTick);
        }

        public void EndTimer(Mobile from)
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }

            LobsterTrap trap;

            if (IsLava)
            {
                trap = new LavaLobsterTrap();
            }
            else
            {
                trap = new LobsterTrap();
            }

            if (Caught.Count > 0)
            {
                trap.Caught = Caught;
                trap.CheckTrap();
            }

            if (from.Backpack == null || !from.Backpack.TryDropItem(from, trap, false))
                trap.MoveToWorld(from.Location, from.Map);

            Delete();
        }

        public void OnTick()
        {
            TickCount++;

            int lostChance = TickCount * 5;
            int random = Utility.Random(100);

            if (lostChance > random)
            {
                OnTrapLost();
            }
            else
            {
                bool skip = Utility.RandomBool();

                if (!skip && Caught.Count <= 5)
                {
                    GetReward(TickCount);
                }
            }
        }

        public void GetReward(int value)
        {
            double bump = value / 100.0;

            Type type = FishInfo.GetSpecialItem(Owner, this, Location, bump, IsLava);

            if (type == null)
            {
                if (IsLava)
                {
                    if (Owner != null)
                    {
                        Owner.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 503168, Owner.NetState); // The fish don't seem to be biting here.
                    }
                }
                else
                {
                    if (Utility.RandomBool())
                        type = typeof(Crab);
                    else
                        type = typeof(Lobster);
                }
            }

            if (type != null)
            {
                if (Owner != null)
                {
                    Owner.CheckSkill(SkillName.Fishing, 0, Owner.Skills[SkillName.Fishing].Cap);
                }

                PublicOverheadMessage(MessageType.Regular, 0, 1116364); // **bob**

                ItemID = 0x44CB;
                Timer.DelayCall(TimeSpan.FromSeconds(5), () => ItemID = 0x44CC);

                Caught.Add(type);
            }
        }

        public void OnTrapLost()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            Effects.SendPacket(Location, Map, new GraphicalEffect(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, 0x352D, Location, Location, 4, 16, true, true));

            IPooledEnumerable eable = GetMobilesInRange(12);

            foreach (Mobile mob in eable)
            {
                if (mob is PlayerMobile && Owner != null)
                    mob.SendLocalizedMessage(1116385, Owner.Name); //~1_NAME~'s trap bouy is pulled beneath the waves.
            }
            eable.Free();

            Delete();
        }

        private bool CanUseTrap(Mobile from)
        {
            if (Owner == null || RootParent != null)
                return false;

            if (!from.InRange(Location, 6))
            {
                from.SendLocalizedMessage(500295); //You are too far away to do that.
                return false;
            }

            //is owner, or in same guild
            if (Owner == from || from.Guild != null && from.Guild == Owner.Guild)
                return true;

            //partied
            if (Party.Get(from) == Party.Get(Owner))
                return true;

            //fel rules
            if (from.Map != null && from.Map.Rules == MapRules.FeluccaRules)
            {
                from.CriminalAction(true);
                from.SendLocalizedMessage(1149823); //The owner of the lobster trap notices you committing a criminal act!

                return true;
            }

            from.SendLocalizedMessage(1116391); //You realize that the trap isn't yours so you leave it alone.
            return false;
        }

        public LobsterTrapMechanism(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            int index = FishInfo.GetIndexFromType(BaitType);
            writer.Write(index);
            writer.Write(EnhancedBait);
            writer.Write(Owner);
            writer.Write(InUse);
            writer.Write(Caught == null ? 0 : Caught.Count);

            if (Caught != null)
            {
                foreach (var c in Caught)
                {
                    writer.Write(c.Name);
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            int index = reader.ReadInt();
            BaitType = index == -1 ? null : FishInfo.GetTypeFromIndex(index);
            EnhancedBait = reader.ReadBool();
            Owner = reader.ReadMobile();
            InUse = reader.ReadBool();

            Caught = new List<Type>();

            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                Type t = ScriptCompiler.FindTypeByName(reader.ReadString());

                if (t != null)
                    Caught.Add(t);
            }

            if (InUse)
                StartTimer();
        }
    }
}
