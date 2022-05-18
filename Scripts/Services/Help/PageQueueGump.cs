using Server.Gumps;
using Server.Network;

namespace Server.Engines.Help
{
    public class MessageSentGump : Gump
    {
        private readonly string m_Name;
        private readonly string m_Text;
        private readonly Mobile m_Mobile;

        public MessageSentGump(Mobile mobile, string name, string text)
            : base(30, 30)
        {
            m_Name = name;
            m_Text = text;
            m_Mobile = mobile;

            Closable = false;

            AddPage(0);

            AddBackground(0, 0, 92, 75, 0xA3C);

            if (mobile != null && mobile.NetState != null && mobile.NetState.IsEnhancedClient)
            {
                AddBackground(5, 7, 82, 61, 9300);
            }
            else
            {
                AddImageTiled(5, 7, 82, 61, 0xA40);
                AddAlphaRegion(5, 7, 82, 61);
            }

            AddImageTiled(9, 11, 21, 53, 0xBBC);

            AddButton(10, 12, 0x7D2, 0x7D2, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(34, 28, 65, 24, 3001002, 0xFFFFFF, false, false); // Message
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            m_Mobile.SendGump(new PageResponseGump(m_Mobile, m_Name, m_Text));
        }
    }
}
