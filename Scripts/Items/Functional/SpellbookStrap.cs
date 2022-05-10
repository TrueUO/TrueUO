namespace Server.Items
{
    public class SpellbookStrap : BaseContainer, IDyable
    {
        public override int LabelNumber => 1159677; // Spellbook Strap

        public override int DefaultMaxItems => 25;
        public override bool DisplaysContent => false;

        [Constructable]
        public SpellbookStrap()
            : base(0xA71F)
        {
            Weight = 3.0;
        }

        public SpellbookStrap(Serial serial)
            : base(serial)
        {
        }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1073841, "{0}\t{1}\t{2}", TotalItems, MaxItems, TotalWeight);
        }

        public bool IsAccept(Item item)
        {
            if (item is Spellbook)
            {
                return true;
            }

            return false;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!IsAccept(dropped))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDrop(from, dropped);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {   
            if (!IsAccept(item))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDropInto(from, item, p);
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
