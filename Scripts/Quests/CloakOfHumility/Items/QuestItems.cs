using Server.Items;
namespace Server.Engines.Quests
{
    public class IronChain : Item
    {
        public override int LabelNumber => 1075788;  // Iron Chain

        [Constructable]
        public IronChain()
            : base(0x1F0A)
        {
            LootType = LootType.Blessed;
        }

        public IronChain(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class GreyCloak : Cloak
    {
        public override int LabelNumber => 1075789; // A Plain Grey Cloak

        [Constructable]
        public GreyCloak()
        {
            LootType = LootType.Blessed;
            Hue = 902;
        }

        public GreyCloak(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class SeasonedSkillet : Skillet
    {
        public override int LabelNumber => 1075774;  // Seasoned Skillet

        [Constructable]
        public SeasonedSkillet()
            : base(70)
        {
            LootType = LootType.Blessed;
        }

        public SeasonedSkillet(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class VillageCauldron : Item
    {
        public override int LabelNumber => 1075775; // Village Cauldron

        [Constructable]
        public VillageCauldron()
            : base(0x9ED)
        {
            LootType = LootType.Blessed;
        }

        public VillageCauldron(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class ShortStool : Item
    {
        public override int LabelNumber => 1075776;  // Short Stool

        [Constructable]
        public ShortStool()
            : base(0x11FC)
        {
            Weight = 3.0;
            LootType = LootType.Blessed;
        }

        public ShortStool(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class FriendshipMug : Item
    {
        public override int LabelNumber => 1042976; // a mug of Ale

        [Constructable]
        public FriendshipMug()
            : base(0x9EF)
        {
            LootType = LootType.Blessed;
        }

        public FriendshipMug(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class BrassRing : GoldRing
    {
        public override int LabelNumber => 1075778;  // Brass Ring

        [Constructable]
        public BrassRing()
        {
            LootType = LootType.Blessed;
        }

        public BrassRing(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class WornHammer : ProspectorsTool
    {
        public override int LabelNumber => 1075779; // Worn Hammer

        [Constructable]
        public WornHammer()
        {
            LootType = LootType.Blessed;
        }

        public WornHammer(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            list.Add(1072351); // Quest Item
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

    public class PairOfWorkGloves : LeatherGloves
    {
        public override int LabelNumber => 1075780; // Pair of Work Gloves

        [Constructable]
        public PairOfWorkGloves()
        {
            Hue = 828;
            LootType = LootType.Blessed;
        }

        public PairOfWorkGloves(Serial serial)
            : base(serial)
        {
        }

        public override bool Nontransferable => true;

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1072351); // Quest Item
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
