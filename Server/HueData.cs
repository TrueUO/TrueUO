using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Server
{
    public static class HueData
    {
        private static readonly int[] _Header = Array.Empty<int>();

        public static Hue[] List { get; } = new Hue[3000];

        public static bool CheckFile => File.Exists(Core.FindDataFile("hues.mul"));

        static HueData()
        {
            int index = 0;

            if (CheckFile)
            {
                string path = Core.FindDataFile("hues.mul");

                if (path != null)
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        int blockCount = (int)fs.Length / 708;

                        if (blockCount > 375)
                        {
                            blockCount = 375;
                        }

                        _Header = new int[blockCount];

                        int structsize = Marshal.SizeOf(typeof(HueEntry));
                        byte[] buffer = new byte[blockCount * (4 + 8 * structsize)];

                        GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                        try
                        {
                            fs.Read(buffer, 0, buffer.Length);

                            long currpos = 0;

                            for (int i = 0; i < blockCount; ++i)
                            {
                                IntPtr ptrheader = new IntPtr((long)gc.AddrOfPinnedObject() + currpos);

                                currpos += 4;

                                _Header[i] = (int)Marshal.PtrToStructure(ptrheader, typeof(int));

                                for (int j = 0; j < 8; ++j, ++index)
                                {
                                    IntPtr ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currpos);

                                    currpos += structsize;

                                    HueEntry cur = (HueEntry)Marshal.PtrToStructure(ptr, typeof(HueEntry));

                                    List[index] = new Hue(index, cur);
                                }
                            }
                        }
                        finally
                        {
                            gc.Free();
                        }
                    }
                }
            }

            while (index < List.Length)
            {
                List[index] = new Hue(index);

                ++index;
            }
        }

        public sealed class Hue
        {
            public int Index { get; }

            public short[] Colors { get; } = new short[32];

            public string Name { get; }

            public short TableStart { get; }
            public short TableEnd { get; }

            internal Hue(int index)
            {
                Name = "Null";
                Index = index;
            }

            internal Hue(int index, HueEntry entry)
            {
                Index = index;

                for (int i = 0; i < 32; ++i)
                {
                    Colors[i] = (short)(entry.Colors[i] | 0x8000);
                }

                TableStart = (short)(entry.TableStart | 0x8000);
                TableEnd = (short)(entry.TableEnd | 0x8000);

                int count = 0;

                while (count < 20 && count < entry.Name.Length && entry.Name[count] != 0)
                {
                    ++count;
                }

                Name = Encoding.Default.GetString(entry.Name, 0, count);
                Name = Name.Replace("\n", " ");
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct HueEntry
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public ushort[] Colors;

            public ushort TableStart;
            public ushort TableEnd;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Name;
        }
    }
}
