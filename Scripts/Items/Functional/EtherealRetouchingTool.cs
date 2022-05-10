using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class EtherealRetouchingTool : Item
    {
        public override int LabelNumber => 1113814;  // Retouching Tool

        [Constructable]
        public EtherealRetouchingTool()
            : base(0x42C6)
        {
        }

        public EtherealRetouchingTool(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.Target = new InternalTarget(this);
                from.SendLocalizedMessage(1113815); // Target the ethereal mount you wish to retouch.
            }
            else
            {
                from.SendLocalizedMessage(1042010); // You must have the object in your backpack to use it.
            }
        }

        private class InternalTarget : Target
        {
            private readonly EtherealRetouchingTool m_Tool;

            public InternalTarget(EtherealRetouchingTool tool)
                : base(-1, false, TargetFlags.None)
            {
                m_Tool = tool;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (!m_Tool.IsChildOf(from.Backpack))
                {
                    from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                }
                else if (targeted is EtherealMount mount)
                {
                    if (!mount.IsChildOf(from.Backpack))
                    {
                        from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
                    }
                    else if (mount is EtherealWarBoar)
                    {
                        from.SendLocalizedMessage(1071117); // You cannot use this item for it.
                    }
                    else 
                    {
                        if (mount.Transparent)
                        {
                            from.SendLocalizedMessage(1113816); // Your ethereal mount's body has been solidified.
                        }
                        else
                        {
                            from.SendLocalizedMessage(1113817); // Your ethereal mount's transparency has been restored.
                        }

                        mount.Transparent = !mount.Transparent;
                        mount.InvalidateProperties();
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1046439); // That is not a valid target.
                }
            }
        }

        public static void AddProperty(EtherealMount mount, ObjectPropertyList list)
        {
            list.Add(1113818, mount.Transparent ? "#1078520" : "#1153298");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
