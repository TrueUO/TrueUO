using Server.Multis;

namespace Server.Mobiles
{
    public class CommissionPlayerVendor : PlayerVendor
    {
        public override bool IsCommission => true;
        public override double CommissionPerc => 5.25;

        public CommissionPlayerVendor(Mobile owner, BaseHouse house)
           : base(owner, house)
        {
        }

        public CommissionPlayerVendor(Serial serial)
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
