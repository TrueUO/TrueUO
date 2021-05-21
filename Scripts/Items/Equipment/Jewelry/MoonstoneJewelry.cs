using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class MoonstoneJewelry : BaseJewel
    {
        [Constructable]
        public MoonstoneJewelry()
            : this(_JewelryType.ElementAt(Utility.Random(_JewelryType.Count)))
        {
        }

        public MoonstoneJewelry(KeyValuePair<int, Layer> random)
           : base(random.Key, random.Value)
        {
            AttachSocket(new MoonstoneJewelryItem());
        }

        public static Dictionary<int, Layer> _JewelryType = new Dictionary<int, Layer>()
        {
            {0x108A, Layer.Ring},
            {0x1F09, Layer.Ring},
            {0x1089, Layer.Neck},
            {0x1F05, Layer.Neck},
            {0x1087, Layer.Earrings},
            {0x1086, Layer.Bracelet},
            {0x1F06, Layer.Bracelet},
            {0x1088, Layer.Neck},
        };

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1115189, string.Format("#{0}", LabelNumber)); // moonstone ~1_NAME~
        }

        public MoonstoneJewelry(Serial serial)
            : base(serial)
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
