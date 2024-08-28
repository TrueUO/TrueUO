using Server.Accounting;
using Server.ContextMenus;
using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Banker : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        [Constructable]
        public Banker()
            : base("the banker")
        { }

        public Banker(Serial serial)
            : base(serial)
        { }

        public override NpcGuild NpcGuild => NpcGuild.MerchantsGuild;

        protected override List<SBInfo> SBInfos => m_SBInfos;

        public static int GetBalance(Mobile m)
        {
            double balance = 0;

            if (m.Account != null)
            {
                int goldStub;
                m.Account.GetGoldBalance(out goldStub, out balance);

                if (balance > int.MaxValue)
                {
                    return int.MaxValue;
                }
            }

            return (int)Math.Max(0, Math.Min(int.MaxValue, balance));
        }

        public static bool Withdraw(Mobile from, int amount, bool message = false)
        {
            if (from.Account != null && from.Account.WithdrawGold(amount))
            {
                if (message)
                {
                    from.SendLocalizedMessage(1155856, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold has been removed from your bank box.
                }

                return true;
            }

            return false;
        }

        public static bool Deposit(Mobile from, int amount, bool message = false)
        {
            if (from.Account != null && from.Account.DepositGold(amount))
            {
                if (message)
                {
                    from.SendLocalizedMessage(1042763, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold was deposited in your account.
                }

                return true;
            }

            return false;
        }

        public static int DepositUpTo(Mobile from, int amount, bool message = false)
        {
            if (from.Account != null && from.Account.DepositGold(amount))
            {
                if (message)
                {
                    from.SendLocalizedMessage(1042763, amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))); // ~1_AMOUNT~ gold was deposited in your account.
                }

                return amount;
            }

            return 0;
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBBanker());
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (from.InRange(Location, 12))
            {
                return true;
            }

            return base.HandlesOnSpeech(from);
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            HandleSpeech(this, e);

            base.OnSpeech(e);
        }

        public static void HandleSpeech(Mobile vendor, SpeechEventArgs e)
        {
            if (!e.Handled && e.Mobile.InRange(vendor, 12))
            {
                if (e.Mobile.Map.Rules != MapRules.FeluccaRules && vendor is BaseVendor baseVendor && !baseVendor.CheckVendorAccess(e.Mobile))
                {
                    return;
                }

                for (var index = 0; index < e.Keywords.Length; index++)
                {
                    int keyword = e.Keywords[index];

                    switch (keyword)
                    {
                        case 0x0000: // *withdraw*
                        {
                            e.Handled = true;

                            if (e.Mobile.Criminal)
                            {
                                // I will not do business with a criminal!
                                vendor.Say(500389);
                                break;
                            }

                            string[] split = e.Speech.Split(' ');

                            if (split.Length >= 2)
                            {
                                int amount;

                                Container pack = e.Mobile.Backpack;

                                if (!int.TryParse(split[1], out amount))
                                {
                                    break;
                                }

                                if (amount > 60000)
                                {
                                    // Thou canst not withdraw so much at one time!
                                    vendor.Say(500381);
                                }
                                else if (pack == null || pack.Deleted || !(pack.TotalWeight < pack.MaxWeight) ||
                                         !(pack.TotalItems < pack.MaxItems))
                                {
                                    // Your backpack can't hold anything else.
                                    vendor.Say(1048147);
                                }
                                else if (amount > 0)
                                {
                                    BankBox box = e.Mobile.Player ? e.Mobile.BankBox : e.Mobile.FindBankNoCreate();

                                    if (box == null || !Withdraw(e.Mobile, amount))
                                    {
                                        // Ah, art thou trying to fool me? Thou hast not so much gold!
                                        vendor.Say(500384);
                                    }
                                    else
                                    {
                                        pack.DropItem(new Gold(amount));

                                        // Thou hast withdrawn gold from thy account.
                                        vendor.Say(1010005);
                                    }
                                }
                            }
                        }
                            break;
                        case 0x0001: // *balance*
                        {
                            e.Handled = true;

                            if (e.Mobile.Criminal)
                            {
                                // I will not do business with a criminal!
                                vendor.Say(500389);
                                break;
                            }

                            if (e.Mobile.Account is Account account)
                            {
                                vendor.Say(1155855, $"{account.TotalPlat:#,0}\t{account.TotalGold:#,0}", 0x3BC);
                                vendor.Say(1155848, $"{account.GetSecureAccountAmount(e.Mobile):#,0}", 0x3BC);
                            }
                        }
                            break;
                        case 0x0002: // *bank*
                        {
                            e.Handled = true;

                            if (e.Mobile.Criminal)
                            {
                                // Thou art a criminal and cannot access thy bank box.
                                vendor.Say(500378);
                                break;
                            }

                            e.Mobile.BankBox.Open();
                        }
                            break;
                    }
                }
            }
        }

        public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
        {
            if (from.Alive)
            {
                OpenBankEntry entry = new OpenBankEntry(this)
                {
                    Enabled = from.Map.Rules == MapRules.FeluccaRules || CheckVendorAccess(from)
                };

                list.Add(entry);
            }

            base.AddCustomContextEntries(from, list);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
