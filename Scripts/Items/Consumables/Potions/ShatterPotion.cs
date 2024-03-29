using Server.Network;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
    public class ShatterPotion : BasePotion
    {
        public override int LabelNumber => 1115759;  // Shatter Potion

        public override bool RequireFreeHand => false;

        [Constructable]
        public ShatterPotion()
            : base(0xF0D, PotionEffect.Shatter)
        {
            Hue = 60;
            Weight = 2.0;
        }

        public ShatterPotion(Serial serial)
            : base(serial)
        {
        }

        public override void Drink(Mobile from)
        {
            if (from.Paralyzed || from.Frozen || from.Spell != null && from.Spell.IsCasting)
            {
                from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
                return;
            }

            int delay = GetDelay(from);

            if (delay > 0)
            {
                from.SendLocalizedMessage(1072529, $"{delay}\t{(delay > 1 ? "seconds." : "second.")}"); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
                return;
            }

            if (from.Target is ThrowTarget targ && targ.Potion == this)
            {
                return;
            }

            from.RevealingAction();

            if (!m_Users.Contains(from))
                m_Users.Add(from);

            from.Target = new ThrowTarget(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        private readonly List<Mobile> m_Users = new List<Mobile>();

        public void Explode_Callback(object state)
        {
            object[] states = (object[])state;

            Explode((Mobile)states[0], (Mobile)states[1], (Map)states[2]);
        }

        public virtual void Explode(Mobile from, Mobile m, Map map)
        {
            if (Deleted || map == null)
                return;

            Consume();

            // Check if any other players are using this potion
            for (int i = 0; i < m_Users.Count; i++)
            {
                if (m_Users[i].Target is ThrowTarget targ && targ.Potion == this)
                {
                    Target.Cancel(from);
                }
            }

            // Effects
            Effects.PlaySound(m.Location, map, 285);

            m.Damage(0);
            Effects.SendPacket(m.Location, m.Map, new ParticleEffect(EffectType.FixedFrom, Serial, Serial.Zero, 0x373A, m.Location, m.Location, 10, 10, false, false, 0, 0, 0, 5051, 1, Serial.Zero, 89, 0));

            int amount = 0;

            if (m.Backpack != null)
            {
                foreach (BasePotion p in m.Backpack.FindItemsByType<BasePotion>())
                {
                    amount += p.Amount;
                }
            }

            if (amount < 20)
            {
                from.SendLocalizedMessage(1115760, from.Name); // ~1_NAME~'s shatter potion hits you, but nothing happens.
            }
            else
            {
                int p = (int)(amount * 0.2);

                if (p > 1)
                {
                    from.SendLocalizedMessage(1115762, $"{from.Name}\t{p}"); // ~1_NAME~'s shatter potion destroys ~2_NUM~ potions in your inventory.                    
                }
                else
                {
                    from.SendLocalizedMessage(1115761, from.Name); // ~1_NAME~'s shatter potion destroys a potion in your inventory.
                }

                for (int i = 0; i < p; i++)
                {
                    if (m.Backpack != null)
                    {
                        List<BasePotion> potions = m.Backpack.FindItemsByType<BasePotion>();

                        potions[Utility.Random(potions.Count)].Consume();
                    }
                }
            }
        }

        #region Delay
        private static readonly Hashtable m_Delay = new Hashtable();

        public static void AddDelay(Mobile m)
        {
            if (m_Delay[m] is Timer timer)
            {
                timer.Stop();
            }

            m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(60), EndDelay_Callback, m);
        }

        public static int GetDelay(Mobile m)
        {
            if (m_Delay[m] is Timer timer && timer.Next > DateTime.UtcNow)
            {
                return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;
            }

            return 0;
        }

        private static void EndDelay_Callback(object obj)
        {
            if (obj is Mobile mobile)
            {
                EndDelay(mobile);
            }
        }

        public static void EndDelay(Mobile m)
        {
            if (m_Delay[m] is Timer timer)
            {
                timer.Stop();
                m_Delay.Remove(m);
            }
        }
        #endregion

        private class ThrowTarget : Target
        {
            public ShatterPotion Potion { get; }

            public ThrowTarget(ShatterPotion potion) : base(12, true, TargetFlags.None)
            {
                Potion = potion;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (Potion.Deleted || Potion.Map == Map.Internal)
                    return;

                if (targeted is Mobile m)
                {
                    if (from.Map == null || !from.CanBeHarmful(m))
                    {
                        return;
                    }

                    AddDelay(from);

                    from.RevealingAction();

                    Effects.SendMovingEffect(from, m, Potion.ItemID, 7, 0, false, false, Potion.Hue, 0);
                    Timer.DelayCall(TimeSpan.FromSeconds(1.0), Potion.Explode_Callback, new object[] { from, m, from.Map });
                }
            }
        }
    }
}
