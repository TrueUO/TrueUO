using Server.Engines.VeteranRewards;
using System;

namespace Server.Items
{
    public class RunebookStrap : BaseContainer, IRewardItem, IDyable
    {
        public override int LabelNumber => 1159676; // Runebook Strap

        private bool m_IsRewardItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem { get => m_IsRewardItem; set { m_IsRewardItem = value; InvalidateProperties(); } }

        public override int DefaultMaxItems => 25;
        public override bool DisplaysContent => false;

        [Constructable]
        public RunebookStrap()
            : base(0xA721)
        {
            LootType = LootType.Blessed;
            Weight = 3.0;
        }

        public RunebookStrap(Serial serial)
            : base(serial)
        {
        }

        public virtual bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;
            return true;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            if (m_IsRewardItem)
                list.Add(1076219); // 3rd Year Veteran Reward

            base.AddWeightProperty(list);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);            

            list.Add(1072241, $"{TotalItems}\t{MaxItems}\t{TotalWeight}\t{MaxWeight}");

            list.Add(1072210, "30"); // Weight reduction: ~1_PERCENTAGE~%            
        }

        public bool IsAccept(Item item)
        {
            if (item is Runebook)
            {
                return true;
            }

            return false;
        }

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
            return type == TotalType.Weight ? GetReducedWeight(base.GetTotal(type)) : base.GetTotal(type);
        }

        public override void UpdateTotal(Item sender, TotalType type, int delta)
        {
            if (type != TotalType.Weight || sender == this || delta == 0)
            {
                base.UpdateTotal(sender, type, delta);
                return;
            }

            int oldRawWeight = base.GetTotal(TotalType.Weight);
            int oldReducedWeight = GetReducedWeight(oldRawWeight);

            base.UpdateTotal(sender, type, delta);

            int newRawWeight = base.GetTotal(TotalType.Weight);
            int newReducedWeight = GetReducedWeight(newRawWeight);

            int reducedDelta = newReducedWeight - oldReducedWeight;
            int correction = reducedDelta - delta;

            if (correction != 0)
            {
                base.UpdateTotal(this, type, correction);
            }
        }

        private int GetReducedWeight(int rawWeight)
        {
            if (rawWeight <= 0)
            {
                return 0;
            }

            return (int)Math.Max(1, rawWeight * 0.3);
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
