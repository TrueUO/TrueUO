namespace Server.Items
{
    [Flipable(0xA325, 0xA327)]
    public class ShipPorthole : Item, IShipwreckedItem
    {
        public override int LabelNumber => 1125789;  // porthole

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipwreckName { get; set; }

        [Constructable]
        public ShipPorthole()
            : base(0xA325)
        {
            Weight = 1.0;
        }

        public ShipPorthole(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (IsShipwreckedItem)
            {
                if (string.IsNullOrEmpty(ShipwreckName))
                {
                    list.Add(1041645); // recovered from a shipwreck                    
                }
                else
                {
                    list.Add(1159011, ShipwreckName); // Recovered from the Shipwreck of ~1_NAME~
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 1))
            {
                return;
            }

            switch (ItemID)
            {
                case 0xA325:
                    ItemID = 0xA326;
                    break;
                case 0xA327:
                    ItemID = 0xA328;
                    break;
                case 0xA326:
                    ItemID = 0xA325;
                    break;
                case 0xA328:
                    ItemID = 0xA327;
                    break;
                default:
                    return;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(ShipwreckName);
            writer.Write(IsShipwreckedItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            ShipwreckName = reader.ReadString();
            IsShipwreckedItem = reader.ReadBool();
        }
    }
}
