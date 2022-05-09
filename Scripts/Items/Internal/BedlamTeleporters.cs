using Server.Mobiles;

namespace Server.Items
{
    public class BedlamTele : Item
    {
        [Constructable]
        public BedlamTele()
            : base(0x124D)
        {
            Movable = false;
        }

        public BedlamTele(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1074161;// Access to Bedlam by invitation only

        public override bool ForceShowProperties => true;

        public override void OnDoubleClick(Mobile from)
        {
            if (from is PlayerMobile player)
            {
                BaseCreature.TeleportPets(player, new Point3D(121, 1682, 0), Map);
                player.MoveToWorld(new Point3D(121, 1682, 0), Map);
            }
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
