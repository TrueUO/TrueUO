using System.Collections.Generic;
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

            // Display send queue info.
            // (Assuming you added a public method GetPendingCount() in NetState that returns the number of pending Grams)
            int pendingCount = ns.GetSendQueuePendingCount();
            e.Mobile.SendMessage("SendQueue Pending Count: {0}", pendingCount);

            // Optionally, display details of each Gram in the send queue
            List<SendQueue.Gram> snapshot = ns.GetSendQueueSnapshot();
            if (snapshot != null && snapshot.Count > 0)
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    SendQueue.Gram gram = snapshot[i];
                    e.Mobile.SendMessage("Gram {0}: Length = {1}, BufferSize = {2}, IsPooled = {3}",
                        i, gram.Length, gram.Buffer.Length, gram.IsPooled);
                }
            }
            else
            {
                e.Mobile.SendMessage("SendQueue is empty.");
            }

            // Display buffer pool stats (using the public properties in NetState)
            e.Mobile.SendMessage("SendBufferPool Count: {0}", NetState.SendBuffers.Count);
            e.Mobile.SendMessage("ReceiveBufferPool Count: {0}", NetState.ReceiveBuffers.Count);
        }
    }
}
