using Server.Items;
using Server.Network;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Mobiles
{
    public class MyrmidexQueen : BaseCreature
    {
        private DateTime _NextCombo1;
        private DateTime _NextCombo2;
        private DateTime _NextEggThrow;

        private List<BaseCreature> _Spawn;

        public override bool AlwaysMurderer => true;
        public override Poison PoisonImmune => Poison.Parasitic;
        public override Poison HitPoison => Poison.Parasitic;
        public override bool Unprovokable => true;

        [Constructable]
        public MyrmidexQueen()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.15, 0.3)
        {
            Body = 1404;
            Name = "Myrmidex Queen";
            BaseSoundID = 959;

            SetHits(60000);
            SetStr(900, 1000);
            SetDex(535);
            SetInt(1000, 1200);

            SetDamage(18, 24);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Poison, 60);

            SetResistance(ResistanceType.Physical, 80, 90);
            SetResistance(ResistanceType.Fire, 60, 70);
            SetResistance(ResistanceType.Cold, 60, 70);
            SetResistance(ResistanceType.Poison, 80, 90);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.Wrestling, 110, 120);
            SetSkill(SkillName.Tactics, 120, 130);
            SetSkill(SkillName.MagicResist, 120, 130);
            SetSkill(SkillName.Anatomy, 0, 10);

            _NextCombo1 = DateTime.UtcNow;
            _NextCombo2 = DateTime.UtcNow;
            _NextEggThrow = DateTime.UtcNow;

            _Spawn = new List<BaseCreature>();

            Fame = 35000;
            Karma = -35000;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.SuperBoss, 5);
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant == null)
                return;

            if (_NextCombo1 < DateTime.UtcNow && 0.1 > Utility.RandomDouble())
            {
                SpitOoze();
                _NextCombo1 = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(15, 30));
            }

            if (_NextCombo2 < DateTime.UtcNow && 0.1 > Utility.RandomDouble())
            {
                if (0.5 > Utility.RandomDouble())
                    DropRocks();
                else
                    RaiseRocks();

                _NextCombo2 = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 40));
            }

            if (_NextEggThrow < DateTime.UtcNow && 0.1 > Utility.RandomDouble())
            {
                ThrowEggs();
                _NextEggThrow = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(90, 120));
            }
        }

        public void ThrowEggs()
        {
            if (Map == null)
                return;

            int delay = 0;

            for (byte i = 0; i <= 0x7; i++)
            {
                Direction d = (Direction)i;
                int xOffset = 0;
                int yOffset = 0;

                Movement.Movement.Offset(d, ref xOffset, ref yOffset);

                int x = X + (27 * xOffset);
                int y = Y + (27 * yOffset);

                Point3D p = new Point3D(x, y, Map.GetAverageZ(x, y));

                if (!Map.CanFit(p, 16, false, false))
                    continue;

                Timer.DelayCall(TimeSpan.FromSeconds(delay), () =>
                    {
                        Entity e = new Entity(Serial.Zero, p, Map);
                        MovingParticles(e, 4313, 10, 0, false, true, 1371, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);

                        Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(2, 3)), () =>
                            {
                                Type t = Utility.RandomList(typeof(MyrmidexWarrior), typeof(MyrmidexDrone), typeof(MyrmidexLarvae));

                                if (Activator.CreateInstance(t) is BaseCreature bc)
                                {
                                    bc.MoveToWorld(p, Map);
                                    _Spawn.Add(bc);
                                }
                            });

                        delay++;
                    });
            }
        }

        public void SpitOoze()
        {
            if (Map == null)
                return;

            IPooledEnumerable eable = Map.GetMobilesInRange(Location, 7);

            foreach (Mobile m in eable)
            {
                if (m != this && SpellHelper.ValidIndirectTarget(this, m) && CanBeHarmful(m, false))
                {
                    List<OozeItem> list = new List<OozeItem>();

                    OozeItem ooze1 = new OozeItem(this, 40222);
                    ooze1.MoveToWorld(m.Location, m.Map);

                    OozeItem ooze2 = new OozeItem(this, Utility.Random(40214, 2));
                    ooze2.MoveToWorld(new Point3D(m.X - 1, m.Y, m.Z), m.Map);

                    OozeItem ooze3 = new OozeItem(this, Utility.Random(40216, 2));
                    ooze3.MoveToWorld(new Point3D(m.X, m.Y + 1, m.Z), m.Map);

                    OozeItem ooze4 = new OozeItem(this, Utility.Random(40218, 2));
                    ooze4.MoveToWorld(new Point3D(m.X, m.Y - 1, m.Z), m.Map);

                    OozeItem ooze5 = new OozeItem(this, Utility.Random(40220, 2));
                    ooze5.MoveToWorld(new Point3D(m.X + 1, m.Y, m.Z), m.Map);

                    OozeItem ooze6 = new OozeItem(this, 40210);
                    ooze6.MoveToWorld(new Point3D(m.X - 1, m.Y + 1, m.Z), m.Map);

                    OozeItem ooze7 = new OozeItem(this, 40211);
                    ooze7.MoveToWorld(new Point3D(m.X + 1, m.Y + 1, m.Z), m.Map);

                    OozeItem ooze8 = new OozeItem(this, 40212);
                    ooze8.MoveToWorld(new Point3D(m.X - 1, m.Y - 1, m.Z), m.Map);

                    OozeItem ooze9 = new OozeItem(this, 40213);
                    ooze9.MoveToWorld(new Point3D(m.X + 1, m.Y - 1, m.Z), m.Map);

                    Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(20, 30)), () =>
                    {
                        ooze1.Delete(); ooze2.Delete(); ooze3.Delete(); ooze4.Delete(); ooze5.Delete();
                        ooze6.Delete(); ooze7.Delete(); ooze8.Delete(); ooze9.Delete();
                    });

                    ooze1.OnMoveOver(m);
                }
            }

            eable.Free();
        }

        public void DropRocks()
        {
            if (Map == null)
                return;

            IPooledEnumerable eable = Map.GetMobilesInRange(Location, 12);
            List<Mobile> random = new List<Mobile>();

            foreach (Mobile m in eable)
            {
                if (m.Alive && m is PlayerMobile && SpellHelper.ValidIndirectTarget(this, m) && CanBeHarmful(m, false))
                    random.Add(m);
            }

            eable.Free();
            Mobile target = null;

            if (random.Count > 0)
                target = random[Utility.Random(random.Count)];

            if (target != null)
            {
                Entity e = new Entity(Serial.Zero, new Point3D(target.X, target.Y, target.Z + 40), target.Map);

                for (int i = 0; i < 5; i++)
                    Timer.DelayCall(TimeSpan.FromMilliseconds(100 * i), () =>
                        {
                            Effects.SendMovingParticles(e, target, 40136, 3, 60, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);
                        });

                Timer.DelayCall(TimeSpan.FromMilliseconds(250), () =>
                    {
                        Effects.SendLocationEffect(target.Location, Map, 40136, 120);
                        target.PrivateOverheadMessage(MessageType.Regular, 0x21, 1156835, target.NetState); // *Crunch Crunch Crunch* 
                    });

                AOS.Damage(target, this, Utility.RandomMinMax(80, 100), 100, 0, 0, 0, 0);
                target.SendSpeedControl(SpeedControlType.WalkSpeed);

                Timer.DelayCall(TimeSpan.FromSeconds(5), () => target.SendSpeedControl(SpeedControlType.Disable));
            }

            ColUtility.Free(random);
        }

        public void RaiseRocks()
        {
            if (Map == null)
                return;

            IPooledEnumerable eable = Map.GetMobilesInRange(Location, 12);
            List<Mobile> random = new List<Mobile>();

            foreach (Mobile m in eable)
            {
                if (m.Alive && m is PlayerMobile && SpellHelper.ValidIndirectTarget(this, m) && CanBeHarmful(m, false))
                    random.Add(m);
            }

            eable.Free();
            Mobile target = null;

            if (random.Count > 0)
                target = random[Utility.Random(random.Count)];

            if (target != null)
            {
                Direction d = Utility.GetDirection(this, target);
                Rectangle2D r = new Rectangle2D(target.X - 8, target.Y - 2, 17, 5);

                switch (d)
                {
                    case Direction.West:
                        r = new Rectangle2D(X - 24, Y - 2, 20, 5); break;
                    case Direction.North:
                        r = new Rectangle2D(X - 2, Y - 24, 5, 20); break;
                    case Direction.East:
                        r = new Rectangle2D(X + 4, Y - 2, 20, 5); break;
                    case Direction.South:
                        r = new Rectangle2D(X - 4, Y + 4, 20, 5); break;
                }

                for (int x = r.X; x <= r.X + r.Width; x++)
                {
                    for (int y = r.Y; y <= r.Y + r.Height; y++)
                    {
                        if (x > X - 4 && x < X + 4 && y > Y - 4 && y < Y + 4)
                            continue;

                        if (0.75 > Utility.RandomDouble())
                        {
                            int id = Utility.RandomList(2282, 2273, 2277, 40106, 40107, 40108, 40106, 40107, 40108, 40106, 40107, 40108);
                            Effects.SendLocationEffect(new Point3D(x, y, Map.GetAverageZ(x, y)), Map, id, 60);
                        }
                    }
                }

                IPooledEnumerable eable2 = Map.GetMobilesInBounds(r);

                foreach (Mobile m in eable2)
                {
                    if (m.Alive && m is PlayerMobile && SpellHelper.ValidIndirectTarget(this, m) && CanBeHarmful(m, false))
                    {
                        if (m.X > X - 4 && m.X < X + 4 && m.Y > Y - 4 && m.Y < Y + 4)
                            continue;

                        m.Freeze(TimeSpan.FromSeconds(2));
                        BleedAttack.BeginBleed(m, this, false);

                        AOS.Damage(target, this, Utility.RandomMinMax(100, 110), 100, 0, 0, 0, 0);
                        m.PrivateOverheadMessage(MessageType.Regular, 0x21, 1156849, m.NetState); // *Rising columns of rock rip through your flesh and concuss you!*
                    }
                }

                eable2.Free();
            }
        }

        public class OozeItem : Item
        {
            public override int LabelNumber => 1156831;  // Noxious Goo

            public BaseCreature Owner { get; set; }

            public OozeItem(BaseCreature bc, int id)
                : base(id)
            {
                Owner = bc;
                Movable = false;
                Hue = 2966;
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (m != Owner && SpellHelper.ValidIndirectTarget(Owner, m) && Owner.CanBeHarmful(m, false))
                {
                    if (0.60 > Utility.RandomDouble())
                    {
                        m.PrivateOverheadMessage(MessageType.Regular, 0x21, 1156832, m.NetState); // *The noxious goo has poisoned you!*
                        m.Poison = Poison.Parasitic;
                    }
                    else
                        m.PrivateOverheadMessage(MessageType.Regular, 0x21, 1156830, m.NetState); // *You are drenched in a noxious goo!*

                    AOS.Damage(m, Owner, Utility.RandomMinMax(40, 60), 0, 0, 0, 100, 0);
                }

                return true;
            }

            public OozeItem(Serial serial)
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

                Delete();
            }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            c.DropItem(new MyrmidexEggsac(Utility.RandomMinMax(5, 10)));
        }

        public override void Delete()
        {
            base.Delete();

            Timer.DelayCall(TimeSpan.FromSeconds(30), () =>
            {
                ColUtility.ForEach(_Spawn.Where(sp => sp != null && sp.Alive), sp => sp.Kill());
                ColUtility.Free(_Spawn);
            });
        }

        public MyrmidexQueen(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(_Spawn.Count);
            _Spawn.ForEach(writer.Write);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            _Spawn = new List<BaseCreature>();

            int c = reader.ReadInt();
            for (int i = 0; i < c; i++)
            {
                if (reader.ReadMobile() is BaseCreature bc)
                {
                    _Spawn.Add(bc);
                }
            }

            _NextCombo1 = DateTime.UtcNow;
            _NextCombo2 = DateTime.UtcNow;
            _NextEggThrow = DateTime.UtcNow;
        }
    }

    public class IgnisFatalis : BaseCreature
    {
        public override bool AlwaysMurderer => true;

        [Constructable]
        public IgnisFatalis()
            : base(AIType.AI_Spellweaving, FightMode.Closest, 10, 1, 0.15, 0.3)
        {
            Body = 0x105;
            Name = "Ignis Fatalis";
            BaseSoundID = 0x56B;

            SetHits(500);
            SetStr(350, 360);
            SetDex(100, 150);
            SetInt(580, 620);

            SetDamage(15, 22);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Energy, 100);

            SetResistance(ResistanceType.Physical, 40, 50);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 60, 70);
            SetResistance(ResistanceType.Poison, 70, 80);
            SetResistance(ResistanceType.Energy, 100);

            SetSkill(SkillName.Wrestling, 100);
            SetSkill(SkillName.Tactics, 100);
            SetSkill(SkillName.MagicResist, 100);
            SetSkill(SkillName.DetectHidden, 100.0);
            SetSkill(SkillName.Magery, 100.0);
            SetSkill(SkillName.EvalInt, 100.0);
            SetSkill(SkillName.Meditation, 100.0);
            SetSkill(SkillName.Focus, 100.0);
            SetSkill(SkillName.Spellweaving, 100.0);

            SetAreaEffect(AreaEffect.AuraOfEnergy);
        }

        public IgnisFatalis(Serial serial)
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
        }
    }
}
