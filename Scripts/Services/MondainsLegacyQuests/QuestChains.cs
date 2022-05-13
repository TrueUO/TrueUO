using System;

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        CovetousGhost = 1,
        HonorOfDeBoors = 2,
        FlintTheQuartermaster = 3
    }

    public class BaseChain
    {
        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[4][];

            Chains[(int)QuestChain.None] = new Type[] { };

            Chains[(int)QuestChain.CovetousGhost] = new[] { typeof(GhostOfCovetousQuest), typeof(SaveHisDadQuest), typeof(FathersGratitudeQuest) };
            Chains[(int)QuestChain.HonorOfDeBoors] = new[] { typeof(HonorOfDeBoorsQuest), typeof(JackTheVillainQuest), typeof(SavedHonorQuest) };
            Chains[(int)QuestChain.FlintTheQuartermaster] = new[] { typeof(ThievesBeAfootQuest), typeof(BibliophileQuest) };
        }

        public Type CurrentQuest { get; set; }
        public Type Quester { get; set; }

        public BaseChain(Type currentQuest, Type quester)
        {
            CurrentQuest = currentQuest;
            Quester = quester;
        }
    }
}
