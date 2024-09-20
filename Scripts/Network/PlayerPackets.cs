using Server.Engines.Help;

namespace Server.Network
{
    public static class PlayerPackets
    {
        public static void Configure()
        {
            PacketHandlers.Register(0x73, 2, false, PingReq);
            PacketHandlers.Register(0x9B, 258, true, HelpRequest);
        }

        public static void PingReq(NetState state, PacketReader pvSrc)
        {
            state.Send(PingAck.Instantiate(pvSrc.ReadByte()));
        }

        public static void HelpRequest(NetState state, PacketReader pvSrc)
        {
            HelpGump.HelpRequest(state.Mobile);
        }
    }
}
