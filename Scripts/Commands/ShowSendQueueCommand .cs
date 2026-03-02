using System;
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

                sb.AppendLine(pendingCount > 0 ? "SendQueue contains pending bytes." : "SendQueue is empty.");

                from.SendMessage(sb.ToString());
            }

            private string FormatTimeSpan(TimeSpan span)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", span.Hours, span.Minutes, span.Seconds);
            }

        }
    }
}
