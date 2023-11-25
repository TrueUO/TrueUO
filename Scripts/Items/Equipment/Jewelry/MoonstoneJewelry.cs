using System;
using System.Collections.Generic;

namespace Server.Items
{
    public static class MoonstoneJewelry
    {
        private static readonly List<BaseJewel> _JewelryList = new List<BaseJewel>();

        public static readonly List<BaseJewel> JewelryList = _JewelryList;

        public static int FeluccaHueIndex { get; set; }
        public static int TrammelHueIndex { get; set; }

        public static void Initialize()
        {
            Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), () =>
            {
                if (_JewelryList.Count != 0)
                {
                    IndexCalculate();

                    for (var index = 0; index < _JewelryList.Count; index++)
                    {
                        var j = _JewelryList[index];

                        if (j == null || j.Deleted || j.Map == Map.Internal)
                        {
                            continue;
                        }

                        ChangeHue(j);
                    }
                }
            });
        }

        private static readonly int[] TrammelHueArray = new int[] { 99, 299, 499, 699, 934, 699, 499, 299 };
        private static readonly int[] FeluccaHueArray = new int[] { 34, 234, 434, 634, 934, 634, 434, 234 };

        public static void IndexCalculate()
        {
            if (TrammelHueIndex < TrammelHueArray.Length - 1)
            {
                TrammelHueIndex++;
            }
            else
            {
                TrammelHueIndex = 0;
            }

            if (FeluccaHueIndex < FeluccaHueArray.Length - 1)
            {
                FeluccaHueIndex++;
            }
            else
            {
                FeluccaHueIndex = 0;
            }
        }

        public static void ChangeHue(Item item)
        {
            var map = item.Map;
            var hue = 934;

            if (map == Map.Felucca || map == Map.Trammel)
            {
                if (map == Map.Felucca)
                {
                    hue = FeluccaHueArray[FeluccaHueIndex];
                }

                if (map == Map.Trammel)
                {
                    hue = TrammelHueArray[TrammelHueIndex];
                }
            }

            item.Hue = hue;
        }

        public static void RemoveList(BaseJewel jewel)
        {
            JewelryList.Remove(jewel);
        }
    }

    public class MoonstoneBracelet : BaseBracelet
    {
        public override bool IsArtifact => true;

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
            list.Add(1115189, $"#{LabelNumber}"); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            MoonstoneJewelry.RemoveList(this);

            base.Delete();
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
        public override bool IsArtifact => true;

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
            list.Add(1115189, $"#{LabelNumber}"); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            MoonstoneJewelry.RemoveList(this);

            base.Delete();
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
        public override bool IsArtifact => true;

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
            list.Add(1115189, $"#{LabelNumber}"); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            MoonstoneJewelry.RemoveList(this);

            base.Delete();
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
        public override bool IsArtifact => true;

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
            list.Add(1115189, $"#{LabelNumber}"); // moonstone ~1_NAME~
        }

        public override void OnMapChange()
        {
            MoonstoneJewelry.ChangeHue(this);
        }

        public override void Delete()
        {
            MoonstoneJewelry.RemoveList(this);

            base.Delete();
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
