using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Server.Spells.SkillMasteries
{
    public class SkillMasteryMove : SpecialMove
    {
        public Dictionary<Mobile, DateTime> Cooldown { get; set; }

        public virtual TimeSpan CooldownPeriod => TimeSpan.MinValue;
        public override bool ValidatesDuringHit => false;

        public override void SendAbilityMessage(Mobile m)
        {
            if (AbilityMessage.Number > 0)
                m.PrivateOverheadMessage(MessageType.Regular, 1150, AbilityMessage.Number, m.NetState);
            else
                m.PrivateOverheadMessage(MessageType.Regular, 1150, false, AbilityMessage.String, m.NetState);
        }

        public override bool Validate(Mobile from)
        {
            SkillMasteryMove move = GetCurrentMove(from) as SkillMasteryMove;

            if ((move == null || move.GetType() != GetType()) && !CheckCooldown(from))
                return false;

            if (from.Player && from.Skills.CurrentMastery != MoveSkill)
            {
                from.SendLocalizedMessage(1115664); // You are not on the correct path for using this mastery ability.
                return false;
            }

            return base.Validate(from);
        }

        public bool CheckCooldown(Mobile from)
        {
            if (CooldownPeriod > TimeSpan.MinValue && IsInCooldown(from))
            {
                double left = (Cooldown[from] - DateTime.UtcNow).TotalMinutes;

                if (left > 1)
                {
                    from.SendLocalizedMessage(1155787, ((int)left).ToString()); // You must wait ~1_minutes~ minutes before you can use this ability.
                }
                else
                {
                    left = (Cooldown[from] - DateTime.UtcNow).TotalSeconds;
                    from.SendLocalizedMessage(1079335, left.ToString("F", CultureInfo.InvariantCulture)); // You must wait ~1_seconds~ seconds before you can use this ability again.
                }

                return false;
            }

            return true;
        }

        public bool CheckWeapon(Mobile from)
        {
            if (!from.Player)
            {
                return true;
            }

            return from.Weapon is BaseWeapon wep && wep.DefSkill == MoveSkill;
        }

        public virtual bool IsInCooldown(Mobile m)
        {
            return Cooldown != null && Cooldown.ContainsKey(m);
        }

        public virtual void AddToCooldown(Mobile m)
        {
            if (CooldownPeriod > TimeSpan.MinValue)
            {
                if (Cooldown == null)
                    Cooldown = new Dictionary<Mobile, DateTime>();

                Cooldown[m] = DateTime.UtcNow + CooldownPeriod;
                Timer.DelayCall(CooldownPeriod, () => RemoveCooldown(m));
            }
        }

        public virtual void RemoveCooldown(Mobile m)
        {
            Cooldown.Remove(m);
        }

        public virtual void OnGotHit(Mobile attacker, Mobile defender, ref int damage)
        {
        }

        public virtual void OnDamaged(Mobile attacker, Mobile defender, DamageType type, ref int damage)
        {
        }
    }
}
