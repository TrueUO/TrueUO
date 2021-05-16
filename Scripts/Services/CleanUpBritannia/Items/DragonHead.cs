using Server.Gumps;

namespace Server.Items
{
    public enum AddonFacing
    {
        South = 0,
        East = 1
    }

    public class DragonHeadAddon : BaseAddon
    {
        public override BaseAddonDeed Deed => new DragonHeadAddonDeed();

        public AddonFacing Facing { get; set; }

        [Constructable]
        public DragonHeadAddon()
            : this(AddonFacing.South)
        {
        }

        [Constructable]
        public DragonHeadAddon(AddonFacing facing)
        {
            Facing = facing;

            switch (facing)
            {
                case AddonFacing.South:
                    AddComponent(new DragonHeadComponent(0x2234), 0, 0, 10);
                    break;
                case AddonFacing.East:
                    AddComponent(new DragonHeadComponent(0x2235), 0, 0, 10);
                    break;

            }
        }

        private class DragonHeadComponent : AddonComponent
        {
            public override bool NeedsWall => true;
            public override Point3D WallPosition
            {
                get
                {
                    switch (ItemID)
                    {
                        default:
                        case 0x2234: return new Point3D(0, -1, 0);
                        case 0x2235: return new Point3D(-1, 0, 0);
                    }
                }
            }

            public DragonHeadComponent(int id)
                : base(id)
            {
            }

            public DragonHeadComponent(Serial serial)
                : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.WriteEncodedInt(0); // version
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadEncodedInt();
            }
        }

        public DragonHeadAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class DragonHeadAddonDeed : BaseAddonDeed, IRewardOption
    {
        public override BaseAddon Addon => new DragonHeadAddon(Facing);

        private AddonFacing Facing { get; set; }

        public override int LabelNumber => 1080209;  // Dragon Head

        [Constructable]
        public DragonHeadAddonDeed()
        {
            LootType = LootType.Blessed;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(RewardOptionGump));
                from.SendGump(new RewardOptionGump(this));
            }
            else
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.       	
        }

        public DragonHeadAddonDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)AddonFacing.South, 1080208);
            list.Add((int)AddonFacing.East, 1080207);
        }


        public void OnOptionSelected(Mobile from, int choice)
        {
            Facing = (AddonFacing)choice;

            if (!Deleted)
                base.OnDoubleClick(from);
        }
    }
}
