#region References
using System;
using System.Collections.Generic;
#endregion

namespace Server.Network
{
	public class SendQueue
	{
        public int PendingCount => _pending.Count;

        public List<Gram> GetSnapshot()
        {
            // Return a shallow copy of the pending Gram objects
            return new List<Gram>(_pending);
        }

		public class Gram
		{
			private static readonly Stack<Gram> _pool = new Stack<Gram>();

			public static Gram Acquire()
			{
                lock (_pool)
                {
                    Gram gram = _pool.Count > 0 ? _pool.Pop() : new Gram();

                    // Acquire a fresh buffer from the pool
                    gram._buffer = AcquireBuffer();

                    // Clear out the buffer to remove any residual data
                    //Array.Clear(gram._buffer, 0, gram._buffer.Length);

                    gram._length = 0;
                    return gram;
                }
			}

			private byte[] _buffer;
			private int _length;

			public byte[] Buffer => _buffer;
			public int Length => _length;
			public int Available => _buffer.Length - _length;
			public bool IsFull => _length == _buffer.Length;

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
                    // Return the current buffer to the pool
                    ReleaseBuffer(_buffer);

                    _buffer = null;
                    _pool.Push(this);
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
			Gram gram = null;

			if (_pending.Count > 0)
			{
				_pending.Dequeue().Release();

				if (_pending.Count > 0)
				{
					gram = _pending.Peek();
				}
			}

			return gram;
		}

		private const int PendingCap = 0x200000;

		public Gram Enqueue(byte[] buffer, int length)
		{
			return Enqueue(buffer, 0, length);
		}

		public Gram Enqueue(byte[] buffer, int offset, int length)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

            if (!(offset >= 0 && offset < buffer.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be greater than or equal to zero and less than the size of the buffer.");
            }

            if (length < 0 || length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length cannot be less than zero or greater than the size of the buffer.");
            }

            if (buffer.Length - offset < length)
            {
                throw new ArgumentException("Offset and length do not point to a valid segment within the buffer.");
            }

            int existingBytes = (_pending.Count * m_CoalesceBufferSize) + (_buffered == null ? 0 : _buffered.Length);

			if (existingBytes + length > PendingCap)
			{
				throw new CapacityExceededException();
			}

			Gram gram = null;

			while (length > 0)
			{
				if (_buffered == null)
				{
					// nothing yet buffered
					_buffered = Gram.Acquire();
				}

				int bytesWritten = _buffered.Write(buffer, offset, length);

				offset += bytesWritten;
				length -= bytesWritten;

				if (_buffered.IsFull)
				{
					if (_pending.Count == 0)
					{
						gram = _buffered;
					}

					_pending.Enqueue(_buffered);
					_buffered = null;
				}
			}

			return gram;
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
	}

	[Serializable]
	public sealed class CapacityExceededException : Exception
	{
		public CapacityExceededException()
			: base("Too much data pending.")
		{ }
	}
}
