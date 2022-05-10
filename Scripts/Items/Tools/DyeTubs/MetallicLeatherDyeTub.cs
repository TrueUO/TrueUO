namespace Server.Items
{
    public class MetallicLeatherDyeTub : DyeTub
    {
        [Constructable]
        public MetallicLeatherDyeTub()
        {
        }

        public MetallicLeatherDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowDyables => false;
        public override bool AllowLeather => true;
        public override int TargetMessage => 1042416;  // Select the leather item to dye.
        public override int FailMessage => 1042418;  // You can only dye leather with this tub.
        public override int LabelNumber => 1153495;  // Metallic Leather Dye Tub
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
