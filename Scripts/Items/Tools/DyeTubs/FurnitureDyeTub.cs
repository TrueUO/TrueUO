using System;

namespace Server.Items
{
    public class FurnitureDyeTub : DyeTub
    {
        [Constructable]
        public FurnitureDyeTub()
        {
        }

        public FurnitureDyeTub(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowDyables => false;
        public override bool AllowFurniture => true;
        public override int TargetMessage => 501019;// Select the furniture to dye.
        public override int FailMessage => 501021;// That is not a piece of furniture.
        public override int LabelNumber => 1041246;// Furniture Dye Tub

        private static Type[] _Dyables =
        {
            typeof(PotionKeg), typeof(CustomizableSquaredDoorMatDeed), typeof(OrnateBedDeed),
            typeof(FourPostBedDeed), typeof(FormalDiningTableDeed), typeof(FluffySpongeDeed),
            typeof(ShelfSpongeDeed), typeof(CactusSpongeDeed), typeof(BarrelSpongeDeed)
        };

        public override Type[] ForcedDyables => _Dyables;

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
