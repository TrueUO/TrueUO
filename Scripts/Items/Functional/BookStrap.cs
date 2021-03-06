using Server.ContextMenus;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class BookStrap : Container, ISecurable, IRewardItem
    {
        public override int LabelNumber => 1126807; // book strap

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level { get; set; }

        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get { return m_IsRewardItem; }
            set { m_IsRewardItem = value; InvalidateProperties(); }
        }

        public override int DefaultMaxItems => 25;

        public bool IsFull => DefaultMaxItems <= Items.Count;

        [Constructable]
        public BookStrap()
            : base(0xA71F)
        {
            LootType = LootType.Blessed;
            Weight = 3.0;
            Level = SecureLevel.CoOwners;
        }

        public override bool DisplaysContent => false;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            if (m_IsRewardItem)
                list.Add(1076219); // 3rd Year Veteran Reward

            base.AddWeightProperty(list);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1073841, "{0}\t{1}\t{2}", TotalItems, MaxItems, TotalWeight);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                base.OnDoubleClick(from);

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else
            {
                base.OnDoubleClick(from);
            }
        }

        public bool CheckAccessible(Mobile from, Item item)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true; // Staff can access anything

            BaseHouse house = BaseHouse.FindHouseAt(item);

            if (house == null)
                return false;

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

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public bool IsAccept(Item item)
        {
            foreach (Type type in _AcceptList)
                if (item.GetType().IsSubclassOf(type))
                    return true;

            return false;
        }

        private readonly Type[] _AcceptList =
        {
            typeof(Spellbook)
        };

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!CheckAccessible(from, this))
            {
                PrivateOverheadMessage(MessageType.Regular, 946, 1010563, from.NetState); // This container is secure.
                return false;
            }

            if (!IsAccept(dropped))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDrop(from, dropped);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!CheckAccessible(from, this))
            {
                PrivateOverheadMessage(MessageType.Regular, 946, 1010563, from.NetState); // This container is secure.
                return false;
            }

            if (!IsAccept(item))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDropInto(from, item, p);
        }

        public BookStrap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)Level);
            writer.Write(m_IsRewardItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Level = (SecureLevel)reader.ReadInt();
            m_IsRewardItem = reader.ReadBool();
        }
    }
}
