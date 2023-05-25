using Server.Commands;
using System;

namespace Server.Engines.NewMagincia
{
    public static class NewMaginciaCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("ViewLottos", AccessLevel.GameMaster, ViewLottos_OnCommand);

            CommandSystem.Register("GenNewMagincia", AccessLevel.GameMaster, GenNewMagincia_OnCommand);
            CommandSystem.Register("DeleteNewMagincia", AccessLevel.Administrator, Delete);
        }

        private static void Delete(CommandEventArgs e)
        {
        }

        public static void GenNewMagincia_OnCommand(CommandEventArgs e)
        {
            Console.WriteLine("Generating New Magincia Housing Lotty System..");

            if (MaginciaLottoSystem.Instance == null)
            {
                MaginciaLottoSystem.Instance = new MaginciaLottoSystem();
                MaginciaLottoSystem.Instance.MoveToWorld(new Point3D(3718, 2049, 5), Map.Trammel);

                Console.WriteLine("Generated {0} New Magincia Housing Plots.", MaginciaLottoSystem.Plots.Count);
            }
            else
            {
                Console.WriteLine("Magincia Housing Lotto System already exists!");
            }
        }

        public static void ViewLottos_OnCommand(CommandEventArgs e)
        {
            if (e.Mobile.AccessLevel > AccessLevel.Player)
            {
                e.Mobile.CloseGump(typeof(LottoTrackingGump));
                e.Mobile.SendGump(new LottoTrackingGump());
            }
        }
    }
}
