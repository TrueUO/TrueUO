#region References
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;
#endregion

namespace Server.Targets
{
    public class AIControlMobileTarget : Target
    {
        private readonly List<BaseAI> m_List;
        private readonly LastOrderType m_Order;
        private readonly BaseCreature m_Mobile;

        public AIControlMobileTarget(BaseAI ai, LastOrderType order)
            : base(-1, false, (order == LastOrderType.Attack ? TargetFlags.Harmful : TargetFlags.None))
        {
            m_List = new List<BaseAI>();
            m_Order = order;

            AddAI(ai);
            m_Mobile = ai.m_Mobile;
        }

        public LastOrderType Order => m_Order;

        public void AddAI(BaseAI ai)
        {
            if (!m_List.Contains(ai))
                m_List.Add(ai);
        }

        protected override void OnTarget(Mobile from, object o)
        {
            if (o is IDamageable damageable)
            {
                for (int i = 0; i < m_List.Count; ++i)
                    m_List[i].EndPickTarget(from, damageable, m_Order);
            }
            else if (o is MoonglowDonationBox box && m_Order == LastOrderType.Transfer && from is PlayerMobile pm)
            {
                pm.SendGump(new ConfirmTransferPetGump(box, pm.Location, m_Mobile));
            }
        }
    }
}
