using System;
using System.Collections.Generic;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a medusa corpse")]
    public class Medusa : BaseSABoss, ICarvable
    {
        private const int InitialStatueAmount = 12;
        private int m_ScalesLeft = 15;

        private List<BaseCreature> m_Statues;
        private DateTime m_NextCarve;
        private DateTime m_GazeDelay;

        public override Type[] UniqueSAList => new[] { typeof(Slither), typeof(IronwoodCompositeBow), typeof(Venom), typeof(PetrifiedSnake), typeof(StoneDragonsTooth), typeof(MedusaFloorTileAddonDeed), typeof(EternalGuardianStaff) };

        public override Type[] SharedSAList => new Type[] { };

        [Constructable]
        public Medusa()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.175, 0.350)
        {
            Name = "Medusa";
            Body = 728;

            SetStr(1235, 1391);
            SetDex(128, 139);
            SetInt(537, 664);

            SetHits(60000);

            SetDamage(21, 28);

            SetDamageType(ResistanceType.Physical, 60);
            SetDamageType(ResistanceType.Fire, 20);
            SetDamageType(ResistanceType.Energy, 20);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 55, 65);
            SetResistance(ResistanceType.Cold, 55, 65);
            SetResistance(ResistanceType.Poison, 80, 90);
            SetResistance(ResistanceType.Energy, 60, 75);

            SetSkill(SkillName.Anatomy, 111.5, 117.9);
            SetSkill(SkillName.EvalInt, 103.1, 128.5);
            SetSkill(SkillName.Magery, 114.7, 120.8);
            SetSkill(SkillName.Meditation, 100.0);
            SetSkill(SkillName.MagicResist, 120.0);
            SetSkill(SkillName.Tactics, 124.8, 135.5);
            SetSkill(SkillName.Wrestling, 119.7, 128.9);
            SetSkill(SkillName.Poisoning, 10.0, 30.0);
            SetSkill(SkillName.DetectHidden, 100.0);

            Fame = 22000;
            Karma = -22000;

            var bow = new Bow
            {
                Attributes =
                {
                    SpellChanneling = 1,
                    CastSpeed = 1
                },

                LootType = LootType.Blessed
            };

            AddItem(bow);

            PackItem(new Arrow(Utility.RandomMinMax(125, 175)));

            m_Statues = new List<BaseCreature>();

            SetWeaponAbility(WeaponAbility.MortalStrike);
        }

        public Medusa(Serial serial)
            : base(serial)
        {
        }

        public override int GetAttackSound() { return 0x612; }
        public override int GetDeathSound() { return 0x613; }
        public override int GetHurtSound() { return 0x614; }
        public override int GetIdleSound() { return 0x615; }

        #region Carve Scales
        public bool Carve(Mobile from, Item item)
        {
            if (m_ScalesLeft > 0)
            {
                if (DateTime.UtcNow < m_NextCarve)
                {
                    from.SendLocalizedMessage(1112677); // The creature is still recovering from the previous harvest. Try again in a few seconds.
                }
                else
                {
                    from.RevealingAction();

                    if (0.2 > Utility.RandomDouble())
                    {
                        int amount = Math.Min(m_ScalesLeft, Utility.RandomMinMax(2, 3));

                        m_ScalesLeft -= amount;

                        Item scales = new MedusaLightScales(amount);

                        if (from.PlaceInBackpack(scales))
                        {
                            from.SendLocalizedMessage(1112676); // You harvest magical resources from the creature and place it in your bag.
                        }
                        else
                        {
                            scales.MoveToWorld(from.Location, from.Map);
                        }

                        m_NextCarve = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);
                        return true;
                    }

                    from.SendLocalizedMessage(1112675, "", 33); // Your attempt fails and angers the creature!!

                    PlaySound(GetHurtSound());

                    Combatant = from;
                }
            }
            else
            {
                from.SendLocalizedMessage(1112674); // There's nothing left to harvest from this creature.                
            }

            return false;
        }
        #endregion

        #region Statues
        private static readonly Type[] m_StatueTypes =
        {
            typeof(OphidianArchmage), typeof(OphidianWarrior), typeof(WailingBanshee),
            typeof(OgreLord), typeof(Dragon), typeof(UndeadGargoyle)
        };

        private BaseCreature CreateStatue()
        {
            try
            {
                BaseCreature bc = (BaseCreature)Activator.CreateInstance(m_StatueTypes[Utility.Random(m_StatueTypes.Length)]);

                bc.Frozen = true;
                bc.Blessed = true;
                bc.HueMod = 2401;
                bc.Tamable = false;
                bc.Direction = (Direction)Utility.Random(8);

                return bc;
            }
            catch
            {
                return null;
            }
        }

        public override void OnBeforeSpawn(Point3D location, Map m)
        {
            base.OnBeforeSpawn(location, m);

            for (int i = 0; i < InitialStatueAmount; i++)
            {
                BaseCreature statue = CreateStatue();

                if (statue != null)
                {
                    Point3D loc = m.GetSpawnPosition(location, 40);

                    statue.MoveToWorld(loc, m);
                    statue.OnBeforeSpawn(loc, m);

                    m_Statues.Add(statue);
                }
            }
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            for (int i = 0; i < m_Statues.Count; i++)
            {
                BaseCreature bc = m_Statues[i];

                if (!bc.Deleted)
                {
                    bc.Delete();
                }
            }
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            base.OnDamage(amount, from, willKill);

            if (0.1 > Utility.RandomDouble() && m_Statues.Count > 0)
            {
                BaseCreature bc = m_Statues[0];

                m_Statues.RemoveAt(0);

                if (bc != null && !bc.Deleted)
                {
                    PublicOverheadMessage(MessageType.Regular, 33, 1112767); // Medusa releases one of the petrified creatures!!

                    bc.Frozen = false;
                    bc.Blessed = false;
                    bc.HueMod = -1;
                }
            }
        }
        #endregion

        #region Replicas
        private EvilReplica BuildReplica(Mobile m)
        {
            if (m is BaseCreature bc)
            {
                return new PetReplica(bc);
            }

            return new PlayerReplica(m);
        }

        private void CreateReplica(Mobile m)
        {
            EvilReplica replica = BuildReplica(m);

            replica.OnBeforeSpawn(m.Location, m.Map);
            replica.MoveToWorld(m.Location, m.Map);

            if (m is BaseCreature pet)
            {
                Mobile master = pet.Summoned ? pet.SummonMaster : pet.ControlMaster;

                if (master != null)
                {
                    master.SendLocalizedMessage(1113285, "", 42); // Beware! A statue of your pet has been created!
                }
            }

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(replica.Unpetrify));
        }
        #endregion

        #region Petrification
        private bool CheckBlockGaze(Mobile m)
        {
            if (m == null)
                return false;

            Item helm = m.FindItemOnLayer(Layer.Helm);
            Item neck = m.FindItemOnLayer(Layer.Neck);
            Item ear = m.FindItemOnLayer(Layer.Earrings);
            Item shi = m.FindItemOnLayer(Layer.TwoHanded);

            bool deflect = false;
            int perc = 0;
            bool fail = false;

            if (helm != null)
            {
                if (helm is BaseArmor armor && armor.GorgonLenseCharges > 0)
                {
                    perc = GetScaleEffectiveness(armor.GorgonLenseType);

                    if (perc > Utility.Random(100))
                    {
                        armor.GorgonLenseCharges--;
                        deflect = true;
                    }
                    else
                    {
                        fail = true;
                    }
                }
                else if (helm is BaseClothing clothing && clothing.GorgonLenseCharges > 0)
                {
                    perc = GetScaleEffectiveness(clothing.GorgonLenseType);

                    if (perc > Utility.Random(100))
                    {
                        clothing.GorgonLenseCharges--;
                        deflect = true;
                    }
                    else
                    {
                        fail = true;
                    }
                }
            }

            if (!deflect && shi != null && shi is BaseShield shield && shield.GorgonLenseCharges > 0)
            {
                perc = GetScaleEffectiveness(shield.GorgonLenseType);

                if (perc > Utility.Random(100))
                {
                    shield.GorgonLenseCharges--;
                    deflect = true;
                }
                else
                {
                    fail = true;
                }
            }

            if (!deflect && neck != null)
            {
                if (neck is BaseArmor armor && armor.GorgonLenseCharges > 0)
                {
                    perc = GetScaleEffectiveness(armor.GorgonLenseType);

                    if (perc > Utility.Random(100))
                    {
                        armor.GorgonLenseCharges--;
                        deflect = true;
                    }
                    else
                    {
                        fail = true;
                    }
                }
                else if (neck is BaseJewel jewel && jewel.GorgonLenseCharges > 0)
                {
                    perc = GetScaleEffectiveness(jewel.GorgonLenseType);

                    if (perc > Utility.Random(100))
                    {
                        jewel.GorgonLenseCharges--;
                        deflect = true;
                    }
                    else
                    {
                        fail = true;
                    }
                }
                else if (neck is BaseClothing clothing && clothing.GorgonLenseCharges > 0)
                {
                    perc = GetScaleEffectiveness(clothing.GorgonLenseType);

                    if (perc > Utility.Random(100))
                    {
                        clothing.GorgonLenseCharges--;
                        deflect = true;
                    }
                    else
                    {
                        fail = true;
                    }
                }
            }

            if (!deflect && ear != null)
            {
                if (ear is BaseJewel jewel && jewel.GorgonLenseCharges > 0)
                {
                    perc = GetScaleEffectiveness(jewel.GorgonLenseType);

                    if (perc > Utility.Random(100))
                    {
                        jewel.GorgonLenseCharges--;
                        deflect = true;
                    }
                    else
                    {
                        fail = true;
                    }
                }
            }

            if (deflect)
            {
                m.SendLocalizedMessage(1112599); // Your Gorgon Lens deflect Medusa's petrifying gaze!
            }

            if (fail)
            {
                m.SendLocalizedMessage(1112621); // Your lenses fail to deflect Medusa's gaze!!
            }

            if (GorgonLense.TotalCharges(m) == 0)
            {
                m.SendLocalizedMessage(1112600); // Your lenses crumble. You are no longer protected from Medusa's gaze!
            }

            return deflect;
        }

        private static int GetScaleEffectiveness(LenseType type)
        {
            switch (type)
            {
                case LenseType.None: return 0;
                case LenseType.Enhanced: return 100;
                case LenseType.Regular: return 50;
                case LenseType.Limited: return 15;
            }

            return 0;
        }

        private void RemovePetrification(Mobile m)
        {
            if (m is BaseCreature bc)
            {
                bc.Frozen = false;
                bc.Blessed = false;
                bc.HueMod = -1;
            }
            else
            {
                m.SolidHueOverride = -1;
                m.Frozen = false;

                m.SendLocalizedMessage(1005603); // You can move again!
                BuffInfo.RemoveBuff(m, BuffIcon.MedusaStone);
            }

            if (0.6 > Utility.RandomDouble())
                CreateReplica(m);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (m_GazeDelay < DateTime.UtcNow)
            {
                DoGaze(attacker);
                m_GazeDelay = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(25, 65));
            }

            if (attacker.Poison == null && InRange(attacker, 2) && 0.6 > Utility.RandomDouble())
            {
                attacker.SendLocalizedMessage(1112368); // You have been poisoned by Medusa's snake-like hair!

                attacker.ApplyPoison(this, Poison.Greater);

                Effects.SendPacket(attacker.Location, attacker.Map, new TargetParticleEffect(attacker, 0x374A, 10, 15, 0, 0, 0x139D, 3, 0));
                Effects.PlaySound(attacker.Location, attacker.Map, 0x574);
            }
        }

        public void DoGaze(Mobile attacker)
        {
            if (!CheckBlockGaze(attacker))
            {
                if (attacker is BaseCreature)
                {
                    BaseCreature pet = attacker as BaseCreature;

                    Mobile master = pet.Summoned ? pet.SummonMaster : pet.ControlMaster;

                    if (master != null)
                        master.SendLocalizedMessage(1113281, "", 42); // Your pet has been petrified!

                    pet.Frozen = true;
                    pet.Blessed = true;
                    pet.HueMod = 0x2E1;
                }
                else
                {
                    attacker.SolidHueOverride = 0x961;
                    attacker.Frozen = true;

                    attacker.SendLocalizedMessage(1112768); // You have been turned to stone!!!
                    BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.MedusaStone, 1153790, 1153825));
                }

                Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback<Mobile>(RemovePetrification), attacker);
            }
        }
        #endregion

        #region Lethal Arrow
        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            if (defender.Poison == null && 0.5 > Utility.RandomDouble())
            {
                defender.SendLocalizedMessage(1112369); // You have been poisoned by Medusa's lethal arrow!

                defender.ApplyPoison(this, Utility.RandomBool() ? Poison.Deadly : Poison.Lethal);

                Effects.SendPacket(defender.Location, defender.Map, new TargetParticleEffect(defender, 0x36CB, 1, 18, 0x43, 5, 0x26B7, 3, 0));
                Effects.PlaySound(defender.Location, defender.Map, 0xDD);
            }
        }
        #endregion

        #region Loot
        public override void GenerateLoot()
        {
            AddLoot(LootPack.SuperBoss, 8);
            AddLoot(LootPack.LootItem<Arrow>(100, 200, true));
            AddLoot(LootPack.LootItem<MedusaStatue>(2.5));
        }

        public override void OnCarve(Mobile from, Corpse corpse, Item with)
        {
            int amount = Utility.Random(5) + 1;

            corpse.DropItem(new MedusaDarkScales(amount));

            if (0.20 > Utility.RandomDouble())
                corpse.DropItem(new MedusaBlood());

            base.OnCarve(from, corpse, with);

            corpse.Carved = true;
        }
        #endregion

        public override bool CanFlee => false;
        public override bool BardImmune => true;
        public override Poison PoisonImmune => Poison.Lethal;

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.WriteMobileList(m_Statues);
            writer.Write(m_ScalesLeft);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Statues = reader.ReadStrongMobileList<BaseCreature>();
            m_ScalesLeft = reader.ReadInt();
        }
    }

    #region Replicas
    public abstract class EvilReplica : BaseCreature
    {
        public override bool DeleteCorpseOnDeath => true;
        public override bool AlwaysMurderer => true;

        public EvilReplica(Mobile m)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = string.Format(NameFormat, m.Name);
            Body = m.Body;
            Hue = m.Hue;

            SetStr(m.Str);
            SetDex(m.Dex);
            SetInt(m.Int);

            SetHits(m.HitsMax);
            SetStam(m.StamMax);
            SetMana(m.ManaMax);

            SetResistance(ResistanceType.Physical, m.PhysicalResistance);
            SetResistance(ResistanceType.Fire, m.FireResistance);
            SetResistance(ResistanceType.Cold, m.ColdResistance);
            SetResistance(ResistanceType.Poison, m.PoisonResistance);
            SetResistance(ResistanceType.Energy, m.EnergyResistance);

            for (int i = 0; i < m.Skills.Length; i++)
            {
                Skill skill = m.Skills[i];

                SetSkill(skill.SkillName, skill.NonRacialValue);
            }

            Frozen = true;

            SolidHueOverride = PetrifiedHue;
            HueMod = PetrifiedHue;
        }

        public EvilReplica(Serial serial)
            : base(serial)
        {
        }

        public abstract int PetrifiedHue { get; }
        public abstract string NameFormat { get; }

        public void OnRequestedAnimation(Mobile from)
        {
            if (Frozen)
            {
                from.Send(new UpdateStatueAnimation(this, 1, 31, 5));
            }
        }

        public virtual void Unpetrify()
        {
            Frozen = false;
            Blessed = false;

            SolidHueOverride = -1;
            HueMod = -1;
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

    public class PetReplica : EvilReplica
    {
        public override WeaponAbility GetWeaponAbility()
        {
            return m_Original.GetWeaponAbility();
        }

        public override int PetrifiedHue => 0x2E1;
        public override string NameFormat => "{0} (evil)";

        private readonly BaseCreature m_Original;

        public PetReplica(BaseCreature m)
            : base(m)
        {
            m_Original = m;

            Blessed = true;

            BaseSoundID = m.BaseSoundID;

            SetHits(m.HitsMax / 2);

            SetDamage(m.DamageMin, m.DamageMax);

            SetDamageType(ResistanceType.Physical, m.PhysicalDamage);
            SetDamageType(ResistanceType.Fire, m.FireDamage);
            SetDamageType(ResistanceType.Cold, m.ColdDamage);
            SetDamageType(ResistanceType.Poison, m.PoisonDamage);
            SetDamageType(ResistanceType.Energy, m.EnergyDamage);
        }

        public PetReplica(Serial serial)
            : base(serial)
        {
        }

        public override void OnBeforeSpawn(Point3D location, Map m)
        {
            base.OnBeforeSpawn(location, m);

            Direction = m_Original.Direction;
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

    public class PlayerReplica : EvilReplica
    {
        public override WeaponAbility GetWeaponAbility()
        {
            if (Weapon == null)
                return null;

            BaseWeapon weapon = this.Weapon as BaseWeapon;

            return Utility.RandomBool() ? weapon.PrimaryAbility : weapon.SecondaryAbility;
        }

        public override int PetrifiedHue => 0x961;
        public override string NameFormat => "{0} the Evil Twin";

        public override Mobile ConstantFocus => m_Original;
        public override bool BardImmune => true;

        private readonly Mobile m_Original;
        private readonly DateTime m_ExpireTime;

        public PlayerReplica(Mobile m)
            : base(m)
        {
            m_Original = m;
            m_ExpireTime = DateTime.UtcNow + TimeSpan.FromMinutes(2.0);

            HairItemID = m.HairItemID;
            HairHue = m.HairHue;
            FacialHairItemID = m.FacialHairItemID;
            FacialHairHue = m.FacialHairHue;

            SetDamage(1, 5);

            SetDamageType(ResistanceType.Physical, 100);

            SetSkill(SkillName.DetectHidden, 100.0);

            SwitchAI();

            for (int i = 0; i < m.Items.Count; i++)
            {
                if (m.Items[i].Layer != Layer.Backpack && m.Items[i].Layer != Layer.Mount && m.Items[i].Layer != Layer.Bank)
                    AddItem(CloneItem(m.Items[i]));
            }
        }

        public PlayerReplica(Serial serial)
            : base(serial)
        {
        }

        public Item CloneItem(Item item)
        {
            Item cloned = new Item(item.ItemID)
            {
                Layer = item.Layer,
                Name = item.Name,
                Hue = item.Hue,
                Weight = item.Weight,
                Movable = false
            };

            return cloned;
        }

        public override void OnThink()
        {
            if (Frozen)
            {
                return;
            }

            if (!m_Original.Alive || m_Original.IsDeadBondedPet || DateTime.UtcNow > m_ExpireTime)
            {
                Kill();
                return;
            }

            if (Map != m_Original.Map || !this.InRange(m_Original, 15))
            {
                Map fromMap = Map;
                Point3D from = Location;

                Map toMap = m_Original.Map;
                Point3D to = m_Original.Location;

                if (toMap != null)
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        Point3D loc = new Point3D(to.X - 4 + Utility.Random(9), to.Y - 4 + Utility.Random(9), to.Z);

                        if (toMap.CanSpawnMobile(loc))
                        {
                            to = loc;
                            break;
                        }

                        loc.Z = toMap.GetAverageZ(loc.X, loc.Y);

                        if (toMap.CanSpawnMobile(loc))
                        {
                            to = loc;
                            break;
                        }
                    }
                }

                Map = toMap;
                Location = to;

                ProcessDelta();

                Effects.SendLocationParticles(EffectItem.Create(from, fromMap, EffectItem.DefaultDuration), 0x3728, 1, 13, 37, 7, 5023, 0);
                FixedParticles(0x3728, 1, 13, 5023, 37, 7, EffectLayer.Waist);

                PlaySound(0x37D);
            }

            Combatant = m_Original;
            FocusMob = m_Original;

            if (AIObject != null)
                AIObject.Action = ActionType.Combat;

            base.OnThink();
        }

        public override bool OnBeforeDeath()
        {
            Effects.PlaySound(Location, Map, 0x10B);
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, TimeSpan.FromSeconds(10.0)), 0x37CC, 1, 50, 2101, 7, 9909, 0);

            Delete();

            return false;
        }

        public override void Unpetrify()
        {
            if (!m_Original.Alive)
            {
                Delete();
                return;
            }

            base.Unpetrify();

            FixedParticles(0x376A, 1, 14, 0x13B5, EffectLayer.Waist);
            PlaySound(0x511);
        }

        private double GetSkill(SkillName name)
        {
            return Skills[name].Value;
        }

        private void SwitchAI()
        {
            AIType ai;

            if (GetSkill(SkillName.Necromancy) > 50.0)
                ai = AIType.AI_Necro;
            else if (GetSkill(SkillName.Mysticism) > 50.0)
                ai = AIType.AI_Mystic;
            else if (GetSkill(SkillName.Spellweaving) > 50.0)
                ai = AIType.AI_Spellweaving;
            else if (GetSkill(SkillName.Magery) > 50.0)
                ai = AIType.AI_Mage;
            else if (GetSkill(SkillName.Archery) > 50.0)
                ai = AIType.AI_Archer;
            else
                ai = AIType.AI_Melee;

            ChangeAIType(ai);
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
    #endregion
}
