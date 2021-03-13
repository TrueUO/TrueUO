namespace Server.Items
{
    public class Transmogrified : ItemSocket
    {
        public override void GetCraftedProperties(ObjectPropertyList list)
        {
            list.Add(1159561); // Transmogrified
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
        }
    }
}
