namespace Server.Items
{
    public class BaseCannonball : Item, ICommodity, ICannonAmmo
    {
        public override int LabelNumber => 1116266; // cannonball
        public override double DefaultWeight => 1.0;

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public virtual AmmunitionType AmmoType => AmmunitionType.Cannonball;

        [Constructable]
        public BaseCannonball(int itemID)
            : this(1, itemID)
        {
        }

        [Constructable]
        public BaseCannonball(int amount, int itemid)
            : base(itemid)
        {
            Stackable = true;
            Amount = amount;
        }

        public BaseCannonball(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class Cannonball : BaseCannonball, ICommodity
    {
        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        [Constructable]
        public Cannonball() : this(1)
        {
        }

        [Constructable]
        public Cannonball(int amount) : this(amount, 16932)
        {
        }

        [Constructable]
        public Cannonball(int amount, int itemid)
            : base(itemid)
        {
            Stackable = true;
            Amount = amount;
        }

        public Cannonball(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class FrostCannonball : BaseCannonball, ICommodity
    {
        public override int LabelNumber => 1116762; // frost cannonball

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override AmmunitionType AmmoType => AmmunitionType.FrostCannonball;

        [Constructable]
        public FrostCannonball() : this(1)
        {
        }

        [Constructable]
        public FrostCannonball(int amount) : this(amount, 16939)
        {
        }

        [Constructable]
        public FrostCannonball(int amount, int itemid)
            : base(itemid)
        {
            Stackable = true;
            Amount = amount;
        }

        public FrostCannonball(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class FlameCannonball : BaseCannonball, ICommodity
    {
        public override int LabelNumber => 1116759; // flame cannonball

        TextDefinition ICommodity.Description => LabelNumber;
        bool ICommodity.IsDeedable => true;

        public override AmmunitionType AmmoType => AmmunitionType.FlameCannonball;

        [Constructable]
        public FlameCannonball() : this(1)
        {
        }

        [Constructable]
        public FlameCannonball(int amount) : this(amount, 17601)
        {
        }

        [Constructable]
        public FlameCannonball(int amount, int itemid)
            : base(itemid)
        {
            Stackable = true;
            Amount = amount;
        }

        public FlameCannonball(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
