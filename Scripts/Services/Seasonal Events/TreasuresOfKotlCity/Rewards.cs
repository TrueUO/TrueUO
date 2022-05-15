using Server.Items;
using System.Collections.Generic;

namespace Server.Engines.TreasuresOfKotlCity
{
    public static class KotlCityRewards
    {
        public static List<CollectionItem> Rewards { get; set; }

        public static void Initialize()
        {
            Rewards = new List<CollectionItem>();

            Rewards.Add(new CollectionItem(typeof(SkeletalHangmanAddonDeed), 0x14EF, 1156982, 0, 10));
            Rewards.Add(new CollectionItem(typeof(KotlSacraficialAltarAddonDeed), 0x14EF, 1124311, 0, 10));
            Rewards.Add(new CollectionItem(typeof(BlackrockMoonstone), 0x9CAA, 1156993, 1175, 10));
            Rewards.Add(new CollectionItem(typeof(AutomatonActuator), 0x9CE9, 1156997, 0, 5));
            Rewards.Add(new CollectionItem(typeof(TreasuresOfKotlRewardDeed), 0x14EF, 1156987, 0, 10));
            Rewards.Add(new CollectionItem(typeof(TreasuresOfKotlRewardDeed), 0x14EF, 1156986, 0, 10));
            Rewards.Add(new CollectionItem(typeof(TreasuresOfKotlRewardDeed), 0x14EF, 1156985, 0, 10));

            Rewards.Add(new CollectionItem(typeof(KatalkotlsRing), 0x1F09, 1156989, 2591, 50));
            Rewards.Add(new CollectionItem(typeof(BootsOfEscaping), 0x1711, 1155607, 0, 50));
        }
    }
}
