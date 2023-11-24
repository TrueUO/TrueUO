namespace Server.Items
{
    public class Transmogrified : ItemSocket
    {
        public string SourceName { get; set; }

        public override void GetCraftedProperties(ObjectPropertyList list)
        {
            if (Owner.Layer == Layer.Helm)
            {
                if (string.IsNullOrWhiteSpace(SourceName))
                {
                    list.Add(1159679, $"#{Owner.LabelNumber}"); // <BASEFONT COLOR=#b66dff>Transmogrified ~1_ITEM~<BASEFONT COLOR=#FFFFFF>
                }
                else
                {
                    list.Add(1159679, SourceName); // <BASEFONT COLOR=#b66dff>Transmogrified ~1_ITEM~<BASEFONT COLOR=#FFFFFF>
                }
            }                
            else
            {
                list.Add(1159561); // Transmogrified
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(SourceName);
        }

        public override void Deserialize(Item owner, GenericReader reader)
        {
            base.Deserialize(owner, reader);
            reader.ReadInt();

            SourceName = reader.ReadString();
        }
    }
}
