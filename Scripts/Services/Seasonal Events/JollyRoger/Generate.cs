using Server.Engines.Points;
using Server.Items;
using System;
using Server.Commands;
using Server.Engines.Fellowship;
using Server.Mobiles;

namespace Server.Engines.JollyRoge
{
    public static class JollyRogerGeneration
    {
        public static void Initialize()
        {
            EventSink.WorldSave += OnWorldSave;

            CommandSystem.Register("GenJollyRoger", AccessLevel.Administrator, Generate);
        }

        public static void Generate(CommandEventArgs e)
        {
            Generate();
        }

        private static void OnWorldSave(WorldSaveEventArgs e)
        {
            CheckEnabled(true);
        }

        public static void CheckEnabled(bool timed = false)
        {
            FellowshipData fellowship = PointsSystem.FellowshipData;

            if (fellowship.Enabled && !fellowship.InSeason)
            {
                if (timed)
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(30), () =>
                    {
                        Utility.WriteConsoleColor(ConsoleColor.Green, "Disabling Jolly Roger");

                        Remove();
                    });
                }
                else
                {
                    Utility.WriteConsoleColor(ConsoleColor.Green, "Auto Disabling Jolly Roger");

                    Remove();
                }
            }
            else if (!fellowship.Enabled && fellowship.InSeason)
            {
                if (timed)
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(30), () =>
                    {
                        Utility.WriteConsoleColor(ConsoleColor.Green, "Enabling Jolly Roger");

                        Generate();
                    });
                }
                else
                {
                    Utility.WriteConsoleColor(ConsoleColor.Green, "Auto Enabling Jolly Roger");

                    Generate();
                }
            }
        }

        #region remove decoration
        public static void Remove()
        {
            RemoveDecoration();
        }

        public static void RemoveDecoration()
        {
            if (AdmiralJacksShipwreckAddon.InstanceTram != null)
            {
                AdmiralJacksShipwreckAddon.InstanceTram.Delete();
                AdmiralJacksShipwreckAddon.InstanceTram = null;
            }

            if (IversRoundingAddon.InstanceTram != null)
            {
                IversRoundingAddon.InstanceTram.Delete();
                IversRoundingAddon.InstanceTram = null;
            }

            if (AdmiralJacksShipwreckAddon.InstanceFel != null)
            {
                AdmiralJacksShipwreckAddon.InstanceFel.Delete();
                AdmiralJacksShipwreckAddon.InstanceFel = null;
            }

            if (IversRoundingAddon.InstanceFel != null)
            {
                IversRoundingAddon.InstanceFel.Delete();
                IversRoundingAddon.InstanceFel = null;
            }

            if (CastleAddon.Instance == null)
            {
                CastleAddon.Instance.Delete();
                CastleAddon.Instance = null;
            }
        }

        #endregion

        public static string[] Ghost =
        {
            "Ghost,One",
            "Ghost,Two",
            "Ghost,Three",
            "Ghost,Four",
            "Ghost,Five",
        };

        public static readonly string EntityName = "JollyRoger";

        public static void Generate()
        {
            BaseMulti shipwreck;
            Item item;
            XmlSpawner sp;
            Static st;
            DarkWoodDoor door;

            XmlSpawner.SpawnObject[] so = new XmlSpawner.SpawnObject[Ghost.Length];

            for (int i = 0; i < Ghost.Length; i++)
            {
                so[i] = new XmlSpawner.SpawnObject(Ghost[i], 1);
            }

            Item tele = new Teleporter(new Point3D(2264, 1574, -28), Map.Ilshenar);
            tele.MoveToWorld(new Point3D(1528, 1341, -3), Map.Ilshenar);
            WeakEntityCollection.Add(EntityName, tele);

            if (HawkwindSpeak.Instance == null)
            {
                HawkwindSpeak.Instance = new HawkwindSpeak();
                HawkwindSpeak.Instance.MoveToWorld(new Point3D(2267, 1563, -28), Map.Ilshenar);
            }

            if (HawkwindTimeLord.Instance == null)
            {
                HawkwindTimeLord.Instance = new HawkwindTimeLord();
                HawkwindTimeLord.Instance.MoveToWorld(new Point3D(2263, 1554, -28), Map.Ilshenar);
            }

            st = new Static(0x1e5d);
            st.MoveToWorld(new Point3D(2263, 1549, -28), Map.Ilshenar);

            st = new Static(0x1e5c);
            st.MoveToWorld(new Point3D(2264, 1549, -28), Map.Ilshenar);

            item = new CastleTrapDoor(new Point3D(982, 1126, 65), Map.Ilshenar, false);
            item.MoveToWorld(new Point3D(1316, 231, -26), Map.Ilshenar);

            item = new CastleTrapDoor(new Point3D(1316, 231, -26), Map.Ilshenar, true);
            item.MoveToWorld(new Point3D(982, 1126, 65), Map.Ilshenar);

            if (CastleAddon.Instance == null)
            {
                CastleAddon.Instance = new CastleAddon();
                CastleAddon.Instance.MoveToWorld(new Point3D(994, 1140, 43), Map.Ilshenar);
            }

            door = new DarkWoodDoor(DoorFacing.WestCCW);
            door.MoveToWorld(new Point3D(981, 1131, 65), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.SouthCCW);
            door.MoveToWorld(new Point3D(990, 1134, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.NorthCW);
            door.MoveToWorld(new Point3D(990, 1133, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.WestCW);
            door.MoveToWorld(new Point3D(998, 1131, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.WestCCW);
            door.MoveToWorld(new Point3D(1012, 1131, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.EastCCW);
            door.MoveToWorld(new Point3D(1004, 1136, 48), Map.Ilshenar);
            
            door = new DarkWoodDoor(DoorFacing.WestCW);
            door.MoveToWorld(new Point3D(1003, 1136, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.WestCCW);
            door.MoveToWorld(new Point3D(1003, 1141, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.EastCW);
            door.MoveToWorld(new Point3D(1004, 1141, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.SouthCCW);
            door.MoveToWorld(new Point3D(990, 1154, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.NorthCW);
            door.MoveToWorld(new Point3D(990, 1153, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.NorthCCW);
            door.MoveToWorld(new Point3D(995, 1153, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.SouthCW);
            door.MoveToWorld(new Point3D(995, 1154, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.SouthCCW);
            door.MoveToWorld(new Point3D(1001, 1154, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.NorthCW);
            door.MoveToWorld(new Point3D(1001, 1153, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.NorthCCW);
            door.MoveToWorld(new Point3D(1006, 1154, 48), Map.Ilshenar);

            door = new DarkWoodDoor(DoorFacing.SouthCW);
            door.MoveToWorld(new Point3D(1006, 1155, 48), Map.Ilshenar);

            if (!Siege.SiegeShard)
            {
                if (AdmiralJacksShipwreckAddon.InstanceTram == null)
                {
                    AdmiralJacksShipwreckAddon.InstanceTram = new AdmiralJacksShipwreckAddon();
                    AdmiralJacksShipwreckAddon.InstanceTram.MoveToWorld(new Point3D(4269, 560, -14), Map.Trammel);
                }

                shipwreck = new BaseMulti(33);
                shipwreck.MoveToWorld(new Point3D(4268, 568, 0), Map.Trammel);

                item = new ShipwreckBook();
                item.MoveToWorld(new Point3D(4266, 572, 0), Map.Trammel);

                if (JackCorpse.InstanceTram == null)
                {
                    JackCorpse.InstanceTram = new JackCorpse();
                    JackCorpse.InstanceTram.MoveToWorld(new Point3D(4267, 574, 0), Map.Trammel);
                }

                if (IversRoundingAddon.InstanceTram == null)
                {
                    IversRoundingAddon.InstanceTram = new IversRoundingAddon();
                    IversRoundingAddon.InstanceTram.MoveToWorld(new Point3D(449, 2083, -5), Map.Trammel);
                }

                item = new IRTeleporter();
                item.MoveToWorld(new Point3D(450, 2083, 34), Map.Trammel);

                if (Shamino.InstanceTram == null)
                {
                    Shamino.InstanceTram = new Shamino();
                    Shamino.InstanceTram.MoveToWorld(new Point3D(450, 2082, 34), Map.Trammel);
                }

                sp = new XmlSpawner(Guid.NewGuid(), 0, 0, 0, 0, "#JollyRogerGhost", 5, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(0), -1, 0x1F4, 1, 0, 10, false, so, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0), null, null, null, null, null, null, null, null, null, 1, null, false, XmlSpawner.TODModeType.Realtime, 1, false, -1, null, false, false, false, null, TimeSpan.FromHours(0), null, false, null);
                WeakEntityCollection.Add(EntityName, sp);
                sp.SpawnRange = 15;
                sp.MoveToWorld(new Point3D(468, 2091, 7), Map.Trammel);
                sp.Respawn();

                if (SherryTheMouse.InstanceTram == null)
                {
                    SherryTheMouse.InstanceTram = new SherryTheMouse();
                    SherryTheMouse.InstanceTram.MoveToWorld(new Point3D(1347, 1644, 72), Map.Trammel);
                }
            }

            if (AdmiralJacksShipwreckAddon.InstanceFel == null)
            {
                AdmiralJacksShipwreckAddon.InstanceFel = new AdmiralJacksShipwreckAddon();
                AdmiralJacksShipwreckAddon.InstanceFel.MoveToWorld(new Point3D(4269, 560, -14), Map.Felucca);
            }

            shipwreck = new BaseMulti(33);
            shipwreck.MoveToWorld(new Point3D(4268, 568, 0), Map.Felucca);

            item = new ShipwreckBook();
            item.MoveToWorld(new Point3D(4266, 572, 0), Map.Felucca);

            if (JackCorpse.InstanceFel == null)
            {
                JackCorpse.InstanceFel = new JackCorpse();
                JackCorpse.InstanceFel.MoveToWorld(new Point3D(4267, 574, 0), Map.Felucca);
            }

            if (IversRoundingAddon.InstanceFel == null)
            {
                IversRoundingAddon.InstanceFel = new IversRoundingAddon();
                IversRoundingAddon.InstanceFel.MoveToWorld(new Point3D(449, 2083, -5), Map.Felucca);
            }

            item = new IRTeleporter();
            item.MoveToWorld(new Point3D(450, 2083, 34), Map.Felucca);

            if (Shamino.InstanceFel == null)
            {
                Shamino.InstanceFel = new Shamino();
                Shamino.InstanceFel.MoveToWorld(new Point3D(450, 2082, 34), Map.Felucca);
            }

            sp = new XmlSpawner(Guid.NewGuid(), 0, 0, 0, 0, "#JollyRogerGhost", 5, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(0), -1, 0x1F4, 1, 0, 10, false, so, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(0), null, null, null, null, null, null, null, null, null, 1, null, false, XmlSpawner.TODModeType.Realtime, 1, false, -1, null, false, false, false, null, TimeSpan.FromHours(0), null, false, null);
            WeakEntityCollection.Add(EntityName, sp);
            sp.SpawnRange = 15;
            sp.MoveToWorld(new Point3D(468, 2091, 7), Map.Felucca);
            sp.Respawn();

            if (SherryTheMouse.InstanceFel == null)
            {
                SherryTheMouse.InstanceFel = new SherryTheMouse();
                SherryTheMouse.InstanceFel.MoveToWorld(new Point3D(1347, 1644, 72), Map.Felucca);
            }
        }
    }
}
