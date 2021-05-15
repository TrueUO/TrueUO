using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Seventh
{
    public class ChainLightningSpell : MagerySpell
    {
        public override DamageType SpellDamageType => DamageType.SpellAOE;

        private static readonly SpellInfo m_Info = new SpellInfo(
            "Chain Lightning", "Vas Ort Grav",
            209,
            9022,
            false,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public ChainLightningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Seventh;
        public override bool DelayedDamage => true;
        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                if (p is Item item)
                {
                    p = item.GetWorldLocation();
                }

                List<IDamageable> targets = new List<IDamageable>();

                foreach (var target in AcquireIndirectTargets(p, 2))
                {
                    targets.Add(target);
                }

                int count = Math.Max(1, targets.Count);

                for (var index = 0; index < targets.Count; index++)
                {
                    IDamageable dam = targets[index];

                    IDamageable id = dam;
                    Mobile m = id as Mobile;

                    double damage = GetNewAosDamage(51, 1, 5, id is PlayerMobile, id);

                    if (count > 2)
                    {
                        damage = (damage * 2) / count;
                    }

                    Mobile source = Caster;
                    SpellHelper.CheckReflect(this, ref source, ref id);

                    if (m != null)
                    {
                        damage *= GetDamageScalar(m);
                    }

                    Effects.SendBoltEffect(id, true, 0, false);

                    Caster.DoHarmful(id);
                    SpellHelper.Damage(this, id, damage, 0, 0, 0, 0, 100);
                }

                ColUtility.Free(targets);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly ChainLightningSpell m_Owner;

            public InternalTarget(ChainLightningSpell owner)
                : base(10, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D p)
                {
                    m_Owner.Target(p);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}
