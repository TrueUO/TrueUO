using System;

namespace Server.Items
{
    public class StatuetteDyeTub : DyeTub
    {
        [Constructable]
        public StatuetteDyeTub()
        {
        }

        public StatuetteDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowDyables => false;
        public override bool AllowStatuettes => true;
        public override int TargetMessage => 1049777;// Target the statuette to dye
        public override int FailMessage => 1049778;// You can only dye veteran reward statuettes with this tub.
        public override int LabelNumber => 1049741;// Reward Statuette Dye Tub
        public override CustomHuePicker CustomHuePicker => CustomHuePicker.LeatherDyeTub;

        private static Type[] _Dyables =
        {
            typeof(MongbatDartboard), typeof(FelineBlessedStatue)
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
