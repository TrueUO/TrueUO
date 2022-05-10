namespace Server.Items
{
    public class RewardBlackDyeTub : DyeTub
    {
        [Constructable]
        public RewardBlackDyeTub()
        {
            Hue = DyedHue = 0x0001;
            Redyable = false;
        }

        public RewardBlackDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1006008;// Black Dye Tub

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
