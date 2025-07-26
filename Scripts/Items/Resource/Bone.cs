namespace Server.Items
{
    public class Bone : Item, ICommodity
    {
        [Constructable]
        public Bone()
            : this(1)
        {
        }

        [Constructable]
        public Bone(int amount)
            : base(0xf7e)
        {
            Stackable = true;
            Amount = amount;
        }

        public Bone(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

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
