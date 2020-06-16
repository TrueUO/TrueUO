using Server.Engines.JollyRoger;

namespace Server.Items
{
    public class Tabard : BaseClothing
    {
        public Shrine _Shrine { get; set; }

        [Constructable]
        public Tabard(Shrine shrine)
            : base(0xA412, Layer.OuterTorso)
        {
            _Shrine = shrine;

            Weight = 3;
            Hue = MysteriousFragment._Color[shrine];
        }

        public Tabard(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1159369, string.Format("the {0}", _Shrine.ToString())); // Tabard of ~1_VIRTUE~
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}

