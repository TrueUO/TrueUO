using Server.Mobiles;
using Server.Commands;
using Server.Targeting;
using Server.Regions;

namespace Server.Services.ShrinkSystem
{
	public class ShrinkCommands
	{
        public static void Initialize()
		{
			CommandHandlers.Register( "Shrink", AccessLevel.GameMaster, Shrink_OnCommand );
        }

        [Usage("Shrink")]
		[Description("Shrinks a creature.")]
		private static void Shrink_OnCommand(CommandEventArgs e)
		{
            if (e.Mobile is PlayerMobile from)
            {
                from.Target = new ShrinkTarget(from, null, true);
            }
        }
    }

	public class ShrinkTarget : Target
	{
		private IShrinkTool m_ShrinkTool;
		private bool m_StaffCommand;

		public ShrinkTarget(Mobile from, IShrinkTool shrinkTool, bool staffCommand)
            : base(10, false, TargetFlags.None)
		{
			m_ShrinkTool = shrinkTool;
			m_StaffCommand = staffCommand;

			from.SendMessage("Target the pet you wish to shrink.");
		}

		protected override void OnTarget( Mobile from, object target )
		{
			BaseCreature pet = target as BaseCreature;

			if (target is PlayerMobile || target is Item)
            {
                from.SendMessage("You cannot shrink that.");
            }
            else if (Spells.SpellHelper.CheckCombat(from))
            {
                from.SendMessage("You cannot shrink your pet while you are fighting.");
            }
            else if (pet == null)
            {
                from.SendMessage("That is not a pet!");
            }
            else if ((pet.BodyValue == 400 || pet.BodyValue == 401) && pet.Controlled == false)
            {
                from.SendMessage( "That person gives you a dirty look!" );
            }
            else if (pet.IsDeadPet)
            {
                from.SendMessage("You cannot shrink the dead!");
            }
            else if (pet.Summoned)
            {
                from.SendMessage("You cannot shrink a summoned creature!");
            }
            else if (!m_StaffCommand && pet.Combatant != null && pet.InRange(pet.Combatant, 12) && pet.Map == pet.Combatant.Map)
            {
                from.SendMessage("Your pet is fighting; you cannot shrink it yet.");
            }
            else if (pet.BodyMod != 0)
            {
                from.SendMessage("You cannot shrink your pet while it is polymorphed.");
            }
            else if (!m_StaffCommand && pet.Controlled == false)
            {
                from.SendMessage("You cannot not shrink wild creatures.");
            }
            else if (!m_StaffCommand && pet.ControlMaster != from)
            {
                from.SendMessage("That is not your pet.");
            }
            else if (!m_StaffCommand && ShrinkItem.IsPackAnimal(pet) && null != pet.Backpack && pet.Backpack.Items.Count > 0)
            {
                from.SendMessage("You must unload this pet's pack before it can be shrunk.");
            }
            else
			{
				if (pet.ControlMaster != from && !pet.Controlled)
				{
                    if (pet.Spawner is SpawnEntry se && se.UnlinkOnTaming)
					{
                        pet.Spawner.Remove(pet); 
						pet.Spawner = null;
					}

					pet.CurrentWayPoint = null;
					pet.ControlMaster = from;
					pet.Controlled = true;
					pet.ControlTarget = null;
					pet.ControlOrder = OrderType.Come;
					pet.Guild = null;
					pet.Delta(MobileDelta.Noto);
				}

				IEntity p1 = new Entity( Serial.Zero, new Point3D( from.X, from.Y, from.Z ), from.Map );
				IEntity p2 = new Entity( Serial.Zero, new Point3D( from.X, from.Y, from.Z + 50 ), from.Map );

				Effects.SendMovingParticles(p2, p1, ShrinkTable.Lookup( pet ), 1, 0, true, false, 0, 3, 1153, 1, 0, EffectLayer.Head, 0x100);
				from.PlaySound(492);
				from.AddToBackpack(new ShrinkItem(pet));

				if (!m_StaffCommand && null != m_ShrinkTool && m_ShrinkTool.ShrinkCharges > 0)
                {
                    m_ShrinkTool.ShrinkCharges--;
                }
            }
		}
	}
}
