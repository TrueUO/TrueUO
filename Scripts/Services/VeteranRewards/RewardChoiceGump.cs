using Server.Gumps;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.VeteranRewards
{
    public class RewardChoiceGump : Gump
    {
        private readonly Mobile m_From;
        private readonly int m_CategoryIndex;
        private readonly int m_Page;

        public RewardChoiceGump(Mobile from)
            : this(from, 0, 0)
        {
        }

        public RewardChoiceGump(Mobile from, int cat, int page)
            : base(100, 100)
        {
            m_From = from;

            from.CloseGump(typeof(RewardChoiceGump));

            m_CategoryIndex = cat;
            m_Page = page;

            RenderBackground();
            RenderCategories();
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int buttonID = info.ButtonID - 1;

            if (buttonID == 0)
            {
                RewardSystem.ComputeRewardInfo(m_From, out int cur, out int max);

                if (cur < max)
                    m_From.SendGump(new RewardNoticeGump(m_From));
            }
            else if (info.ButtonID == 1000)
            {
                m_From.SendGump(new RewardChoiceGump(m_From, m_CategoryIndex, m_Page+1));
            }
            else if (info.ButtonID == 1001)
            {
                m_From.SendGump(new RewardChoiceGump(m_From, m_CategoryIndex, m_Page - 1));
            }
            else if (buttonID >= 19999)
            {
                m_From.SendGump(new RewardChoiceGump(m_From, buttonID - 19999, 0));
            }
            else
            {
                --buttonID;

                int type = (buttonID % 20);
                int index = (buttonID / 20);

                RewardCategory[] categories = RewardSystem.Categories;

                if (type >= 0 && type < categories.Length)
                {
                    RewardCategory category = categories[type];

                    if (index >= 0 && index < category.Entries.Count)
                    {
                        RewardEntry entry = category.Entries[index];

                        if (!RewardSystem.HasAccess(m_From, entry))
                            return;

                        m_From.SendGump(new RewardConfirmGump(m_From, entry));
                    }
                }
            }
        }

        private void RenderBackground()
        {
            AddPage(0);

            AddBackground(0, 0, 820, 620, 0x6DB);
            AddHtmlLocalized(10, 10, 800, 18, 1114513, "#1159424", 0x43FF, false, false); // Ultima Online Veteran Rewards Program
            AddHtmlLocalized(10, 37, 800, 72, 1114513, "#1159425", 0x43FF, false, false); // Thank you for being part of the Ultima Online community! As a token of our appreciation, you may select from the following in-game reward items listed below. The gift items will be attributed to the character you have logged-in with on the shard you are on when you choose the item(s). The number of rewards you are entitled to are listed below and are for your entire account. To read more about these rewards before making a selection, feel free to visit the <A HREF="https://uo.com/wiki/ultima-online-wiki/items/veteran-rewards/">uo.com website</A>.

            RewardSystem.ComputeRewardInfo(m_From, out int cur, out int max);

            AddHtmlLocalized(160, 118, 650, 18, 1159426, string.Format("{0}@{1}", cur.ToString(), (max - cur).ToString()), 0x7FF0, false, false); // You have already chosen ~1_COUNT~ rewards.  You have ~2_COUNT~ remaining rewards to choose.
            AddECHandleInput();
        }

        private void RenderCategories()
        {
            RewardCategory[] categories = RewardSystem.Categories;

            for (int i = 0; i < categories.Length; ++i)
            {
                if (!RewardSystem.HasAccess(m_From, categories[i]))
                {
                    continue;
                }

                AddButton(18 + (i * 130), 160, m_CategoryIndex == i ? 0x635 : 0x634, 0x637, 20000 + i, GumpButtonType.Reply, 0);

                if (categories[i].NameString != null)
                    AddHtml(18 + (i * 130), 162, 125, 18, string.Format("<div align=CENTER>{0}</div>", categories[i].NameString), false, false);
                else
                    AddHtmlLocalized(18 + (i * 130), 162, 125, 18, 1114513, string.Format("#{0}", categories[i].Name), 0xC63, false, false);
            }

            RenderCategory(categories[m_CategoryIndex]);
        }

        private int GetButtonID(int type, int index)
        {
            return 2 + (index * 20) + type;
        }

        private void RenderCategory(RewardCategory category)
        {
            List<RewardEntry> entries = category.Entries;            

            Dictionary<int, RewardEntry> l = new Dictionary<int, RewardEntry>();

            for (int j = 0; j < entries.Count; ++j)
            {
                RewardEntry entry = entries[j];

                if (!RewardSystem.HasAccess(m_From, entry))
                    continue;

                l[j] = entry;
            }

            int rewardcount = l.Count;

            if ((m_Page + 1) * 60 < rewardcount)
                AddButton(554, 10, 0x15E1, 0x15E5, 1000, GumpButtonType.Reply, 0); // Next Page Button

            if (m_Page > 0)
                AddButton(554, 10, 0x15E3, 0x15E7, 1001, GumpButtonType.Reply, 0); // Previous Page Button

            for (int i = 0, index = m_Page * 60; i < 60 && index < rewardcount; ++i, ++index)
            {
                var item = l.ElementAt(index);
                RewardEntry entry = item.Value;

                if (entry.NameString != null)
                    AddHtml(50 + ((i / 20) * 250), 200 + ((i % 20) * 18), 200, 18, entry.NameString, false, false);
                else
                    AddHtmlLocalized(50 + ((i / 20) * 250), 200 + ((i % 20) * 18), 200, 18, entry.Name, 0x7FFF, false, false);

                AddButton(30 + ((i / 20) * 250), 200 + ((i % 20) * 18), 0x845, 0x846, GetButtonID(m_CategoryIndex, item.Key), GumpButtonType.Reply, 0);
            }
        }
    }
}
