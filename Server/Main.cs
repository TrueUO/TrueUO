#region References
using Server.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

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
            get { return _Profiling; }
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

                _ProfileStart = (_Profiling ? DateTime.UtcNow : DateTime.MinValue);
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

        public static Version Version => Assembly.GetName().Version;

        public static Process Process { get; private set; }
        public static Thread Thread { get; private set; }

        public static MultiTextWriter MultiConsoleOut { get; private set; }

        /* In this game engine, time is divided into fixed-size, discrete intervals often called
		 * 'ticks'. A tick constitutes one loop through the main engine loop (see the Run()
		 * function). Below, define a target for how long a game engine tick should be in milliseconds.
		 * A server running at 40Hz, for instance, has 25ms ticks. This is just a request - the actual
		 * number will be a power of two in hardware ticks. */
		private static readonly int REQUESTED_MILLISECONDS_PER_TICK = 25;

        /* Hardware provides time in units also called ticks, but these ticks are different for each
		 * system. Calculate the number of hardware ticks in a millisecond. */
		public static readonly long HW_TICKS_PER_MILLISECOND = Stopwatch.Frequency / 1000;

        /* Figure out how many hardware ticks are in the requested game engine tick size, then round
		 * to the nearest power of two less than that value. This will be the smallest resolution of
		 * time kept in the game engine. A power of two is used because this value is often used
		 * as a divisor in other calculations. Dividing in general is slow, but dividing by a power
		 * of two is fast. */
		public static readonly int HW_TICKS_PER_ENGINE_TICK_POW_2 = (int)Math.Log(REQUESTED_MILLISECONDS_PER_TICK * HW_TICKS_PER_MILLISECOND, 2);
		public static readonly long HW_TICKS_PER_ENGINE_TICK = 1 << HW_TICKS_PER_ENGINE_TICK_POW_2;

        /* Calculate the actual number of milliseconds per game engine tick, after rounding the hardware
		 * ticks to the nearest power of two. */
		public static readonly double MILLISECONDS_PER_ENGINE_TICK = (double)HW_TICKS_PER_ENGINE_TICK / HW_TICKS_PER_MILLISECOND;

        /* Cached value of the current time, in units of timer ticks.
		 * This value is updated each time the main game engine loops around. */
		private static long m_Now = Stopwatch.GetTimestamp() >> HW_TICKS_PER_ENGINE_TICK_POW_2;
		
		/* Return the number of game engine ticks elapsed since the last
		 * system reboot. */
		public static long Now { get { return m_Now; } }

        public static long TickCount { get { return (long)(Now * MILLISECONDS_PER_ENGINE_TICK); } }

        public static readonly bool Is64Bit = Environment.Is64BitProcess;

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

            foreach (string p in DataDirectories)
            {
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
            return FindDataFile(String.Format(format, args));
        }

        #region Expansions
        public static Expansion Expansion => Expansion.EJ;

        public static bool T2A => Expansion >= Expansion.T2A;
        public static bool UOR => Expansion >= Expansion.UOR;
        public static bool UOTD => Expansion >= Expansion.UOTD;
        public static bool LBR => Expansion >= Expansion.LBR;
        public static bool AOS => Expansion >= Expansion.AOS;
        public static bool SE => Expansion >= Expansion.SE;
        public static bool ML => Expansion >= Expansion.ML;
        public static bool SA => Expansion >= Expansion.SA;
        public static bool HS => Expansion >= Expansion.HS;
        public static bool TOL => Expansion >= Expansion.TOL;
        public static bool EJ => Expansion >= Expansion.EJ;
        #endregion

        public static string ExePath => _ExePath ?? (_ExePath = Assembly.Location);

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
                    Server.Diagnostics.ExceptionLogging.LogException(ex);
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
                        Server.Diagnostics.ExceptionLogging.LogException(ex);
                    }
                }

                if (!close && !Service)
                {
                    try
                    {
                        foreach (Listener l in MessagePump.Listeners)
                        {
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
            if (World.Saving || (Service && type == ConsoleEventType.CTRL_LOGOFF_EVENT))
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
                Console.Write("Exiting...");

            World.WaitForWriteCompletion();

            if (!_Crashed)
            {
                EventSink.InvokeShutdown(new ShutdownEventArgs());
            }

            if (Debug)
                Console.WriteLine("done");
        }

        public static void Main(string[] args)
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
                    Console.WriteLine("An Ultima Online server emulator written in C# - Visit https://www.servuo.com for more information.\n\n");
                    Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName + " [Parameter]\n\n");
                    Console.WriteLine("     -debug              Starting ServUO in Debug Mode. Debug Mode is being used in Core and Scripts to give extended inforamtion during runtime.");
                    Console.WriteLine("     -haltonwarning      ServUO halts if any warning is raised during compilation of scripts.");
                    Console.WriteLine("     -h or -help         Displays this help text.");
                    Console.WriteLine("     -nocache            No known effect.");
                    Console.WriteLine("     -noconsole          No user interaction during startup and runtime.");
                    Console.WriteLine("     -profile            Enables profiling allowing to get performance diagnostic information of packets, timers etc. in AdminGump -> Maintenance. Use with caution. This increases server load.");
                    Console.WriteLine("     -service            This parameter should be set if you're running ServUO as a Windows Service. No user interaction. *Windows only*");
                    Console.WriteLine("     -usehrt             Enables High Resolution Timing if requirements are met. Increasing the resolution of the timer. *Windows only*");
                    Console.WriteLine("     -vb                 Enables compilation of VB.NET Scripts. Without this option VB.NET Scripts are skipped.");

                    System.Environment.Exit(0);
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
                Server.Diagnostics.ExceptionLogging.LogException(e);
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
                "ServUO - [https://www.servuo.com] Version {0}.{1}, Build {2}.{3} - Build on {4} UTC - Debug",
                ver.Major,
                ver.Minor,
                ver.Build,
                ver.Revision,
                buildDate);
