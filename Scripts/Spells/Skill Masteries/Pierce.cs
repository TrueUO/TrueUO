using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

/*The fencer executes a piercing move on their opponent causing stamina drain on the 
  victim based on the fencer's fencing and tactics skill, and mastery level.*/

namespace Server.Spells.SkillMasteries
{
    public class PierceSpell : SkillMasteryMove
    {
        public override int BaseMana => 20;
        public override double RequiredSkill => 90.0;

        public override SkillName MoveSkill => SkillName.Fencing;
        public override TextDefinition AbilityMessage => new(1155991);  // You ready yourself to pierce your opponent!

        private Dictionary<Mobile, Timer> _Table;

        public override bool Validate(Mobile from)
        {
            if (!CheckWeapon(from))
            {
                from.SendLocalizedMessage(1156005); // You must have a fencing weapon equipped to use this ability.
                return false;
            }

            return base.Validate(from);
        }

        public override void OnUse(Mobile from)
        {
            if (from.Player)
            {
                from.PlaySound(from.Female ? 0x338 : 0x44A);
            }
            else if (from is BaseCreature bc)
            {
                bc.PlaySound(bc.GetAngerSound());
            }

            from.FixedParticles(0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentMove(attacker);

            if (attacker.Weapon is BaseWeapon && (_Table == null || !_Table.ContainsKey(attacker)))
            {
                int toDrain = (int)(attacker.Skills[MoveSkill].Value + attacker.Skills[SkillName.Tactics].Value + (MasteryInfo.GetMasteryLevel(attacker, SkillName.Fencing) * 40) / 3);
                toDrain /= 3;

                Timer t;

                if (_Table == null)
                    _Table = new Dictionary<Mobile, Timer>();

                _Table[attacker] = t = new InternalTimer(this, attacker, defender, toDrain);
                t.Start();

                attacker.PrivateOverheadMessage(MessageType.Regular, 1150, 1155993, attacker.NetState); // You deliver a piercing blow!
                defender.FixedEffect(0x36BD, 20, 10, 2725, 5);

                int drain = (int)(defender.StamMax * (toDrain / 100.0));

                BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.Pierce, 1155994, 1155995, TimeSpan.FromSeconds(10), defender, (drain / 7).ToString()));
                //-~1_VAL~ Stamina Regeneration.
            }
            else
            {
                attacker.SendLocalizedMessage(1095215); // Your target is already under the effect of this attack.
            }
        }

        public void RemoveEffects(Mobile attacker)
        {
            if (_Table != null && _Table.TryGetValue(attacker, out Timer value))
            {
                value.Stop();
                _Table.Remove(attacker);
            }
        }

        private class InternalTimer : Timer
        {
            public Mobile Attacker { get; }
            public Mobile Defender { get; }
            public PierceSpell Spell { get; }
            public int ToDrain { get; }
            public int Ticks { get; private set; }

            public InternalTimer(PierceSpell spell, Mobile attacker, Mobile defender, int toDrain)
                : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            {
                Spell = spell;
                Attacker = attacker;
                Defender = defender;
                ToDrain = toDrain;

                Ticks = 0;
            }

            protected override void OnTick()
            {
                if (!Defender.Alive || Ticks >= 10)
                {
                    Spell.RemoveEffects(Attacker);
                    return;
                }

                Ticks++;
                Defender.Stam = Math.Max(0, Defender.Stam - ((ToDrain / 10) + Utility.RandomMinMax(-2, 2)));
            }
        }
    }
}
