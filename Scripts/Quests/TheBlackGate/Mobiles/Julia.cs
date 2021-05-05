using Server.Items;
using System;

namespace Server.Engines.Quests
{
    public class Julia : MondainQuester
    {
        [Constructable]
        public Julia()
            : base("Julia", "the Sacrificed")
        {
        }

        public Julia(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(TheTreasuredSacrificeQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            Race = Race.Human;
            Body = 0x191;

            Hue = 0;
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

    public class TheTreasuredSacrificeQuest : BaseQuest
    {
        public TheTreasuredSacrificeQuest()
        {
            AddObjective(new CollectionsObtainObjective(typeof(ShepherdsCrookOfHumility), "Shepherd's Crook of Humility (Replica)", 1));
            AddReward(new BaseReward(1075852)); // A better understanding of Britannia's people
        }

        public override object Title => "The Treasured Sacrifice";

        public override object Description => "With the runes of Virtue destroyed the Shrines of Virtue quickly fell to the Fellowship menace. The only hope to rebuild them is to restore the protective power the runes once provided. You encounter the ghost of the famed tinker Julia who has uncoverd a way to rekindle that protection and allow the Shrines to be rebuilt. In order to asist you must accomplish two goals. First you must collect a vial of blood from where Julia was slain to reconnect her spirit to this plane. Second you must obtain charms from 7 other companions. With their help the shrines can be rebuilt!";

        public override object Refuse => "The quest for virtue is not for one that is faint of heart. Return when you are up for the adventure.";

        public override object Uncomplete => "You must visit the companions and obtain a charm from each of them so Julia can attempt to recraft the Runes of Virtue and restore the Shrines of Britainnia! Mariah is overcome with insanity from the tetrahedron and is resting at a healer in Moonglow. Iolo is tending to those in need at the poorhouses South of Britain. Geoffrey is coordinating response to the Fellowship from The Hand of Death in Jhelom. Jaana is defending those accused of theft at the Court of Truth in Yew. Dupre is telling stories of victory at the Keg and Anchor in Trinsic. Shamino's spirit can be felt at his statue in Skara Brae. Katrina is helping some simple sheep farmers in New Magincia. The troll still torment the bridge north of Vesper and east of Minoc near where Julia was killed.";

        public override object Complete => "With your help, Julia has collected charms from all the companions. Your sacrifice of these powerful relics does not go unnoticed. Julia smiles a ghostly grin and you feel a warmth about the air as she begins to bind the charms to ethereal plane.";

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
