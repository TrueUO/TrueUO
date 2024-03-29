using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Spells.SkillMasteries
{
    public class ElementalFurySpell : SkillMasterySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
                "Anticipate Hit", "",
                -1,
                9002
            );

        public override double RequiredSkill => 90;
        public override int RequiredMana => 20;

        public override SkillName CastSkill => SkillName.Throwing;
        public override SkillName DamageSkill => SkillName.Tactics;
        public override bool CheckManaBeforeCast => !HasSpell(Caster, GetType());

        private int _MaxAdd;
        private ResistanceType _Type;

        private Dictionary<Mobile, int> _Table;

        public ElementalFurySpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (IsInCooldown(Caster, GetType()))
                return false;

            if (!CheckWeapon())
            {
                Caster.SendLocalizedMessage(1156016); // You must have a throwing weapon equipped to use this ability.
                return false;
            }

            if (GetSpell(Caster, GetType()) is ElementalFurySpell spell)
            {
                spell.Expire();
                return false;
            }

            return base.CheckCast();
        }

        public override void SendCastEffect()
        {
            Caster.PlaySound(0x457);
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                Caster.PlaySound(0x101);

                Caster.PrivateOverheadMessage(MessageType.Regular, 1150, 1156017, Caster.NetState);  // *Your throw is enhanced by the Elemental Fury!*

                double skill = BaseSkillBonus;

                TimeSpan duration = TimeSpan.FromSeconds(skill);
                _MaxAdd = (int)(skill / 10) + Utility.RandomMinMax(-1, 0);

                Expires = DateTime.UtcNow + duration;
                BeginTimer();

                _Type = GetResistanceType(GetWeapon());

                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.ElementalFury, 1156018, 1156019, duration, Caster, $"{_Type.ToString()}\t69\t{_MaxAdd.ToString()}"));
                //Each attack the caster deals with ~1_TYPE~ damage will add up to ~3_VAL~ damage to the Fury Pool. Once the Fury Pool 
                //reaches ~2_VAL~ the throwing weapon will unleash the Elemental Fury.
            }

            FinishSequence();
        }

        public override void OnHit(Mobile defender, ref int damage)
        {
            if (_Table == null)
                _Table = new Dictionary<Mobile, int>();

            _Table.TryAdd(defender, 0);

            _Table[defender] += Math.Min(_MaxAdd, damage);

            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ElementalFuryDebuff, 1155920, BuffInfo.Blank, ""));

            if (_Table[defender] >= 69)
            {
                defender.FixedParticles(0x3709, 10, 30, 5052, 2719, 0, EffectLayer.LeftFoot, 0);
                defender.PlaySound(0x208);

                int d;

                if (defender is PlayerMobile)
                    d = (int)(BaseSkillBonus / 6);
                else
                    d = (int)(BaseSkillBonus / 3) + Utility.RandomMinMax(40, 60);

                switch (_Type)
                {
                    case ResistanceType.Physical:
                        AOS.Damage(defender, Caster, d, 100, 0, 0, 0, 0);
                        break;
                    case ResistanceType.Fire:
                        AOS.Damage(defender, Caster, d, 0, 100, 0, 0, 0);
                        break;
                    case ResistanceType.Cold:
                        AOS.Damage(defender, Caster, d, 0, 0, 100, 0, 0);
                        break;
                    case ResistanceType.Poison:
                        AOS.Damage(defender, Caster, d, 0, 0, 0, 100, 0);
                        break;
                    case ResistanceType.Energy:
                        AOS.Damage(defender, Caster, d, 0, 0, 0, 0, 100);
                        break;
                }

                BuffInfo.RemoveBuff(defender, BuffIcon.ElementalFuryDebuff);
                _Table.Remove(defender);
            }
        }

        public override void EndEffects()
        {
            BuffInfo.RemoveBuff(Caster, BuffIcon.ElementalFury);

            if (_Table != null)
            {
                foreach (Mobile m in _Table.Keys)
                {
                    BuffInfo.RemoveBuff(m, BuffIcon.ElementalFuryDebuff);
                }

                _Table.Clear();
            }
        }

        private ResistanceType GetResistanceType(BaseWeapon weapon)
        {
            if (weapon == null)
                return ResistanceType.Physical;

            int phys, fire, cold, pois, nrgy, chaos, direct;
            weapon.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            int highest = phys;
            int type = 0;

            if (fire > phys)
            {
                type = 1;
                highest = fire;
            }

            if (cold > highest)
            {
                type = 2;
                highest = cold;
            }

            if (pois > highest)
            {
                type = 3;
                highest = pois;
            }

            if (nrgy > highest)
            {
                type = 4;
                highest = nrgy;
            }

            return (ResistanceType)type;
        }
    }
}
