using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using Server.Engines.Khaldun;

namespace Server.SkillHandlers
{
    internal class SpiritSpeak
    {
        public static void Initialize()
        {
            SkillInfo.Table[32].Callback = OnUse;
        }

        private static Dictionary<Mobile, Timer> _Table;

        private static TimeSpan OnUse(Mobile m)
        {
            if (m.Spell != null && m.Spell.IsCasting)
            {
                m.SendLocalizedMessage(502642); // You are already casting a spell.
            }
            else if (BeginSpiritSpeak(m))
            {
                return TimeSpan.FromSeconds(5.0);
            }

            return TimeSpan.Zero;
        }

        private static bool BeginSpiritSpeak(Mobile m)
        {
            if (_Table == null || !_Table.ContainsKey(m))
            {
                m.RevealingAction();
                m.Freeze(TimeSpan.FromSeconds(1));

                m.Animate(AnimationType.Spell, 1);
                m.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1062074, "", false); // Anh Mi Sah Ko
                m.PlaySound(0x24A);

                if (_Table == null)
                {
                    _Table = new Dictionary<Mobile, Timer>();
                }

                _Table[m] = new SpiritSpeakTimerNew(m);
                return true;
            }

            return false;
        }

        public static bool IsInSpiritSpeak(Mobile m)
        {
            return _Table != null && _Table.ContainsKey(m);
        }

        private static void Remove(Mobile m)
        {
            if (_Table != null && _Table.TryGetValue(m, out Timer value))
            {
                if (value != null)
                {
                    value.Stop();
                }

                m.SendSpeedControl(m.Region is Regions.TwistedWealdDesert ? SpeedControlType.WalkSpeed : SpeedControlType.Disable);

                _Table.Remove(m);

                if (_Table.Count == 0)
                {
                    _Table = null;
                }
            }
        }

        public static void CheckDisrupt(Mobile m)
        {
            if (_Table != null && _Table.ContainsKey(m))
            {
                if (m is PlayerMobile)
                {
                    m.SendLocalizedMessage(500641); // Your concentration is disturbed, thus ruining thy spell.
                }

                m.FixedEffect(0x3735, 6, 30);
                m.PlaySound(0x5C);

                m.NextSkillTime = Core.TickCount;

                Remove(m);
            }
        }

        private class SpiritSpeakTimerNew : Timer
        {
            private Mobile Caster { get; }

            public SpiritSpeakTimerNew(Mobile m)
                : base(TimeSpan.FromSeconds(1))
            {
                Start();
                Caster = m;
            }

            protected override void OnTick()
            {
                Corpse toChannel = null;

                IPooledEnumerable eable = Caster.GetObjectsInRange(3);

                foreach (object objs in eable)
                {
                    if (objs is Corpse corpse && !corpse.Channeled && !corpse.Animated)
                    {
                        toChannel = corpse;
                        break;
                    }

                    if (objs is SageHumbolt humbolt && humbolt.OnSpiritSpeak(Caster))
                    {
                        eable.Free();
                        Remove(Caster);
                        Stop();
                        return;
                    }
                }

                eable.Free();

                int max, min, mana, number;

                if (toChannel != null)
                {
                    min = 1 + (int)(Caster.Skills[SkillName.SpiritSpeak].Value * 0.25);
                    max = min + 4;
                    mana = 0;
                    number = 1061287; // You channel energy from a nearby corpse to heal your wounds.
                }
                else
                {
                    min = 1 + (int)(Caster.Skills[SkillName.SpiritSpeak].Value * 0.25);
                    max = min + 4;
                    mana = 10;
                    number = 1061286; // You channel your own spiritual energy to heal your wounds.
                }

                if (Caster.Mana < mana)
                {
                    Caster.SendLocalizedMessage(1061285); // You lack the mana required to use this skill.
                }
                else
                {
                    Caster.CheckSkill(SkillName.SpiritSpeak, 0.0, 120.0);

                    if (Utility.RandomDouble() > Caster.Skills[SkillName.SpiritSpeak].Value / 100.0)
                    {
                        Caster.SendLocalizedMessage(502443); // You fail your attempt at contacting the netherworld.
                    }
                    else
                    {
                        if (toChannel != null)
                        {
                            toChannel.Channeled = true;
                            toChannel.Hue = 0x835;
                        }

                        Caster.Mana -= mana;
                        Caster.SendLocalizedMessage(number);

                        if (min > max)
                        {
                            min = max;
                        }

                        Caster.Hits += Utility.RandomMinMax(min, max);

                        Caster.FixedParticles(0x375A, 1, 15, 9501, 2100, 4, EffectLayer.Waist);
                    }
                }

                Remove(Caster);
                Stop();
            }
        }
    }
}
