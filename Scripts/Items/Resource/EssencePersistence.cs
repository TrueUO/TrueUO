namespace Server.Items
{
    public class EssencePersistence : Item, ICommodity
    {
        [Constructable]
        public EssencePersistence()
            : this(1)
        {
        }

        [Constructable]
        public EssencePersistence(int amount)
            : base(0x571C)
        {
            Stackable = true;
            Amount = amount;
        }

        public EssencePersistence(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1113343;// essence of persistence
        public override int Hue => 37;
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

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
