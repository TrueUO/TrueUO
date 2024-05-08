using Server.Engines.PartySystem;

using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.Fourth
{
    public class ArchProtectionSpell : MagerySpell
    {
        private static readonly SpellInfo _Info = new SpellInfo(
            "Arch Protection", "Vas Uus Sanct",
            239,
            9011,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);

        public ArchProtectionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, _Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fourth;

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

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 2);

                    foreach (Mobile m in eable)
                    {
                        if (Caster.CanBeBeneficial(m, false))
                        {
                            targets.Add(m);
                        }
                    }

                    eable.Free();
                }

                Party party = Party.Get(Caster);

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    if (m == Caster || (party != null && party.Contains(m)))
                    {
                        Caster.DoBeneficial(m);
                        Second.ProtectionSpell.Toggle(Caster, m, true);
                    }
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private readonly ArchProtectionSpell _Owner;

            public InternalTarget(ArchProtectionSpell owner)
                : base(10, true, TargetFlags.None)
            {
                _Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D p)
                {
                    _Owner.Target(p);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                _Owner.FinishSequence();
            }
        }
    }
}
