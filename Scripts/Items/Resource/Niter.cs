using Server.Engines.Harvest;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class NiterDeposit : Item
    {
        public enum NiterSize
        {
            Gigantic = 1,
            Massive,
            Huge,
            Large,
            Small
        }

        private NiterSize m_Size;
        private int m_Hits;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Hits
        {
            get => m_Hits;
            set
            {
                m_Hits = value;
                InvalidateSize();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public NiterSize Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                InvalidateID();
            }
        }

        public override bool Decays => true;

        [Constructable]
        public NiterDeposit() : this(Utility.RandomMinMax(1, 5))
        {
        }

        [Constructable]
        public NiterDeposit(int size)
        {
            if (size < 1) size = 1;
            if (size > 5) size = 5;

            m_Hits = 40 * size;
            InvalidateSize();
            Movable = false;
        }

        public void OnMine(Mobile from, Item tool)
        {
            if (tool is IUsesRemaining remaining && remaining.UsesRemaining < 1)
                return;

            from.Direction = from.GetDirectionTo(Location);
            from.Animate(11, 5, 1, true, false, 0);

            Timer.DelayCall(TimeSpan.FromSeconds(1), DoMine, new object[] { from, tool });
        }

        public void DoMine(object obj)
        {
            object[] os = (object[])obj;
            Mobile from = (Mobile)os[0];
            Item tool = (Item)os[1];

            if (from != null && from.CheckSkill(SkillName.Mining, 60.0, 100.0))
            {
                Container pack = from.Backpack;
                int count = 1;

                if (from.Skills[SkillName.Mining].Value > 100 && Utility.RandomBool())
                    count++;

                from.SendLocalizedMessage(1149924, count.ToString()); //You extract ~1_COUNT~ saltpeter from the niter deposit.
                Saltpeter sp = new Saltpeter(count);

                if (pack == null || !pack.TryDropItem(from, sp, false))
                    sp.MoveToWorld(from.Location, from.Map);

                Hits--;

                from.PlaySound(Utility.RandomMinMax(0x125, 0x126));
                CheckTool(tool);

                if (m_Hits <= 0)
                {
                    from.SendMessage("You have mined the last of the niter deposit.");
                    Delete();
                }
            }
            else if (from != null)
            {
                from.SendLocalizedMessage(1149923); //You mine the niter deposit but fail to produce any usable saltpeter.
            }
        }

        public void CheckTool(Item tool)
        {
            if (tool != null && tool is IUsesRemaining toolWithUses)
            {
                toolWithUses.ShowUsesRemaining = true;

                if (toolWithUses.UsesRemaining > 0)
                    --toolWithUses.UsesRemaining;

                if (toolWithUses.UsesRemaining < 1)
                    tool.Delete();
            }
        }

        public void InvalidateSize()
        {
            if (m_Hits > 160)
                Size = NiterSize.Gigantic;
            else if (m_Hits > 120)
                Size = NiterSize.Massive;
            else if (m_Hits > 80)
                Size = NiterSize.Huge;
            else if (m_Hits > 40)
                Size = NiterSize.Large;
            else
                Size = NiterSize.Small;
        }

        public void InvalidateID()
        {
            switch (m_Size)
            {
                default:
                case NiterSize.Gigantic: ItemID = 4962; break;
                case NiterSize.Massive: ItemID = 4961; break;
                case NiterSize.Huge: ItemID = 4967; break;
                case NiterSize.Large: ItemID = 4964; break;
                case NiterSize.Small: ItemID = 4965; break;
            }
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1149912, m_Size.ToString());
        }

        public static bool HasBeenChecked(HarvestBank bank)
        {
            if (_BankTable.TryGetValue(bank, out DateTime value) && value < DateTime.UtcNow)
            {
                _BankTable.Remove(bank);
            }

            return _BankTable.ContainsKey(bank);
        }

        private static readonly Dictionary<HarvestBank, DateTime> _BankTable = new Dictionary<HarvestBank, DateTime>();

        public static void AddBank(HarvestBank bank)
        {
            if (bank == null)
            {
                return;
            }

            _BankTable[bank] = DateTime.UtcNow + TimeSpan.FromMinutes(5);
        }

        private static void DefragBanks()
        {
            List<HarvestBank> toRemove = new List<HarvestBank>();

            foreach (KeyValuePair<HarvestBank, DateTime> kvp in _BankTable)
            {
                if (kvp.Value < DateTime.UtcNow)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (HarvestBank bank in toRemove)
            {
                _BankTable.Remove(bank);
            }
        }

        public NiterDeposit(Serial serial) :
            base(serial)
        {
        }

        public static void Initialize()
        {
            EventSink.AfterWorldSave += DoDefrag;
        }

        private static void DoDefrag(AfterWorldSaveEventArgs e)
        {
            Timer.DelayCall(TimeSpan.FromSeconds(30), DefragBanks);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_Hits);
            writer.Write((int)m_Size);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Hits = reader.ReadInt();
            m_Size = (NiterSize)reader.ReadInt();
        }
    }
}
