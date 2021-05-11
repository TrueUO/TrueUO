namespace Server.Items
{
    public class ParasiticPotion : BasePoisonPotion
    {
        [Constructable]
        public ParasiticPotion()
            : base(PotionEffect.Parasitic)
        {
        }

        public ParasiticPotion(Serial serial)
            : base(serial)
        {
        }

        public override Poison Poison => Poison.Parasitic;
        public override double MinPoisoningSkill => 95.0;
        public override double MaxPoisoningSkill => 100.0;
        public override int LabelNumber => 1072848;// Parasitic Poison
        public override int Hue => 0x17C;

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
