#region References
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Server.Diagnostics;
#endregion

namespace Server.Network
{
    public class MessagePump
    {
        private Queue<NetState> m_Queue;
        private Queue<NetState> m_WorkingQueue;

        public Listener[] Listeners { get; set; }

        public MessagePump()
        {
            System.Net.IPEndPoint[] ipep = Listener.EndPoints;
            Listeners = new Listener[ipep.Length];
            bool success = false;

            do
            {
                for (int i = 0; i < ipep.Length; i++)
                {
                    Listeners[i] = new Listener(ipep[i]);
                    success = true;
                }

                if (!success)
                {
                    Utility.PushColor(ConsoleColor.Yellow);
                    Console.WriteLine("Retrying...");
                    Utility.PopColor();
                    Thread.Sleep(10000);
                }
            }
            while (!success);

            m_Queue = new Queue<NetState>();
            m_WorkingQueue = new Queue<NetState>();
        }

        private void CheckListener()
        {
            for (int index = 0; index < Listeners.Length; index++)
            {
                Listener l = Listeners[index];
                Socket[] accepted = l.Slice();

                for (int i = 0; i < accepted.Length; i++)
                {
                    Socket s = accepted[i];
                    NetState ns = new NetState(s, this);

                    ns.Start();

                    if (ns.Running && Display(ns))
                    {
                        Utility.PushColor(ConsoleColor.Green);
                        Console.WriteLine("Client: {0}: Connected. [{1} Online]", ns, NetState.Instances.Count);
                        Utility.PopColor();
                    }
                }
            }
        }

        public static bool Display(NetState ns)
        {
            if (ns == null)
            {
                return false;
            }

            string state = ns.ToString();
            string[] noDisplay = { "192.99.10.155", "192.99.69.21" };

            for (int index = 0; index < noDisplay.Length; index++)
            {
                if (noDisplay[index] == state)
                {
                    return false;
                }
            }

            return true;
        }

        public void OnReceive(NetState ns)
        {
            lock (this)
            {
                m_Queue.Enqueue(ns);
            }

            Core.Set();
        }

        public void Slice()
        {
            CheckListener();

            // Swap the working and main queues
            lock (this)
            {
                (m_WorkingQueue, m_Queue) = (m_Queue, m_WorkingQueue);
            }

            while (m_WorkingQueue.Count > 0)
            {
                NetState ns = m_WorkingQueue.Dequeue();
                if (ns.Running)
                {
                    ns.ProcessThrottledPackets();
                    HandleReceive(ns);
                }
            }
        }

        private const int _BufferSize = 4096;
        public static readonly BufferPool ThrottledBufferPool = new BufferPool("Throttled", 16, _BufferSize);

        public static bool HandleSeed(NetState ns, SegmentedByteQueue buffer)
        {
            if (buffer.GetPacketId() == 0xEF)
            {
                ns.Seeded = true;
                return true;
            }

            if (buffer.Length >= 4)
            {
                byte[] temp = new byte[4];
                int copied = buffer.Peek(temp, 0, 4, startIndex: 0);
                if (copied < 4)
                {
                    return false;
                }

                buffer.Advance(4);

                uint seed = (uint)((temp[0] << 24) | (temp[1] << 16) | (temp[2] << 8) | temp[3]);
                if (seed == 0)
                {
                    Utility.PushColor(ConsoleColor.Red);
                    Console.WriteLine("Login: {0}: Invalid Client", ns);
                    Utility.PopColor();

                    ns.Dispose();
                    return false;
                }

                ns.Seed = seed;
                ns.Seeded = true;

                return true;
            }

            return false;
        }

        public static bool CheckEncrypted(NetState ns, int packetID)
        {
            if (!ns.SentFirstPacket && packetID != 0xF0 && packetID != 0xF1 &&
                packetID != 0xCF && packetID != 0x80 && packetID != 0x91 &&
                packetID != 0xA4 && packetID != 0xEF && packetID != 0xE4 && packetID != 0xFF)
            {
                Utility.PushColor(ConsoleColor.Red);
                Console.WriteLine("Client: {0}: Encrypted Client Unsupported", ns);
                Utility.PopColor();
                ns.Dispose();
                return true;
            }
            return false;
        }

