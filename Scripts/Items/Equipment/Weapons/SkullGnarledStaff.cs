namespace Server.Items
{
    [Flipable(41795, 41796)]
    public class SkullGnarledStaff : GnarledStaff
    {
        public override int LabelNumber => 1125819;  // skull gnarled staff

        [Constructable]
        public SkullGnarledStaff()
        {
            ItemID = 41795;
        }

        public SkullGnarledStaff(Serial serial)
            : base(serial)
        {
        }

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
