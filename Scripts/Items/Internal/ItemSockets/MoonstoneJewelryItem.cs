using System;

namespace Server.Items
{
    public class MoonstoneJewelryItem : ItemSocket
    {
        public override TimeSpan TickDuration => TimeSpan.FromMinutes(2);

        public MoonstoneJewelryItem()
        {
            BeginTimer();
        }

        public override void OnMapChange()
        {
            ChangeHue();
        }

        public void ChangeHue()
        {
            var p = Owner.Location;
            var map = Owner.Map;
            var hue = 960;

            if (Owner.RootParent is Mobile m)
            {
                p = m.Location;
            }

            if (map == Map.Felucca || map == Map.Trammel)
            {
                if (map == Map.Felucca)
                {
                    hue = 1628;
                }

                if (map == Map.Trammel)
                {
                    hue = 1319;
                }

                var moonhue = hue + (int)Clock.GetMoonPhase(map, p.X, p.Y);

                Clock.GetTime(map, p.X, p.Y, out int hours, out int minutes);

                if (hours >= 20)
                {
                    hue = moonhue;
                }
                else if (hours >= 19)
                {
                    hue = moonhue - 1;
                }
                else if (hours >= 18)
                {
                    hue = moonhue - 2;
                }
                else if (hours >= 17)
                {
                    hue = moonhue - 3;
                }
            }

            Owner.Hue = hue;
        }

        protected override void OnTick()
        {
            ChangeHue();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(Item owner, GenericReader reader)
        {
            base.Deserialize(owner, reader);
            reader.ReadInt();

            BeginTimer();
        }
    }
}
