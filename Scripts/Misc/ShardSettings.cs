using Server.Accounting;
using Server.Services.TownCryer;

namespace Server
{
    public static class ShardSettings
    {
        [CallPriority(int.MinValue)]
        public static void Configure()
        {
            AccountGold.ConvertOnBank = true;
            VirtualCheck.UseEditGump = true;

            TownCryerSystem.Enabled = true;

            Mobile.InsuranceEnabled = !Siege.SiegeShard;
            Mobile.VisibleDamageType = VisibleDamageType.Related;

            AOS.DisableStatInfluences();

            Mobile.AOSStatusHandler = AOS.GetStatus;
        }
    }
}
