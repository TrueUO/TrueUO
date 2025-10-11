using Server.Engines.Craft;

namespace Server.Items
{
    public class SewingKit : BaseTool
    {
        [Constructable]
        public SewingKit()
            : base(0xF9D)
        {
        }

        [Constructable]
        public SewingKit(int uses)
            : base(uses, 0xF9D)
        {
        }

        public SewingKit(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

        public override CraftSystem CraftSystem => DefTailoring.CraftSystem;

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
