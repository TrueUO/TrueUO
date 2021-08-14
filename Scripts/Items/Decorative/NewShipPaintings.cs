namespace Server.Items
{
    public class NewShipPaintings : Item, IShipwreckedItem, IFlipable
    {
        public override int LabelNumber => 1126145; // painting

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipwreckName { get; set; }

        [Constructable]
        public NewShipPaintings()
            : base(Utility.RandomList(0xA2DC, 0xA2DD, 0xA2DE, 0xA2DF, 0xA2E0, 0xA2E1, 0xA2E2, 0xA2E3, 0xA2E4, 0xA2E5, 0xA2E6,
                0xA2E7, 0xA2E8, 0xA2E9, 0xA2EA, 0xA2EB, 0xA2EC, 0xA2ED, 0xA2EE, 0xA2EF, 0xA2F0, 0xA2F1, 0xA2F2, 0xA2F3, 0xA2F4, 0xA2F5))
        {
        }

        public NewShipPaintings(Serial serial)
            : base(serial)
        {
        }

        public virtual void OnFlip(Mobile m)
        {
            switch (ItemID)
            {
                case 0xA2DC: ItemID = 0xA2DD; break;
                case 0xA2DD: ItemID = 0xA2DC; break;
                case 0xA2DE: ItemID = 0xA2DF; break;
                case 0xA2DF: ItemID = 0xA2DE; break;
                case 0xA2E0: ItemID = 0xA2E1; break;
                case 0xA2E1: ItemID = 0xA2E0; break;
                case 0xA2E2: ItemID = 0xA2E3; break;
                case 0xA2E3: ItemID = 0xA2E2; break;
                case 0xA2E4: ItemID = 0xA2E5; break;
                case 0xA2E5: ItemID = 0xA2E4; break;
                case 0xA2E6: ItemID = 0xA2E7; break;
                case 0xA2E7: ItemID = 0xA2E6; break;
                case 0xA2E8: ItemID = 0xA2E9; break;
                case 0xA2E9: ItemID = 0xA2E8; break;
                case 0xA2EA: ItemID = 0xA2EB; break;
                case 0xA2EB: ItemID = 0xA2EA; break;
                case 0xA2EC: ItemID = 0xA2ED; break;
                case 0xA2ED: ItemID = 0xA2EC; break;
                case 0xA2EE: ItemID = 0xA2EF; break;
                case 0xA2EF: ItemID = 0xA2EE; break;
                case 0xA2F0: ItemID = 0xA2F1; break;
                case 0xA2F1: ItemID = 0xA2F0; break;
                case 0xA2F2: ItemID = 0xA2F3; break;
                case 0xA2F3: ItemID = 0xA2F2; break;
                case 0xA2F4: ItemID = 0xA2F5; break;
                case 0xA2F5: ItemID = 0xA2F4; break;
            }
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
