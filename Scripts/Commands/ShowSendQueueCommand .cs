using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;
using Server.Targeting;

namespace Server.Commands
{
    public class ShowSendQueueCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("ShowSendQueue", AccessLevel.Administrator, new CommandEventHandler(ShowSendQueue_OnCommand));
        }

        [Usage("ShowSendQueue")]
        [Description("Displays the current SendQueue details for a targeted player.")]
        public static void ShowSendQueue_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Target a player to view their SendQueue details.");
            e.Mobile.Target = new ShowSendQueueTarget();
        }

        private class ShowSendQueueTarget : Target
        {
            public ShowSendQueueTarget() : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Mobile targ = targeted as Mobile;
                if (targ == null)
                {
                    from.SendMessage("That is not a valid target.");
                    return;
                }

                NetState ns = targ.NetState;
                if (ns == null)
                {
                    from.SendMessage("The target is not online.");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("SendQueue details for " + targ.Name);
                sb.AppendLine("IP: " + ns.Address);
                sb.AppendLine("Connected for: " + FormatTimeSpan(ns.ConnectedFor));

                int pendingCount = ns.GetSendQueuePendingCount();
                sb.AppendLine("SendQueue Pending Count: " + pendingCount);

                List<SendQueue.Gram> snapshot = ns.GetSendQueueSnapshot();
                if (snapshot != null && snapshot.Count > 0)
                {
                    for (int i = 0; i < snapshot.Count; i++)
                    {
                        SendQueue.Gram gram = snapshot[i];
                        sb.AppendLine(string.Format("Gram {0}: Length = {1}, Buffer Size = {2}", i, gram.Length, gram.Buffer.Length));
                    }
                    // If there's exactly one Gram, display a hex dump of its contents.
                    if (snapshot.Count == 1)
                    {
                        SendQueue.Gram gram = snapshot[0];
                        int dumpLength = Math.Min(gram.Length, 64);
                        string hexDump = GetHexDump(gram.Buffer, 0, dumpLength);
                        sb.AppendLine(string.Format("Hex Dump (first {0} bytes): {1}", dumpLength, hexDump));
                    }
                }
                else
                {
                    sb.AppendLine("SendQueue is empty.");
                }

                from.SendMessage(sb.ToString());
            }

            private string FormatTimeSpan(TimeSpan span)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", span.Hours, span.Minutes, span.Seconds);
            }

            private string GetHexDump(byte[] data, int offset, int length)
            {
                StringBuilder hex = new StringBuilder();
                for (int i = offset; i < offset + length; i++)
                {
                    hex.AppendFormat("{0:X2} ", data[i]);
                }
                return hex.ToString().Trim();
            }
        }
    }
}
