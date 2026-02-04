using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Server
{
    public static class NativeReader
    {
        private static readonly INativeReader m_NativeReader;

        static NativeReader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                m_NativeReader = new NativeReaderWin32();
            }
            else
            {
                m_NativeReader = new NativeReaderUnix();
            }
        }

        public static unsafe void Read(IntPtr handle, void* buffer, int length)
        {
            m_NativeReader.Read(handle, buffer, length);
        }
    }

    public interface INativeReader
    {
        unsafe void Read(IntPtr handle, void* buffer, int length);
    }

    // Windows version (kept if you also want Windows support)
    public sealed class NativeReaderWin32 : INativeReader
    {
        public unsafe void Read(IntPtr handle, void* buffer, int length)
        {
            // Use SafeFileHandle + FileStream for managed Windows read
            using var fs = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, false), FileAccess.Read);
            byte[] temp = new byte[length];
            int read = fs.Read(temp, 0, length);
            fixed (byte* p = temp)
            {
                Buffer.MemoryCopy(p, buffer, length, read);
            }
        }
    }

    // Linux / Unix version
    public sealed class NativeReaderUnix : INativeReader
    {
        public unsafe void Read(IntPtr fd, void* buffer, int length)
        {
            // Use managed FileStream for simplicity
            var handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(fd, false);
            using var fs = new FileStream(handle, FileAccess.Read);
            byte[] temp = new byte[length];
            int read = fs.Read(temp, 0, length);
            fixed (byte* p = temp)
            {
                Buffer.MemoryCopy(p, buffer, length, read);
            }
        }
    }
}
