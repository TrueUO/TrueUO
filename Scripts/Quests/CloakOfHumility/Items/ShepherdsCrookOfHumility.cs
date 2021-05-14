namespace Server.Items
{
    public class ShepherdsCrookOfHumility : ShepherdsCrook
    {
        public override int LabelNumber => 1075791; // A Replica of the Shepherd's Crook of Humility

        [Constructable]
        public ShepherdsCrookOfHumility()
        {
            Hue = 902;
        }

        public ShepherdsCrookOfHumility(Serial serial)
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
