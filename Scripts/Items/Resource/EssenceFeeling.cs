namespace Server.Items
{
    public class EssenceFeeling : Item, ICommodity
    {
        [Constructable]
        public EssenceFeeling()
            : this(1)
        {
        }

        [Constructable]
        public EssenceFeeling(int amount)
            : base(0x571C)
        {
            Stackable = true;
            Amount = amount;
        }

        public EssenceFeeling(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1113339;// essence of feeling
        public override int Hue => 455;
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
