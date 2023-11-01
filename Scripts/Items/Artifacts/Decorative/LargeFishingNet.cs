using Server.Targeting;
using System;

namespace Server.Items
{
    [Flipable(7845, 7846)]
    public class LargeFishingNet : Item
    {
        public override bool IsArtifact => true;
        public override int LabelNumber => 1149955;

        [Constructable]
        public LargeFishingNet()
            : base(7845)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.SendMessage("Target a corpse you'd like to net.");
                from.BeginTarget(10, false, TargetFlags.None, Net_OnTarget);
            }
        }

        public void Net_OnTarget(Mobile from, object targeted)
        {
            if (targeted is Corpse corpse && (corpse.Owner == null || !corpse.Owner.Player))
            {
                if (SOS.ValidateDeepWater(corpse.Map, corpse.X, corpse.Y))
                {
                    from.Animate(12, 5, 1, true, false, 0);
                    Timer.DelayCall(TimeSpan.FromSeconds(0.5), new TimerStateCallback(MoveCorpse), new object[] { corpse, from });
                }
                else
                    from.SendLocalizedMessage(1010485); // You can only use this in deep water!
            }
            else
                from.SendMessage("You can only net corpses!");
        }

        public void MoveCorpse(object o)
        {
            object[] ojs = (object[])o;

            if (ojs[0] is Corpse c && ojs[1] is Mobile from)
            {
                c.MoveToWorld(from.Location, from.Map);
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1041645); // recovered from a shipwrecklist
        }

        public LargeFishingNet(Serial serial)
            : base(serial)
        {
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
    }

}
