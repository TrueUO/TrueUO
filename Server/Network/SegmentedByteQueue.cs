using System;
using System.Collections.Generic;

namespace Server.Network
{
    /// <summary>
    /// Represents a segment of byte data.
    /// </summary>
    public class ByteQueueSegment
    {
        public byte[] Buffer;
        public int Offset;   // start index in Buffer for valid data
        public int Count;    // number of valid bytes in this segment

        public ByteQueueSegment(byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }
    }

    /// <summary>
    /// A segmented queue for bytes that uses a linked list of segments.
    /// Provides methods to Enqueue data, remove (Advance) processed data,
    /// and to peek at the first byte or first three bytes (for headers).
    /// </summary>
    public class SegmentedByteQueue
    {
        private readonly LinkedList<ByteQueueSegment> _Segments = new LinkedList<ByteQueueSegment>();
        public int Length { get; private set; }

        /// <summary>
        /// Clears all segments.
        /// </summary>
        public void Clear()
        {
            _Segments.Clear();
            Length = 0;
        }

        /// <summary>
        /// Enqueues data by copying it into a new segment.
        /// </summary>
        public void Enqueue(byte[] buffer, int offset, int count)
        {
            if (count <= 0)
            {
                return;
            }

            // Copy data into a new array (I might optimize this further)
            byte[] copy = new byte[count];

            Buffer.BlockCopy(buffer, offset, copy, 0, count);

            _Segments.AddLast(new ByteQueueSegment(copy, 0, count));

            Length += count;
        }

        /// <summary>
        /// Advances the queue by removing 'count' bytes from the beginning.
        /// </summary>
        public void Advance(int count)
        {
            if (count <= 0 || count > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            int remaining = count;
            while (remaining > 0 && _Segments.Count > 0)
            {
                if (_Segments.First != null)
                {
                    ByteQueueSegment seg = _Segments.First.Value;
                    if (seg.Count > remaining)
                    {
                        seg.Offset += remaining;
                        seg.Count -= remaining;
                        remaining = 0;
                    }
                    else
                    {
                        remaining -= seg.Count;
                        _Segments.RemoveFirst();
                    }
                }
            }

            Length -= count;
        }

        /// <summary>
        /// Peeks at the first byte in the queue without removing it.
        /// Returns 0xFF if no data is available.
        /// </summary>
        public byte GetPacketId()
        {
            if (Length < 1)
            {
                return 0xFF;
            }

            // Get the first segment's first byte.
            return _Segments.First.Value.Buffer[_Segments.First.Value.Offset];
        }

        /// <summary>
        /// Reads two bytes (starting at index 1) to determine the packet length.
        /// Returns 0 if insufficient data.
        /// </summary>
        public int GetPacketLength()
        {
            if (Length < 3)
            {
                return 0;
            }

            // We need the bytes at overall positions 1 and 2.
            byte[] temp = new byte[2];
            int copied = Peek(temp, 0, 2, startIndex: 1);
            if (copied < 2)
            {
                return 0;
            }

            return (temp[0] << 8) | temp[1];
        }

        /// <summary>
        /// Peeks at up to 'count' bytes starting at the given overall index without removing them.
        /// </summary>
        public int Peek(byte[] output, int outputOffset, int count, int startIndex = 0)
        {
            if (count <= 0 || Length <= startIndex)
            {
                return 0;
            }

            int copied = 0;
            int currentPos = 0;
            foreach (ByteQueueSegment seg in _Segments)
            {
                if (currentPos + seg.Count <= startIndex)
                {
                    currentPos += seg.Count;
                    continue;
                }

                // Compute start index within this segment.
                int segStart = seg.Offset;
                if (startIndex > currentPos)
                {
                    segStart += startIndex - currentPos;
                }

                int available = seg.Count - Math.Max(0, startIndex - currentPos);
                int toCopy = Math.Min(available, count - copied);

                Buffer.BlockCopy(seg.Buffer, segStart, output, outputOffset + copied, toCopy);

                copied += toCopy;

                currentPos += seg.Count;

                if (copied >= count)
                {
                    break;
                }
            }

            return copied;
        }
    }
}
