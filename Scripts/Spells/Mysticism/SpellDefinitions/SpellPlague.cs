using Server.Mobiles;

using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Mysticism
{
    public class SpellPlagueSpell : MysticSpell
    {
        private static readonly SpellInfo _Info = new SpellInfo(
                "Spell Plague", "Vas Rel Jux Ort",
                230,
                9022,
                Reagent.DaemonBone,
                Reagent.DragonBlood,
                Reagent.Nightshade,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle => SpellCircle.Seventh;

        public SpellPlagueSpell(Mobile caster, Item scroll)
            : base(caster, scroll, _Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool OnInstantCast(IEntity target)
        {
            Target t = new InternalTarget(this);
            if (Caster.InRange(target, t.Range) && Caster.InLOS(target))
            {
                t.Invoke(Caster, target);
                return true;
            }
            else
                return false;
        }

        public void OnTarget(object o)
        {
            Mobile m = o as Mobile;

            if (m == null)
            {
                return;
            }

            if (!(m is PlayerMobile || m is BaseCreature))
            {
                Caster.SendLocalizedMessage(1080194); // Your target cannot be affected by spell plague.
            }
            else if (CheckResisted(m))
            {
                m.SendLocalizedMessage(1080199); //You resist spell plague.
                Caster.SendLocalizedMessage(1080200); //Your target resists spell plague.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.CheckReflect(this, Caster, ref m);

                SpellHelper.Turn(Caster, m);

                Caster.PlaySound(0x658);

                m.FixedParticles(0x375A, 1, 17, 9919, 1161, 7, EffectLayer.Waist);
                m.FixedParticles(0x3728, 1, 13, 9502, 1161, 7, (EffectLayer)255);

                if (!_Table.TryGetValue(m, out List<SpellPlagueTimer> value) || value == null)
                {
                    value = new List<SpellPlagueTimer>();

                    _Table.Add(m, value);
                }

                value.Add(new SpellPlagueTimer(Caster, m, TimeSpan.FromSeconds(8)));

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.SpellPlague, 1031690, 1080167, TimeSpan.FromSeconds(8), m));

                DoExplosion(m, Caster, true, 1);
            }

            FinishSequence();
        }

        private static readonly Dictionary<Mobile, List<SpellPlagueTimer>> _Table = new Dictionary<Mobile, List<SpellPlagueTimer>>();

        public static bool HasSpellPlague(Mobile from)
        {
            foreach (KeyValuePair<Mobile, List<SpellPlagueTimer>> kvp in _Table)
            {
                if (kvp.Value != null)
                {
                    for (int index = 0; index < kvp.Value.Count; index++)
                    {
                        SpellPlagueTimer timer = kvp.Value[index];

                        if (timer.Caster == from)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void OnMobileDamaged(Mobile from)
        {
            if (_Table.TryGetValue(from, out List<SpellPlagueTimer> value) && value.Count > 0 && value[0].NextUse < DateTime.UtcNow)
            {
                int amount = value[0].Amount;
                bool doExplosion = false;
                double mod = from.Skills[SkillName.MagicResist].Value >= 70.0 ? (from.Skills[SkillName.MagicResist].Value / 1000 * 3) : 0.0;

                if (mod < 0)
                {
                    mod = 0.01;
                }

                if (amount == 0 && 0.90 - mod > Utility.RandomDouble())
                {
                    doExplosion = true;
                }
                else if (amount == 1 && 0.60 - mod > Utility.RandomDouble())
                {
                    doExplosion = true;
                }
                else if (amount == 2 && 0.30 - mod > Utility.RandomDouble())
                {
                    doExplosion = true;
                }

                if (doExplosion)
                {
                    SpellPlagueTimer timer = value[0];

                    timer.NextUse = DateTime.UtcNow + TimeSpan.FromSeconds(1.5);

                    DoExplosion(from, timer.Caster, false, amount);
                    timer.Amount++;
                }
            }
        }

        public static void DoExplosion(Mobile from, Mobile caster, bool initial, int amount)
        {
            double damage = BonusDamage(caster) + Utility.RandomMinMax(22, 24);

            if (initial)
            {
                int sdiBonus = SpellHelper.GetSpellDamageBonus(caster, from, SkillName.Mysticism, from is PlayerMobile);

                damage *= 100 + sdiBonus;
                damage /= 100;
            }
            else
            {
                switch (amount)
                {
                    default: break;
                    case 0: 
                    case 1: damage /= 2; break;
                    case 2: damage /= 1.66; break;
                    case 3: damage /= 1.33; break;
                }
            }

            from.PlaySound(0x658);

            from.FixedParticles(0x375A, 1, 17, 9919, 1161, 7, EffectLayer.Waist);
            from.FixedParticles(0x3728, 1, 13, 9502, 1161, 7, (EffectLayer)255);

            SpellHelper.Damage(null, TimeSpan.Zero, from, caster, (int)damage, 0, 0, 0, 0, 0, DFAlgorithm.Standard, 100, 0);
        }

        public static int BonusDamage(Mobile caster)
        {
            double skill = Math.Max(caster.Skills[SkillName.Focus].Value, caster.Skills[SkillName.Imbuing].Value);

            if (skill <= 20)
            {
                return 0;
            }

            if (skill <= 25)
            {
                return 2;
            }

            if (skill <= 30)
            {
                return 4;
            }

            if (skill <= 35)
            {
                return 7;
            }

            if (skill <= 40)
            {
                return 9;
            }

            if (skill <= 50)
            {
                return 13;
            }

            if (skill <= 60)
            {
                return 18;
            }

            if (skill <= 70)
            {
                return 22;
            }

            if (skill <= 80)
            {
                return 28;
            }

            if (skill <= 90)
            {
                return 32;
            }

            if (skill <= 100)
            {
                return 37;
            }

            if (skill <= 110)
            {
                return 41;
            }

            return 46;
        }

        public static void RemoveFromList(Mobile from)
        {
            if (_Table.TryGetValue(from, out List<SpellPlagueTimer> value) && value.Count > 0)
            {
                Mobile caster = value[0].Caster;

                value.Remove(value[0]);

                if (value.Count == 0)
                {
                    _Table.Remove(from);
                    BuffInfo.RemoveBuff(from, BuffIcon.SpellPlague);
                }

                foreach (KeyValuePair<Mobile, List<SpellPlagueTimer>> kvp in _Table)
                {
                    for (int index = 0; index < kvp.Value.Count; index++)
                    {
                        SpellPlagueTimer timer = kvp.Value[index];

                        if (timer.Caster == caster)
                        {
                            return;
                        }
                    }
                }

                BuffInfo.RemoveBuff(caster, BuffIcon.SpellPlague);
            }
        }

        public class InternalTarget : Target
        {
            private SpellPlagueSpell Owner { get; }

            public InternalTarget(SpellPlagueSpell owner)
                : this(owner, false)
            {
            }

            public InternalTarget(SpellPlagueSpell owner, bool allowland)
                : base(12, allowland, TargetFlags.Harmful)
            {
                Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o == null)
                {
                    return;
                }

                if (!from.CanSee(o))
                {
                    from.SendLocalizedMessage(500237); // Target can not be seen.
                }
                else
                {
                    SpellHelper.Turn(from, o);
                    Owner.OnTarget(o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                Owner.FinishSequence();
            }
        }
    }

    public class SpellPlagueTimer : Timer
    {
        private readonly Mobile _Caster;
        private readonly Mobile _Owner;
        private int _Amount;
        private DateTime _NextUse;

        public Mobile Caster => _Caster;

        public int Amount
        {
            get => _Amount;
            set
            {
                _Amount = value;

                if (_Amount >= 3)
                {
                    EndTimer();
                }
            }
        }

        public DateTime NextUse { get => _NextUse; set => _NextUse = value; }

        public SpellPlagueTimer(Mobile caster, Mobile owner, TimeSpan duration)
            : base(duration)
        {
            _Caster = caster;
            _Owner = owner;
            _Amount = 0;
            _NextUse = DateTime.UtcNow;

            Start();
        }

        protected override void OnTick()
        {
            EndTimer();
        }

        private void EndTimer()
        {
            Stop();
            SpellPlagueSpell.RemoveFromList(_Owner);
        }
    }
}
