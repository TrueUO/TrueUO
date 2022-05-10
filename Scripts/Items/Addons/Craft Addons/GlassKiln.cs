using Server.Engines.Craft;
using Server.Gumps;

namespace Server.Items
{
    public class GlassKilnAddon : CraftAddon
    {
        public override CraftSystem CraftSystem => DefGlassblowing.CraftSystem;

        public override BaseAddonDeed Deed
        {
            get
            {
                GlassKilnDeed deed = new GlassKilnDeed(Tools.Count > 0 ? Tools[0].UsesRemaining : 0);

                return deed;
            }
        }

        [Constructable]
        public GlassKilnAddon(DirectionType type, int uses)
        {
            switch (type)
            {
                case DirectionType.South:
                    AddCraftComponent(new AddonToolComponent(CraftSystem, 0xA530, 0xA531, 1157072, 1157073, 1126312, uses, this), 0, 0, 0);
                    AddComponent(new ToolDropComponent(0xA52E, 1015081), 0, 1, 0);
                    break;
                case DirectionType.East:
                    AddCraftComponent(new AddonToolComponent(CraftSystem, 0xA534, 0xA535, 1157072, 1157073, 1126312, uses, this), 0, 0, 0);
                    AddComponent(new ToolDropComponent(0xA52F, 1015081), 1, 0, 0);
                    break;
            }
        }

        public GlassKilnAddon(Serial serial)
            : base(serial)
        {
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

    public class GlassKilnDeed : CraftAddonDeed, IRewardOption
    {
        public override int LabelNumber => 1159420;  // Glass Kiln

        public override BaseAddon Addon
        {
            get
            {
                GlassKilnAddon addon = new GlassKilnAddon(_Direction, UsesRemaining);

                return addon;
            }
        }

        private DirectionType _Direction;

        [Constructable]
        public GlassKilnDeed()
            : this(0)
        {
        }

        [Constructable]
        public GlassKilnDeed(int uses)
            : base(uses)
        {
        }

        public GlassKilnDeed(Serial serial)
            : base(serial)
        {
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add((int)DirectionType.South, 1075386); // South
            list.Add((int)DirectionType.East, 1075387); // East
        }

        public void OnOptionSelected(Mobile from, int choice)
        {
            _Direction = (DirectionType)choice;

            if (!Deleted)
                base.OnDoubleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(AddonOptionGump));
                from.SendGump(new AddonOptionGump(this, 1154194)); // Choose a Facing:
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
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
