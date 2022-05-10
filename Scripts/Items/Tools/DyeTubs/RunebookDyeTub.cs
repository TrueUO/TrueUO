namespace Server.Items
{
    public class RunebookDyeTub : DyeTub
    {
        [Constructable]
        public RunebookDyeTub()
        {
        }

        public RunebookDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowDyables => false;
        public override bool AllowRunebooks => true;
        public override int TargetMessage => 1049774;// Target the runebook or runestone to dye
        public override int FailMessage => 1049775;// You can only dye runestones or runebooks with this tub.
        public override int LabelNumber => 1049740;// Runebook Dye Tub
        public override CustomHuePicker CustomHuePicker => CustomHuePicker.LeatherDyeTub;

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
