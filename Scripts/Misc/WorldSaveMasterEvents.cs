using Server.Accounting;
using Server.Engines.Astronomy;
using Server.Engines.BulkOrders;
using Server.Engines.CannedEvil;
using Server.Engines.Craft;
using Server.Engines.Distillation;
using Server.Engines.Fellowship;
using Server.Engines.Help;
using Server.Engines.JollyRoger;
using Server.Engines.MyrmidexInvasion;
using Server.Engines.Plants;
using Server.Engines.Points;
using Server.Engines.Quests;
using Server.Engines.Quests.TimeLord;
using Server.Engines.SeasonalEvents;
using Server.Engines.SphynxFortune;
using Server.Engines.UOStore;
using Server.Engines.VendorSearching;
using Server.Engines.VoidPool;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Services.Community_Collections;
using Server.Services.Virtues;

namespace Server.Misc
{
    internal class WorldMasterEvents
    {
        public static void Initialize()
        {
            EventSink.WorldSave += OnSave;
            EventSink.AfterWorldSave += AfterWorldSave;
        }

        private static void OnSave(WorldSaveEventArgs e)
        {
            Accounts.Save();
            BaseBoat.OnSave();
            BulkOrderSystem.OnSave();
            ChampionSystem.OnSave();
            CollectionsSystem.OnSave();
            CompassionSage.OnSave();
            CraftContext.OnSave();
            CreateWorldData.OnSave();
            DisguisePersistence.OnSave();
            DistillationContext.OnSave();
            EnchantedHotItem.OnSave();
            FellowshipDonationBox.OnSave();
            Gareth.OnSave();
            GlobalTownCrierEntryList.OnSave();
            HelpPersistence.OnSave();
            HighSeasPersistance.OnSave();
            MiningCooperative.OnSave();
            MondainQuestData.OnSave();
            MyrmidexInvasionSystem.OnSave();
            PointsSystem.OnSave();
            PotionOfGloriousFortune.OnSave();
            SeasonalEventSystem.OnSave();
            SphynxFortune.OnSave();
            ReforgingContext.OnSave();
            SherryStrongBox.OnSave();
            SpawnerPersistence.OnSave();
            TimeForLegendsQuest.OnSave();
            UltimaStore.OnSave();
            VendorSearch.OnSave();
            VoidPoolStats.OnSave();
            WeakEntityCollection.Save();
            //Forsaken Foes
            Worker.OnSave();

            if (AstronomySystem.Enabled)
            {
                AstronomySystem.OnSave();
            }

            if (Siege.SiegeShard)
            {
                Siege.OnSave();
            }

            if (!AutoRestart.Enabled)
            {
                PlantSystem.OnSave();
            }
        }

        private static void AfterWorldSave(AfterWorldSaveEventArgs e)
        {
           FountainOfLife.CheckRecharge();
           HonestyVirtue.OnAfterSave();
           SeedOfLife.CheckCleanup();
           SeasonalEventSystem.AfterSave();

           if (Siege.SiegeShard)
           {
               Siege.OnAfterSave();
           }
        }
    }
}
