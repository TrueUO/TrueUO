using Server.Mobiles;
using Server.Network;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Server.Engines.Blackthorn
{
    public class WrongLevel3 : DungeonRegion
    {
        private readonly List<Mobile> DeathList = new List<Mobile>();

        public WrongLevel3(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override void OnDeath(Mobile m)
        {
            if (m is PlayerMobile)
            {
                m.MoveToWorld(new Point3D(5703, 639, 0), Map);

                if (!DeathList.Remove(m))
                {
                    m.Resurrect();
                    DeathList.Add(m);
                }

                Timer.DelayCall(TimeSpan.FromSeconds(2), () => m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1152076)); // You are captured by the jailor and returned to your cell.                       
            }
        }

        public override void OnExit(Mobile m)
        {
            if (m is PlayerMobile)
            {
                DeathList.Remove(m);
            }

            base.OnExit(m);
        }
    }

    public class WrongJail : DungeonRegion
    {
        public WrongJail(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override bool AllowHarmful(Mobile from, IDamageable target)
        {
            return false;
        }
    }
}
