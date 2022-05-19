using Server.Engines.VeteranRewards;

namespace Server.Items
{
    public class ContestMiniHouse : MiniHouseAddon
    {
        private bool m_IsRewardItem;

        [Constructable]
        public ContestMiniHouse()
            : base(MiniHouseType.MalasMountainPass)
        {
        }

        [Constructable]
        public ContestMiniHouse(MiniHouseType type)
            : base(type)
        {
        }

        public ContestMiniHouse(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                ContestMiniHouseDeed deed = new ContestMiniHouseDeed(Type)
                {
                    IsRewardItem = m_IsRewardItem
                };

                return deed;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get => m_IsRewardItem;
            set
            {
                m_IsRewardItem = value;
                InvalidateProperties();
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version

            writer.Write(m_IsRewardItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();

            m_IsRewardItem = reader.ReadBool();
        }
    }

    public class ContestMiniHouseDeed : MiniHouseDeed, IRewardItem
    {
        private bool m_IsRewardItem;

        [Constructable]
        public ContestMiniHouseDeed()
            : base(MiniHouseType.MalasMountainPass)
        {
        }

        [Constructable]
        public ContestMiniHouseDeed(MiniHouseType type)
            : base(type)
        {
        }

        public ContestMiniHouseDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                ContestMiniHouse addon = new ContestMiniHouse(Type)
                {
                    IsRewardItem = m_IsRewardItem
                };

                return addon;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get => m_IsRewardItem;
            set
            {
                m_IsRewardItem = value;
                InvalidateProperties();
            }
        }
        
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version

            writer.Write(m_IsRewardItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();

            m_IsRewardItem = reader.ReadBool();
        }
    }
}
