using Server.Commands;
using Server.Items;

namespace Server.Engines.ExploringTheDeep
{
    public static class GenerateExploringTheDeep
    {
        public static readonly string EntityName = "exploringthedeep";

        public static void Initialize()
        {
            CommandSystem.Register("GenExploringTheDeep", AccessLevel.Administrator, Generate);
            CommandSystem.Register("DelExploringTheDeep", AccessLevel.Administrator, Delete);
        }

        public static void Generate(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            Delete(e);
            Generate(m);

            m.SendMessage("Exploring the Deep Quest Line Generated!");
        }

        public static void Delete(CommandEventArgs e)
        {
            WeakEntityCollection.Delete(EntityName);
            WeakEntityCollection.Delete(WinchAssembly.EntityName);

            SpawnerPersistence.RemoveSpawnsFromXmlFile("Spawns", "GravewaterLake");
        }

        public static void Generate(Mobile m)
        {
            #region Gravewater Lake Finish

            CommandSystem.Handle(m, CommandSystem.Prefix + "XmlLoad Spawns/GravewaterLake.xml");

            CommandSystem.Handle(m, CommandSystem.Prefix + "GenWinchAssembly");

            // StorageLocker

            StorageLocker storagelocker = new StorageLocker(Parts.Flywheel);
            WeakEntityCollection.Add(EntityName, storagelocker);
            storagelocker.MoveToWorld(new Point3D(6421, 1753, 0), Map.Trammel);
            storagelocker.Active = true;

            storagelocker = new StorageLocker(Parts.BearingAssembly);
            WeakEntityCollection.Add(EntityName, storagelocker);
            storagelocker.MoveToWorld(new Point3D(6441, 1753, 0), Map.Trammel);
            storagelocker.Active = true;

            storagelocker = new StorageLocker(Parts.PowerCore);
            WeakEntityCollection.Add(EntityName, storagelocker);
            storagelocker.MoveToWorld(new Point3D(6441, 1733, 0), Map.Trammel);
            storagelocker.Active = true;

            storagelocker = new StorageLocker(Parts.WireSpool);
            WeakEntityCollection.Add(EntityName, storagelocker);
            storagelocker.MoveToWorld(new Point3D(6421, 1733, 0), Map.Trammel);
            storagelocker.Active = true;

            Item door = new LightWoodDoor(DoorFacing.SouthCW);
            WeakEntityCollection.Add(EntityName, door);
            door.Hue = 2952;
            door.MoveToWorld(new Point3D(6427, 1735, 0), Map.Trammel);

            door = new LightWoodDoor(DoorFacing.SouthCW);
            WeakEntityCollection.Add(EntityName, door);
            door.Hue = 2952;
            door.MoveToWorld(new Point3D(6427, 1752, 0), Map.Trammel);

            door = new LightWoodDoor(DoorFacing.SouthCCW);
            WeakEntityCollection.Add(EntityName, door);
            door.Hue = 2952;
            door.MoveToWorld(new Point3D(6435, 1735, 0), Map.Trammel);

            door = new LightWoodDoor(DoorFacing.SouthCCW);
            WeakEntityCollection.Add(EntityName, door);
            door.Hue = 2952;
            door.MoveToWorld(new Point3D(6435, 1752, 0), Map.Trammel);

            door = new LightWoodDoor(DoorFacing.WestCW);
            WeakEntityCollection.Add(EntityName, door);
            door.Hue = 2952;
            door.MoveToWorld(new Point3D(6431, 1727, 0), Map.Trammel);

            door = new LightWoodDoor(DoorFacing.EastCCW);
            WeakEntityCollection.Add(EntityName, door);
            door.Hue = 2952;
            door.MoveToWorld(new Point3D(6432, 1727, 0), Map.Trammel);

            Static decor = new Static(0x1EAF);
            WeakEntityCollection.Add(EntityName, decor);
            decor.MoveToWorld(new Point3D(6310, 1704, 11), Map.Trammel);

            decor = new Static(0x1ED5);
            WeakEntityCollection.Add(EntityName, decor);
            decor.MoveToWorld(new Point3D(6310, 1705, -5), Map.Trammel);

            decor = new Static(0x10A4);
            decor.MoveToWorld(new Point3D(6310, 1703, 8), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new Static(0x2E3D);
            decor.MoveToWorld(new Point3D(6311, 1703, 19), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new Static(0x3A8);
            decor.MoveToWorld(new Point3D(6309, 1704, 20), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new Static(0x3A8);
            decor.MoveToWorld(new Point3D(6310, 1704, 20), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new Static(0x3A6);
            decor.MoveToWorld(new Point3D(6309, 1703, 24), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new Static(0x3A6);
            decor.MoveToWorld(new Point3D(6310, 1703, 24), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            Item ladder = new ShipLadder(new Point3D(6302, 1672, 0), Map.Trammel, 0x08A6);
            ladder.MoveToWorld(new Point3D(6431, 1699, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, ladder);

            ladder = new ShipLadder(new Point3D(6432, 1699, 0), Map.Trammel, 0x08A6);
            ladder.MoveToWorld(new Point3D(6304, 1672, -5), Map.Trammel);
            WeakEntityCollection.Add(EntityName, ladder);

            ladder = new ShipLadder(new Point3D(1699, 1646, -115), Map.Malas, 0x14FA);
            ladder.MoveToWorld(new Point3D(6278, 1773, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, ladder);

            Item sign = new ShipSign(0xBD2, 1154461); // Use Ladder to Return to Foredeck
            sign.MoveToWorld(new Point3D(6400, 1658, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, sign);

            sign = new ShipSign(0xBCF, 1154492); // Use the rope to return to the surface
            sign.MoveToWorld(new Point3D(6278, 1773, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, sign);

            sign = new ShipSign(0xBD1, 1154463); // Warning! Only those with proper gear may enter the lake for salvage operations! Enter at your own risk! No Pets!
            sign.MoveToWorld(new Point3D(1698, 1566, -110), Map.Malas);
            WeakEntityCollection.Add(EntityName, sign);

            Item tele = new Teleporter(new Point3D(6445, 1743, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(6321, 1710, -35), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Teleporter(new Point3D(6445, 1743, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(6321, 1711, -35), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Teleporter(new Point3D(6322, 1710, -35), Map.Trammel);
            tele.MoveToWorld(new Point3D(6447, 1741, 1), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Teleporter(new Point3D(6322, 1710, -35), Map.Trammel);
            tele.MoveToWorld(new Point3D(6447, 1742, 1), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Teleporter(new Point3D(6322, 1710, -35), Map.Trammel);
            tele.MoveToWorld(new Point3D(6447, 1743, 1), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Teleporter(new Point3D(6322, 1710, -35), Map.Trammel);
            tele.MoveToWorld(new Point3D(6447, 1744, 1), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Teleporter(new Point3D(6322, 1710, -35), Map.Trammel);
            tele.MoveToWorld(new Point3D(6447, 1745, 1), Map.Trammel);
            WeakEntityCollection.Add(EntityName, tele);

            tele = new Whirlpool(new Point3D(6274, 1787, 0), Map.Trammel);
            tele.MoveToWorld(new Point3D(1700, 1638, -115), Map.Malas);
            WeakEntityCollection.Add(EntityName, tele);

            Item item = new AnkhWest();
            item.MoveToWorld(new Point3D(1694, 1562, -109), Map.Malas);
            WeakEntityCollection.Add(EntityName, item);

            item = new DungeonHitchingPost();
            item.MoveToWorld(new Point3D(1702, 1552, -109), Map.Malas);
            WeakEntityCollection.Add(EntityName, item);

            #endregion
        }
    }
}
