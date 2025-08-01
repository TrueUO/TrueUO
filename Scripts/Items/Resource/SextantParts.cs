namespace Server.Items
{
    [Flipable(0x1059, 0x105A)]
    public class SextantParts : Item
    {
        [Constructable]
        public SextantParts()
            : this(1)
        {
        }

        [Constructable]
        public SextantParts(int amount)
            : base(0x1059)
        {
            Stackable = true;
            Amount = amount;
        }

        public SextantParts(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

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
