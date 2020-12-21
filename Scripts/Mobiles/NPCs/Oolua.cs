using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    
    public class Oolua : MondainQuester
    {
        [Constructable]
        public Oolua()
            : base("Lorekeeper Oolua", "the keeper of tradition")
        {
        }

        public Oolua(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(DreadhornQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            CantWalk = true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
