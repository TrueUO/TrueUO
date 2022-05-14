namespace Server.Items
{
    public class ClimbingVine : Item
    {
        public override int LabelNumber => 1023307;  // vines

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D ClimbLocation { get; set; }

        [Constructable]
        public ClimbingVine()
            : this(Point3D.Zero)
        {
        }

        [Constructable]
        public ClimbingVine(Point3D p)
            : base(0x1AA1)
        {
            ClimbLocation = p;
        }

        public ClimbingVine(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SayTo(from, 1156290, 1153); // *The vines looks as though they may be strong enough to support climbing...*

            if (ClimbLocation != Point3D.Zero && from.InRange(GetWorldLocation(), 2) && Z >= from.Z)
            {
                from.MoveToWorld(ClimbLocation, Map);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(ClimbLocation);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            ClimbLocation = reader.ReadPoint3D();
        }
    }
}
