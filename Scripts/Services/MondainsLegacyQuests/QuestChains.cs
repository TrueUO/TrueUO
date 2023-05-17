#region References
using System;
#endregion

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        CovetousGhost = 4,
        HonestBeggar = 6,
        Marauders = 8,
        UnfadingMemories = 15,
        DoughtyWarriors = 18,
        HonorOfDeBoors = 19,
        LaifemTheWeaver = 20,
        CloakOfHumility = 21,
        ValleyOfOne = 22,
        MyrmidexAlliance = 23,
        EodonianAlliance = 24,
        FlintTheQuartermaster = 25,
        RightingWrong = 28,
        Ritual = 29,
        Ritual2 = 30
    }

    public class BaseChain
    {
        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[16][];

            Chains[(int)QuestChain.None] = new Type[] { };

            Chains[(int)QuestChain.CovetousGhost] = new[] { typeof(GhostOfCovetousQuest), typeof(SaveHisDadQuest), typeof(FathersGratitudeQuest) };
            Chains[(int)QuestChain.HonestBeggar] = new[] { typeof(HonestBeggarQuest), typeof(ReginasThanksQuest) };
            Chains[(int)QuestChain.Marauders] = new[] { typeof(MaraudersQuest), typeof(TheBrainsOfTheOperationQuest), typeof(TheBrawnQuest), typeof(TheBiggerTheyAreQuest) };
            Chains[(int)QuestChain.UnfadingMemories] = new[] { typeof(UnfadingMemoriesOneQuest), typeof(UnfadingMemoriesTwoQuest), typeof(UnfadingMemoriesThreeQuest) };
            Chains[(int)QuestChain.DoughtyWarriors] = new[] { typeof(DoughtyWarriorsQuest), typeof(DoughtyWarriors2Quest), typeof(DoughtyWarriors3Quest) };
            Chains[(int)QuestChain.HonorOfDeBoors] = new[] { typeof(HonorOfDeBoorsQuest), typeof(JackTheVillainQuest), typeof(SavedHonorQuest) };
            Chains[(int)QuestChain.LaifemTheWeaver] = new[] { typeof(ShearingKnowledgeQuest), typeof(WeavingFriendshipsQuest), typeof(NewSpinQuest)};
            Chains[(int)QuestChain.CloakOfHumility] = new[] { typeof(TheQuestionsQuest), typeof(CommunityServiceMuseumQuest), typeof(CommunityServiceZooQuest), typeof(CommunityServiceLibraryQuest), typeof(WhosMostHumbleQuest) };
            Chains[(int)QuestChain.ValleyOfOne] = new[] { typeof(TimeIsOfTheEssenceQuest), typeof(UnitingTheTribesQuest) };
            Chains[(int)QuestChain.MyrmidexAlliance] = new[] { typeof(TheZealotryOfZipactriotlQuest), typeof(DestructionOfZipactriotlQuest) };
            Chains[(int)QuestChain.EodonianAlliance] = new[] { typeof(ExterminatingTheInfestationQuest), typeof(InsecticideAndRegicideQuest) };
            Chains[(int)QuestChain.FlintTheQuartermaster] = new[] { typeof(ThievesBeAfootQuest), typeof(BibliophileQuest) };
            Chains[(int)QuestChain.RightingWrong] = new[] { typeof(RightingWrongQuest2), typeof(RightingWrongQuest3), typeof(RightingWrongQuest4) };
            Chains[(int)QuestChain.Ritual] = new[] { typeof(RitualQuest.ScalesOfADreamSerpentQuest), typeof(RitualQuest.TearsOfASoulbinderQuest), typeof(RitualQuest.PristineCrystalLotusQuest) };
            Chains[(int)QuestChain.Ritual2] = new[] { typeof(RitualQuest.HairOfTheDryadQueen), typeof(RitualQuest.HeartOfTheNightTerror) };
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
