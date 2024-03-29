using Server.Engines.Quests;
using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    [CorpseName("an ice wyrm corpse")]
    public class IceWyrm : WhiteWyrm
    {
        public static List<IceWyrm> Instances { get; set; }

        [Constructable]
        public IceWyrm()
        {
            Name = "Ice Wyrm";
            Hue = 2729;
            Body = 180;

            SetResistance(ResistanceType.Cold, 100);

            Timer SelfDeleteTimer = new InternalSelfDeleteTimer(this);

            SelfDeleteTimer.Start();

            Tamable = false;

            if (Instances == null)
            {
                Instances = new List<IceWyrm>();
            }

            Instances.Add(this);
        }

        public static IceWyrm Spawn(Point3D platLoc, Map platMap)
        {
            if (Instances != null && Instances.Count > 0)
                return null;

            IceWyrm creature = new IceWyrm
            {
                Home = platLoc,
                RangeHome = 4
            };
            creature.MoveToWorld(platLoc, platMap);

            return creature;
        }

        public class InternalSelfDeleteTimer : Timer
        {
            private readonly IceWyrm Mare;

            public InternalSelfDeleteTimer(Mobile p) : base(TimeSpan.FromMinutes(10))
            {
                Mare = (IceWyrm)p;
            }
            protected override void OnTick()
            {
                if (Mare.Map != Map.Internal)
                {
                    Mare.Delete();
                    Stop();
                }
            }
        }

        public override void OnDeath(Container c)
        {
            List<DamageStore> rights = GetLootingRights();

            HashSet<Mobile> set = new HashSet<Mobile>();

            for (var index = 0; index < rights.Count; index++)
            {
                var x = rights[index];

                Mobile m = x.m_Mobile;

                if (set.Add(m) && m is PlayerMobile pm && pm.ExploringTheDeepQuest == ExploringTheDeepQuestChain.CusteauPerron)
                {
                    Item item = new IceWyrmScale();

                    if (pm.Backpack == null || !pm.Backpack.TryDropItem(pm, item, false))
                    {
                        pm.BankBox.DropItem(item);
                    }

                    pm.SendLocalizedMessage(1154489); // You received a Quest Item!
                }
            }

            if (Instances != null && Instances.Contains(this))
            {
                Instances.Remove(this);
            }

            base.OnDeath(c);
        }

        public override bool ReacquireOnMovement => true;

        public override int Meat => 0;
		public override int Scales => 0;
        public override int Hides => 0;
		
		public override void GenerateLoot()
        {
			// Kept blank to zero out the loot created by it's base class
        }

        public override void OnAfterDelete()
        {
            Instances.Remove(this);

            base.OnAfterDelete();
        }

        public IceWyrm(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Instances = new List<IceWyrm>();
            Instances.Add(this);

            Timer SelfDeleteTimer = new InternalSelfDeleteTimer(this);
            SelfDeleteTimer.Start();
        }
    }
}
