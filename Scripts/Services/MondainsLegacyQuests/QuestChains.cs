using System;

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None = 0,

        DoughtyWarriors = 1,
        HonorOfDeBoors = 2,
        FlintTheQuartermaster = 3,
        PaladinsOfTrinsic = 4,
        RightingWrong = 5,
        Ritual = 6,
        Ritual2 = 7
    }

    public class BaseChain
    {
        public static Type[][] Chains { get; }

        static BaseChain()
        {
            Chains = new Type[8][];

            Chains[(int)QuestChain.None] = new Type[] { };

            Chains[(int)QuestChain.DoughtyWarriors] = new[] { typeof(DoughtyWarriorsQuest), typeof(DoughtyWarriors2Quest), typeof(DoughtyWarriors3Quest) };
            Chains[(int)QuestChain.HonorOfDeBoors] = new[] { typeof(HonorOfDeBoorsQuest), typeof(JackTheVillainQuest), typeof(SavedHonorQuest) };
            Chains[(int)QuestChain.FlintTheQuartermaster] = new[] { typeof(ThievesBeAfootQuest), typeof(BibliophileQuest) };
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
