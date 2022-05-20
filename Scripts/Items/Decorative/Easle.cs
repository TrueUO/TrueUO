namespace Server.Items
{
    [Furniture]
    public class EasleSouth : Item
    {
        [Constructable]
        public EasleSouth()
            : base(0xF66)
        {
            Weight = 25.0;
        }

        public EasleSouth(Serial serial)
            : base(serial)
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

    [Furniture]
    public class EasleEast : Item
    {
        [Constructable]
        public EasleEast()
            : base(0xF68)
        {
            Weight = 25.0;
        }

        public EasleEast(Serial serial)
            : base(serial)
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

    [Furniture]
    public class EasleNorth : Item
    {
        [Constructable]
        public EasleNorth()
            : base(0xF6A)
        {
            Weight = 25.0;
        }

        public EasleNorth(Serial serial)
            : base(serial)
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
