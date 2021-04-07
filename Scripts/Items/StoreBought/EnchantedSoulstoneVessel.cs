using Server.Engines.VeteranRewards;
using Server.Network;
using System;

namespace Server.Items
{
    public class EnchantedSoulstoneVessel : Container, IRewardItem
    {
        public override int LabelNumber => 1126839; // Enchanted Soulstone Vessel

        public override int DefaultGumpID => 0x9D54;

        private Mobile m_Owned;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owned { get => m_Owned; set { m_Owned = value; InvalidateProperties(); } }

        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem { get => m_IsRewardItem; set { m_IsRewardItem = value; InvalidateProperties(); } }

        public override int DefaultMaxItems => 20;

        public bool IsFull => DefaultMaxItems <= Items.Count;

        [Constructable]
        public EnchantedSoulstoneVessel()
            : base(0xA73F)
        {
            LootType = LootType.Blessed;
            Weight = 10.0;
        }

        public EnchantedSoulstoneVessel(Serial serial)
            : base(serial)
        {
        }

        public bool CheckAccount(Mobile from)
        {
            return Owned != null && Owned.Account == from.Account;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                base.OnDoubleClick(from);

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else if (Owned == null)
            {
                from.SendLocalizedMessage(1159625); // This item can only be used when secured within a house by its account owner.
            }
            else if (!CheckAccount(from))
            {
                from.SendLocalizedMessage(1159624); // This item does not belong to this account.
            }
            else
            {
                base.OnDoubleClick(from);
            }
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
            typeof(SoulStone)
        };

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (Owned == null)
            {
                from.SendLocalizedMessage(1159625); // This item can only be used when secured within a house by its account owner.
                return false;
            }

            if (!CheckAccount(from))
            {
                from.SendLocalizedMessage(1159624); // This item does not belong to this account.
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
            if (Owned == null)
            {
                from.SendLocalizedMessage(1159625); // This item can only be used when secured within a house by its account owner.
                return false;
            }

            if (!CheckAccount(from))
            {
                from.SendLocalizedMessage(1159624); // This item does not belong to this account.
                return false;
            }

            if (!IsAccept(item))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDropInto(from, item, p);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (Owned != null)
            {
                list.Add(1072304, Owned.Name); // Owned by ~1_name~
                list.Add(1155526); // Account Bound
            }
            else
            {
                list.Add(1159626); // Account Bound Once Secured
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
