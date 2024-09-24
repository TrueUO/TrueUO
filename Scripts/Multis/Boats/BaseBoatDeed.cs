using Server.Gumps;
using Server.Items;
using Server.Regions;

namespace Server.Multis
{
    public abstract class BaseBoatDeed : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int MultiID { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Offset { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Direction BoatDirection { get; set; }

        public virtual bool IsRowBoatDeed => false;

        public BaseBoatDeed(int id, Point3D offset)
            : base(0x14F2)
        {
            Weight = 1.0;

            MultiID = id;
            Offset = offset;
            BoatDirection = Direction.North;
        }

        public BaseBoatDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(MultiID);
            writer.Write(Offset);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            MultiID = reader.ReadInt();
            Offset = reader.ReadPoint3D();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                BaseBoat boat = BaseBoat.FindBoatAt(from, from.Map);

                if (from.AccessLevel < AccessLevel.GameMaster && (from.Map == Map.Ilshenar || from.Map == Map.Malas))
                {
                    from.SendLocalizedMessage(1010567, null, 0x25); // You may not place a boat from this location.
                }
                else if (BaseBoat.HasBoat(from) && !Boat.IsRowBoat)
                {
                    from.SendLocalizedMessage(1116758); // You already have a ship deployed!
                }
                else if (from.Region.IsPartOf(typeof(HouseRegion)) || boat != null && (boat.GetType() == Boat.GetType() || !boat.IsRowBoat && !IsRowBoatDeed))
                {
                    from.SendLocalizedMessage(1010568, null, 0x25); // You may not place a ship while on another ship or inside a house.
                }
                else if (!from.HasGump(typeof(BoatPlacementGump)))
                {
                    from.SendLocalizedMessage(502482); // Where do you wish to place the ship?

                    from.SendGump(new BoatPlacementGump(this, from));
                }
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        public abstract BaseBoat Boat { get; }

        public void OnPlacement(Mobile from, Point3D p, int itemID, Direction d)
        {
            if (Deleted)
            {
                return;
            }

            Map map = from.Map;

            if (map == null)
                return;

            if (from.AccessLevel < AccessLevel.GameMaster && (map == Map.Ilshenar || map == Map.Malas))
            {
                from.SendLocalizedMessage(1043284); // A ship can not be created here.
                return;
            }

            BaseBoat b = BaseBoat.FindBoatAt(from, from.Map);

            if (from.Region.IsPartOf(typeof(HouseRegion)) || b != null && (b.GetType() == Boat.GetType() || !b.IsRowBoat && !IsRowBoatDeed))
            {
                from.SendLocalizedMessage(1010568, null, 0x25); // You may not place a ship while on another ship or inside a house.
                return;
            }

            BoatDirection = d;
            BaseBoat boat = Boat;

            if (boat == null)
                return;

            p = new Point3D(p.X - Offset.X, p.Y - Offset.Y, p.Z - Offset.Z);

            if (BaseBoat.IsValidLocation(p, map) && boat.CanFit(p, map, itemID))
            {
                if (boat.IsRowBoat)
                {
                    BaseBoat lastRowBoat = null;
                    foreach (var item in World.Items.Values)
                    {
                        if (item is BaseBoat baseBoat && baseBoat.Owner == from && baseBoat.IsRowBoat && baseBoat.Map != Map.Internal)
                        {
                            bool hasNoMobilesOnBoard = true;

                            // Check if the boat has any mobiles on board
                            foreach (Mobile _ in baseBoat.MobilesOnBoard)
                            {
                                hasNoMobilesOnBoard = false;
                                break;
                            }

                            if (hasNoMobilesOnBoard)
                            {
                                // Check if this is the boat with the maximum Serial
                                if (lastRowBoat == null || baseBoat.Serial > lastRowBoat.Serial)
                                {
                                    lastRowBoat = baseBoat;
                                }
                            }
                        }
                    }

                    if (lastRowBoat != null)
                    {
                        lastRowBoat.Delete();
                    }
                }
                else
                {
                    Delete();
                }

                boat.Owner = from;
                boat.ItemID = itemID;

                if (boat is BaseGalleon galleon)
                {
                    galleon.SecurityEntry = new SecurityEntry(galleon);
                    galleon.BaseBoatHue = RandomBasePaintHue();
                }

                if (boat.IsClassicBoat)
                {
                    uint keyValue = boat.CreateKeys(from);

                    if (boat.PPlank != null)
                        boat.PPlank.KeyValue = keyValue;

                    if (boat.SPlank != null)
                        boat.SPlank.KeyValue = keyValue;
                }

                boat.MoveToWorld(p, map);
                boat.OnAfterPlacement(true);

                LighthouseAddon addon = LighthouseAddon.GetLighthouse(from);

                if (addon != null)
                {
                    if (boat.CanLinkToLighthouse)
                        from.SendLocalizedMessage(1154592); // You have linked your boat lighthouse.
                    else
                        from.SendLocalizedMessage(1154597); // Failed to link to lighthouse.
                }
            }
            else
            {
                boat.Delete();
                from.SendLocalizedMessage(1043284); // A ship can not be created here.
            }
        }

        private int RandomBasePaintHue()
        {
            if (0.6 > Utility.RandomDouble())
            {
                return Utility.RandomMinMax(1701, 1754);
            }

            return Utility.RandomMinMax(1801, 1908);
        }
    }
}
