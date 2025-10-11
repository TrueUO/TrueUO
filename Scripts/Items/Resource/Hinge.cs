namespace Server.Items
{
    [Flipable(0x1055, 0x1056)]
    public class Hinge : Item
    {
        [Constructable]
        public Hinge()
            : this(1)
        {
        }

        [Constructable]
        public Hinge(int amount)
            : base(0x1055)
        {
            Stackable = true;
            Amount = amount;
        }

        public Hinge(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

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
