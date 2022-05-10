using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Server.Accounting;
using Server.Diagnostics;
using Server.Network;

namespace Server.Misc
{
    internal static class ServerConsole
    {
        private static readonly Func<string> _Listen = Console.ReadLine;

        private static string _Command;

        private static Timer _PollTimer;

        private static bool _HearConsole;

        public static void Initialize()
        {
            EventSink.ServerStarted += () =>
            {
                PollCommands();

                if (_HearConsole)
                {
                    Console.WriteLine("Now listening to the whole shard.");
                }
            };

            EventSink.Speech += args =>
            {
                if (args.Mobile == null || !_HearConsole)
                {
                    return;
                }

                try
                {
                    if (args.Mobile.Region.Name.Length > 0)
                    {
                        Console.WriteLine(args.Mobile.Name + " (" + args.Mobile.Region.Name + "): " + args.Speech);
                    }
                    else
                    {
                        Console.WriteLine("" + args.Mobile.Name + ": " + args.Speech + "");
                    }
                }
                catch (Exception e)
                {
                    ExceptionLogging.LogException(e);
                }
            };
        }

        private static void PollCommands()
        {
            _PollTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromMilliseconds(100), ProcessCommand);

            _Listen.BeginInvoke(r => ProcessInput(_Listen.EndInvoke(r)), null);
        }

        private static void ProcessInput(string input)
        {
            if (!Core.Crashed && !Core.Closing)
            {
                Interlocked.Exchange(ref _Command, input);
            }
        }

        private static void ProcessCommand()
        {
            if (Core.Crashed || Core.Closing || World.Loading || World.Saving)
            {
                return;
            }

            if (string.IsNullOrEmpty(_Command))
            {
                return;
            }

            ProcessCommand(_Command);

            Interlocked.Exchange(ref _Command, string.Empty);

            _Listen.BeginInvoke(r => ProcessInput(_Listen.EndInvoke(r)), null);
        }

        private static void ProcessCommand(string input)
        {
            input = input.Trim();

            if (input.StartsWith("bc", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(2).Trim();

                BroadcastMessage(AccessLevel.Player, 0x35, string.Format("[Admin] {0}", sub));

                Console.WriteLine("[World]: {0}", sub);
                return;
            }

            if (input.StartsWith("sc", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(2).Trim();

                BroadcastMessage(AccessLevel.Counselor, 0x32, string.Format("[Admin] {0}", sub));

                Console.WriteLine("[Staff]: {0}", sub);
                return;
            }

            if (input.StartsWith("ban", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(3).Trim();

                List<NetState> states = NetState.Instances;

                if (states.Count == 0)
                {
                    Console.WriteLine("There are no players online.");
                    return;
                }

                NetState ns = states.Find(o => o.Account != null && o.Mobile != null && Insensitive.StartsWith(sub, o.Mobile.RawName));

                if (ns != null)
                {
                    Console.WriteLine("[Ban]: {0}: Mobile: '{1}' Account: '{2}'", ns, ns.Mobile.RawName, ns.Account.Username);

                    ns.Dispose();
                }

                return;
            }

            if (input.StartsWith("kick", StringComparison.OrdinalIgnoreCase))
            {
                string sub = input.Substring(4).Trim();

                List<NetState> states = NetState.Instances;

                if (states.Count == 0)
                {
                    Console.WriteLine("There are no players online.");
                    return;
                }

                NetState ns = states.Find(o => o.Account != null && o.Mobile != null && Insensitive.StartsWith(sub, o.Mobile.RawName));

                if (ns != null)
                {
                    Console.WriteLine("[Kick]: {0}: Mobile: '{1}' Account: '{2}'", ns, ns.Mobile.RawName, ns.Account.Username);

                    ns.Dispose();
                }

                return;
            }

            switch (input.Trim())
            {
                case "crash":
                    {
                        Timer.DelayCall(() => { throw new Exception("Forced Crash"); });
                    }
                    break;
                case "shutdown":
                    {
                        AutoSave.Save();
                        Core.Kill(false);
                    }
                    break;
                case "shutdown nosave":
                    {
                        Core.Kill(false);
                    }
                    break;
                case "restart":
                    {
                        AutoSave.Save();
                        Core.Kill(true);
                    }
                    break;
                case "save recompile":
                    {
                        var path = AutoRestart.RecompilePath;

                        if (!File.Exists(path))
                        {
                            Console.WriteLine("Unable to Re-Compile due to missing file: {0}", AutoRestart.RecompilePath);
                        }
                        else
                        {
                            AutoSave.Save();

                            Process.Start(path);
                            Core.Kill();
                        }
                    }
                    break;
                case "nosave recompile":
                    {
                        var path = AutoRestart.RecompilePath;

                        if (!File.Exists(path))
                        {
                            Console.WriteLine("Unable to Re-Compile due to missing file: {0}", AutoRestart.RecompilePath);
                        }
                        else
                        {
                            Process.Start(path);
                            Core.Kill();
                        }
                    }
                    break;
                case "restart nosave":
                    {
                        Core.Kill(true);
                    }
                    break;
                case "online":
                    {
                        List<NetState> states = NetState.Instances;

                        if (states.Count == 0)
                        {
                            Console.WriteLine("There are no users online at this time.");
                        }

                        for (var index = 0; index < states.Count; index++)
                        {
                            NetState t = states[index];
                            Account a = t.Account as Account;

                            if (a == null)
                            {
                                continue;
                            }

                            Mobile m = t.Mobile;

                            if (m != null)
                            {
                                Console.WriteLine("- Account: {0}, Name: {1}, IP: {2}", a.Username, m.Name, t);
                            }
                        }
                    }
                    break;
                case "save":
                    AutoSave.Save();
                    break;
                case "hear": // Credit to Zippy for the HearAll script!
                    {
                        _HearConsole = !_HearConsole;

                        Console.WriteLine("{0} sending speech to the console.", _HearConsole ? "Now" : "No longer");
                    }
                    break;
                default:
                    DisplayHelp();
                    break;
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine(" ");
            Console.WriteLine("Commands:");
            Console.WriteLine("crash           - Forces an exception to be thrown.");
            Console.WriteLine("save            - Performs a forced save.");
            Console.WriteLine("shutdown        - Performs a forced save then shuts down the server.");
            Console.WriteLine("shutdown nosave - Shuts down the server without saving.");
            Console.WriteLine("restart         - Sends a message to players informing them that the server is");
            Console.WriteLine("                  restarting, performs a forced save, then shuts down and");
            Console.WriteLine("                  restarts the server.");
            Console.WriteLine("restart nosave  - Restarts the server without saving.");
            Console.WriteLine("online          - Shows a list of every person online:");
            Console.WriteLine("                  Account, Char Name, IP.");
            Console.WriteLine("bc <message>    - Type this command and your message after it.");
            Console.WriteLine("                  It will then be sent to all players.");
            Console.WriteLine("sc <message>    - Type this command and your message after it.");
            Console.WriteLine("                  It will then be sent to all staff.");
            Console.WriteLine("hear            - Copies all local speech to this console:");
            Console.WriteLine("                  Char Name (Region name): Speech.");
            Console.WriteLine("ban <name>      - Kicks and bans the users account.");
            Console.WriteLine("kick <name>     - Kicks the user.");
            Console.WriteLine("help|?          - Shows this list.");
            Console.WriteLine(" ");
        }

        public static void BroadcastMessage(AccessLevel ac, int hue, string message)
        {
            World.Broadcast(hue, false, ac, message);
        }
    }
}
