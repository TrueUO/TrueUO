namespace Server.Items
{
    public class AllegiancePouch : Backpack
    {
        public override int LabelNumber => 1113953;  //Allegiance Pouch

        [Constructable]
        public AllegiancePouch()
        {
            Hue = 2958;
            Weight = 1.0;
            LootType = LootType.Regular;

            DropItem(new OrderBanner());
            DropItem(new ChaosBanner());
        }

        public AllegiancePouch(Serial serial)
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
