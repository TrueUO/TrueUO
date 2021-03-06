namespace Server.Items
{
    public class BlackGateStatuette : BaseStatuette
    {
        public override int LabelNumber => 1159552; // Black Gate Statuette

        private static readonly int[] m_Sounds = new int[]
        {
            526, 1664
        };

        [Constructable]
        public BlackGateStatuette()
            : base(0xA723)
        {
            Weight = 1.0;
        }

        public BlackGateStatuette(Serial serial)
            : base(serial)
        {
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (TurnedOn && IsLockedDown && (!m.Hidden || m.IsPlayer()) && Utility.InRange(m.Location, Location, 2) && !Utility.InRange(oldLocation, Location, 2))
                Effects.PlaySound(Location, Map, m_Sounds[Utility.Random(m_Sounds.Length)]);

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
