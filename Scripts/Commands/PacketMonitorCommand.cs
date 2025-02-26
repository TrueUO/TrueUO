using Server.Network;

namespace Server.Commands
{
    public class PacketMonitorCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("MonitorPackets", AccessLevel.Administrator, new CommandEventHandler(MonitorPackets_OnCommand));
        }

        [Usage("MonitorPackets")]
        [Description("Displays debugging information about the send queue and buffer pool stats.")]
        public static void MonitorPackets_OnCommand(CommandEventArgs e)
        {
            NetState ns = e.Mobile.NetState;
            if (ns == null)
            {
                e.Mobile.SendMessage("No active network state found.");
                return;
            }

            e.Mobile.SendMessage("=== Packet Monitor ===");
            // Display connection info
            e.Mobile.SendMessage("Address: {0}", ns.Address);
            e.Mobile.SendMessage("Connected For: {0}", ns.ConnectedFor);

            // Display buffer pool stats (using the public properties in NetState)
            e.Mobile.SendMessage("SendBufferPool Count: {0}", NetState.SendBuffers.Count);
            e.Mobile.SendMessage("ReceiveBufferPool Count: {0}", NetState.ReceiveBuffers.Count);
        }
    }
}
