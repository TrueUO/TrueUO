using Server.Engines.Craft;
using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(ExodusSacrificalGargishDagger))]
    [Flipable(0x2D21, 0x2D2D)]
    public class ExodusSacrificalDagger : BaseKnife
    {
        public override int LabelNumber => 1153500;  // exodus sacrificial dagger
        private int m_Lifespan;
        private Timer m_Timer;

        [Constructable]
        public ExodusSacrificalDagger() : base(0x2D2D)
        {
            Weight = 4.0;
            Layer = Layer.OneHanded;
            Hue = 2500;

            if (Lifespan > 0)
            {
                m_Lifespan = Lifespan;
                StartTimer();
            }
        }

        public override int InitMinHits => 60;
        public override int InitMaxHits => 60;
        public override int StrengthReq => 15;
        public override SkillName DefSkill => SkillName.Fencing;
        public override float Speed => 2.00f;
        public override int MinDamage => 10;
        public override int MaxDamage => 12;
        public override int PhysicalResistance => 12;

        public override void OnDoubleClick(Mobile from)
        {
            RobeofRite robe = from.FindItemOnLayer(Layer.OuterTorso) as RobeofRite;
            ExodusSacrificalDagger dagger = from.FindItemOnLayer(Layer.OneHanded) as ExodusSacrificalDagger;

            if (Party.Get(from) == null)
            {
                from.SendLocalizedMessage(1153596); // You must join a party with the players you wish to perform the ritual with. 
            }
            else if (robe == null || dagger == null)
            {
                from.SendLocalizedMessage(1153591); // Thou art not properly attired to perform such a ritual.
            }
            else if (!((PlayerMobile)from).UseSummoningRite)
            {
                from.SendLocalizedMessage(1153603); // You must first use the Summoning Rite on a Summoning Tome.
            }
            else
            {
                from.SendLocalizedMessage(1153604); // Target the summoning tome or yourself to declare your intentions for performing this ritual...
                from.Target = new SacrificalTarget(this);
            }
        }

        public class SacrificalTarget : Target
        {
            private readonly Item m_Dagger;

            public SacrificalTarget(Item dagger) : base(2, true, TargetFlags.None)
            {
                m_Dagger = dagger;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is ExodusTomeAltar altar)
                {
                    if (altar.CheckParty(altar.Owner, from))
                    {
                        bool SacrificalRitual = altar.Rituals.Find(s => s.RitualMobile == from).Ritual2;

                        if (!SacrificalRitual)
                        {
                            ((PlayerMobile)from).UseSummoningRite = false;
                            from.Say(1153605); // *You thrust the dagger into your flesh as tribute to Exodus!*
                            altar.Rituals.Find(s => s.RitualMobile == from).Ritual2 = true;
                            m_Dagger.Delete();
                            Misc.Titles.AwardKarma(from, 10000, true);
                            Effects.SendLocationParticles(EffectItem.Create(altar.Location, altar.Map, TimeSpan.FromSeconds(2)), 0x373A, 10, 10, 2023);

                            from.SendLocalizedMessage(1153598, from.Name); // ~1_PLAYER~ has read the Summoning Rite! 
                        }
                        else
                        {
                            from.SendLocalizedMessage(1153599); // You've already used this item in another ritual. 
                        }
                    }
                    else
                    {
                        from.SendLocalizedMessage(1153595); // You must first join the party of the person who built this altar.
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1153601); // That is not a Summoning Tome. 
                }
            }
        }

        public ExodusSacrificalDagger(Serial serial) : base(serial)
        {
        }

        public virtual int Lifespan => 604800;

        [CommandProperty(AccessLevel.GameMaster)]
        public int TimeLeft
        {
            get => m_Lifespan;
            set
            {
                m_Lifespan = value;
                InvalidateProperties();
            }
        }
        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (Lifespan > 0)
            {
                TimeSpan t = TimeSpan.FromSeconds(m_Lifespan);

                int weeks = t.Days / 7;
                int days = t.Days;
                int hours = t.Hours;
                int minutes = t.Minutes;

                if (weeks > 0)
                    list.Add(string.Format("Lifespan: {0} {1}", weeks, weeks == 1 ? "week" : "weeks"));
                else if (days > 0)
                    list.Add(string.Format("Lifespan: {0} {1}", days, days == 1 ? "day" : "days"));
                else if (hours > 0)
                    list.Add(string.Format("Lifespan: {0} {1}", hours, hours == 1 ? "hour" : "hours"));
                else if (minutes > 0)
                    list.Add(string.Format("Lifespan: {0} {1}", minutes, minutes == 1 ? "minute" : "minutes"));
                else
                    list.Add(1072517, m_Lifespan.ToString()); // Lifespan: ~1_val~ seconds
            }
        }

        public virtual void StartTimer()
        {
            if (m_Timer != null)
                return;

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Slice);
        }

        public virtual void StopTimer()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        public virtual void Slice()
        {
            m_Lifespan -= 10;

            InvalidateProperties();

            if (m_Lifespan <= 0)
                Decay();
        }

        public virtual void Decay()
        {
            if (RootParent is Mobile mobile)
            {
                Mobile parent = mobile;

                if (Name == null)
                    parent.SendLocalizedMessage(1072515, "#" + LabelNumber); // The ~1_name~ expired...
                else
                    parent.SendLocalizedMessage(1072515, Name); // The ~1_name~ expired...

                Effects.SendLocationParticles(EffectItem.Create(parent.Location, parent.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(parent.Location, parent.Map, 0x201);
            }
            else
            {
                Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(Location, Map, 0x201);
            }

            StopTimer();
            Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Lifespan);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Lifespan = reader.ReadInt();

            StartTimer();
        }
    }

    [Flipable(0x0902, 0x406A)]
    public class ExodusSacrificalGargishDagger : ExodusSacrificalDagger
    {
        [Constructable]
        public ExodusSacrificalGargishDagger()
        {
            ItemID = 0x406A;
            Weight = 4.0;
        }

        public ExodusSacrificalGargishDagger(Serial serial) : base(serial)
        {
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
    }
}
