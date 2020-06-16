using Server.Items;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Necromancy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.JollyRoger
{
    [CorpseName("a human corpse")]
    public class ShrineMaster : BaseCreature
    {
        public static SkillName RandomSpecialty()
        {
            return _Specialties.ElementAt(Utility.RandomList(_Specialties.Count)).Key;
        }

        private static readonly Dictionary<SkillName, string> _Specialties = new Dictionary<SkillName, string>()
        {
            {SkillName.Swords,  "the Swordsman"},
            {SkillName.Fencing, "the Fencer"},
            {SkillName.Macing, "the Macer"},
            {SkillName.Archery, "the Archer"},
            {SkillName.Magery, "the Wizard"},
            {SkillName.Mysticism, "the Mystic"},
            {SkillName.Bushido, "the Sampire"},
            {SkillName.Necromancy, "the Necromancer"},
            {SkillName.Poisoning, "the Assassin"},
            {SkillName.Peacemaking, "the Bard"}
        };

        private MasterType _Type;
        private SkillName _Specialty;

        private bool _Sampire;
        private DateTime _NextSpecial;

        public override bool AlwaysMurderer => true;
        public override double HealChance => AI == AIType.AI_Melee || AI == AIType.AI_Paladin ? 1.0 : 0.0;
        public override double WeaponAbilityChance => AI == AIType.AI_Melee || AI == AIType.AI_Paladin ? 0.4 : 0.1;
        public override bool CanStealth => _Specialty == SkillName.Ninjitsu;

        public override WeaponAbility GetWeaponAbility()
        {
            BaseWeapon wep = Weapon as BaseWeapon;

            if (wep != null)
            {
                return 0.6 > Utility.RandomDouble() ? wep.PrimaryAbility : wep.SecondaryAbility;
            }

            return null;
        }

        public override bool UseSmartAI => true;
        public virtual bool CanDoSpecial => SpellCaster;

        public virtual double MinSkill => 105.0;
        public virtual double MaxSkill => 130.0;
        public virtual int MinResist => 20;
        public virtual int MaxResist => 30;

        public bool SpellCaster => AI != AIType.AI_Melee && AI != AIType.AI_Ninja && AI != AIType.AI_Samurai && AI != AIType.AI_Paladin;

        [Constructable]
        public ShrineMaster(MasterType type, ShrineBattleController controller)
            : this(RandomSpecialty(), type, controller)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ShrineBattleController _Controller { get; set; }

        [Constructable]
        public ShrineMaster(SkillName specialty, MasterType type, ShrineBattleController controller)
            : base(GetAI(specialty), FightMode.Closest, 10, 1, .2, .1)
        {
            _Specialty = specialty;
            _Type = type;
            _Controller = _Controller;

            if (_Specialty == SkillName.Bushido && Utility.RandomBool())
                _Sampire = true;

            if (Female = Utility.RandomBool())
            {
                //Body = 0x191;
                Name = NameList.RandomName("female");
            }
            else
            {
                //Body = 0x190;
                Name = NameList.RandomName("male");
            }

            SetBody();

            Title = _Specialties[specialty];

            SetStr(250);
            SetDex(SpellCaster ? 150 : 200);
            SetInt(SpellCaster ? 1000 : 5000);

            SetHits(20000, 25000);

            if (AI == AIType.AI_Melee)
                SetDamage(22, 30);
            else if (!SpellCaster)
                SetDamage(20, 28);
            else
                SetDamage(10, 20);

            Fame = 48000;
            Karma = -48000;

            SetResists();
            SetSkills();
            EquipSpecialty();

            _NextSpecial = DateTime.UtcNow;

            if (_Sampire)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
                    {
                        VampiricEmbraceSpell spell = new VampiricEmbraceSpell(this, null);
                        spell.Cast();
                    });
            }

            SetAbility();
        }

        public virtual void SetSkills()
        {
            SetSkill(SkillName.Wrestling, MinSkill, MaxSkill);
            SetSkill(SkillName.Tactics, MinSkill, MaxSkill);
            SetSkill(SkillName.Anatomy, MinSkill, MaxSkill);

            switch (_Specialty)
            {
                case SkillName.Swords: // Swordsman
                    SetSkill(SkillName.Swords, MinSkill, MaxSkill);
                    SetSkill(SkillName.Tactics, MinSkill, MaxSkill);
                    SetSkill(SkillName.Parry, MinSkill, MaxSkill);
                    SetSkill(SkillName.Bushido, MinSkill, MaxSkill);
                    break;
                case SkillName.Fencing: // Fencer
                    SetSkill(SkillName.Fencing, MinSkill, MaxSkill);
                    SetSkill(SkillName.Tactics, MinSkill, MaxSkill);
                    SetSkill(SkillName.Parry, MinSkill, MaxSkill);
                    SetSkill(SkillName.Ninjitsu, MinSkill, MaxSkill);
                    break;
                case SkillName.Macing: // Macer
                    SetSkill(SkillName.Macing, MinSkill, MaxSkill);
                    SetSkill(SkillName.Tactics, MinSkill, MaxSkill);
                    SetSkill(SkillName.Parry, MinSkill, MaxSkill);
                    break;
                case SkillName.Archery: // Archer
                    SetSkill(SkillName.Archery, MinSkill, MaxSkill);
                    SetSkill(SkillName.MagicResist, MinSkill, MaxSkill);
                    SetSkill(SkillName.Swords, MinSkill, MaxSkill);
                    SetSkill(SkillName.Tactics, MinSkill, MaxSkill);
                    break;
                case SkillName.Magery: // Wizard
                    SetSkill(SkillName.Magery, MinSkill, MaxSkill);
                    SetSkill(SkillName.EvalInt, MinSkill, MaxSkill);
                    SetSkill(SkillName.Meditation, MinSkill, MaxSkill);
                    SetSkill(SkillName.MagicResist, MinSkill, MaxSkill);
                    break;
                case SkillName.Mysticism: // Mystic
                    SetSkill(SkillName.Mysticism, MinSkill, MaxSkill);
                    SetSkill(SkillName.Focus, MinSkill, MaxSkill);
                    SetSkill(SkillName.Meditation, MinSkill, MaxSkill);
                    SetSkill(SkillName.MagicResist, MinSkill, MaxSkill);
                    break;
                case SkillName.Necromancy: // Necromancer
                    SetSkill(SkillName.Necromancy, MinSkill, MaxSkill);
                    SetSkill(SkillName.SpiritSpeak, MinSkill, MaxSkill);
                    SetSkill(SkillName.MagicResist, MinSkill, MaxSkill);
                    break;
                case SkillName.Bushido: // Sampire
                    SetSkill(SkillName.Chivalry, MinSkill, MaxSkill);
                    SetSkill(SkillName.Bushido, MinSkill, MaxSkill);
                    SetSkill(SkillName.Necromancy, MinSkill, MaxSkill);
                    SetSkill(SkillName.SpiritSpeak, MinSkill, MaxSkill);
                    SetSkill(SkillName.Swords, MinSkill, MaxSkill);
                    SetSkill(SkillName.Parry, MinSkill, MaxSkill);
                    break;
                case SkillName.Poisoning: // Assassin
                    SetSkill(SkillName.Hiding, MinSkill, MaxSkill);
                    SetSkill(SkillName.Stealth, MinSkill, MaxSkill);
                    SetSkill(SkillName.Poisoning, MinSkill, MaxSkill);
                    SetSkill(SkillName.Swords, MinSkill, MaxSkill);
                    SetSkill(SkillName.Fencing, MinSkill, MaxSkill);
                    SetSkill(SkillName.Ninjitsu, MinSkill, MaxSkill);
                    break;
                case SkillName.Peacemaking: // Bard
                    SetSkill(SkillName.Musicianship, MinSkill, MaxSkill);
                    SetSkill(SkillName.Peacemaking, MinSkill, MaxSkill);
                    break;
            }
        }

        public virtual void SetAbility()
        {
            switch (_Specialty)
            {
                case SkillName.Peacemaking:
                    SetSpecialAbility(SpecialAbility.HowlOfCacophony);
                    break;
                case SkillName.Swords:
                    SetSpecialAbility(SpecialAbility.AngryFire);
                    SetSpecialAbility(SpecialAbility.SearingWounds);
                    SetWeaponAbility(WeaponAbility.FrenziedWhirlwind);
                    break;
                case SkillName.Fencing:
                    SetWeaponAbility(WeaponAbility.Feint);
                    SetWeaponAbility(WeaponAbility.ArmorIgnore);
                    break;
                case SkillName.Macing:
                    SetWeaponAbility(WeaponAbility.CrushingBlow);
                    break;
                case SkillName.Archery:
                    SetWeaponAbility(WeaponAbility.PsychicAttack);
                    SetWeaponAbility(WeaponAbility.ForceArrow);
                    SetWeaponAbility(WeaponAbility.ParalyzingBlow);
                    break;
                case SkillName.Necromancy:
                    SetSpecialAbility(SpecialAbility.ManaDrain);
                    SetSpecialAbility(SpecialAbility.LifeLeech);
                    break;
                case SkillName.Poisoning:
                    SetAreaEffect(AreaEffect.PoisonBreath);
                    break;
            }
        }

        public virtual void SetBody()
        {
            switch (_Specialty)
            {
                default:
                    if (0.75 > Utility.RandomDouble())
                        Race = Race.Human;
                    else
                        Race = Race.Elf; break;
                case SkillName.Archery:
                case SkillName.Spellweaving: Race = Race.Elf; break;
            }

            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();

            FacialHairItemID = Race.RandomFacialHair(Female);
            FacialHairHue = Race.RandomHairHue();

            Hue = Race.RandomSkinHue();
        }

        public virtual void SetResists()
        {
            SetResistance(ResistanceType.Physical, MinResist, MaxResist);
            SetResistance(ResistanceType.Fire, MinResist, MaxResist);
            SetResistance(ResistanceType.Cold, MinResist, MaxResist);
            SetResistance(ResistanceType.Poison, MinResist, MaxResist);
            SetResistance(ResistanceType.Energy, MinResist, MaxResist);
        }

        public virtual void EquipSpecialty()
        {
            SetWearable(new ThighBoots());
            SetWearable(new BodySash(), Utility.RandomSlimeHue());

            switch (_Specialty)
            {
                case SkillName.Chivalry:
                    SetWearable(RandomSwordWeapon());
                    PaladinEquip();
                    break;
                case SkillName.Swords:
                    SetWearable(RandomSwordWeapon());
                    StandardMeleeEquip();
                    break;
                case SkillName.Fencing:
                    SetWearable(RandomFencingWeapon());
                    StandardMeleeEquip();
                    break;
                case SkillName.Macing:
                    SetWearable(RandomMaceWeapon());
                    StandardMeleeEquip();
                    break;
                case SkillName.Archery:
                    SetWearable(RandomArhceryWeapon());
                    StandardMeleeEquip();
                    break;
                case SkillName.Magery:
                    SetWearable(RandomMageWeapon());
                    StandardMageEquip();
                    break;
                case SkillName.Mysticism:
                    SetWearable(RandomMageWeapon());
                    StandardMageEquip();
                    break;
                case SkillName.Spellweaving:
                    SetWearable(RandomMageWeapon());
                    StandardMageEquip();
                    break;
                case SkillName.Necromancy:
                    SetWearable(RandomMageWeapon());
                    StandardMageEquip();
                    break;
                case SkillName.Bushido:
                    BaseWeapon w = RandomSamuraiWeapon() as BaseWeapon;
                    SetWearable(w);

                    SetWearable(new LeatherSuneate());
                    SetWearable(new LeatherJingasa());
                    SetWearable(new LeatherDo());
                    SetWearable(new LeatherHiroSode());
                    SetWearable(new SamuraiTabi(Utility.RandomNondyedHue()));

                    if (_Sampire)
                        w.WeaponAttributes.HitLeechHits = 100;

                    SetSkill(SkillName.Parry, 120);
                    break;
                case SkillName.Ninjitsu:
                    SetWearable(RandomNinjaWeapon());

                    LeatherNinjaBelt belt = new LeatherNinjaBelt();
                    belt.UsesRemaining = 20;
                    belt.Poison = Poison.Greater;
                    belt.PoisonCharges = 20;
                    SetWearable(belt);

                    for (int i = 0; i < 2; i++)
                    {
                        Fukiya f = new Fukiya();
                        f.UsesRemaining = 10;
                        f.Poison = Poison.Greater;
                        f.PoisonCharges = 10;
                        f.Movable = false;
                        PackItem(f);
                    }

                    SetWearable(new NinjaTabi());
                    SetWearable(new LeatherNinjaJacket());
                    SetWearable(new LeatherNinjaHood());
                    SetWearable(new LeatherNinjaPants());
                    SetWearable(new LeatherNinjaMitts());

                    break;
                case SkillName.Poisoning:
                    BaseWeapon wep = RandomAssassinWeapon() as BaseWeapon;
                    wep.Poison = Poison.Lethal;
                    wep.PoisonCharges = 100;
                    SetWearable(wep);

                    SetWearable(new LeatherChest());
                    SetWearable(new LeatherLegs());
                    SetWearable(new LeatherGloves());
                    SetWearable(new LeatherGorget());
                    break;
            }
        }

        private void PaladinEquip()
        {
            SetWearable(Loot.Construct(new Type[] { typeof(Bascinet), typeof(Helmet), typeof(PlateHelm) }), 1153);

            SetWearable(new PlateChest());
            SetWearable(new PlateLegs());
            SetWearable(new PlateGloves());
            SetWearable(new PlateGorget());
            SetWearable(new PlateArms());
            SetWearable(new MetalKiteShield());

            SetSkill(SkillName.Parry, 120);
        }

        private void StandardMeleeEquip()
        {
            SetWearable(Loot.Construct(new Type[] { typeof(Bascinet), typeof(Helmet), typeof(LeatherCap), typeof(RoyalCirclet) }));

            SetWearable(new ChainChest());
            SetWearable(new ChainLegs());
            SetWearable(new RingmailGloves());
            SetWearable(new LeatherGorget());
        }

        private void StandardMageEquip()
        {
            bool mage = AI == AIType.AI_Mage;

            SetWearable(new WizardsHat(), mage ? Utility.RandomBlueHue() : Utility.RandomRedHue());
            SetWearable(new Robe(), mage ? Utility.RandomBlueHue() : Utility.RandomRedHue());
            SetWearable(new LeatherGloves());
        }

        public static AIType GetAI(SkillName skill)
        {
            switch (skill)
            {
                default: return AIType.AI_Melee;
                case SkillName.Ninjitsu: return AIType.AI_Ninja;
                case SkillName.Bushido: return AIType.AI_Samurai;
                case SkillName.Chivalry: return AIType.AI_Paladin;
                case SkillName.Magery: return AIType.AI_Mage;
                case SkillName.Necromancy: return AIType.AI_NecroMage;
                case SkillName.Spellweaving: return AIType.AI_Spellweaving;
                case SkillName.Mysticism: return AIType.AI_Mystic;
            }
        }

        public Item RandomSwordWeapon()
        {
            if (Race == Race.Elf)
                return Loot.Construct(new Type[] { typeof(ElvenMachete), typeof(RadiantScimitar) });

            return Loot.Construct(new Type[] { typeof(Broadsword), typeof(Longsword), typeof(Katana), typeof(Halberd), typeof(Bardiche), typeof(VikingSword) });
        }

        public Item RandomFencingWeapon()
        {
            if (Race == Race.Elf)
                return Loot.Construct(new Type[] { typeof(Leafblade), typeof(WarCleaver), typeof(AssassinSpike) });

            return Loot.Construct(new Type[] { typeof(Kryss), typeof(Spear), typeof(ShortSpear), typeof(Lance), typeof(Pike) });
        }

        public Item RandomMaceWeapon()
        {
            return Loot.Construct(new Type[] { typeof(Mace), typeof(WarHammer), typeof(WarAxe), typeof(BlackStaff), typeof(QuarterStaff), typeof(WarMace), typeof(DiamondMace), typeof(Scepter) });
        }

        public Item RandomArhceryWeapon()
        {
            if (Race == Race.Elf)
                return Loot.Construct(new Type[] { typeof(MagicalShortbow), typeof(ElvenCompositeLongbow) });

            return Loot.Construct(new Type[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(RepeatingCrossbow) });
        }

        public Item RandomMageWeapon()
        {
            return Loot.Construct(new Type[] { typeof(Spellbook), typeof(GnarledStaff), typeof(BlackStaff), typeof(QuarterStaff), typeof(WildStaff) });
        }

        public Item RandomSamuraiWeapon()
        {
            return Loot.Construct(new Type[] { typeof(Lajatang), typeof(Wakizashi), typeof(NoDachi) });
        }

        public Item RandomNinjaWeapon()
        {
            return Loot.Construct(new Type[] { typeof(Wakizashi), typeof(Tessen), typeof(Nunchaku), typeof(Daisho), typeof(Sai), typeof(Tekagi), typeof(Kama), typeof(Katana) });
        }

        public Item RandomAssassinWeapon()
        {
            return Loot.Construct(new Type[] { typeof(Cleaver), typeof(ButcherKnife), typeof(Kryss), typeof(Dagger) });
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1154196 + (int)_Type);
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Blessed && _Controller != null && _Controller.MasterBlessCheck(this))
            {
                Blessed = false;
            }

            if (Combatant == null)
                return;

            if (!Utility.InRange(Location, Home, 20))
            {
                Timer.DelayCall(TimeSpan.FromSeconds(5), () => { Location = Home; });
            }

            if (CanDoSpecial && InRange(Combatant, 4) && 0.1 > Utility.RandomDouble() && _NextSpecial < DateTime.UtcNow)
            {
                DoSpecial();

                _NextSpecial = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 60));
            }
            else if (_Sampire)
            {
                if (0.1 > Utility.RandomDouble() && Weapon is BaseWeapon && !CurseWeaponSpell.IsCursed(this, (BaseWeapon)Weapon))
                {
                    CurseWeaponSpell spell = new CurseWeaponSpell(this, null);
                    spell.Cast();
                }
                else if (!TransformationSpellHelper.UnderTransformation(this, typeof(VampiricEmbraceSpell)))
                {
                    VampiricEmbraceSpell spell = new VampiricEmbraceSpell(this, null);
                    spell.Cast();
                }
            }
        }

        private void DoSpecial()
        {
            if (Map == null || Map == Map.Internal)
                return;

            Map m = Map;

            for (int i = 0; i < 4; i++)
            {
                Timer.DelayCall(TimeSpan.FromMilliseconds(i * 50), o =>
                {
                    Server.Misc.Geometry.Circle2D(Location, m, o, (pnt, map) =>
                    {
                        Effects.SendLocationEffect(pnt, map, Utility.RandomBool() ? 14000 : 14013, 14, 20, 2018, 0);
                    });
                }, i);
            }

            Timer.DelayCall(TimeSpan.FromMilliseconds(200), () =>
                {
                    if (m != null)
                    {
                        List<Mobile> list = new List<Mobile>();
                        IPooledEnumerable eable = m.GetMobilesInRange(Location, 4);

                        foreach (Mobile mob in eable)
                        {
                            if (mob.AccessLevel > AccessLevel.Player)
                                continue;

                            if (mob is PlayerMobile || (mob is BaseCreature && ((BaseCreature)mob).GetMaster() is PlayerMobile) && CanBeHarmful(mob))
                                list.Add(mob);
                        }

                        list.ForEach(mob =>
                            {
                                AOS.Damage(mob, this, Utility.RandomMinMax(80, 90), 0, 0, 0, 0, 0, 100, 0);
                            });

                        list.Clear();
                        list.TrimExcess();
                    }
                });
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            List<DamageStore> rights = GetLootingRights();
            rights.Sort();

            List<Mobile> list = rights.Select(x => x.m_Mobile).Where(m => m.InRange(c.Location, 20)).ToList();

            if (list.Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Mobile drop;
                    Item item = ShrineBattleController.CreateItem(list[0]);

                    if (list.Count == 1 || i >= list.Count)
                        drop = list[0];
                    else
                        drop = list[i];

                    if (_Controller != null)
                    {
                        WOSAnkhOfSacrifice.AddReward(drop, _Controller.Shrine);
                        _Controller.OnMasterDestroyed();
                    }

                    drop.SendLocalizedMessage(1159318); // You notice the Fellowship Insignia on your fallen foe's equipment and decide it may be of some value...

                    if (drop.Backpack == null || !drop.Backpack.TryDropItem(drop, item, false))
                    {
                        drop.BankBox.DropItem(item);
                        drop.SendLocalizedMessage(1079730); // The item has been placed into your bank box.
                    }
                }
            }

            ColUtility.Free(list);
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 2);
            AddLoot(LootPack.LootItemCallback(CheckAbilityLootItem, 100.0, Utility.RandomMinMax(10, 25), false, true));
        }

        protected Item CheckAbilityLootItem(IEntity e)
        {
            if (AbilityProfile != null && AbilityProfile.HasAbility(SpecialAbility.Heal))
            {
                return new Bandage();
            }

            return null;
        }

        public ShrineMaster(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write((int)_Specialty);
            writer.Write((int)_Type);
            writer.Write(_Sampire);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            _Specialty = (SkillName)reader.ReadInt();
            _Type = (MasterType)reader.ReadInt();
            _Sampire = reader.ReadBool();

            _NextSpecial = DateTime.UtcNow;
        }
    }
}
