using Server.Network;
using System;

namespace Server.Items
{
    [Flipable(0x2A5D, 0x2A61)]
    public class DisturbingPortraitComponent : AddonComponent
    {
        public override int LabelNumber => 1074479;// Disturbing portrait

        private Timer _Timer;

        public DisturbingPortraitComponent()
            : base(0x2A5D)
        {
            _Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), Change);
        }

        public DisturbingPortraitComponent(Serial serial)
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
            if (ItemID < 0x2A61)
            {
                ItemID = Utility.RandomMinMax(0x2A5D, 0x2A60);
            }
            else
            {
                ItemID = Utility.RandomMinMax(0x2A61, 0x2A64);
            }
        }
    }

    public class DisturbingPortraitAddon : BaseAddon
    {
        [Constructable]
        public DisturbingPortraitAddon()
        {
            AddComponent(new DisturbingPortraitComponent(), 0, 0, 0);
        }

        public DisturbingPortraitAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed => new DisturbingPortraitDeed();

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

    public class DisturbingPortraitDeed : BaseAddonDeed
    {
        public override int LabelNumber => 1074479;// Disturbing portrait

        [Constructable]
        public DisturbingPortraitDeed()
        {
            LootType = LootType.Blessed;
        }

        public DisturbingPortraitDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon => new DisturbingPortraitAddon();
       
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
