#region References
using System;
using System.Collections.Generic;
#endregion

namespace Server.Network
{
	public class SendQueue
	{
		public class Gram
		{
            public static Gram CreateDirect(int length)
            {
                // Create a new Gram with a buffer sized exactly for the packet.
                Gram gram = new Gram
                {
                    _buffer = new byte[length],
                    _length = 0,
                    IsPooled = false
                };

                return gram;
            }

			private static readonly Stack<Gram> _pool = new Stack<Gram>();

			public static Gram Acquire()
			{
				lock (_pool)
				{
					Gram gram;

					if (_pool.Count > 0)
					{
						gram = _pool.Pop();
					}
					else
					{
						gram = new Gram();
					}
					// Acquire a new buffer from the pool
					gram._buffer = AcquireBuffer();
					gram._length = 0;
					// Default: assume it is pooled; it may be overwritten later if needed.
					gram.IsPooled = true;
					return gram;
				}
			}

			private byte[] _buffer;
			private int _length;
			public bool IsPooled;  // NEW: tracks if the buffer is from the pool

			public byte[] Buffer => _buffer;

			public int Length => _length;

			public int Available => _buffer.Length - _length;

			private Gram()
			{ }

			public int Write(byte[] buffer, int offset, int length)
			{
				int write = Math.Min(length, Available);

				System.Buffer.BlockCopy(buffer, offset, _buffer, _length, write);

				_length += write;

				return write;
			}

			public void Release()
			{
				lock (_pool)
				{
					_pool.Push(this);
					ReleaseBuffer(_buffer);
				}
			}
		}

		private static int m_CoalesceBufferSize = 512;
		private static BufferPool m_UnusedBuffers = new BufferPool("Coalesced", 2048, m_CoalesceBufferSize);

		public static int CoalesceBufferSize
		{
			get => m_CoalesceBufferSize;
			set
			{
				if (m_CoalesceBufferSize == value)
				{
					return;
				}

				BufferPool old = m_UnusedBuffers;

				lock (old)
				{
					if (m_UnusedBuffers != null)
					{
						m_UnusedBuffers.Free();
					}

					m_CoalesceBufferSize = value;
					m_UnusedBuffers = new BufferPool("Coalesced", 2048, m_CoalesceBufferSize);
				}
			}
		}

		public static byte[] AcquireBuffer()
		{
			lock (m_UnusedBuffers)
				return m_UnusedBuffers.AcquireBuffer();
		}

		public static void ReleaseBuffer(byte[] buffer)
		{
			lock (m_UnusedBuffers)
			{
				if (buffer != null && buffer.Length == m_CoalesceBufferSize)
				{
					m_UnusedBuffers.ReleaseBuffer(buffer);
				}
			}
		}

		private readonly Queue<Gram> _pending;

		private Gram _buffered;

		public bool IsFlushReady => _pending.Count == 0 && _buffered != null;

		public bool IsEmpty => _pending.Count == 0 && _buffered == null;

		public SendQueue()
		{
			_pending = new Queue<Gram>();
		}

		public Gram CheckFlushReady()
		{
			Gram gram = _buffered;
			_pending.Enqueue(_buffered);
			_buffered = null;
			return gram;
		}

		public Gram Dequeue()
		{
			if (_pending.Count > 0)
			{
				return _pending.Dequeue();
			}
			return null;
		}

        public void Clear()
		{
			if (_buffered != null)
			{
				_buffered.Release();
				_buffered = null;
			}

			while (_pending.Count > 0)
			{
				_pending.Dequeue().Release();
			}
		}

        public int PendingCount
        {
            get
            {
                int count = _pending.Count;
                if (_buffered != null)
                    count++;
                return count;
            }
        }

        public List<Gram> GetSnapshot()
        {
            List<Gram> snapshot = new List<Gram>();
            lock (_pending)
            {
                snapshot.AddRange(_pending);
            }
            if (_buffered != null)
                snapshot.Add(_buffered);
            return snapshot;
        }
	}

	[Serializable]
	public sealed class CapacityExceededException : Exception
	{
		public CapacityExceededException()
			: base("Too much data pending.")
		{ }
	}
}
