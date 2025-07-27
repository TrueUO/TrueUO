using Server.Engines.Craft;

namespace Server.Items
{
    [Flipable(0xfbb, 0xfbc)]
    public class Tongs : BaseTool
    {
        [Constructable]
        public Tongs()
            : base(0xFBB)
        {
        }

        [Constructable]
        public Tongs(int uses)
            : base(uses, 0xFBB)
        {
        }

        public Tongs(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

        public override CraftSystem CraftSystem => DefBlacksmithy.CraftSystem;

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
