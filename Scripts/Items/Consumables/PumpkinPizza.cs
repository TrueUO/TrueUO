namespace Server.Items
{
    public class PumpkinPizza : CheesePizza
    {
        public override int LabelNumber => 1153775;  // Pumpkin Pizza
        public override int Hue => 0xF3;

        [Constructable]
        public PumpkinPizza()
        {
        }

        public PumpkinPizza(Serial serial)
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
