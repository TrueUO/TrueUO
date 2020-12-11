namespace Server.Items
{
    [Flipable(0xA5F6, 0xA5F7)]
    public class StainedGlassWindow1 : Item
    {
        public override int LabelNumber => 1126510; // stained glass window

        [Constructable]
        public StainedGlassWindow1()
            : base(0xA5F6)
        {
        }

        public StainedGlassWindow1(Serial serial)
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
	
	[Flipable(0xA5F8, 0xA5F9)]
    public class StainedGlassWindow2 : Item
    {
        public override int LabelNumber => 1126510; // stained glass window

        [Constructable]
        public StainedGlassWindow2()
            : base(0xA5F8)
        {
        }

        public StainedGlassWindow2(Serial serial)
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
	
	[Flipable(0xA5FA, 0xA5FB)]
    public class StainedGlassWindow3 : Item
    {
        public override int LabelNumber => 1126510; // stained glass window

        [Constructable]
        public StainedGlassWindow3()
            : base(0xA5FA)
        {
        }

        public StainedGlassWindow3(Serial serial)
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
	
	[Flipable(0xA5FC, 0xA5FD)]
    public class StainedGlassWindow4 : Item
    {
        public override int LabelNumber => 1126510; // stained glass window

        [Constructable]
        public StainedGlassWindow4()
            : base(0xA5FC)
        {
        }

        public StainedGlassWindow4(Serial serial)
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
