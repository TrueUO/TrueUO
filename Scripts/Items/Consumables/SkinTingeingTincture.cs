using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
    public class SkinTingeingTincture : Item
    {
        public override int LabelNumber => 1114770;  //Skin Tingeing Tincture

        [Constructable]
        public SkinTingeingTincture()
            : base(0xEFF)
        {
            Hue = 90;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1114771); // Apply Directly to Forehead
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (IsChildOf(m.Backpack) && m is PlayerMobile mobile)
            {
                if (!mobile.HasGump(typeof(InternalGump)))
                {
                    BaseGump.SendGump(new InternalGump(mobile, this));
                }
            }
        }

        public SkinTingeingTincture(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        private class InternalGump : BaseGump
        {
            public override int GetTypeID()
            {
                return 0xF3EA1;
            }

            public SkinTingeingTincture Item { get; }
            public int SelectedHue { get; set; }

            public InternalGump(PlayerMobile pm, SkinTingeingTincture item)
                : base(pm)
            {
                Item = item;
            }

            public override void AddGumpLayout()
            {
                AddBackground(0, 0, 460, 300, 2620);

                int[] list = GetHueList();

                int rows = 8;
                int start = 40;
                

                int x = start;
                int y = start;
                int displayHue;

                for (int i = 0; i < list.Length; i++)
                {
                    if (i > 0 && i % rows == 0)
                    {
                        x = start;
                        y += 22;
                    }

                    displayHue = list[i];

                    AddImage(x, y, 210, displayHue);
                    AddButton(x, y, 212, 212, i + 100, GumpButtonType.Reply, 0);

                    x += 21;
                }

                displayHue = SelectedHue != 0 ? SelectedHue : User.Hue ^ 0x8000;

                AddImage(240, 0, GetPaperdollImage(), displayHue);

                AddButton(250, 260, 239, 238, 1, GumpButtonType.Reply, 0);
                AddButton(50, 260, 242, 241, 0, GumpButtonType.Reply, 0);
            }

            public override void OnResponse(RelayInfo info)
            {
                int button = info.ButtonID;

                if (button >= 100)
                {
                    button -= 100;

                    int[] list = GetHueList();

                    if (button < list.Length)
                    {
                        SelectedHue = list[button];
                        Refresh(true, false);
                    }
                }
                else if (button == 1 && Item != null)
                {
                    if (SelectedHue != 0)
                    {
                        User.Hue = User.Race.ClipSkinHue(SelectedHue & 0x3FFF) | 0x8000;
                        Item.Delete();
                    }
                }
            }

            private int GetPaperdollImage()
            {
                return User.Female ? 13 : 12;
            }

            private int[] GetHueList()
            {
                return HumanSkinHues;
            }

            private static int[] _HumanSkinHues;

            public static int[] HumanSkinHues
            {
                get
                {
                    if (_HumanSkinHues == null)
                    {
                        _HumanSkinHues = new int[57];

                        for (int i = 0; i < _HumanSkinHues.Length; i++)
                        {
                            _HumanSkinHues[i] = i + 1001;
                        }
                    }

                    return _HumanSkinHues;
                }
            }
        }
    }
}
