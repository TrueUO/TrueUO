using System;

namespace Server.Items
{
    public class Candelabra : BaseLight, IShipwreckedItem
    {
        public override int LitItemID => 0xB1D;
        public override int UnlitItemID => 0xA27;

        [Constructable]
        public Candelabra()
            : base(0xA27)
        {
            Duration = TimeSpan.Zero; // Never burnt out
            Burning = false;
            Light = LightType.Circle225;
            Weight = 3.0;
        }

        public Candelabra(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2);

            writer.Write(ShipwreckName);
            writer.Write(IsShipwreckedItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        ShipwreckName = reader.ReadString();
                        break;
                    }
                case 1:
                    {
                        IsShipwreckedItem = reader.ReadBool();
                        break;
                    }
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

        #region IShipwreckedItem Members
        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipwreckName { get; set; }
        #endregion
    }
}
