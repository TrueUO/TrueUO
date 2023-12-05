using Server.Network;

namespace Server.Misc
{
    public class LoginStats
    {
        public static void OnLogin(Mobile m)
        {
            int userCount = NetState.Instances.Count;
            int itemCount = World.Items.Count;
            int mobileCount = World.Mobiles.Count;

            m.SendMessage("Welcome, {0}! There {1} currently {2} user{3} online, with {4} item{5} and {6} mobile{7} in the world.",
                m.Name,
                userCount == 1 ? "is" : "are",
                userCount, userCount == 1 ? "" : "s",
                itemCount, itemCount == 1 ? "" : "s",
                mobileCount, mobileCount == 1 ? "" : "s");

            if (m.IsStaff())
            {
                Engines.Help.PageQueue.Pages_OnCalled(m);
            }
        }
    }
}
