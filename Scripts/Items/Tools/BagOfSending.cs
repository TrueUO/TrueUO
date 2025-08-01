using Server.ContextMenus;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public enum BagOfSendingHue
    {
        Yellow,
        Blue,
        Red,
        Green
    }

    public class BagOfSending : Item, TranslocationItem
    {
        private int m_Charges;
        private int m_Recharges;
        private BagOfSendingHue m_BagOfSendingHue;

        [Constructable]
        public BagOfSending()
            : this(RandomHue())
        {
        }

        [Constructable]
        public BagOfSending(BagOfSendingHue hue)
            : base(0xE76)
        {
            BagOfSendingHue = hue;

            m_Charges = Utility.RandomMinMax(3, 9);
        }

        public BagOfSending(Serial serial)
            : base(serial)
        {
        }

        public override double DefaultWeight => 2.0;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges
        {
            get => m_Charges;
            set
            {
                if (value > MaxCharges)
                    m_Charges = MaxCharges;
                else if (value < 0)
                    m_Charges = 0;
                else
                    m_Charges = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Recharges
        {
            get => m_Recharges;
            set
            {
                if (value > MaxRecharges)
                    m_Recharges = MaxRecharges;
                else if (value < 0)
                    m_Recharges = 0;
                else
                    m_Recharges = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxCharges => 30;
        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxRecharges => 255;

        public override int LabelNumber => 1054104; // a bag of sending

        [CommandProperty(AccessLevel.GameMaster)]
        public BagOfSendingHue BagOfSendingHue
        {
            get => m_BagOfSendingHue;
            set
            {
                m_BagOfSendingHue = value;

                switch (value)
                {
                    case BagOfSendingHue.Yellow:
                        Hue = 0x8A5;
                        break;
                    case BagOfSendingHue.Blue:
                        Hue = 0x8AD;
                        break;
                    case BagOfSendingHue.Red:
                        Hue = 0x89B;
                        break;
                    case BagOfSendingHue.Green:
                        Hue = 0x08A0;
                        break;
                }
            }
        }

        public static BagOfSendingHue RandomHue()
        {
            switch (Utility.Random(4))
            {
                case 0:
                    return BagOfSendingHue.Yellow;
                case 1:
                    return BagOfSendingHue.Blue;
                case 2:
                    return BagOfSendingHue.Red;
                default:
                    return BagOfSendingHue.Green;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060741, m_Charges.ToString()); // charges: ~1_val~
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive)
            {
                list.Add(new UseBagEntry(this, Charges > 0 && IsChildOf(from.Backpack)));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Region.IsPartOf<Regions.Jail>())
            {
                from.SendLocalizedMessage(1078497); // You cannot use that right now
            }
            else if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1054107); // This item must be in your backpack.
            }
            else if (Charges == 0)
            {
                from.SendLocalizedMessage(1042544); // This item is out of charges.
            }
            else
            {
                from.SendLocalizedMessage(1150597); // Select the item you wish to send to your bank box.
                from.Target = new SendTarget(this);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(1); // version

            writer.WriteEncodedInt(m_Recharges);
            writer.WriteEncodedInt(m_Charges);
            writer.WriteEncodedInt((int)m_BagOfSendingHue);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();

            m_Recharges = reader.ReadEncodedInt();
            m_Charges = Math.Min(reader.ReadEncodedInt(), MaxCharges);
            m_BagOfSendingHue = (BagOfSendingHue)reader.ReadEncodedInt();
        }

        private class UseBagEntry : ContextMenuEntry
        {
            private readonly BagOfSending m_Bag;

            public UseBagEntry(BagOfSending bag, bool enabled)
                : base(6189)
            {
                m_Bag = bag;

                if (!enabled)
                {
                    Flags |= CMEFlags.Disabled;
                }
            }

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                {
                    return;
                }

                Mobile from = Owner.From;

                if (from.CheckAlive())
                {
                    m_Bag.OnDoubleClick(from);
                }
            }
        }

        private class SendTarget : Target
        {
            private readonly BagOfSending m_Bag;

            public SendTarget(BagOfSending bag)
                : base(-1, false, TargetFlags.None)
            {
                m_Bag = bag;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Bag.Deleted)
                    return;

                if (from.Region.IsPartOf<Regions.Jail>())
                {
                    from.SendLocalizedMessage(1078497); // You cannot use that right now
                }
                else if (!m_Bag.IsChildOf(from.Backpack))
                {
                    from.SendLocalizedMessage(1054107); // This item must be in your backpack.
                }
                else if (m_Bag.Charges == 0)
                {
                    from.SendLocalizedMessage(1042544); // This item is out of charges.
                }
                else if (targeted is Item item)
                {
                    int reqCharges = 1; 

                    if (!item.IsChildOf(from.Backpack))
                    {
                        from.SendLocalizedMessage(1054152); // You may only send items from your backpack to your bank box.
                    }
                    else if (item is BagOfSending || item is Container)
                    {
                        from.SendLocalizedMessage(1079428); // You cannot send a container through the bag of sending
                    }
                    else if (!item.VerifyMove(from) || item.LootType == LootType.Cursed || item is Engines.Quests.QuestItem || item.QuestItem || item is ArcaneFocus)
                    {
                        from.SendLocalizedMessage(1054109); // The bag of sending rejects that item.
                    }
                    else if (Spells.SpellHelper.IsDoomGauntlet(from.Map, from.Location))
                    {
                        from.SendLocalizedMessage(1062089); // You cannot use that here.
                    }
                    else if (!from.BankBox.TryDropItem(from, item, false))
                    {
                        from.SendLocalizedMessage(1054110); // Your bank box is full.
                    }
                    else if (reqCharges > m_Bag.Charges)
                    {
                        from.SendLocalizedMessage(1042544); // This item is out of charges.
                    }
                    else
                    {
                        m_Bag.Charges -= reqCharges;
                        from.SendLocalizedMessage(1054150); // The item was placed in your bank box.
                    }
                }
            }
        }
    }
}
