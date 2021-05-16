using Server.Targeting;
using System;

namespace Server.Items
{
    public interface ICommodity /* added IsDeedable prop so expansion-based deedables can determine true/false */
    {
        TextDefinition Description { get; }
        bool IsDeedable { get; }
    }

    public static class CommodityDeedExtensions
    {
        public static int GetAmount(this Container cont, Type type, bool recurse, bool includeDeeds)
        {
            int amount = cont.GetAmount(type, recurse);

            Item[] deeds = cont.FindItemsByType(typeof(CommodityDeed), recurse);

            for (var index = 0; index < deeds.Length; index++)
            {
                var item = deeds[index];
                var deed = (CommodityDeed) item;

                if (deed.Commodity == null)
                {
                    continue;
                }

                if (deed.Commodity.GetType() == type)
                {
                    amount += deed.Commodity.Amount;
                }
            }

            return amount;
        }

        public static int GetAmount(this Container cont, Type[] types, bool recurse, bool includeDeeds)
        {
            int amount = cont.GetAmount(types, recurse);

            Item[] deeds = cont.FindItemsByType(typeof(CommodityDeed), recurse);

            for (var index = 0; index < deeds.Length; index++)
            {
                var item = deeds[index];
                var deed = (CommodityDeed) item;

                if (deed.Commodity == null)
                {
                    continue;
                }

                for (var i = 0; i < types.Length; i++)
                {
                    Type type = types[i];

                    if (deed.Commodity.GetType() == type)
                    {
                        amount += deed.Commodity.Amount;
                        break;
                    }
                }
            }

            return amount;
        }

        public static int ConsumeTotal(this Container cont, Type type, int amount, bool recurse, bool includeDeeds)
        {
            int left = amount;

            Item[] items = cont.FindItemsByType(type, recurse);

            for (var index = 0; index < items.Length; index++)
            {
                Item item = items[index];

                if (item.Amount <= left)
                {
                    left -= item.Amount;
                    item.Delete();
                }
                else
                {
                    item.Amount -= left;
                    left = 0;
                    break;
                }
            }

            if (!includeDeeds)
            {
                return amount - left;
            }

            Item[] deeds = cont.FindItemsByType(typeof(CommodityDeed), recurse);

            for (var index = 0; index < deeds.Length; index++)
            {
                var item = deeds[index];
                var deed = (CommodityDeed) item;

                if (deed.Commodity == null)
                {
                    continue;
                }

                if (deed.Commodity.GetType() != type)
                {
                    continue;
                }

                if (deed.Commodity.Amount <= left)
                {
                    left -= deed.Commodity.Amount;
                    deed.Delete();
                }
                else
                {
                    deed.Commodity.Amount -= left;
                    deed.InvalidateProperties();
                    left = 0;
                    break;
                }
            }

            return amount - left;
        }
    }

    public class CommodityDeed : Item
    {
        public CommodityDeed(Item commodity)
            : base(0x14F0)
        {
            Weight = 1.0;
            Hue = 0x47;
            LootType = LootType.Blessed;

            Commodity = commodity;
        }

        [Constructable]
        public CommodityDeed()
            : this(null)
        {
        }

        public CommodityDeed(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Item Commodity { get; private set; }


        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (Commodity == null)
            {
                list.Add(1047016);
            }
            else if(Commodity is ICommodity c)
            {
            	if (c.Description.Number > 0)
	            {
	                list.Add(1115599, $"{Commodity.Amount}\t#{c.Description.Number}");
	            }
	            else if (c.Description.String != null)
	            {
	                list.Add(1115599, $"{Commodity.Amount}\t{c.Description.String}");
	            }
	            else
	            {
	                list.Add(1115599, $"{Commodity.Amount}\t#{Commodity.LabelNumber}");
	            }
            }
            else
            {
                list.Add(1115599, $"{Commodity.Amount}\t#{Commodity.LabelNumber}");
            }
        }

        public bool SetCommodity(Item item)
        {
            InvalidateProperties();

            if (Commodity == null && item is ICommodity iItem && iItem.IsDeedable)
            {
                Commodity = item;
                Commodity.Internalize();
                Hue = 0x592;

                InvalidateProperties();

                return true;
            }

            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write(Commodity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Commodity = reader.ReadItem();
        }

        public override void OnDelete()
        {
            if (Commodity != null)
                Commodity.Delete();

            base.OnDelete();
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (Commodity != null)
            {
                list.Add(1060747); // filled
            }
            else
            {
                list.Add(1060748); // unfilled
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            int number;

            BankBox box = from.FindBankNoCreate();
            CommodityDeedBox cox = CommodityDeedBox.Find(this);
            GalleonHold hold = RootParent as GalleonHold;

            // Veteran Rewards mods
            if (Commodity != null)
            {
                if (box != null && IsChildOf(box))
                {
                    number = 1047031; // The commodity has been redeemed.

                    box.DropItem(Commodity);

                    Commodity = null;
                    Delete();
                }
                else if (cox != null)
                {
                    if (cox.IsSecure)
                    {
                        number = 1047031; // The commodity has been redeemed.

                        cox.DropItem(Commodity);

                        Commodity = null;
                        Delete();
                    }
                    else
                        number = 1080525; // The commodity deed box must be secured before you can use it.
                }
                else if (hold != null)
                {
                    number = 1047031; // The commodity has been redeemed.

                    hold.DropItem(Commodity);
                    Commodity = null;

                    Delete();
                }
                else
                {
                    number = 1080526; // That must be in your bank box or commodity deed box to use it.
                }
            }
            else if (cox != null && !cox.IsSecure)
            {
                number = 1080525; // The commodity deed box must be secured before you can use it.
            }
            else if ((box == null || !IsChildOf(box)) && cox == null && hold == null)
            {
                number = 1080526; // That must be in your bank box or commodity deed box to use it.
            }
            else
            {
                number = 1047029; // Target the commodity to fill this deed with.

                from.Target = new InternalTarget(this);
            }

            from.SendLocalizedMessage(number);
        }

        private class InternalTarget : Target
        {
            private readonly CommodityDeed m_Deed;

            public InternalTarget(CommodityDeed deed)
                : base(3, false, TargetFlags.None)
            {
                m_Deed = deed;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Deed.Deleted)
                {
                    return;
                }

                int number;

                if (m_Deed.Commodity != null)
                {
                    number = 1047028; // The commodity deed has already been filled.
                }
                else if (targeted is Item item)
                {
                    BankBox box = from.FindBankNoCreate();
                    CommodityDeedBox cox = CommodityDeedBox.Find(m_Deed);

                    // Veteran Rewards mods
                    if (box != null && m_Deed.IsChildOf(box) && item.IsChildOf(box) || cox != null && cox.IsSecure && item.IsChildOf(cox) || item.RootParent is GalleonHold)
                    {
                        if (m_Deed.SetCommodity(item))
                        {
                            number = 1047030; // The commodity deed has been filled.
                        }
                        else
                        {
                            number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
                        }
                    }
                    else
                    {
                        number = 1080526; // That must be in your bank box or commodity deed box to use it.
                    }
                }
                else
                {
                    number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
                }

                from.SendLocalizedMessage(number);
            }
        }
    }
}
