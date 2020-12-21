using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    
    public class Koole : MondainQuester
    {
        [Constructable]
        public Koole()
            : base("Koole", "the arcanist")
        {
        }

        public Koole(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(DisciplineQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
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
