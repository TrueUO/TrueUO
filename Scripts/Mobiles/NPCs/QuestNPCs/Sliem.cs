using System;

namespace Server.Engines.Quests
{
    public class Sliem : MondainQuester
    {
        [Constructable]
        public Sliem()
            : base("Sliem", "the Fence")
        {
        }

        public Sliem(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[]
                {
                    typeof (UnusualGoods)
                };

        public override void InitBody()
        {
            Race = Race.Human;
            InitStats(100, 100, 25);
            Female = false;
            Body = 666;
            HairItemID = 16987;
            HairHue = 1801;
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
