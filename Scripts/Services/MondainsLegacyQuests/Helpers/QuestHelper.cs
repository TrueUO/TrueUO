using Server.Accounting;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Regions;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests
{
    public class QuestHelper
    {
        public static void RemoveAcceleratedSkillgain(PlayerMobile from)
        {
            Region region = from.Region;

            while (region != null)
            {
                if (region is ApprenticeRegion apprenticeRegion && apprenticeRegion.Table[from] is BuffInfo)
                {
                    BuffInfo.RemoveBuff(from, (BuffInfo)apprenticeRegion.Table[from]);
                    apprenticeRegion.Table[from] = null;
                }

                region = region.Parent;
            }
        }

        public static BaseQuest RandomQuest(PlayerMobile from, Type[] quests, object quester)
        {
            return RandomQuest(from, quests, quester, quests != null && quests.Length == 1);
        }

        public static BaseQuest RandomQuest(PlayerMobile from, Type[] quests, object quester, bool message)
        {
            if (quests == null)
                return null;

            BaseQuest quest = null;

            if (quester is ITierQuester tierQuester)
            {
                quest = TierQuestInfo.RandomQuest(from, tierQuester);
            }
            else if (quests.Length > 0)
            {
                // give it 10 tries to generate quest
                for (int i = 0; i < 10; i++)
                {
                    quest = Construct(quests[Utility.Random(quests.Length)]);
                }
            }

            if (quest != null)
            {
                quest.Owner = from;
                quest.Quester = quester;

                if (CanOffer(from, quest, quester, message))
                {
                    return quest;
                }

                if (quester is Mobile mobile && message)
                {
                    if (mobile is MondainQuester mondainQuester)
                    {
                        mondainQuester.OnOfferFailed();
                    }
                    else
                    {
                        mobile.Say(1080107); // I'm sorry, I have nothing for you at this time.
                    }
                }
            }

            return null;
        }

        public static bool CanOffer(PlayerMobile from, BaseQuest quest, object quester, bool message)
        {
            if (!quest.CanOffer())
            {
                return false;
            }

            if (quest.ChainID != QuestChain.None)
            {
                if (from.Chains.ContainsKey(quest.ChainID) && FirstChainQuest(quest, quest.Quester))
                {
                    return false;
                }

                // if player already has an active quest from the chain
                if (InChainProgress(from, quest))
                {
                    return false;
                }
            }

            // Check if the quest is in any character's log on the account if UseAccountWideQuestRestrictions is true
            if (quest.UseAccountWideQuestRestrictions && IsQuestInAccountQuestLog(from, quest.GetType()))
            {
                if (message && quester is Mobile mobile)
                {
                    mobile.Say("I'm sorry, someone else on your account is already undertaking this quest."); 
                }

                return false;
            }

            if (!Delayed(from, quest, quester, message))
            {
                return false;
            }

            for (int i = quest.Objectives.Count - 1; i >= 0; i--)
            {
                Type type = quest.Objectives[i].Type();

                if (type == null)
                {
                    continue;
                }

                for (int j = from.Quests.Count - 1; j >= 0; j--)
                {
                    BaseQuest pQuest = from.Quests[j];

                    for (int k = pQuest.Objectives.Count - 1; k >= 0; k--)
                    {
                        BaseObjective obj = pQuest.Objectives[k];

                        if (type == obj.Type() && (quest.ChainID == QuestChain.None || quest.ChainID == pQuest.ChainID))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool Delayed(PlayerMobile player, BaseQuest quest, object quester, bool message)
        {
            // Check for account-wide restart delay
            if (quest.UseAccountWideQuestRestrictions && IsQuestInRestartDelayForAccount(player, quest.GetType(), out DateTime? earliestRestartTime))
            {
                if (message && quester is Mobile mobile && earliestRestartTime != null)
                {
                    DisplayDelayMessage(mobile, player, earliestRestartTime.Value);
                }

                return false;
            }

            // Check for character-specific restart delay
            QuestRestartInfo charRestartInfo = GetRestartInfo(player, quest.GetType());

            if (charRestartInfo != null)
            {
                DateTime endTime = charRestartInfo.RestartTime;

                if (DateTime.UtcNow < endTime)
                {
                    if (message && quester is Mobile mobile)
                    {
                        DisplayDelayMessage(mobile, player, endTime);
                    }

                    return false;
                }

                if (quest.RestartDelay > TimeSpan.Zero)
                {
                    player.DoneQuests.Remove(charRestartInfo);
                }
            }

            return true;
        }

        private static void DisplayDelayMessage(Mobile quester, PlayerMobile player, DateTime endTime)
        {
            if (quester != null)
            {
                TimeSpan ts = endTime - DateTime.UtcNow;
                string str;

                if (ts.TotalDays > 1)
                {
                    str = $"I cannot offer this quest again for about {(int)ts.TotalDays} more days.";
                }
                else if (ts.TotalHours > 1)
                {
                    str = $"I cannot offer this quest again for about {(int)ts.TotalHours} more hours.";
                }
                else if (ts.TotalMinutes > 1)
                {
                    str = $"I cannot offer this quest again for about {(int)ts.TotalMinutes} more minutes.";
                }
                else
                {
                    str = "I can offer this quest again very soon.";
                }

                quester.SayTo(player, false, str);
            }
        }

        public static bool IsQuestInRestartDelayForAccount(PlayerMobile player, Type questType, out DateTime? earliestEndTime)
        {
            earliestEndTime = null;

            // Get the account associated with the player
            Account account = player.Account as Account;

            if (account == null)
            {
                return false;
            }

            // Iterate through all characters in the account
            for (int i = 0; i < account.Length; i++)
            {
                Mobile m = account[i];

                if (m is PlayerMobile pm)
                {
                    QuestRestartInfo restartInfo = GetRestartInfo(pm, questType);

                    if (restartInfo != null)
                    {
                        DateTime endTime = restartInfo.RestartTime;

                        if (DateTime.UtcNow < endTime && (earliestEndTime == null || endTime < earliestEndTime))
                        {
                            earliestEndTime = endTime;
                        }
                    }
                }
            }

            return earliestEndTime != null;
        }

        public static bool IsQuestInAccountQuestLog(PlayerMobile player, Type questType)
        {
            Account account = player.Account as Account;

            if (account == null)
            {
                return false;
            }

            // Iterate through all characters in the account
            for (int i = 0; i < account.Length; i++)
            {
                Mobile m = account[i];

                if (m is PlayerMobile pm)
                {
                    foreach (BaseQuest quest in pm.Quests)
                    {
                        if (quest.GetType() == questType)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static QuestRestartInfo GetRestartInfo(PlayerMobile pm, Type quest)
        {
            for (var index = 0; index < pm.DoneQuests.Count; index++)
            {
                var ri = pm.DoneQuests[index];

                if (ri.QuestType == quest)
                {
                    return ri;
                }
            }

            return null;
        }

        public static bool CheckDoneOnce(PlayerMobile player, BaseQuest quest, Mobile quester, bool message)
        {
            return quest.DoneOnce && CheckDoneOnce(player, quest.GetType(), quester, message);
        }

        public static bool CheckDoneOnce(PlayerMobile player, Type questType, Mobile quester, bool message)
        {
            for (var index = 0; index < player.DoneQuests.Count; index++)
            {
                var x = player.DoneQuests[index];

                if (x.QuestType == questType)
                {
                    if (message && quester != null)
                    {
                        quester.SayTo(player, 1075454, 0x3B2); // I can not offer you the quest again.
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool TryReceiveQuestItem(PlayerMobile player, Type type, TimeSpan delay)
        {
            if (type.IsSubclassOf(typeof(Item)))
            {
                QuestRestartInfo info = null;

                for (var index = 0; index < player.DoneQuests.Count; index++)
                {
                    var x = player.DoneQuests[index];

                    if (x.QuestType == type)
                    {
                        info = x;
                        break;
                    }
                }

                if (info != null)
                {
                    DateTime endTime = info.RestartTime;

                    if (DateTime.UtcNow < endTime)
                    {
                        TimeSpan ts = endTime - DateTime.UtcNow;

                        if (ts.Days > 0)
                        {
                            player.SendLocalizedMessage(1158377, $"{ts.Days}\tday[s]");
                        }
                        else if (ts.Hours > 0)
                        {
                            player.SendLocalizedMessage(1158377, $"{ts.Hours}\thour[s]");
                        }
                        else
                        {
                            player.SendLocalizedMessage(1158377, $"{ts.Minutes}\tminute[s]");
                        }

                        return false;
                    }

                    info.Reset(delay);
                }
                else
                {
                    player.DoneQuests.Add(new QuestRestartInfo(type, delay));
                }

                return true;
            }

            return false;
        }

        public static void Delay(PlayerMobile player, Type type, TimeSpan delay)
        {
            QuestRestartInfo restartInfo = GetRestartInfo(player, type);

            if (restartInfo != null)
            {
                restartInfo.Reset(delay);
                return;
            }

            player.DoneQuests.Add(new QuestRestartInfo(type, delay));
        }

        /// <summary>
        /// Called in BaseQuestItem.cs
        /// </summary>
        /// <param name="player"></param>
        /// <param name="quests"></param>
        /// <returns></returns>
        public static bool InProgress(PlayerMobile player, Type[] quests)
        {
            if (quests == null)
            {
                return false;
            }

            BaseQuest quest = null;

            for (var i = 0; i < player.Quests.Count; i++)
            {
                var q = player.Quests[i];

                bool any = false;

                for (var index = 0; index < quests.Length; index++)
                {
                    var questerType = quests[index];

                    if (questerType == q.GetType())
                    {
                        any = true;
                        break;
                    }
                }

                if (any)
                {
                    quest = q;
                    break;
                }
            }

            if (quest != null)
            {
                if (quest.Completed)
                {
                    player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
                }
                else
                {
                    player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
                    quest.InProgress();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Called in MondainQuester.cs
        /// </summary>
        /// <param name="player"></param>
        /// <param name="quester"></param>
        /// <returns></returns>
        public static bool InProgress(PlayerMobile player, Mobile quester)
        {
            BaseQuest quest = null;

            for (var index = 0; index < player.Quests.Count; index++)
            {
                var q = player.Quests[index];

                if (q.QuesterType == quester.GetType())
                {
                    quest = q;
                    break;
                }
            }

            if (quest != null)
            {
                if (quest.Completed)
                {
                    if (quest.Complete == null && !AnyRewards(quest))
                    {
                        if (TryDeleteItems(quest))
                            quest.GiveRewards();
                    }
                    else
                        player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
                }
                else
                {
                    player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
                    quest.InProgress();
                }

                return true;
            }

            return false;
        }

        public static bool AnyRewards(BaseQuest quest)
        {
            for (int i = 0; i < quest.Rewards.Count; i++)
            {
                BaseReward reward = quest.Rewards[i];

                if (reward.Type != null)
                    return true;
            }

            return false;
        }

        public static bool DeliveryArrived(PlayerMobile player, BaseVendor vendor)
        {
            for (int i = 0; i < player.Quests.Count; i++)
            {
                BaseQuest quest = player.Quests[i];

                for (int j = 0; j < quest.Objectives.Count; j++)
                {
                    BaseObjective objective = quest.Objectives[j];

                    if (objective is DeliverObjective deliver && deliver.Update(vendor))
                    {
                        if (quest.Completed)
                        {
                            player.SendLocalizedMessage(1046258, null, 0x23); // Your quest is complete.
                            player.PlaySound(quest.CompleteSound);

                            quest.OnCompleted();

                            if (vendor is MondainQuester quester)
                            {
                                player.SendGump(new MondainQuestGump(player, quest, MondainQuestGump.Section.Complete, false, true, quester));
                            }
                            else
                            {
                                player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public static bool QuestLimitReached(PlayerMobile player)
        {
            if (player.Quests.Count >= 10)
            {
                player.SendLocalizedMessage(1075141); // You are too busy with other tasks at this time.
                return true;
            }

            return false;
        }

        public static bool FirstChainQuest(BaseQuest quest, object quester)
        {
            return quest != null && BaseChain.Chains[(int)quest.ChainID] != null && BaseChain.Chains[(int)quest.ChainID].Length > 0 && BaseChain.Chains[(int)quest.ChainID][0] == quest.GetType();
        }

        public static Type FindFirstChainQuest(BaseQuest quest)
        {
            if (quest == null || quest.ChainID == QuestChain.None || BaseChain.Chains[(int)quest.ChainID] == null || BaseChain.Chains[(int)quest.ChainID].Length == 0)
                return null;

            return BaseChain.Chains[(int)quest.ChainID][0];
        }

        public static bool InChainProgress(PlayerMobile pm, BaseQuest quest)
        {
            for (var index = 0; index < pm.Quests.Count; index++)
            {
                var q = pm.Quests[index];

                if (q.ChainID != QuestChain.None && q.ChainID == quest.ChainID && q.GetType() != quest.GetType())
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ValidateRegion(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            for (var index = 0; index < Region.Regions.Count; index++)
            {
                var r = Region.Regions[index];

                if (r.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public static void CompleteQuest(PlayerMobile from, BaseQuest quest)
        {
            if (quest == null)
                return;

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                BaseObjective obj = quest.Objectives[i];

                obj.Complete();
            }

            from.SendLocalizedMessage(1046258, null, 0x23); // Your quest is complete.
            from.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
            from.PlaySound(quest.CompleteSound);

            quest.OnCompleted();
        }

        public static void DeleteItems(PlayerMobile from, Type itemType, int amount, bool questItem)
        {
            if (from.Backpack == null || itemType == null || amount <= 0)
                return;

            Item[] items = from.Backpack.FindItemsByType(itemType);

            int deleted = 0;

            for (int i = items.Length - 1; i >= 0 && deleted < amount; i--)
            {
                Item item = items[i];

                if (item.QuestItem || !questItem)
                {
                    item.QuestItem = false;

                    if (deleted + item.Amount > amount)
                    {
                        item.Amount -= amount - deleted;
                        deleted += amount - deleted;
                    }
                    else
                    {
                        item.Delete();
                        deleted += item.Amount;
                    }
                }
            }

            if (deleted < amount)
            {
                for (int i = from.Items.Count - 1; i >= 0 && deleted < amount; i--)
                {
                    Item item = from.Items[i];

                    if (item.QuestItem || !questItem)
                    {
                        if (itemType.IsInstanceOfType(item))
                        {
                            deleted += item.Amount;

                            item.Delete();
                        }
                    }
                }
            }
        }

        public static void DeleteItems(BaseQuest quest)
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                BaseObjective objective = quest.Objectives[i];

                DeleteItems(quest.Owner, objective.Type(), objective.MaxProgress, true);
                RemoveStatus(quest.Owner, objective.Type());
            }
        }

        public static bool TryDeleteItems(BaseQuest quest)
        {
            if (quest == null)
                return false;

            bool complete = false;

            for (int i = 0; i < quest.Objectives.Count && !complete; i++)
            {
                if (quest.Objectives[i] is ObtainObjective)
                {
                    ObtainObjective obtain = (ObtainObjective)quest.Objectives[i];

                    if (CountQuestItems(quest.Owner, obtain.Obtain) >= obtain.MaxProgress)
                    {
                        if (!quest.AllObjectives)
                        {
                            complete = true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (quest.Objectives[i] is DeliverObjective)
                {
                    DeliverObjective deliver = (DeliverObjective)quest.Objectives[i];

                    if (quest.StartingItem != null)
                    {
                        continue;
                    }

                    if (deliver.MaxProgress > CountQuestItems(quest.Owner, deliver.Delivery))
                    {
                        quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.
                        deliver.Fail();

                        return false;
                    }
                }
            }

            DeleteItems(quest);

            return true;
        }

        public static int CountQuestItems(PlayerMobile from, Type type)
        {
            int count = 0;

            if (from.Backpack == null)
                return count;

            Item[] items = from.Backpack.FindItemsByType(type);

            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];

                if (item.QuestItem)
                    count += item.Amount;
            }

            return count;
        }

        public static int RemoveStatus(PlayerMobile from, Type type)
        {
            if (type == null)
                return 0;

            Item[] items = from.Backpack.FindItemsByType(type);

            int count = 0;

            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];

                if (item.QuestItem)
                {
                    count += 1;
                    item.QuestItem = false;
                }
            }

            return count;
        }

        public static void RemoveStatus(PlayerMobile from, Item item)
        {
            for (int i = from.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = from.Quests[i];

                for (int j = quest.Objectives.Count - 1; j >= 0; j--)
                {
                    if (quest.Objectives[j] is ObtainObjective)
                    {
                        ObtainObjective obtain = (ObtainObjective)quest.Objectives[j];

                        if (obtain.Obtain != null && obtain.Obtain.IsInstanceOfType(item))
                        {
                            obtain.CurProgress -= item.Amount;
                            item.QuestItem = false;
                            from.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
                            return;
                        }
                    }
                    else if (quest.Objectives[j] is DeliverObjective)
                    {
                        DeliverObjective deliver = (DeliverObjective)quest.Objectives[j];

                        if (deliver.Delivery != null && deliver.Delivery.IsInstanceOfType(item))
                        {
                            from.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.
                            DeleteItems(from, deliver.Delivery, deliver.MaxProgress, false);
                            deliver.Fail();
                            item.Delete();
                            return;
                        }
                    }
                }
            }
        }

        public static void OnKilledBy(BaseCreature killed, Mobile killer)
        {
            if (killer is PlayerMobile pm)
            {
                CheckCreature(pm, killed);
            }
        }

        public static bool CheckCreature(PlayerMobile player, Mobile creature)
        {
            for (int i = player.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = player.Quests[i];

                for (int j = quest.Objectives.Count - 1; j >= 0; j--)
                {
                    if (quest.Objectives[j] is SlayObjective)
                    {
                        SlayObjective slay = (SlayObjective)quest.Objectives[j];

                        if (slay.Update(creature))
                        {
                            if (quest.Completed)
                            {
                                quest.OnCompleted();
                            }
                            else if (slay.Completed)
                            {
                                player.PlaySound(quest.UpdateSound);
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool CheckItem(PlayerMobile player, Item item)
        {
            for (int i = player.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = player.Quests[i];

                for (int j = quest.Objectives.Count - 1; j >= 0; j--)
                {
                    BaseObjective objective = quest.Objectives[j];

                    if (objective is ObtainObjective obtain && obtain.Update(item))
                    {
                        if (quest.Completed)
                        {
                            quest.OnCompleted();
                        }
                        else if (obtain.Completed)
                        {
                            player.PlaySound(quest.UpdateSound);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public static bool CheckRewardItem(PlayerMobile player, Item item)
        {
            for (var index = 0; index < player.Quests.Count; index++)
            {
                BaseQuest quest = player.Quests[index];

                for (var i = 0; i < quest.Objectives.Count; i++)
                {
                    var obj = quest.Objectives[i];

                    if (obj is ObtainObjective)
                    {
                        for (var index1 = 0; index1 < quest.Objectives.Count; index1++)
                        {
                            BaseObjective objective = quest.Objectives[index1];

                            if (objective is ObtainObjective obtain && obtain.IsObjective(item))
                            {
                                obtain.CurProgress += item.Amount;
                                quest.OnObjectiveUpdate(item);
                                return true;
                            }
                        }

                        break;
                    }
                }
            }

            return false;
        }

        public static bool CheckSkill(PlayerMobile player, Skill skill)
        {
            for (int i = player.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = player.Quests[i];

                for (int j = quest.Objectives.Count - 1; j >= 0; j--)
                {
                    BaseObjective objective = quest.Objectives[j];

                    if (objective is ApprenticeObjective apprentice && apprentice.Update(skill))
                    {
                        if (quest.Completed)
                        {
                            quest.OnCompleted();
                        }
                        else if (apprentice.Completed)
                        {
                            player.PlaySound(quest.UpdateSound);
                        }
                    }
                }
            }

            return false;
        }

        public static bool EnhancedSkill(PlayerMobile player, Skill skill)
        {
            if (player == null || player.Region == null || skill == null)
                return false;

            for (int i = player.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = player.Quests[i];

                for (int j = quest.Objectives.Count - 1; j >= 0; j--)
                {
                    BaseObjective objective = quest.Objectives[j];

                    if (objective is ApprenticeObjective apprentice && !apprentice.Completed && apprentice.Region != null)
                    {
                        if (player.Region.IsPartOf(apprentice.Region) && skill.SkillName == apprentice.Skill)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static BaseQuest Construct(Type type)
        {
            if (type == null)
            {
                return null;
            }

            try
            {
                return Activator.CreateInstance(type) as BaseQuest;
            }
            catch
            {
                return null;
            }
        }

        public static void StartTimer(PlayerMobile player)
        {
            if (player == null || player.Quests == null)
                return;

            for (int i = player.Quests.Count - 1; i >= 0; i--)
                player.Quests[i].StartTimer();
        }

        public static void StopTimer(PlayerMobile player)
        {
            if (player == null || player.Quests == null)
                return;

            for (int i = player.Quests.Count - 1; i >= 0; i--)
                player.Quests[i].StopTimer();
        }

        public static void GetContextMenuEntries(List<ContextMenuEntry> list)
        {
            if (list == null)
                return;

            list.Add(new SelectQuestItem());
        }

        public static bool HasQuest<T>(PlayerMobile from) where T : BaseQuest
        {
            return GetQuest(from, typeof(T)) != null;
        }

        public static bool HasQuest(PlayerMobile from, Type t)
        {
            return GetQuest(from, t) != null;
        }

        public static BaseQuest GetQuest(PlayerMobile from, Type type)
        {
            if (type == null)
                return null;

            for (int i = from.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = from.Quests[i];

                if (quest.GetType() == type)
                    return quest;
            }

            return null;
        }

        public static T GetQuest<T>(PlayerMobile pm) where T : BaseQuest
        {
            BaseQuest first = null;

            for (var index = 0; index < pm.Quests.Count; index++)
            {
                var quest = pm.Quests[index];

                if (quest.GetType() == typeof(T))
                {
                    first = quest;
                    break;
                }
            }

            return first as T;
        }

        public static void QuestionQuestCheck(PlayerMobile from)
        {
            for (int i = from.Quests.Count - 1; i >= 0; i--)
            {
                BaseQuest quest = from.Quests[i];

                if (quest.IsQuestionQuest && !quest.Completed)
                    quest.RemoveQuest();
            }
        }
    }

    public class SelectQuestItem : ContextMenuEntry
    {
        public SelectQuestItem()
            : base(6169)
        {
        }

        public override void OnClick()
        {
            if (!Owner.From.Alive)
            {
                return;
            }

            Owner.From.SendLocalizedMessage(1072352); // Target the item you wish to toggle Quest Item status on <ESC> to cancel
            Owner.From.BeginTarget(-1, false, TargetFlags.None, ToggleQuestItem_Callback);
        }

        private void ToggleQuestItem_Callback(Mobile from, object obj)
        {
            if (from is PlayerMobile player)
            {
                if (obj is Item item)
                {
                    if (item.Parent != null && item.Parent == player.Backpack)
                    {
                        if (!QuestHelper.CheckItem(player, item))
                        {
                            player.SendLocalizedMessage(1072355, null, 0x23); // That item does not match any of your quest criteria
                        }
                    }
                    else
                    {
                        player.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
                    }
                }
                else
                {
                    player.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
                }

                player.BeginTarget(-1, false, TargetFlags.None, ToggleQuestItem_Callback);
            }
        }
    }
}
