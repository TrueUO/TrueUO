using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class GenericBuyInfo : IBuyItemInfo
    {
        public static Dictionary<Type, int> BuyPrices = new Dictionary<Type, int>();
        private Type m_Type;
        private string m_Name;
        private int m_Price;
        private int m_MaxAmount, m_Amount;
        private int m_ItemID;
        private int m_Hue;
        private object[] m_Args;
        private IEntity m_DisplayEntity;
        private int m_PriceScalar;
        private bool m_Stackable;
        private int m_TotalBought;
        private int m_TotalSold;

        public GenericBuyInfo(Type type, int price, int amount, int itemID, int hue, bool stacks = false)
            : this(null, type, price, amount, itemID, hue, null, stacks)
        {
        }

        public GenericBuyInfo(string name, Type type, int price, int amount, int itemID, int hue, bool stacks = false)
            : this(name, type, price, amount, itemID, hue, null, stacks)
        {
        }

        public GenericBuyInfo(Type type, int price, int amount, int itemID, int hue, object[] args, bool stacks = false)
            : this(null, type, price, amount, itemID, hue, args, stacks)
        {
        }

        public GenericBuyInfo(string name, Type type, int price, int amount, int itemID, int hue, object[] args, bool stacks = false)
        {
            if (type != null)
            {
                BuyPrices[type] = price;
            }

            m_Type = type;
            m_Price = price;
            m_ItemID = itemID;
            m_Hue = hue;
            m_Args = args;
            m_Stackable = stacks;

            if (type != null && EconomyItem)
            {
                m_MaxAmount = m_Amount = BaseVendor.EconomyStockAmount;
            }
            else
            {
                m_MaxAmount = m_Amount = amount;
            }

            if (Siege.SiegeShard)
            {
                m_Price *= 3;
            }

            if (name == null)
            {
                m_Name = itemID < 0x4000 ? (1020000 + itemID).ToString() : (1078872 + itemID).ToString();
            }
            else
            {
                m_Name = name;
            }
        }

        public virtual int ControlSlots => 0;
        public virtual bool CanCacheDisplay => false;

        public Type Type { get => m_Type; set => m_Type = value; }
        public string Name { get => m_Name; set => m_Name = value; }

        public int DefaultPrice => m_PriceScalar;
        public int PriceScalar { get => m_PriceScalar; set => m_PriceScalar = value; }

        public int TotalBought { get => m_TotalBought; set => m_TotalBought = value; }
        public int TotalSold { get => m_TotalSold; set => m_TotalSold = value; }

        public virtual bool Stackable { get => m_Stackable; set => m_Stackable = value; }

        public bool EconomyItem => BaseVendor.UseVendorEconomy && m_Stackable;

        public int Price
        {
            get
            {
                int ecoInc = 0;

                if (EconomyItem)
                {
                    if (m_TotalBought >= BaseVendor.BuyItemChange)
                    {
                        ecoInc += m_TotalBought / BaseVendor.BuyItemChange;
                    }

                    if (m_TotalSold >= BaseVendor.SellItemChange)
                    {
                        ecoInc -= m_TotalSold / BaseVendor.SellItemChange;
                    }
                }

                if (m_PriceScalar != 0)
                {
                    if (m_Price > 5000000)
                    {
                        long price = m_Price;

                        price *= m_PriceScalar;
                        price += 50;
                        price /= 100;

                        if (price > int.MaxValue)
                            price = int.MaxValue;

                        if (EconomyItem && (int)price + ecoInc < 2)
                        {
                            return 2;
                        }

                        return (int)price + ecoInc;
                    }

                    if (EconomyItem && (m_Price * m_PriceScalar + 50) / 100 + ecoInc < 2)
                    {
                        return 2;
                    }

                    return (m_Price * m_PriceScalar + 50) / 100 + ecoInc;
                }

                if (EconomyItem && m_Price + ecoInc < 2)
                {
                    return 2;
                }

                return m_Price + ecoInc;
            }
            set => m_Price = value;
        }

        public int ItemID { get => m_ItemID; set => m_ItemID = value; }
        public int Hue { get => m_Hue; set => m_Hue = value; }

        public int Amount
        {
            get => m_Amount;
            set
            {
                // Amount is ALWAYS 500
                if (EconomyItem)
                {
                    m_Amount = BaseVendor.EconomyStockAmount;
                }
                else
                {
                    if (value < 0)
                    {
                        value = 0;
                    }

                    m_Amount = value;
                }
            }
        }
        public int MaxAmount
        {
            get
            {
                // Amount is ALWAYS 500
                if (EconomyItem)
                {
                    return BaseVendor.EconomyStockAmount;
                }

                return m_MaxAmount;
            }
            set => m_MaxAmount = value;
        }

        public object[] Args { get => m_Args; set => m_Args = value; }

        public void DeleteDisplayEntity()
        {
            if (m_DisplayEntity == null)
                return;

            m_DisplayEntity.Delete();
            m_DisplayEntity = null;
        }

        public IEntity GetDisplayEntity()
        {
            if (m_DisplayEntity != null && !IsDeleted(m_DisplayEntity))
                return m_DisplayEntity;

            bool canCache = CanCacheDisplay;

            if (canCache)
                m_DisplayEntity = DisplayCache.Cache.Lookup(m_Type);

            if (m_DisplayEntity == null || IsDeleted(m_DisplayEntity))
                m_DisplayEntity = GetEntity();

            DisplayCache.Cache.Store(m_Type, m_DisplayEntity, canCache);

            return m_DisplayEntity;
        }

        //get a new instance of an object (we just bought it)
        public virtual IEntity GetEntity()
        {
            if (m_Args == null || m_Args.Length == 0)
            {
                return (IEntity)Activator.CreateInstance(m_Type);
            }

            return (IEntity)Activator.CreateInstance(m_Type, m_Args);
        }

        //Attempt to restock with item, (return true if restock sucessful)
        public virtual bool Restock(Item item, int amount)
        {
            if (item == null || item.GetType() != m_Type)
            {
                return false;
            }

            return EconomyItem;
        }

        public virtual void OnRestock()
        {
            if (m_Amount <= 0)
            {
                m_MaxAmount *= 2;

                if (m_MaxAmount >= 999)
                    m_MaxAmount = 999;
            }
            else
            {
                /* NOTE: According to UO.com, the quantity is halved if the item does not reach 0
                * Here we implement differently: the quantity is halved only if less than half
                * of the maximum quantity was bought. That is, if more than half is sold, then
                * there's clearly a demand and we should not cut down on the stock.
                */
                int halfQuantity = m_MaxAmount;

                if (halfQuantity >= 999)
                    halfQuantity = 640;
                else if (halfQuantity > 20)
                    halfQuantity /= 2;

                if (m_Amount >= halfQuantity)
                    m_MaxAmount = halfQuantity;
            }

            m_Amount = m_MaxAmount;
        }

        private bool IsDeleted(IEntity obj)
        {
            if (obj is Item item)
                return item.Deleted;
            if (obj is Mobile mobile)
                return mobile.Deleted;

            return false;
        }

        public void OnBought(Mobile buyer, BaseVendor vendor, IEntity entity, int amount)
        {
            if (EconomyItem)
            {
                var infos = vendor.GetBuyInfo();

                for (var index = 0; index < infos.Length; index++)
                {
                    IBuyItemInfo info = infos[index];

                    if (info is GenericBuyInfo bii && (bii.Type == m_Type || m_Type == typeof(UncutCloth) && bii.Type == typeof(Cloth) || m_Type == typeof(Cloth) && bii.Type == typeof(UncutCloth)))
                    {
                        bii.TotalBought += amount;
                    }
                }
            }

            EventSink.InvokeValidVendorPurchase(new ValidVendorPurchaseEventArgs(buyer, vendor, entity, m_Price));
        }

        public void OnSold(BaseVendor vendor, int amount)
        {
            if (EconomyItem)
            {
                var infos = vendor.GetBuyInfo();

                for (var index = 0; index < infos.Length; index++)
                {
                    IBuyItemInfo info = infos[index];

                    if (info is GenericBuyInfo bii && (bii.Type == m_Type || m_Type == typeof(UncutCloth) && bii.Type == typeof(Cloth) || m_Type == typeof(Cloth) && bii.Type == typeof(UncutCloth)))
                    {
                        bii.TotalSold += amount;
                    }
                }
            }
        }

        public static bool IsDisplayCache(IEntity e)
        {
            if (e is Mobile mobile)
            {
                return DisplayCache.Cache.Mobiles != null && DisplayCache.Cache.Mobiles.Contains(mobile);
            }

            return DisplayCache.Cache.Table != null && DisplayCache.Cache.Table.ContainsValue(e);
        }

        private class DisplayCache : Container
        {
            private static DisplayCache m_Cache;
            private Dictionary<Type, IEntity> m_Table;
            private List<Mobile> m_Mobiles;

            public List<Mobile> Mobiles => m_Mobiles;
            public Dictionary<Type, IEntity> Table => m_Table;

            public DisplayCache()
                : base(0)
            {
                m_Table = new Dictionary<Type, IEntity>();
                m_Mobiles = new List<Mobile>();
            }

            public DisplayCache(Serial serial)
                : base(serial)
            {
            }

            public static DisplayCache Cache
            {
                get
                {
                    if (m_Cache == null || m_Cache.Deleted)
                        m_Cache = new DisplayCache();

                    return m_Cache;
                }
            }
            public IEntity Lookup(Type key)
            {
                IEntity e;

                m_Table.TryGetValue(key, out e);
                return e;
            }

            public void Store(Type key, IEntity obj, bool cache)
            {
                if (cache)
                    m_Table[key] = obj;

                if (obj is Item item)
                    AddItem(item);
                else if (obj is Mobile mobile)
                    m_Mobiles.Add(mobile);
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                for (int i = 0; i < m_Mobiles.Count; ++i)
                    m_Mobiles[i].Delete();

                m_Mobiles.Clear();

                for (int i = Items.Count - 1; i >= 0; --i)
                    if (i < Items.Count)
                        Items[i].Delete();

                if (m_Cache == this)
                    m_Cache = null;
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(0); // version

                writer.Write(m_Mobiles);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadInt();

                m_Mobiles = reader.ReadStrongMobileList();

                for (int i = 0; i < m_Mobiles.Count; ++i)
                    m_Mobiles[i].Delete();

                m_Mobiles.Clear();

                for (int i = Items.Count - 1; i >= 0; --i)
                    if (i < Items.Count)
                        Items[i].Delete();

                if (m_Cache == null)
                    m_Cache = this;
                else
                    Delete();

                m_Table = new Dictionary<Type, IEntity>();
            }
        }
    }

    public class GenericBuyInfo<T> : GenericBuyInfo
    {
        public Action<T, GenericBuyInfo> CreateCallback { get; }

        public GenericBuyInfo(int price, int amount, int itemID, int hue, bool stacks = false, Action<T, GenericBuyInfo> callback = null)
            : this(null, price, amount, itemID, hue, null, stacks, callback)
        {
        }

        public GenericBuyInfo(string name, int price, int amount, int itemID, int hue, bool stacks = false, Action<T, GenericBuyInfo> callback = null)
            : this(name, price, amount, itemID, hue, null, stacks, callback)
        {
        }

        public GenericBuyInfo(int price, int amount, int itemID, int hue, object[] args, bool stacks = false, Action<T, GenericBuyInfo> callback = null)
            : this(null, price, amount, itemID, hue, args, stacks, callback)
        {
        }

        public GenericBuyInfo(string name, int price, int amount, int itemID, int hue, object[] args, bool stacks = false, Action<T, GenericBuyInfo> callback = null)
            : base(name, typeof(T), price, amount, itemID, hue, args, stacks)
        {
            CreateCallback = callback;
        }

        public override IEntity GetEntity()
        {
            var entity = base.GetEntity();

            if (CreateCallback != null)
            {
                CreateCallback((T)entity, this);
            }

            return entity;
        }
    }
}
