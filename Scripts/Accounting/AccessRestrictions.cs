using Server.Misc;
using System;
using System.IO;
using System.Net;

namespace Server
{
    public class AccessRestrictions
    {
        public static void Initialize()
        {
            EventSink.SocketConnect += EventSink_SocketConnect;
        }

        private static void EventSink_SocketConnect(SocketConnectEventArgs e)
        {
            try
            {
                IPAddress ip = ((IPEndPoint)e.Socket.RemoteEndPoint).Address;

                if (Firewall.IsBlocked(ip))
                {
                    Utility.PushColor(ConsoleColor.Red);
                    Console.WriteLine("Client: {0}: Firewall blocked connection attempt.", ip);
                    Utility.PopColor();
                    e.AllowConnection = false;
                }
                else if (IPLimiter.SocketBlock && !IPLimiter.Verify(ip))
                {
                    Utility.PushColor(ConsoleColor.Red);
                    Console.WriteLine("Client: {0}: Past IP limit threshold", ip);
                    Utility.PopColor();

                    using (StreamWriter op = new StreamWriter("ipLimits.log", true))
                        op.WriteLine("{0}\tPast IP limit threshold\t{1}", ip, DateTime.UtcNow);

                    e.AllowConnection = false;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.ExceptionLogging.LogException(ex);
                e.AllowConnection = false;
            }
        }
    }
}
