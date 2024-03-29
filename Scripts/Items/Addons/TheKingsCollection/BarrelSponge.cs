using Server.Multis;
using System;

namespace Server.Items
{
    public class BarrelSpongeAddon : BaseAddon
    {
        public override bool ForceShowProperties => true;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextResourceCount { get; set; }

        private int m_ResourceCount;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResourceCount { get => m_ResourceCount; set { m_ResourceCount = value; UpdateProperties(); } }

        [Constructable]
        public BarrelSpongeAddon()
           : this(0, DateTime.UtcNow + TimeSpan.FromDays(7))
        {
        }

        [Constructable]
        public BarrelSpongeAddon(int resCount, DateTime nextuse)
        {
            NextResourceCount = nextuse;
            ResourceCount = resCount;

            AddComponent(new BarrelSpongeComponent(0x4C30), 0, 0, 0);
        }

        public BarrelSpongeAddon(Serial serial)
            : base(serial)
        {
        }

        private readonly Type[] m_Potions =
        {
            typeof(ShatterPotion),
            typeof(FearEssence),
            typeof(InvisibilityPotion),
            typeof(DarkglowPotion),
            typeof(GreaterConflagrationPotion),
            typeof(ParasiticPotion),
            typeof(ExplodingTarPotion),
            typeof(GreaterConfusionBlastPotion)
        };

        public override void OnComponentUsed(AddonComponent component, Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(from);

            if (house != null && (house.IsOwner(from) || house.LockDowns.ContainsKey(this) && house.LockDowns[this] == from))
            {
                if (ResourceCount > 0)
                {
                    Item item = Loot.Construct(m_Potions);

                    if (item == null)
                    {
                        return;
                    }

                    ResourceCount--;

                    from.AddToBackpack(item);
                    from.SendLocalizedMessage(1154176); // Potions have been placed in your backpack. 
                }
            }
            else
            {
                from.SendLocalizedMessage(502092); // You must be in your house to do this.
            }
        }

        public override BaseAddonDeed Deed => new BarrelSpongeDeed(m_ResourceCount);

        public override void GetProperties(ObjectPropertyList list, AddonComponent c)
        {
            list.Add(1154178, ResourceCount.ToString()); // Potions: ~1_COUNT~
        }

        private class BarrelSpongeComponent : LocalizedAddonComponent
        {
            public BarrelSpongeComponent(int id)
                : base(id, 1098376) // Barrel Sponge
            {
            }

            public BarrelSpongeComponent(Serial serial)
                : base(serial)
            {
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

        private void TryGiveResourceCount()
        {
            if (NextResourceCount < DateTime.UtcNow)
            {
                ResourceCount = Math.Min(140, m_ResourceCount + 35);
                NextResourceCount = DateTime.UtcNow + TimeSpan.FromDays(7);

                UpdateProperties();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            TryGiveResourceCount();

            writer.Write(m_ResourceCount);
            writer.Write(NextResourceCount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_ResourceCount = reader.ReadInt();
            NextResourceCount = reader.ReadDateTime();
        }
    }

    [Furniture]
    public class BarrelSpongeDeed : BaseAddonDeed
    {
        private int m_ResourceCount;

        [CommandProperty(AccessLevel.GameMaster)]
        public int ResourceCount
        {
            get => m_ResourceCount;
            set
            {
                m_ResourceCount = value;
                InvalidateProperties();
            }
        }

        public override int LabelNumber => 1098376;  // Barrel Sponge
        public override bool IsArtifact => true; // allows dying of the deed.

        [Constructable]
        public BarrelSpongeDeed()
            : this(0)
        {
        }

        [Constructable]
        public BarrelSpongeDeed(int resCount)
        {
            LootType = LootType.Blessed;

            ResourceCount = resCount;
        }

        public BarrelSpongeDeed(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1154178, ResourceCount.ToString()); // Potions: ~1_COUNT~
        }

        public override BaseAddon Addon =>
            new BarrelSpongeAddon(m_ResourceCount, DateTime.UtcNow + TimeSpan.FromDays(7));

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write(m_ResourceCount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_ResourceCount = reader.ReadInt();
                    break;
                case 0:
                    break;
            }
        }
    }
}
