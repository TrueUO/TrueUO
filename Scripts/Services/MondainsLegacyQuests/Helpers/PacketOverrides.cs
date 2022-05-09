using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Engines.Quests
{
    public class PacketOverrides
    {
        public static void Initialize()
        {
            Timer.DelayCall(TimeSpan.Zero, Override);
        }

        public static void Override()
        {
            PacketHandlers.RegisterEncoded(0x32, true, QuestButton);
        }

        public static void QuestButton(NetState state, IEntity e, EncodedReader reader)
        {
            if (state.Mobile is PlayerMobile from)
            {
                from.CloseGump(typeof(MondainQuestGump));
                from.SendGump(new MondainQuestGump(from));
            }
        }
    }
}
