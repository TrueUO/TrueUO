using Server.Targeting;
using System;

namespace Server.Items
{
    public class Bait : Item
    {
        private Type m_BaitType;
        private int m_UsesRemaining;
        private int m_Index;
        private bool m_Enhanced;

        [CommandProperty(AccessLevel.GameMaster)]
        public Type BaitType => m_BaitType;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining { get => m_UsesRemaining; set { m_UsesRemaining = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Index
        {
            get => m_Index;
            set
            {
                m_Index = value;

                if (value < 0)
                    m_Index = 0;
                if (value >= FishInfo.FishInfos.Count)
                    m_Index = FishInfo.FishInfos.Count - 1;

                m_BaitType = FishInfo.GetTypeFromIndex(m_Index);
                Hue = FishInfo.GetFishHue(m_Index);
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Enhanced { get => m_Enhanced; set { m_Enhanced = value; InvalidateProperties(); } }

        [Constructable]
        public Bait()
            : this(Utility.RandomMinMax(0, 34), 1)
        {
        }

        [Constructable]
        public Bait(int index, int remaining)
            : base(0x996)
        {
            Index = index;
            m_UsesRemaining = remaining;
        }

        public Bait(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.Target = new InternalTarget(this);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            object label = FishInfo.GetFishLabel(m_Index);

            if (m_Enhanced)
            {
                //~1_token~ ~2_token~ bait
                if (label is int i)
                    list.Add(1116464, "#{0}\t#{1}", 1116470, i);
                else if (label is string s)
                    list.Add(1116464, "#{0}\t{1}", 1116470, s);
            }
            else if (label is int i)
                list.Add(1116465, $"#{i}"); // ~1_token~ bait
            else if (label is string s)
                list.Add(1116465, s);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1116466, m_UsesRemaining.ToString()); // amount: ~1_val~
        }

        private class InternalTarget : Target
        {
            private readonly Bait m_Bait;

            public InternalTarget(Bait bait)
                : base(0, false, TargetFlags.None)
            {
                m_Bait = bait;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted == m_Bait || m_Bait.UsesRemaining <= 0)
                    return;

                if (targeted is FishingPole pole)
                {
                    if (pole.BaitType != null)
                        return;

                    pole.BaitType = m_Bait.BaitType;

                    if (m_Bait.Enhanced)
                        pole.EnhancedBait = true;

                    from.SendLocalizedMessage(1149759); // You bait the hook.

                    m_Bait.UsesRemaining--;
                }
                else if (targeted is LobsterTrap trap)
                {
                    if (trap.BaitType != null)
                        return;                   

                    if (trap.Amount > m_Bait.UsesRemaining)
                    {
                        from.SendLocalizedMessage(1149758); // There is not enough to bait the whole stack.
                        return;
                    }

                    trap.BaitType = m_Bait.BaitType;
                    trap.EnhancedBait = m_Bait.Enhanced;

                    m_Bait.UsesRemaining -= trap.Amount;

                    from.SendLocalizedMessage(1149760); // You bait the trap.
                }
                else if (targeted is Bait bait)
                {
                    if (bait.IsChildOf(from.Backpack))
                    {
                        if (bait.BaitType == m_Bait.BaitType && bait.Enhanced == m_Bait.Enhanced)
                        {
                            m_Bait.UsesRemaining += bait.UsesRemaining;
                            bait.Delete();
                            from.SendLocalizedMessage(1116469); // You combine these baits into one cup and destroy the other cup.
                        }
                    }
                    else
                    {
                        from.SendLocalizedMessage(1154125); // You must be carrying that in order to combine them.
                    }

                    return;
                }

                if (m_Bait.UsesRemaining <= 0)
                {
                    m_Bait.Delete();
                    from.SendLocalizedMessage(1116467); // Your bait is used up so you destroy the container.
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_UsesRemaining);
            writer.Write(m_Index);
            writer.Write(m_Enhanced);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_UsesRemaining = reader.ReadInt();
            m_Index = reader.ReadInt();
            m_Enhanced = reader.ReadBool();

            if (m_Index < 0)
                m_Index = 0;
            if (m_Index >= FishInfo.FishInfos.Count)
                m_Index = FishInfo.FishInfos.Count - 1;

            m_BaitType = FishInfo.FishInfos[m_Index].Type;
        }
    }
}
