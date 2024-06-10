using Server.Commands;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Misc
{
    public static class AutoSave
    {
        private static readonly string[] m_Backups =
        {
            "Third Backup",
            "Second Backup",
            "Most Recent"
        };

        private static readonly TimeSpan m_Delay;
        private static readonly TimeSpan m_Warning;

        private static readonly Timer m_Timer;

        public static bool SavesEnabled { get; set; }

        static AutoSave()
        {
            SavesEnabled = Config.Get("AutoSave.Enabled", true);

            m_Delay = Config.Get("AutoSave.Frequency", TimeSpan.FromMinutes(5.0));
            m_Warning = Config.Get("AutoSave.WarningTime", TimeSpan.Zero);

            m_Timer = Timer.DelayCall(m_Delay - m_Warning, m_Delay, Tick);
            m_Timer.Stop();
        }

        public static void Initialize()
        {
            m_Timer.Start();

            CommandSystem.Register("SetSaves", AccessLevel.Administrator, SetSaves_OnCommand);
        }

        [Usage("SetSaves <true | false>")]
        [Description("Enables or disables automatic shard saving.")]
        public static void SetSaves_OnCommand(CommandEventArgs e)
        {
            if (e.Length == 1)
            {
                SavesEnabled = e.GetBoolean(0);

                e.Mobile.SendMessage("Saves have been {0}.", SavesEnabled ? "enabled" : "disabled");
            }
            else
                e.Mobile.SendMessage("Format: SetSaves <true | false>");
        }

        public static void Save(bool oldSaveStrategy = false)
        {
            if (AutoRestart.Restarting || CreateWorld.WorldCreating)
                return;

            World.WaitForWriteCompletion();

            try
            {
                if (!Backup())
                    Console.WriteLine("WARNING: Automatic backup FAILED");
            }
            catch (Exception e)
            {
                Console.WriteLine("WARNING: Automatic backup FAILED:\n{0}", e);
                Diagnostics.ExceptionLogging.LogException(e);
            }

            World.Save(true, oldSaveStrategy);

            if(oldSaveStrategy)
            {
                BackupCopy();
                World.Save(true);
            }

        }

        private static void Tick()
        {
            if (!SavesEnabled || AutoRestart.Restarting || CreateWorld.WorldCreating)
                return;

            if (m_Warning == TimeSpan.Zero)
                Save();
            else
            {
                int s = (int)m_Warning.TotalSeconds;
                int m = s / 60;
                s %= 60;

                if (m > 0 && s > 0)
                    World.Broadcast(0x35, false, "The world will save in {0} minute{1} and {2} second{3}.", m, m != 1 ? "s" : "", s, s != 1 ? "s" : "");
                else if (m > 0)
                    World.Broadcast(0x35, false, "The world will save in {0} minute{1}.", m, m != 1 ? "s" : "");
                else
                    World.Broadcast(0x35, false, "The world will save in {0} second{1}.", s, s != 1 ? "s" : "");

                Timer.DelayCall(m_Warning, () => Save(true));
            }
        }

        private static bool Backup()
        {
            if (m_Backups.Length == 0)
                return false;

            string root = Path.Combine(Core.BaseDirectory, "Backups/Automatic");

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            string tempRoot = Path.Combine(Core.BaseDirectory, "Backups/Temp");

            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, true);

            string[] existing = Directory.GetDirectories(root);

            bool anySuccess = existing.Length == 0;

            for (int i = 0; i < m_Backups.Length; ++i)
            {
                DirectoryInfo dir = Match(existing, m_Backups[i]);

                if (dir == null)
                {
                    continue;
                }

                if (i > 0)
                {
                    try
                    {
                        dir.MoveTo(Path.Combine(root, m_Backups[i - 1]));

                        anySuccess = true;
                    }
                    catch (Exception e) { Diagnostics.ExceptionLogging.LogException(e); }
                }
                else
                {
                    try
                    {
                        dir.MoveTo(tempRoot);
                    }
                    catch (Exception e) { Diagnostics.ExceptionLogging.LogException(e); }

                    try
                    {
                        dir.Delete(true);
                    }
                    catch (Exception e) { Diagnostics.ExceptionLogging.LogException(e); }
                }
            }

            string saves = Path.Combine(Core.BaseDirectory, "Saves");

            if (Directory.Exists(saves))
                Directory.Move(saves, Path.Combine(root, m_Backups[m_Backups.Length - 1]));

            return anySuccess;
        }

        private static void BackupCopy()
        {
            if (m_Backups.Length == 0)
                return;

            string root = Path.Combine(Core.BaseDirectory, "DailyBackups/Automatic");

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            string tempRoot = Path.Combine(Core.BaseDirectory, "DailyBackups/Temp");

            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, true);

            string[] existing = Directory.GetDirectories(root);

            for (int i = 0; i < m_Backups.Length; ++i)
            {
                DirectoryInfo dir = Match(existing, m_Backups[i]);

                if (dir == null)
                {
                    continue;
                }

                if (i > 0)
                {
                    try
                    {
                        dir.MoveTo(Path.Combine(root, m_Backups[i - 1]));
                    }
                    catch (Exception e) { Diagnostics.ExceptionLogging.LogException(e); }
                }
                else
                {
                    try
                    {
                        dir.MoveTo(tempRoot);
                    }
                    catch (Exception e) { Diagnostics.ExceptionLogging.LogException(e); }

                    try
                    {
                        dir.Delete(true);
                    }
                    catch (Exception e) { Diagnostics.ExceptionLogging.LogException(e); }
                }
            }

            string saves = Path.Combine(Core.BaseDirectory, "Saves");

            if (Directory.Exists(saves))
                Directory.Move(saves, Path.Combine(root, m_Backups[m_Backups.Length - 1]));
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static DirectoryInfo Match(IReadOnlyList<string> paths, string match)
        {
            for (int i = 0; i < paths.Count; ++i)
            {
                DirectoryInfo info = new DirectoryInfo(paths[i]);

                if (info.Name.StartsWith(match))
                    return info;
            }

            return null;
        }
    }
}
