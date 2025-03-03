using Server.Commands;
using Server.Engines.Quests;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Server.Regions;

/* This script has a purpose, and please adhere to the advice before adding versions.
 * This is used for modifying, removing, adding existing spawners, etc for existing shards,
 * used for modifying, removing, adding existing spawners, etc for existing shards.
 * As this is a collaborative effort for ServUO, it's important that any modifications to 
 * existing shards be handled for new shards.  For example, if your swapping out some spawners,
 * common practice will be to edit the spawner files for fresh-loaded servers. Please refer to
 * ServUO.com community with any questions or concerns.
 */

namespace Server
{
    public class SpawnerPersistence
    {
        [Flags]
        public enum SpawnerVersion
        {
            None = 0x00000000,
            Initial = 0x00000001,
            Sphinx = 0x00000002,
            IceHoundRemoval = 0x00000004,
            PaladinAndKrakin = 0x00000008,
            TrinsicPaladins = 0x00000010,
            HonestyItems = 0x00000020,
            TramKhaldun = 0x00000040,
            FixAddonDeco = 0x00000080,
            LifeStealers = 0x00000100,
            LootNerf2 = 0x00000200,
            RemoveUnused = 0x00000400,
            RemoveUnused2 = 0x00000800,
            RemoveTeleporters = 0x00001000,
            DestardSpawners = 0x00002000,
            DoomSpawners    = 0x00004000
        }

        public static string FilePath = Path.Combine("Saves/Misc", "SpawnerPresistence.bin");

        private static bool _FirstRun = true;

        private static int _Version;
        public static int Version => _Version;

        public static SpawnerVersion VersionFlag { get; set; }

        private static bool _SpawnsConverted;
        public static bool SpawnsConverted => _SpawnsConverted;

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        public static void Initialize()
        {
            if (!_FirstRun)
            {
                CheckVersion();
            }
            else if (_Version == 0) // new server, no need to run the new stuff.
            {
                // This way, fresh servers won't duplicate any spawners that should have already been adjusted for a fresh server
                foreach (int i in Enum.GetValues(typeof(SpawnerVersion)))
                {
                    if (i == 0x00000000)
                        continue;

                    VersionFlag |= (SpawnerVersion)i;
                }
            }

            #region Commands
            CommandSystem.Register("ConvertSpawners", AccessLevel.Administrator, e =>
            {
                string str = "By selecting OK, you will wipe all XmlSpawners that were placed via World Load, and will replace " +
                             "with standard spawners. Any existing spawner with special symbols, such as , <> / will not be converted. " +
                             "Be advised, this process will take several minutes to complete.";

                if (_SpawnsConverted)
                    str += "<br><br>You have already ran this conversion. Run Again?";

                e.Mobile.SendGump(new WarningGump(1019005, 30720, str, 0xFFFFFF, 400, 300, (from, ok, state) =>
                {
                    if (ok)
                    {
                        from.SendMessage("Stand by while spawners are converted. This may take a few minutes...");
                        Timer.DelayCall(ConvertXmlToSpawners);
                    }
                }, null, true));
            });

            CommandSystem.Register("RevertXmlSpawners", AccessLevel.Administrator, e =>
                {
                    string str = "By selecting OK, you will wipe all XmlSpawners that were left over from conversion to " +
                                 "standard spawners. All standard spawners will be deleted, and xmlspawners will be re-added. " +
                                 "Be advised, this process will take several minutes to complete.";

                    e.Mobile.SendGump(new WarningGump(1019005, 30720, str, 0xFFFFFF, 400, 300, (from, ok, state) =>
                    {
                        if (ok)
                        {
                            from.SendMessage("Stand by while spawners are converted back to XmlSpawners. This may take a few minutes...");
                            Timer.DelayCall(() => RevertToXmlSpawners(e.Mobile));
                        }
                    }, null, true));
                });

            CommandSystem.Register("WipeAllXmlSpawners", AccessLevel.Administrator, e =>
                {
                    WipeSpawnersFromFile();
                });
            #endregion
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                FilePath,
                writer =>
                {
                    writer.Write(12);

                    writer.Write((int)VersionFlag);

                    writer.Write(false);
                    writer.Write(_SpawnsConverted);
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                    FilePath,
                    reader =>
                    {
                        _Version = reader.ReadInt();

                        if (_Version > 10)
                            VersionFlag = (SpawnerVersion)reader.ReadInt();

                        if (_Version > 2)
                        {
                            _FirstRun = reader.ReadBool();
                            _SpawnsConverted = reader.ReadBool();
                        }
                    });
        }

