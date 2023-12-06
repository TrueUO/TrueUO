using Server.Gumps;
using System;
using System.Collections;

namespace Server.Mobiles
{
    public class XmlSpawnerDefaults
    {
        public class DefaultEntry
        {
            public string AccountName;
            public string PlayerName;
            public TimeSpan MinDelay = TimeSpan.FromMinutes(5);
            public TimeSpan MaxDelay = TimeSpan.FromMinutes(10);
            public TimeSpan RefractMin = TimeSpan.FromMinutes(0);
            public TimeSpan RefractMax = TimeSpan.FromMinutes(0);
            public TimeSpan TODStart = TimeSpan.FromMinutes(0);
            public TimeSpan TODEnd = TimeSpan.FromMinutes(0);
            public TimeSpan Duration = TimeSpan.FromMinutes(0);
            public TimeSpan DespawnTime = TimeSpan.FromHours(0);
            public bool Group;
            public int Team;
            public int ProximitySound = 0x1F4;
            public string SpeechTrigger;
            public string SkillTrigger;
            public int SequentialSpawn = -1;
            public bool HomeRangeIsRelative = true;
            public int SpawnRange = 5;
            public int HomeRange = 5;
            public int ProximityRange = -1;
            public XmlSpawner.TODModeType TODMode = XmlSpawner.TODModeType.Realtime;
            public int KillReset = 1;
            public string SpawnerName = "Spawner";
            public bool AllowGhostTrig;
            public bool AllowNPCTrig;
            public bool SpawnOnTrigger;
            public bool SmartSpawning;
            public bool ExternalTriggering;
            public string TriggerOnCarried;
            public string NoTriggerOnCarried;
            public string ProximityMsg;
            public double TriggerProbability = 1;
            public string PlayerTriggerProp;
            public string TriggerObjectProp;
            public string DefsExt;
            public string[] NameList;
            public bool[] SelectionList;
            public int AddGumpX = 440;
            public int AddGumpY;
            public int SpawnerGumpX;
            public int SpawnerGumpY;
            public int FindGumpX;
            public int FindGumpY;

            // these are additional defaults that are not set by XmlAdd but can be used by other routines such as the custom properties gump to determine 
            // whether properties have been changed from spawner default values
            public bool Running = true;

            public bool AutoNumber;
            public int AutoNumberValue;

            public XmlAddCAGCategory CurrentCategory;
            public int CurrentCategoryPage;
            public int CategorySelectionIndex = -1;

            public XmlSpawner LastSpawner;
            public Map StartingMap;
            public Point3D StartingLoc;
            public bool ShowExtension;

            public bool IgnoreUpdate;
        }
        public static ArrayList DefaultEntryList;

        public static DefaultEntry GetDefaults(string account, string name)
        {
            // find the default entry corresponding to the account and username
            if (DefaultEntryList != null)
            {
                for (int i = 0; i < DefaultEntryList.Count; i++)
                {
                    DefaultEntry entry = (DefaultEntry)DefaultEntryList[i];
                    if (entry != null && string.Compare(entry.PlayerName, name, true) == 0 && string.Compare(entry.AccountName, account, true) == 0)
                    {
                        return entry;
                    }
                }
            }
            // if not found then add one
            DefaultEntry newentry = new DefaultEntry
            {
                PlayerName = name,
                AccountName = account
            };
            if (DefaultEntryList == null)
                DefaultEntryList = new ArrayList();
            DefaultEntryList.Add(newentry);
            return newentry;
        }
    }
}
