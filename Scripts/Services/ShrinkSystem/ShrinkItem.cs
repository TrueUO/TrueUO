using Server.Mobiles;
using Server.Spells;

namespace Server.Services.ShrinkSystem
{
    public interface IShrinkItem
    {
        BaseCreature ShrunkenPet { get; }
    }

	public class ShrinkItem : Item, IShrinkItem
	{
        // Settings
        public static double TamingRequired = 0; // set to zero for no skill requirement to use shrink tools

        // Persisted
        private Mobile m_Owner;
		private BaseCreature m_Pet;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get => m_Owner; set { m_Owner = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public BaseCreature ShrunkenPet
		{
			get => m_Pet; set { m_Pet = value; InvalidateProperties(); }
		}

		public ShrinkItem(BaseCreature pet)
            : base(ShrinkTable.Lookup(pet))
		{
			ShrinkPet(pet);

            Name = "a shrunken pet";
            Weight = 10;
            Hue = m_Pet.Hue;
            LootType = m_Pet.IsBonded ? LootType.Blessed : LootType.Regular;
        }

        public ShrinkItem(Serial serial)
            : base(serial)
        {
        }

		public override void OnDoubleClick(Mobile from)
		{
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else if (m_Pet == null || m_Pet.Deleted || ItemID == 0xFAA)
            {
                from.SendMessage("Due to unforeseen circumstances your pet is lost forever.");
            }
            else if (from.Followers + m_Pet.ControlSlots > from.FollowersMax)
            {
                from.SendMessage("You have to many followers to claim this pet.");
            }
            else if (SpellHelper.CheckCombat(from))
            {
                from.SendMessage("You cannot reclaim your pet while your fighting.");
            }
            else if (!m_Pet.CanBeControlledBy(from))
            {
                from.SendMessage("You do not have the required skills to control this pet.");
            }
            else
            {
                m_Pet.SetControlMaster(from);
                m_Pet.IsStabled = false;

                m_Pet.MoveToWorld(from.Location, from.Map);

                if (from != m_Owner)
                {
                    m_Pet.IsBonded = false;
                }

                m_Pet = null;

                Delete();
            }
        }

		private void ShrinkPet(BaseCreature pet)
		{
			m_Pet = pet;
            m_Owner = pet.ControlMaster;

            m_Pet.ControlTarget = null;
            m_Pet.ControlOrder = OrderType.Stay;
            m_Pet.Internalize();

			m_Pet.SetControlMaster(null);
            m_Pet.SummonMaster = null;
            m_Pet.Loyalty = BaseCreature.MaxLoyalty;
            m_Pet.IsStabled = true;
            m_Pet.StopDeleteTimer();
        }

		public override void Delete()
		{
			if (m_Pet != null)	// Don't orphan pets on the internal map
            {
                m_Pet.Delete();
            }

            base.Delete();
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (null == m_Pet || m_Pet.Deleted)
            {
                return;
            }

            var wrestling = m_Pet.Skills[SkillName.Wrestling].Base;
            var tactics = m_Pet.Skills[SkillName.Tactics].Base;
            var anatomy = m_Pet.Skills[SkillName.Anatomy].Base;
            var poisoning = m_Pet.Skills[SkillName.Poisoning].Base;
            var magery = m_Pet.Skills[SkillName.Magery].Base;
            var eval = m_Pet.Skills[SkillName.EvalInt].Base;
            var resist = m_Pet.Skills[SkillName.MagicResist].Base;
            var meditation = m_Pet.Skills[SkillName.Meditation].Base;
            
            list.Add(1060663, "Name\t{0} Breed: {1}", m_Pet.Name, m_Pet.GetType().Name);

            if (m_Pet.IsBonded)
            {
                list.Add(1049608);
            }

            list.Add(1061640, m_Owner == null ? "nobody (WILD)" : m_Owner.Name); // Owner: ~1_OWNER~
            list.Add(1060659, "Stats\tStrength {0}, Dexterity {1}, Intelligence {2}", m_Pet.RawStr, m_Pet.RawDex, m_Pet.RawInt);
            list.Add(1060660, "Combat Skills\tWrestling {0}, Tactics {1}, Anatomy {2}, Poisoning {3}", wrestling, tactics, anatomy, poisoning);
            list.Add(1060661, "Magic Skills\tMagery {0}, Eval Intel {1}, Magic Resist {2}, Meditation {3}", magery, eval, resist, meditation);
        }

        public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
            writer.Write(0); // version

			writer.Write(m_Owner);
			writer.Write(m_Pet);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
            reader.ReadInt();

            m_Owner = (PlayerMobile)reader.ReadMobile();
            m_Pet = (BaseCreature)reader.ReadMobile();

            if (m_Pet != null)
            {
                m_Pet.IsStabled = true;
            }
        }
	}
}
