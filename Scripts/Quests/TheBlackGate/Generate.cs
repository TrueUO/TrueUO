using Server.Commands;
using Server.Items;

namespace Server.Engines.Quests
{
    public static class GenerateTheBlackGate
    {
        public static readonly string EntityName = "theblackgate";

        public static void Initialize()
        {
            CommandSystem.Register("GenBlackGate", AccessLevel.Administrator, Generate);
            CommandSystem.Register("DelBlackGate", AccessLevel.Administrator, Delete);
        }

        public static void Generate(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            Delete(e);
            Generate(m);

            m.SendMessage("The Black Gate Quest Generated!");
        }

        public static void Generate(Mobile m)
        {
            Mobile quester = new Julia();
            quester.MoveToWorld(new Point3D(2572, 526, 15), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Katrina();
            quester.MoveToWorld(new Point3D(3711, 2249, 20), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Shamino();
            quester.MoveToWorld(new Point3D(742, 2163, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Dupre();
            quester.MoveToWorld(new Point3D(1932, 2790, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Jaana();
            quester.MoveToWorld(new Point3D(339, 873, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Geoffrey();
            quester.MoveToWorld(new Point3D(1434, 3878, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Mariah();
            quester.MoveToWorld(new Point3D(4385, 1088, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            quester = new Iolo();
            quester.MoveToWorld(new Point3D(1395, 1810, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, quester);

            Item decor = new SpilledBlood();
            decor.MoveToWorld(new Point3D(2698, 463, 15), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new RolledParchment();
            decor.MoveToWorld(new Point3D(612, 872, 0), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new BonesOfAFallenRanger();
            decor.MoveToWorld(new Point3D(6064, 92, 22), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);

            decor = new RawGinsengDecoration();
            decor.MoveToWorld(new Point3D(3386, 319, 4), Map.Trammel);
            WeakEntityCollection.Add(EntityName, decor);
        }

        public static void Delete(CommandEventArgs e)
        {
            WeakEntityCollection.Delete(EntityName);
        }
    }
}
