using Server.Engines.Craft;
using System;

namespace Server.Items
{
    [Flipable(0x3158, 0x3159)]
    public class MountedDreadHorn : Item, ICraftable
    {
        public override int LabelNumber => 1074464;// mounted Dread Horn

        [Constructable]
        public MountedDreadHorn()
            : base(0x3158)
        {
            Weight = 1.0;
        }

        public MountedDreadHorn(Serial serial)
            : base(serial)
        {
        }

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            Hue = CraftResources.GetHue(CraftResources.GetFromType(resourceType));

            return 1;
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
