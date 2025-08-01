using Server.Engines.Craft;
using System;

namespace Server.Items
{
    public class BlankScroll : Item, ICommodity, ICraftable
    {
        [Constructable]
        public BlankScroll()
            : this(1)
        {
        }

        [Constructable]
        public BlankScroll(int amount)
            : base(0xEF3)
        {
            Stackable = true;
            Amount = amount;
        }

        public BlankScroll(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 1.0;

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

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

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
        {
            Amount = 5;
            return 1;
        }
    }
}
