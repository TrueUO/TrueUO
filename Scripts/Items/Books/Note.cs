using Server.Gumps;

namespace Server.Items
{
    public abstract class Note : Item // BaseNote - Base class.
    {
        protected abstract int[] Contents { get; }

        protected Note()
            : base(0x14ED)
        {
        }

        protected Note(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m.InRange(GetWorldLocation(), 3))
            {
                m.CloseGump(typeof(InternalGump));
                m.SendGump(new InternalGump(this));
            }
        }

        private class InternalGump : Gump
        {
            public InternalGump(Note note)
                : base(245, 200)
            {
                var mNote = note;

                int page = 0;
                int pages = mNote.Contents.Length;

                AddImage(0, 0, 0x27);

                page++;
                AddPage(page);                

                for (int i = 0; i < mNote.Contents.Length; i++)
                {
                    int cliloc = mNote.Contents[i];

                    bool endPage = false;

                    if (cliloc <= 0)
                    {
                        continue;
                    }

                    if (page == 1)
                    {
                        endPage = true;
                    }
                    else if (page <= pages)
                    {
                        endPage = true;
                    }

                    AddHtmlLocalized(45, 30, 165, 200, cliloc, 0x0, false, false);

                    AddLabel(90, 245, 0x0, string.Format("    {0}", i + 1));

                    if (page < pages)
                    {
                        AddButton(203, 267, 0x825, 0x825, 0, GumpButtonType.Page, page + 1);
                    }

                    if (page - 1 > 0)
                    {
                        AddButton(203, 5, 0x824, 0x824, 0, GumpButtonType.Page, page - 1);
                    }

                    if (endPage)
                    {
                        page++;
                        AddPage(page);
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version < 1)
            {
                reader.ReadString();
                reader.ReadInt();
            }
        }
    }
}
