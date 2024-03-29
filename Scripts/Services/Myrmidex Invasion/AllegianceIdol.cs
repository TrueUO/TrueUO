using Server.Gumps;
using Server.Mobiles;
using System;

namespace Server.Engines.MyrmidexInvasion
{
    public class AllegianceIdol : Item
    {
        private Allegiance _AllegianceType;

        [CommandProperty(AccessLevel.GameMaster)]
        public Allegiance AllegianceType
        {
            get => _AllegianceType;
            set
            {
                _AllegianceType = value;

                if (_AllegianceType == Allegiance.Myrmidex)
                {
                    ItemID = 9730;
                    Hue = 2503;
                }
                else
                {
                    ItemID = 17099;
                    Hue = 0;
                }

                InvalidateProperties();
            }
        }

        [Constructable]
        public AllegianceIdol(Allegiance allegiance) : base(allegiance == Allegiance.Myrmidex ? 9730 : 17099)
        {
            AllegianceType = allegiance;

            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from is PlayerMobile pm && pm.InRange(GetWorldLocation(), 3))
            {
                AllianceEntry entry = MyrmidexInvasionSystem.GetEntry(pm);

                if (entry != null)
                {
                    if (entry.Allegiance == _AllegianceType)
                    {
                        pm.SendLocalizedMessage(1156637, $"#{(int)entry.Allegiance}"); // You have already declared allegiance to the ~1_SIDE~!  You may only change your allegiance once every 2 hours.
                    }
                    else if (entry.JoinTime + TimeSpan.FromHours(2) > DateTime.UtcNow)
                    {
                        pm.SendLocalizedMessage(1156633); // You cannot declare allegiance to that side.
                    }
                    else
                    {
                        pm.SendGump(
                            new ConfirmCallbackGump(pm,
                            (int)_AllegianceType,
                            $"Your current allegiance is with the {(entry.Allegiance == Allegiance.Tribes ? "Eodonians" : "Myrmidex")}.  Select yes to pledge your allegiance to the {(_AllegianceType == Allegiance.Tribes ? "Eodonians" : "Myrmidex")}.",
                            entry,
                            confirm: (m, state) =>
                                {
                                    if (m.Region.IsPartOf<BattleRegion>())
                                    {
                                        m.SendLocalizedMessage(1156632); // You cannot switch allegiance in the midst of the battle field!
                                    }
                                    else
                                    {
                                        MyrmidexInvasionSystem.System.Join((PlayerMobile)from, AllegianceType);
                                    }
                                }));
                    }
                }
                else
                    MyrmidexInvasionSystem.System.Join(pm, _AllegianceType);
            }
            else
                from.SendLocalizedMessage(1149687); //You are too far away.
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1156640, "#1156638"); // ~1_TEAMS~ Allegiance Idol
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1156639); // Double click to declare or check allegiance
        }

        public AllegianceIdol(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)_AllegianceType);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _AllegianceType = (Allegiance)reader.ReadInt();
        }
    }
}
