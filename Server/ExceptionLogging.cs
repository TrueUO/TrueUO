using System;
using System.IO;

namespace Server.Diagnostics
{
    public class ExceptionLogging
    {
        public static string LogDirectory { get; }

        private static StreamWriter _Output;

        public static StreamWriter Output
        {
            get
            {
                if (_Output == null)
                {
					_Output = new StreamWriter(Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToLongDateString()}.log"), true)
					{
						AutoFlush = true
					};

                    _Output.WriteLine("##############################");
                    _Output.WriteLine("Exception log started on {0}", DateTime.UtcNow);
                    _Output.WriteLine();
                }

                return _Output;
            }
        }

        static ExceptionLogging()
        {
			string directory = Path.Combine(Core.BaseDirectory, "Logs/Exceptions");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            LogDirectory = directory;
        }

        public static void LogException(Exception e)
        {
            Utility.WriteConsoleColor(ConsoleColor.Red, "Caught Exception:");
            Utility.WriteConsoleColor(ConsoleColor.DarkRed, e.ToString());

            Output.WriteLine("Exception Caught: {0}", DateTime.UtcNow);
            Output.WriteLine(e);
            Output.WriteLine();
        }

        public static void LogException(Exception e, string arg)
        {
            Utility.WriteConsoleColor(ConsoleColor.Red, "Caught Exception: {0}", arg);
            Utility.WriteConsoleColor(ConsoleColor.DarkRed, e.ToString());

            Output.WriteLine("Exception Caught: {0}", DateTime.UtcNow);
            Output.WriteLine(e);
            Output.WriteLine();
        }
    }
}
