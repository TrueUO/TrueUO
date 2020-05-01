using Server.Mobiles;
using Server.Items;

using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Regions
{
    public class TeleportRegion : BaseRegion
    {
        public static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(400);

        public Dictionary<WorldLocation, WorldLocation> TeleLocs { get; set; }

        public TeleportRegion(string name, Map map, Rectangle3D[] recs, Dictionary<WorldLocation, WorldLocation> points)
            : base(name, map, GetParent(recs, map), recs)
        {
            TeleLocs = points;

            if (points != null)
            {
                GoLocation = points.Keys.FirstOrDefault(l => l.Location != Point3D.Zero).Location;
            }
        }

        public TeleportRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        private static Region GetParent(Rectangle3D[] recs, Map map)
        {
            return Region.Find(new Point3D(recs[0].Start.X, recs[0].Start.Y, recs[0].Start.Z), map);
        }

        public override void OnEnter(Mobile m)
        {
            if (m is PlayerMobile && m.CanBeginAction(typeof(Teleporter)))
            {
                WorldLocation loc = TeleLocs.Keys.FirstOrDefault(l => l.Location.X == m.X && l.Location.Y == m.Y && l.Location.Z >= m.Z - 5 && l.Location.Z <= m.Z + 5 && l.Map == m.Map);

                if (loc != null)
                {
                    var destinationPoint = TeleLocs[loc].Location;
                    var destinationMap = TeleLocs[loc].Map;

                    if (destinationPoint != Point3D.Zero && destinationMap != null && destinationMap != Map.Internal)
                    {
                        m.BeginAction(typeof(Teleporter));
                        m.Frozen = true;

                        Timer.DelayCall(TimeSpan.FromMilliseconds(400), () =>
                        {
                            BaseCreature.TeleportPets(m, destinationPoint, destinationMap);
                            m.MoveToWorld(destinationPoint, destinationMap);
                            m.Frozen = false;

                            Timer.DelayCall(TimeSpan.FromMilliseconds(250), () => m.EndAction(typeof(Teleporter)));
                        });
                    }
                }
            }
        }

        public static void Initialize()
        {
            Timer.DelayCall(() =>
            {
                string filePath = Path.Combine("Data", "TeleporterRegions.xml");

                if (!File.Exists(filePath))
                    return;

                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlElement root = doc["TeleporterRegions"];
                var unique = 1;

                foreach (XmlElement region in root.GetElementsByTagName("Teleporter"))
                {
                    var list = new Dictionary<WorldLocation, WorldLocation>();

                    Map locMap = null;

                    foreach (XmlElement tile in region.GetElementsByTagName("tiles"))
                    {
                        var attr = Utility.GetAttribute(tile, "from", "(0, 0, 0)");
                        Point3D from = Point3D.Parse(attr);
                        Map fromMap = Map.Parse(Utility.GetAttribute(tile, "frommap", null));

                        Point3D to = Point3D.Parse(Utility.GetAttribute(tile, "to", "(0, 0, 0)"));
                        Map toMap = Map.Parse(Utility.GetAttribute(tile, "tomap", null));

                        if (fromMap == null)
                        {
                            throw new ArgumentException(String.Format("Map parsed as null: {0}", from));
                        }

                        if (toMap == null)
                        {
                            throw new ArgumentException(String.Format("Map parsed as null: {0}", to));
                        }

                        list.Add(new WorldLocation(from, fromMap), new WorldLocation(to, toMap));

                        if (list.Count == 1)
                        {
                            locMap = fromMap;
                        }
                    }

                    Rectangle3D[] recs = new Rectangle3D[list.Count];
                    var i = 0;

                    foreach (var kvp in list)
                    {
                        recs[i++] = new Rectangle3D(kvp.Key.Location.X, kvp.Key.Location.Y, kvp.Key.Location.Z - 5, 1, 1, 10);
                    }

                    var teleRegion = new TeleportRegion(string.Format("Teleport Region {0}", unique.ToString()), locMap, recs, list);
                    teleRegion.Register();

                    unique++;
                }
                Console.WriteLine("Initialized {0} Teleporter Regions.", (unique - 1).ToString());
            });
        }
    }
}
