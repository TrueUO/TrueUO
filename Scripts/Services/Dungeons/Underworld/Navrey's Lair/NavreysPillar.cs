using System;

namespace Server.Items
{
    public enum NavreysPillarState
    {
        Off,
        On,
        Hot
    }

    public enum PillarType
    {
        Three = 1,
        Six,
        Nine
    }

    public class NavreysPillar : Item
    {
        private NavreysPillarState m_State;
        private InternalTimer m_Timer;
        private NavreysController m_Controller;
        public PillarType m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public PillarType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public NavreysPillarState State
        {
            get => m_State;
            set
            {
                m_State = value;

                if (m_Timer != null)
                {
                    m_Timer.Stop();
                    m_Timer = null;
                }

                switch (m_State)
                {
                    case NavreysPillarState.Off:
                    {
                        Hue = 0x456;
                        break;
                    }
                    case NavreysPillarState.On:
                    {
                        Hue = 0;
                        break;
                    }
                    case NavreysPillarState.Hot:
                    {
                        m_Timer = new InternalTimer(this);
                        m_Timer.Start();
                        break;
                    }
                }
            }
        }

        public NavreysPillar(NavreysController controller, PillarType type)
            : base(0x3BF)
        {
            m_Controller = controller;
            m_Type = type;
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(this, 3) && m_State == NavreysPillarState.On)
            {
                // IMPORTANT: repair/shuffle BEFORE starting the timer so the duration matches the corrected Type.
                m_Controller?.EnsureMechanismReady();

                State = NavreysPillarState.Hot;
                m_Controller?.CheckPillars();
            }
        }

        public NavreysPillar(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write((int)m_State);
            writer.Write(m_Controller);
            writer.Write((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            // IMPORTANT: read Type before applying State (State may start a timer)
            NavreysPillarState state = (NavreysPillarState)reader.ReadInt();
            m_Controller = (NavreysController)reader.ReadItem();
            m_Type = (PillarType)reader.ReadInt();

            State = state;
        }

        private static int GetTicksForType(PillarType type)
        {
            // timer ticks every 0.5 seconds:
            // 3s => 6 ticks, 6s => 12 ticks, 9s => 18 ticks
            switch (type)
            {
                case PillarType.Three:
                {
                    return 6;
                }
                case PillarType.Six:
                {
                    return 12;
                }
                case PillarType.Nine:
                {
                    return 18;
                }
                default:
                {
                    return 12; // safe fallback
                }
            }
        }

        private class InternalTimer : Timer
        {
            private readonly NavreysPillar m_Pillar;
            private int m_Ticks;

            public InternalTimer(NavreysPillar pillar)
                // start after 0.5 seconds (avoids shaving 0.5s off due to immediate tick at TimeSpan.Zero)
                : base(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5))
            {
                m_Pillar = pillar;
                m_Ticks = GetTicksForType(pillar.Type);
            }

            protected override void OnTick()
            {
                m_Ticks--;

                m_Pillar.Hue = 0x461 + (m_Ticks & 1);

                if (m_Ticks <= 0)
                {
                    Stop();
                    m_Pillar.State = NavreysPillarState.On;
                }
            }
        }
    }
}