#else
            Console.WriteLine(
				"ServUO - [https://www.servuo.com] Version {0}.{1}, Build {2}.{3} - Build on {4} UTC - Release",
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

            if (MultiProcessor || Is64Bit)
            {
                Utility.PushColor(ConsoleColor.Green);
                Console.WriteLine(
                    "Core: Optimizing for {0} {2}processor{1}",
                    ProcessorCount,
                    ProcessorCount == 1 ? "" : "s",
                    Is64Bit ? "64-bit " : "");
                Utility.PopColor();
            }

            string dotnet = null;

            if (Type.GetType("Mono.Runtime") != null)
            {
                MethodInfo displayName = Type.GetType("Mono.Runtime").GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);

                if (displayName != null)
                {
                    dotnet = displayName.Invoke(null, null).ToString();

                    Utility.PushColor(ConsoleColor.Yellow);
                    Console.WriteLine("Core: Unix environment detected");
                    Utility.PopColor();

                    Unix = true;
                }
            }
            else
            {
                m_ConsoleEventHandler = OnConsoleEvent;
                UnsafeNativeMethods.SetConsoleCtrlHandler(m_ConsoleEventHandler, true);
            }

#if NETFX_472
            dotnet = "4.7.2";
#endif

#if NETFX_48
            dotnet = "4.8";
#endif

            if (String.IsNullOrEmpty(dotnet))
                dotnet = "MONO/CSC/Unknown";

            Utility.PushColor(ConsoleColor.Green);
            Console.WriteLine("Core: Compiled for " + (Unix ? "MONO and running on {0}" : ".NET {0}"), dotnet);
            Utility.PopColor();

            if (GCSettings.IsServerGC)
            {
                Utility.PushColor(ConsoleColor.Green);
                Console.WriteLine("Core: Server garbage collection mode enabled");
                Utility.PopColor();
            }

            Utility.PushColor(ConsoleColor.DarkYellow);
            Console.WriteLine("RandomImpl: {0} ({1})", RandomImpl.Type.Name, RandomImpl.IsHardwareRNG ? "Hardware" : "Software");
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

            ScriptCompiler.Invoke("Configure");

            Region.Load();
            World.Load();

            MessagePump = new MessagePump();

            foreach (Map m in Map.AllMaps)
            {
                m.Tiles.Force();
            }

            NetState.Initialize();

            EventSink.InvokeServerStarted();

            Run();
		}

		private static void Run()
		{
			try
			{
				long last = Now;
				long next;

				while (!Closing)
				{
					/* First, get the current time */
					next = Stopwatch.GetTimestamp() >> HW_TICKS_PER_ENGINE_TICK_POW_2;

					/* Figure out how many ticks should have occurred between the last time through
					 * the loop and now. The goal is to execute the loop only once every
					 * MILLISECONDS_PER_ENGINE_TICK. */
					if (next == last)
					{
						/* The loop is currently running fast. Sleep a little while */
						Thread.Sleep((int)MILLISECONDS_PER_ENGINE_TICK / 2);
						continue;
					}

					if (next > (last + 2))
					{
						/* If more than two ticks occurred since we last got here, the server is falling behind. */
						Console.WriteLine("Game Engine Unable to Keep up with Tick Rate");
					}

					last = next;

					while (Now < next)
					{
						/* Advance the current time by 1 tick */
						m_Now++;

						Mobile.ProcessDeltaQueue();
						Item.ProcessDeltaQueue();

						Timer.Slice();
						MessagePump.Slice();

						NetState.FlushAll();
						NetState.ProcessDisposedQueue();

						Slice?.Invoke();
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

            foreach (Assembly a in ScriptCompiler.Assemblies)
            {
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

                    if (
                        t.GetMethod(
                            "Serialize",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
                    {
                        if (warningSb == null)
                        {
                            warningSb = new StringBuilder();
                        }

                        warningSb.AppendLine("       - No Serialize() method");
                    }

                    if (
                        t.GetMethod(
                            "Deserialize",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
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

            using (
                StreamWriter writer =
                    new StreamWriter(
                        new FileStream(FileName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                writer.WriteLine(">>>Logging started on {0:f}.", DateTime.Now);
                //f = Tuesday, April 10, 2001 3:51 PM 
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
            foreach (TextWriter t in _Streams)
            {
                t.Write(ch);
            }
        }

        public override void WriteLine(string line)
        {
            foreach (TextWriter t in _Streams)
            {
                t.WriteLine(line);
            }
        }

        public override void WriteLine(string line, params object[] args)
        {
            WriteLine(String.Format(line, args));
        }

        public override Encoding Encoding => Encoding.Default;
    }
}
