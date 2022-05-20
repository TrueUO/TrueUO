using System;

namespace Server.Items
{
    public class GargoyleCandelabra : BaseLight, IShipwreckedItem
    {
        public override int LitItemID => 0x40BE;
        public override int UnlitItemID => 0x4039;

        [Constructable]
        public GargoyleCandelabra()
            : base(0x4039)
        {
            Duration = TimeSpan.Zero; // Never burnt out
            Burning = false;
            Light = LightType.Circle225;
            Weight = 3.0;
        }

        public GargoyleCandelabra(Serial serial)
            : base(serial)
        {
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

        #region IShipwreckedItem Members
        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipwreckName { get; set; }
        #endregion
    }
}
