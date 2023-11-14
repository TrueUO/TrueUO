using Server.Engines.VeteranRewards;
using Server.Network;
using System;

namespace Server.Items
{
    public class EnchantedSoulstoneVessel : Container, IRewardItem
    {
        public override int LabelNumber => 1126839; // Enchanted Soulstone Vessel

        public override int DefaultGumpID => 0x9D54;

        private Mobile _Owned;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owned { get => _Owned; set { _Owned = value; InvalidateProperties(); } }

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

        private static bool WillAccept(Item item)
        {
            if (item.GetType() == typeof(SoulStone) || item.GetType().IsSubclassOf(typeof(SoulStone)))
            {
                return true;
            }

            return false;
        }

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

            if (!WillAccept(dropped))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            if (IsFull)
            {
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

            if (!WillAccept(item))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            if (IsFull)
            {
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
            writer.Write(1);

            writer.Write(_Owned);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                {
                    _Owned = reader.ReadMobile();
                    break;
                }
            }
        }
    }
}
