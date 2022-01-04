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
				if (0 == m_Charges || 0 == (m_Charges = value))
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
}
