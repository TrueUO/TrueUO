using Server.Items;
using System.Collections.Generic;

namespace Server.Engines.Blackthorn
{
    public static class BlackthornRewards
    {
        public static List<CollectionItem> Rewards { get; set; }

        public static void Initialize()
        {
            Rewards = new List<CollectionItem>();

            Rewards.Add(new CollectionItem(typeof(DecorativeShardShieldDeed), 0x14EF, 1153729, 0, 10));
            Rewards.Add(new CollectionItem(typeof(BlackthornPainting1), 0x4C63, 1023744, 0, 10));
            Rewards.Add(new CollectionItem(typeof(BlackthornPainting2), 0x4C65, 1023744, 0, 10));
        }
    }
}