        /// <summary>
        /// Checks version, and calls code appropriately.  Version 10 implements SpawnerFlag so servers don't miss out and skip versions.
        /// After this point, there is no need to increase version anymore unless any changes 
        /// </summary>
        public static void CheckVersion()
        {
            switch (_Version)
            {
                case 12:
                case 11:
                    if ((VersionFlag & SpawnerVersion.DoomSpawners) == 0)
                    {
                        UpdateDoomSpawners();
                        VersionFlag |= SpawnerVersion.DoomSpawners;
                    }

                    if ((VersionFlag & SpawnerVersion.DestardSpawners) == 0)
                    {
                        UpdateDestardSpawners();
                        VersionFlag |= SpawnerVersion.DestardSpawners;
                    }

                    if ((VersionFlag & SpawnerVersion.RemoveTeleporters) == 0)
                    {
                        RemoveTeleporters();
                        VersionFlag |= SpawnerVersion.RemoveTeleporters;
                    }

                    if ((VersionFlag & SpawnerVersion.RemoveUnused2) == 0)
                    {
                        RemoveUnused2();
                        VersionFlag |= SpawnerVersion.RemoveUnused2;
                    }

                    if ((VersionFlag & SpawnerVersion.RemoveUnused) == 0)
                    {
                        RemoveUnused();
                        VersionFlag |= SpawnerVersion.RemoveUnused;
                    }

                    if ((VersionFlag & SpawnerVersion.LootNerf2) == 0)
                    {
                        LootNerf2();
                        VersionFlag |= SpawnerVersion.LootNerf2;
                    }

                    if ((VersionFlag & SpawnerVersion.LifeStealers) == 0)
                    {
                        SpawnLifeStealers();
                        VersionFlag |= SpawnerVersion.LifeStealers;
                    }

                    if ((VersionFlag & SpawnerVersion.FixAddonDeco) == 0)
                    {
                        FixAddonDeco();
                        VersionFlag |= SpawnerVersion.FixAddonDeco;
                    }

                    if ((VersionFlag & SpawnerVersion.TramKhaldun) == 0)
                    {
                        GenerateTramKhaldun();
                        VersionFlag |= SpawnerVersion.TramKhaldun;
                    }

                    if ((VersionFlag & SpawnerVersion.HonestyItems) == 0)
                    {
                        ConvertHonestyItems();
                        VersionFlag |= SpawnerVersion.HonestyItems;
                    }

                    if ((VersionFlag & SpawnerVersion.TrinsicPaladins) == 0)
                    {
                        SpawnTrinsicPaladins();
                        VersionFlag |= SpawnerVersion.TrinsicPaladins;
                    }

                    if ((VersionFlag & SpawnerVersion.PaladinAndKrakin) == 0)
                    {
                        RemovePaladinsAndKrakens();
                        VersionFlag |= SpawnerVersion.PaladinAndKrakin;
                    }

                    if ((VersionFlag & SpawnerVersion.IceHoundRemoval) == 0)
                    {
                        RemoveIceHounds();
                        VersionFlag |= SpawnerVersion.IceHoundRemoval;
                    }

                    if ((VersionFlag & SpawnerVersion.Sphinx) == 0)
                    {
                        AddSphinx();
                        VersionFlag |= SpawnerVersion.Sphinx;
                    }
                    goto case 10;
                case 10:
                    if ((VersionFlag & SpawnerVersion.Initial) == 0)
                        VersionFlag |= SpawnerVersion.Initial;
                    break;
                case 9:
                    LoadFromXmlSpawner("Spawns/twistedweald.xml", Map.Ilshenar, "TwistedWealdTrigger1");
                    LoadFromXmlSpawner("Spawns/twistedweald.xml", Map.Ilshenar, "TwistedWealdTrigger2");
                    LoadFromXmlSpawner("Spawns/twistedweald.xml", Map.Ilshenar, "TwistedWealdTrigger3");
                    LoadFromXmlSpawner("Spawns/twistedweald.xml", Map.Ilshenar, "TwistedWealdTrigger4");
                    ReplaceUnderworldVersion9();
                    break;
                case 8:
                    ReplaceSolenHivesVersion8();
                    break;
                case 7:
                case 6:
                    ReplaceTwistedWealdVersion7();
                    RunicReforging.ItemNerfVersion6();
                    break;
                case 5:
                    HonestyItemsVersion5();
                    break;
                case 4:
                    BrigandsVersion4();
                    break;
                case 3:
                    break;
                case 2: // Nothing
                    break;
                case 1:
                    RemoveSpawnVersion1();
                    break;
                case 0:
                    CheckSmartSpawn(typeof(BaseVendor), true);
                    CheckQuestQuesters();
                    break;
            }
        }

        public static void ToConsole(string str, ConsoleColor color = ConsoleColor.Green)
        {
            Utility.PushColor(color);
            Console.WriteLine("[Spawner Persistence v{0}] {1}", _Version.ToString(), str);
            Utility.PopColor();
        }

        #region Update Doom Spawners
        public static void UpdateDoomSpawners()
        {
            ReplaceSpawnersByRegionName("Doom", Map.Malas, "doom");
        }
        #endregion

        #region Update Destard Spawners
        public static void UpdateDestardSpawners()
        {
            ReplaceSpawnersByRegionName("Destard", Map.Trammel, "Destard");
            ReplaceSpawnersByRegionName("Destard", Map.Felucca, "Destard");
        }
    #endregion

        #region Remove Teleporters
    public static void RemoveTeleporters()
        {
            WeakEntityCollection.Delete("tel");
            var delCount = 0;

            Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
            {
                IPooledEnumerable eable;

                for (var index = 0; index < Region.Regions.Count; index++)
                {
                    Region region = Region.Regions[index];

                    if (region is TeleportRegion reg)
                    {
                        for (var i = 0; i < reg.Area.Length; i++)
                        {
                            var rec = reg.Area[i];

                            eable = reg.Map.GetItemsInBounds(new Rectangle2D(rec.Start.X, rec.Start.Y, rec.Width, rec.Height));

                            foreach (object o in eable)
                            {
                                if (o is Teleporter tele)
                                {
                                    delCount++;
                                    tele.Delete();
                                }
                            }
                        }
                    }
                }

                ToConsole($"{delCount} additional Teleporters deleted.");
            });
        }
        #endregion

        #region Remove Unused 2
        public static void RemoveUnused2()
        {
            Remove("xmlquestnpc");
            Remove("HiddenFigure");
            Remove("JedahEntille");
            Remove("EnshroudedFigure");
            Remove("MilitiaFighter");
            Remove("Seekerofadventure");
            Remove("Noble");
            Remove("peasant");
            Remove("orderguard");
            Remove("Chaosguard");

            Remove("bridegroom");
            Remove("merchant");
            Remove("baseescortable");

        }
        #endregion

        #region Remove Unused
        public static void RemoveUnused()
        {
            Remove("Emino");
            Remove("FierceDragon");
            Remove("HaochisGuardsman");
            Remove("Mardoth");
            Remove("DeadlyImp");
            Remove("Relnia");
            Remove("Zoel");
            Remove("Horus");
            Remove("Haochi");
        }
        #endregion

        #region Loot Nerf 2
        public static void LootNerf2()
        {
            RunicReforging.LootNerf2();
        }
        #endregion

        #region Spawn Lifestealers
        public static void SpawnLifeStealers()
        {
            LoadFromXmlSpawner("Spawns/termur.xml", Map.TerMur, "LifeStealer");
        }
        #endregion

        #region Addon Decoraction Fix
        public static void FixAddonDeco()
        {
            Type t = typeof(AddonComponent);

            Decorate.GenerateRestricted("deco", "Data/Decoration/Britannia", t, true, Map.Trammel, Map.Felucca);
            Decorate.GenerateRestricted("deco", "Data/Decoration/Trammel", t, true, Map.Trammel);
            Decorate.GenerateRestricted("deco", "Data/Decoration/Felucca", t, true, Map.Felucca);
            Decorate.GenerateRestricted("deco", "Data/Decoration/Ilshenar", t, true, Map.Ilshenar);
            Decorate.GenerateRestricted("deco", "Data/Decoration/Malas", t, true, Map.Malas);
            Decorate.GenerateRestricted("deco", "Data/Decoration/Tokuno", t, true, Map.Tokuno);
        }
        #endregion

