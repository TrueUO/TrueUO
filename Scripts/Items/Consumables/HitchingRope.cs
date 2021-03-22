using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class HitchingRope : Item
    {
        public override int LabelNumber => 1071124; //  hitching rope

        [Constructable]
        public HitchingRope()
            : base(0x14F8)
        {
            Hue = 2418;
            Weight = 10;
        }

        public HitchingRope(Serial serial)
            : base(serial)
        {
        }
        
        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack) || from.InRange(GetWorldLocation(), 2) && Movable)
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1071159); // Select the hitching post you want to supply hitching rope.
                from.Target = new InternalTarget(this);
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        private class InternalTarget : Target
        {
            private readonly HitchingRope m_Rope;

            public InternalTarget(HitchingRope rope)
                : base(-1, false, TargetFlags.None)
            {
                m_Rope = rope;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Rope.Deleted)
                    return;

                if (targeted is HitchingPost postItem)
                {
                    var maxuse = postItem.Replica ? 15 : 30;

                    if (postItem.UsesRemaining >= maxuse)
                    {
                        from.SendLocalizedMessage(1038293); // It looks almost new.
                    }
                    else if (postItem.Replica && postItem.Charges <= 0 && postItem.UsesRemaining == 0)
                    {
                        from.SendLocalizedMessage(1071157); // This hitching post is damaged. You can't use it any longer.
                    }
                    else
                    {
                        postItem.Charges -= 1;
                        postItem.UsesRemaining = maxuse;

                        m_Rope.Delete();

                        from.SendLocalizedMessage(1071158, postItem.Name); // You have successfully resupplied the ~1_POST~ with the hitching rope.
                    }
                }
                else if (targeted is PetCastleAddon pc)
                {
                    if (pc.UsesRemaining >= 30)
                    {
                        from.SendLocalizedMessage(1038293); // It looks almost new.
                    }
                    else
                    {
                        pc.UsesRemaining = 30;

                        m_Rope.Delete();

                        from.SendLocalizedMessage(1071158, pc.Name); // You have successfully resupplied the ~1_POST~ with the hitching rope.
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1071117); // You cannot use this item for it.
                }
            }
        }
    }
}
