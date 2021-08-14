namespace Server.Items
{
    public interface IShipwreckedItem
    {
        bool IsShipwreckedItem { get; set; }
        string ShipwreckName { get; set; }
    }

    public class ShipwreckedItem : Item, IDyable, IShipwreckedItem, IFlipable
    {
        private bool m_IsBarnacleItem;

        public ShipwreckedItem(int itemID, bool barnacle)
            : base(itemID)
        {
            m_IsBarnacleItem = barnacle;

            int weight = ItemData.Weight;

            if (weight >= 255 || weight <= 0)
            {
                weight = 1;
            }

            Weight = weight;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            if (m_IsBarnacleItem)
            {
                if (LabelNumber > 0)
                {
                    list.Add(1151075, string.Format("#{0}", LabelNumber)); //barnacle covered ~1_token~
                }
                else
                {
                    list.Add(1151075, ItemData.Name); //barnacle covered ~1_token~
                }

                if (string.IsNullOrEmpty(ShipwreckName))
                {
                    list.Add(1041645); // recovered from a shipwreck                    
                }
                else
                {
                    list.Add(1159011, ShipwreckName); // Recovered from the Shipwreck of ~1_NAME~
                }
            }
            else
            {
                base.AddNameProperties(list);

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

        public virtual void OnFlip(Mobile m)
        {
            switch (ItemID)
            {
                case 0x0E9F: ItemID = 0x0EC8; break;
                case 0x0EC8: ItemID = 0x0E9F; break;
                case 0x0EC9: ItemID = 0x0EE7; break;
                case 0x0EE7: ItemID = 0x0EC9; break;
                case 0x0EA1: ItemID++; break;
                case 0x0EA2: ItemID--; break;
                case 0x0EA3: ItemID++; break;
                case 0x0EA4: ItemID--; break;
                case 0x0EA5: ItemID = 0x0EA7; break;
                case 0x0EA6: ItemID = 0x0EA8; break;
                case 0x0EA7: ItemID = 0x0EA5; break;
                case 0x0EA8: ItemID = 0x0EA6; break;
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

        public ShipwreckedItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2);

            writer.Write(ShipwreckName);
            writer.Write(m_IsBarnacleItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    ShipwreckName = reader.ReadString();
                    goto case 1;
                case 1:
                    m_IsBarnacleItem = reader.ReadBool();
                    goto case 0;
                case 0:
                    break;
            }
        }

        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            if (ItemID >= 0x13A4 && ItemID <= 0x13AE)
            {
                Hue = sender.DyedHue;
                return true;
            }

            from.SendLocalizedMessage(sender.FailMessage);
            return false;
        }

        #region IShipwreckedItem Members
        public bool IsShipwreckedItem
        {
            get => true; // It's a Shipwrecked Item item.
            set
            {
            }
        }

        public string ShipwreckName { get; set; }
        #endregion
    }
}
