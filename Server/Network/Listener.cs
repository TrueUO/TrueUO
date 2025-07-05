using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Server.Network
{
	public sealed class Listener : IDisposable
	{
		private Socket m_Listener;
		private PingListener _PingListener;

		private readonly Queue<Socket> m_Accepted;
		private readonly object m_AcceptedSyncRoot;

		private readonly AsyncCallback m_OnAccept;

		private static readonly Socket[] m_EmptySockets = Array.Empty<Socket>();

		public static IPEndPoint[] EndPoints { get; set; }

		public Listener(IPEndPoint ipep)
		{
			m_Accepted = new Queue<Socket>();
			m_AcceptedSyncRoot = ((ICollection)m_Accepted).SyncRoot;

			m_Listener = Bind(ipep);

			if (m_Listener == null)
			{
				return;
			}

			DisplayListener();
			_PingListener = new PingListener(ipep);

			m_OnAccept = OnAccept;
			try
			{
				IAsyncResult res = m_Listener.BeginAccept(m_OnAccept, m_Listener);
			}
			catch (SocketException ex)
			{
				NetState.TraceException(ex);
			}
			catch (ObjectDisposedException)
			{ }
		}

		private static Socket Bind(IPEndPoint ipep)
		{
			Socket s = new Socket(ipep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				s.LingerState.Enabled = false;

				// Default is 'false' starting Windows Vista and Server 2008. Source: https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.exclusiveaddressuse(v=vs.110).aspx?f=255&MSPPError=-2147217396
				s.ExclusiveAddressUse = false;

				s.Bind(ipep);
				s.Listen(8);

				return s;
			}
			catch (Exception e)
			{
				if (e is SocketException se)
				{
                    if (se.ErrorCode == 10048)
					{
						// WSAEADDRINUSE
						Utility.PushColor(ConsoleColor.Red);
						Console.WriteLine("Listener Failed: {0}:{1} (In Use)", ipep.Address, ipep.Port);
						Utility.PopColor();
					}
					else if (se.ErrorCode == 10049)
					{
						// WSAEADDRNOTAVAIL
						Utility.PushColor(ConsoleColor.Red);
						Console.WriteLine("Listener Failed: {0}:{1} (Unavailable)", ipep.Address, ipep.Port);
						Utility.PopColor();
					}
					else
					{
						Utility.PushColor(ConsoleColor.Red);
						Console.WriteLine("Listener Exception:");
						Console.WriteLine(e);
						Utility.PopColor();
					}
				}

				return null;
			}
		}

		private void DisplayListener()
		{
			IPEndPoint ipep = m_Listener.LocalEndPoint as IPEndPoint;

			if (ipep == null)
			{
				return;
			}

			if (ipep.Address.Equals(IPAddress.Any) || ipep.Address.Equals(IPAddress.IPv6Any))
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

                for (var index = 0; index < adapters.Length; index++)
                {
                    NetworkInterface adapter = adapters[index];
                    IPInterfaceProperties properties = adapter.GetIPProperties();

                    for (var i = 0; i < properties.UnicastAddresses.Count; i++)
                    {
                        UnicastIPAddressInformation unicast = properties.UnicastAddresses[i];

                        if (ipep.AddressFamily == unicast.Address.AddressFamily)
                        {
                            Utility.PushColor(ConsoleColor.Green);
                            Console.WriteLine("Listening: {0}:{1}", unicast.Address, ipep.Port);
                            Utility.PopColor();
                        }
                    }
                }
            }
			else
			{
				Utility.PushColor(ConsoleColor.Green);
				Console.WriteLine("Listening: {0}:{1}", ipep.Address, ipep.Port);
				Utility.PopColor();
			}

			Utility.PushColor(ConsoleColor.DarkGreen);
			Console.WriteLine("----------------------------------------------------------------------");
			Utility.PopColor();
		}

		private void OnAccept(IAsyncResult asyncResult)
		{
			Socket listener = (Socket)asyncResult.AsyncState;

			Socket accepted = null;

			try
			{
				accepted = listener.EndAccept(asyncResult);
			}
			catch (SocketException ex)
			{
				NetState.TraceException(ex);
			}
			catch (ObjectDisposedException)
			{
				return;
			}

			if (accepted != null)
			{
				if (VerifySocket(accepted))
				{
					Enqueue(accepted);
				}
				else
				{
					Release(accepted);
				}
			}

			try
			{
				listener.BeginAccept(m_OnAccept, listener);
			}
			catch (SocketException ex)
			{
				NetState.TraceException(ex);
			}
			catch (ObjectDisposedException)
			{ }
		}

		private static bool VerifySocket(Socket socket)
		{
			try
			{
				SocketConnectEventArgs args = new SocketConnectEventArgs(socket);

				EventSink.InvokeSocketConnect(args);

				return args.AllowConnection;
			}
			catch (Exception ex)
			{
				NetState.TraceException(ex);

				return false;
			}
		}

		private void Enqueue(Socket socket)
		{
			lock (m_AcceptedSyncRoot)
			{
				m_Accepted.Enqueue(socket);
			}

			Core.Set();
		}

		private static void Release(Socket socket)
		{
			try
			{
				socket.Shutdown(SocketShutdown.Both);
			}
			catch (SocketException ex)
			{
				NetState.TraceException(ex);
			}

			try
			{
				socket.Close();
			}
			catch (SocketException ex)
			{
				NetState.TraceException(ex);
			}
		}

		public Socket[] Slice()
		{
			Socket[] array;

			lock (m_AcceptedSyncRoot)
			{
				if (m_Accepted.Count == 0)
				{
					return m_EmptySockets;
				}

				array = m_Accepted.ToArray();
				m_Accepted.Clear();
			}

			return array;
		}

		public void Dispose()
		{
			Socket socket = Interlocked.Exchange(ref m_Listener, null);

			if (socket != null)
			{
				socket.Close();
			}

			if (_PingListener == null)
			{
				return;
			}

			_PingListener.Dispose();
			_PingListener = null;
		}
	}
}
