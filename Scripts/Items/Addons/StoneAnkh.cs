using Server.Gumps;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class StoneAnkhComponent : AddonComponent
    {
        public StoneAnkhComponent(int itemID)
            : base(itemID)
        {
            Weight = 1.0;
        }

        public StoneAnkhComponent(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties => true;

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

    public class StoneAnkh : BaseAddon
    {
        [Constructable]
        public StoneAnkh()
            : this(true)
        {
        }

        [Constructable]
        public StoneAnkh(bool east)
        {
            if (east)
            {
                AddComponent(new StoneAnkhComponent(0x2), 0, 0, 0);
                AddComponent(new StoneAnkhComponent(0x3), 0, -1, 0);
            }
            else
            {
                AddComponent(new StoneAnkhComponent(0x5), 0, 0, 0);
                AddComponent(new StoneAnkhComponent(0x4), -1, 0, 0);
            }
        }

        public StoneAnkh(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                StoneAnkhDeed deed = new StoneAnkhDeed();

                return deed;
            }
        }

        public override void OnChop(Mobile from)
        {
            from.SendLocalizedMessage(500489); // You can't use an axe on that.
        }

        public override void OnComponentUsed(AddonComponent c, Mobile from)
        {
            if (from.InRange(Location, 2))
            {
                BaseHouse house = BaseHouse.FindHouseAt(this);
                BaseAddon addon = c.Addon;

                if (house != null && (house.IsOwner(from) || addon != null && house.Addons.ContainsKey(addon) && house.Addons[addon] == from))
                {
                    from.CloseGump(typeof(RewardDemolitionGump));
                    from.SendGump(new RewardDemolitionGump(this, 1049783)); // Do you wish to re-deed this decoration?
                }
                else
                    from.SendLocalizedMessage(1049784); // You can only re-deed this decoration if you are the house owner or originally placed the decoration.
            }
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
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

    public class StoneAnkhDeed : BaseAddonDeed
    {
        private bool m_East;
        
        [Constructable]
        public StoneAnkhDeed()
        {
        }

        public StoneAnkhDeed(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1049773;// deed for a stone ankh

        public override BaseAddon Addon
        {
            get
            {
                StoneAnkh addon = new StoneAnkh(m_East);

                return addon;
            }
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(InternalGump));
                from.SendGump(new InternalGump(this));
            }
            else
            {
                from.SendLocalizedMessage(1042038); // You must have the object in your backpack to use it.    
            }
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

        private void SendTarget(Mobile m)
        {
            base.OnDoubleClick(m);
        }

        private class InternalGump : Gump
        {
            private readonly StoneAnkhDeed m_Deed;
            public InternalGump(StoneAnkhDeed deed)
                : base(150, 50)
            {
                m_Deed = deed;

                Closable = true;
                Disposable = true;
                Dragable = true;
                Resizable = false;

                AddPage(0);

                AddBackground(0, 0, 300, 150, 0xA28);

                AddItem(90, 30, 0x4);
                AddItem(112, 30, 0x5);
                AddButton(50, 35, 0x867, 0x869, (int)Buttons.South, GumpButtonType.Reply, 0); // South

                AddItem(170, 30, 0x2);
                AddItem(192, 30, 0x3);
                AddButton(145, 35, 0x867, 0x869, (int)Buttons.East, GumpButtonType.Reply, 0); // East
            }

            private enum Buttons
            {
                Cancel,
                South,
                East
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (m_Deed == null || m_Deed.Deleted)
                    return;

                if (info.ButtonID != (int)Buttons.Cancel)
                {
                    m_Deed.m_East = info.ButtonID == (int)Buttons.East;
                    m_Deed.SendTarget(sender.Mobile);
                }
            }
        }
    }
}
