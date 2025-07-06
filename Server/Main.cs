using Server.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public delegate void Slice();

    public static class Core
    {
        static Core()
        {
            DataDirectories = new List<string>();

            GlobalMaxUpdateRange = 24;
            GlobalUpdateRange = 18;
            GlobalRadarRange = 40;
        }

        public static Action<CrashedEventArgs> CrashedHandler { get; set; }

        public static bool Crashed => _Crashed;

        private static bool _Crashed;
        private static string _BaseDirectory;
        private static string _ExePath;

        private static bool _Cache = true;

        private static bool _Profiling;
        private static DateTime _ProfileStart;
        private static TimeSpan _ProfileTime;

        public static MessagePump MessagePump { get; set; }

        public static Slice Slice;

        public static bool Profiling
        {
            get => _Profiling;
            set
            {
                if (_Profiling == value)
                {
                    return;
                }

                _Profiling = value;

                if (_ProfileStart > DateTime.MinValue)
                {
                    _ProfileTime += DateTime.UtcNow - _ProfileStart;
                }

                _ProfileStart = _Profiling ? DateTime.UtcNow : DateTime.MinValue;
            }
        }

        public static TimeSpan ProfileTime
        {
            get
            {
                if (_ProfileStart > DateTime.MinValue)
                {
                    return _ProfileTime + (DateTime.UtcNow - _ProfileStart);
                }

                return _ProfileTime;
            }
        }

        public static bool Service { get; private set; }

        public static bool NoConsole { get; private set; }
        public static bool Debug { get; private set; }

        public static bool HaltOnWarning { get; private set; }
        public static bool VBdotNet { get; private set; }

        public static List<string> DataDirectories { get; private set; }

        public static Assembly Assembly { get; set; }

        public static Process Process { get; private set; }
        public static Thread Thread { get; private set; }

        public static MultiTextWriter MultiConsoleOut { get; private set; }

        private static readonly long _TickOrigin = Stopwatch.GetTimestamp();

        public static long TickCount => (Stopwatch.GetTimestamp() - _TickOrigin) * 1000L / Stopwatch.Frequency;

        public static readonly bool Is64Bit = Environment.Is64BitProcess;
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly string Framework = RuntimeInformation.FrameworkDescription;

        public static bool MultiProcessor { get; private set; }
        public static int ProcessorCount { get; private set; }

        public static bool Unix { get; private set; }

        public static string FindDataFile(string path)
        {
            if (DataDirectories.Count == 0)
            {
                throw new InvalidOperationException("Attempted to FindDataFile before DataDirectories list has been filled.");
            }

            string fullPath = null;

            for (int index = 0; index < DataDirectories.Count; index++)
            {
                string p = DataDirectories[index];
                fullPath = Path.Combine(p, path);

                if (File.Exists(fullPath))
                {
                    break;
                }

                fullPath = null;
            }

            return fullPath;
        }

        public static string FindDataFile(string format, params object[] args)
        {
            return FindDataFile(string.Format(format, args));
        }

        public static Expansion Expansion => Expansion.EJ;

        public static string ExePath => _ExePath ?? (_ExePath = IsWindows ? Path.ChangeExtension(Assembly.Location, ".exe") : Assembly.Location);

        public static string BaseDirectory
        {
            get
            {
                if (_BaseDirectory == null)
                {
                    try
                    {
                        _BaseDirectory = ExePath;

                        if (_BaseDirectory.Length > 0)
                        {
                            _BaseDirectory = Path.GetDirectoryName(_BaseDirectory);
                        }
                    }
                    catch
                    {
                        _BaseDirectory = "";
                    }
                }

                return _BaseDirectory;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.IsTerminating ? "Error:" : "Warning:");
            Console.WriteLine(e.ExceptionObject);

            if (e.IsTerminating)
            {
                _Crashed = true;

                bool close = false;

                CrashedEventArgs args = new CrashedEventArgs(e.ExceptionObject as Exception);

                try
                {
                    EventSink.InvokeCrashed(args);
                    close = args.Close;
                }
                catch (Exception ex)
                {
                    Diagnostics.ExceptionLogging.LogException(ex);
                }

                if (CrashedHandler != null)
                {
                    try
                    {
                        CrashedHandler(args);
                        close = args.Close;
                    }
                    catch (Exception ex)
                    {
                        Diagnostics.ExceptionLogging.LogException(ex);
                    }
                }

                if (!close && !Service)
                {
                    try
                    {
                        for (int index = 0; index < MessagePump.Listeners.Length; index++)
                        {
                            Listener l = MessagePump.Listeners[index];
                            l.Dispose();
                        }
                    }
                    catch
                    {
                    }

                    Console.WriteLine("This exception is fatal, press return to exit");
                    Console.ReadLine();
                }

                Kill();
            }
        }

        private enum ConsoleEventType
        {
            CTRL_C_EVENT,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private delegate bool ConsoleEventHandler(ConsoleEventType type);

        private static ConsoleEventHandler m_ConsoleEventHandler;

        private static class UnsafeNativeMethods
        {
            [DllImport("Kernel32")]
            public static extern bool SetConsoleCtrlHandler(ConsoleEventHandler callback, bool add);
        }

        private static bool OnConsoleEvent(ConsoleEventType type)
        {
            if (World.Saving || Service && type == ConsoleEventType.CTRL_LOGOFF_EVENT)
            {
                return true;
            }

            Kill(); //Kill -> HandleClosed will handle waiting for the completion of flushing to disk

            return true;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            HandleClosed();
        }

        public static bool Closing { get; private set; }

        private static int _CycleIndex = -1;
        private static readonly double[] _CyclesPerSecond = new double[100];

        public static double CyclesPerSecond => _CycleIndex >= 0 ? _CyclesPerSecond[_CycleIndex] : 0;

        public static double AverageCPS
        {
            get
            {
                double t = 0.0d;
                int c = 0;

                for (int i = 0; i < _CycleIndex && i < _CyclesPerSecond.Length; ++i)
                {
                    t += _CyclesPerSecond[i];
                    ++c;
                }

                return t / Math.Max(c, 1);
            }
        }

        public static void Kill()
        {
            Kill(false);
        }

        public static void Kill(bool restart)
        {
            HandleClosed();

            if (restart)
            {
                Process.Start(ExePath, Arguments);
            }

            Process.Kill();
        }

        private static void HandleClosed()
        {
            if (Closing)
            {
                return;
            }

            Closing = true;

            if (Debug)
            {
                Console.Write("Exiting...");
            }

            World.WaitForWriteCompletion();

            if (!_Crashed)
            {
                EventSink.InvokeShutdown(new ShutdownEventArgs());
            }

            if (Debug)
            {
                Console.WriteLine("done");
            }
        }

        private static readonly AutoResetEvent _Signal = new AutoResetEvent(true);

        public static void Set()
        {
            _Signal.Set();
        }

        public static void Setup(IEnumerable<string> args)
        {
#if DEBUG
            Debug = true;
#endif
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            foreach (string a in args)
            {
                if (Insensitive.Equals(a, "-debug"))
                {
                    Debug = true;
                }
                else if (Insensitive.Equals(a, "-service"))
                {
                    Service = true;
                }
                else if (Insensitive.Equals(a, "-profile"))
                {
                    Profiling = true;
                }
                else if (Insensitive.Equals(a, "-nocache"))
                {
                    _Cache = false;
                }
                else if (Insensitive.Equals(a, "-haltonwarning"))
                {
                    HaltOnWarning = true;
                }
                else if (Insensitive.Equals(a, "-vb"))
                {
                    VBdotNet = true;
                }
                else if (Insensitive.Equals(a, "-noconsole"))
                {
                    NoConsole = true;
                }
                else if (Insensitive.Equals(a, "-h") || Insensitive.Equals(a, "-help"))
                {
                    Console.WriteLine("An Ultima Online server emulator written in C# - Visit https://github.com/TrueUO/TrueUO for more information.\n\n");
                    Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + " [Parameter]\n\n");
                    Console.WriteLine("     -debug              Starting TrueUO in Debug Mode. Debug Mode is being used in Core and Scripts to give extended information during runtime.");
                    Console.WriteLine("     -haltonwarning      TrueUO halts if any warning is raised during compilation of scripts.");
                    Console.WriteLine("     -h or -help         Displays this help text.");
                    Console.WriteLine("     -nocache            No known effect.");
                    Console.WriteLine("     -noconsole          No user interaction during startup and runtime.");
                    Console.WriteLine("     -profile            Enables profiling allowing to get performance diagnostic information of packets, timers etc. in AdminGump -> Maintenance. Use with caution. This increases server load.");
                    Console.WriteLine("     -service            This parameter should be set if you're running TrueUO as a Windows Service. No user interaction. *Windows only*");
                    Console.WriteLine("     -usehrt             Enables High Resolution Timing if requirements are met. Increasing the resolution of the timer. *Windows only*");
                    Console.WriteLine("     -vb                 Enables compilation of VB.NET Scripts. Without this option VB.NET Scripts are skipped.");

                    Environment.Exit(0);
                }
            }

            if (!Environment.UserInteractive || Service)
            {
                NoConsole = true;
            }

            try
            {
                if (Service)
                {
                    if (!Directory.Exists("Logs"))
                    {
                        Directory.CreateDirectory("Logs");
                    }

                    Console.SetOut(MultiConsoleOut = new MultiTextWriter(new FileLogger("Logs/Console.log")));
                }
                else
                {
                    Console.SetOut(MultiConsoleOut = new MultiTextWriter(Console.Out));
                }
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }

            Thread = Thread.CurrentThread;
            Process = Process.GetCurrentProcess();
            Assembly = Assembly.GetEntryAssembly();

            if (Thread != null)
            {
                Thread.Name = "Core Thread";
            }

            if (BaseDirectory.Length > 0)
            {
                Directory.SetCurrentDirectory(BaseDirectory);
            }

            Version ver = Assembly.GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(ver.Build).AddSeconds(ver.Revision * 2);

            Utility.PushColor(ConsoleColor.Cyan);
#if DEBUG
            Console.WriteLine(
                "TrueUO - [https://github.com/TrueUO/TrueUO] Version {0}.{1}, Build {2}.{3} - Build on {4} UTC - Debug",
                ver.Major,
                ver.Minor,
                ver.Build,
                ver.Revision,
                buildDate);
#else
			Console.WriteLine(
				"TrueUO - [https://github.com/TrueUO/TrueUO] Version {0}.{1}, Build {2}.{3} - Build on {4} UTC - Release",
				ver.Major,
				ver.Minor,
				ver.Build,
				ver.Revision,
				buildDate);
#endif
            Utility.PopColor();

            string s = Arguments;

            if (s.Length > 0)
            {
                Utility.PushColor(ConsoleColor.Yellow);
                Console.WriteLine("Core: Running with arguments: {0}", s);
                Utility.PopColor();
            }

            ProcessorCount = Environment.ProcessorCount;

            if (ProcessorCount > 1)
            {
                MultiProcessor = true;
            }

            if (MultiProcessor)
            {
                Utility.PushColor(ConsoleColor.Green);
                Console.WriteLine($"Core: Optimizing for {ProcessorCount} processor{(ProcessorCount == 1 ? "" : "s")}");
                Utility.PopColor();
            }

            if (IsWindows)
            {
                m_ConsoleEventHandler = OnConsoleEvent;
                UnsafeNativeMethods.SetConsoleCtrlHandler(m_ConsoleEventHandler, true);
            }

            Utility.PushColor(ConsoleColor.Green);
            Console.WriteLine($"Core: Running on {Framework}");
            Utility.PopColor();

            if (GCSettings.IsServerGC)
            {
                Utility.PushColor(ConsoleColor.Green);
                Console.WriteLine("Core: Server garbage collection mode enabled");
                Utility.PopColor();
            }

            Utility.PushColor(ConsoleColor.DarkYellow);
            Console.WriteLine("Core: High resolution timing ({0})", Stopwatch.IsHighResolution ? "Supported" : "Unsupported");
            Utility.PopColor();

            Utility.PushColor(ConsoleColor.Green);
            Console.WriteLine("Core: Loading config...");
            Config.Load();
            Utility.PopColor();

            while (!ScriptCompiler.Compile(Debug, _Cache))
            {
                Utility.PushColor(ConsoleColor.Red);
                Console.WriteLine("Scripts: One or more scripts failed to compile or no script files were found.");
                Utility.PopColor();

                if (Service)
                {
                    return;
                }

                Console.WriteLine(" - Press return to exit, or R to try again.");

                if (Console.ReadKey(true).Key != ConsoleKey.R)
                {
                    return;
                }
            }

            Timer.Init(TickCount);

            ScriptCompiler.Invoke("Configure");

            Region.Load();
            World.Load();

            ScriptCompiler.Invoke("Initialize");

            MessagePump = new MessagePump();

            for (int index = 0; index < Map.AllMaps.Count; index++)
            {
                TileMatrix.Force();
            }

            NetState.Initialize();
        }

        private static void WaitForInterval(double durationMilliSeconds)
        {
            double durationTicks = Math.Round(durationMilliSeconds * Stopwatch.Frequency / 1000.0); 
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.ElapsedTicks < durationTicks)
            {
                if (durationMilliSeconds > 20)
                {
                    _Signal.WaitOne((int)durationMilliSeconds);
                }
            }
        }

        public static void Run()
        {
            try
            {
                const int interval = 100;
                const double intervalDurationMs = 1000.0 / interval;
                const int calculationIntervalMilliseconds = 1000;

                int loopCount = 0;

                Stopwatch stopwatch = Stopwatch.StartNew();

                while (!Closing)
                {
                    long last = Stopwatch.GetTimestamp();

                    Mobile.ProcessDeltaQueue();
                    Item.ProcessDeltaQueue();

                    Timer.Slice(TickCount);

                    MessagePump.Slice();

                    NetState.FlushAll();
                    NetState.ProcessDisposedQueue();

                    Slice?.Invoke();

                    double currentThreadDuration = (Stopwatch.GetTimestamp() - last) * (1000.0 / Stopwatch.Frequency);

                    if (currentThreadDuration < intervalDurationMs && (intervalDurationMs - currentThreadDuration) > 0)
                    {
                        WaitForInterval(intervalDurationMs - currentThreadDuration);
                    }

                    loopCount++;

                    double loopFrequency = loopCount / stopwatch.Elapsed.TotalSeconds;

                    if (++_CycleIndex >= _CyclesPerSecond.Length)
                    {
                        _CycleIndex = 0;
                    }

                    _CyclesPerSecond[_CycleIndex] = loopFrequency;

                    if (stopwatch.ElapsedMilliseconds >= calculationIntervalMilliseconds)
                    {
                        loopCount = 0;
                        stopwatch.Restart();
                    }
                }
            }
            catch (Exception e)
            {
                CurrentDomain_UnhandledException(null, new UnhandledExceptionEventArgs(e, true));
            }
        }

        public static string Arguments
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (Debug)
                {
                    Utility.Separate(sb, "-debug", " ");
                }

                if (Service)
                {
                    Utility.Separate(sb, "-service", " ");
                }

                if (Profiling)
                {
                    Utility.Separate(sb, "-profile", " ");
                }

                if (!_Cache)
                {
                    Utility.Separate(sb, "-nocache", " ");
                }

                if (HaltOnWarning)
                {
                    Utility.Separate(sb, "-haltonwarning", " ");
                }

                if (VBdotNet)
                {
                    Utility.Separate(sb, "-vb", " ");
                }

                if (NoConsole)
                {
                    Utility.Separate(sb, "-noconsole", " ");
                }

                return sb.ToString();
            }
        }

        public static int GlobalUpdateRange { get; set; }
        public static int GlobalMaxUpdateRange { get; set; }
        public static int GlobalRadarRange { get; set; }

        private static int m_ItemCount, m_MobileCount, m_CustomsCount;

        public static int ScriptItems => m_ItemCount;
        public static int ScriptMobiles => m_MobileCount;
        public static int ScriptCustoms => m_CustomsCount;

        public static void VerifySerialization()
        {
            m_ItemCount = 0;
            m_MobileCount = 0;
            m_CustomsCount = 0;

            VerifySerialization(Assembly.GetCallingAssembly());

            for (int index = 0; index < ScriptCompiler.Assemblies.Length; index++)
            {
                Assembly a = ScriptCompiler.Assemblies[index];
                VerifySerialization(a);
            }
        }

        private static readonly Type[] m_SerialTypeArray = { typeof(Serial) };

        private static void VerifyType(Type t)
        {
            bool isItem = t.IsSubclassOf(typeof(Item));

            if (isItem || t.IsSubclassOf(typeof(Mobile)))
            {
                if (isItem)
                {
                    Interlocked.Increment(ref m_ItemCount);
                }
                else
                {
                    Interlocked.Increment(ref m_MobileCount);
                }

                StringBuilder warningSb = null;

                try
                {
                    if (t.GetConstructor(m_SerialTypeArray) == null)
                    {
                        warningSb = new StringBuilder();

                        warningSb.AppendLine("       - No serialization constructor");
                    }

                    if (t.GetMethod("Serialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
                    {
                        if (warningSb == null)
                        {
                            warningSb = new StringBuilder();
                        }

                        warningSb.AppendLine("       - No Serialize() method");
                    }

                    if (t.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
                    {
                        if (warningSb == null)
                        {
                            warningSb = new StringBuilder();
                        }

                        warningSb.AppendLine("       - No Deserialize() method");
                    }

                    if (warningSb != null && warningSb.Length > 0)
                    {
                        Utility.PushColor(ConsoleColor.Yellow);
                        Console.WriteLine("Warning: {0}\n{1}", t, warningSb);
                        Utility.PopColor();
                    }
                }
                catch
                {
                    Utility.PushColor(ConsoleColor.Yellow);
                    Console.WriteLine("Warning: Exception in serialization verification of type {0}", t);
                    Utility.PopColor();
                }
            }
        }

        private static void VerifySerialization(Assembly a)
        {
            if (a != null)
            {
                Parallel.ForEach(a.GetTypes(), VerifyType);
            }
        }
    }

    public class FileLogger : TextWriter
    {
        public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

        private bool _NewLine;

        public string FileName { get; private set; }

        public FileLogger(string file)
            : this(file, false)
        { }

        public FileLogger(string file, bool append)
        {
            FileName = file;

            using (StreamWriter writer = new StreamWriter(new FileStream(FileName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                writer.WriteLine(">>>Logging started on {0:f}.", DateTime.Now);
            }

            _NewLine = true;
        }

        public override void Write(char ch)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                if (_NewLine)
                {
                    writer.Write(DateTime.UtcNow.ToString(DateFormat));
                    _NewLine = false;
                }

                writer.Write(ch);
            }
        }

        public override void Write(string str)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                if (_NewLine)
                {
                    writer.Write(DateTime.UtcNow.ToString(DateFormat));
                    _NewLine = false;
                }

                writer.Write(str);
            }
        }

        public override void WriteLine(string line)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                if (_NewLine)
                {
                    writer.Write(DateTime.UtcNow.ToString(DateFormat));
                }

                writer.WriteLine(line);
                _NewLine = true;
            }
        }

        public override Encoding Encoding => Encoding.Default;
    }

    public class MultiTextWriter : TextWriter
    {
        private readonly List<TextWriter> _Streams;

        public MultiTextWriter(params TextWriter[] streams)
        {
            _Streams = new List<TextWriter>(streams);

            if (_Streams.Count < 0)
            {
                throw new ArgumentException("You must specify at least one stream.");
            }
        }

        public void Add(TextWriter tw)
        {
            _Streams.Add(tw);
        }

        public void Remove(TextWriter tw)
        {
            _Streams.Remove(tw);
        }

        public override void Write(char ch)
        {
            for (int index = 0; index < _Streams.Count; index++)
            {
                TextWriter t = _Streams[index];

                t.Write(ch);
            }
        }

        public override void WriteLine(string line)
        {
            for (int index = 0; index < _Streams.Count; index++)
            {
                TextWriter t = _Streams[index];

                t.WriteLine(line);
            }
        }

        public override void WriteLine(string line, params object[] args)
        {
            WriteLine(string.Format(line, args));
        }

        public override Encoding Encoding => Encoding.Default;
    }
}
