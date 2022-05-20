namespace Server.Items.Holiday
{
    public class PaintedPorcelainMask : BasePaintedMask
    {
        [Constructable]
        public PaintedPorcelainMask()
            : base(0x4BA7)
        {
        }

        public PaintedPorcelainMask(Serial serial)
            : base(serial)
        {
        }

        public override string MaskName => "Porcelain Mask";

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
