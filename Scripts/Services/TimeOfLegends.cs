using Server.Commands;
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

            ShadowguardController.SetupShadowguard(e.Mobile);
            
            MacawSpawner.Generate();

            CommandSystem.Handle(e.Mobile, CommandSystem.Prefix + "XmlLoad Spawns/Eodon.xml");

            e.Mobile.SendMessage("Time Of Legends world generating complete.");
        }
    }
}
