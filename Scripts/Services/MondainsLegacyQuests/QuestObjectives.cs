using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests
{
    public class BaseObjective
    {
        private BaseQuest m_Quest;
        private int m_MaxProgress;
        private int m_CurProgress;
        private int m_Seconds;
        private bool m_Timed;
        public BaseObjective()
            : this(1, 0)
        {
        }

        public BaseObjective(int maxProgress)
            : this(maxProgress, 0)
        {
        }

        public BaseObjective(int maxProgress, int seconds)
        {
            m_MaxProgress = maxProgress;
            m_Seconds = seconds;

            if (seconds > 0)
                Timed = true;
            else
                Timed = false;
        }

        public BaseQuest Quest { get => m_Quest; set => m_Quest = value; }

        public int MaxProgress { get => m_MaxProgress; set => m_MaxProgress = value; }

        public int CurProgress
        {
            get => m_CurProgress;
            set
            {
                m_CurProgress = value;

                if (Completed)
                    OnCompleted();

                if (m_CurProgress == -1)
                    OnFailed();

                if (m_CurProgress < -1)
                    m_CurProgress = -1;
            }
        }
        public int Seconds
        {
            get => m_Seconds;
            set
            {
                m_Seconds = value;

                if (m_Seconds < 0)
                    m_Seconds = 0;
            }
        }

        public bool Timed { get => m_Timed; set => m_Timed = value; }

        public bool Completed => CurProgress >= MaxProgress;
        public bool Failed => CurProgress == -1;

        public virtual object ObjectiveDescription => null;

        public virtual void Complete()
        {
            CurProgress = MaxProgress;
        }

        public virtual void Fail()
        {
            CurProgress = -1;
        }

        public virtual void OnAccept()
        {
        }

        public virtual void OnCompleted()
        {
        }

        public virtual void OnFailed()
        {
        }

        public virtual Type Type()
        {
            return null;
        }

        public virtual bool Update(object obj)
        {
            return false;
        }

        public virtual void UpdateTime()
        {
            if (!Timed || Failed)
                return;

            if (Seconds > 0)
            {
                Seconds -= 1;
            }
            else if (!Completed)
            {
                m_Quest.Owner.SendLocalizedMessage(1072258); // You failed to complete an objective in time!

                Fail();
            }
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.Write(m_CurProgress);
            writer.Write(m_Seconds);
        }

        public virtual void Deserialize(GenericReader reader)
        {
            reader.ReadEncodedInt();

            m_CurProgress = reader.ReadInt();
            m_Seconds = reader.ReadInt();
        }
    }

    public class SlayObjective : BaseObjective
    {
        public SlayObjective(Type creature, string name, int amount)
            : this(new[] { creature }, name, amount, 0, null, 0)
        {
        }

        public SlayObjective(Type creature, string name, int amount, int label, string region)
            : this(new[] { creature }, name, amount, label, region, 0)
        {
        }

        public SlayObjective(Type creature, string name, int amount, string region)
            : this(new[] { creature }, name, amount, 0, region, 0)
        {
        }

        public SlayObjective(Type creature, string name, int amount, int seconds)
            : this(new[] { creature }, name, amount, 0, null, seconds)
        {
        }

        public SlayObjective(string name, int amount, params Type[] creatures)
            : this(creatures, name, amount, 0, null, 0)
        {
        }

        public SlayObjective(string name, int amount, string region, params Type[] creatures)
            : this(creatures, name, amount, 0, region, 0)
        {
        }

        public SlayObjective(string name, int amount, int seconds, params Type[] creatures)
            : this(creatures, name, amount, 0, null, seconds)
        {
        }

        public SlayObjective(Type[] creatures, string name, int amount, int label, string region, int seconds)
            : base(amount, seconds)
        {
            Creatures = creatures;
            Name = name;
            Label = label;

            if (region != null)
            {
                Region = QuestHelper.ValidateRegion(region) ? region : null;

                if (Region == null)
                    Console.WriteLine("Invalid region name ('{0}') in '{1}' objective!", region, GetType());
            }
        }

        public Type[] Creatures { get; }
        public string Name { get; }
        public string Region { get; }
        public int Label { get; }

        public virtual void OnKill(Mobile killed)
        {
            if (Completed)
                Quest.Owner.SendLocalizedMessage(1075050); // You have killed all the required quest creatures of this type.
            else
                Quest.Owner.SendLocalizedMessage(1075051, (MaxProgress - CurProgress).ToString()); // You have killed a quest creature. ~1_val~ more left.
        }

        public virtual bool IsObjective(Mobile mob)
        {
            if (Creatures == null)
            {
                return false;
            }

            for (int index = 0; index < Creatures.Length; index++)
            {
                Type type = Creatures[index];

                if (type.IsInstanceOfType(mob))
                {
                    if (Region != null && !mob.Region.IsPartOf(Region))
                    {
                        return false;
                    }

                    if (mob is BaseCreature bc && bc.Controlled)
                    {
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        public override bool Update(object obj)
        {
            if (obj is Mobile mob && IsObjective(mob))
            {
                if (!Completed)
                {
                    CurProgress += 1;
                }

                OnKill(mob);
                return true;
            }

            return false;
        }

        public override Type Type()
        {
            return Creatures != null && Creatures.Length > 0 ? Creatures[0] : null;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class ObtainObjective : BaseObjective
    {
        private Type m_Obtain;
        private string m_Name;
        private int m_Image;
        private int m_Hue;

        public ObtainObjective(Type obtain, string name, int amount)
            : this(obtain, name, amount, 0, 0)
        {
        }

        public ObtainObjective(Type obtain, string name, int amount, int image)
            : this(obtain, name, amount, image, 0)
        {
        }

        public ObtainObjective(Type obtain, string name, int amount, int image, int seconds)
            : this(obtain, name, amount, image, seconds, 0)
        {
        }

        public ObtainObjective(Type obtain, string name, int amount, int image, int seconds, int hue)
            : base(amount, seconds)
        {
            m_Obtain = obtain;
            m_Name = name;
            m_Image = image;
            m_Hue = hue;
        }

        public Type Obtain { get => m_Obtain; set => m_Obtain = value; }

        public string Name { get => m_Name; set => m_Name = value; }
        public int Image { get => m_Image; set => m_Image = value; }
        public int Hue { get => m_Hue; set => m_Hue = value; }

        public override bool Update(object obj)
        {
            if (obj is Item obtained && IsObjective(obtained))
            {
                if (!obtained.QuestItem)
                {
                    CurProgress += obtained.Amount;

                    obtained.QuestItem = true;
                    Quest.Owner.SendLocalizedMessage(1072353); // You set the item to Quest Item status

                    Quest.OnObjectiveUpdate(obtained);
                }
                else
                {
                    CurProgress -= obtained.Amount;

                    obtained.QuestItem = false;
                    Quest.Owner.SendLocalizedMessage(1072354); // You remove Quest Item status from the item
                }

                return true;
            }

            return false;
        }

        public virtual bool IsObjective(Item item)
        {
            if (m_Obtain == null)
                return false;

            if (m_Obtain.IsInstanceOfType(item))
                return true;

            return false;
        }

        public override Type Type()
        {
            return m_Obtain;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class DeliverObjective : BaseObjective
    {
        private Type m_Delivery;
        private string m_DeliveryName;
        private Type m_Destination;
        private string m_DestName;

        public DeliverObjective(Type delivery, string deliveryName, int amount, Type destination, string destName)
            : this(delivery, deliveryName, amount, destination, destName, 0)
        {
        }

        public DeliverObjective(Type delivery, string deliveryName, int amount, Type destination, string destName, int seconds)
            : base(amount, seconds)
        {
            m_Delivery = delivery;
            m_DeliveryName = deliveryName;

            m_Destination = destination;
            m_DestName = destName;
        }

        public Type Delivery { get => m_Delivery; set => m_Delivery = value; }
        public string DeliveryName { get => m_DeliveryName; set => m_DeliveryName = value; }
        public Type Destination { get => m_Destination; set => m_Destination = value; }
        public string DestName { get => m_DestName; set => m_DestName = value; }

        public override void OnAccept()
        {
            if (Quest.StartingItem != null)
            {
                Quest.StartingItem.QuestItem = true;
                return;
            }

            int amount = MaxProgress;

            while (amount > 0 && !Failed)
            {
                Item item = Loot.Construct(m_Delivery);

                if (item == null)
                {
                    Fail();
                    break;
                }

                if (item.Stackable)
                {
                    item.Amount = amount;
                    amount = 1;
                }

                if (!Quest.Owner.PlaceInBackpack(item))
                {
                    Quest.Owner.SendLocalizedMessage(503200); // You do not have room in your backpack for 
                    Quest.Owner.SendLocalizedMessage(1075574); // Could not create all the necessary items. Your quest has not advanced.

                    Fail();

                    break;
                }

                item.QuestItem = true;

                amount -= 1;
            }

            if (Failed)
            {
                QuestHelper.DeleteItems(Quest.Owner, m_Delivery, MaxProgress - amount, false);

                Quest.RemoveQuest();
            }
        }

        public override bool Update(object obj)
        {
            if (m_Delivery == null || m_Destination == null)
                return false;

            if (Failed)
            {
                Quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.
                return false;
            }

            if (obj is BaseVendor)
            {
                if (Quest.StartingItem != null)
                {
                    Complete();
                    return true;
                }

                if (m_Destination.IsInstanceOfType(obj))
                {
                    if (MaxProgress < QuestHelper.CountQuestItems(Quest.Owner, Delivery))
                    {
                        Quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.						
                        Fail();
                    }
                    else
                        Complete();

                    return true;
                }
            }

            return false;
        }

        public override Type Type()
        {
            return m_Delivery;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class EscortObjective : BaseObjective
    {
        public string Region { get; }
        public int Fame { get; }
        public int Compassion { get; }
        public int Label { get; }

        public EscortObjective(string region)
            : this(region, 10, 200, 0, 0)
        {
        }

        public EscortObjective(int label, string region)
            : this(region, 10, 200, 0, label)
        {
        }

        public EscortObjective(string region, int fame)
            : this(region, fame, 200)
        {
        }

        public EscortObjective(string region, int fame, int compassion)
            : this(region, fame, compassion, 0, 0)
        {
        }

        public EscortObjective(string region, int fame, int compassion, int seconds, int label)
            : base(1, seconds)
        {
            if (region != null)
            {
                Region = QuestHelper.ValidateRegion(region) ? region : null;

                if (Region == null)
                    Console.WriteLine("Invalid region name ('{0}') in '{1}' objective!", region, GetType());
            }

            Fame = fame;
            Compassion = compassion;
            Label = label;
        }

        public override void OnCompleted()
        {
            QuestRestartInfo first = null;

            for (var index = 0; index < Quest.Owner.DoneQuests.Count; index++)
            {
                var info = Quest.Owner.DoneQuests[index];

                if (info.QuestType == typeof(ResponsibilityQuest))
                {
                    first = info;
                    break;
                }
            }

            if (Quest != null && Quest.Owner != null && Quest.Owner.Murderer && first == null)
            {
                QuestHelper.Delay(Quest.Owner, typeof(ResponsibilityQuest), Quest.RestartDelay);
            }

            base.OnCompleted();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class ApprenticeObjective : BaseObjective
    {
        private SkillName m_Skill;
        private string m_Region;
        private object m_Enter;
        private object m_Leave;

        public ApprenticeObjective(SkillName skill, int cap)
            : this(skill, cap, null, null, null)
        {
        }

        public ApprenticeObjective(SkillName skill, int cap, string region, object enterRegion, object leaveRegion)
            : base(cap)
        {
            m_Skill = skill;

            if (region != null)
            {
                m_Region = QuestHelper.ValidateRegion(region) ? region : null;
                m_Enter = enterRegion;
                m_Leave = leaveRegion;

                if (m_Region == null)
                    Console.WriteLine("Invalid region name ('{0}') in '{1}' objective!", region, GetType());
            }
        }

        public SkillName Skill { get => m_Skill; set => m_Skill = value; }

        public string Region { get => m_Region; set => m_Region = value; }
        public object Enter { get => m_Enter; set => m_Enter = value; }
        public object Leave { get => m_Leave; set => m_Leave = value; }

        public override bool Update(object obj)
        {
            if (Completed)
                return false;

            if (obj is Skill skill)
            {
                if (skill.SkillName != m_Skill)
                    return false;

                if (Quest.Owner.Skills[m_Skill].Base >= MaxProgress)
                {
                    Complete();
                    return true;
                }
            }

            return false;
        }

        public override void OnAccept()
        {
            Region region = Quest.Owner.Region;

            while (region != null)
            {
                if (region is ApprenticeRegion)
                    region.OnEnter(Quest.Owner);

                region = region.Parent;
            }
        }

        public override void OnCompleted()
        {
            QuestHelper.RemoveAcceleratedSkillgain(Quest.Owner);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }

    public class QuestionAndAnswerObjective : BaseObjective
    {
        private int _CurrentIndex;

        private readonly List<int> m_Done = new List<int>();
        private readonly QuestionAndAnswerEntry[] m_EntryTable;

        public virtual QuestionAndAnswerEntry[] EntryTable => m_EntryTable;

        public QuestionAndAnswerObjective(int count, QuestionAndAnswerEntry[] table)
            : base(count)
        {
            m_EntryTable = table;
            _CurrentIndex = -1;
        }

        public QuestionAndAnswerEntry GetRandomQandA()
        {
            if (m_EntryTable == null || m_EntryTable.Length == 0 || m_EntryTable.Length - m_Done.Count <= 0)
                return null;

            if (_CurrentIndex >= 0 && _CurrentIndex < m_EntryTable.Length)
            {
                return m_EntryTable[_CurrentIndex];
            }

            int ran;

            do
            {
                ran = Utility.Random(m_EntryTable.Length);
            }
            while (m_Done.Contains(ran));

            _CurrentIndex = ran;
            return m_EntryTable[ran];
        }

        public override bool Update(object obj)
        {
            m_Done.Add(_CurrentIndex);
            _CurrentIndex = -1;

            if (!Completed)
                CurProgress++;

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version

            writer.Write(_CurrentIndex);

            writer.Write(m_Done.Count);
            for (int i = 0; i < m_Done.Count; i++)
                writer.Write(m_Done[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version > 0)
            {
                _CurrentIndex = reader.ReadInt();
            }

            int c = reader.ReadInt();
            for (int i = 0; i < c; i++)
                m_Done.Add(reader.ReadInt());
        }
    }
}
