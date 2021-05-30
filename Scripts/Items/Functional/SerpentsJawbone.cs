using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;
using System.Collections.Generic;

namespace Server.Items
{
    public class SerpentsJawbone : Item, ISecurable
    {
        public static Dictionary<int, Point3D> Locations { get; set; }

        public static void Initialize()
        {
            Locations = new Dictionary<int, Point3D>
            {
                [1157135] = new Point3D(1156, 1143, -24), // The Village of Lakeshire		
                [1157619] = new Point3D(644, 854, -56), // The Rat Fort		
                [1157620] = new Point3D(1363, 1075, -13),  // Reg Volom			
                [1016410] = new Point3D(1572, 1046, -8), // Twin Oaks Tavern			
                [1157621] = new Point3D(984, 622, -80), // The Oasis			
                [1078308] = new Point3D(1746, 1221, -1), // Blood Dungeon		
                [1111764] = new Point3D(912, 1362, -21), // Cyclops Dungeon			
                [1111765] = new Point3D(824, 774, -80), // Exodus Dungeon		
                [1111766] = new Point3D(349, 1434, 16), // The Kirin Passage			
                [1157622] = new Point3D(971, 303, 54), // Pass of Karnaugh			
                [1157623] = new Point3D(1033, 1154, -24), // The Rat Cave		
                [1078315] = new Point3D(541, 466, -72), // Terort Skitas			
                [1111825] = new Point3D(1450, 1477, -29), // Twisted Weald			
                [1113002] = new Point3D(642, 1307, -55), // Wisp Dungeon			
                [1157624] = new Point3D(753, 497, -62), // Gwenno's Memorial			
                [1157625] = new Point3D(1504, 628, -14), // Desert Gypsy Camp			
                [1113000] = new Point3D(1785, 573, 71) // Rock Dungeon
            };
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level { get; set; }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public override int LabelNumber => 1157654;  // Serpent's Jawbone

        [Constructable]
        public SerpentsJawbone()
            : base(0x9F74)
        {
        }

        public override bool ForceShowProperties => true;

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            if (IsLockedDown || IsSecure)
            {
                from.CloseGump(typeof(InternalGump));
                from.SendGump(new InternalGump(from as PlayerMobile, this));
            }
            else
            {
                from.SendLocalizedMessage(502692); // This must be in a house and be locked down to work.
            }
        }

        private class InternalGump : Gump
        {
            public Item Jawbone { get; set; }
            public PlayerMobile User { get; set; }

            public InternalGump(PlayerMobile pm, Item jawbone)
                : base(100, 100)
            {
                Jawbone = jawbone;
                User = pm;

                AddGumpLayout();
            }

            public void AddGumpLayout()
            {
                AddPage(0);

                AddBackground(0, 0, 370, 428, 0x1400);

                AddHtmlLocalized(10, 10, 350, 18, 1114513, "#1156704", 0x56BA, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>

                ColUtility.For(Locations, (i, key, value) =>
                {
                    AddButton(10, 41 + (i * 20), 1209, 1210, key, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(50, 41 + (i * 20), 150, 20, key, 0x7FFF, false, false);
                });
            }

            public override void OnResponse(NetState state, RelayInfo info)
            {
                if (info.ButtonID > 0)
                {
                    int id = info.ButtonID;

                    if (Locations.ContainsKey(id))
                    {
                        Point3D p = Locations[id];

                        if (CheckTravel(p))
                        {
                            Effects.SendPacket(User.Location, User.Map, new ParticleEffect(EffectType.FixedFrom, User.Serial, Server.Serial.Zero, 0x3728, User.Location, User.Location, 10, 10, false, false, 0, 0, 0, 2023, 1, User.Serial, 80, 0));
                            Effects.PlaySound(User.Location, User.Map, 496);

                            BaseCreature.TeleportPets(User, p, Map.Ilshenar);
                            User.MoveToWorld(p, Map.Ilshenar);
                            Effects.PlaySound(p, Map.Ilshenar, 0x1FE);
                        }
                    }
                }
            }

            private bool CheckTravel(Point3D p)
            {
                if (!User.InRange(Jawbone.GetWorldLocation(), 2) || User.Map != Jawbone.Map)
                {
                    User.SendLocalizedMessage(1076766); // That is too far away.
                }
                else if (SpellHelper.RestrictRedTravel && User.Murderer)
                {
                    User.SendLocalizedMessage(1019004); // You are not allowed to travel there.
                }
                else if (Engines.VvV.VvVSigil.ExistsOn(User))
                {
                    User.SendLocalizedMessage(1019004); // You are not allowed to travel there.
                }
                else if (User.Criminal)
                {
                    User.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                }
                else if (SpellHelper.CheckCombat(User))
                {
                    User.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                }
                else if (User.Spell != null)
                {
                    User.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
                }
                else if (User.Map == Map.Ilshenar && User.InRange(p, 1))
                {
                    User.SendLocalizedMessage(1019003); // You are already there.
                }
                else
                    return true;

                return false;
            }
        }

        public SerpentsJawbone(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)Level);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Level = (SecureLevel)reader.ReadInt();
        }
    }
}
