namespace Server.Items
{
    [Flipable(0xA773, 0xA774)]
    public class OpalEncrustedMobius : Item
    {
        private int _DisplayCliloc;

        [CommandProperty(AccessLevel.GameMaster)]
        public int DisplayCliloc { get { return _DisplayCliloc; } set { _DisplayCliloc = value; InvalidateProperties(); } }

        public override int LabelNumber => 1159740; // Opal Encrusted Mobius

        [Constructable]
        public OpalEncrustedMobius()
            : base(0xA773)
        {
            _DisplayCliloc = Utility.Random(1159745, 8);
        }

        public OpalEncrustedMobius(Serial serial)
            : base(serial)
        {
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
