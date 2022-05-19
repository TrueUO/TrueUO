namespace Server.Items
{
    public class SpecialDyeTub : DyeTub
    {
        [Constructable]
        public SpecialDyeTub()
        {
        }

        public SpecialDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override CustomHuePicker CustomHuePicker => CustomHuePicker.SpecialDyeTub;

        public override int LabelNumber => 1041285;// Special Dye Tub

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
