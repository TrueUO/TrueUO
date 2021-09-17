using Server.Engines.Craft;

namespace Server.Items
{
    public class ShieldOrb : BaseShield, IRepairable
    {
        public override int LabelNumber => 1126890;  // shield orb
        public CraftSystem RepairSystem => DefBlacksmithy.CraftSystem;

        public override int BasePhysicalResistance => 0;
        public override int BaseFireResistance => 0;
        public override int BaseColdResistance => 0;
        public override int BasePoisonResistance => 1;
        public override int BaseEnergyResistance => 0;
		
        public override int InitMinHits => 45;
        public override int InitMaxHits => 60;
		
        public override int StrReq => 20;

        [Constructable]
        public ShieldOrb()
            : base(0xA772)
        {
            Weight = 1.0;
        }

        public ShieldOrb(Serial serial)
            : base(serial)
        {
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }
    }
}
