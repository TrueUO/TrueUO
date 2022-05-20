namespace Server.Items
{
    public class OrigamiPaper : Item
    {
        [Constructable]
        public OrigamiPaper()
            : base(0x2830)
        {
        }

        public OrigamiPaper(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030288;// origami paper

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                Delete();

                Item i = null;

                switch (Utility.Random((from.BAC >= 7) ? 8 : 7)) //switch ( Utility.Random( (from.BAC >= 5) ? 6 : 5) )
                {
                    case 0: i = new OrigamiButterfly(); break;
                    case 1: i = new OrigamiSwan(); break;
                    case 2: i = new OrigamiFrog(); break;
                    case 3: i = new OrigamiShape(); break;
                    case 4: i = new OrigamiSongbird(); break;
                    case 5: i = new OrigamiFish(); break;
                    case 6: i = new OrigamiDragon(); break;
                    case 7: i = new OrigamiBunny(); break;
                }

                if (i != null)
                    from.AddToBackpack(i);

                from.SendLocalizedMessage(1070822); // You fold the paper into an interesting shape.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiButterfly : Item
    {
        [Constructable]
        public OrigamiButterfly()
            : base(0x2838)
        {
        }

        public OrigamiButterfly(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030296;// a delicate origami butterfly

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiSwan : Item
    {
        [Constructable]
        public OrigamiSwan()
            : base(0x2839)
        {
        }

        public OrigamiSwan(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030297;// a delicate origami swan

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiFrog : Item
    {
        [Constructable]
        public OrigamiFrog()
            : base(0x283A)
        {
        }

        public OrigamiFrog(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030298;// a delicate origami frog

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiShape : Item
    {
        [Constructable]
        public OrigamiShape()
            : base(0x283B)
        {
        }

        public OrigamiShape(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030299;// an intricate geometric origami shape

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiSongbird : Item
    {
        [Constructable]
        public OrigamiSongbird()
            : base(0x283C)
        {
        }

        public OrigamiSongbird(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030300;// a delicate origami songbird

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiFish : Item
    {
        [Constructable]
        public OrigamiFish()
            : base(0x283D)
        {
        }

        public OrigamiFish(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1030301;// a delicate origami fish

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiDragon : Item
    {
        [Constructable]
        public OrigamiDragon()
            : base(0x4B1C)
        {

            Name = "a delicate origami dragon";
            Weight = 1.0;
        }

        public OrigamiDragon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class OrigamiBunny : Item
    {
        [Constructable]
        public OrigamiBunny()
            : base(0x4B1F)
        {
            Name = "a delicate origami bunny";
            Weight = 1.0;
        }

        public OrigamiBunny(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
