using Server.Commands;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Items
{
    public class NavreysController : Item
    {
        #region Generation
        public static void Initialize()
        {
            CommandSystem.Register("GenNavrey", AccessLevel.Developer, GenNavrey_Command);
            CommandSystem.Register("FixNavreyPillars", AccessLevel.GameMaster, FixNavreyPillars_Command);
            CommandSystem.Register("NavreyStatus", AccessLevel.GameMaster, NavreyStatus_Command);
        }

        [Usage("GenNavrey")]
        private static void GenNavrey_Command(CommandEventArgs e)
        {
            GenNavery(e.Mobile);
        }

        /// <summary>
        /// Repairs the 3/6/9 pillar type invariant for the existing controller (if any).
        /// Useful to fix a live, already-corrupted world state without restarting.
        /// </summary>
        [Usage("FixNavreyPillars")]
        private static void FixNavreyPillars_Command(CommandEventArgs e)
        {
            NavreysController controller = FindController();

            if (controller == null || controller.Deleted)
            {
                e.Mobile.SendMessage("Navrey controller not found.");
                return;
            }

            controller.ForceRepairTypes();
            e.Mobile.SendMessage("Navrey pillars repaired (forced 3/6/9).");
        }

        /// <summary>
        /// Prints the current pillar types and time to next daily shuffle.
        /// </summary>
        [Usage("NavreyStatus")]
        private static void NavreyStatus_Command(CommandEventArgs e)
        {
            NavreysController controller = FindController();

            if (controller == null || controller.Deleted)
            {
                e.Mobile.SendMessage("Navrey controller not found.");
                return;
            }

            controller.CheckDailyShuffleAndRepair();

            e.Mobile.SendMessage("Navrey Pillar Status:");
            e.Mobile.SendMessage($"  Next shuffle in: {controller.TypeRestart}");
            e.Mobile.SendMessage($"  Next shuffle UTC: {controller.NextShuffleUtc:yyyy-MM-dd HH:mm:ss}");

            for (int i = 0; i < controller.m_Pillars.Length; i++)
            {
                NavreysPillar p = controller.m_Pillars[i];

                if (p == null || p.Deleted)
                {
                    continue;
                }

                e.Mobile.SendMessage($"  Pillar {i}: {p.Type} at {p.Location} (Serial: {p.Serial})");
            }
        }

        public static void GenNavery(Mobile m)
        {
            if (Check())
            {
                m.SendMessage("Navrey spawner is already present.");
            }
            else
            {
                m.SendMessage("Creating Navrey Night-Eyes Lair...");

                NavreysController controller = new NavreysController();

                m.SendMessage("Generation completed!");
            }
        }

        private static bool Check()
        {
            foreach (Item item in World.Items.Values)
            {
                if (item is NavreysController && !item.Deleted)
                {
                    return true;
                }
            }

            return false;
        }

        private static NavreysController FindController()
        {
            foreach (Item item in World.Items.Values)
            {
                if (item is NavreysController c && !c.Deleted)
                {
                    return c;
                }
            }

            return null;
        }
        #endregion

        internal NavreysPillar[] m_Pillars;
        private Navrey m_Navrey;

        // Next time the daily pillar type shuffle should occur (UTC midnight schedule).
        private DateTime m_NextShuffleUtc;

        // expose schedule to staff via [props on the controller.
        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextShuffleUtc
        {
            get => m_NextShuffleUtc;
            set => m_NextShuffleUtc = value;
        }

        /// <summary>
        /// Time remaining until next daily shuffle.
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TypeRestart
        {
            get
            {
                TimeSpan ts = m_NextShuffleUtc - DateTime.UtcNow;
                return ts < TimeSpan.Zero ? TimeSpan.Zero : ts;
            }
        }

        public bool AllPillarsHot
        {
            get
            {
                for (int i = 0; i < m_Pillars.Length; i++)
                {
                    NavreysPillar pillar = m_Pillars[i];

                    if (pillar == null || pillar.Deleted || pillar.State != NavreysPillarState.Hot)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public NavreysController()
            : base(0x1F13)
        {
            Name = "Navrey Spawner - Do not remove !!";
            Visible = false;
            Movable = false;

            MoveToWorld(new Point3D(1054, 861, -31), Map.TerMur);

            m_Pillars = new NavreysPillar[3];

            m_Pillars[0] = new NavreysPillar(this, PillarType.Three);
            m_Pillars[0].MoveToWorld(new Point3D(1071, 847, 0), Map.TerMur);

            m_Pillars[1] = new NavreysPillar(this, PillarType.Six);
            m_Pillars[1].MoveToWorld(new Point3D(1039, 879, 0), Map.TerMur);

            m_Pillars[2] = new NavreysPillar(this, PillarType.Nine);
            m_Pillars[2].MoveToWorld(new Point3D(1039, 850, 0), Map.TerMur);

            // Daily random rotation schedule: next UTC midnight.
            m_NextShuffleUtc = DateTime.UtcNow.Date.AddDays(1);

            // Enforce invariant immediately (fixes any accidental edits)
            CheckDailyShuffleAndRepair();

            Respawn();
        }

        public void OnNavreyKilled()
        {
            SetAllPillars(NavreysPillarState.Off);
            Timer.DelayCall(TimeSpan.FromMinutes(10.0), Respawn);
        }

        public void Respawn()
        {
            CheckDailyShuffleAndRepair();

            Navrey navrey = new Navrey(this)
            {
                RangeHome = 20
            };

            navrey.MoveToWorld(Map.GetSpawnPosition(Location, 10), Map);

            SetAllPillars(NavreysPillarState.On);

            m_Navrey = navrey;
        }

        public void ResetPillars()
        {
            if (m_Navrey != null && !m_Navrey.Deleted && m_Navrey.Alive)
            {
                SetAllPillars(NavreysPillarState.On);
            }
        }

        /// <summary>
        /// Called by a pillar right before it turns Hot.
        /// IMPORTANT: this ensures the type is repaired BEFORE the timer duration is computed.
        /// </summary>
        public void EnsureMechanismReady()
        {
            CheckDailyShuffleAndRepair();
        }

        public override void Delete()
        {
            base.Delete();

            if (m_Pillars != null)
            {
                for (int i = 0; i < m_Pillars.Length; i++)
                {
                    NavreysPillar pillar = m_Pillars[i];

                    if (pillar != null && !pillar.Deleted)
                    {
                        pillar.Delete();
                    }
                }
            }

            if (m_Navrey != null && !m_Navrey.Deleted)
            {
                m_Navrey.Delete();
            }
        }

        public void CheckPillars()
        {
            // sanity
            CheckDailyShuffleAndRepair();

            if (AllPillarsHot)
            {
                m_Navrey.UsedPillars = true;

                Timer t = new RockRainTimer(m_Navrey);
                t.Start();

                SetAllPillars(NavreysPillarState.Off);
                Timer.DelayCall(TimeSpan.FromMinutes(5.0), ResetPillars);
            }
        }

        private void SetAllPillars(NavreysPillarState state)
        {
            for (int i = 0; i < m_Pillars.Length; i++)
            {
                NavreysPillar pillar = m_Pillars[i];

                if (pillar != null && !pillar.Deleted)
                {
                    pillar.State = state;
                }
            }
        }

        /// <summary>
        /// Public entry point for GM command to repair a live corrupted state.
        /// </summary>
        public void ForceRepairTypes()
        {
            EnsurePillarTypesAreValid();
        }

        private static readonly PillarType[] _RequiredTypes =
        [
            PillarType.Three,
            PillarType.Six,
            PillarType.Nine
        ];

        /// <summary>
        /// Ensures the three pillars always contain exactly one 3, one 6, and one 9.
        /// If it detects duplicates or invalid values, it repairs them deterministically.
        /// </summary>
        private void EnsurePillarTypesAreValid()
        {
            if (m_Pillars == null || m_Pillars.Length != 3)
            {
                return;
            }

            bool[] present = new bool[3];
            int[] bad = new int[3];
            int badCount = 0;

            for (int i = 0; i < 3; i++)
            {
                NavreysPillar p = m_Pillars[i];

                if (p == null || p.Deleted)
                {
                    bad[badCount++] = i;
                    continue;
                }

                int idx = GetRequiredIndex(p.Type);

                // invalid type or already-present => mark as bad
                if (idx < 0 || present[idx])
                {
                    bad[badCount++] = i;
                    continue;
                }

                present[idx] = true;
            }

            // fill any missing required types into bad pillars
            int fill = 0;

            for (int t = 0; t < 3 && fill < badCount; t++)
            {
                if (!present[t])
                {
                    int i = bad[fill++];
                    NavreysPillar p = m_Pillars[i];

                    if (p != null && !p.Deleted)
                    {
                        p.Type = _RequiredTypes[t];
                    }
                }
            }
        }

        private static int GetRequiredIndex(PillarType t)
        {
            switch (t)
            {
                case PillarType.Three: return 0;
                case PillarType.Six: return 1;
                case PillarType.Nine: return 2;
                default: return -1;
            }
        }

        /// <summary>
        /// randomly assigns the 3/6/9 types to the three pillars (always one of each).
        /// </summary>
        private void ShuffleTypes()
        {
            if (m_Pillars == null || m_Pillars.Length != 3)
            {
                return;
            }

            PillarType[] types = [PillarType.Three, PillarType.Six, PillarType.Nine];

            // shuffle using Utility.Random
            for (int i = types.Length - 1; i > 0; i--)
            {
                int j = Utility.Random(i + 1);
                PillarType tmp = types[i];
                types[i] = types[j];
                types[j] = tmp;
            }

            for (int i = 0; i < 3; i++)
                m_Pillars[i].Type = types[i];
        }

        /// <summary>
        /// Enforces 3/6/9 uniqueness always and performs the once-per-day random shuffle.
        /// </summary>
        private void CheckDailyShuffleAndRepair()
        {
            // Repair immediately if corrupted.
            EnsurePillarTypesAreValid();

            // Apply daily shuffle if it's time (UTC midnight schedule).
            if (DateTime.UtcNow >= m_NextShuffleUtc)
            {
                ShuffleTypes();
                EnsurePillarTypesAreValid();

                // Next shuffle at next UTC midnight
                m_NextShuffleUtc = DateTime.UtcNow.Date.AddDays(1);
            }
        }

        public NavreysController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(m_Navrey);

            writer.Write(m_Pillars[0]);
            writer.Write(m_Pillars[1]);
            writer.Write(m_Pillars[2]);

            writer.Write(m_NextShuffleUtc);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version

            m_Navrey = (Navrey)reader.ReadMobile();

            m_Pillars = new NavreysPillar[3];

            m_Pillars[0] = (NavreysPillar)reader.ReadItem();
            m_Pillars[1] = (NavreysPillar)reader.ReadItem();
            m_Pillars[2] = (NavreysPillar)reader.ReadItem();

            m_NextShuffleUtc = reader.ReadDateTime();

            CheckDailyShuffleAndRepair();

            if (m_Navrey == null)
            {
                Timer.DelayCall(TimeSpan.Zero, Respawn);
            }
            else
            {
                SetAllPillars(NavreysPillarState.On);
            }
        }

        private class RockRainTimer : Timer
        {
            private readonly Navrey m_Navrey;
            private int m_Ticks;

            public RockRainTimer(Navrey navrey)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(0.25))
            {
                m_Navrey = navrey;
                m_Ticks = 120;

                m_Navrey.CantWalk = true;
            }

            protected override void OnTick()
            {
                m_Ticks--;

                Point3D dest = m_Navrey.Location;
                Point3D orig = new Point3D(dest.X - Utility.RandomMinMax(3, 4), dest.Y - Utility.RandomMinMax(8, 9), dest.Z + Utility.RandomMinMax(41, 43));
                int itemId = Utility.RandomMinMax(0x1362, 0x136D);
                int speed = Utility.RandomMinMax(5, 10);
                int hue = Utility.RandomBool() ? 0 : Utility.RandomMinMax(0x456, 0x45F);

                Effects.SendPacket(orig, m_Navrey.Map, new HuedEffect(EffectType.Moving, Serial.Zero, Serial.Zero, itemId, orig, dest, speed, 0, false, false, hue, 4));
                Effects.SendPacket(orig, m_Navrey.Map, new HuedEffect(EffectType.Moving, Serial.Zero, Serial.Zero, itemId, orig, dest, speed, 0, false, false, hue, 4));

                Effects.PlaySound(m_Navrey.Location, m_Navrey.Map, 0x15E + Utility.Random(3));
                Effects.PlaySound(m_Navrey.Location, m_Navrey.Map, Utility.RandomList(0x305, 0x306, 0x307, 0x309));

                int amount = Utility.RandomMinMax(75, 100);
                IPooledEnumerable eable = m_Navrey.GetMobilesInRange(1);

                foreach (Mobile m in eable)
                {
                    if (m == null || !m.Alive)
                    {
                        continue;
                    }

                    if (m is Navrey)
                    {
                        m.Damage(amount);
                        m.SendDamageToAll(amount);
                    }
                    else
                    {
                        m.RevealingAction();
                        AOS.Damage(m, amount, 100, 0, 0, 0, 0);
                    }
                }
                eable.Free();

                if (m_Ticks == 0 || !m_Navrey.Alive)
                {
                    m_Navrey.CantWalk = false;
                    Stop();
                }
            }
        }
    }
}
