namespace Server.Items
{
    [Flipable(0xA775, 0xA776)]
    public class OpalStandingHarp : Item
    {
        private int _DisplayCliloc;

        [CommandProperty(AccessLevel.GameMaster)]
        public int DisplayCliloc { get { return _DisplayCliloc; } set { _DisplayCliloc = value; InvalidateProperties(); } }

        public override int LabelNumber => 1159741; // Opal Standing Harp

        [Constructable]
        public OpalStandingHarp()
            : base(0xA775)
        {
            _DisplayCliloc = Utility.Random(1159745, 8);
        }

        public OpalStandingHarp(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 3))
            {
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else
            {
                Effects.PlaySound(from.Location, from.Map, Utility.RandomList(913, 914, 915));
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1159743, $"#{_DisplayCliloc}"); // Hand set by Jewelers at ~1_JEWELER~
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(_DisplayCliloc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _DisplayCliloc = reader.ReadInt();
        }
    }
}
