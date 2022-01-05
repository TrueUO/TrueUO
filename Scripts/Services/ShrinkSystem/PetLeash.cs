using Server.Mobiles;
using Server.Targeting;

namespace Server.Services.ShrinkSystem
{
    public interface IShrinkTool
    {
        int ShrinkCharges { get; set; }
    }

	public class PetLeash : Item, IShrinkTool
	{	
		private int m_Charges = -1; // set to -1 for infinite uses

		[CommandProperty( AccessLevel.GameMaster )]
		public int ShrinkCharges
		{
			get => m_Charges;
            set
			{
				if (m_Charges == 0 || (m_Charges = value) == 0)
                {
                    Delete();
                }
                else
                {
                    InvalidateProperties();
                }
            }
		}

		[Constructable]
		public PetLeash()
            : base(0x1374)
		{
			Weight = 1.0;
			Movable = true;
			Name = "Pet Leash";
			LootType = LootType.Blessed;
		}

		public PetLeash(Serial serial)
            : base(serial)
		{
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (m_Charges >= 0)
            {
                list.Add(1060658, "Charges\t{0}", m_Charges.ToString());
            }
        }

		public override void OnDoubleClick(Mobile from)
		{
			bool isStaff = from.AccessLevel != AccessLevel.Player;

			if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else if (isStaff || from.Skills[ SkillName.AnimalTaming ].Value >= ShrinkItem.TamingRequired)
            {
                from.Target = new ShrinkTarget(from, this);
            }
            else
            {
                from.SendMessage("You must have at least " + ShrinkItem.TamingRequired + " animal taming to use a pet leash.");
            }
        }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(m_Charges);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
            reader.ReadInt();

			m_Charges = reader.ReadInt();
		}
	}

    public class ShrinkTarget : Target
	{
		private readonly IShrinkTool _ShrinkTool;

        public ShrinkTarget(Mobile from, IShrinkTool shrinkTool)
            : base(10, false, TargetFlags.None)
		{
			_ShrinkTool = shrinkTool;

            from.SendMessage("Target the pet you wish to shrink.");
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
            if (!(targeted is BaseCreature pet)) // null check
            {
                from.SendMessage("That is not a pet!");
            }
            else if (!pet.Controlled)
            {
                from.SendMessage("You cannot not shrink wild creatures.");
            }
            else if (pet.ControlMaster != from)
            {
                from.SendMessage("That is not your pet.");
            }
            else if (pet.IsDeadPet)
            {
                from.SendMessage("You cannot shrink the dead!");
            }
            else if (pet.Summoned || pet.Allured || pet is BaseTalismanSummon)
            {
                from.SendMessage("You cannot shrink a summoned creature!");
            }
            else if (pet.Combatant != null && pet.InRange(pet.Combatant, 12) && pet.Map == pet.Combatant.Map || Spells.SpellHelper.CheckCombat(from))
            {
                from.SendMessage("You or your pet are engaged in combat; you cannot shrink it yet.");
            }
            else if ((pet is PackLlama || pet is PackHorse || pet is Beetle) && pet.Backpack != null && pet.Backpack.Items.Count > 0)
            {
                from.SendMessage("You must unload this pet's pack before it can be shrunk.");
            }
            else
			{
                IEntity p1 = new Entity(Serial.Zero, new Point3D(from.X, from.Y, from.Z), from.Map);
				IEntity p2 = new Entity(Serial.Zero, new Point3D(from.X, from.Y, from.Z + 50 ), from.Map);

                Effects.SendMovingParticles(p2, p1, ShrinkTable.Lookup(pet), 1, 0, true, false, 0, 3, 1153, 1, 0, EffectLayer.Head, 0x100);

				from.PlaySound(492);
				from.AddToBackpack(new ShrinkItem(pet));

				if (_ShrinkTool != null && _ShrinkTool.ShrinkCharges > 0)
                {
                    _ShrinkTool.ShrinkCharges--;
                }
            }
		}
    }
}
