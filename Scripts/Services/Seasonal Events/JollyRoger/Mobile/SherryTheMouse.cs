using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Quests;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Fellowship
{
    public class SherryStrongBox : Item
    {
        public List<Mobile> Permission;

        [Constructable]
        public SherryStrongBox()
            : base(0xE80)
        {
            Weight = 0.0;

            Permission = new List<Mobile>();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 2))
            {
                if (Permission.Any(x => x == from))
                {
                    Item item = new SheetMusicForStones();
                    from.AddToBackpack(item);
                    from.SendLocalizedMessage(1152339, item.Name.ToString()); // A reward of ~1_ITEM~ has been placed in your backpack.
                    Permission.Remove(from);
                }
                else
                {
                    PrivateOverheadMessage(MessageType.Regular, 0x47E, 500648, from.NetState); // This chest seems to be locked.
                }
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public SherryStrongBox(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(Permission == null ? 0 : Permission.Count);

            if (Permission != null)
            {
                Permission.ForEach(x => writer.Write(x));
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Permission = new List<Mobile>();

            int permissoncount = reader.ReadInt();
            for (int x = 0; x < permissoncount; x++)
            {
                Mobile m = reader.ReadMobile();

                if (m != null)
                    Permission.Add(m);
            }
        }
    }

    public class SherryLute : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public string Note { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public SherryTheMouse Controller { get; set; }

        [Constructable]
        public SherryLute()
            : base(0xEB3)
        {
            Weight = 0.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 1))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else
            {
                from.PlaySound(0x4C);
                PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159341, from.NetState, Note); // *You strum the lute, it is tuned to ~1_NOTE~*

                if (Controller != null)
                {
                    if (Controller._List.ContainsKey(from))
                    {
                        if (Controller._List.Any(x => x.Key == from && x.Value.Contains(Note)))
                        {
                            Controller._List.Remove(from);
                        }
                        else
                        {
                            var temp = Controller._List[from].ToList();
                            temp.Add(Note);

                            bool correct = false;

                            int i;

                            for (i = 0; i < temp.Count; i++)
                            {
                                if (temp[i] == Controller.Notes[i])
                                {
                                    correct = true;
                                }
                                else
                                {
                                    correct = false;
                                }
                            }

                            if (correct)
                            {
                                Controller._List[from] = temp.ToArray();
                                //Controller._List[from].ToList().ForEach(x => Console.WriteLine(x));
                            }
                            else
                            {
                                Controller._List.Remove(from);
                            }

                            if (i == 8)
                            {
                                from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159342, from.NetState); // *You hear a click as the chest in the corner unlocks!*
                                Controller.Box.Permission.Add(from);
                                Controller._List.Remove(from);
                                Controller.ChangeNotes();
                            }
                        }
                    }
                    else if (Controller.Notes[0] == Note)
                    {
                        Controller._List.Add(from, new[] { Note });
                        //Controller._List[from].ToList().ForEach(x => Console.WriteLine(x));
                    }                   
                }
            }
        }

        public SherryLute(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(Note);
            writer.Write(Controller);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Note = reader.ReadString();
            Controller = reader.ReadMobile() as SherryTheMouse;
        }
    }

    public class SherryTheMouse : BaseQuester
    {
        public static SherryTheMouse InstanceTram { get; set; }
        public static SherryTheMouse InstanceFel { get; set; }

        public SherryStrongBox Box { get; set; }
        public List<SherryLute> LuteList;
        public Dictionary<Mobile, string[]> _List;
        public string[] NoteList;

        [Constructable]
        public SherryTheMouse()
            : base("the Mouse")
        {
            LuteList = new List<SherryLute>();
            _List = new Dictionary<Mobile, string[]>();

            NoteList = RandomNotes();

            Timer.DelayCall(TimeSpan.FromSeconds(5), () =>
            {
                SherryStrongBox b = new SherryStrongBox();
                Box = b;
                b.MoveToWorld(new Point3D(1347, 1642, 80), Map);

                for (int i = 0; i < LuteLocations.Length; i++)
                {
                    Point3D p = LuteLocations[i];

                    SherryLute sl = new SherryLute();
                    LuteList.Add(sl);
                    sl.Note = NoteList[i];
                    sl.Controller = this;

                    sl.MoveToWorld(p, Map);
                }
            });
        }

        public override void InitBody()
        {
            base.InitBody();

            Name = "Sherry";

            Body = 0xEE;
        }

        public override void OnDelete()
        {
            if (Box != null)
            {
                Box.Delete();
            }

            if (LuteList != null)
            {
                LuteList.ForEach(f => f.Delete());
                LuteList.Clear();
            }            

            base.OnDelete();
        }

        public void ChangeNotes()
        {
            NoteList = RandomNotes();

            if (LuteList != null)
            {
                for (int i = 0; i < LuteList.Count; i++)
                {
                    if (LuteList[i] != null)
                    {
                        LuteList[i].Note = NoteList[i];
                    }
                }
            }
        }

        public string[] RandomNotes()
        {
            return Notes.ToList().Select(x => new { n = x, rand = Notes[Utility.Random(8)] }).OrderBy(x => x.rand).Select(x => x.n).ToArray();
        }

        public string[] Notes = new string[]
        {
            "C4", "D", "E", "F", "G", "A", "B", "C5"
        };

        private readonly Point3D[] LuteLocations = new Point3D[]
        {
            new Point3D(1350, 1646, 80), new Point3D(1350, 1650, 80), new Point3D(1350, 1655, 80), new Point3D(1350, 1659, 80),
            new Point3D(1355, 1646, 80), new Point3D(1355, 1650, 80), new Point3D(1355, 1655, 80), new Point3D(1355, 1659, 80)
        };

        public override void OnTalk(PlayerMobile player, bool contextMenu)
        {
        }

        public override bool CanTalkTo(PlayerMobile to)
        {
            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            Gump g = new Gump(100, 100);
            g.AddBackground(0, 0, 454, 640, 0x24A4);
            g.AddImage(60, 40, 0x6D2);
            g.AddHtmlLocalized(27, 389, 398, 18, 1114513, "#115938", 0xC63, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            g.AddHtmlLocalized(27, 416, 398, 174, 1159386, 0xC63, false, true); // You have found yourself here, or have I made it so you find yourself here? *grins* Alas, here you are! <br><br>Virtue is being drained from your world at the hands of the Fellowship under the guise of their altruistic intent. Do not be fooled, their objective is nefarious and their presence in Britannia is a pox upon our shared devotion to the Virtues. <br><br>The Fellowship has been successful in destroying the Runes of Virtue and the encouraging greed in those coveting Fellowship Treasure. This is fueling the destruction of Shrines across Britannia. To combat this trend, I reached deep inside the timeline and placed fragments of the Runes of Virtue in treasure chests hidden throughout the realm. With these fragments we can begin to restore the Shrines. Place these mysterious fragments at Shrines across Britannia to lure the armies of the Fellowship from hiding. <br><br>If you best eight of these armies at a single shrine you have restored the most fragments, your devotion to Virtue will be rewarded with the Tabard of Virtue. This tabard will reflect the Virtue to which you have restored the most fragments. A corresponding title will also be bestowed upon you. <br><br>For those who are truly devout and summon the Courage to best three of the armies at each Shrine, they will be awarded the Cloak of the Virtuous - a truly auspicious honor! <br><br>When you have completed your quest visit the Ankh behind me to claim your Tabard of Virtue. To claim the Cloak of the Virtuous approach each representation of the Virtues surrounding me. When you are true to each, the Cloak of the Virtuous shall be yours!

            from.SendGump(g);
        }

        public SherryTheMouse(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Box);

            writer.Write(LuteList == null ? 0 : LuteList.Count);

            if (LuteList != null)
            {
                LuteList.ForEach(x => writer.Write(x));
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Box = reader.ReadItem() as SherryStrongBox;

            int lutecount = reader.ReadInt();
            for (int x = 0; x < lutecount; x++)
            {
                SherryLute l = reader.ReadItem() as SherryLute;

                if (l != null)
                    LuteList.Add(l);
            }

            if (Map == Map.Trammel)
            {
                InstanceTram = this;
            }

            if (Map == Map.Felucca)
            {
                InstanceFel = this;
            }

            ChangeNotes();
        }
    }
}
