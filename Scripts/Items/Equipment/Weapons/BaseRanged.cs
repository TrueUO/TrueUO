using System;
using Server.Mobiles;
using Server.Network.Packets;
using Server.Spells;

namespace Server.Items
{
    public abstract class BaseRanged : BaseMeleeWeapon
    {
        public abstract int EffectID { get; }
        public abstract Type AmmoType { get; }
        public abstract Item Ammo { get; }

        public override int DefHitSound => 0x234;
        public override int DefMissSound => 0x238;

        public override SkillName DefSkill => SkillName.Archery;
        public override WeaponType DefType => WeaponType.Ranged;
        public override WeaponAnimation DefAnimation => WeaponAnimation.ShootXBow;

        private Timer m_RecoveryTimer; // so we don't start too many timers
        private int m_Velocity;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Balanced
        {
            get => Attributes.BalancedWeapon > 0;
            set
            {
                if (value)
                    Attributes.BalancedWeapon = 1;
                else
                    Attributes.BalancedWeapon = 0;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Velocity
        {
            get => m_Velocity;
            set
            {
                m_Velocity = value;
                InvalidateProperties();
            }
        }

        public BaseRanged(int itemID)
            : base(itemID)
        { }

        public BaseRanged(Serial serial)
            : base(serial)
        { }

        public override TimeSpan OnSwing(Mobile attacker, IDamageable damageable)
        {
            long nextShoot;

            if (attacker is PlayerMobile mobile)
                nextShoot = mobile.NextMovementTime + 250;
            else
                nextShoot = attacker.LastMoveTime + attacker.ComputeMovementSpeed();

            // Make sure we've been standing still for .25/.5/1 second depending on Era
            if (nextShoot <= Core.TickCount || WeaponAbility.GetCurrentAbility(attacker) is MovingShot)
            {
                bool canSwing = !attacker.Paralyzed && !attacker.Frozen;

                if (canSwing)
                {
                    Spell sp = attacker.Spell as Spell;

                    canSwing = sp == null || !sp.IsCasting || !sp.BlocksMovement;
                }

                if (canSwing && attacker.HarmfulCheck(damageable))
                {
                    attacker.DisruptiveAction();
                    attacker.Send(new SwingPacket(0, attacker, damageable));

                    if (OnFired(attacker, damageable))
                    {
                        if (CheckHit(attacker, damageable))
                        {
                            OnHit(attacker, damageable);
                        }
                        else
                        {
                            OnMiss(attacker, damageable);
                        }
                    }
                }

                attacker.RevealingAction();

                return GetDelay(attacker);
            }

            attacker.RevealingAction();

            return TimeSpan.FromSeconds(0.25);
        }

        public override void OnHit(Mobile attacker, IDamageable damageable, double damageBonus)
        {
            if (AmmoType != null && attacker.Player && damageable is Mobile m && !m.Player && (m.Body.IsAnimal || m.Body.IsMonster) && 0.4 >= Utility.RandomDouble())
            {
                Item ammo = Ammo;

                if (ammo != null)
                {
                    m.AddToBackpack(ammo);
                }
            }

            base.OnHit(attacker, damageable, damageBonus);
        }

        public override void OnMiss(Mobile attacker, IDamageable damageable)
        {
            if (attacker.Player && 0.4 >= Utility.RandomDouble() && attacker is PlayerMobile p && AmmoType != null)
            {
                Type ammo = AmmoType;

                if (!p.RecoverableAmmo.TryAdd(ammo, 1))
                {
                    p.RecoverableAmmo[ammo]++;
                }

                if (!p.Warmode)
                {
                    if (m_RecoveryTimer == null)
                    {
                        m_RecoveryTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), p.RecoverAmmo);
                    }

                    if (!m_RecoveryTimer.Running)
                    {
                        m_RecoveryTimer.Start();
                    }
                }
            }

            base.OnMiss(attacker, damageable);
        }

        public virtual bool OnFired(Mobile attacker, IDamageable damageable)
        {
            WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

            // Respect special moves that use no ammo
            if (ability != null && ability.ConsumeAmmo == false)
            {
                return true;
            }

            if (attacker.Player)
            {
                BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
                Container pack = attacker.Backpack;

                int lowerAmmo = AosAttributes.GetValue(attacker, AosAttribute.LowerAmmoCost);

                if (quiver == null || Utility.Random(100) >= lowerAmmo)
                {
                    // consume ammo
                    if (quiver != null && quiver.ConsumeTotal(AmmoType, 1))
                    {
                        quiver.InvalidateWeight();
                    }
                    else if (pack == null || !pack.ConsumeTotal(AmmoType, 1))
                    {
                        return false;
                    }
                }
                else if (quiver.FindItemByType(AmmoType) == null && (pack == null || pack.FindItemByType(AmmoType) == null))
                {
                    // lower ammo cost should not work when we have no ammo at all
                    return false;
                }
            }

            attacker.MovingEffect(damageable, EffectID, 18, 1, false, false);

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(4); // version

            writer.Write(m_Velocity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Velocity = reader.ReadInt();
        }
    }
}
