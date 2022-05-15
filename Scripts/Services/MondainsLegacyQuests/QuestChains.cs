#region References
using System;
#endregion

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        HonestBeggar = 1,
        DoughtyWarriors = 2,
        HonorOfDeBoors = 3,
        FlintTheQuartermaster = 4,
        AnimalTraining = 5,
        PaladinsOfTrinsic = 6,
        RightingWrong = 7,
        Ritual = 8,
        Ritual2 = 9
    }

    public class BaseChain
    {
        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[10][];

            Chains[(int)QuestChain.None] = new Type[] { };

            Chains[(int)QuestChain.HonestBeggar] = new[] { typeof(HonestBeggarQuest), typeof(ReginasThanksQuest) };
            Chains[(int)QuestChain.DoughtyWarriors] = new[] { typeof(DoughtyWarriorsQuest), typeof(DoughtyWarriors2Quest), typeof(DoughtyWarriors3Quest) };
            Chains[(int)QuestChain.HonorOfDeBoors] = new[] { typeof(HonorOfDeBoorsQuest), typeof(JackTheVillainQuest), typeof(SavedHonorQuest) };
            Chains[(int)QuestChain.FlintTheQuartermaster] = new[] { typeof(ThievesBeAfootQuest), typeof(BibliophileQuest) };
            Chains[(int)QuestChain.AnimalTraining] = new[] { typeof(TamingPetQuest), typeof(UsingAnimalLoreQuest), typeof(LeadingIntoBattleQuest), typeof(TeachingSomethingNewQuest) };
            Chains[(int)QuestChain.PaladinsOfTrinsic] = new[] { typeof(PaladinsOfTrinsic), typeof(PaladinsOfTrinsic2) };
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
