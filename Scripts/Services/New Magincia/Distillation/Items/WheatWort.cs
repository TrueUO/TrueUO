namespace Server.Items
{
    public class WheatWort : Item
    {
        public override int LabelNumber => 1150275;  // wheat wort

        [Constructable]
        public WheatWort() : this(1)
        {
        }

        [Constructable]
        public WheatWort(int amount)
            : base(0x1848)
        {
            Stackable = true;
            Amount = amount;
            Hue = 1281;
        }

        public WheatWort(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
