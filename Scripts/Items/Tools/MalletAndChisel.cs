using Server.Engines.Craft;

namespace Server.Items
{
    public class MalletAndChisel : BaseTool
    {
        [Constructable]
        public MalletAndChisel()
            : base(0x12B3)
        {
        }

        [Constructable]
        public MalletAndChisel(int uses)
            : base(uses, 0x12B3)
        {
        }

        public MalletAndChisel(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

        public override CraftSystem CraftSystem => DefMasonry.CraftSystem;

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
