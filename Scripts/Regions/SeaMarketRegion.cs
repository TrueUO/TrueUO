using Server.Commands;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Server.Regions
{
    public class SeaMarketRegion : BaseRegion
    {
        private static readonly TimeSpan _KickDuration = TimeSpan.FromMinutes(20);

        private static SeaMarketRegion _Region1;
        private static SeaMarketRegion _Region2;

        private Timer m_Timer;

        private static Timer m_BlabTimer;
        private static bool m_RestrictBoats;

        private readonly Dictionary<BaseBoat, DateTime> m_BoatTable = new Dictionary<BaseBoat, DateTime>();
        public Dictionary<BaseBoat, DateTime> BoatTable => m_BoatTable;

        public static bool RestrictBoats
        {
            get => m_RestrictBoats;
            set
            {
                m_RestrictBoats = value;

                if (value)
                {
                    _Region1?.StartTimer();
                    _Region2?.StartTimer();
                }
                else
                {
                    _Region1?.StopTimer();
                    _Region2?.StopTimer();
                }
            }
        }

        public static Rectangle2D[] Bounds => _Bounds;

        private static readonly Rectangle2D[] _Bounds =
        {
            new Rectangle2D(4529, 2296, 45, 112)
        };

        public SeaMarketRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override void OnRegister()
        {
            if (_Region1 == null)
            {
                _Region1 = this;
            }
            else if (_Region2 == null)
            {
                _Region2 = this;
            }
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            switch (type)
            {
                case TravelCheckType.RecallTo:
                case TravelCheckType.GateTo:
                    {
                        return BaseBoat.FindBoatAt(p, Map) != null;
                    }
                case TravelCheckType.Mark:
                    {
                        return false;
                    }
            }

            return base.CheckTravel(traveller, p, type);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        #region Pirate Blabbing
        public static Dictionary<Mobile, DateTime> m_PirateBlabTable = new Dictionary<Mobile, DateTime>();
        private static readonly TimeSpan _BlabDuration = TimeSpan.FromMinutes(1);

        public static void TryPirateBlab(Mobile from, Mobile npc)
        {
            if (m_PirateBlabTable.ContainsKey(from) && m_PirateBlabTable[from] > DateTime.UtcNow || BountyQuestSpawner.Bounties.Count <= 0)
            {
                return;
            }

            //Make of list of bounties on their map
            List<Mobile> bounties = new List<Mobile>();
            foreach (Mobile mob in BountyQuestSpawner.Bounties.Keys)
            {
                if (mob.Map == from.Map && mob is PirateCaptain && !bounties.Contains(mob))
                    bounties.Add(mob);
            }

            if (bounties.Count > 0)
            {
                Mobile bounty = bounties[Utility.Random(bounties.Count)];

                if (bounty != null)
                {
                    PirateCaptain capt = (PirateCaptain)bounty;

                    int xLong = 0, yLat = 0;
                    int xMins = 0, yMins = 0;
                    bool xEast = false, ySouth = false;
                    Point3D loc = capt.Location;
                    Map map = capt.Map;

                    string locArgs;

                    if (Sextant.Format(loc, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
                    {
                        locArgs = $"{yLat}°{yMins}'{(ySouth ? "S" : "N")},{xLong}°{xMins}'{(xEast ? "E" : "W")}";
                    }
                    else
                    {
                        locArgs = "?????";
                    }

                    string combine = $"{(capt.PirateName > -1 ? $"#{capt.PirateName}" : capt.Name)}\t{locArgs}";

                    int cliloc = Utility.RandomMinMax(1149856, 1149865);
                    npc.SayTo(from, cliloc, combine);

                    m_PirateBlabTable[from] = DateTime.UtcNow + _BlabDuration;
                }
            }

            ColUtility.Free(bounties);
        }

        public static void CheckBlab_Callback()
        {
            CheckBabble(_Region1);
            CheckBabble(_Region2);
            CheckBabble(TokunoDocksRegion.Instance);
        }

        public static void CheckBabble(Region r)
        {
            if (r == null)
            {
                return;
            }

            foreach (Mobile player in r.GetEnumeratedMobiles())
            {
                if (player is PlayerMobile && player.Alive)
                {
                    IPooledEnumerable eable = player.GetMobilesInRange(4);

                    foreach (Mobile mob in eable)
                    {
                        if (mob is BaseVendor || mob is GalleonPilot)
                        {
                            TryPirateBlab(player, mob);
                            break;
                        }
                    }

                    eable.Free();
                }
            }
        }
        #endregion

        #region Boat Restriction
        public void StartTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
            }

            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        public void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
            }

            m_Timer = null;
        }

        public List<BaseBoat> GetBoats()
        {
            List<BaseBoat> list = new List<BaseBoat>();

            foreach (BaseMulti multi in GetEnumeratedMultis())
            {
                if (multi is BaseBoat boat)
                {
                    list.Add(boat);
                }
            }

            return list;
        }

        public void OnTick()
        {
            if (!m_RestrictBoats)
            {
                StopTimer();
                return;
            }

            List<BaseBoat> boats = GetBoats();
            List<BaseBoat> toRemove = new List<BaseBoat>();

            foreach (KeyValuePair<BaseBoat, DateTime> kvp in m_BoatTable)
            {
                BaseBoat boat = kvp.Key;
                DateTime moveBy = kvp.Value;

                if (boat == null || !boats.Contains(boat) || boat.Deleted)
                {
                    toRemove.Add(boat);
                }
                else if (DateTime.UtcNow >= moveBy && KickBoat(boat))
                {
                    toRemove.Add(boat);
                }
                else
                {
                    if (boat.Owner != null && boat.Owner.NetState != null)
                    {
                        TimeSpan ts = moveBy - DateTime.UtcNow;

                        if ((int)ts.TotalMinutes <= 10)
                        {
                            int rem = Math.Max(1, (int)ts.TotalMinutes);
                            boat.Owner.SendLocalizedMessage(1149787 + (rem - 1));
                        }
                    }
                }
            }

            for (int index = 0; index < boats.Count; index++)
            {
                BaseBoat boat = boats[index];

                if (!m_BoatTable.ContainsKey(boat) && !boat.IsMoving && boat.Owner != null && boat.Owner.AccessLevel == AccessLevel.Player)
                {
                    AddToTable(boat);
                }
            }

            for (int index = 0; index < toRemove.Count; index++)
            {
                BaseBoat b = toRemove[index];

                m_BoatTable.Remove(b);
            }

            ColUtility.Free(toRemove);
            ColUtility.Free(boats);
        }

        public void AddToTable(BaseBoat boat)
        {
            if (m_BoatTable.ContainsKey(boat))
            {
                return;
            }

            m_BoatTable.Add(boat, DateTime.UtcNow + _KickDuration);

            if (boat.Owner != null && boat.Owner.NetState != null)
            {
                boat.Owner.SendMessage("You can only dock your boat here for {0} minutes.", (int)_KickDuration.TotalMinutes);
            }
        }

        private readonly Rectangle2D[] _KickLocs =
        {
            new Rectangle2D(_Bounds[0].X - 100, _Bounds[0].X - 100, 200 + _Bounds[0].Width, 100),
            new Rectangle2D(_Bounds[0].X - 100, _Bounds[0].Y, 100, _Bounds[0].Height + 100),
            new Rectangle2D(_Bounds[0].X, _Bounds[0].Y + _Bounds[0].Height, _Bounds[0].Width + 100, 100),
            new Rectangle2D(_Bounds[0].X + _Bounds[0].Width, _Bounds[0].Y, 100, _Bounds[0].Height)
        };

        public bool KickBoat(BaseBoat boat)
        {
            if (boat == null || boat.Deleted)
            {
                return false;
            }

            for (int i = 0; i < 25; i++)
            {
                Rectangle2D rec = _KickLocs[Utility.Random(_KickLocs.Length)];

                int x = Utility.RandomMinMax(rec.X, rec.X + rec.Width);
                int y = Utility.RandomMinMax(rec.Y, rec.Y + rec.Height);
                int z = boat.Z;

                Point3D p = new Point3D(x, y, z);

                if (boat.CanFit(p, boat.Map, boat.ItemID))
                {
                    boat.Teleport(x - boat.X, y - boat.Y, z - boat.Z);

                    if (boat.Owner != null && boat.Owner.NetState != null)
                    {
                        boat.SendMessageToAllOnBoard(1149785); //A strong tide comes and carries your boat to deeper water.
                    }

                    return true;
                }
            }

            return false;
        }

        private class InternalTimer : Timer
        {
            private readonly SeaMarketRegion _Region;

            public InternalTimer(SeaMarketRegion reg)
                : base(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
            {
                _Region = reg;
            }

            protected override void OnTick()
            {
                if (_Region != null)
                {
                    _Region.OnTick();
                }
            }
        }

        public static void StartTimers_Callback()
        {
            RestrictBoats = m_RestrictBoats;

            m_BlabTimer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), CheckBlab_Callback);
            m_BlabTimer.Start();
        }
        #endregion

        public static void Save(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(m_RestrictBoats);
        }

        public static void Load(GenericReader reader)
        {
            reader.ReadInt();

            m_RestrictBoats = reader.ReadBool();

            Timer.DelayCall(TimeSpan.FromSeconds(30), StartTimers_Callback);
        }

        public static void SetRestriction_OnCommand(CommandEventArgs e)
        {
            if (m_RestrictBoats)
            {
                RestrictBoats = false;

                e.Mobile.SendMessage("Boat restriction in the sea market is no longer active.");
            }
            else
            {
                RestrictBoats = true;

                e.Mobile.SendMessage("Boat restriction in the sea market is now active.");
            }
        }
    }
}
