using Server.Network;
using System;

namespace Server.Items
{
    [Flipable(0x2A65, 0x2A67)]
    public class UnsettlingPortraitComponent : AddonComponent
    {
        public override int LabelNumber => 1074480;// Unsettling portrait

        private Timer _Timer;

        public UnsettlingPortraitComponent()
            : base(0x2A65)
        {
            _Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), Change);
        }

        public UnsettlingPortraitComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Utility.InRange(Location, from.Location, 2))
            {
                Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x567, 0x568));
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (_Timer != null && _Timer.Running)
            {
                _Timer.Stop();
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

            _Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), Change);
        }

        private void Change()
        {
            if (ItemID == 0x2A65)
            {
                ItemID += 1;
            }
            else if (ItemID == 0x2A66)
            {
                ItemID -= 1;
            }
            else if (ItemID == 0x2A67)
            {
                ItemID += 1;
            }
            else if (ItemID == 0x2A68)
            {
                ItemID -= 1;
            }
        }
    }

    public class UnsettlingPortraitAddon : BaseAddon
    {
        [Constructable]
        public UnsettlingPortraitAddon()
        {
            AddComponent(new UnsettlingPortraitComponent(), 0, 0, 0);
        }

        public UnsettlingPortraitAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed => new UnsettlingPortraitDeed();

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

    public class UnsettlingPortraitDeed : BaseAddonDeed
    {
        public override int LabelNumber => 1074480;// Unsettling portrait

        [Constructable]
        public UnsettlingPortraitDeed()
        {
            LootType = LootType.Blessed;
        }

        public UnsettlingPortraitDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon => new UnsettlingPortraitAddon();
        
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
}
