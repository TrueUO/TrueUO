using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseEquipableLight : BaseLight
    {
        [Constructable]
        public BaseEquipableLight(int itemID)
            : base(itemID)
        {
            Layer = Layer.TwoHanded;
        }

        public BaseEquipableLight(Serial serial)
            : base(serial)
        {
        }

        public override void Ignite()
        {
            // Not holding the item, but it is in the mobiles backpack.
            if (Parent is not Mobile && RootParent is Mobile mobile)
            {
                // Prevents forcing their equip when double-clicked inside a pack animals backpack.
                if (mobile is BaseCreature)
                {
                    return;
                }

                if (mobile.EquipItem(this))
                {
                    if (this is Candle)
                    {
                        mobile.SendLocalizedMessage(502969); // You put the candle in your left hand.
                    }
                    else if (this is Torch)
                    {
                        mobile.SendLocalizedMessage(502971); // You put the torch in your left hand.
                    }

                    base.Ignite();
                }
                else
                {
                    mobile.SendLocalizedMessage(502449); // You cannot hold this item.
                }
            }
            else
            {
                base.Ignite();
            }
        }

        public override void OnAdded(object parent)
        {
            if (Burning && parent is Container)
            {
                Douse();
            }

            base.OnAdded(parent);
        }

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