        #region Tram Khaldun Generation
        public static void GenerateTramKhaldun()
        {
            Region region = null;

            for (var index = 0; index < Region.Regions.Count; index++)
            {
                var r = Region.Regions[index];

                if (r.Map == Map.Felucca && r.Name == "Khaldun")
                {
                    region = r;
                    break;
                }
            }

            if (region != null)
            {
                int spawners = 0;
                int teleporters = 0;

                foreach (Item item in region.GetEnumeratedItems())
                {
                    if (item is XmlSpawner spawner)
                    {
                        CopyAndPlaceItem(spawner, spawner.Location, Map.Trammel);
                        spawners++;
                    }
                }

                foreach (Item item in region.GetEnumeratedItems())
                {
                    if (item is Teleporter teleporter)
                    {
                        CopyAndPlaceItem(teleporter, teleporter.Location, Map.Trammel);
                        teleporters++;
                    }
                }

                ToConsole($"Copied {spawners} khaldun spawners, {teleporters} teleporters and placed in trammel!");
            }
            else
            {
                ToConsole("No region -Khaldun- Found!", ConsoleColor.Red);
            }

            Decorate.GenerateFromFile("deco", Path.Combine("Data/Decoration/Trammel", "khaldun.cfg"), Map.Trammel);

            KhaldunEntranceAddon entAddon = new KhaldunEntranceAddon();
            entAddon.MoveToWorld(new Point3D(6013, 3785, 18), Map.Trammel);

            KhaldunCampAddon campAddon = new KhaldunCampAddon();
            campAddon.MoveToWorld(new Point3D(6003, 3772, 24), Map.Trammel);

            KhaldunWorkshop workshop = new KhaldunWorkshop();
            workshop.MoveToWorld(new Point3D(6020, 3747, 18), Map.Trammel);

            Teleporter tele = new Teleporter(new Point3D(5571, 1299, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(6011, 3787, 23), Map.Trammel);

            tele = new Teleporter(new Point3D(5571, 1299, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(6012, 3787, 23), Map.Trammel);

            tele = new Teleporter(new Point3D(5572, 1299, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(6013, 3787, 23), Map.Trammel);

            tele = new Teleporter(new Point3D(5572, 1299, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(6014, 3787, 23), Map.Trammel);
        }
        #endregion

        #region Honesty Item Conversion
        public static void ConvertHonestyItems()
        {
            int convert = 0;

            foreach (Item item in World.Items.Values)
            {
                if (item.HonestyItem)
                {
                    if (!item.HasSocket<HonestyItemSocket>())
                    {
                        item.AttachSocket(new HonestyItemSocket());
                        convert++;
                    }
                }
            }

            ToConsole(string.Format("Converted {0} honesty items and attached Honesty Item Socket!", convert));
        }
        #endregion

        #region Trinny Paladins
        public static void SpawnTrinsicPaladins()
        {
            LoadFromXmlSpawner("Spawns/trammel.xml", Map.Trammel, "TrinsicPaladinSpawner");
            LoadFromXmlSpawner("Spawns/felucca.xml", Map.Felucca, "TrinsicPaladinSpawner");
        }
        #endregion

        #region Remove Paladins And Krakens
        public static void RemovePaladinsAndKrakens()
        {
            Remove("HirePaladin");
            Remove("Kraken", sp => !Region.Find(sp.Location, sp.Map).IsPartOf("Shame"));
            ToConsole("Paladins and Krakens removed from spawners.");
        }
        #endregion

        #region Remove Ice Hounds
        public static void RemoveIceHounds()
        {
            Remove("icehound");
            ToConsole("Ice Hounds removed from spawners.");
        }
        #endregion

        #region Version 11
        public static void AddSphinx()
        {
            Engines.GenerateForgottenPyramid.Generate(null);
            ToConsole("Generated Fortune Sphinx.");
        }
        #endregion

        #region Version 9
        public static void ReplaceUnderworldVersion9()
        {
            ReplaceSpawnersByRegionName("Underworld", Map.TerMur, "underworld");

            ReplaceSpawnersByRectangle(new Rectangle2D(5640, 1776, 295, 263), Map.Trammel, null);
            ReplaceSpawnersByRectangle(new Rectangle2D(5640, 1776, 295, 263), Map.Felucca, "solenhives");

            QuestHintItem hint = new DuganMissingQuestCorpse();
            hint.MoveToWorld(new Point3D(1038, 1182, -52), Map.TerMur);

            Static item = new Static(7400);
            item.MoveToWorld(new Point3D(1040, 1181, -53), Map.TerMur);

            item = new Static(7390);
            item.MoveToWorld(new Point3D(1041, 1185, -50), Map.TerMur);

            item = new Static(7390);
            item.MoveToWorld(new Point3D(1036, 1185, -52), Map.TerMur);

            hint = new FlintLostLogbookHint();
            hint.MoveToWorld(new Point3D(1044, 976, -30), Map.TerMur);

            hint = new FlintLostBarrelHint();
            hint.MoveToWorld(new Point3D(1043, 1003, -43), Map.TerMur);

            hint = new FlintLostBarrelHint();
            hint.MoveToWorld(new Point3D(1048, 1027, -32), Map.TerMur);

            GenerateUnderworldRooms.GenerateRevealTiles();

            ToConsole("Placed Quest Statics.");
        }
        #endregion

        #region Version 8
        public static void ReplaceSolenHivesVersion8()
        {
            ReplaceSpawnersByRectangle(new Rectangle2D(5640, 1776, 295, 263), Map.Trammel, null);
            ReplaceSpawnersByRectangle(new Rectangle2D(5640, 1776, 295, 263), Map.Felucca, "solenhives");
        }
        #endregion

        #region Version 6 & 7
        public static void ReplaceTwistedWealdVersion7()
        {
            ReplaceSpawnersByRegionName("Twisted Weald", Map.Ilshenar, "twistedweald");
        }
        #endregion

        #region Version 5
        public static void HonestyItemsVersion5()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
                {
                    int count = 0;

                    foreach (Item item in World.Items.Values)
                    {
                        if (item.HonestyItem && !ItemFlags.GetTaken(item))
                        {
                            RunicReforging.GenerateRandomItem(item, 0, 100, 1000);
                            count++;
                        }
                    }

                    ToConsole(string.Format("Honesty items given magical properties: {0}", count.ToString()));
                });
        }
        #endregion

        #region Version 4
        public static void BrigandsVersion4()
        {
            Replace("humanbrigand", "brigand", null);
            Replace("humanbrigandcamp", "brigandcamp", null);
        }
        #endregion

        #region Version 1
        private static void RemoveSpawnVersion1()
        {
            Remove("SeaHorse");
            Delete("Valem");
        }
        #endregion

        #region Version 0
        private static Dictionary<Type, Type[]> QuestQuesterTypes;

        /// <summary>
        /// Any quests that have questers as null, will assign the quester. Some quests don't have questers...
        /// </summary>
        public static void CheckQuestQuesters()
        {
            ToConsole("Assigning Questers where null...");

            QuestQuesterTypes = new Dictionary<Type, Type[]>();

            foreach (MondainQuester quester in World.Mobiles.Values.OfType<MondainQuester>())
            {
                Type t = quester.GetType();

                if (QuestQuesterTypes.ContainsKey(t))
                    continue;

                Type[] quests = quester.Quests;

                if (quests != null && quests.Length > 0)
                    QuestQuesterTypes[t] = quests;
            }

            foreach (BaseQuestItem item in World.Items.Values.OfType<BaseQuestItem>())
            {
                Type t = item.GetType();

                if (QuestQuesterTypes.ContainsKey(t))
                    continue;

                Type[] quests = item.Quests;

                if (quests != null && quests.Length > 0)
                    QuestQuesterTypes[t] = quests;
            }

            int count = 0;

            foreach (PlayerMobile pm in World.Mobiles.Values.OfType<PlayerMobile>())
            {
                foreach (BaseQuest quest in pm.Quests.Where(q => q.QuesterType == null))
                {
                    foreach (KeyValuePair<Type, Type[]> kvp in QuestQuesterTypes)
                    {
                        if (quest.QuesterType != null)
                            break;

                        foreach (Type type in kvp.Value)
                        {
                            if (type == quest.GetType())
                            {
                                quest.QuesterType = kvp.Key;
                                count++;
                                break;
                            }
                        }
                    }
                }
            }

            ToConsole(string.Format("Quester Re-assignment: {0} quests re-assigned quester type. Some quester types may still be null. These quests will need to be quit.", count.ToString()), ConsoleColor.DarkRed);
        }

        /// <summary>
        /// Disables SmartSpawning on XmlSpawners for any spawners that have paramater 'check' as a spawn object type
        /// </summary>
        /// <param name="check">System.Type to look for in SpawnObject lines</param>
        /// <param name="subclasses">Can the spawn object type derive from check?</param>
        private static void CheckSmartSpawn(Type check, bool subclasses)
        {
            int count = 0;

            List<XmlSpawner> spawners = World.Items.Values.OfType<XmlSpawner>().Where(s => s.SmartSpawning).ToList();

            foreach (XmlSpawner spawner in spawners)
            {
                if (CheckSmartSpawn(spawner, check, subclasses))
                    count++;
            }

            ColUtility.Free(spawners);

            ToConsole(string.Format("Smart Spawn Removal: {0} spawners [type {1}] smart spawning disabled.", count.ToString(), check.Name));
        }

        private static bool CheckSmartSpawn(XmlSpawner spawner, Type check, bool subclasses)
        {
            foreach (XmlSpawner.SpawnObject obj in spawner.SpawnObjects)
            {
                if (obj.TypeName != null)
                {
                    Type t = ScriptCompiler.FindTypeByName(BaseXmlSpawner.ParseObjectType(obj.TypeName));

                    if (t != null && (t == check || subclasses && t.IsSubclassOf(check)))
                    {
                        spawner.SmartSpawning = false;

                        if (spawner.CurrentCount == 0)
                            spawner.DoRespawn = true;

                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Deletes the entire spawner if 'current' if found as an spawn object
        /// </summary>
        /// <param name="current"></param>
        public static void Delete(string current)
        {
            List<XmlSpawner> toDelete = new List<XmlSpawner>();

            foreach (XmlSpawner spawner in World.Items.Values.OfType<XmlSpawner>())
            {
                for (var index = 0; index < spawner.SpawnObjects.Length; index++)
                {
                    XmlSpawner.SpawnObject obj = spawner.SpawnObjects[index];

                    if (obj == null || obj.TypeName == null)
                    {
                        continue;
                    }

                    string typeName = obj.TypeName.ToLower();
                    string lookingFor = current.ToLower();

                    if (typeName.IndexOf(lookingFor) >= 0)
                    {
                        toDelete.Add(spawner);
                        break;
                    }
                }
            }

            for (var index = 0; index < toDelete.Count; index++)
            {
                XmlSpawner spawner = toDelete[index];

                spawner.Delete();
            }

            ToConsole($"Spawner Deletion: deleted {toDelete.Count} spawners containing -{current}-.");

            ColUtility.Free(toDelete);
        }

        /// <summary>
        /// Replaces a certain string value with another in any XmlSpawner SpawnObject line
        /// </summary>
        /// <param name="current">What we're looing for</param>
        /// <param name="replace">What we're replacing it with</param>
        /// <param name="check">if the SpawnObject line contains check, we ignore this line altogether</param>
        public static void Replace(string current, string replace, string check)
        {
            int count = 0;

            foreach (ISpawner spawner in World.Items.Values.OfType<ISpawner>())
            {
                if (Replace(spawner, current, replace, check))
                {
                    count++;
                }
            }

            ToConsole(string.Format("Spawn Replacement: {0} spawners replaced [{1} replaced with {2}].", count.ToString(), current, replace));
        }

        /// <summary>
        /// Replaces a certain string value with another in any XmlSpawner SpawnObject line
        /// </summary>
        /// <param name="current">What we're looing for</param>
        /// <param name="replace">What we're replacing it with</param>
        /// <param name="name">executes replace only if spawner name contains parameter</param>
        /// <param name="check">if the SpawnObject line contains check, we ignore this line altogether</param>
        public static void Replace(string current, string replace, string name, string check)
        {
            int count = 0;

            foreach (ISpawner spawner in World.Items.Values.OfType<ISpawner>().Where(s => s is Item item && item.Name != null && item.Name.ToLower().IndexOf(name.ToLower()) >= 0))
            {
                if (Replace(spawner, current, replace, check))
                    count++;
            }

            ToConsole(string.Format("Spawn Replacement: {0} spawners named {1} replaced [{2} replaced with {3}].", count.ToString(), name, current, replace));
        }

        public static bool Replace(ISpawner spwner, string current, string replace, string check)
        {
            bool replaced = false;

            if (spwner is XmlSpawner xmlSpawner)
            {
                for (var index = 0; index < xmlSpawner.SpawnObjects.Length; index++)
                {
                    XmlSpawner.SpawnObject obj = xmlSpawner.SpawnObjects[index];

                    if (obj == null || obj.TypeName == null)
                    {
                        continue;
                    }

                    string typeName = obj.TypeName.ToLower();
                    string lookingFor = current.ToLower();

                    if (typeName.IndexOf(lookingFor) >= 0)
                    {
                        if (string.IsNullOrEmpty(check) || typeName.IndexOf(check) < 0)
                        {
                            obj.TypeName = typeName.Replace(lookingFor, replace);

                            if (!replaced)
                            {
                                replaced = true;
                            }
                        }
                    }
                }
            }
            else if (spwner is Spawner spawner)
            {
                for (int i = 0; i < spawner.SpawnObjects.Count; i++)
                {
                    SpawnObject so = spawner.SpawnObjects[i];

                    string typeName = so.SpawnName.ToLower();
                    string lookingFor = current.ToLower();

                    if (typeName.IndexOf(lookingFor) >= 0)
                    {
                        if (string.IsNullOrEmpty(check) || typeName.IndexOf(check) < 0)
                        {
                            so.SpawnName = typeName.Replace(lookingFor, replace);

                            if (!replaced)
                            {
                                replaced = true;
                            }
                        }
                    }
                }
            }

            return replaced;
        }

        /// <summary>
        /// Removes a SpawnerObject string, either the string or entire line
        /// </summary>
        /// <param name="toRemove">string to remove from line</param>
        public static void Remove(string toRemove, Func<XmlSpawner, bool> predicate = null)
        {
            int count = 0;
            int deleted = 0;

            List<XmlSpawner> list = new List<XmlSpawner>(World.Items.Values.OfType<XmlSpawner>());

            for (var index = 0; index < list.Count; index++)
            {
                XmlSpawner spawner = list[index];

                if (predicate == null || predicate(spawner))
                {
                    count += Remove(spawner, toRemove, ref deleted);
                }
            }

            ColUtility.Free(list);
            ToConsole(string.Format("Spawn Removal: {0} spawn lines removed containing -{1}-. [{2} deleted].", count.ToString(), toRemove, deleted));
        }

        public static int Remove(XmlSpawner spawner, string toRemove, ref int deleted)
        {
            List<XmlSpawner.SpawnObject> remove = new List<XmlSpawner.SpawnObject>();

            foreach (XmlSpawner.SpawnObject obj in spawner.SpawnObjects)
            {
                if (obj == null || obj.TypeName == null)
                    continue;

                string typeName = obj.TypeName.ToLower();
                string lookingFor = toRemove.ToLower();

                if (typeName.IndexOf(lookingFor) >= 0)
                {
                    remove.Add(obj);
                }
            }

            int count = remove.Count;

            for (var index = 0; index < remove.Count; index++)
            {
                XmlSpawner.SpawnObject obj = remove[index];

                spawner.RemoveSpawnObject(obj);

                foreach (IEntity e in obj.SpawnedObjects.OfType<IEntity>())
                {
                    e.Delete();
                    deleted++;
                }
            }

            ColUtility.Free(remove);
            return count;
        }

        public static void CopyAndPlaceItem(Item oldItem, Point3D p, Map map)
        {
            Item newItem = (Item) Activator.CreateInstance(oldItem.GetType());

            Dupe.CopyProperties(oldItem, newItem);

            oldItem.OnAfterDuped(newItem);

            newItem.MoveToWorld(p, map);

            if (newItem is XmlSpawner spawner)
            {
                spawner.DoRespawn = true;
            }
            else if (newItem is Teleporter teleporter)
            {
                teleporter.MapDest = map;
            }
        }

        /// <summary>
        /// performs a pre-specified action (use lamba with action) if the conditions are met
        /// </summary>
        /// <param name="lineCheck">Condition for a specific string in the spawn object line</param>
        /// <param name="nameCheck">Condition for the spawner name</param>
        /// <param name="exempt">Condition to prevent action being performed on spawner</param>
        /// <param name="action">action to be performed, setup in calling method</param>
        public static void ActionOnSpawner(Type typeCheck, string lineCheck, string nameCheck, string exempt, Action<ISpawner> action, bool inherits = false)
        {
            int count = 0;

            if (action != null)
            {
                List<ISpawner> list = World.Items.Values.OfType<ISpawner>().Where(s =>
                    nameCheck == null || s is Item item && item.Name != null && item.Name.ToLower().IndexOf(nameCheck.ToLower()) >= 0).ToList();

                foreach (ISpawner spawner in list)
                {
                    if (ActionOnSpawner(spawner, typeCheck, lineCheck, exempt, action, inherits))
                        count++;
                }

                ColUtility.Free(list);
            }

            ToConsole(string.Format("Spawner Action: Performed action to {0} spawners{1}",
                count.ToString(), lineCheck != null ? " containing " + lineCheck + "." : typeCheck != null ? " containing " + typeCheck.Name + "." : "."));
        }

        public static bool ActionOnSpawner(ISpawner spawner, Type typeCheck, string lineCheck, string exempt, Action<ISpawner> action, bool inherits)
        {
            string[] list = GetSpawnList(spawner);

            if (list == null)
                return false;

            foreach (string str in list)
            {
                if (string.IsNullOrEmpty(str))
                    continue;

                string spawnObject = str.ToLower();

                if (typeCheck != null)
                {
                    Type t;

                    if (spawner is Spawner)
                        t = ScriptCompiler.FindTypeByName(spawnObject);
                    else
                        t = ScriptCompiler.FindTypeByName(BaseXmlSpawner.ParseObjectType(spawnObject));

                    if (t == typeCheck || t != null && inherits && t.IsSubclassOf(typeCheck))
                    {
                        if (action != null)
                            action(spawner);

                        return true;
                    }
                }
                else
                {
                    string lookFor = lineCheck != null ? lineCheck.ToLower() : null;

                    if ((lookFor == null || spawnObject.IndexOf(lookFor) >= 0) && (exempt == null || spawnObject.IndexOf(exempt.ToLower()) <= 0))
                    {
                        if (action != null)
                            action(spawner);

                        return true;
                    }
                }
            }

            return false;
        }

        private static string[] GetSpawnList(ISpawner spawner)
        {
            string[] list = null;

            if (spawner is XmlSpawner xmlSpawner && xmlSpawner.SpawnObjects != null && xmlSpawner.SpawnObjects.Length > 0)
            {
                list = xmlSpawner.SpawnObjects.Select(obj => obj.TypeName).ToArray();
            }
            else if (spawner is Spawner spawn && spawn.SpawnObjects != null && spawn.SpawnObjects.Count > 0)
            {
                List<SpawnObject> names = spawn.SpawnObjects;

                list = new string[names.Count];

                for (int i = 0; i < names.Count; i++)
                {
                    list[i] = names[i].SpawnName;
                }
            }

            return list;
        }

        public static void ReplaceSpawnersByRegionName(string region, Map map, string file)
        {
            string path = null;

            if (file != null)
            {
                path = string.Format("Spawns/{0}.xml", file);

                if (!File.Exists(path))
                {
                    ToConsole(string.Format("Cannot proceed. {0} does not exist.", file), ConsoleColor.Red);
                    return;
                }
            }

            foreach (Region r in Region.Regions.Where(reg => reg.Map == map && reg.Name == region))
            {
                List<Item> list = r.GetEnumeratedItems().Where(i => i is XmlSpawner || i is Spawner).ToList();

                for (var index = 0; index < list.Count; index++)
                {
                    Item item = list[index];

                    item.Delete();
                }

                ToConsole($"Deleted {list.Count} Spawners in {region}.");
                ColUtility.Free(list);
            }

            if (path != null)
            {
                LoadFromXmlSpawner(path, map);
            }
        }

        public static void ReplaceSpawnersByRectangle(Rectangle2D rec, Map map, string file)
        {
            string path = null;

            if (file != null)
            {
                path = $"Spawns/{file}.xml";

                if (!File.Exists(path))
                {
                    ToConsole($"Cannot proceed. {file} does not exist.", ConsoleColor.Red);
                    return;
                }
            }

            IPooledEnumerable eable = map.GetItemsInBounds(rec);
            List<Item> list = new List<Item>();

            foreach (Item item in eable)
            {
                if (item is XmlSpawner || item is Spawner)
                {
                    list.Add(item);
                }
            }

            for (var index = 0; index < list.Count; index++)
            {
                Item item = list[index];

                item.Delete();
            }

            ToConsole($"Deleted {list.Count} Spawners in {map}.");

            ColUtility.Free(list);
            eable.Free();

            if (path != null)
            {
                LoadFromXmlSpawner(path, map);
            }
        }

        public static void LoadFromXmlSpawner(string location, Map map, string prefix = null)
        {
            string filename = XmlSpawner.LocateFile(location);

            string SpawnerPrefix = prefix == null ? string.Empty : prefix;
            int processedmaps;
            int processedspawners;

            XmlSpawner.XmlLoadFromFile(filename, SpawnerPrefix, null, Point3D.Zero, map, false, 0, false, out processedmaps, out processedspawners);

            ToConsole($"Created {processedspawners} spawners from {location} with -{(SpawnerPrefix == string.Empty ? "NO" : SpawnerPrefix)}- prefix.");
        }

        #region XmlSpawner to Spawner Conversion
        public static void ConvertXmlToSpawners()
        {
            string filename = "Spawns";

            if (Directory.Exists(filename))
            {
                List<string> files = null;

                string[] dirs = null;

                try
                {
                    files = new List<string>(Directory.GetFiles(filename, "*.xml"));
                    dirs = Directory.GetDirectories(filename);
                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }

                if (dirs != null && dirs.Length > 0)
                {
                    for (var index = 0; index < dirs.Length; index++)
                    {
                        string dir = dirs[index];

                        try
                        {
                            string[] dirFiles = Directory.GetFiles(dir, "*.xml");
                            files.AddRange(dirFiles);
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }
                    }
                }

                if (files != null)
                {
                    ToConsole($"Found {files.Count} Xmlspawner files for conversion.", files.Count > 0 ? ConsoleColor.Green : ConsoleColor.Red);

                    if (files.Count > 0)
                    {
                        int converted = 0;
                        int failed = 0;
                        int keep = 0;

                        for (var i = 0; i < files.Count; i++)
                        {
                            string file = files[i];

                            FileStream fs = null;

                            try
                            {
                                fs = File.Open(file, FileMode.Open, FileAccess.Read);
                            }
                            catch (Exception e)
                            {
                                Diagnostics.ExceptionLogging.LogException(e);
                            }

                            if (fs == null)
                            {
                                ToConsole($"Unable to open {filename} for loading", ConsoleColor.Red);
                                continue;
                            }

                            DataSet ds = new DataSet("Spawns");

                            try
                            {
                                ds.ReadXml(fs);
                            }
                            catch
                            {
                                fs.Close();
                                ToConsole($"Error reading xml file {filename}", ConsoleColor.Red);
                                continue;
                            }

                            if (ds.Tables != null && ds.Tables.Count > 0)
                            {
                                if (ds.Tables["Points"] != null && ds.Tables["Points"].Rows.Count > 0)
                                {
                                    for (var index = 0; index < ds.Tables["Points"].Rows.Count; index++)
                                    {
                                        DataRow dr = ds.Tables["Points"].Rows[index];

                                        string id = null;

                                        try
                                        {
                                            id = (string) dr["UniqueId"];
                                        }
                                        catch (Exception e)
                                        {
                                            Diagnostics.ExceptionLogging.LogException(e);
                                        }

                                        bool convert = id != null && ConvertSpawner(id, dr);

                                        if (!convert)
                                        {
                                            Point3D loc = Point3D.Zero;
                                            Map spawnMap = null;

                                            try
                                            {
                                                loc = new Point3D(int.Parse((string) dr["CentreX"]), int.Parse((string) dr["CentreY"]), int.Parse((string) dr["CentreZ"]));

                                                spawnMap = Map.Parse((string) dr["Map"]);
                                            }
                                            catch (Exception e)
                                            {
                                                Diagnostics.ExceptionLogging.LogException(e);
                                            }

                                            if (loc != Point3D.Zero && spawnMap != null && spawnMap != Map.Internal)
                                            {
                                                if (!ConvertSpawnerByLocation(loc, spawnMap, dr, ref keep))
                                                {
                                                    failed++;
                                                }
                                                else
                                                {
                                                    converted++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            converted++;
                                        }
                                    }
                                }
                            }

                            fs.Close();
                        }

                        if (converted > 0)
                        {
                            ToConsole($"Converted {converted} XmlSpawners to standard spawners.");
                        }

                        if (failed > 0)
                        {
                            ToConsole($"Failed to convert {failed} XmlSpawners to standard spawners. {keep} kept due to XmlSpawner Functionality", ConsoleColor.Red);
                        }

                        _SpawnsConverted = true;
                    }
                    else
                    {
                        ToConsole($"Directory Not Found: {filename}", ConsoleColor.Red);
                    }
                }
            }
        }

        private static bool ConvertSpawnerByLocation(Point3D p, Map map, DataRow dr, ref int keep)
        {
            XmlSpawner spawner = World.Items.Values.OfType<XmlSpawner>().FirstOrDefault(s => s.Location == p && s.Map == map);

            return ConvertSpawner(spawner, dr, ref keep);
        }

        private static bool ConvertSpawner(string id, DataRow dr)
        {
            XmlSpawner spawner = World.Items.Values.OfType<XmlSpawner>().FirstOrDefault(s => s.UniqueId == id);

            int c = 0;
            return ConvertSpawner(spawner, dr, ref c);
        }

        private static bool DeleteSpawner(string id)
        {
            if (id == null)
                return false;

            XmlSpawner spawner = World.Items.Values.OfType<XmlSpawner>().FirstOrDefault(s => s.UniqueId == id);

            if (spawner != null)
            {
                spawner.Delete();
                return true;
            }

            return false;
        }

        private static bool ConvertSpawner(XmlSpawner spawner, DataRow d, ref int keep)
        {
            if (spawner != null)
            {
                SpawnObject[] spawns = new SpawnObject[spawner.SpawnObjects.Length];

                for (int i = 0; i < spawner.SpawnObjects.Length; i++)
                {
                    XmlSpawner.SpawnObject obj = spawner.SpawnObjects[i];

                    if (obj == null || obj.TypeName == null)
                        continue;

                    spawns[i] = new SpawnObject(obj.TypeName, obj.MaxCount);
                }

                if (HasSpecialXmlSpawnerString(spawns))
                {
                    keep++;
                    return false;
                }

                Spawner newSpawner = new Spawner(spawner.MaxCount,
                                                 spawner.MinDelay,
                                                 spawner.MaxDelay,
                                                 spawner.Team,
                                                 spawner.SpawnRange,
                                                 spawns.ToList())
                {
                    Group = spawner.Group,
                    Running = spawner.Running
                };

                newSpawner.MoveToWorld(spawner.Location, spawner.Map);
                spawner.Delete();
                return true;
            }

            return false;
        }

        private static bool HasSpecialXmlSpawnerString(SpawnObject[] spawns)
        {
            foreach (SpawnObject obj in spawns)
            {
                if (obj.SpawnName != null)
                {
                    foreach (string s in _SpawnerSymbols)
                    {
                        if (obj.SpawnName.Contains(s))
                            return true;
                    }
                }
            }

            return false;
        }

        private static readonly string[] _SpawnerSymbols =
        {
            "/", "<", ">", ",", "{", "}"
        };

        public static void RevertToXmlSpawners(Mobile from)
        {
            string filename = "Spawns";

            if (Directory.Exists(filename))
            {
                List<string> files = null;
                string[] dirs = null;

                try
                {
                    files = new List<string>(Directory.GetFiles(filename, "*.xml"));
                    dirs = Directory.GetDirectories(filename);
                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }

                if (dirs != null && dirs.Length > 0)
                {
                    foreach (string dir in dirs)
                    {
                        try
                        {
                            string[] dirFiles = Directory.GetFiles(dir, "*.xml");
                            files.AddRange(dirFiles);
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }
                    }
                }

                ToConsole($"Found {(files == null ? 0 : files.Count)} Xmlspawner files for removal.", files != null && files.Count > 0 ? ConsoleColor.Green : ConsoleColor.Red);
                ToConsole("Deleting spawners...", ConsoleColor.Cyan);
                long start = Core.TickCount;

                if (files != null && files.Count > 0)
                {
                    int deletedxml = 0;
                    int nospawner = 0;

                    foreach (string file in files)
                    {
                        FileStream fs = null;

                        try
                        {
                            fs = File.Open(file, FileMode.Open, FileAccess.Read);
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }

                        if (fs == null)
                        {
                            ToConsole(string.Format("Unable to open {0} for loading", filename), ConsoleColor.Red);
                            continue;
                        }

                        DataSet ds = new DataSet("Spawns");

                        try
                        {
                            ds.ReadXml(fs);
                        }
                        catch
                        {
                            fs.Close();
                            ToConsole(string.Format("Error reading xml file {0}", filename), ConsoleColor.Red);
                            continue;
                        }

                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables["Points"] != null && ds.Tables["Points"].Rows.Count > 0)
                            {
                                foreach (DataRow dr in ds.Tables["Points"].Rows)
                                {
                                    string id = null;

                                    try
                                    {
                                        id = (string)dr["UniqueId"];
                                    }
                                    catch (Exception e)
                                    {
                                        Diagnostics.ExceptionLogging.LogException(e);
                                    }

                                    if (DeleteSpawner(id))
                                    {
                                        deletedxml++;
                                    }
                                    else
                                    {
                                        nospawner++;
                                    }
                                }
                            }
                        }

                        fs.Close();
                    }

                    ToConsole(string.Format("Deleted {0} XmlSpawners [{1} no id] in {2} seconds.", deletedxml, nospawner, ((Core.TickCount - start) / 1000).ToString()), ConsoleColor.Cyan);
                }
                else
                {
                    ToConsole(string.Format("Directory Not Found: {0}", filename), ConsoleColor.Red);
                }
            }
        }
        #endregion

        /// <summary>
        /// Deletes all spawners from a specific file. This can be used to delete spawners from a specific system where the spawner wasn't 
        /// Generated from the Spawn Folder.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filename"></param>
        public static void RemoveSpawnsFromXmlFile(string directory, string filename)
        {
            if (Directory.Exists(directory))
            {
                List<string> files = null;

                try
                {
                    files = new List<string>(Directory.GetFiles(directory, filename + ".xml"));
                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }

                ToConsole(string.Format("Found {0} Xmlspawner files for removal.", files == null ? "0" : files.Count.ToString()), files != null && files.Count > 0 ? ConsoleColor.Green : ConsoleColor.Red);
                ToConsole("Deleting spawners...", ConsoleColor.Cyan);

                if (files != null && files.Count > 0)
                {
                    int deletedxml = 0;

                    foreach (string file in files)
                    {
                        FileStream fs = null;

                        try
                        {
                            fs = File.Open(file, FileMode.Open, FileAccess.Read);
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }

                        if (fs == null)
                        {
                            ToConsole($"Unable to open {filename} for loading", ConsoleColor.Red);
                            continue;
                        }

                        DataSet ds = new DataSet("Spawns");

                        try
                        {
                            ds.ReadXml(fs);
                        }
                        catch
                        {
                            fs.Close();
                            ToConsole($"Error reading xml file {filename}", ConsoleColor.Red);
                            continue;
                        }

                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables["Points"] != null && ds.Tables["Points"].Rows.Count > 0)
                            {
                                foreach (DataRow dr in ds.Tables["Points"].Rows)
                                {
                                    string id = null;

                                    try
                                    {
                                        id = (string)dr["UniqueId"];
                                    }
                                    catch (Exception e)
                                    {
                                        Diagnostics.ExceptionLogging.LogException(e);
                                    }

                                    if (DeleteSpawner(id))
                                    {
                                        deletedxml++;
                                    }
                                }
                            }
                        }

                        fs.Close();
                    }

                    ToConsole($"Deleted {deletedxml} XmlSpawners from {directory}/{filename}.xml.", ConsoleColor.Cyan);
                }
                else
                {
                    ToConsole($"File Not Found: {filename}", ConsoleColor.Red);
                }
            }
        }

        /// <summary>
        /// Used in place of XmlSpawner wipe all spawners. This iterates through the Spawn Folder and deletes those spawners only.
        /// This will keep spawners for seprate systems in place. This is called in DeleteWorld gump.
        /// </summary>
        public static void WipeSpawnersFromFile()
        {
            string filename = "Spawns";

            if (Directory.Exists(filename))
            {
                List<string> files = null;
                string[] dirs = null;

                try
                {
                    files = new List<string>(Directory.GetFiles(filename, "*.xml"));
                    dirs = Directory.GetDirectories(filename);
                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }

                if (dirs != null && dirs.Length > 0)
                {
                    for (var index = 0; index < dirs.Length; index++)
                    {
                        string dir = dirs[index];

                        try
                        {
                            string[] dirFiles = Directory.GetFiles(dir, "*.xml");

                            files.AddRange(dirFiles);
                        }
                        catch (Exception e)
                        {
                            Diagnostics.ExceptionLogging.LogException(e);
                        }
                    }
                }

                if (files != null)
                {
                    ToConsole($"Found {files.Count} Xmlspawner files for conversion.", files.Count > 0 ? ConsoleColor.Green : ConsoleColor.Red);
                    ToConsole("Deleting spawners...", ConsoleColor.Cyan);

                    long start = Core.TickCount;

                    if (files.Count > 0)
                    {
                        int deletedxml = 0;
                        int nodelelete = 0;

                        for (var index = 0; index < files.Count; index++)
                        {
                            string file = files[index];

                            FileStream fs = null;

                            try
                            {
                                fs = File.Open(file, FileMode.Open, FileAccess.Read);
                            }
                            catch (Exception e)
                            {
                                Diagnostics.ExceptionLogging.LogException(e);
                            }

                            if (fs == null)
                            {
                                ToConsole($"Unable to open {filename} for loading", ConsoleColor.Red);
                                continue;
                            }

                            DataSet ds = new DataSet("Spawns");

                            try
                            {
                                ds.ReadXml(fs);
                            }
                            catch
                            {
                                fs.Close();
                                ToConsole($"Error reading xml file {filename}", ConsoleColor.Red);
                                continue;
                            }

                            if (ds.Tables != null && ds.Tables.Count > 0)
                            {
                                if (ds.Tables["Points"] != null && ds.Tables["Points"].Rows.Count > 0)
                                {
                                    for (var i = 0; i < ds.Tables["Points"].Rows.Count; i++)
                                    {
                                        DataRow dr = ds.Tables["Points"].Rows[i];

                                        string id = null;

                                        try
                                        {
                                            id = (string) dr["UniqueId"];
                                        }
                                        catch (Exception e)
                                        {
                                            Diagnostics.ExceptionLogging.LogException(e);
                                        }

                                        if (DeleteSpawner(id))
                                        {
                                            deletedxml++;
                                        }
                                        else
                                        {
                                            bool deleted = false;

                                            try
                                            {
                                                Point3D loc = new Point3D(int.Parse((string) dr["CentreX"]), int.Parse((string) dr["CentreY"]), int.Parse((string) dr["CentreZ"]));
                                                Map spawnMap = Map.Parse((string) dr["Map"]);

                                                string name = (string) dr["Name"];

                                                if (spawnMap != null)
                                                {
                                                    IPooledEnumerable eable = spawnMap.GetItemsInRange(loc, 0);

                                                    foreach (Item item in eable)
                                                    {
                                                        if (item is XmlSpawner && item.Name == name)
                                                        {
                                                            item.Delete();
                                                            deletedxml++;
                                                            deleted = true;
                                                            break;
                                                        }
                                                    }

                                                    eable.Free();
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Diagnostics.ExceptionLogging.LogException(e);
                                            }

                                            if (!deleted)
                                            {
                                                nodelelete++;

                                                ToConsole($"Failed to Delete: {(string)dr["Name"]} in {file}");
                                            }
                                        }
                                    }
                                }
                            }

                            fs.Close();
                        }

                        ToConsole($"Deleted {deletedxml} XmlSpawners [{nodelelete} failed] in {((Core.TickCount - start) / 1000).ToString()} seconds.", ConsoleColor.Cyan);
                    }
                    else
                    {
                        ToConsole($"Directory Not Found: {filename}", ConsoleColor.Red);
                    }
                }
            }
        }
    }
}
