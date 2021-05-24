using Server.ContextMenus;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Network;

namespace Server.Engines.Plants
{
    [Flipable(19288, 19290)]
    public class SeedBox : Container, IRewardItem, ISecurable
    {
        public static readonly int MaxSeeds = 5000;
        public static readonly int MaxUnique = 300;

        public override int DefaultMaxItems => MaxUnique;
        public override bool DisplaysContent => false;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem { get; set; }

        public List<SeedEntry> Entries { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TotalCount
        {
            get
            {
                int count = 0;

                if (Entries != null)
                {
                    for (var index = 0; index < Entries.Count; index++)
                    {
                        var e = Entries[index];

                        count += e == null || e.Seed == null ? 0 : e.Seed.Amount;
                    }
                }

                return count;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UniqueCount => Entries == null ? 0 : Entries.Count(e => e != null && e.Seed != null && e.Seed.Amount > 0);

        public override int DefaultMaxWeight => 0;
        public override double DefaultWeight => 10.0;

        [Constructable]
        public SeedBox()
            : base(19288)
        {
            Entries = new List<SeedEntry>();
            LootType = LootType.Blessed;
        }

        public override int GetTotal(TotalType type)
        {
            return 0;
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m.InRange(GetWorldLocation(), 3) && m is PlayerMobile mobile)
            {
                if (CheckAccessible(m, this))
                {
                    BaseGump.SendGump(new SeedBoxGump(mobile, this));
                }
                else
                {
                    PrivateOverheadMessage(MessageType.Regular, 946, 1010563, m.NetState); // This container is secure.
                }
            }
            else
            {
                m.SendLocalizedMessage(500446); // That is too far away.
            }

        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            SetSecureLevelEntry.AddTo(from, this, list);
        }        

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is Seed seed)
            {
                return TryAddSeed(from, seed);
            }

            from.SendLocalizedMessage(1151838); // This item cannot be stored in the seed box.
            return false;
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            return false;
        }

        private bool CheckAccessible(Mobile from, Item item)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                return true; // Staff can access anything
            }

            BaseHouse house = BaseHouse.FindHouseAt(item);

            if (house == null)
            {
                return false;
            }

            switch (Level)
            {
                case SecureLevel.Owner: return house.IsOwner(from);
                case SecureLevel.CoOwners: return house.IsCoOwner(from);
                case SecureLevel.Friends: return house.IsFriend(from);
                case SecureLevel.Anyone: return true;
                case SecureLevel.Guild: return house.IsGuildMember(from);
            }

