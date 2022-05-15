using Server.Mobiles;
using Server.Regions;
using Server.Spells;
using System.Xml;

namespace Server.Engines.Blackthorn
{
    public class BlackthornDungeon : DungeonRegion
    {
        public BlackthornDungeon(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            if (traveller.AccessLevel > AccessLevel.Player)
            {
                return true;
            }

            return type > TravelCheckType.Mark;
        }

        public override void OnDeath(Mobile m)
        {
            if (m is BaseCreature creature && Map == Map.Trammel && InvasionController.TramInstance != null)
            {
                InvasionController.TramInstance.OnDeath(creature);
            }

            if (m is BaseCreature baseCreature && Map == Map.Felucca && InvasionController.FelInstance != null)
            {
                InvasionController.FelInstance.OnDeath(baseCreature);
            }
        }
    }
}
