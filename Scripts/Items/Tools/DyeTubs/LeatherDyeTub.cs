using System;

namespace Server.Items
{
    public class LeatherDyeTub : DyeTub
    {
        [Constructable]
        public LeatherDyeTub()
        {
        }

        public LeatherDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowDyables => false;
        public override bool AllowLeather => true;
        public override int TargetMessage => 1042416;// Select the leather item to dye.
        public override int FailMessage => 1042418;// You can only dye leather with this tub.
        public override int LabelNumber => 1041284;// Leather Dye Tub
        public override CustomHuePicker CustomHuePicker => CustomHuePicker.LeatherDyeTub;

        private static Type[] _Dyables =
        {
            typeof(WoodlandBelt), typeof(BarbedWhip), typeof(BladedWhip), typeof(SpikedWhip)
        };

        public override Type[] ForcedDyables => _Dyables;

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
    }
}
