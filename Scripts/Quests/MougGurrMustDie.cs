using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    public class SayonaraSzavetraQuest : BaseQuest
    {
        public SayonaraSzavetraQuest()
        {
            AddObjective(new SlayObjective(typeof(Szavetra), "szavetra", 1, "Sanctuary"));

            AddReward(new BaseReward(typeof(RewardBox), 1072584));
        }

        public override QuestChain ChainID => QuestChain.MiniBoss;
        /* Sayonara, Szavetra */
        public override object Title => 1072375;
        /* Hmm, maybe you aren't entirely worthless.  I suspect a demoness of Szavetra's calibre will tear you 
        apart ...  We might as well find out.  Kill the succubus, yada yada, and you'll be richly rewarded. */
        public override object Description => 1072578;
        /* Hah!  I knew you couldn't handle it. */
        public override object Refuse => 1072579;
        /* Hahahaha!  I can see the fear in your eyes.  Pathetic.  Szavetra is waiting for you. */
        public override object Uncomplete => 1072581;
        /* Amazing!  Simply astonishing ... you survived.  Well, I supposed I should indulge your avarice 
        with a reward.*/
        public override object Complete => 1072582;
        public override bool CanOffer()
        {
            return MondainsLegacy.Sanctuary;
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
