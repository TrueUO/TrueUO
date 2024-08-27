using Server.Accounting;

namespace Server.Items
{
    public class Gold : Item
    {
        [Constructable]
        public Gold()
            : this(1)
        {
        }

        [Constructable]
        public Gold(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Gold(int amount)
            : base(0xEED)
        {
            Stackable = true;
            Amount = amount;
        }

        public Gold(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 0.02 / 3;

        public override int GetDropSound()
        {
            if (Amount <= 1)
            {
                return 0x2E4;
            }
            if (Amount <= 5)
            {
                return 0x2E5;
            }

            return 0x2E6;
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

            if (owner == null || owner.Account == null || !owner.Account.DepositGold(Amount))
            {
                return;
            }

            owner.SendLocalizedMessage(1042763, Amount.ToString("#,0"));

            Delete();

            ((Container)parent).UpdateTotals();
        }

        public override int GetTotal(TotalType type)
        {
            int baseTotal = base.GetTotal(type);

            if (type == TotalType.Gold)
            {
                baseTotal += Amount;
            }

            return baseTotal;
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

        protected override void OnAmountChange(int oldValue)
        {
            int newValue = Amount;

            UpdateTotal(this, TotalType.Gold, newValue - oldValue);
        }
    }
}
