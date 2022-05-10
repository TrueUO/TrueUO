using Server.Mobiles;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Ninjitsu;
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
                return true;

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

    public class BlackthornCastle : GuardedRegion
    {
        public static readonly Point3D[] StableLocs = { new Point3D(1510, 1543, 25),
            new Point3D(1516, 1542, 25), new Point3D(1520, 1542, 25), new Point3D(1525, 1542, 25) };

        public BlackthornCastle(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (m.AccessLevel > AccessLevel.Player)
            {
                return base.OnBeginSpellCast(m, s);
            }

            int loc;

            if (s is PaladinSpell)
            {
                loc = 1062075; // You cannot use a Paladin ability here.
            }
            else if (s is NinjaMove || s is NinjaSpell || s is SamuraiSpell || s is SamuraiMove)
            {
                loc = 1062938; // That ability does not seem to work here.
            }
            else
            {
                loc = 502629;
            }

            m.SendLocalizedMessage(loc);
            return false;
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            if (traveller.AccessLevel > AccessLevel.Player)
                return true;

            return type > TravelCheckType.Mark;
        }
    }
}
