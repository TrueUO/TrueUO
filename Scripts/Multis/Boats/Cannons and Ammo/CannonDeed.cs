using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
    public enum CannonPower
    {
        Light,
        Heavy,
        Massive,
        Pumpkin
    }

    public abstract class ShipCannonDeed : Item
    {
        public abstract CannonPower CannonType { get; }

        public ShipCannonDeed() : base(5362)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                BaseGalleon galleon = BaseGalleon.FindGalleonAt(from, from.Map);

                if (galleon != null)
                {
                    if (galleon.Owner == from)
                    {
                        from.Target = new InternalTarget(this, galleon);
                    }
                    else
                    {
                        from.SendLocalizedMessage(1116627); // You must be the owner of the ship to do this.
                    }
                }
                else
                    from.SendLocalizedMessage(1116625); //You must be on the ship to deploy a weapon.
            }
        }

        private class InternalTarget : Target
        {
            public ShipCannonDeed Deed { get; }
            public BaseGalleon Galleon { get; }

            public InternalTarget(ShipCannonDeed deed, BaseGalleon galleon)
                : base(2, false, TargetFlags.None)
            {
                Deed = deed;
                Galleon = galleon;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                Map map = from.Map;

                if (targeted is IPoint3D target3D)
                {
                    Point3D pnt = new Point3D(target3D);

                    BaseGalleon galleon = BaseGalleon.FindGalleonAt(new Point2D(pnt.X, pnt.Y), map);

                    if (galleon != null && Galleon == galleon)
                    {
                        galleon.TryAddCannon(from, pnt, Deed);
                    }
                }
            }
        }

        public ShipCannonDeed(Serial serial) : base(serial) { }

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

    public class CulverinDeed : ShipCannonDeed
    {
        public override CannonPower CannonType => CannonPower.Light;
        public override int LabelNumber => 1095793;

        [Constructable]
        public CulverinDeed()
        {
            Hue = 1117;
        }

        public CulverinDeed(Serial serial) : base(serial) { }

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

    public class CarronadeDeed : ShipCannonDeed
    {
        public override CannonPower CannonType => CannonPower.Heavy;
        public override int LabelNumber => 1095794;

        [Constructable]
        public CarronadeDeed()
        {
            Hue = 1118;
        }

        public CarronadeDeed(Serial serial) : base(serial) { }

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

    public class BlundercannonDeed : ShipCannonDeed
    {
        public override CannonPower CannonType => CannonPower.Massive;
        public override int LabelNumber => 1095794;

        [Constructable]
        public BlundercannonDeed()
        {
            Hue = 1126;
        }

        public BlundercannonDeed(Serial serial) : base(serial) { }

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
