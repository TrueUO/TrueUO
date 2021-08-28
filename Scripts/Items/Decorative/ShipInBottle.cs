namespace Server.Items
{
    [Flipable(0xA321, 0xA322)]
    public class ShipInBottle : Item, IShipwreckedItem
    {
        public override int LabelNumber => 1125786;  // ship in a bottle

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipwreckName { get; set; }

        [Constructable]
        public ShipInBottle()
            : base(0xA321)
        {
            Weight = 1.0;
        }

        public ShipInBottle(Serial serial)
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
