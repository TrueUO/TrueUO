using Server.Items;
using System;

namespace Server.Engines.Quests
{
   
    public class Rollarn : MondainQuester
    {
        [Constructable]
        public Rollarn()
            : base("Lorekeeper Rollarn", "the keeper of tradition")
        {
        }

        public Rollarn(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new Type[]
                {
                    typeof(WarriorsOfTheGemkeeperQuest)
                };
        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
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
            reader.ReadInt();
        }
    }

    public class PersonalLetterAhie : BaseQuestItem
    {
        [Constructable]
        public PersonalLetterAhie()
            : base(0x14ED)
        {
        }

        public PersonalLetterAhie(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1073128;// A personal letter addressed to: Ahie
        public override int Lifespan => 1800;
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
