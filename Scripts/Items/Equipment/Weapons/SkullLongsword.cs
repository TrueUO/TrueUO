namespace Server.Items
{
    [Flipable(41793, 41794)]
    public class SkullLongsword : Longsword
    {
        public override int LabelNumber => 1125817;  // skull longsword

        [Constructable]
        public SkullLongsword()
        {
            ItemID = 41793;
        }

        public SkullLongsword(Serial serial)
            : base(serial)
        {
        }

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
