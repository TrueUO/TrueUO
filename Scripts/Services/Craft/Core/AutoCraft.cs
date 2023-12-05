using Server.Items;
using Server.Prompts;
using System;
using System.Collections.Generic;

namespace Server.Engines.Craft
{
    public class MakeNumberCraftPrompt : Prompt
    {
        private readonly Mobile _From;
        private readonly CraftSystem _CraftSystem;
        private readonly CraftItem _CraftItem;
        private readonly ITool _Tool;

        public MakeNumberCraftPrompt(Mobile from, CraftSystem system, CraftItem item, ITool tool)
        {
            _From = from;
            _CraftSystem = system;
            _CraftItem = item;
            _Tool = tool;
        }

        public override void OnCancel(Mobile from)
        {
            _From.SendLocalizedMessage(501806); //Request cancelled.
            from.SendGump(new CraftGump(_From, _CraftSystem, _Tool, null));
        }

        public override void OnResponse(Mobile from, string text)
        {
            int amount = Utility.ToInt32(text);

            if (amount < 1 || amount > 100)
            {
                from.SendLocalizedMessage(1112587); // Invalid Entry.
                ResendGump();
            }
            else
            {
                AutoCraftTimer.EndTimer(from);

                new AutoCraftTimer(_From, _CraftSystem, _CraftItem, _Tool, amount, TimeSpan.FromSeconds(_CraftSystem.Delay * _CraftSystem.MaxCraftEffect + 1.0), TimeSpan.FromSeconds(_CraftSystem.Delay * _CraftSystem.MaxCraftEffect + 1.0));

                CraftContext context = _CraftSystem.GetContext(from);

                if (context != null)
                {
                    context.MakeTotal = amount;
                }
            }
        }

        public void ResendGump()
        {
            _From.SendGump(new CraftGump(_From, _CraftSystem, _Tool, null));
        }
    }

    public class AutoCraftTimer : Timer
    {
        private static readonly Dictionary<Mobile, AutoCraftTimer> _AutoCraftTable = new Dictionary<Mobile, AutoCraftTimer>();
        public static Dictionary<Mobile, AutoCraftTimer> AutoCraftTable => _AutoCraftTable;

        private readonly Mobile m_From;
        private readonly CraftSystem m_CraftSystem;
        private readonly CraftItem m_CraftItem;
        private readonly ITool m_Tool;
        private readonly int m_Amount;
        private int m_Attempts;
        private int m_Ticks;
        private readonly Type m_TypeRes;

        public int Amount => m_Amount;
        public int Attempts => m_Attempts;

        public AutoCraftTimer(Mobile from, CraftSystem system, CraftItem item, ITool tool, int amount, TimeSpan delay, TimeSpan interval)
            : base(delay, interval)
        {
            m_From = from;
            m_CraftSystem = system;
            m_CraftItem = item;
            m_Tool = tool;
            m_Amount = amount;
            m_Ticks = 0;
            m_Attempts = 0;

            CraftContext context = m_CraftSystem.GetContext(m_From);

            if (context != null)
            {
                CraftSubResCol res = m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;
                int resIndex = m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;

                if (resIndex > -1)
                {
                    m_TypeRes = res.GetAt(resIndex).ItemType;
                }
            }

            _AutoCraftTable[from] = this;

            Start();
        }

        public AutoCraftTimer(Mobile from, CraftSystem system, CraftItem item, ITool tool, int amount)
            : this(from, system, item, tool, amount, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3))
        {
        }

        protected override void OnTick()
        {
            m_Ticks++;

            if (m_From.NetState == null)
            {
                EndTimer(m_From);
                return;
            }

            CraftItem();

            if (m_Ticks >= m_Amount)
            {
                EndTimer(m_From);
            }
        }

        private void CraftItem()
        {
            if (m_From.HasGump(typeof(CraftGump)))
            {
                m_From.CloseGump(typeof(CraftGump));
            }

            if (m_From.HasGump(typeof(CraftGumpItem)))
            {
                m_From.CloseGump(typeof(CraftGumpItem));
            }

            m_Attempts++;

            if (m_CraftItem.TryCraft != null)
            {
                m_CraftItem.TryCraft(m_From, m_CraftItem, m_Tool);
            }
            else
            {
                m_CraftSystem.CreateItem(m_From, m_CraftItem.ItemType, m_TypeRes, m_Tool, m_CraftItem);
            }
        }

        public static void EndTimer(Mobile from)
        {
            if (_AutoCraftTable.TryGetValue(from, out AutoCraftTimer value))
            {
                value.Stop();

                _AutoCraftTable.Remove(from);
            }
        }

        public static bool HasTimer(Mobile from)
        {
            return from != null && _AutoCraftTable.ContainsKey(from);
        }
    }
}
