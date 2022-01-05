using System.Collections.Generic;
using Server.ContextMenus;
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
        public static bool AllowLocking = false;		// Allow players to lock the shrunken pet or not
        public static double TamingRequired = 0;		// set to zero for no skill requirement to use shrink tools

        // Persisted
        private bool m_Locked;
		private Mobile m_Owner;
		private BaseCreature m_Pet;

		// Not persisted; lazy loaded.
        private string m_Breed;
        private bool m_IsBonded;
		private string m_Name;
		private int m_RawStr;
		private int m_RawDex;
		private int m_RawInt;
		private double m_Wrestling;
		private double m_Tactics;
		private double m_Anatomy;
		private double m_Poisoning;
		private double m_Magery;
		private double m_EvalInt;
		private double m_MagicResist;
		private double m_Meditation;

        [CommandProperty( AccessLevel.GameMaster )]
		public bool Locked
		{
			get => m_Locked; set { m_Locked = value; InvalidateProperties(); }
		}

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
            Hue = m_Pet.Hue;
            Weight = 10;

            LootType = pet.IsBonded ? LootType.Blessed : LootType.Regular;
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
            else if (m_Locked && m_Owner != from)
			{
				from.SendMessage("This is locked and only the owner can claim this pet while locked.");
				from.SendMessage("This item is now being returned to its owner.");
				m_Owner.AddToBackpack(this);
				m_Owner.SendMessage("Your pet {0} has been returned to you because it was locked and {1} was trying to claim it.", m_Breed, from.Name);
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
        }

		public override void Delete()
		{
			if (m_Pet != null)	// Don't orphan pets on the internal map
            {
                m_Pet.Delete();
            }

            base.Delete();
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if ((AllowLocking || m_Locked) && from.Alive && m_Owner == from)
			{
				if (!m_Locked)
                {
                    list.Add(new LockShrinkItem(from, this));
                }
                else
                {
                    list.Add(new UnLockShrinkItem(from, this));
                }
            }
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (null == m_Pet || m_Pet.Deleted)
            {
                return;
            }

            m_IsBonded = m_Pet.IsBonded;
            m_Name = m_Pet.Name;
            m_Breed = m_Pet.GetType().Name;
            m_RawStr = m_Pet.RawStr;
            m_RawDex = m_Pet.RawDex;
            m_RawInt = m_Pet.RawInt;
            m_Wrestling = m_Pet.Skills[SkillName.Wrestling].Base;
            m_Tactics = m_Pet.Skills[SkillName.Tactics].Base;
            m_Anatomy = m_Pet.Skills[SkillName.Anatomy].Base;
            m_Poisoning = m_Pet.Skills[SkillName.Poisoning].Base;
            m_Magery = m_Pet.Skills[SkillName.Magery].Base;
            m_EvalInt = m_Pet.Skills[SkillName.EvalInt].Base;
            m_MagicResist = m_Pet.Skills[SkillName.MagicResist].Base;
            m_Meditation = m_Pet.Skills[SkillName.Meditation].Base;
            
            if (AllowLocking || m_Locked)	// Only show lock status when locking enabled or already locked
            {
                list.Add(1049644, m_Locked ? "Locked" : "Unlocked");
            }

            list.Add(1060663, "Name\t{0} Breed: {1}", m_Name, m_Breed);

            if (m_IsBonded)
            {
                list.Add(1049608);
            }

            list.Add(1061640, m_Owner == null ? "nobody (WILD)" : m_Owner.Name); // Owner: ~1_OWNER~
            list.Add(1060659, "Stats\tStrength {0}, Dexterity {1}, Intelligence {2}", m_RawStr, m_RawDex, m_RawInt);
            list.Add(1060660, "Combat Skills\tWrestling {0}, Tactics {1}, Anatomy {2}, Poisoning {3}", m_Wrestling, m_Tactics, m_Anatomy, m_Poisoning);
            list.Add(1060661, "Magic Skills\tMagery {0}, Eval Intel {1}, Magic Resist {2}, Meditation {3}", m_Magery, m_EvalInt, m_MagicResist, m_Meditation);
        }

        public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
            writer.Write(0); // version

			writer.Write(m_Locked);
			writer.Write(m_Owner);
			writer.Write(m_Pet);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
            reader.ReadInt();

            m_Locked = reader.ReadBool();
            m_Owner = (PlayerMobile)reader.ReadMobile();
            m_Pet = (BaseCreature)reader.ReadMobile();

            if (m_Pet != null)
            {
                m_Pet.IsStabled = true;
            }
        }
	}

	public class LockShrinkItem : ContextMenuEntry
	{
		private readonly Mobile _From;
		private readonly ShrinkItem _ShrinkItem;

		public LockShrinkItem(Mobile from, ShrinkItem shrink)
            : base(2029, 5)
		{
			_From = from;
			_ShrinkItem = shrink;
		}

		public override void OnClick()
		{
			_ShrinkItem.Locked = true;
            _From.SendMessage(38, "You have locked this shrunken pet so only you can reclaim it.");
		}
	}

	public class UnLockShrinkItem : ContextMenuEntry
	{
		private Mobile _From;
		private ShrinkItem _ShrinkItem;

		public UnLockShrinkItem(Mobile from, ShrinkItem shrink)
            : base(2033, 5)
		{
			_From = from;
			_ShrinkItem = shrink;
		}

		public override void OnClick()
		{
			_ShrinkItem.Locked = false;
            _From.SendMessage(38, "You have unlocked this shrunken pet, now anyone can reclaim it as theirs.");
		}
	}
}
