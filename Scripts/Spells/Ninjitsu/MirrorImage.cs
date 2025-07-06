using Server.Items;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using System;
using System.Collections.Generic;

namespace Server.Spells.Ninjitsu
{
    public class MirrorImage : NinjaSpell
    {
        private static readonly Dictionary<Mobile, int> _CloneCount = new Dictionary<Mobile, int>();

        private static readonly SpellInfo _Info = new SpellInfo(
            "Mirror Image", null,
            -1,
            9002);

        public MirrorImage(Mobile caster, Item scroll)
            : base(caster, scroll, _Info)
        {
        }

        public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);
        public override double RequiredSkill => 20.0;
        public override int RequiredMana => 10;
        public override bool BlockedByAnimalForm => false;

        public static bool HasClone(Mobile m)
        {
            return _CloneCount.ContainsKey(m);
        }

        public static void AddClone(Mobile m)
        {
            if (m == null)
            {
                return;
            }

            if (_CloneCount.TryGetValue(m, out int value))
            {
                _CloneCount[m] = ++value;
            }
            else
            {
                _CloneCount[m] = 1;
            }
        }

        public static void RemoveClone(Mobile m)
        {
            if (m == null)
            {
                return;
            }

            if (_CloneCount.TryGetValue(m, out int value))
            {
                _CloneCount[m] = --value;

                if (value == 0)
                {
                    _CloneCount.Remove(m);
                }
            }
        }

        public override bool CheckCast()
        {
            if (Caster.Mounted)
            {
                Caster.SendLocalizedMessage(1063132); // You cannot use this ability while mounted.
                return false;
            }

            if (Caster.Followers + 1 > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1063133); // You cannot summon a mirror image because you have too many followers.
                return false;
            }

            if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
            {
                Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
                return false;
            }

            if (Caster.Flying)
            {
                Caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
                return false;
            }

            return base.CheckCast();
        }

        public override bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
        {
            return false;
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.SendLocalizedMessage(1063134); // You begin to summon a mirror image of yourself.
        }

        public override void OnCast()
        {
            if (Caster.Mounted)
            {
                Caster.SendLocalizedMessage(1063132); // You cannot use this ability while mounted.
            }
            else if (Caster.Followers + 1 > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1063133); // You cannot summon a mirror image because you have too many followers.
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
            {
                Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
            }
            else if (CheckSequence())
            {
                Caster.FixedParticles(0x376A, 1, 14, 0x13B5, EffectLayer.Waist);
                Caster.PlaySound(0x511);

                new Clone(Caster).MoveToWorld(Caster.Location, Caster.Map);
            }

            FinishSequence();
        }

        public static Clone GetDeflect(Mobile attacker, Mobile defender)
        {
            Clone clone = null;

            if (HasClone(defender) && (defender.Skills.Ninjitsu.Value / 133.2) > Utility.RandomDouble())
            {
                IPooledEnumerable eable = defender.GetMobilesInRange(4);

                foreach (Mobile m in eable)
                {
                    clone = m as Clone;

                    if (clone != null && clone.Summoned && clone.SummonMaster == defender)
                    {
                        attacker?.SendLocalizedMessage(1063141); // Your attack has been diverted to a nearby mirror image of your target!

                        defender.SendLocalizedMessage(1063140); // You manage to divert the attack onto one of your nearby mirror images.
                        break;
                    }
                }

                eable.Free();
            }

            return clone;
        }
    }
}

namespace Server.Mobiles
{
    public class Clone : BaseCreature
    {
        public override bool AlwaysAttackable => m_Caster is Travesty;
        public override bool AlwaysMurderer => m_Caster is BaseCreature bc && bc.AlwaysMurderer;

        private Mobile m_Caster;

        public Clone(Mobile caster)
            : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.2, 0.4)
        {
            m_Caster = caster;

            Body = caster.Body;

            Hue = caster.Hue;
            Female = caster.Female;

            Name = caster.Name;
            NameHue = caster.NameHue;

            Title = caster.Title;
            Kills = caster.Kills;

            HairItemID = caster.HairItemID;
            HairHue = caster.HairHue;

            FacialHairItemID = caster.FacialHairItemID;
            FacialHairHue = caster.FacialHairHue;

            for (int i = 0; i < caster.Skills.Length; ++i)
            {
                Skills[i].Base = caster.Skills[i].Base;
                Skills[i].Cap = caster.Skills[i].Cap;
            }

            for (int i = 0; i < caster.Items.Count; i++)
            {
                AddItem(CloneItem(caster.Items[i]));
            }

            Warmode = true;

            Summoned = true;
            SummonMaster = caster;

            ControlOrder = LastOrderType.Follow;
            FollowTarget = caster;

            TimeSpan duration = TimeSpan.FromSeconds(30 + caster.Skills.Ninjitsu.Fixed / 40);

            new UnsummonTimer(this, duration).Start();
            SummonEnd = DateTime.UtcNow + duration;

            MirrorImage.AddClone(m_Caster);

            IgnoreMobiles = true;
        }

        public Clone(Serial serial)
            : base(serial)
        {
        }

        public override bool DeleteCorpseOnDeath => true;
        public override bool IsDispellable => false;
        public override bool Commandable => false;
        protected override BaseAI ForcedAI => new CloneAI(this);

        public override bool CanDetectHidden => false;

        public override bool IsHumanInTown()
        {
            return false;
        }

        public override bool OnMoveOver(Mobile m)
        {
            return true;
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            Delete();
        }

        public override void OnDelete()
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 15, 5042);

            base.OnDelete();
        }

        public override void OnAfterDelete()
        {
            MirrorImage.RemoveClone(m_Caster);
            base.OnAfterDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version

            writer.Write(m_Caster);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();

            m_Caster = reader.ReadMobile();

            MirrorImage.AddClone(m_Caster);
        }

        private Item CloneItem(Item item)
        {
            Item newItem = new Item(item.ItemID)
            {
                Hue = item.Hue,
                Layer = item.Layer
            };

            return newItem;
        }
    }
}

namespace Server.Mobiles
{
    public class CloneAI : BaseAI
    {
        public CloneAI(Clone m)
            : base(m)
        {
            m.CurrentSpeed = m.ActiveSpeed;
        }

        public override bool Think()
        {
            // Clones only follow their owners
            Mobile master = m_Mobile.SummonMaster;

            if (master != null && master.Map == m_Mobile.Map && master.InRange(m_Mobile, m_Mobile.RangePerception))
            {
                int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(master);
                bool bRun = iCurrDist > 5;

                WalkMobileRange(master, 2, bRun, 0, 1);
            }
            else
            {
                WalkRandom(2, 2, 1);
            }

            return true;
        }
    }
}
