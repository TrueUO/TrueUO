namespace Server.Items
{
    public class MetallicDyeTub : DyeTub
    {
        [Constructable]
        public MetallicDyeTub()
        {
        }

        public MetallicDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowDyables => false;
        public override bool AllowMetal => true;
        public override int TargetMessage => 1080393;  // Select the metal item to dye.
        public override int FailMessage => 1080394;  // You can only dye metal with this tub.
        public override int LabelNumber => 1150067;  // Metallic Dye Tub
        public override CustomHuePicker CustomHuePicker => CustomHuePicker.MetallicDyeTub;

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
