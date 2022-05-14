using Server.Mobiles;

namespace Server.Items
{
    public class CitadelTele : Item
    {
        [Constructable]
        public CitadelTele()
            : base(0xE3F)
        {
            Movable = false;
        }

        public CitadelTele(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from is PlayerMobile player)
            {
                BaseCreature.TeleportPets(player, new Point3D(107, 1883, 0), Map.Malas);
                player.MoveToWorld(new Point3D(107, 1883, 0), Map.Malas);
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
