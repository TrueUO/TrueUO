using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class Shamino : MondainQuester
    {
        [Constructable]
        public Shamino()
            : base("A Statue Of Shamino")
        {
        }

        public Shamino(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(TheSpiritRangerQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            CantWalk = true;
            Female = false;
            Race = Race.Human;
            Body = 0x190;

            Hue = 2955;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            SetWearable(new StuddedArms(), 2955);
            SetWearable(new StuddedChest(), 2955);
            SetWearable(new StuddedGloves(), 2955);
            SetWearable(new ThighBoots(), 2955);
            SetWearable(new Cloak(), 2955);
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

    public class TheSpiritRangerQuest : BaseQuest
    {
        public TheSpiritRangerQuest()
        {
            AddObjective(new ObtainObjective(typeof(RangersNecklace), "Ranger's Necklace", 1, 0x3BB5, 0, 2498));
            AddReward(new BaseReward(typeof(AnkhNecklace), 1, "Ankh Necklace", 0x3BB5, 2498));
        }

        public override object Title => "The Spirit Ranger";

        public override object Description => "You come upon a statue Ranger of the Ranger King Shamino, immediately your spirit is one with the statue, you know what must be done. A ranger has fallen deep inside Hythloth. Until the spirit of the ranger can be returned to the ethereal void there is great unrest in the spirit world. Without a word being spoken you know that you must find the fallen ranger deep inside Hythloth and return to Shamino's statue.";

        public override object Refuse => "The statue goes cold and silent. Your spirit is weak.";

        public override object Uncomplete => "Your spirit guides you. Venture deep inside Hythloth and find the fallen ranger. Only then can the ranger's spirit be at peace.";

        public override object Complete => "Deep inside Hythloth you encounter vile horrors! Hunched daemons who barely fit inside the stone walls of the dungeon attack any who dare violate their den. Despite these horrors you prevail and recover an ankh pendant from the fallen ranger's corpse. You've returned the pendant to Shamino's statue. As you place the pendant on the statue a great wave of relief washes over you. The pendant is now yours and your spirit is joined with the ethereal void!";

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
