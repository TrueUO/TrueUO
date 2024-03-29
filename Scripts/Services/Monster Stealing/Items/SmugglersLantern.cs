namespace Server.Items
{
    public class SmugglersLantern : Lantern
    {
        public override int LabelNumber => 1071521;  // Smuggler's Lantern

        [Constructable]
        public SmugglersLantern()
        {
            Hue = Utility.RandomMinMax(192, 291);
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            list.Add(1079766); // Spell Channeling
        }

        public SmugglersLantern(Serial serial)
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
}
