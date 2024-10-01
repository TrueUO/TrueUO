using Server.Spells;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class ForceArrow : WeaponAbility
    {
        public override int BaseMana => 20;

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
            {
                return;
            }

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1074381); // You fire an arrow of pure force.
            defender.SendLocalizedMessage(1074382); // You are struck by a force arrow!

            if (0.4 > Utility.RandomDouble())
            {
                defender.Combatant = null;
                defender.Warmode = false;
            }

            ForceArrowInfo info = GetInfo(attacker, defender);

            if (info == null)
            {
                BeginForceArrow(attacker, defender);
            }
            else
            {
                if (info.Timer != null && info.Timer.Running)
                {
                    info.Timer.IncreaseExpiration();

                    BuffInfo.RemoveBuff(defender, BuffIcon.ForceArrow);
                    BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ForceArrow, 1151285, 1151286, info.DefenseChanceMalus.ToString()));
                }
            }

            if (defender.Spell is Spell spell && spell.IsCasting)
            {
                spell.Disturb(DisturbType.Hurt, false, true);
            }
        }

        private static readonly Dictionary<Mobile, List<ForceArrowInfo>> _Table = new Dictionary<Mobile, List<ForceArrowInfo>>();

        public static void BeginForceArrow(Mobile attacker, Mobile defender)
        {
            ForceArrowInfo info = new ForceArrowInfo(attacker, defender);
            info.Timer = new ForceArrowTimer(info);

            if (!_Table.TryGetValue(attacker, out List<ForceArrowInfo> value))
            {
                value = new List<ForceArrowInfo>();
                _Table[attacker] = value;
            }

            value.Add(info);

            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ForceArrow, 1151285, 1151286, info.DefenseChanceMalus.ToString()));
        }

        public static void EndForceArrow(ForceArrowInfo info)
        {
            if (info == null)
            {
                return;
            }

            Mobile attacker = info.Attacker;

            if (_Table.TryGetValue(attacker, out List<ForceArrowInfo> value) && value.Contains(info))
            {
                value.Remove(info);

                if (value.Count == 0)
                {
                    _Table.Remove(attacker);
                }
            }

            BuffInfo.RemoveBuff(info.Defender, BuffIcon.ForceArrow);
        }

        public static bool HasForceArrow(Mobile attacker, Mobile defender)
        {
            if (!_Table.TryGetValue(attacker, out List<ForceArrowInfo> value))
            {
                return false;
            }

            for (int index = 0; index < value.Count; index++)
            {
                ForceArrowInfo info = value[index];

                if (info.Defender == defender)
                {
                    return true;
                }
            }

            return false;
        }

        public static ForceArrowInfo GetInfo(Mobile attacker, Mobile defender)
        {
            if (!_Table.TryGetValue(attacker, out List<ForceArrowInfo> value))
            {
                return null;
            }

            for (int index = 0; index < value.Count; index++)
            {
                ForceArrowInfo info = value[index];

                if (info.Defender == defender)
                {
                    return info;
                }
            }

            return null;
        }

        public class ForceArrowInfo
        {
            private readonly Mobile _Attacker;
            private readonly Mobile _Defender;
            private ForceArrowTimer _Timer;
            private int _DefenseChanceMalus;

            public Mobile Attacker => _Attacker;
            public Mobile Defender => _Defender;
            public ForceArrowTimer Timer { get => _Timer; set => _Timer = value; }
            public int DefenseChanceMalus { get => _DefenseChanceMalus; set => _DefenseChanceMalus = value; }

            public ForceArrowInfo(Mobile attacker, Mobile defender)
            {
                _Attacker = attacker;
                _Defender = defender;
                _DefenseChanceMalus = 10;
            }
        }

        public class ForceArrowTimer : Timer
        {
            private readonly ForceArrowInfo _Info;
            private DateTime _Expires;

            public ForceArrowTimer(ForceArrowInfo info)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1))
            {
                _Info = info;
                
                _Expires = DateTime.UtcNow + TimeSpan.FromSeconds(10);

                Start();
            }

            protected override void OnTick()
            {
                if (_Expires < DateTime.UtcNow)
                {
                    Stop();
                    EndForceArrow(_Info);
                }
            }

            public void IncreaseExpiration()
            {
                _Expires = _Expires + TimeSpan.FromSeconds(2);

                _Info.DefenseChanceMalus += 5;
            }
        }
    }
}
