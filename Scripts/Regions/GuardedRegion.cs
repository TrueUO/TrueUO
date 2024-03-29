using Server.Commands;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Server.Regions
{
    public class GuardedRegion : BaseRegion
    {
        private static readonly object[] _GuardParams = new object[1];

        private readonly Dictionary<Mobile, GuardTimer> _GuardCandidates = new Dictionary<Mobile, GuardTimer>();

        private readonly Type _GuardType;

        private bool _Disabled;

        public GuardedRegion(string name, Map map, int priority, params Rectangle3D[] area)
            : base(name, map, priority, area)
        {
            _GuardType = DefaultGuardType;
        }

        public GuardedRegion(string name, Map map, int priority, params Rectangle2D[] area)
            : base(name, map, priority, area)
        {
            _GuardType = DefaultGuardType;
        }

        public GuardedRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            XmlElement el = xml["guards"];

            if (ReadType(el, "type", ref _GuardType, false))
            {
                if (!typeof(Mobile).IsAssignableFrom(_GuardType))
                {
                    Console.WriteLine("Invalid guard type for region '{0}'", this);
                    _GuardType = DefaultGuardType;
                }
            }
            else
            {
                _GuardType = DefaultGuardType;
            }

            bool disabled = false;
            if (ReadBoolean(el, "disabled", ref disabled, false))
            {
                Disabled = disabled;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Disabled { get => _Disabled; set => _Disabled = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual bool AllowReds => true;

        public virtual Type DefaultGuardType
        {
            get
            {
                if (Map == Map.Ilshenar || Map == Map.Malas)
                {
                    return typeof(ArcherGuard);
                }

                return typeof(WarriorGuard);
            }
        }

        public static void Initialize()
        {
            CommandSystem.Register("CheckGuarded", AccessLevel.GameMaster, CheckGuarded_OnCommand);
            CommandSystem.Register("SetGuarded", AccessLevel.Administrator, SetGuarded_OnCommand);
            CommandSystem.Register("ToggleGuarded", AccessLevel.Administrator, ToggleGuarded_OnCommand);
        }

        public static GuardedRegion Disable(GuardedRegion reg)
        {
            reg.Disabled = true;
            return reg;
        }

        public virtual bool IsDisabled()
        {
            return _Disabled;
        }

        public virtual bool CheckVendorAccess(BaseVendor vendor, Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster || IsDisabled())
            {
                return true;
            }

            return !from.Murderer;
        }

        public override bool OnBeginSpellCast(Mobile m, ISpell s)
        {
            if (!IsDisabled() && !s.OnCastInTown(this))
            {
                m.SendLocalizedMessage(500946); // You cannot cast this in town!
                return false;
            }

            return base.OnBeginSpellCast(m, s);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        public override void MakeGuard(Mobile focus)
        {
            BaseGuard useGuard = null;
            IPooledEnumerable eable = focus.GetMobilesInRange(8);

            foreach (Mobile m in eable)
            {
                if (m is BaseGuard g && g.Focus == null) // idling
                {
                    useGuard = g;
                    break;
                }
            }

            eable.Free();

            if (useGuard == null)
            {
                _GuardParams[0] = focus;

                try
                {
                    Activator.CreateInstance(_GuardType, _GuardParams);
                }
                catch (Exception e)
                {
                    Diagnostics.ExceptionLogging.LogException(e);
                }
            }
            else
            {
                useGuard.Focus = focus;
            }
        }

        public override void OnEnter(Mobile m)
        {
            if (IsDisabled())
            {
                return;
            }

            if (!AllowReds && m.Murderer)
            {
                CheckGuardCandidate(m);
            }
        }

        public override void OnExit(Mobile m)
        {
            if (IsDisabled())
            {
                return;
            }
        }

        public override void OnSpeech(SpeechEventArgs args)
        {
            base.OnSpeech(args);

            if (IsDisabled())
            {
                return;
            }

            if (args.Mobile.Alive && args.HasKeyword(0x0007)) // *guards*
            {
                CallGuards(args.Mobile.Location);
            }
        }

        public override void OnAggressed(Mobile aggressor, Mobile aggressed, bool criminal)
        {
            base.OnAggressed(aggressor, aggressed, criminal);

            if (!IsDisabled() && aggressor != aggressed && criminal && Utility.InRange(aggressor.Location, aggressed.Location, 12))
            {
                CheckGuardCandidate(aggressor, aggressor is BaseCreature creature && creature.IsAggressiveMonster);
            }
        }

        public override void OnGotBeneficialAction(Mobile helper, Mobile helped)
        {
            base.OnGotBeneficialAction(helper, helped);

            if (IsDisabled() || Siege.SiegeShard)
            {
                return;
            }

            int noto = Notoriety.Compute(helper, helped);

            if (helper != helped && (noto == Notoriety.Criminal || noto == Notoriety.Murderer))
            {
                CheckGuardCandidate(helper);
            }
        }

        public override void OnCriminalAction(Mobile m, bool message)
        {
            base.OnCriminalAction(m, message);

            if (!IsDisabled())
            {
                CheckGuardCandidate(m);
            }
        }

        public void CheckGuardCandidate(Mobile m)
        {
            CheckGuardCandidate(m, false);
        }

        public void CheckGuardCandidate(Mobile m, bool autoCallGuards)
        {
            if (IsDisabled())
            {
                return;
            }

            if (IsGuardCandidate(m))
            {
                _GuardCandidates.TryGetValue(m, out GuardTimer timer);

                if (autoCallGuards)
                {
                    MakeGuard(m);

                    if (timer != null)
                    {
                        timer.Stop();
                        _GuardCandidates.Remove(m);
                        m.SendLocalizedMessage(502276); // Guards can no longer be called on you.
                    }
                }
                else if (timer == null)
                {
                    timer = new GuardTimer(m, _GuardCandidates);
                    timer.Start();

                    _GuardCandidates[m] = timer;
                    m.SendLocalizedMessage(502275); // Guards can now be called on you!

                    Map map = m.Map;

                    if (map != null)
                    {
                        Mobile fakeCall = null;
                        double prio = 0.0;

                        IPooledEnumerable eable = m.GetMobilesInRange(8);

                        foreach (Mobile v in eable)
                        {
                            if (!v.Player && v != m && !IsGuardCandidate(v) && (v is BaseCreature bc ? bc.IsHumanInTown() : v.Body.IsHuman && v.Region.IsPartOf(this)))
                            {
                                double dist = m.GetDistanceToSqrt(v);

                                if (fakeCall == null || dist < prio)
                                {
                                    fakeCall = v;
                                    prio = dist;
                                }
                            }
                        }

                        eable.Free();

                        if (fakeCall != null)
                        {
                            fakeCall.Say(Utility.RandomList(1007037, 501603, 1013037, 1013038, 1013039, 1013041, 1013042, 1013043, 1013052));
                            MakeGuard(m);
                            timer.Stop();
                            _GuardCandidates.Remove(m);
                            m.SendLocalizedMessage(502276); // Guards can no longer be called on you.
                        }
                    }
                }
                else
                {
                    timer.Stop();
                    timer.Start();
                }
            }
        }

        public void CallGuards(Point3D p)
        {
            if (IsDisabled())
            {
                return;
            }

            IPooledEnumerable eable = Map.GetMobilesInRange(p, 14);

            foreach (Mobile m in eable)
            {
                if (IsGuardCandidate(m))
                {
                    if (_GuardCandidates.ContainsKey(m) || !AllowReds && m.Murderer && m.Region.IsPartOf(this))
                    {
                        _GuardCandidates.TryGetValue(m, out GuardTimer timer);

                        if (timer != null)
                        {
                            timer.Stop();
                            _GuardCandidates.Remove(m);
                        }

                        MakeGuard(m);
                        m.SendLocalizedMessage(502276); // Guards can no longer be called on you.
                    }
                    else if (m is BaseCreature creature && creature.IsAggressiveMonster && creature.Region.IsPartOf(this))
                    {
                        MakeGuard(creature);
                    }

                    break;
                }
            }

            eable.Free();
        }

        public bool IsGuardCandidate(Mobile m)
        {
            if (m is BaseGuard || m.GuardImmune || !m.Alive || m.IsStaff() || m.Blessed || m is BaseCreature bc && bc.IsInvulnerable || IsDisabled())
            {
                return false;
            }

            return !AllowReds && m.Murderer || m.Criminal || m is BaseCreature creature && creature.IsAggressiveMonster;
        }

        [Usage("CheckGuarded")]
        [Description("Returns a value indicating if the current region is guarded or not.")]
        private static void CheckGuarded_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            GuardedRegion reg = (GuardedRegion)from.Region.GetRegion(typeof(GuardedRegion));

            if (reg == null)
            {
                from.SendMessage("You are not in a guardable region.");
            }
            else if (reg.Disabled)
            {
                from.SendMessage("The guards in this region have been disabled.");
            }
            else
            {
                from.SendMessage("This region is actively guarded.");
            }
        }

        [Usage("SetGuarded <true|false>")]
        [Description("Enables or disables guards for the current region.")]
        private static void SetGuarded_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length == 1)
            {
                GuardedRegion reg = (GuardedRegion)from.Region.GetRegion(typeof(GuardedRegion));

                if (reg == null)
                {
                    from.SendMessage("You are not in a guardable region.");
                }
                else
                {
                    reg.Disabled = !e.GetBoolean(0);

                    if (reg.Disabled)
                    {
                        from.SendMessage("The guards in this region have been disabled.");
                    }
                    else
                    {
                        from.SendMessage("The guards in this region have been enabled.");
                    }
                }
            }
            else
            {
                from.SendMessage("Format: SetGuarded <true|false>");
            }
        }

        [Usage("ToggleGuarded")]
        [Description("Toggles the state of guards for the current region.")]
        private static void ToggleGuarded_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            GuardedRegion reg = (GuardedRegion)from.Region.GetRegion(typeof(GuardedRegion));

            if (reg == null)
            {
                from.SendMessage("You are not in a guardable region.");
            }
            else
            {
                reg.Disabled = !reg.Disabled;

                if (reg.Disabled)
                {
                    from.SendMessage("The guards in this region have been disabled.");
                }
                else
                {
                    from.SendMessage("The guards in this region have been enabled.");
                }
            }
        }

        private class GuardTimer : Timer
        {
            private readonly Mobile _Mobile;
            private readonly Dictionary<Mobile, GuardTimer> _Table;

            public GuardTimer(Mobile m, Dictionary<Mobile, GuardTimer> table)
                : base(TimeSpan.FromSeconds(15.0))
            {
                _Mobile = m;
                _Table = table;
            }

            protected override void OnTick()
            {
                if (_Table.Remove(_Mobile))
                {
                    _Mobile.SendLocalizedMessage(502276); // Guards can no longer be called on you.
                }
            }
        }
    }
}
