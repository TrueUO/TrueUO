using Server.Gumps;
using System;

namespace Server.Items
{
    public abstract class BaseLocalizedBook : Item
    {
        public virtual object Title => "a book";
        public virtual object Author => "unknown";

        public abstract int[] Contents { get; }

        public BaseLocalizedBook() : base(4082)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else
            {
                from.CloseGump(typeof(InternalGump));
                from.SendGump(new InternalGump(this));
                from.SendSound(0x55);
            }
        }

        private class InternalGump : Gump
        {
            private readonly BaseLocalizedBook m_Book;

            public InternalGump(BaseLocalizedBook book)
                : base(245, 200)
            {
                TypeID = 0x237B;
                m_Book = book;
                int page = 0;
                int pages = (int)Math.Ceiling(m_Book.Contents.Length / 2.0);

                AddImage(0, 0, 0x1FE);                

                page++;
                AddPage(page);

                if (book.Title is int iTitle)
                    AddHtmlLocalized(40, 30, 150, 48, iTitle, 0x0, false, false);
                else if (book.Title is string sTitle)
                    AddHtml(40, 30, 150, 48, sTitle, false, false);
                else
                    AddLabel(40, 30, 0, "A Book");

                AddHtmlLocalized(40, 160, 150, 16, 1113300, 0x0, false, false); // by

                if (book.Author is int iAuthor)
                    AddHtmlLocalized(40, 180, 150, 32, iAuthor, 0x0, false, false);
                else if (book.Author is string sAuthor)
                    AddHtml(40, 180, 150, 32, sAuthor, false, false);
                else
                    AddLabel(40, 180, 0, "unknown");

                for (int i = 0; i < m_Book.Contents.Length; i++)
                {
                    int cliloc = m_Book.Contents[i];
                    bool endPage = false;
                    int x = 40;
                    int y = 30;
                    int width = 145;

                    if (cliloc <= 0)
                        continue;

                    if (page == 1)
                    {
                        x = 230;
                        endPage = true;
                    }
                    else
                    {
                        if ((i + 1) % 2 == 0)
                        {
                            x = 40;
                            y = 35;
                            width = 150;
                        }
                        else if (page <= pages)
                        {
                            endPage = true;
                            x = 230;
                        }
                    }

                    AddHtmlLocalized(x, y, width, 160, cliloc, 0x0, false, false);

                    if ((i + 1) % 2 == 0)
                    {
                        AddLabel(90, 200, 0x0, string.Format(" {0}", i + 1));
                    }
                    else
                    {
                        AddLabel(250, 200, 0x0, string.Format("      {0}", i + 1));
                    }

                    if (page < pages)
                    {
                        AddButton(356, 0, 0x200, 0x200, 0, GumpButtonType.Page, page + 1);
                    }

                    if (page - 1 > 0)
                    {
                        AddButton(0, 0, 0x1FF, 0x1FF, 0, GumpButtonType.Page, page - 1);
                    }

                    if (endPage)
                    {
                        page++;
                        AddPage(page);
                    }
                }
            }
        }

        public BaseLocalizedBook(Serial serial)
            : base(serial)
        {
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
