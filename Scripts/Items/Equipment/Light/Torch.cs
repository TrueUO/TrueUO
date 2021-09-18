using System;

namespace Server.Items
{
    public class Torch : BaseEquipableLight
    {
        public override int LitItemID => 0xA12;
        public override int UnlitItemID => 0xF6B;

        public override int LitSound => 0x54;
        public override int UnlitSound => 0x4BB;

        [Constructable]
        public Torch()
            : base(0xF6B)
        {
            Duration = Burnout ? TimeSpan.FromMinutes(30) : TimeSpan.Zero;

            Burning = false;
            Light = LightType.Circle300;
            Weight = 1.0;
        }

        public Torch(Serial serial)
            : base(serial)
        {
        }

        public override void OnAdded(object parent)
        {
            if (parent is Mobile mobile)
            {
                var holding = mobile.FindItemOnLayer(Layer.TwoHanded);

                if (holding is Torch && Burning)
                {
                    Mobiles.MeerMage.StopEffect(mobile, true);
                    SwarmContext.CheckRemove(mobile);
                }
            }
        }

        public override void Ignite()
        {
            base.Ignite();

            if (Parent is Mobile mobile)
            {
                var holding = mobile.FindItemOnLayer(Layer.TwoHanded);

                if (holding is Torch && Burning)
                {
                    Mobiles.MeerMage.StopEffect(mobile, true);
                    SwarmContext.CheckRemove(mobile);
                }
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
    }
}
