using Server.Engines.VeteranRewards;
using Server.Multis;
using Server.Network;
using System;

namespace Server.Items
{
    public class GargishTotemOfEssenceComponent : AddonComponent
    {
        public override bool ForceShowProperties => true;

        public GargishTotemOfEssenceComponent(int id)
            : base(id)
        {
        }

        public GargishTotemOfEssenceComponent(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            Addon.GetProperties(list);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class GargishTotemOfEssence : BaseAddon
    {
        public override int LabelNumber => 1126813; // totem

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextResourceCount { get; set; }

        public override bool ForceShowProperties => true;

        [Constructable]
        public GargishTotemOfEssence()
        {
            AddComponent(new GargishTotemOfEssenceComponent(0xA725), 0, 0, 0);
            NextResourceCount = DateTime.UtcNow + TimeSpan.FromDays(7);
            Weight = 0;
        }

        public GargishTotemOfEssence(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1159590, ResourceCount.ToString()); // Essences: ~1_COUNT~	
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                GargishTotemOfEssenceDeed deed = new GargishTotemOfEssenceDeed
                {
                    IsRewardItem = IsRewardItem
                };

                return deed;
            }
        }

        private int m_ResourceCount;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResourceCount { get => m_ResourceCount; set { m_ResourceCount = value; UpdateProperties(); } }

        public override void OnComponentUsed(AddonComponent c, Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this) || !(from.Z - Z > -3 && from.Z - Z < 3))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else if (house != null && (house.IsOwner(from) || house.LockDowns.ContainsKey(this) && house.LockDowns[this] == from))
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

                    if (res != null)
                    {
                        ResourceCount--;
                        from.SendLocalizedMessage(1159557); // Essences have been placed in your backpack.
                        from.AddToBackpack(res);
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

            writer.Write(IsRewardItem);
            writer.Write(m_ResourceCount);
            writer.Write(NextResourceCount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            IsRewardItem = reader.ReadBool();
            m_ResourceCount = reader.ReadInt();
            NextResourceCount = reader.ReadDateTime();
        }

        private void TryGiveResourceCount()
        {
            if (NextResourceCount < DateTime.UtcNow)
            {
                ResourceCount += Utility.Random(1, 10);
                NextResourceCount = DateTime.UtcNow + TimeSpan.FromDays(7);

                UpdateProperties();
            }
        }
    }

    public class GargishTotemOfEssenceDeed : BaseAddonDeed, IRewardItem, IDyable
    {
        public override int LabelNumber => 1159556; // Gargish Totem of Essence

        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem { get => m_IsRewardItem; set { m_IsRewardItem = value; InvalidateProperties(); } }

        private int m_ResourceCount;

        [Constructable]
        public GargishTotemOfEssenceDeed()
        {
            LootType = LootType.Blessed;
        }

        public GargishTotemOfEssenceDeed(Serial serial)
            : base(serial)
        {
        }        

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;
            return true;
        }

        public override BaseAddon Addon
        {
            get
            {
                GargishTotemOfEssence addon = new GargishTotemOfEssence
                {
                    IsRewardItem = m_IsRewardItem
                };

                return addon;
            }
        }        

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_IsRewardItem)
                list.Add(1076223); // 7th Year Veteran Reward

            if (m_ResourceCount > 0)
                list.Add(1159590, m_ResourceCount.ToString()); // Essences: ~1_COUNT~	
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
