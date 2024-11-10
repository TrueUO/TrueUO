using System;
using System.Collections.Generic;

namespace Server.Spells.Bushido
{
    public class Confidence : SamuraiSpell
    {
        private static readonly SpellInfo m_Info = new(
            "Confidence", null,
            -1,
            9002);

        private static readonly Dictionary<Mobile, Timer> _Table = new Dictionary<Mobile, Timer>();
        private static readonly Dictionary<Mobile, Timer> _RegenTable = new Dictionary<Mobile, Timer>();

        public Confidence(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.25);
        public override double RequiredSkill => 25.0;
        public override int RequiredMana => 10;

        public static bool IsConfident(Mobile m)
        {
            return _Table.ContainsKey(m);
        }

        public static void BeginConfidence(Mobile m)
        {
            if (_Table.TryGetValue(m, out Timer timer))
            {
                timer.Stop();
            }

            timer = new InternalTimer(m);

            _Table[m] = timer;

            timer.Start();

            double bushido = m.Skills[SkillName.Bushido].Value;
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Confidence, 1060596, 1153809, TimeSpan.FromSeconds(4), m, $"{(int) (bushido / 12)}\t{(int) (bushido / 5)}\t100")); // Successful parry will heal for 1-~1_HEAL~ hit points and refresh for 1-~2_STAM~ stamina points.<br>+~3_HP~ hit point regeneration (4 second duration).

            int anticipateHitBonus = SkillMasteries.MasteryInfo.AnticipateHitBonus(m);

            if (anticipateHitBonus > 0)
            {
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.AnticipateHit, 1155905, 1156057, TimeSpan.FromSeconds(4), m, $"{anticipateHitBonus}\t75")); // ~1_CHANCE~% chance to reduce Confidence heal by ~2_REDUCE~% when hit. 
            }
        }

        public static void EndConfidence(Mobile m)
        {
            if (!_Table.TryGetValue(m, out Timer timer))
            {
                return;
            }

            timer.Stop();

            _Table.Remove(m);

            OnEffectEnd(m, typeof(Confidence));

            BuffInfo.RemoveBuff(m, BuffIcon.Confidence);
            BuffInfo.RemoveBuff(m, BuffIcon.AnticipateHit);
        }

        public static bool IsRegenerating(Mobile m)
        {
            return _RegenTable.ContainsKey(m);
        }

        public static void BeginRegenerating(Mobile m)
        {
            if (_RegenTable.TryGetValue(m, out Timer timer))
            {
                timer.Stop();
            }

            timer = new RegenTimer(m);

            _RegenTable[m] = timer;

            timer.Start();
        }

        public static void StopRegenerating(Mobile m)
        {
            Timer t;
            int anticipateHitBonus = SkillMasteries.MasteryInfo.AnticipateHitBonus(m);

            if (anticipateHitBonus >= Utility.Random(100) && _RegenTable.TryGetValue(m, out t))
            {
                if (t is RegenTimer timer)
                {
                    timer.Hits /= 2;
                }

                return;
            }

            if (_RegenTable.TryGetValue(m, out t))
            {
                t.Stop();
            }

            _RegenTable.Remove(m);

            BuffInfo.RemoveBuff(m, BuffIcon.AnticipateHit);
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.FixedEffect(0x37C4, 10, 7, 4, 3);
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.SendLocalizedMessage(1063115); // You exude confidence.

                Caster.FixedParticles(0x375A, 1, 17, 0x7DA, 0x960, 0x3, EffectLayer.Waist);
                Caster.PlaySound(0x51A);

                OnCastSuccessful(Caster);

                BeginConfidence(Caster);
                BeginRegenerating(Caster);
            }

            FinishSequence();
        }

        private class InternalTimer : Timer
        {
            private readonly Mobile _Mobile;

            public InternalTimer(Mobile m)
                : base(TimeSpan.FromSeconds(15.0))
            {
                _Mobile = m;
            }

            protected override void OnTick()
            {
                EndConfidence(_Mobile);
                _Mobile.SendLocalizedMessage(1063116); // Your confidence wanes.
            }
        }

        private class RegenTimer : Timer
        {
            private readonly Mobile _Mobile;
            private int _Ticks;
            private int _Hits;

            public int Hits { get => _Hits; set => _Hits = value; }

            public RegenTimer(Mobile m)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                _Mobile = m;
                _Hits = 15 + (m.Skills.Bushido.Fixed * m.Skills.Bushido.Fixed / 57600);
            }

            protected override void OnTick()
            {
                ++_Ticks;

                if (_Ticks >= 5)
                {
                    _Mobile.Hits += (_Hits - (_Hits * 4 / 5));
                    StopRegenerating(_Mobile);
                }

                _Mobile.Hits += _Hits / 5;
            }
        }
    }
}
