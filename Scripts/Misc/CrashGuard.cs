using Server.Accounting;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;

namespace Server.Misc
{
    public class CrashGuard
    {
        private static readonly bool Enabled = true;
        private static readonly bool SaveBackup = true;
        private static readonly bool RestartServer = true;
        private static readonly bool GenerateReport = true;
        public static void Initialize()
        {
            if (Enabled) // If enabled, register our crash event handler
                EventSink.Crashed += CrashGuard_OnCrash;
        }

        public static void CrashGuard_OnCrash(CrashedEventArgs e)
        {
            if (GenerateReport)
                GenerateCrashReport(e);

            World.WaitForWriteCompletion();

            if (SaveBackup)
                Backup();

            if (RestartServer)
                Restart(e);
        }

        private static void SendEmail(string filePath)
        {
            Console.Write("Crash: Sending email...");

            MailMessage message = new MailMessage(Email.FromAddress, Email.CrashAddresses)
            {
                Subject = "Automated TrueUO Crash Report",

                Body = "Automated TrueUO Crash Report. See attachment for details."
            };

            message.Attachments.Add(new Attachment(filePath));

            if (Email.Send(message))
                Console.WriteLine("done");
            else
                Console.WriteLine("failed");
        }

        private static string GetRoot()
        {
            try
            {
                return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
                return "";
            }
        }

        private static string Combine(string path1, string path2)
        {
            if (path1.Length == 0)
                return path2;

            return Path.Combine(path1, path2);
        }

        private static void Restart(CrashedEventArgs e)
        {
            string root = GetRoot();

            Console.Write("Crash: Restarting...");

            try
            {
                Process.Start(Core.ExePath, Core.Arguments);
                Console.WriteLine("done");

                e.Close = true;
            }
            catch
            {
                Console.WriteLine("failed");
            }
        }

        private static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void CreateDirectory(string path1, string path2)
        {
            CreateDirectory(Combine(path1, path2));
        }

        private static void CopyWildcardFile(string rootOrigin, string rootBackup, string path, string fileName, string fileExtension)
        {
            string originPath = Combine(rootOrigin, path);
            string backupPath = Combine(rootBackup, path);
            string pattern = fileName + "*." + fileExtension;

            String[] files = Directory.GetFiles(originPath, pattern);

            foreach (String file in files)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Copy(file, file.Replace(originPath, backupPath));
                    }

                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }
            }

        }

        private static void CopyFile(string rootOrigin, string rootBackup, string path)
        {
            string originPath = Combine(rootOrigin, path);
            string backupPath = Combine(rootBackup, path);

            try
            {
                if (File.Exists(originPath))
                    File.Copy(originPath, backupPath);
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }
        }

        private static void Backup()
        {
            Console.Write("Crash: Backing up...");

            try
            {
                string timeStamp = GetTimeStamp();

                string root = GetRoot();
                string rootBackup = Combine(root, $"Backups/Crashed/{timeStamp}/");
                string rootOrigin = Combine(root, "Saves/");

                // Create new directories
                CreateDirectory(rootBackup);
                CreateDirectory(rootBackup, "Accounts/");
                CreateDirectory(rootBackup, "Items/");
                CreateDirectory(rootBackup, "Mobiles/");
                CreateDirectory(rootBackup, "Guilds/");
                CreateDirectory(rootBackup, "Regions/");

                // Copy files
                CopyFile(rootOrigin, rootBackup, "Accounts/Accounts.xml");

                CopyWildcardFile(rootOrigin, rootBackup, "Items","Items", "bin");
                CopyWildcardFile(rootOrigin, rootBackup, "Items","Items","idx");
                CopyFile(rootOrigin, rootBackup, "Items/Items.tdb");

                CopyFile(rootOrigin, rootBackup, "Mobiles/Mobiles.bin");
                CopyFile(rootOrigin, rootBackup, "Mobiles/Mobiles.idx");
                CopyFile(rootOrigin, rootBackup, "Mobiles/Mobiles.tdb");

                CopyFile(rootOrigin, rootBackup, "Guilds/Guilds.bin");
                CopyFile(rootOrigin, rootBackup, "Guilds/Guilds.idx");

                CopyFile(rootOrigin, rootBackup, "Regions/Regions.bin");
                CopyFile(rootOrigin, rootBackup, "Regions/Regions.idx");

                Console.WriteLine("done");
            }
            catch
            {
                Console.WriteLine("failed");
            }
        }

        private static void GenerateCrashReport(CrashedEventArgs e)
        {
            Console.Write("Crash: Generating report...");

            try
            {
                string timeStamp = GetTimeStamp();
                string fileName = $"Crash {timeStamp}.log";

                string root = GetRoot();
                string filePath = Combine(root, fileName);

                using (StreamWriter op = new StreamWriter(filePath))
                {
                    Version ver = Core.Assembly.GetName().Version;

                    op.WriteLine("Server Crash Report");
                    op.WriteLine("===================");
                    op.WriteLine();
                    op.WriteLine("TrueUO Version {0}.{1}, Build {2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                    op.WriteLine("Operating System: {0}", Environment.OSVersion);
                    op.WriteLine(".NET Framework: {0}", Environment.Version);
                    op.WriteLine("Time: {0}", DateTime.UtcNow);

                    try
                    {
                        op.WriteLine("Mobiles: {0}", World.Mobiles.Count);
                    }
                    catch (Exception ex)
                    {
                        Diagnostics.ExceptionLogging.LogException(ex);
                    }

                    try
                    {
                        op.WriteLine("Items: {0}", World.Items.Count);
                    }
                    catch (Exception ex)
                    {
                        Diagnostics.ExceptionLogging.LogException(ex);
                    }

                    op.WriteLine("Exception:");
                    op.WriteLine(e.Exception);
                    op.WriteLine();

                    op.WriteLine("Clients:");

                    try
                    {
                        List<NetState> states = NetState.Instances;

                        op.WriteLine("- Count: {0}", states.Count);

                        for (int i = 0; i < states.Count; ++i)
                        {
                            NetState state = states[i];

                            op.Write("+ {0}:", state);

                            if (state.Account is Account a)
                            {
                                op.Write(" (account = {0})", a.Username);
                            }

                            Mobile m = state.Mobile;

                            if (m != null)
                            {
                                op.Write(" (mobile = 0x{0:X} '{1}')", m.Serial.Value, m.Name);
                            }

                            op.WriteLine();
                        }
                    }
                    catch
                    {
                        op.WriteLine("- Failed");
                    }
                }

                Console.WriteLine("done");

                if (Email.FromAddress != null && Email.CrashAddresses != null)
                {
                    SendEmail(filePath);
                }
            }
            catch
            {
                Console.WriteLine("failed");
            }
        }

        private static string GetTimeStamp()
        {
            DateTime now = DateTime.UtcNow;

            return $"{now.Day}-{now.Month}-{now.Year}-{now.Hour}-{now.Minute}-{now.Second}";
        }
    }
}
