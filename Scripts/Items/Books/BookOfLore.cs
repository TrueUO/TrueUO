using Server.Gumps;
using Server.Multis;
using Server.Network;
using Server.Targeting;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public enum Story
    {
        SherryTheMouse,
    }

    public class StoryDefinition
    {
        public Story Story { get; }
        public int PageID { get; }
        public string ArticlePart1 { get; }
        public string ArticlePart2 { get; }
        public int BGumpID { get; } // background gump
        public int UPCGumpID { get; } // upper left corner gump
        public int BRCGumpID { get; } // bottom right corner gump

        public StoryDefinition(Story story, int pid, string artic1, string artic2, int bid, int upcid, int brcid)
        {
            Story = story;
            PageID = pid;
            ArticlePart1 = artic1;
            ArticlePart2 = artic2;
            BGumpID = bid;
            UPCGumpID = upcid;
            BRCGumpID = brcid;
        }
    }

    public class PageOfLore : Item
    {
        public static StoryDefinition[] Table { get; } =
        {
            new StoryDefinition(Story.SherryTheMouse, 1, "Once upon a time, in the city of Britain, there was a mouse named Sherry. She was a friendly mouse, one that you might offer a piece of cheese every now and again. Sherry lived a relatively normal life, for a mouse at least.", "This was all until that fateful day when she met the most peculiar of people whilst out rummaging the streets of Britain!", 0xAF1, 0xAB6, 0),
            new StoryDefinition(Story.SherryTheMouse, 2, "Before she could even know it Sherry the Mouse was staring straight up a very tall mage! She admired his long flowing robes and magical oddities. She squeaked up at him, \"Are you a real mage?\"",  "\"Why yes! Yes I am!\", he replied. She was so overcome with excitement! She'd never met a real mage before! \"Can we go on an adventure? I've never recalled before!\" The mage, amused at her enthusiasm obliged and before long they vanished!", 0xAF1, 0xAB6, 0xAAF),
            new StoryDefinition(Story.SherryTheMouse, 3, "In an instant Sherry found herself in a frigid tundra. The Mage had taken her to ice island! She'd never seen snow before and this was quite a threat! The air was quiet and she felt herself begin to shiver. The mage, noticing her discomfort", "Offered Sherry a scrap of fabric which she fashioned into a makeshift coat. \"Where would you like to see next?\" Sherry could barely container herself, \"The world!\" she squeaked!", 0xAFB, 0xAB6, 0xAAF)
        };

        public static string GetStoryName(Story story)
        {
            string storyname;

            switch(story)
            {
                default:
                case Story.SherryTheMouse:
                    {
                        storyname = "Sherry The Mouse";
                        break;
                    }
            }

            return storyname;
        }

        public override int LabelNumber => 1159634; // Page of Lore

        public Story Story { get; set; }
        public int PageID { get; set; }

        [Constructable]
        public PageOfLore(Story story, int pid)
            : base(0x46B3)
        {
            Story = story;
            PageID = pid;
        }

        public PageOfLore(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
            }
            else
            {
                from.BeginTarget(6, false, TargetFlags.None, OnTarget);
                from.SendLocalizedMessage(1159629); // Which Book of Lore do you wish to place this page?
            }
        }

        public void OnTarget(Mobile from, object obj)
        {
            if (Deleted)
                return;

            if (obj is Item item)
            {
                if (!item.IsChildOf(from.Backpack))
                {
                    from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
                    return;
                }

                if (item is BookOfLore book)
                {
                    if (book.Content.Any())
                    {
                        if (book.Story != Story)
                        {
                            from.SendLocalizedMessage(1159631); // This page is not part of that story...
                        }
                        else if (book.Content.Contains(PageID))
                        {
                            from.SendLocalizedMessage(1159632); // This page is already part of that story...
                        }
                        else
                        {
                            AddStory(book, from);
                        }
                    }
                    else
                    {
                        AddStory(book, from);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1159630); // That is not a valid Book of Lore.
                }
            }
        }

        public void AddStory(BookOfLore book, Mobile from)
        {
            book.Content.Add(PageID);
            book.Story = Story;
            from.PlaySound(585);
            from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159633, from.NetState);
            Delete();
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            var content = Table.FirstOrDefault(x => x.Story == Story && x.PageID == PageID);

            list.Add(1157254, "The Tale of " + Misc.ServerList.ServerName);
            list.Add(1114778, GetStoryName(content.Story));
            list.Add(1159635, content.PageID.ToString());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(PageID);
            writer.Write((int)Story);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            PageID = reader.ReadInt();
            Story = (Story)reader.ReadInt();
        }
    }

    public class BookOfLore : Item, ISecurable
    {
        public override int LabelNumber => 1159520; // Book of Lore

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurrentPage { get; set; }

        public Story Story { get; set; }

        public List<int> Content { get; set; }

        [Constructable]
        public BookOfLore()
            : base(0xA75F)
        {
            Content = new List<int>();
        }

        public BookOfLore(Serial serial)
            : base(serial)
        {
        }

        public bool CheckAccessible(Mobile from, Item item)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true; // Staff can access anything

            BaseHouse house = BaseHouse.FindHouseAt(item);

            if (house == null)
                return false;

            switch (Level)
            {
                case SecureLevel.Owner: return house.IsOwner(from);
                case SecureLevel.CoOwners: return house.IsCoOwner(from);
                case SecureLevel.Friends: return house.IsFriend(from);
                case SecureLevel.Anyone: return true;
                case SecureLevel.Guild: return house.IsGuildMember(from);
            }

            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else if (Content == null || !Content.Any())
            {
                from.SendLocalizedMessage(1159628); // This book contains no pages...
            }
            else
            {
                if (from.HasGump(typeof(BookOfLoreGump)))
                    return;

                from.SendGump(new BookOfLoreGump(this, CurrentPage));
            }
        }

        public class BookOfLoreGump : Gump
        {
            public BookOfLore Book { get; }
            public int Page { get; }

            public BookOfLoreGump(BookOfLore book, int page)
                : base(100, 100)
            {
                Book = book;

                var pages = book.Content.OrderBy(x => x);

                if (page == 0)
                {
                    page = pages.First();
                }

                if (!pages.Contains(page))
                {
                    page = pages.Last();
                }

                Page = page;
                Book.CurrentPage = page;

                var content = PageOfLore.Table.FirstOrDefault(x => x.Story == book.Story && x.PageID == Page);

                AddPage(0);

                AddImage(0, 0, 0xA9C);
                AddImage(85, 18, 0xA9D);
                AddHtml(163, 45, 150, 70, string.Format("<BASEFONT COLOR=#000080><DIV ALIGN=CENTER>The Tale of {0}</DIV></BASEFONT>", Misc.ServerList.ServerName), false, false);
                AddHtml(100, 120, 270, 20, string.Format("<BASEFONT COLOR=#15156A><DIV ALIGN=CENTER>{0}</DIV></BASEFONT>", PageOfLore.GetStoryName(book.Story)), false, false);
                AddHtml(115, 145, 250, 126, string.Format("<BASEFONT COLOR=#1F1F1F>{0}</BASEFONT>", content.ArticlePart1), false, false);
                AddHtml(115, 275, 250, 126, string.Format("<BASEFONT COLOR=#1F1F1F>{0}</BASEFONT>", content.ArticlePart2), false, false);
                AddButton(200, 420, 0x15E3, 0x15E7, 11001, GumpButtonType.Reply, 0);
                AddHtml(110, 419, 250, 18, string.Format("<BASEFONT COLOR=#1F1F1F><DIV ALIGN=CENTER>{0}</DIV></BASEFONT>", Page.ToString()), false, false);
                AddButton(250, 420, 0x15E1, 0x15E5, 11000, GumpButtonType.Reply, 0);

                AddImage(435, 52, content.BGumpID);
                AddImage(426, 42, 0xA9E);

                if (content.UPCGumpID != 0)
                {
                    AddImage(418, 35, content.UPCGumpID);
                    AddImage(409, 25, 0xAA1);
                }

                if (content.BRCGumpID != 0)
                {
                    AddImage(591, 363, content.BRCGumpID);
                    AddImage(582, 353, 0xAA1);
                }
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (Book.Deleted)
                    return;

                Mobile m = sender.Mobile;

                switch (info.ButtonID)
                {
                    case 11001:
                        m.SendGump(new BookOfLoreGump(Book, Page - 1));
                        break;
                    case 11000:
                        m.SendGump(new BookOfLoreGump(Book, Page + 1));
                        break;
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)Level);
            writer.Write(CurrentPage);
            writer.Write((int)Story);

            writer.Write(Content.Count);

            Content.ForEach(s =>
            {
                writer.Write(s);
            });

        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Level = (SecureLevel)reader.ReadInt();
            CurrentPage = reader.ReadInt();
            Story = (Story)reader.ReadInt();

            int count = reader.ReadInt();

            Content = new List<int>();

            for (int i = count; i > 0; i--)
            {
                int id = reader.ReadInt();

                Content.Add(id);
            }
        }
    }
}
