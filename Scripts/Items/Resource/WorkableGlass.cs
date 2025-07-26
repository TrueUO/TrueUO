namespace Server.Items
{
    public class WorkableGlass : Item, ICommodity
    {
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override int LabelNumber => 1154170;  // workable glass

        [Constructable]
        public WorkableGlass() : this(1)
        {
        }

        [Constructable]
        public WorkableGlass(int amount) : base(19328)
        {
            Stackable = true;
            Amount = amount;
        }

        public WorkableGlass(Serial serial)
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
