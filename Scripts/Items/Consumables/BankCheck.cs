using Server.Accounting;

namespace Server.Items
{
    public class BankCheck : Item
    {
        private int m_Worth;

        public BankCheck(Serial serial)
            : base(serial)
        { }

        [Constructable]
        public BankCheck(int worth)
            : base(0x14F0)
        {
            Weight = 1.0;
            Hue = 0x34;
            LootType = LootType.Blessed;

            m_Worth = worth;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Worth
        {
            get => m_Worth;
            set
            {
                m_Worth = value;
                InvalidateProperties();
            }
        }

        public override int LabelNumber => 1041361;  // A bank check

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Worth);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Worth = reader.ReadInt();
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060738, m_Worth.ToString("#,0")); // value: ~1_val~
        }

#if NEWPARENT
		public override void OnAdded(IEntity parent)
#else
        public override void OnAdded(object parent)
#endif
        {
            base.OnAdded(parent);

            Mobile owner = null;

            Container root = parent as Container;

            while (root != null && root.Parent is Container container)
            {
                root = container;
            }

            parent = root ?? parent;

            if (parent is BankBox box && AccountGold.ConvertOnBank)
            {
                owner = box.Owner;
            }

            if (owner == null || owner.Account == null || !owner.Account.DepositGold(Worth))
            {
                return;
            }

            owner.SendLocalizedMessage(1042763, Worth.ToString("#,0"));

            Delete();

            ((Container)parent).UpdateTotals();
        }

        public override void OnDoubleClick(Mobile from)
        {
            Container box = from.Backpack;

            if (box == null || !IsChildOf(box))
            {
                from.SendLocalizedMessage(1080058); // This must be in your backpack to use it.
                return;
            }

            Delete();

            if (from.Account != null)
            {
                from.Account.DepositGold(m_Worth);
            }

            // Gold was deposited in your account:
            from.SendLocalizedMessage(1042672, true, m_Worth.ToString("#,0"));
        }
    }
}
