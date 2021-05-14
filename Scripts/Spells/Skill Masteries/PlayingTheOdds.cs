using Server.Items;
using System;

namespace Server.Spells.SkillMasteries
{
    public class PlayingTheOddsSpell : SkillMasterySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
                "Playing the Odds", "",
                -1,
                9002
            );

        public override double RequiredSkill => 90;
        public override double UpKeep => 0;  // get
        public override int RequiredMana => 25;
        public override bool PartyEffects => true;

        public override SkillName CastSkill => SkillName.Archery;
        public override SkillName DamageSkill => SkillName.Tactics;

        private int _HCIBonus;
        private int _SSIBonus;

        public PlayingTheOddsSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void SendCastEffect()
        {
            base.SendCastEffect();

            Effects.SendTargetParticles(Caster, 0x3709, 10, 30, 2728, 0, 9907, EffectLayer.LeftFoot, 0);
        }

        public override bool CheckCast()
        {
            if (IsInCooldown(Caster, GetType()))
                return false;

            if (!CheckWeapon())
            {
                Caster.SendLocalizedMessage(1156000); // You must have an Archery weapon to use this ability!
                return false;
            }

            if (UnderPartyEffects(Caster, typeof(PlayingTheOddsSpell)))
            {
                Caster.SendLocalizedMessage(1062945); // That ability is already in effect.
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            BaseWeapon wep = GetWeapon();

            if (wep != null && CheckSequence())
            {
                wep.PlaySwingAnimation(Caster);

                double skill = (Caster.Skills[CastSkill].Value + Caster.Skills[DamageSkill].Value) / 2;
                TimeSpan duration = TimeSpan.FromMinutes(1);

                _HCIBonus = (int)Math.Max(45, skill / 2.667);
                _SSIBonus = (int)Math.Max(30, skill / 4);

                string args = $"{Caster.Name}\t{_HCIBonus.ToString()}\t{_SSIBonus.ToString()}";
                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.PlayingTheOddsDebuff, 1155913, 1156091, duration, Caster));
                //Your bow range has been reduced as you play the odds.

                UpdateParty(true);

                Caster.SendLocalizedMessage(1156091); // Your bow range has been reduced as you play the odds.

                Expires = DateTime.UtcNow + duration;
                BeginTimer();

                AddToCooldown(TimeSpan.FromSeconds(90));

                foreach (IDamageable target in AcquireIndirectTargets(Caster.Location, 5))
                {
                    if (target is Mobile mob && HitLower.ApplyDefense(mob))
                    {
                        if (wep is BaseRanged ranged && !(ranged is BaseThrown))
                        {
                            Caster.MovingEffect(mob, ranged.EffectID, 18, 1, false, false);
                        }

                        mob.PlaySound(0x28E);
                        Effects.SendTargetEffect(mob, 0x37BE, 1, 4, 0x23, 3);

                        Caster.DoHarmful(mob);
                    }
                }

                wep.InvalidateProperties();
            }

            FinishSequence();
        }

        public override void AddPartyEffects(Mobile m)
        {
            m.PlaySound(0x101);
            m.FixedEffect(0x13B2, 10, 20, 2728, 5);
            m.FixedEffect(0x37C4, 10, 20, 2728, 5);

            if (m != Caster)
            {
                string args = $"{Caster.Name}\t{_HCIBonus.ToString()}\t{_SSIBonus.ToString()}";
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.PlayingTheOdds, 1155913, 1155998, Expires - DateTime.UtcNow, m, args));
                //~1_NAME~ grants you the following:<br>+~2_VAl~% Hit Chance Increase.<br>+~3_VAL~% Swing Speed Increase.
            }
        }

        public override void EndEffects()
        {
            if (PartyList != null)
            {
                for (var index = 0; index < PartyList.Count; index++)
                {
                    Mobile m = PartyList[index];

                    RemovePartyEffects(m);
                }
            }

            BaseWeapon wep = GetWeapon();

            wep?.InvalidateProperties();

            RemovePartyEffects(Caster);
            Caster.SendLocalizedMessage(1156092); // Your bow range has returned to normal.
        }

        public override void RemovePartyEffects(Mobile m)
        {
            BuffInfo.RemoveBuff(m, BuffIcon.PlayingTheOdds);
        }

        public static int HitChanceBonus(Mobile m)
        {
            if (GetSpellForParty(m, typeof(PlayingTheOddsSpell)) is PlayingTheOddsSpell spell)
            {
                return spell._HCIBonus;
            }

            return 0;
        }

        public static int SwingSpeedBonus(Mobile m)
        {
            if (GetSpellForParty(m, typeof(PlayingTheOddsSpell)) is PlayingTheOddsSpell spell)
            {
                return spell._SSIBonus;
            }

            return 0;
        }

        public static int RangeModifier(BaseWeapon weapon)
        {
            if (weapon is BaseRanged && !(weapon is BaseThrown) && weapon.RootParent is Mobile m && GetSpell(m, typeof(PlayingTheOddsSpell)) is PlayingTheOddsSpell)
            {
                return weapon.DefMaxRange / 2;
            }

            return weapon.DefMaxRange;
        }
    }
}
