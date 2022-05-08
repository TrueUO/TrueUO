using Server.Commands;
using Server.Engines.CannedEvil;
using Server.Engines.Shadowguard;
using Server.Items;

namespace Server
{
    public static class TimeOfLegends
    {
        public static void Initialize()
        {
            CommandSystem.Register("DecorateTOL", AccessLevel.GameMaster, DecorateTOL_OnCommand);
        }

        public static bool FindItem(int x, int y, int z, Map map, Item test)
        {
            return FindItem(new Point3D(x, y, z), map, test);
        }

        public static bool FindItem(Point3D p, Map map, Item test)
        {
            IPooledEnumerable eable = map.GetItemsInRange(p);

            foreach (Item item in eable)
            {
                if (item.Z == p.Z && item.ItemID == test.ItemID)
                {
                    eable.Free();
                    return true;
                }
            }

            eable.Free();
            return false;
        }

        [Usage("DecorateTOL")]
        [Description("Generates Time of Legends world decoration.")]
        private static void DecorateTOL_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Generating Time Of Legends world decoration, please wait.");

            Decorate.Generate("tol", "Data/Decoration/TimeOfLegends/TerMur", Map.TerMur);
            Decorate.Generate("tol", "Data/Decoration/TimeOfLegends/Felucca", Map.Felucca);

            ChampionSpawn sp = new ChampionSpawn
            {
                Type = ChampionSpawnType.DragonTurtle
            };
            sp.MoveToWorld(new Point3D(451, 1696, 65), Map.TerMur);
            sp.Active = true;
            WeakEntityCollection.Add("tol", sp);

            sp = new ChampionSpawn
            {
                SpawnRadius = 35,
                SpawnMod = .5,
                KillsMod = .5,
                Type = ChampionSpawnType.DragonTurtle
            };
            sp.MoveToWorld(new Point3D(7042, 1889, 60), Map.Felucca);
            sp.Active = true;
            WeakEntityCollection.Add("tol", sp);

            PublicMoongate gate = new PublicMoongate();
            gate.MoveToWorld(new Point3D(719, 1863, 40), Map.TerMur);

            ShadowguardController.SetupShadowguard(e.Mobile);
            Engines.MyrmidexInvasion.GenerateMyrmidexQuest.Generate();

            MacawSpawner.Generate();

            CommandSystem.Handle(e.Mobile, CommandSystem.Prefix + "XmlLoad Spawns/Eodon.xml");

            e.Mobile.SendMessage("Time Of Legends world generating complete.");
        }
    }
}
