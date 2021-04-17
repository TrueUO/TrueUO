using Server.Engines.Points;
using System;

namespace Server.Engines.UOStore
{
    public enum CurrencyType
    {
        None,
        Sovereigns,
        Gold,
        PointsSystem,
        Custom
    }

    public delegate int CustomCurrencyHandler(Mobile m, int consume);

    public static class Configuration
    {
        public static bool Enabled { get; set; }
        public static string Website { get; }

        /// <summary>
        ///     A hook to allow handling of custom currencies.
        ///     This implementation should be treated as such;
        ///     If 'consume' is less than zero, return the currency total.
        ///     Else deduct from the currency total, return the amount consumed.
        /// </summary>
        public static CustomCurrencyHandler ResolveCurrency { get; }

        public static CurrencyType CurrencyImpl { get; }
        public static string CurrencyName { get; }
        public static bool CurrencyDisplay { get; }

        public static PointsType PointsImpl { get; }

        public static double CostMultiplier { get; }

        public static int CartCapacity { get; }

        static Configuration()
        {
            Enabled = Config.Get("Store.Enabled", true);
            Website = Config.Get("Store.Website", "https://uo.com/ultima-store/");

            ResolveCurrency = Config.GetDelegate("Store.ResolveCurrency", (CustomCurrencyHandler)null);

            CurrencyImpl = Config.GetEnum("Store.CurrencyImpl", CurrencyType.Sovereigns);
            CurrencyName = Config.Get("Store.CurrencyName", "Sovereigns");
            CurrencyDisplay = Config.Get("Store.CurrencyDisplay", true);

            PointsImpl = Config.GetEnum("Store.PointsImpl", PointsType.None);

            CostMultiplier = Config.Get("Store.CostMultiplier", 1.0);
            CartCapacity = Config.Get("Store.CartCapacity", 10);
        }

        public static int GetCustomCurrency(Mobile m)
        {
            if (ResolveCurrency != null)
            {
                return ResolveCurrency(m, -1);
            }

            m.SendMessage(1174, "Currency is not set up for this system. Contact a shard administrator.");

            Utility.WriteConsoleColor(ConsoleColor.Red, "[Ultima Store]: No custom currency method has been implemented.");

            return 0;
        }

        public static int DeductCustomCurrecy(Mobile m, int amount)
        {
            if (ResolveCurrency != null)
            {
                return ResolveCurrency(m, amount);
            }

            m.SendMessage(1174, "Currency is not set up for this system. Contact a shard administrator.");

            Utility.WriteConsoleColor(ConsoleColor.Red, "[Ultima Store]: No custom currency deduction method has been implemented.");

            return 0;
        }
    }
}
