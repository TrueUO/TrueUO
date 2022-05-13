namespace Server.Items
{
    public class StasisChamberPowerCore : Item
    {
        public override int LabelNumber => 1156623;

        [Constructable]
        public StasisChamberPowerCore()
            : base(40155)
        {
        }

        public StasisChamberPowerCore(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
