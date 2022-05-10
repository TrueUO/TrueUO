namespace Server.Items
{
    public class RoseInAVase : Item 
    {
        [Constructable]
        public RoseInAVase()
            : base(0x0EB0)
        {
            Hue = 0x20;
        }

        public RoseInAVase(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1023760; // A Rose in a Vase	

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
