using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class Mariah : MondainQuester
    {
        [Constructable]
        public Mariah()
            : base("Mariah")
        {
        }

        public Mariah(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(TheHonestyInInsanityQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            CantWalk = true;
            Female = true;
            Race = Race.Human;

            Hue = 33770;
            HairItemID = 0x203C;
            HairHue = 1129;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Robe());
            AddItem(new Shoes(443));
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

    public class TheHonestyInInsanityQuest : BaseQuest
    {
        public override bool DoneOnce => true;

        public TheHonestyInInsanityQuest()
        {
            AddObjective(new ObtainObjective(typeof(CompassionSage), "Compassion Sage", 1, 0x1844, 0, 66));
            AddObjective(new ObtainObjective(typeof(RawGinseng), "Raw Ginseng", 1, 0x18EB));

            AddReward(new BaseReward(typeof(Illumination), "An Illumination", 0x1C13, 2747));
        }

        public override object Title => "The Honesty in Insanity";

        public override object Description => "You are told by the healers that Mariah's condition is dire. After utilizing mana corrupted by the tetrahedron, she has been overcome with insane and unintelligible ramblings. The healers tell you her only hope for a cure is salve made from compassion sage carried by Controllers deep in the exodus dungeon and raw ginseng growing in the far northern forests of the continent.";

        public override object Refuse => "Mariah's fate will rest in those who seek the truth in what Mariah has discovered in the depths of her insanity.";

        public override object Uncomplete => "You must find compassion sage and raw ginseng before it is too late! Venture to the Exodus Dungeon in Ilshenar, there you will find Compassion Sage carried by Controllers. Raw ginseng can be found growing in the northern forest of Britannia.";

        public override object Complete => "The healers start right away on preparing the salve. Mariah's condition improves with each passing moment. She thanks you immensely for your service during her time of need. While suffering maladies of the tetrahedron Mariah has drafted Illuminations from the Book of Truth. She gives you one with smile, \"Be true to the origins of Britannia and you will succeed in your endeavor.\"";

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