        public void HandleReceive(NetState ns)
        {
            // Use the new segmented queue from NetState.
            SegmentedByteQueue buffer = ns.Buffer;

            if (buffer == null || buffer.Length <= 0)
            {
                return;
            }

            lock (buffer)
            {
                if (!ns.Seeded && !HandleSeed(ns, buffer))
                {
                    return;
                }

                int length = buffer.Length;
                while (length > 0 && ns.Running)
                {
                    byte packetID = buffer.GetPacketId();

                    if (CheckEncrypted(ns, packetID))
                    {
                        return;
                    }

                    PacketHandler handler = NetState.GetHandler(packetID);
                    if (handler == null)
                    {
#if DEBUG
                        byte[] data = new byte[length];
                        int copied = buffer.Peek(data, 0, length, 0);
                        buffer.Advance(copied);
                        new PacketReader(data, copied, false).Trace(ns);
#else
                        buffer.Advance(length);
#endif
                        return;
                    }

                    int packetLength = handler.Length;
                    if (packetLength <= 0)
                    {
                        if (length >= 3)
                        {
                            packetLength = buffer.GetPacketLength();
                            if (packetLength < 3)
                            {
                                ns.Dispose();
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (length < packetLength)
                    {
                        return;
                    }

                    if (handler.Ingame)
                    {
                        if (ns.Mobile == null)
                        {
                            Utility.PushColor(ConsoleColor.Red);
                            Console.WriteLine("Client: {0}: Packet (0x{1:X2}) Requires State Mobile", ns, packetID);
                            Utility.PopColor();
                            ns.Dispose();
                            return;
                        }
                        if (ns.Mobile.Deleted)
                        {
                            Utility.PushColor(ConsoleColor.Red);
                            Console.WriteLine("Client: {0}: Packet (0x{1:X2}) Invalid State Mobile", ns, packetID);
                            Utility.PopColor();
                            ns.Dispose();
                            return;
                        }
                    }

                    // Throttling branch
                    ThrottlePacketCallback throttler = handler.ThrottleCallback;
                    if (throttler != null && !throttler(packetID, ns, out bool drop))
                    {
                        if (!drop)
                        {
                            // For delayed packets, copy the packet data from the segmented queue.
                            byte[] temp = ThrottledBufferPool.AcquireBuffer();
                            if (temp.Length < packetLength)
                            {
                                temp = new byte[packetLength];
                            }

                            int copied = buffer.Peek(temp, 0, packetLength, 0);
                            buffer.Advance(packetLength);

                            // Enqueue the delayed data into the NetState's throttled storage.
                            ns.ThrottledQueue.Enqueue(temp, 0, copied);
                        }
                        else
                        {
                            // For dropped packets, simply remove the data.
                            buffer.Advance(packetLength);
                        }
                        return;
                    }

                    PacketReceiveProfile prof = null;
                    if (Core.Profiling)
                    {
                        prof = PacketReceiveProfile.Acquire(packetID);
                    }

                    prof?.Start();

                    // Copy packet data into a contiguous array.
                    byte[] packetBuffer = new byte[packetLength];
                    int copiedData = buffer.Peek(packetBuffer, 0, packetLength, 0);
                    if (copiedData < packetLength)
                    {
                        return; // not enough data
                    }

                    buffer.Advance(packetLength);

                    // Process packet using PacketReader.
                    if (packetBuffer.Length > 0)
                    {
                        PacketReader reader = new PacketReader(packetBuffer, packetLength, handler.Length != 0);
                        handler.OnReceive(ns, reader);

                        ns.SetPacketTime(packetID);
                    }

                    prof?.Finish(packetLength);

                    length = buffer.Length;
                }
            }
        }
    }
}
