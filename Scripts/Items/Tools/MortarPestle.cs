using Server.Engines.Craft;

namespace Server.Items
{
    public class MortarPestle : BaseTool
    {
        [Constructable]
        public MortarPestle()
            : base(0xE9B)
        {
        }

        [Constructable]
        public MortarPestle(int uses)
            : base(uses, 0xE9B)
        {
        }

        public MortarPestle(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

        public override CraftSystem CraftSystem => DefAlchemy.CraftSystem;

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
