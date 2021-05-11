using Server.Gumps;
using Server.Network;

namespace Server.Engines.Quests
{
    public class HumilityItemQuestGump : Gump
    {
        private readonly HumilityQuestMobile m_Mobile;
        private readonly WhosMostHumbleQuest m_Quest;
        private readonly int m_NPCIndex;

        public HumilityItemQuestGump(HumilityQuestMobile mobile, WhosMostHumbleQuest quest, int index)
            : base(75, 25)
        {
            m_Mobile = mobile;
            m_Quest = quest;
            m_NPCIndex = index;

            Disposable = false;
            Closable = false;
            AddImageTiled(50, 20, 400, 400, 0x1404);
            AddImageTiled(83, 15, 350, 15, 0x280A);
            AddImageTiled(50, 29, 30, 390, 0x28DC);
            AddImageTiled(34, 140, 17, 279, 0x242F);
            AddImage(48, 135, 0x28AB);
            AddImage(-16, 285, 0x28A2);
            AddImage(0, 10, 0x28B5);
            AddImage(25, 0, 0x28B4);
            AddImageTiled(415, 29, 44, 390, 0xA2D);
            AddImageTiled(415, 29, 30, 390, 0x28DC);
            AddLabel(100, 50, 0x481, "");
            AddImage(370, 50, 0x589);
            AddImage(379, 60, 0x15E8);
            AddImage(425, 0, 0x28C9);
            AddImage(34, 419, 0x2842);
            AddImage(442, 419, 0x2840);
            AddImageTiled(51, 419, 392, 17, 0x2775);
            AddHtmlLocalized(130, 45, 270, 16, 1049010, 0x7FFF, false, false); // Quest Offer
            AddHtmlLocalized(98, 156, 312, 180, mobile.Greeting + 1, 0x15F90, false, true);
            AddImage(90, 33, 0x232D);
            AddImageTiled(130, 65, 175, 1, 0x238D);
            AddButton(95, 395, 0x2EE9, 0x2EEB, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(0, 0, 0, 0, 1011036, false, false); // OKAY
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (m_NPCIndex < 0 || m_NPCIndex >= m_Quest.Infos.Count)
                return;

            Mobile from = state.Mobile;

            int cliloc;
            string args;

            if (0.5 > Utility.RandomDouble() || m_NPCIndex == 6)
            {
                cliloc = m_Mobile.Greeting + 2;
                args = string.Format("#{0}", m_Quest.Infos[m_NPCIndex].NeedsLoc);
            }
            else
            {
                cliloc = m_Mobile.Greeting + 3;
                args = string.Format("#{0}", m_Quest.Infos[m_NPCIndex].GivesLoc);
            }

            m_Mobile.SayTo(from, cliloc, args);
        }
    }
}
