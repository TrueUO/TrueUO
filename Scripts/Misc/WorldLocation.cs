namespace Server
{
    public class WorldLocation
    {
        public Point3D Location { get; }
        public Map Map { get; }

        public WorldLocation(int x, int y, int z, Map map)
            : this(new Point3D(x, y, z), map)
        {
        }

        public WorldLocation(Point3D p, Map map)
        {
            Location = p;
            Map = map;
        }

        public WorldLocation(IEntity e)
        {
            Location = e.Location;
            Map = e.Map;
        }

        public override string ToString()
        {
            return $"({Location.X}, {Location.Y}, {Location.Z}) [{(Map == null ? "(Null)" : Map.ToString())}]";
        }
    }
}
