using Server.Engines.Points;
using Server.Items;
using System;

namespace Server.Engines.JollyRoge
{
    public static class JollyRogerGeneration
    {
        public static void Initialize()
        {
            EventSink.WorldSave += OnWorldSave;
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
        }

        #endregion


        public static void Generate()
        {
            BaseMulti shipwreck;

            if (!Siege.SiegeShard)
            {
                if (AdmiralJacksShipwreckAddon.InstanceTram == null)
                {
                    AdmiralJacksShipwreckAddon.InstanceTram = new AdmiralJacksShipwreckAddon();
                    AdmiralJacksShipwreckAddon.InstanceTram.MoveToWorld(new Point3D(4269, 560, -14), Map.Trammel);
                }

                shipwreck = new BaseMulti(33);
                shipwreck.MoveToWorld(new Point3D(4268, 568, 0), Map.Trammel);

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
            }

            if (AdmiralJacksShipwreckAddon.InstanceFel == null)
            {
                AdmiralJacksShipwreckAddon.InstanceFel = new AdmiralJacksShipwreckAddon();
                AdmiralJacksShipwreckAddon.InstanceFel.MoveToWorld(new Point3D(4269, 560, -14), Map.Felucca);
            }

            shipwreck = new BaseMulti(33);
            shipwreck.MoveToWorld(new Point3D(4268, 568, 0), Map.Felucca);

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
        }
    }
}
