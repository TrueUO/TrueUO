using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Spells.SkillMasteries
{
    public enum TrainingType
    {
        Empowerment,
        Berserk,
        ConsumeDamage,
        AsOne
    }

    public class CombatTrainingSpell : SkillMasterySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
                "Combat Training", "",
                -1,
                9002
            );

        public override double UpKeep
        {
            get
            {
                double taming = Caster.Skills[CastSkill].Base;
                double lore = Caster.Skills[SkillName.AnimalLore].Base;
                bool asone = SpellType == TrainingType.AsOne;

                double skillvalue = (taming + (lore / 2));
                int mastery_base = 12;
                if (skillvalue < 150) mastery_base = 12;
                if (skillvalue < 165) mastery_base = 10;
                if (skillvalue < 180) mastery_base = 8;
                if (skillvalue >= 180) mastery_base = 6;

                return asone ? mastery_base * 2 : mastery_base;
            }
        }

        public override double RequiredSkill => 90;
        public override int RequiredMana => 40;
        public override bool PartyEffects => false;
        public override SkillName CastSkill => SkillName.AnimalTaming;
        public override bool CheckManaBeforeCast => !HasSpell(Caster, GetType());

        public TrainingType SpellType { get; set; }

        private int _Phase;
        private int _DamageTaken;
        private bool _Expired;

        public int Phase { get => _Phase; set => _Phase = value; }
        public int DamageTaken { get => _DamageTaken; set => _DamageTaken = value; }

        public CombatTrainingSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool Cast()
        {
            CombatTrainingSpell spell = GetSpell(Caster, typeof(CombatTrainingSpell)) as CombatTrainingSpell;

            if (spell != null)
            {
                spell.Expire();
                return false;
            }

            return base.Cast();
        }

        public override void SendCastEffect()
        {
            base.SendCastEffect();

            Effects.SendPacket(Caster.Location, Caster.Map, new HuedEffect(EffectType.FixedFrom, Caster.Serial, Serial.Zero, 0x37C4, Caster.Location, Caster.Location, 10, 14, false, false, 4, 0, 3));
            Caster.PrivateOverheadMessage(MessageType.Regular, 52, true, "You ready your pet for combat, increasing its battle effectiveness!", Caster.NetState);
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void OnSelected(TrainingType type, Mobile target)
        {            
            if (!CheckSequence())
            {
                FinishSequence();
                return;
            }

            Effects.SendPacket(target.Location, target.Map, new ParticleEffect(EffectType.FixedFrom, target.Serial, Serial.Zero, 0x376A, target.Location, target.Location, 1, 32, false, false, 1262, 0, 0, 9502, 1, target.Serial, 199, 0));
            Effects.SendPacket(target.Location, target.Map, new GraphicalEffect(EffectType.FixedFrom, target.Serial, Serial.Zero, 0x375A, target.Location, target.Location, 35, 90, true, true));

            /* As One - Requires multiple pets to active */
            if (type == TrainingType.AsOne && Caster is PlayerMobile pm)
            {
                var list = pm.AllFollowers.Where(x => x.Map != Map.Internal && x.InRange(Caster, 100) && x != target).ToList();

                if (list.Count > 0)
                {
                    list.ForEach(x =>
                    {
                        Effects.SendPacket(x.Location, x.Map, new ParticleEffect(EffectType.FixedFrom, x.Serial, Serial.Zero, 0x376A, x.Location, x.Location, 1, 32, false, false, 1262, 0, 0, 9502, 1, x.Serial, 199, 0));
                        Effects.SendPacket(x.Location, x.Map, new GraphicalEffect(EffectType.FixedFrom, x.Serial, Serial.Zero, 0x375A, x.Location, x.Location, 35, 90, true, true));
                    });
                }
                else
                {
                    Caster.SendLocalizedMessage(1156110); // Your ability was canceled.
                    FinishSequence();
                    return;
                }
            }
            
            SpellType = type;
            Target = target;

            _Phase = 0;

            BeginTimer();            

            BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.CombatTraining, 1155933, 1156107, $"{SpellType.ToString()}\t{Target.Name}\t{ScaleUpkeep().ToString()}"));
            //You train ~2_NAME~ to use ~1_SKILLNAME~.<br>Mana Upkeep: ~3_COST~

            FinishSequence();
        }

        public override void EndEffects()
        {
            BuffInfo.RemoveBuff(Caster, BuffIcon.CombatTraining);
            Caster.SendSound(0x1ED);

            _Expired = true;
        }

        protected override void DoEffects()
        {
            Effects.SendPacket(Caster.Location, Caster.Map, new ParticleEffect(EffectType.FixedFrom, Caster.Serial, Serial.Zero, 0x376A, Caster.Location, Caster.Location, 1, 32, false, false, 1262, 0, 0, 9502, 1, Caster.Serial, 215, 0));
            Caster.DisruptiveAction();
        }

        public override bool OnTick()
        {
            if (Target == null || Target.IsDeadBondedPet /* || Target.Map != Caster.Map*/)
            {
                Expire();
                return false;
            }

            if (SpellType == TrainingType.AsOne && Caster is PlayerMobile pm && pm.AllFollowers.Count(m => m.Map != Map.Internal && m.InRange(pm.Location, 15)) < 2)
            {
                Expire();
                return false;
            }

            return base.OnTick();
        }

        public double DamageMod
        {
            get
            {
                if (Target == null || SpellType == TrainingType.AsOne)
                    return 0.0;

                double dam = _DamageTaken / (Target.HitsMax * .66);

                if (dam > 1.0) dam = 1.0;

                return dam;
            }
        }

        private void EndPhase1()
        {
            if (_Expired)
                return;

            _Phase = 2;

            Server.Timer.DelayCall(TimeSpan.FromSeconds(SpellType == TrainingType.Berserk ? 8 : 10), EndPhase2);
        }

        private void EndPhase2()
        {
            if (_Expired)
                return;

            _DamageTaken = 0;
            _Phase = 0;

            if (SpellType == TrainingType.Berserk)
            {
                AddRageCooldown(Target);
            }
        }

        public static void CheckDamage(Mobile attacker, Mobile defender, DamageType type, ref int damage)
        {
            if (defender is BaseCreature bc && (bc.Controlled || bc.Summoned))
            {
                CombatTrainingSpell spell = GetSpell<CombatTrainingSpell>(sp => sp.Target == defender);

                if (spell != null)
                {
                    int storedDamage = damage;

                    switch (spell.SpellType)
                    {
                        case TrainingType.Empowerment:
                            break;
                        case TrainingType.Berserk:
                            if (InRageCooldown(bc))
                            {
                                return;
                            }

                            if (spell.Phase > 1)
                            {
                                damage -= (int)(damage * spell.DamageMod);
                                bc.FixedParticles(0x376A, 10, 30, 5052, 1261, 7, EffectLayer.LeftFoot, 0);
                            }
                            break;
                        case TrainingType.ConsumeDamage:
                            if (spell.Phase < 2)
                            {
                                bc.SendDamagePacket(attacker, damage);
                                damage = 0;
                            }
                            break;
                        case TrainingType.AsOne:
                            if (bc.GetMaster() is PlayerMobile)
                            {
                                PlayerMobile pm = bc.GetMaster() as PlayerMobile;
                                List<Mobile> list = pm.AllFollowers.Where(m => m.Map != Map.Internal && m.InRange(pm, 15) && m.CanBeHarmful(attacker)).ToList();

                                if (list.Count > 1)
                                {
                                    damage /= list.Count;

                                    foreach (Mobile m in list)
                                    {
                                        Effects.SendPacket(m.Location, m.Map, new ParticleEffect(EffectType.FixedFrom, m.Serial, Serial.Zero, 0x374A, m.Location, m.Location, 1, 32, false, false, 2734, 0, 0, 9502, 1, m.Serial, 42, 0));
                                        
                                        if (m != defender)
                                        {
                                            m.Damage(damage, attacker, true, false);
                                        }
                                    }
                                }

                                ColUtility.Free(list);
                            }

                            return;
                    }

                    if (spell.Phase < 2)
                    {
                        if (spell.Phase != 1)
                        {
                            spell.Phase = 1;

                            if (spell.SpellType != TrainingType.AsOne && (spell.SpellType != TrainingType.Berserk || !InRageCooldown(bc)))
                            {
                                Server.Timer.DelayCall(TimeSpan.FromSeconds(5), spell.EndPhase1);
                            }
                        }

                        if (spell.DamageTaken == 0)
                            bc.FixedEffect(0x3779, 10, 30, 1743, 0);

                        spell.DamageTaken += storedDamage;
                    }
                }
            }
            else if (attacker is BaseCreature creature && (creature.Controlled || creature.Summoned))
            {
                CombatTrainingSpell spell = GetSpell<CombatTrainingSpell>(sp => sp.Target == attacker);

                if (spell != null)
                {
                    switch (spell.SpellType)
                    {
                        case TrainingType.Empowerment:
                            if (spell.Phase > 1)
                            {
                                damage += (int)(damage * spell.DamageMod);
                                creature.FixedParticles(0x376A, 10, 30, 5052, 1261, 7, EffectLayer.LeftFoot, 0);
                            }
                            break;
                        case TrainingType.Berserk:
                        case TrainingType.ConsumeDamage:
                        case TrainingType.AsOne:
                            break;
                    }
                }
            }
        }

        public static void OnCreatureHit(Mobile attacker, Mobile defender, ref int damage)
        {
            if (attacker is BaseCreature bc && (bc.Controlled || bc.Summoned))
            {
                CombatTrainingSpell spell = GetSpell<CombatTrainingSpell>(sp => sp.Target == attacker);

                if (spell != null)
                {
                    switch (spell.SpellType)
                    {
                        case TrainingType.Empowerment:
                            break;
                        case TrainingType.Berserk:
                            if (spell.Phase > 1)
                            {
                                damage += (int)(damage * spell.DamageMod);
                                bc.FixedParticles(0x376A, 10, 30, 5052, 1261, 7, EffectLayer.LeftFoot, 0);
                            }
                            break;
                        case TrainingType.ConsumeDamage:
                        case TrainingType.AsOne:
                            break;
                    }
                }
            }
        }

        public static int RegenBonus(Mobile m)
        {
            if (m is BaseCreature bc && (bc.Controlled || bc.Summoned))
            {
                CombatTrainingSpell spell = GetSpell<CombatTrainingSpell>(sp => sp.Target == m);

                if (spell != null && spell.SpellType == TrainingType.ConsumeDamage && spell.Phase > 1)
                {
                    return (int)(30.0 * spell.DamageMod);
                }
            }

            return 0;
        }

        public static int GetHitChanceBonus(Mobile m)
        {
            if (m is BaseCreature bc && (bc.Controlled || bc.Summoned))
            {
                CombatTrainingSpell spell = GetSpell<CombatTrainingSpell>(sp => sp.Target == m);

                if (spell != null && spell.SpellType == TrainingType.ConsumeDamage && spell.Phase > 1)
                {
                    return (int)(45 * spell.DamageMod);
                }
            }

            return 0;
        }

        public class InternalTarget : Target
        {
            public CombatTrainingSpell Spell { get; }

            public InternalTarget(CombatTrainingSpell spell)
                : base(8, false, TargetFlags.None)
            {
                Spell = spell;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Engines.Despise.DespiseCreature)
                {
                    return;
                }

                if (targeted is BaseCreature bc && bc.GetMaster() == from && from.Spell == Spell)
                {
                    from.SendSound(0x64E);
                    Effects.SendPacket(from.Location, from.Map, new ParticleEffect(EffectType.FixedFrom, from.Serial, Serial.Zero, 0x3779, from.Location, from.Location, 1, 15, false, false, 63, 0, 0, 5060, 1, from.Serial, 215, 0));
                    
                    int taming = (int)from.Skills[SkillName.AnimalTaming].Value;
                    int lore = (int)from.Skills[SkillName.AnimalLore].Value;

                    from.CheckTargetSkill(SkillName.AnimalTaming, bc, taming - 25, taming + 25);
                    from.CheckTargetSkill(SkillName.AnimalLore, bc, lore - 25, lore + 25);

                    from.CloseGump(typeof(ChooseTrainingGump));
                    from.SendGump(new ChooseTrainingGump(from, bc, Spell));
                }
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                from.SendLocalizedMessage(1156110); // Your ability was canceled.
                Spell.FinishSequence();
            }
        }

        public static void AddRageCooldown(Mobile m)
        {
            if (_RageCooldown == null)
                _RageCooldown = new Dictionary<Mobile, Timer>();

            _RageCooldown[m] = Server.Timer.DelayCall(TimeSpan.FromSeconds(60), EndRageCooldown, m);
        }

        public static bool InRageCooldown(Mobile m)
        {
            return _RageCooldown != null && _RageCooldown.ContainsKey(m);
        }

        public static void EndRageCooldown(Mobile m)
        {
            if (_RageCooldown != null && _RageCooldown.ContainsKey(m))
            {
                _RageCooldown.Remove(m);
            }
        }

        public static Dictionary<Mobile, Timer> _RageCooldown;
    }

    public class ChooseTrainingGump : Gump
    {
        public CombatTrainingSpell Spell { get; }
        public Mobile Caster { get; }
        public BaseCreature Target { get; }

        public const int Hue = 0xEF9;

        public ChooseTrainingGump(Mobile caster, BaseCreature target, CombatTrainingSpell spell)
            : base(200, 100)
        {
            Spell = spell;
            Caster = caster;
            Target = target;

            AddPage(0);

            AddBackground(10, 10, 250, 178, 0x2436);
            AddAlphaRegion(20, 20, 230, 158);

            AddImage(220, 20, 0x28E0);
            AddImage(220, 72, 0x28E0);
            AddImage(220, 124, 0x28E0);
            AddItem(188, 16, 0x1AE3);
            AddItem(198, 168, 0x1AE1);
            AddItem(8, 15, 0x1AE2);
            AddItem(2, 168, 0x1AE0);

            AddHtmlLocalized(30, 26, 200, 20, 1156113, Hue, false, false); // Select Training

            int y = 53;
            if (MasteryInfo.HasLearned(caster, SkillName.AnimalTaming, 1))
            {
                AddButton(27, y, 0x25E6, 0x25E7, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, y - 2, 150, 20, 1156109, Hue, false, false); // Empowerment
                y += 21;
            }

            if (MasteryInfo.HasLearned(caster, SkillName.AnimalTaming, 1))
            {
                AddButton(27, y, 0x25E6, 0x25E7, 4, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, y, 150, 20, 1157544, Hue, false, false); // As One

                y += 21;
            }

            if (MasteryInfo.HasLearned(caster, SkillName.AnimalTaming, 2))
            {
                AddButton(27, y, 0x25E6, 0x25E7, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, y, 150, 20, 1153271, Hue, false, false); // Berserk
                y += 21;
            }

            if (MasteryInfo.HasLearned(caster, SkillName.AnimalTaming, 3))
            {
                AddButton(27, y, 0x25E6, 0x25E7, 3, GumpButtonType.Reply, 0);
                AddHtmlLocalized(50, y, 150, 20, 1156108, Hue, false, false); // Consume Damage                
            }            
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 0)
            {
                Spell.FinishSequence();
                state.Mobile.SendLocalizedMessage(1156110); // Your ability was canceled. 
                return;
            }

            Spell.OnSelected((TrainingType)info.ButtonID - 1, Target);
        }
    }
}
