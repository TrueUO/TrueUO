using Server.Engines.VeteranRewards;
using System;
using System.Linq;

namespace Server.Items
{
    public class RuneBookStrap : BaseContainer, IRewardItem, IDyable
    {
        public override int LabelNumber => 1126807; // book strap

        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {
            get { return m_IsRewardItem; }
            set { m_IsRewardItem = value; InvalidateProperties(); }
        }

        public override int DefaultMaxItems => 25;

        public bool IsFull => DefaultMaxItems <= Items.Count;

        [Constructable]
        public RuneBookStrap()
            : base(0xA721)
        {
            LootType = LootType.Blessed;
            Weight = 3.0;
        }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;
            return true;
        }

        public override bool DisplaysContent => false;

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            if (m_IsRewardItem)
                list.Add(1076219); // 3rd Year Veteran Reward

            base.AddWeightProperty(list);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);            

            list.Add(1072241, "{0}\t{1}\t{2}\t{3}", TotalItems, MaxItems, TotalWeight, MaxWeight);

            list.Add(1072210, "30"); // Weight reduction: ~1_PERCENTAGE~%            
        }

        public bool IsAccept(Item item)
        {
            return _AcceptList.Any(t => t == item.GetType());
        }

        private readonly Type[] _AcceptList =
        {
            typeof(Runebook), typeof(RunicAtlas)
        };

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!IsAccept(dropped))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDrop(from, dropped);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!IsAccept(item))
            {
                from.SendLocalizedMessage(1074836); // The container cannot hold that type of object.
                return false;
            }

            return base.OnDragDropInto(from, item, p);
        }

        public override int GetTotal(TotalType type)
        {
            if (type == TotalType.Weight)
            {
                int weight = base.GetTotal(type);

                if (weight > 0)
                    return (int)Math.Max(1, (base.GetTotal(type) * 0.3));
            }

            return base.GetTotal(type);
        }

        public RuneBookStrap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_IsRewardItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_IsRewardItem = reader.ReadBool();
        }
    }
}
