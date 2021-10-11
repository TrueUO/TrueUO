namespace Server.Items
{
    [Flipable(0x13B2, 0x13B1)]
    public class JukaBow : Bow
    {
        [Constructable]
        public JukaBow()
        { 
		}

        public JukaBow(Serial serial)
            : base(serial)
        { 
		}

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsModified => Slayer2 != SlayerName.None;

        public override bool CanEquip(Mobile from)
        {
            if (IsModified)
            {
                return base.CanEquip(from);
            }

            from.SendMessage("You cannot equip this bow until a bowyer modifies it.");
            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsModified)
            {
                from.SendMessage("That has already been modified.");
            }
            else if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1080058); // This must be in your backpack to use it.
            }
            else if (from.Skills[SkillName.Fletching].Base < 100.0)
            {
                from.SendMessage("Only a grandmaster bowyer can modify this weapon.");
            }
            else
            {
                from.BeginTarget(2, false, Targeting.TargetFlags.None, OnTargetGears);
                from.SendLocalizedMessage(1010614); // Which set of gears will you use?
            }
        }

        public void OnTargetGears(Mobile from, object targ)
        {
            Gears g = targ as Gears;

            if (g == null || !g.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1010623); // You must use gears.
            }
            else if (IsModified)
            {
                from.SendMessage("That has already been modified.");
            }
            else if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1080058); // This must be in your backpack to use it.
            }
            else if (from.Skills[SkillName.Fletching].Base < 100.0)
            {
                from.SendMessage("Only a grandmaster bowyer can modify this weapon.");
            }
            else
            {
                g.Consume();

                Hue = 0x453;
                Slayer2 = BaseRunicTool.GetRandomSlayer();

                from.SendMessage("You modify the bow.");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
