using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Engines.Help;

namespace Server.Network
{
    public static class PlayerPackets
    {
        public static void Configure()
        {
            PacketHandlers.Register(0x9B, 258, true, HelpRequest);
        }

        public static void HelpRequest(NetState state, PacketReader pvSrc)
        {
            HelpGump.HelpRequest(state.Mobile);
        }
    }
}
