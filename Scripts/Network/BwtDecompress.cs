using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Server
{
    /// <summary>
    /// Provides decompression for new cliloc files using the BWT algorithm.
    /// This implementation is ported from the ClassicUO client.
    /// </summary>
    public static class BwtDecompress
    {
        public static byte[] Decompress(byte[] buffer)
        {
            byte[] output = null;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
            {
                // Read header (first 4 bytes). In some implementations this is the uncompressed length.
                uint header = reader.ReadUInt32();
                // We'll use 'len' later; it is set to 0 and then, if zero, we default it to the sum of partial data.
                uint len = 0u;

                // Read first character (byte)
                byte firstChar = reader.ReadByte();

                Span<ushort> table = new ushort[256 * 256];
                BuildTable(table, firstChar);

                // Read the remainder of the data into a list
                byte[] list = new byte[reader.BaseStream.Length - 4];
                int i = 0;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte currentValue = firstChar;
                    ushort value = table[currentValue];

                    if (currentValue > 0)
                    {
                        do
                        {
                            table[currentValue] = table[currentValue - 1];
                        }
                        while (--currentValue > 0);
                    }

                    table[0] = value;
                    list[i++] = (byte)value;

                    // Ensure we don't read past the stream end.
                    if (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        firstChar = reader.ReadByte();
                    }
                }

                output = InternalDecompress(list, len);
            }

            return output;
        }

        static void BuildTable(Span<ushort> table, byte startValue)
        {
            int index = 0;
            byte firstByte = startValue;
            byte secondByte = 0;

            for (int i = 0; i < 256 * 256; i++)
            {
                ushort val = (ushort)(firstByte + (secondByte << 8));
                table[index++] = val;

                firstByte++;
                if (firstByte == 0)
                {
                    secondByte++;
                }
            }
           
            table.Sort();
        }

        static byte[] InternalDecompress(Span<byte> input, uint len)
        {
            // Create a symbol table and frequency arrays.
            Span<char> symbolTable = stackalloc char[256];
            Span<char> frequency = stackalloc char[256];
            Span<int> partialInput = stackalloc int[256 * 3];

            partialInput.Clear();

            for (int i = 0; i < 256; i++)
            {
                symbolTable[i] = (char)i;
            }

            // Ensure we have enough data to copy.
            if (input.Length < 1024)
            {
                throw new InvalidOperationException("Input data is too short for decompression.");
            }

            input.Slice(0, 1024).CopyTo(MemoryMarshal.AsBytes(partialInput));

            int sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += partialInput[i];
            }

            if (len == 0)
            {
                len = (uint)sum;
            }

            if (sum != len)
            {
                return [];
            }

            byte[] output = new byte[len];

            int count = 0;
            int nonZeroCount = 0;

            for (int i = 0; i < 256; i++)
            {
                if (partialInput[i] != 0)
                {
                    nonZeroCount++;
                }
            }

            Frequency(partialInput, frequency);

            for (int i = 0, m = 0; i < nonZeroCount; ++i)
            {
                byte freq = (byte)frequency[i];
                symbolTable[input[m + 1024]] = (char)freq;
                partialInput[freq + 256] = m + 1;
                m += partialInput[freq];
                partialInput[freq + 512] = m;
            }

            byte val = (byte)symbolTable[0];

            if (len != 0)
            {
                do
                {
                    ref int firstValRef = ref partialInput[val + 256];
                    output[count] = val;

                    if (firstValRef >= partialInput[val + 512])
                    {
                        if (nonZeroCount-- > 0)
                        {
                            ShiftLeft(symbolTable, nonZeroCount);
                            val = (byte)symbolTable[0];
                        }
                    }
                    else
                    {
                        char idx = (char)input[firstValRef + 1024];
                        firstValRef++;

                        if (idx != 0)
                        {
                            ShiftLeft(symbolTable, idx);
                            symbolTable[(byte)idx] = (char)val;
                            val = (byte)symbolTable[0];
                        }
                    }

                    count++;
                } while (count < len);
            }

            return output;
        }

        static void Frequency(Span<int> input, Span<char> output)
        {
            Span<int> tmp = stackalloc int[256];

            input.Slice(0, tmp.Length).CopyTo(tmp);

            for (int i = 0; i < 256; i++)
            {
                uint value = 0;
                byte index = 0;

                for (int j = 0; j < 256; j++)
                {
                    if (tmp[j] > value)
                    {
                        index = (byte)j;
                        value = (uint)tmp[j];
                    }
                }

                if (value == 0)
                {
                    break;
                }

                output[i] = (char)index;
                tmp[index] = 0;
            }
        }

        static void ShiftLeft(Span<char> input, int max)
        {
            for (int i = 0; i < max; ++i)
            {
                input[i] = input[i + 1];
            }
        }
    }
}