            return false;
        }

        public bool TryAddSeed(Mobile from, Seed seed, int index = -1)
        {
            if (!CheckAccessible(from, seed))
            {
                PrivateOverheadMessage(MessageType.Regular, 946, 1010563, from.NetState); // This container is secure.
                return false;
            }

            if (!from.Backpack.CheckHold(from, seed, true, true) || seed.Amount <= 0)
            {
                return false;
            }

            if (!from.InRange(GetWorldLocation(), 3) || from.Map != Map)
            {
                return false;
            }

            if (TotalCount + seed.Amount <= MaxSeeds)
            {
                SeedEntry entry = GetExisting(seed);

                if (entry != null)
                {
                    entry.Seed.Amount += seed.Amount;
                    seed.Delete();
                }
                else if (UniqueCount < MaxUnique)
                {
                    entry = new SeedEntry(seed);
                    DropItem(seed);

                    seed.Movable = false;
                }
                else
                {
                    from.SendLocalizedMessage(1151839); // There is not enough room in the box.
                }

                if (entry != null)
                {
                    InvalidateProperties();

                    if (Entries.Contains(entry))
                    {
                        if (index > -1 && index < Entries.Count - 1)
                        {
                            Entries.Remove(entry);
                            AddEntry(entry, index);
                        }
                    }
                    else
                    {
                        if (index > -1 && index < Entries.Count)
                        {
                            AddEntry(entry, index);
                        }
                        else
                            AddEntry(entry);
                    }

                    from.SendLocalizedMessage(1151846); // You put the seed in the seedbox.

                    if (from is PlayerMobile mobile)
                    {
                        var gump = mobile.FindGump<SeedBoxGump>();

                        if (gump != null)
                        {
                            gump.CheckPage(entry);
                            gump.Refresh();
                        }
                        else
                        {
                            gump = new SeedBoxGump(mobile, this);
                            gump.CheckPage(entry);

                            BaseGump.SendGump(gump);
                        }
                    }

                    return true;
                }
            }
            else
            {
                from.SendLocalizedMessage(1151839); // There is not enough room in the box.
            }

            return false;
        }

        private void AddEntry(SeedEntry entry, int index = -1)
        {
            if (index == -1)
            {
                TrimEntries();
                Entries.Add(entry);
            }
            else if (index >= 0 && index < Entries.Count)
            {
                Entries.Insert(index, entry);
            }

            if (Entries.Count > 0)
            {
                if (ItemID == 19288)
                {
                    ItemID = 19289;
                }
                else if (ItemID == 19290)
                {
                    ItemID = 19291;
                }
            }
        }

        private void RemoveEntry(SeedEntry entry)
        {
            Entries.Remove(entry);
            TrimEntries();

            if (Entries.Count == 0)
            {
                if (ItemID == 19289)
                {
                    ItemID = 19288;
                }
                else if (ItemID == 19291)
                {
                    ItemID = 19290;
                }
            }
        }

        public void DropSeed(Mobile from, SeedEntry entry, int amount)
        {
            if (!from.InRange(GetWorldLocation(), 3))
            {
                return;
            }

            if (amount > entry.Seed.Amount)
                amount = entry.Seed.Amount;

            Seed seed;

            if (amount == entry.Seed.Amount)
            {
                seed = entry.Seed;
                entry.Seed = null;
            }
            else
            {
                seed = new Seed(entry.Seed.PlantType, entry.Seed.PlantHue, true)
                {
                    Amount = amount
                };

                entry.Seed.Amount -= amount;
            }

            seed.Movable = true;

            if (from.Backpack == null || !from.Backpack.TryDropItem(from, seed, false))
            {
                seed.MoveToWorld(from.Location, from.Map);
                from.SendLocalizedMessage(1151844); // There is not enough room in your backpack!
            }

            if (entry.Seed != null && entry.Seed.Amount <= 0)
            {
                entry.Seed.Delete();
                entry.Seed = null;
            }

            if (entry.Seed == null || entry.Seed.Amount <= 0)
            {
                RemoveEntry(entry);
            }
        }

        public SeedEntry GetExisting(Seed seed)
        {
            for (var index = 0; index < Entries.Count; index++)
            {
                var e = Entries[index];

                if (e != null && e.Seed != null && e.Seed.PlantType == seed.PlantType && e.Seed.PlantHue == seed.PlantHue)
                {
                    return e;
                }
            }

            return null;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (IsRewardItem)
            {
                list.Add(1076220); // 4th Year Veteran Reward
            }

            list.Add(1151847, string.Format("{0}\t{1}", TotalCount.ToString(), MaxSeeds.ToString())); // Seeds in Box: ~1_val~ / ~2_val~
            list.Add(1151848, string.Format("{0}\t{1}", UniqueCount.ToString(), MaxUnique.ToString())); // Unique Seeds In Box: ~1_val~ / ~2_val~
        }

        private void CheckEntries()
        {
            List<Item> toDelete = new List<Item>(Items);

            for (var index = 0; index < toDelete.Count; index++)
            {
                Item item = toDelete[index];

                if (item != null && item.Amount == 0)
                {
                    item.Delete();
                }
            }

            List<SeedEntry> entries = new List<SeedEntry>(Entries);

            for (var index = 0; index < entries.Count; index++)
            {
                SeedEntry entry = entries[index];

                if (entry != null && (entry.Seed == null || entry.Seed.Amount == 0 || entry.Seed.Deleted))
                {
                    Entries.Remove(entry);
                }
            }

            ColUtility.Free(entries);
            ColUtility.Free(toDelete);
        }

        public void TrimEntries()
        {
            int lastIndex = Entries.FindLastIndex(e => e != null);

            if (lastIndex + 1 < Entries.Count - 1)
            {
                Entries.RemoveRange(lastIndex + 1, (Entries.Count - 1) - lastIndex);
            }

            Entries.TrimExcess();
        }

        public SeedBox(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write(IsRewardItem);
            writer.Write((int)Level);

            writer.Write(Entries.Count);
            for (int i = 0; i < Entries.Count; i++)
            {
                SeedEntry entry = Entries[i];

                if (entry == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(1);
                    entry.Serialize(writer);
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Entries = new List<SeedEntry>();

            IsRewardItem = reader.ReadBool();
            Level = (SecureLevel)reader.ReadInt();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                switch (reader.ReadInt())
                {
                    default:
                    case 0:
                        Entries.Add(null);
                        break;
                    case 1:
                        SeedEntry entry = new SeedEntry(reader);

                        if (entry.Seed != null)
                            Entries.Add(entry);
                        break;
                }
            }

            Timer.DelayCall(
                () =>
                {
                    for (var index = 0; index < Items.Count; index++)
                    {
                        Item item = Items[index];

                        if (item.Movable)
                        {
                            item.Movable = false;
                        }
                    }
                });

            Timer.DelayCall(TimeSpan.FromSeconds(10), CheckEntries);
        }
    }
}
