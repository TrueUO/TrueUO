namespace Server.Spells.Necromancy
{
    public abstract class TransformationSpell : NecromancerSpell, ITransformationSpell
    {
        public TransformationSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public abstract int Body { get; }
        public virtual int Hue => 0;
        public virtual int PhysResistOffset => 0;
        public virtual int FireResistOffset => 0;
        public virtual int ColdResistOffset => 0;
        public virtual int PoisResistOffset => 0;
        public virtual int NrgyResistOffset => 0;
        public override bool BlockedByHorrificBeast => false;
        public virtual int TickRate => 1;

        public override bool CheckCast()
        {
            return TransformationSpellHelper.CheckCast(Caster, this) && base.CheckCast();
        }

        public override void OnCast()
        {
            TransformationSpellHelper.OnCast(Caster, this);

            FinishSequence();
        }

        public virtual void OnTick(Mobile m)
        {
        }

        public virtual void DoEffect(Mobile m)
        {
        }

        public virtual void RemoveEffect(Mobile m)
        {
        }
    }
}
