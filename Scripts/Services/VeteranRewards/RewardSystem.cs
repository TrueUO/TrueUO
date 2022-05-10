using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.VeteranRewards
{
    public class RewardSystem
    {
        public static bool Enabled = Config.Get("VetRewards.Enabled", true);
        public static int SkillCap = Config.Get("PlayerCaps.TotalSkillCap", 7000);
        public static int SkillCapBonus = Config.Get("VetRewards.SkillCapBonus", 200);
        public static int SkillCapBonusLevels = Config.Get("VetRewards.SkillCapBonusLevels", 4);
        public static TimeSpan RewardInterval = Config.Get("VetRewards.RewardInterval", TimeSpan.FromDays(30.0d));
        public static int StartingLevel = Config.Get("VetRewards.StartingLevel", 0);

        private static RewardCategory[] m_Categories;
        private static RewardList[] m_Lists;
        public static RewardCategory[] Categories
        {
            get
            {
                if (m_Categories == null)
                    SetupRewardTables();

                return m_Categories;
            }
        }
        public static RewardList[] Lists
        {
            get
            {
                if (m_Lists == null)
                    SetupRewardTables();

                return m_Lists;
            }
        }
        public static bool HasAccess(Mobile mob, RewardCategory category)
        {
            List<RewardEntry> entries = category.Entries;

            for (int j = 0; j < entries.Count; ++j)
            {
                //RewardEntry entry = entries[j];
                if (HasAccess(mob, entries[j]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasAccess(Mobile mob, RewardEntry entry)
        {
            TimeSpan ts;
            return HasAccess(mob, entry.List, out ts);
        }

        public static bool HasAccess(Mobile mob, RewardList list, out TimeSpan ts)
        {
            if (list == null)
            {
                ts = TimeSpan.Zero;
                return false;
            }

            Account acct = mob.Account as Account;

            if (acct == null)
            {
                ts = TimeSpan.Zero;
                return false;
            }

            TimeSpan totalTime = (DateTime.UtcNow - acct.Created) + TimeSpan.FromDays(RewardInterval.TotalDays * StartingLevel);

            ts = (list.Age - totalTime);


            if (ts <= TimeSpan.Zero)
                return true;

            return false;
        }

        public static int GetRewardLevel(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
                return 0;

            return GetRewardLevel(acct);
        }

        public static int GetRewardLevel(Account acct)
        {
            TimeSpan totalTime = (DateTime.UtcNow - acct.Created);
            TimeSpan ositotalTime = (DateTime.UtcNow - new DateTime(1997, 9, 24));

            int level = (int)(totalTime.TotalDays / RewardInterval.TotalDays);
            int levelosi = (int)(ositotalTime.TotalDays / 365);

            if (level < 0)
                level = 0;

            level += StartingLevel;

            return Math.Min(level, levelosi);
        }

        public static bool HasHalfLevel(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
                return false;

            TimeSpan totalTime = (DateTime.UtcNow - acct.Created);

            double level = (totalTime.TotalDays / RewardInterval.TotalDays);

            return level >= 0.5;
        }

        public static bool ConsumeRewardPoint(Mobile mob)
        {
            int cur, max;

            ComputeRewardInfo(mob, out cur, out max);

            if (cur >= max)
                return false;

            Account acct = mob.Account as Account;

            if (acct == null)
                return false;

            //if ( mob.AccessLevel < AccessLevel.GameMaster )
            acct.SetTag("numRewardsChosen", (cur + 1).ToString());

            return true;
        }

        public static void ComputeRewardInfo(Mobile mob, out int cur, out int max)
        {
            int level;

            ComputeRewardInfo(mob, out cur, out max, out level);
        }

        public static void ComputeRewardInfo(Mobile mob, out int cur, out int max, out int level)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
            {
                cur = max = level = 0;
                return;
            }

            level = GetRewardLevel(acct);

            if (level == 0)
            {
                cur = max = 0;
                return;
            }

            string tag = acct.GetTag("numRewardsChosen");

            if (string.IsNullOrEmpty(tag))
                cur = 0;
            else
                cur = Utility.ToInt32(tag);

            if (level >= 6)
                max = 9 + ((level - 6) * 2);
            else
                max = 2 + level;
        }

        public static int GetRewardYearLabel(Item item, object[] args)
        {
            int level = GetRewardYear(item, args);

            return 1076216 + ((level < 10) ? level : (level < 12) ? ((level - 9) + 4240) : ((level - 11) + 37585));
        }

        public static int GetRewardYear(Item item, object[] args)
        {
            if (m_Lists == null)
                SetupRewardTables();

            Type type = item.GetType();

            for (int i = 0; i < m_Lists.Length; ++i)
            {
                RewardList list = m_Lists[i];
                RewardEntry[] entries = list.Entries;

                for (int j = 0; j < entries.Length; ++j)
                {
                    if (entries[j].ItemType == type)
                    {
                        if (args == null && entries[j].Args.Length == 0)
                            return i + 1;

                        if (args.Length == entries[j].Args.Length)
                        {
                            bool match = true;

                            for (int k = 0; match && k < args.Length; ++k)
                                match = (args[k].Equals(entries[j].Args[k]));

                            if (match)
                                return i + 1;
                        }
                    }
                }
            }

            // no entry?
            return 0;
        }

        public static void SetupRewardTables()
        {
            RewardCategory monsterStatues = new RewardCategory(1159427); // Statuettes
            RewardCategory cloaksAndRobes = new RewardCategory(1159428); // Equippables
            RewardCategory etherealSteeds = new RewardCategory(1049751); // Ethereal Steeds
            RewardCategory specialDyeTubs = new RewardCategory(1049753); // Dye Tubs
            RewardCategory houseAddOns = new RewardCategory(1049754); // House Add-Ons
            RewardCategory miscellaneous = new RewardCategory(1011173); // Miscellaneous

            m_Categories = new RewardCategory[]
            {
                monsterStatues,
                cloaksAndRobes,
                etherealSteeds,
                specialDyeTubs,
                houseAddOns,
                miscellaneous
            };

            m_Lists = new RewardList[]
            {
                new RewardList(RewardInterval, 1, new RewardEntry[]
                {
                    new RewardEntry(specialDyeTubs, 1006008, typeof(RewardBlackDyeTub)),
                    new RewardEntry(specialDyeTubs, 1006013, typeof(FurnitureDyeTub)),
                    new RewardEntry(specialDyeTubs, 1006047, typeof(SpecialDyeTub)),

                    new RewardEntry(monsterStatues, 1006024, typeof(MonsterStatuette), MonsterStatuetteType.Crocodile),
                    new RewardEntry(monsterStatues, 1006025, typeof(MonsterStatuette), MonsterStatuetteType.Daemon),
                    new RewardEntry(monsterStatues, 1006026, typeof(MonsterStatuette), MonsterStatuetteType.Dragon),
                    new RewardEntry(monsterStatues, 1006027, typeof(MonsterStatuette), MonsterStatuetteType.EarthElemental),
                    new RewardEntry(monsterStatues, 1006028, typeof(MonsterStatuette), MonsterStatuetteType.Ettin),
                    new RewardEntry(monsterStatues, 1006029, typeof(MonsterStatuette), MonsterStatuetteType.Gargoyle),
                    new RewardEntry(monsterStatues, 1006030, typeof(MonsterStatuette), MonsterStatuetteType.Gorilla),
                    new RewardEntry(monsterStatues, 1006031, typeof(MonsterStatuette), MonsterStatuetteType.Lich),
                    new RewardEntry(monsterStatues, 1006032, typeof(MonsterStatuette), MonsterStatuetteType.Lizardman),
                    new RewardEntry(monsterStatues, 1006033, typeof(MonsterStatuette), MonsterStatuetteType.Ogre),
                    new RewardEntry(monsterStatues, 1006034, typeof(MonsterStatuette), MonsterStatuetteType.Orc),
                    new RewardEntry(monsterStatues, 1006035, typeof(MonsterStatuette), MonsterStatuetteType.Ratman),
                    new RewardEntry(monsterStatues, 1006036, typeof(MonsterStatuette), MonsterStatuetteType.Skeleton),
                    new RewardEntry(monsterStatues, 1006037, typeof(MonsterStatuette), MonsterStatuetteType.Troll),
                    new RewardEntry(monsterStatues, 1155746, typeof(MonsterStatuette), MonsterStatuetteType.FleshRenderer),
                    new RewardEntry(monsterStatues, 1156367, typeof(MonsterStatuette), MonsterStatuetteType.DragonTurtle),
                    new RewardEntry(monsterStatues, 1158875, typeof(MonsterStatuette), MonsterStatuetteType.Krampus),
                    new RewardEntry(monsterStatues, 1159417, typeof(MonsterStatuette), MonsterStatuetteType.Pig),

                    new RewardEntry(etherealSteeds, 1006019, typeof(EtherealHorse)),

                    new RewardEntry(houseAddOns,    1062692, typeof(ContestMiniHouseDeed), MiniHouseType.MalasMountainPass),
                    new RewardEntry(houseAddOns,    1072216, typeof(ContestMiniHouseDeed), MiniHouseType.ChurchAtNight),

                    new RewardEntry(miscellaneous,  1076155, typeof(RedSoulstone)),
                    new RewardEntry(miscellaneous,  1080523, typeof(CommodityDeedBox)),
                    new RewardEntry(miscellaneous,  1113945,  typeof(CrystalPortal)),
                    new RewardEntry(miscellaneous,  1150074,  typeof(CorruptedCrystalPortal)),

                    new RewardEntry(miscellaneous,    1123603,  typeof(CoralTheOwl)),
                    new RewardEntry(miscellaneous,    1151769,  typeof(GreaterBraceletOfBinding)),
                    new RewardEntry(miscellaneous,    1156371,  typeof(Auction.AuctionSafeDeed))
                }),
                new RewardList(RewardInterval, 2, new RewardEntry[]
                {
                    new RewardEntry(specialDyeTubs, 1006052, typeof(LeatherDyeTub)),

                    new RewardEntry(monsterStatues, 1155747, typeof(MonsterStatuette), MonsterStatuetteType.CrystalElemental),
                    new RewardEntry(monsterStatues, 1157078, typeof(MonsterStatuette), MonsterStatuetteType.TRex),
                    new RewardEntry(monsterStatues, 1158877, typeof(MonsterStatuette), MonsterStatuetteType.KhalAnkur),
                    new RewardEntry(monsterStatues, 1159418, typeof(MonsterStatuette), MonsterStatuetteType.Goat),

                    new RewardEntry(houseAddOns,    1006048, typeof(BannerDeed)),
                    new RewardEntry(houseAddOns,    1006049, typeof(FlamingHeadDeed)),
                    new RewardEntry(houseAddOns,    1080409, typeof(MinotaurStatueDeed))
                }),
                new RewardList(RewardInterval, 3, new RewardEntry[]
                {
                    new RewardEntry(monsterStatues, 1006038, typeof(MonsterStatuette), MonsterStatuetteType.Cow),
                    new RewardEntry(monsterStatues, 1006039, typeof(MonsterStatuette), MonsterStatuetteType.Zombie),
                    new RewardEntry(monsterStatues, 1006040, typeof(MonsterStatuette), MonsterStatuetteType.Llama),
                    new RewardEntry(monsterStatues, 1155748, typeof(MonsterStatuette), MonsterStatuetteType.DarkFather),
                    new RewardEntry(monsterStatues, 1157079, typeof(MonsterStatuette), MonsterStatuetteType.Zipactriotal),
                    new RewardEntry(monsterStatues, 1158876, typeof(MonsterStatuette), MonsterStatuetteType.KrampusMinion),
                    new RewardEntry(monsterStatues, 1159419, typeof(MonsterStatuette), MonsterStatuetteType.IceFiend),

                    new RewardEntry(etherealSteeds, 1006051, typeof(EtherealLlama)),
                    new RewardEntry(etherealSteeds, 1006050, typeof(EtherealOstard)),

                    new RewardEntry(houseAddOns,    1080407, typeof(PottedCactusDeed))
                }),
                new RewardList(RewardInterval, 4, new RewardEntry[]
                {
                    new RewardEntry(specialDyeTubs, 1049740, typeof(RunebookDyeTub)),

                    new RewardEntry(monsterStatues, 1049742, typeof(MonsterStatuette), MonsterStatuetteType.Ophidian),
                    new RewardEntry(monsterStatues, 1049743, typeof(MonsterStatuette), MonsterStatuetteType.Reaper),
                    new RewardEntry(monsterStatues, 1049744, typeof(MonsterStatuette), MonsterStatuetteType.Mongbat),
                    new RewardEntry(monsterStatues, 1155745, typeof(MonsterStatuette), MonsterStatuetteType.PlatinumDragon),
                    new RewardEntry(monsterStatues, 1157993, typeof(MonsterStatuette), MonsterStatuetteType.Pyros),
                    new RewardEntry(monsterStatues, 1157994, typeof(MonsterStatuette), MonsterStatuetteType.Lithos),
                    new RewardEntry(monsterStatues, 1157992, typeof(MonsterStatuette), MonsterStatuetteType.Hydros),
                    new RewardEntry(monsterStatues, 1157991, typeof(MonsterStatuette), MonsterStatuetteType.Stratos),

                    new RewardEntry(etherealSteeds, 1049746, typeof(EtherealKirin)),
                    new RewardEntry(etherealSteeds, 1049745, typeof(EtherealUnicorn)),
                    new RewardEntry(etherealSteeds, 1049747, typeof(EtherealRidgeback)),

                    new RewardEntry(houseAddOns,    1049737, typeof(DecorativeShieldDeed)),
                    new RewardEntry(houseAddOns,    1049738, typeof(HangingSkeletonDeed)),

                    new RewardEntry(miscellaneous,  1098160, typeof(Plants.SeedBox)),
                    new RewardEntry(miscellaneous,  1158880, typeof(EmbroideryTool))
                }),
                new RewardList(RewardInterval, 5, new RewardEntry[]
                {
                    new RewardEntry(specialDyeTubs, 1049741, typeof(StatuetteDyeTub)),
                    new RewardEntry(specialDyeTubs, 1153495, typeof(MetallicLeatherDyeTub)),
                    new RewardEntry(specialDyeTubs, 1150067, typeof(MetallicDyeTub)),

                    new RewardEntry(monsterStatues, 1049768, typeof(MonsterStatuette), MonsterStatuetteType.Gazer),
                    new RewardEntry(monsterStatues, 1049769, typeof(MonsterStatuette), MonsterStatuetteType.FireElemental),
                    new RewardEntry(monsterStatues, 1049770, typeof(MonsterStatuette), MonsterStatuetteType.Wolf),
                    new RewardEntry(monsterStatues, 1157080, typeof(MonsterStatuette), MonsterStatuetteType.MyrmidexQueen),

                    new RewardEntry(etherealSteeds, 1049749, typeof(EtherealSwampDragon)),
                    new RewardEntry(etherealSteeds, 1049748, typeof(EtherealBeetle)),
                    new RewardEntry(houseAddOns,    1049739, typeof(StoneAnkhDeed)),
                    new RewardEntry(houseAddOns,    1080384, typeof(BloodyPentagramDeed)),
                    new RewardEntry(houseAddOns,    1154582, typeof(LighthouseAddonDeed)),
                    new RewardEntry(houseAddOns,    1158860, typeof(RepairBenchDeed))
                }),
                new RewardList(RewardInterval, 6, new RewardEntry[]
                {
                    new RewardEntry(houseAddOns,    1076188, typeof(CharacterStatueMaker), StatueType.Jade),
                    new RewardEntry(houseAddOns,    1076189, typeof(CharacterStatueMaker), StatueType.Marble),
                    new RewardEntry(houseAddOns,    1076190, typeof(CharacterStatueMaker), StatueType.Bronze),
                    new RewardEntry(houseAddOns,    1080527, typeof(RewardBrazierDeed))
                }),
                new RewardList(RewardInterval, 7, new RewardEntry[]
                {
                    new RewardEntry(houseAddOns,    1076157, typeof(CannonDeed)),
                    new RewardEntry(houseAddOns,    1080550, typeof(TreeStumpDeed)),
                    new RewardEntry(houseAddOns,    1151835, typeof(SheepStatueDeed)),
                    new RewardEntry(houseAddOns,    1123504, typeof(SewingMachineDeed)),
                    new RewardEntry(houseAddOns,    1123577, typeof(SmithingPressDeed)),
                    new RewardEntry(houseAddOns,    1156369, typeof(SpinningLatheDeed)),
                    new RewardEntry(houseAddOns,    1156370, typeof(FletchingStationDeed)),
                    new RewardEntry(houseAddOns,    1157071, typeof(BBQSmokerDeed)),
                    new RewardEntry(houseAddOns,    1157070, typeof(AlchemyStationDeed)),
                    new RewardEntry(houseAddOns,    1157989, typeof(WritingDeskDeed)),
                    new RewardEntry(houseAddOns,    1125529, typeof(TinkerBenchDeed))
                }),
                new RewardList(RewardInterval, 8, new RewardEntry[]
                {
                    new RewardEntry(miscellaneous,  1076158, typeof(WeaponEngravingTool)),
                    new RewardEntry(houseAddOns,   1153535, typeof(DaviesLockerAddonDeed)),
                    new RewardEntry(houseAddOns,   1154583, typeof(GadgetryTableAddonDeed))
                }),
                new RewardList(RewardInterval, 9, new RewardEntry[]
                {
                    new RewardEntry(monsterStatues, 1153592, typeof(MonsterStatuette), MonsterStatuetteType.Virtuebane),
                    new RewardEntry(etherealSteeds, 1076159, typeof(RideablePolarBear)),
                    new RewardEntry(houseAddOns,    1080549, typeof(WallBannerDeed))
                }),
                new RewardList(RewardInterval, 10, new RewardEntry[]
                {
                    new RewardEntry(monsterStatues, 1080520, typeof(MonsterStatuette), MonsterStatuetteType.Harrower),
                    new RewardEntry(monsterStatues, 1080521, typeof(MonsterStatuette), MonsterStatuetteType.Efreet),

                    new RewardEntry(etherealSteeds, 1080386, typeof(EtherealCuSidhe)),

                    new RewardEntry(houseAddOns,    1080548, typeof(MiningCartDeed)),
                    new RewardEntry(houseAddOns,    1080397, typeof(AnkhOfSacrificeDeed)),
                    new RewardEntry(houseAddOns,    1150120, typeof(SkullRugAddonDeed)),
                    new RewardEntry(houseAddOns,    1150121, typeof(RoseRugAddonDeed)),
                    new RewardEntry(houseAddOns,    1150122, typeof(DolphinRugAddonDeed)),
                    new RewardEntry(houseAddOns,    1157996, typeof(KoiPondDeed)),
                    new RewardEntry(houseAddOns,    1158881, typeof(WaterWheelDeed))
                }),
                new RewardList(RewardInterval, 11, new RewardEntry[]
                {
                    new RewardEntry(etherealSteeds, 1113908, typeof(EtherealReptalon)),

                    new RewardEntry(monsterStatues, 1113800, typeof(MonsterStatuette), MonsterStatuetteType.TerathanMatriarch),
                    new RewardEntry(monsterStatues, 1153593, typeof(MonsterStatuette), MonsterStatuetteType.Navrey),

                    new RewardEntry(miscellaneous,  1113814, typeof(EtherealRetouchingTool))
                }),
                new RewardList(RewardInterval, 12, new RewardEntry[]
                {
                    new RewardEntry(etherealSteeds, 1113813, typeof(EtherealHiryu)),

                    new RewardEntry(monsterStatues, 1113801, typeof(MonsterStatuette), MonsterStatuetteType.FireAnt),

                    new RewardEntry(houseAddOns,    1113954, typeof(AllegiancePouch))
                }),
                new RewardList(RewardInterval, 13, new RewardEntry[]
                {
                    new RewardEntry(etherealSteeds, 1150006, typeof(EtherealBoura)),
                    new RewardEntry(monsterStatues, 1153594, typeof(MonsterStatuette), MonsterStatuetteType.Exodus)
                }),
                new RewardList(RewardInterval, 15, new RewardEntry[]
                {
                    new RewardEntry(etherealSteeds, 1154589, typeof(EtherealTiger)),
                    new RewardEntry(etherealSteeds, 1155723, typeof(EtherealAncientHellHound)),
                    new RewardEntry(etherealSteeds, 1157081, typeof(EtherealTarantula)),
                    new RewardEntry(etherealSteeds, 1157081, typeof(EtherealWarBoar)),

                    new RewardEntry(houseAddOns,    1153491, typeof(GardenShedDeed))
                }),
                new RewardList(RewardInterval, 20, new RewardEntry[]
                {
                    new RewardEntry(etherealSteeds, 1157995, typeof(EtherealSerpentineDragon))
                })
            };
        }

        public static void Initialize()
        {
            if (Enabled)
                EventSink.Login += EventSink_Login;
        }

        private static void EventSink_Login(LoginEventArgs e)
        {
            if (!e.Mobile.Alive)
                return;

            int cur, max, level;

            ComputeRewardInfo(e.Mobile, out cur, out max, out level);

            if (level > SkillCapBonusLevels)
                level = SkillCapBonusLevels;
            else if (level < 0)
                level = 0;

            e.Mobile.SkillsCap = SkillCap + SkillCapBonus;

            if (e.Mobile is PlayerMobile && !((PlayerMobile)e.Mobile).HasStatReward && HasHalfLevel(e.Mobile))
            {
                Gumps.BaseGump.SendGump(new StatRewardGump((PlayerMobile)e.Mobile));
            }

            if (cur < max)
                e.Mobile.SendGump(new RewardNoticeGump(e.Mobile));
        }
    }
}
