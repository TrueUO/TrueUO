using Server.Items;
using System;
using System.Collections.Generic;

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

            CantWalk = true;
            Female = true;
            Race = Race.Human;
            Body = 0x191;

            Hue = 0;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            SetWearable(new DeathShroud());
            SetWearable(new SmithHammer(), 2500);
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
            AddObjective(new ObtainObjective(typeof(BarristersRobe), "A Barrister's Robe", 1, 0x1F03, 0, 1367));
            AddObjective(new ObtainObjective(typeof(AnkhNecklace), "Ankh Necklace", 1, 0x3BB5, 0, 2498));
            AddObjective(new ObtainObjective(typeof(VialOfBlood), "A Vile of Blood", 1, 0xE24, 0, 44));
            AddObjective(new ObtainObjective(typeof(Illumination), "An Illumination", 1, 0x1C13, 0, 2747));
            AddObjective(new ObtainObjective(typeof(MugOfPurpleAle), "Mug of Purple Ale", 1, 0x9EF, 0, 1158));
            AddObjective(new ObtainObjective(typeof(EmbroideredPillow), "An Embroidered Pillow", 1, 0x9E1D, 0, 2125));
            AddObjective(new ObtainObjective(typeof(BrokenFellowshipSword), "A Broken Fellowship Sword", 1, 0xA33F, 0, 2117));
            AddObjective(new ObtainObjective(typeof(ShornWool), "Shorn Wool", 1, 0xDFE, 0, 2051));
            AddReward(new BaseReward(null, 1, "A Virtue Rune", VirtueRune.GetRandomVirtueID(), 0));
        }

        public override object Title => "The Treasured Sacrifice";

        public override object Description => "With the runes of Virtue destroyed the Shrines of Virtue quickly fell to the Fellowship menace. The only hope to rebuild them is to restore the protective power the runes once provided. You encounter the ghost of the famed tinker Julia who has uncoverd a way to rekindle that protection and allow the Shrines to be rebuilt. In order to assist you must accomplish two goals. First you must collect a vial of blood from where Julia was slain to reconnect her spirit to this plane. Second you must obtain charms from 7 other companions. With their help the shrines can be rebuilt!";

        public override object Refuse => "The quest for virtue is not for one that is faint of heart. Return when you are up for the adventure.";

        public override object Uncomplete => "You must visit the companions and obtain a charm from each of them so Julia can attempt to re-craft the Runes of Virtue and restore the Shrines of Britainnia! Mariah is overcome with insanity from the tetrahedron and is resting at a healer in Moonglow. Iolo is tending to those in need at the poorhouses South of Britain. Geoffrey is coordinating response to the Fellowship from The Hand of Death in Jhelom. Jaana is defending those accused of theft at the Court of Truth in Yew. Dupre is telling stories of victory at the Keg and Anchor in Trinsic. Shamino's spirit can be felt at his statue in Skara Brae. Katrina is helping some simple sheep farmers in New Magincia. The troll still torment the bridge north of Vesper and east of Minoc near where Julia was killed.";

        public override object Complete => "With your help, Julia has collected charms from all the companions. Your sacrifice of these powerful relics does not go unnoticed. Julia smiles a ghostly grin and you feel a warmth about the air as she begins to bind the charms to ethereal plane.";

        public override void GiveRewards()
        {
            base.GiveRewards();

            var virtue = new KeyValuePair<string, int>();

            foreach (var x in VirtueRune.virtueList)
            {
                BaseReward first = null;

                for (var index = 0; index < Rewards.Count; index++)
                {
                    var reward = Rewards[index];

                    first = reward;
                }

                if (first != null && x.Value == first.Image)
                {
                    virtue = x;
                    break;
                }
            }

            Owner.AddToBackpack(new VirtueRune(virtue.Key, virtue.Value));
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
