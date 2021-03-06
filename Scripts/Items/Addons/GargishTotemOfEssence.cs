using Server.Engines.VeteranRewards;
using Server.Multis;
using Server.Network;
using System;

namespace Server.Items
{
    public class GargishTotemOfEssence : BaseAddon, IRewardItem
    {
        public override int LabelNumber => 1032289; // totem

        private bool m_IsRewardItem;
        private int m_ResourceCount;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextResourceCount { get; set; }

        public override bool ForceShowProperties => true;

        [Constructable]
        public GargishTotemOfEssence(int itemID)
            : base(0xA725)
        {
            NextResourceCount = DateTime.UtcNow + TimeSpan.FromDays(7);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1159590, ResourceCount.ToString()); // Essences: ~1_COUNT~	
        }

        public GargishTotemOfEssence(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                GargishTotemOfEssenceDeed deed = new GargishTotemOfEssenceDeed
                {
                    IsRewardItem = m_IsRewardItem,
                    ResourceCount = m_ResourceCount
                };

                return deed;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get { return m_IsRewardItem; }
            set { m_IsRewardItem = value; UpdateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResourceCount
        {
            get
            {
                return m_ResourceCount;
            }
            set
            {
                m_ResourceCount = value;

                if (Components.Count > 0)
                {
                    if (m_ResourceCount == 0 && Components[0].ItemID != 0x4A95)
                        Components[0].ItemID = 0x4A95;
                    else if (m_ResourceCount > 0 && Components[0].ItemID != 0x4A94)
                        Components[0].ItemID = 0x4A94;
                }

                UpdateProperties();
            }
        }

        public override void OnComponentUsed(AddonComponent c, Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this) || !((from.Z - Z) > -3 && (from.Z - Z) < 3))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else if (house != null && house.IsOwner(from))
            {
                if (m_ResourceCount > 0)
                {
                    Item res = null;

                    switch (Utility.Random(10))
                    {
                        case 0: res = new EssenceBalance(); break;
                        case 1: res = new EssenceSingularity(); break;
                        case 2: res = new EssenceDiligence(); break;
                        case 3: res = new EssenceAchievement(); break;
                        case 4: res = new EssencePrecision(); break;
                        case 5: res = new EssencePassion(); break;
                        case 6: res = new EssenceFeeling(); break;
                        case 7: res = new EssenceOrder(); break;
                        case 8: res = new EssenceControl(); break;
                        case 9: res = new EssenceDirection(); break;
                    }

                    int amount = Math.Min(1, 10);

                    if (res != null)
                    {
                        res.Amount = amount;

                        if (!from.PlaceInBackpack(res))
                        {
                            res.Delete();
                            from.SendLocalizedMessage(1078837); // Your backpack is full! Please make room and try again.
                        }
                        else
                        {
                            ResourceCount -= amount;
                            PublicOverheadMessage(MessageType.Regular, 0, 1151834, m_ResourceCount.ToString()); // Resources: ~1_COUNT~
                        }
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1150083); // There are currently no resources to collect.
                }
            }
            else
            {
                from.SendLocalizedMessage(502092); // You must be in your house to do this.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            TryGiveResourceCount();

            writer.Write(m_IsRewardItem);
            writer.Write(m_ResourceCount);

            writer.Write(NextResourceCount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_IsRewardItem = reader.ReadBool();
            m_ResourceCount = reader.ReadInt();

            NextResourceCount = reader.ReadDateTime();
        }

        private void TryGiveResourceCount()
        {
            if (NextResourceCount < DateTime.UtcNow)
            {
                ResourceCount = Math.Min(100, m_ResourceCount + 10);
                NextResourceCount = DateTime.UtcNow + TimeSpan.FromDays(1);
            }
        }
    }

    public class GargishTotemOfEssenceDeed : BaseAddonDeed, IRewardItem
    {
        private bool m_IsRewardItem;
        private int m_ResourceCount;

        [Constructable]
        public GargishTotemOfEssenceDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public GargishTotemOfEssenceDeed(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1159556; // Gargish Totem of Essence

        public override BaseAddon Addon
        {
            get
            {
                GargishTotemOfEssence addon = new GargishTotemOfEssence(m_ResourceCount > 0 ? 0x4A94 : 0x4A95)
                {
                    IsRewardItem = m_IsRewardItem,
                    ResourceCount = m_ResourceCount
                };

                return addon;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get { return m_IsRewardItem; }
            set { m_IsRewardItem = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResourceCount
        {
            get { return m_ResourceCount; }
            set { m_ResourceCount = value; InvalidateProperties(); }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_IsRewardItem)
                list.Add(1076223); // 7th Year Veteran Reward
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_IsRewardItem && !RewardSystem.CheckIsUsableBy(from, this, null))
                return;

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
            }
            else
                base.OnDoubleClick(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_IsRewardItem);
            writer.Write(m_ResourceCount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_IsRewardItem = reader.ReadBool();
            m_ResourceCount = reader.ReadInt();
        }
    }
}
