using Server.Engines.Craft;

namespace Server.Items
{
    public class PirateShield : BaseShield, IRepairable
    {
        public override int LabelNumber => 1126593; // shield
        public CraftSystem RepairSystem => DefCarpentry.CraftSystem;

        [Constructable]
        public PirateShield()
            : base(0xA649)
        {
            Weight = 8.0;
        }

        public PirateShield(Serial serial)
            : base(serial)
        {
        }

        public override int BasePhysicalResistance => 0;
        public override int BaseFireResistance => 0;
        public override int BaseColdResistance => 0;
        public override int BasePoisonResistance => 1;
        public override int BaseEnergyResistance => 0;
        public override int InitMinHits => 50;
        public override int InitMaxHits => 65;
        public override int StrReq => 20;

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }
    }
}
