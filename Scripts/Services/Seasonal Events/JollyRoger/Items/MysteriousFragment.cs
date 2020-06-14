using Server.Engines.JollyRoger;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class MysteriousFragment : Item
    {
        public override int LabelNumber => 1159025;  // mysterious fragment

        private Timer m_Timer;

        [Constructable]
        public MysteriousFragment()
            : base(0x1F13)
        {
            Hue = Utility.RandomList(_Color.Values.ToArray());
        }

        private static readonly Dictionary<Shrine, int> _Color = new Dictionary<Shrine, int>()
        {
            { Shrine.Spirituality, 2500 },
            { Shrine.Compassion, 1912 },
            { Shrine.Honor, 1918 },
            { Shrine.Honesty, 1916 },
            { Shrine.Humility, 1910 },
            { Shrine.Justice, 1914 },
            { Shrine.Valor, 1920 },
            { Shrine.Sacrifice, 1922 }
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

            if (region != null && region._Controller != null && !region._Controller.Enabled && _Color.ToList().Find(x => x.Value == Hue).Key == region._Controller.Shrine)
            {
                if (m_Timer != null)
                {
                    m_Timer.Stop();
                }

                from.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159028, from.NetState); // *The fragment settles into the ground and surges with power as it begins to sink!*
                Effects.SendPacket(Location, Map, new GraphicalEffect(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, 0x3735, Location, Location, 1, 120, true, true));
                from.PlaySound(0x5C);
                m_Timer = new FragmentTimer(region._Controller, this, from);
                m_Timer.Start();
            }

            return drop;
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
            }

            return true;
        }

        public override bool OnDragLift(Mobile from)
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
            }

            return true;
        }

        private class FragmentTimer : Timer
        {
            private readonly ShrineBattleController _Controller;
            private readonly Item _Item;
            private readonly Mobile _Mobile;

            public FragmentTimer(ShrineBattleController controller, Item item, Mobile m)
                : base(TimeSpan.FromSeconds(5.0))
            {
                _Controller = controller;
                _Item = item;
                _Mobile = m;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                if (_Controller != null && _Item != null && _Mobile != null)
                {
                    _Controller.FragmentCount++;

                    _Item.Delete();
                    Effects.SendPacket(_Item.Location, _Item.Map, new GraphicalEffect(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, 0x377A, _Item.Location, _Item.Location, 1, 72, true, true));

                    _Mobile.PrivateOverheadMessage(MessageType.Regular, 0x47E, 1159029, _Mobile.NetState); // *You feel a slight energy pulse through you...*
                    Effects.SendPacket(_Mobile.Location, _Mobile.Map, new GraphicalEffect(EffectType.FixedFrom, _Mobile.Serial, Serial.Zero, 0x377A, _Mobile.Location, _Mobile.Location, 1, 72, true, true));
                    _Mobile.PlaySound(0x202);

                    if (_Mobile is PlayerMobile pm)
                    {
                        pm.ShrineTitle = 1159320 + (int)_Controller.Shrine;
                    }
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
