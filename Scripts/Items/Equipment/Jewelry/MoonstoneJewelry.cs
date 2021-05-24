using System;
using System.Collections.Generic;

namespace Server.Items
{
    public static class MoonstoneJewelry
    {
        private static readonly List<BaseJewel> _JewelryList = new List<BaseJewel>();

        public static readonly List<BaseJewel> JewelryList = _JewelryList;

        public static void Initialize()
        {
            Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), () =>
            {
                for (var index = 0; index < _JewelryList.Count; index++)
                {
                    var j = _JewelryList[index];

                    if (j == null || j.Deleted || j.Map == Map.Internal)
                    {
                        continue;
                    }

                    ChangeHue(j);
                }
            });
        }

        public static void ChangeHue(Item item)
        {
            var p = item.Location;
            var map = item.Map;
            var hue = 960;

            if (item.RootParent is Mobile m)
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

            item.Hue = hue;
        }
    }

    public class MoonstoneBracelet : BaseBracelet
    {
        [Constructable]
        public MoonstoneBracelet()
            : base(Utility.RandomList(0x1086, 0x1F06))
        {
            MoonstoneJewelry.JewelryList.Add(this);
        }

        public MoonstoneBracelet(Serial serial)
            : base(serial)
        {
        }  

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1115189, string.Format("#{0}", LabelNumber)); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            base.Delete();

            if (MoonstoneJewelry.JewelryList.Contains(this))
                MoonstoneJewelry.JewelryList.Remove(this);
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

            MoonstoneJewelry.JewelryList.Add(this);
        }
    }

    public class MoonstoneEarrings : BaseEarrings
    {
        [Constructable]
        public MoonstoneEarrings()
            : base(0x1087)
        {
            MoonstoneJewelry.JewelryList.Add(this);
        }

        public MoonstoneEarrings(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1115189, string.Format("#{0}", LabelNumber)); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            base.Delete();

            if (MoonstoneJewelry.JewelryList.Contains(this))
                MoonstoneJewelry.JewelryList.Remove(this);
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

            MoonstoneJewelry.JewelryList.Add(this);
        }
    }

    public class MoonstoneRing : BaseRing
    {
        [Constructable]
        public MoonstoneRing()
            : base(Utility.RandomList(0x108A, 0x1F09))
        {
            MoonstoneJewelry.JewelryList.Add(this);
        }

        public MoonstoneRing(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1115189, string.Format("#{0}", LabelNumber)); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            base.Delete();

            if (MoonstoneJewelry.JewelryList.Contains(this))
                MoonstoneJewelry.JewelryList.Remove(this);
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

            MoonstoneJewelry.JewelryList.Add(this);
        }
    }

    public class MoonstoneNecklace : BaseNecklace
    {
        [Constructable]
        public MoonstoneNecklace()
            : base(Utility.RandomList(0x1088, 0x1089, 0x1F05))
        {
            MoonstoneJewelry.JewelryList.Add(this);
        }

        public MoonstoneNecklace(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1115189, string.Format("#{0}", LabelNumber)); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            base.Delete();

            if (MoonstoneJewelry.JewelryList.Contains(this))
                MoonstoneJewelry.JewelryList.Remove(this);
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

            MoonstoneJewelry.JewelryList.Add(this);
        }
    }
}
