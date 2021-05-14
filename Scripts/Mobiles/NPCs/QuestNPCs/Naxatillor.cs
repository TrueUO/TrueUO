using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    public class TheArisenQuest : BaseQuest
    {
        public TheArisenQuest()
        {
            AddObjective(new SlayObjective(typeof(GargoyleShade), "Gargoyle Shade", 10));
            AddObjective(new SlayObjective(typeof(EffetePutridGargoyle), "Effete Putrid Gargoyle", 10));
            AddObjective(new SlayObjective(typeof(EffeteUndeadGargoyle), "Effete Undead Gargoyle", 10));

            AddReward(new BaseReward(typeof(NecklaceofDiligence), 1113137)); // Necklace of Diligence
        }

        public override bool AllObjectives => false;
        public override bool DoneOnce => true;

        /* The Arisen */
        public override object Title => 1112538;

        /* We need your assistance with a matter that is most grave. To the north,
		 * from within the Tomb of Kings, the Arisen are emerging each night to
		 * attack the Holy City.
		 * 
		 * Shortly after we unsealed the entrance to the Abyss, found in the depths
		 * of the Tomb, strange happenings began to occur. At first, there were only 
		 * a few reports of strange noises coming from the Tomb of Kings late at
		 * night. Investigating the Tomb during the daytime uncovered nothing unusual,
		 * so we sent someone to seek out the source of these noises one night. When
		 * morning came and he had not returned, we knew something was amiss.
		 * 
		 * We sent word to the Royal City asking for help, but the following night,
		 * unspeakable evil erupted from the entrance to the Tomb! A defense of the
		 * city was quickly marshaled, but the Arisen proved to be quite powerful.
		 * Unfortunately, they are also persistent, as every night since then, this
		 * city, the original birthplace of our people, has faced wave after wave of
		 * Arisen. We know not the cause of the attacks, nor the source, as
		 * investigations by daylight yield little. We have been hard pressed just to
		 * defend the Elders here, much less push the Arisen back into the Tomb.
		 * 
		 * We could use your help in this! Either help defend the Holy City at night,
		 * or enter the Tomb of Kings itself and seek out the Arisen at their source.
		 * It is your choice to make, as you know your own abilities best. If you
		 * decide to enter the Tomb of Kings, you'll need to speak the words "ord"
		 * and "an-ord" to pass the Serpent's Breath.<br><br>Succeed in this task,
		 * and I shall reward you well. */
        public override object Description => 1112539;

        /* To decide not to help would bring you great shame. */
        public override object Refuse => 1112540;

        /* Help defend the Holy City or head down into the tombs! */
        public override object Uncomplete => 1112541;

        /* You have proven yourself both brave and worthy! Know that you have both
		 * our gratitude and our blessing to enter the Abyss, if you so wish.
		 * 
		 * All who wish to enter must seek out the Shrine of Singularity to the
		 * North for further meditation. Only those found to be on the Sacred
		 * Quest will be allowed to enter the Abyss. I would advise that you seek
		 * out some of the ancient texts in the Holy City Museum, which you can find
		 * to the south, so that you might focus better while meditating at the
		 * Shrine. As promised, here is your reward. */
        public override object Complete => 1112543;

        // Good work! Now return to Naxatilor.
        public override int CompleteMessage { get { return 1112542; } }

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

    public class Naxatilor : MondainQuester
    {
        [Constructable]
        public Naxatilor()
            : base("Naxatillor", "The Seer")
        {
        }

        public Naxatilor(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => new[] { typeof(TheArisenQuest) };

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            CantWalk = true;
            Body = 666;
            HairItemID = 16987;
            HairHue = 1801;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());

            AddItem(new GargishClothChest(Utility.RandomNeutralHue()));
            AddItem(new GargishClothKilt(Utility.RandomNeutralHue()));
            AddItem(new GargishClothLegs(Utility.RandomNeutralHue()));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
