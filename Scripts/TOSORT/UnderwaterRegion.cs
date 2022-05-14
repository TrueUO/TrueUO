using Server.Items;
using Server.Mobiles;
using System.Xml;

namespace Server.Regions
{
    public class Underwater : BaseRegion
    {
        public Underwater(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (!base.OnMoveInto(m, d, newLocation, oldLocation))
                return false;

            if (m is PlayerMobile pm)
            {
                int equipment = 0;

                for (var index = 0; index < pm.Items.Count; index++)
                {
                    var i = pm.Items[index];

                    if ((i is CanvassRobe || i is BootsOfBallast || i is NictitatingLens || i is AquaPendant) && i.Parent is Mobile mobile && mobile.FindItemOnLayer(i.Layer) == i)
                    {
                        equipment++;
                    }
                }

                if (pm.AccessLevel == AccessLevel.Player)
                {
                    if (pm.Mounted || pm.Flying)
                    {
                        pm.SendLocalizedMessage(1154411); // You cannot proceed while mounted or flying!
                        return false;
                    }

                    if (pm.AllFollowers.Count != 0)
                    {
                        int count = 0;

                        for (var index = 0; index < pm.AllFollowers.Count; index++)
                        {
                            var x = pm.AllFollowers[index];

                            if (x is Paralithode)
                            {
                                count++;
                            }
                        }

                        if (count == 0)
                        {
                            pm.SendLocalizedMessage(1154412); // You cannot proceed while pets are under your control!
                            return false;
                        }
                    }
                    else if (equipment < 4)
                    {
                        pm.SendLocalizedMessage(1154413); // You couldn't hope to survive proceeding without the proper equipment...
                        return false;
                    }
                }
            }
            else if (m is BaseCreature && !(m is Paralithode))
            {
                return false;
            }

            return true;
        }

        public override void OnExit(Mobile m)
        {
            if (m is Paralithode)
            {
                m.Delete();
            }
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        public override bool CheckTravel(Mobile m, Point3D newLocation, Spells.TravelCheckType travelType)
        {
            return false;
        }
    }
}
