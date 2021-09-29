using Server.Engines.Craft;

namespace Server.Items
{
    public class LanternShield : BaseShield, IRepairable
    {
        public override int LabelNumber => 1124445; // lantern
        public CraftSystem RepairSystem => DefTinkering.CraftSystem;

        [Constructable]
        public LanternShield()
            : base(0xA76A)
        {
            Weight = 1.0;
        }

        public LanternShield(Serial serial)
            : base(serial)
        {
        }

        public override int BasePhysicalResistance => 0;
        public override int BaseFireResistance => 0;
        public override int BaseColdResistance => 0;
        public override int BasePoisonResistance => 0;
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
