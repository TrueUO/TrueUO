using Server.Accounting;
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

                }),
                new RewardList(RewardInterval, 2, new RewardEntry[]
                {
                    
                }),
                new RewardList(RewardInterval, 3, new RewardEntry[]
                {
                    
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
