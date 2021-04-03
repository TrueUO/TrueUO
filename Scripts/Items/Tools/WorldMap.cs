using System;

namespace Server.Items
{
    public class NavigatorsWorldMap : WorldMap
    {
        public override int LabelNumber => 1075500;  // Navigator's World Map

        [Constructable]
        public NavigatorsWorldMap()
        {
            ItemID = 0x14EB;
            LootType = LootType.Blessed;
            Hue = 483;

            SetDisplay(0, 0, 5119, 4095, 200, 200);
        }

        public NavigatorsWorldMap(Serial serial)
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
    }

    public class WorldMap : MapItem
    {
        [Constructable]
        public WorldMap()
        {
            SetDisplay(0, 0, 5100, 4084, 400, 400);
        }

        public override void CraftInit(Mobile from)
        {
            Facet = from.Map;            

            if (Facet == Map.Trammel || Facet == Map.Felucca)
            {
                double skillValue = from.Skills[SkillName.Cartography].Value;

                int x1 = 0;

                if (skillValue < 71)
                {
                    x1 = (int)(20 + (50 * Math.Abs(Math.Floor(skillValue) - 70)));
                }

                SetDisplay(x1, 0, 5100, 4084, 400, 400);
            }
            else
                SetDisplayByFacet();
        }

        public override int LabelNumber => 1015233;  // world map

        public WorldMap(Serial serial) : base(serial)
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
