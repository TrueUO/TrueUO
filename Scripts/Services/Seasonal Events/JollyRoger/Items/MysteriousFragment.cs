using Server.Engines.JollyRoger;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class ShrineDef
    {
        public Shrine Shrine { get; set; }
        public int Hue { get; set; }
        public int TitleCliloc { get; set; }

        public ShrineDef(Shrine s, int h, int tc)
        {
            Shrine = s;
            Hue = h;
            TitleCliloc = tc;
        }
    }

    public class MysteriousFragment : Item
    {
        public override int LabelNumber => 1159025;  // mysterious fragment

        private Timer _Timer;

        private ShrineBattleController _Controller;

        [Constructable]
        public MysteriousFragment()
            : base(0x1F13)
        {
            Hue = Utility.RandomList(ShrineDef.Select(x => x.Hue).ToArray());
        }

        public static readonly List<ShrineDef> ShrineDef = new List<ShrineDef>()
        {
            new ShrineDef(Shrine.Spirituality, 2500, 1159320),
            new ShrineDef(Shrine.Compassion, 1912, 1159321),
            new ShrineDef(Shrine.Honor, 1918, 1159322),
            new ShrineDef(Shrine.Honesty, 1916, 1159323),
            new ShrineDef(Shrine.Humility, 1910, 1159324),
            new ShrineDef(Shrine.Justice, 1914 , 1159325),
            new ShrineDef(Shrine.Valor, 1920, 1159326),
            new ShrineDef(Shrine.Sacrifice, 1922, 1159327),
        };

        public override void OnDoubleClick(Mobile from)
        {
            Gump g = new Gump(100, 100);
            g.AddBackground(0, 0, 454, 400, 0x24A4);
            g.AddItem(75, 120, ItemID, Hue);
            g.AddHtmlLocalized(177, 50, 250, 18, 1114513, "#1159025", 0x3442, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            g.AddHtmlLocalized(177, 77, 250, 36, 1114513, "#1159026", 0x3442, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            g.AddHtmlLocalized(177, 122, 250, 228, 1159027, 0xC63, true, true); // The item appears to be the jagged fragment of a larger piece.  While you cannot quite discern the origins or purpose of such a piece, it is no doubt fascinating.  The color shimmers with a strange brilliance that you feel you have seen before, yet cannot quite place.  Whatever created this fragment did so with awesome force.


            from.SendGump(g);

            from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1157722, "its origin", from.NetState); // *Your proficiency in ~1_SKILL~ reveals more about the item*
            from.SendSound(from.Female ? 0x30B : 0x41A);
        }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            var drop = base.DropToWorld(from, p);

            var region = (ShrineBattleRegion)Region.Find(new Point3D(p.X, p.Y, p.Z), from.Map).GetRegion(typeof(ShrineBattleRegion));

            if (region != null && region._Controller != null)
            {
                _Controller = region._Controller;
            }

            if (!_Controller.Active && _Controller.FragmentCount < 8 &&
                ShrineDef.FirstOrDefault(x => x.Hue == Hue).Shrine == _Controller.Shrine)
            {
                if (_Timer != null)
                {
                    _Timer.Stop();
                }

                _Controller.FragmentCount++;

                from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159028, from.NetState); // *The fragment settles into the ground and surges with power as it begins to sink!*
                Effects.SendPacket(Location, Map, new GraphicalEffect(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, 0x3735, Location, Location, 1, 120, true, true));
                from.PlaySound(0x5C);
                _Timer = new FragmentTimer(this, from);
                _Timer.Start();
            }

            return drop;
        }

        public override bool OnDragLift(Mobile from)
        {
            if (_Controller != null)
            {
                _Controller.FragmentCount--;
                _Controller = null;
            }

            if (_Timer != null)
            {
                _Timer.Stop();
            }

            return true;
        }

        private class FragmentTimer : Timer
        {
            private readonly MysteriousFragment _Item;
            private readonly Mobile _Mobile;

            public FragmentTimer(MysteriousFragment item, Mobile m)
                : base(TimeSpan.FromSeconds(5.0))
            {
                _Item = item;
                _Mobile = m;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                if (_Item != null && _Item._Controller != null && _Mobile != null)
                {
                    Effects.SendPacket(_Item.Location, _Item.Map, new GraphicalEffect(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, 0x377A, _Item.Location, _Item.Location, 1, 72, true, true));

                    _Mobile.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159029, _Mobile.NetState); // *You feel a slight energy pulse through you...*
                    Effects.SendPacket(_Mobile.Location, _Mobile.Map, new GraphicalEffect(EffectType.FixedFrom, _Mobile.Serial, Serial.Zero, 0x377A, _Mobile.Location, _Mobile.Location, 1, 72, true, true));
                    _Mobile.PlaySound(0x202);

                    if (_Item._Controller.FragmentCount == 8)
                    {
                        _Item._Controller.Active = true;
                        _Item._Controller.FragmentCount = 0;
                    }

                    var cliloc = ShrineDef.FirstOrDefault(x => x.Hue == _Item.Hue).TitleCliloc;

                    if (_Mobile is PlayerMobile pm && pm.ShrineTitle != cliloc)
                    {
                        pm.ShrineTitle = cliloc;
                    }
                    _Item.Delete();
                }
            }
        }

        public MysteriousFragment(Serial serial)
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
            int version = reader.ReadInt();
        }
    }
}
