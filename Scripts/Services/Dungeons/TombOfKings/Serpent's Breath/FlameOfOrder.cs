using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class FlameOfOrder : Item
    {
        public override int LabelNumber => 1112127;  // Flame of Order

        private List<EnergyBarrier> m_Barriers;
        private List<Blocker> m_Blockers;
        private List<LOSBlocker> m_LOSBlockers;
        private List<SBMessageTrigger> m_MsgTriggers;

        [Constructable]
        public FlameOfOrder(Point3D location, Map map)
            : base(0x19AB)
        {
            Movable = false;
            Light = LightType.Circle225;

            MoveToWorld(location, map);

            m_Barriers = new List<EnergyBarrier>(m_BarrierLocations.Length);
            m_Blockers = new List<Blocker>(m_BarrierLocations.Length);
            m_LOSBlockers = new List<LOSBlocker>(m_BarrierLocations.Length);
            m_MsgTriggers = new List<SBMessageTrigger>(m_MsgTriggerLocations.Length);

            for (var index = 0; index < m_BarrierLocations.Length; index++)
            {
                Point3D loc = m_BarrierLocations[index];

                m_Barriers.Add(new EnergyBarrier(loc, map));

                Blocker blocker = new Blocker();

                blocker.MoveToWorld(loc, map);
                m_Blockers.Add(blocker);

                LOSBlocker losblocker = new LOSBlocker();

                losblocker.MoveToWorld(loc, map);
                m_LOSBlockers.Add(losblocker);
            }

            for (var index = 0; index < m_MsgTriggerLocations.Length; index++)
            {
                Point3D loc = m_MsgTriggerLocations[index];

                SBMessageTrigger trigger = new SBMessageTrigger(this);

                trigger.MoveToWorld(loc, map);
                m_MsgTriggers.Add(trigger);
            }
        }

        public override bool HandlesOnSpeech => true;

        public override void OnSpeech(SpeechEventArgs e)
        {
            string mantra = e.Speech.ToLower();

            if (Visible && e.Mobile.InRange(this, 2) && mantra == "ord")
            {
                Visible = false;

                for (var index = 0; index < m_Barriers.Count; index++)
                {
                    EnergyBarrier barrier = m_Barriers[index];
                    barrier.Active = false;
                }

                for (var index = 0; index < m_Blockers.Count; index++)
                {
                    Blocker blocker = m_Blockers[index];
                    blocker.Delete();
                }

                for (var index = 0; index < m_LOSBlockers.Count; index++)
                {
                    LOSBlocker losblocker = m_LOSBlockers[index];
                    losblocker.Delete();
                }

                m_Blockers.Clear();
                m_LOSBlockers.Clear();

                Timer.DelayCall(TimeSpan.FromMinutes(2.0), RestoreBarrier);
            }
        }

        protected void RestoreBarrier()
        {
            for (var index = 0; index < m_Barriers.Count; index++)
            {
                EnergyBarrier barrier = m_Barriers[index];
                barrier.Active = true;
            }

            for (var index = 0; index < m_BarrierLocations.Length; index++)
            {
                Point3D loc = m_BarrierLocations[index];

                Blocker blocker = new Blocker();

                blocker.MoveToWorld(loc, Map);
                m_Blockers.Add(blocker);

                LOSBlocker losblocker = new LOSBlocker();

                losblocker.MoveToWorld(loc, Map);
                m_LOSBlockers.Add(losblocker);
            }

            Visible = true;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            for (var index = 0; index < m_Blockers.Count; index++)
            {
                Blocker blocker = m_Blockers[index];
                blocker.Delete();
            }

            for (var index = 0; index < m_LOSBlockers.Count; index++)
            {
                LOSBlocker losblocker = m_LOSBlockers[index];
                losblocker.Delete();
            }

            for (var index = 0; index < m_MsgTriggers.Count; index++)
            {
                SBMessageTrigger trigger = m_MsgTriggers[index];
                trigger.Delete();
            }

            for (var index = 0; index < m_Barriers.Count; index++)
            {
                EnergyBarrier barrier = m_Barriers[index];
                barrier.Delete();
            }
        }

        private static readonly Point3D[] m_BarrierLocations =
        {
            new Point3D( 33, 205, 0 ),
            new Point3D( 34, 205, 0 ),
            new Point3D( 35, 205, 0 ),
            new Point3D( 36, 205, 0 ),
            new Point3D( 37, 205, 0 )
        };

        private static readonly Point3D[] m_MsgTriggerLocations =
        {
            new Point3D( 33, 203, 0 ),
            new Point3D( 34, 203, 0 ),
            new Point3D( 35, 203, 0 ),
            new Point3D( 36, 203, 0 ),
            new Point3D( 37, 203, 0 )
        };

        public FlameOfOrder(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Barriers.Count);

            for (int i = 0; i < m_Barriers.Count; i++)
                writer.Write(m_Barriers[i]);

            writer.Write(m_Blockers.Count);

            for (int i = 0; i < m_Blockers.Count; i++)
                writer.Write(m_Blockers[i]);

            writer.Write(m_LOSBlockers.Count);

            for (int i = 0; i < m_LOSBlockers.Count; i++)
                writer.Write(m_LOSBlockers[i]);

            writer.Write(m_MsgTriggers.Count);

            for (int i = 0; i < m_MsgTriggers.Count; i++)
                writer.Write(m_MsgTriggers[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            int amount = reader.ReadInt(); // barrier

            m_Barriers = new List<EnergyBarrier>(amount);

            for (int i = 0; i < amount; i++)
                m_Barriers.Add(reader.ReadItem() as EnergyBarrier);

            amount = reader.ReadInt(); // blockers

            m_Blockers = new List<Blocker>(amount);

            for (int i = 0; i < amount; i++)
                m_Blockers.Add(reader.ReadItem() as Blocker);

            amount = reader.ReadInt();

            m_LOSBlockers = new List<LOSBlocker>(amount);

            for (int i = 0; i < amount; i++)
                m_LOSBlockers.Add(reader.ReadItem() as LOSBlocker);

            amount = reader.ReadInt(); // msg triggers

            m_MsgTriggers = new List<SBMessageTrigger>(amount);

            for (int i = 0; i < amount; i++)
                m_MsgTriggers.Add(reader.ReadItem() as SBMessageTrigger);

            if (!Visible)
                Timer.DelayCall(TimeSpan.Zero, RestoreBarrier);
        }
    }
}
