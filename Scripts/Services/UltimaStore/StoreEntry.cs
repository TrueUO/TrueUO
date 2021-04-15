using Server.Items;
using System;

namespace Server.Engines.UOStore
{
    public class StoreEntry
    {
        public Type ItemType { get; }
        public TextDefinition[] Name { get; }
        public int Tooltip { get; }
        public int GumpID { get; }
        public int ItemID { get; }
        public int Hue { get; }
        public int Price { get; }
        public StoreCategory Category { get; }
        public Func<Mobile, StoreEntry, Item> Constructor { get; }

        public int Cost => (int)Math.Ceiling(Price * Configuration.CostMultiplier);

        public StoreEntry(Type itemType, TextDefinition name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
            : this(itemType, new[] { name }, tooltip, itemID, gumpID, hue, cost, cat, constructor)
        { }

        public StoreEntry(Type itemType, TextDefinition[] name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
        {
            ItemType = itemType;
            Name = name;
            Tooltip = tooltip;
            ItemID = itemID;
            GumpID = gumpID;
            Hue = hue;
            Price = cost;
            Category = cat;
            Constructor = constructor;
        }

        public bool Construct(Mobile m, bool test = false)
        {
            Item item;

            if (Constructor != null)
            {
                item = Constructor(m, this);
            }
            else
            {
                item = Activator.CreateInstance(ItemType) as Item;
            }

            if (item != null)
            {
                if (item is IAccountRestricted restricted)
                {
                    restricted.Account = m.Account.Username;
                }

                if (m.Backpack == null || !m.Alive || !m.Backpack.TryDropItem(m, item, false))
                {
                    UltimaStore.AddPendingItem(m, item);

                    // Your purchased will be delivered to you once you free up room in your backpack.
                    // Your purchased item will be delivered to you once you are resurrected.
                    m.SendLocalizedMessage(m.Alive ? 1156846 : 1156848);
                }
                else if (item is IPromotionalToken && ((IPromotionalToken)item).ItemName != null)
                {
                    // A token has been placed in your backpack. Double-click it to redeem your ~1_PROMO~.
                    m.SendLocalizedMessage(1075248, ((IPromotionalToken)item).ItemName.ToString());
                }
                else if (item.LabelNumber > 0 || item.Name != null)
                {
                    string name = item.LabelNumber > 0 ? ("#" + item.LabelNumber) : item.Name;

                    m.SendLocalizedMessage(1156844, name); // Your purchase of ~1_ITEM~ has been placed in your backpack.
                }
                else
                {
                    m.SendLocalizedMessage(1156843); // Your purchased item has been placed in your backpack.
                }

                if (test)
                {
                    item.Delete();
                }

                return true;
            }

            Utility.WriteConsoleColor(ConsoleColor.Red, string.Format("[Ultima Store Warning]: {0} failed to construct.", ItemType.Name));

            return false;
        }
    }
}
