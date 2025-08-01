namespace Server.Items
{
    /// <summary>
    /// Send two arrows flying at your opponent if you're mounted. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public sealed class DoubleShot : WeaponAbility
    {
        public override int BaseMana => 30;

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            if (attacker.Weapon is BaseWeapon wep)
            {
                wep.ProcessingMultipleHits = true;
            }

            return true;
        }

        public override SkillName GetSecondarySkill(Mobile from)
        {
            return from.Skills[SkillName.Ninjitsu].Base > from.Skills[SkillName.Bushido].Base ? SkillName.Ninjitsu : SkillName.Bushido;
        }

        public override double GetRequiredSecondarySkill(Mobile from)
        {
            if (from.Weapon is BaseWeapon weapon)
            {
                if (weapon.PrimaryAbility == this)
                {
                    return 30.0;
                }

                if (weapon.SecondaryAbility == this)
                {
                    return 50.0;
                }
            }

            return 200.0;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            Use(attacker, defender);
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            Use(attacker, defender);
        }

        public override bool Validate(Mobile from)
        {
            if (base.Validate(from))
            {
                if (from.Mounted)
                    return true;

                from.SendLocalizedMessage(1070770); // You can only execute this attack while mounted!
                ClearCurrentAbility(from);
            }

            return false;
        }

        public void Use(Mobile attacker, Mobile defender)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true) || attacker.Weapon == null)	//sanity
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063348); // You launch two shots at once!
            defender.SendLocalizedMessage(1063349); // You're attacked with a barrage of shots!

            defender.FixedParticles(0x37B9, 1, 19, 0x251D, EffectLayer.Waist);

            attacker.Weapon.OnSwing(attacker, defender);

            if (attacker.Weapon is BaseWeapon weapon)
                weapon.ProcessingMultipleHits = false;
        }
    }
}
