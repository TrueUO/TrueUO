using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests.RitualQuest
{
    public class Prugyilonus : MondainQuester
    {
        public static Prugyilonus Instance { get; set; }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new Prugyilonus();
                Instance.MoveToWorld(new Point3D(750, 3344, 61), Map.TerMur);
            }
        }

        public override Type[] Quests => new[] { typeof(ScalesOfADreamSerpentQuest) };

        public Prugyilonus()
            : base("Prugyilonus", "the Advisor to the Queen")
        {
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);
            Race = Race.Gargoyle;

            CantWalk = true;

            Hue = 34547;
            HairItemID = Race.RandomHair(false);
            HairHue = Race.RandomHairHue();
        }

        public override void InitOutfit()
        {
            SetWearable(new GargishFancyRobe(), 1345);
        }

        public Prugyilonus(Serial serial) : base(serial)
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
            reader.ReadInt(); // version

            Instance = this;
        }
    }

    public class Linzrom : MondainQuester
    {
        public static Linzrom Instance { get; set; }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new Linzrom();
                Instance.MoveToWorld(new Point3D(740, 3350, 55), Map.TerMur);
            }
        }

        public override Type[] Quests => new[] { typeof(ScalesOfADreamSerpentQuest) };

        public Linzrom()
            : base("Linzrom", "the Guard Captain")
        {
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);
            Race = Race.Gargoyle;

            CantWalk = true;

            Hue = 34529;
            HairItemID = Race.RandomHair(false);
            HairHue = Race.RandomHairHue();
        }

        public override void InitOutfit()
        {
            SetWearable(new GargishPlateChest(), 2501);
            SetWearable(new CrescentBlade(), 2501);
        }

        public override void OnTalk(PlayerMobile player)
        {
            if (QuestHelper.GetQuest(player, typeof(PristineCrystalLotusQuest)) is PristineCrystalLotusQuest q && q.Completed)
            {
                base.OnTalk(player);
            }
            else
            {
                SayTo(player, 1080107, 0x3B2); // I'm sorry, I have nothing for you at this time.
            }
        }

        public Linzrom(Serial serial)
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

            Instance = this;
        }
    }

    public class Bexil : MondainQuester
    {
        public override Type[] Quests => new[] { typeof(CatchMeIfYouCanQuest) };

        public static Bexil Instance { get; set; }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new Bexil();
                Instance.MoveToWorld(new Point3D(662, 3819, -43), Map.TerMur);
            }
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m.Backpack.GetAmount(typeof(DreamSerpentScale)) == 0)
            {
                base.OnDoubleClick(m);
            }
            else
            {
                SayTo(m, 1151355, 0x3B2); // You may not obtain more than one of this item.
                SayTo(m, 1080107, 0x3B2); // I'm sorry, I have nothing for you at this time.
            }
        }

        public Bexil()
            : base("Bexil", "the Dream Serpent")
        {
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);
            Body = 0xCE;
            Hue = 2069;

            CantWalk = true;
        }

        public Bexil(Serial serial) : base(serial)
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
            reader.ReadInt(); // version

            Instance = this;
        }
    }

    public class BexilPunchingBag : BaseCreature
    {
        public override bool InitialInnocent => true;

        private readonly Dictionary<Mobile, int> _Table = new Dictionary<Mobile, int>();
        private DateTime _NextTeleport;

        public BexilPunchingBag()
            : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.4, 0.8)
        {
            Name = "Bexil";
            Title = "the Dream Serpent";

            Body = 0xCE;
            Hue = 1976;
            BaseSoundID = 0x5A;

            SetHits(1000000);
        }

        private IDamageable _Combatant;

        public override IDamageable Combatant
        {
            get => _Combatant;
            set => _Combatant = value;
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant is Mobile && _NextTeleport < DateTime.UtcNow)
            {
                Map map = Map;

                Point3D p;

                do
                {
                    int x = X + Utility.RandomMinMax(-10, 10);
                    int y = Y + Utility.RandomMinMax(-10, 10);

                    p = new Point3D(x, y, map.GetAverageZ(x, y));
                }
                while (!map.CanSpawnMobile(p.X, p.Y, map.GetAverageZ(p.X, p.Y)) || !Region.Find(p, map).IsPartOf<CatchMeIfYouCanQuest.BexilRegion>());

                Effects.SendLocationParticles(EffectItem.Create(Location, map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                MoveToWorld(p, map);
                Effects.SendLocationParticles(EffectItem.Create(Location, map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);

                _NextTeleport = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(1, 3));
            }
        }

        public override int Damage(int amount, Mobile from, bool informMount, bool checkDisrupt)
        {
            if (from is PlayerMobile mobile)
            {
                CatchMeIfYouCanQuest quest = QuestHelper.GetQuest<CatchMeIfYouCanQuest>(mobile);

                if (quest != null)
                {
                    quest.Objectives[0].Update(this);

                    if (quest.Completed)
                    {
                        DreamSerpentCharm.CompleteQuest(mobile);
                    }
                }
            }

            return 0;
        }

        public override void Delete()
        {
            BexilPunchingBag bex = new BexilPunchingBag();
            bex.MoveToWorld(new Point3D(403, 3391, 38), Map.TerMur);

            base.Delete();
        }

        public BexilPunchingBag(Serial serial) : base(serial)
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
            reader.ReadInt(); // version
        }
    }

    public class Grubbix : MondainQuester
    {
        public override Type[] Quests => new[] { typeof(FilthyLifeStealersQuest) };

        public static Grubbix Instance { get; set; }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new Grubbix();
                Instance.MoveToWorld(new Point3D(1106, 3138, -43), Map.TerMur);
            }
        }

        public Grubbix()
            : base("Grubbix", "the Soulbinder")
        {
        }

        public override void InitBody()
        {
            CantWalk = true;

            InitStats(100, 100, 25);
            Body = 0x100;
            Hue = 2076;
        }

        public Grubbix(Serial serial) : base(serial)
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
            reader.ReadInt(); // version

            Instance = this;
        }
    }

    public class QueenOakwhisper : BaseQuester
    {
        public static QueenOakwhisper Instance { get; set; }

        public static void Initialize()
        {
            var map = Map.TerMur;

            if (Instance == null)
            {
                Instance = new QueenOakwhisper();
                Instance.MoveToWorld(new Point3D(846, 2773, 0), map);
            }

            if (map.FindItem<Teleporter>(new Point3D(856, 2783, 5)) == null)
            {
                var tele = new Teleporter
                {
                    PointDest = new Point3D(4446, 3692, 0),
                    MapDest = Map.Trammel
                };
                tele.MoveToWorld(new Point3D(856, 2783, 5), map);
            }

            if (map.FindItem<Static>(new Point3D(856, 2783, 5)) == null)
            {
                var st = new Static(0x375A)
                {
                    Weight = 0
                };
                st.MoveToWorld(new Point3D(856, 2783, 5), map);
            }
        }

        public override void OnTalk(PlayerMobile player, bool contextMenu)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            PlayerMobile pm = (PlayerMobile) m;

            if (m.AccessLevel > AccessLevel.Player || QuestHelper.HasQuest(pm, typeof(HairOfTheDryadQueen)))
            {
                var quest = QuestHelper.GetQuest<KnowledgeOfNature>(pm);

                if (quest != null && quest.Completed)
                {
                    SayTo(m, 1151355, 0x3B2); // You may not obtain more than one of this item.
                    SayTo(m, 1080107, 0x3B2); // I'm sorry, I have nothing for you at this time.
                }

                if (quest == null)
                {
                    quest = new KnowledgeOfNature
                    {
                        Owner = pm,
                        Quester = this
                    };

                    pm.SendGump(new MondainQuestGump(quest));
                }
                else if (quest.Completed)
                {
                    pm.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
                }
                else if (!pm.HasGump(typeof(QAndAGump)))
                {
                    pm.SendGump(new QAndAGump(pm, quest));
                }
            }
        }

        public QueenOakwhisper()
        {
            Name = "Queen Oakwhisper";
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);
            Body = 0x2D8;
            Hue = 2655;
            CantWalk = true;
        }

        public override int GetIdleSound()
        {
            return 1404;
        }

        public QueenOakwhisper(Serial serial)
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
            reader.ReadInt(); // version

            Instance = this;
        }
    }
}
