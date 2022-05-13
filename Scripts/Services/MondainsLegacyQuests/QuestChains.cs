using System;

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        CovetousGhost = 1,
        HonorOfDeBoors = 2,
        ValleyOfOne = 3,
        MyrmidexAlliance = 4,
        EodonianAlliance = 5,
        FlintTheQuartermaster = 6
    }

    public class BaseChain
    {
        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[7][];

            Chains[(int)QuestChain.None] = new Type[] { };

            Chains[(int)QuestChain.CovetousGhost] = new[] { typeof(GhostOfCovetousQuest), typeof(SaveHisDadQuest), typeof(FathersGratitudeQuest) };
            Chains[(int)QuestChain.HonorOfDeBoors] = new[] { typeof(HonorOfDeBoorsQuest), typeof(JackTheVillainQuest), typeof(SavedHonorQuest) };
            Chains[(int)QuestChain.ValleyOfOne] = new[] { typeof(TimeIsOfTheEssenceQuest), typeof(UnitingTheTribesQuest) };
            Chains[(int)QuestChain.MyrmidexAlliance] = new[] { typeof(TheZealotryOfZipactriotlQuest), typeof(DestructionOfZipactriotlQuest) };
            Chains[(int)QuestChain.EodonianAlliance] = new[] { typeof(ExterminatingTheInfestationQuest), typeof(InsecticideAndRegicideQuest) };
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
