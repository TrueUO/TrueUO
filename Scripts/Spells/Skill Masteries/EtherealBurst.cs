using System;

namespace Server.Spells.SkillMasteries
{
    public class EtherealBurstSpell : SkillMasterySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
                "Ethereal Blast", "Uus Ort Grav",
                -1,
                9002,
                Reagent.Bloodmoss,
                Reagent.Ginseng,
                Reagent.MandrakeRoot
            );

        public override double RequiredSkill => 90;
        public override double UpKeep => 0;
        public override int RequiredMana => 0;
        public override bool PartyEffects => false;

        public override SkillName CastSkill => SkillName.Magery;
        public override SkillName DamageSkill => SkillName.EvalInt;

        public EtherealBurstSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                int level = GetMasteryLevel();
                int mana = Math.Min(Caster.ManaMax, (int)((Caster.Skills[CastSkill].Value * 0.6 + Caster.Skills[DamageSkill].Value * 0.4) * (float)level / 3));

                Caster.Mana += mana;

                int duration = 360;            

                if (level == 2)
                    duration = 240;
                else if (level == 3)
                    duration = 120;

                AddToCooldown(TimeSpan.FromMinutes(duration));

                Caster.PlaySound(0x102);
                Effects.SendTargetParticles(Caster, 0x376A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
                Caster.SendLocalizedMessage(1155791); // You feel rejuvenated!
            }

            FinishSequence();
        }
    }
}
