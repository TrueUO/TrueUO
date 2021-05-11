namespace Server.Items
{
    public class YewTreeStatuette : BaseStatuette
    {
        public override int LabelNumber => 1159554; // Yew Tree Statuette

        [Constructable]
        public YewTreeStatuette()
            : base(0xA724)
        {
            Weight = 1.0;
        }

        public YewTreeStatuette(Serial serial)
            : base(serial)
        {
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
                Effects.PlaySound(Location, Map, Utility.Random(10));

            base.OnMovement(m, oldLocation);
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
