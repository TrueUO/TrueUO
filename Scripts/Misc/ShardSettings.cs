using Server.Accounting;
using Server.Services.TownCryer;

namespace Server
{
    public static class ShardSettings
    {
        [CallPriority(int.MinValue)]
        public static void Configure()
        {
            AccountGold.Enabled = true;
            AccountGold.ConvertOnBank = true;
            AccountGold.ConvertOnTrade = false;
            VirtualCheck.UseEditGump = true;

            TownCryerSystem.Enabled = true;

            Mobile.VisibleDamageType = VisibleDamageType.Related;

            AOS.DisableStatInfluences();

            Mobile.AOSStatusHandler = AOS.GetStatus;
        }
    }
}
