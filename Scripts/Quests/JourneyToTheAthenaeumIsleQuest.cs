using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
    public class JourneyToTheAthenaeumIsleQuest : BaseQuest
    {
        // Journey to the Athenaeum Isle
        public override object Title => 1150929;

        /*Greetings, adventurer. <br><br>	As you know, my people have suffered the 
        * incessant onslaught of the Void and its minions for as long as Gargish 
        * history exists. Protecting Ter Mur from the darkness, and its desire to
        * consume the land completely, is a burden passed down from one ruler to another
        * upon ascension to the throne.  During my rule, I have been more successful 
        * than my predecessors but, now, I fear that the greatest evil both myself and 
        * my people have ever faced is about to return.<br><br>	Long ago, Ter Mur was
        * assaulted by the most formidable and horrid servant of the Void it had ever faced.
        * Called Scelestus the Defiler, this daemon proved invincible to any weapon or spell
        * that was utilized against him. I was unable to defeat him and was forced to 
        * imprison him instead. Sadly, my own daughter was caught in the spell and stands 
        * imprisoned next to the daemon. It has been this way for a thousand years now.
        * <br><br>	I have received word that the isle which houses the daemon, Athenaeum
        * Isle, is once again swarming with daemons. Based on the description provided to 
        * me, I believe these are the minions of the Defiler himself. They have no doubt 
        * crawled out of the dark in anticipation of their master’s return. In truth, the
        * prison I placed him within will not last forever.<br><br>	I ask that you journey
        * to the southwestern flight tower, adventurer, and head further southwest towards
        * the shore. Near the water's edge, you will find an ancient teleport site which 
        * will transport you to the isle. Once there, please slay as many of these monsters
        * as you can. Additionally, please keep your eye out for any documents that you may
        * discover. This isle was the former home of our Great Library and, when it fell, 
        * not all of the documents and books were able to be taken to the new location here 
        * in the Royal City.<br><br>	Slay the beasts and return to me any documents that 
        * you acquire.<br><br>	Be careful, and go with honor.*/
        public override object Description => 1150902;

        // Understood. Perhaps you are not as brave as I initially thought. Be on your way, then.
        public override object Refuse => 1150930;

        // You have returned. Did you manage to slay the beasts and obtain any documents that may be of interest?
        public override object Uncomplete => 1150931;

        /*You have returned! I cannot thank you enough for the service you have done me, 
        * adventurer. <br><br>	The documents that you have retrieved may seem unimportant 
        * to you, as they are naught but random letters and doctrines. But they each 
        * represent an echo of the past, musings of our ancestors. I had always meant to 
        * return to the former library and retrieve all that I could, but I had thought they
        * were safe, gathering dust in the ruins. I will immediately have these cleaned and
        * placed in the Great Library here in the Royal City.<br><br>	As thanks, I offer 
        * you this book. It is the chronicle of my life, of the arrival of the Defiler, and 
        * a history of my people. In hopes that you will be granted further understanding of
        * the impending danger we suffer, I offer it to you as a gesture of friendship and 
        * goodwill.<br><br>	Thank you again, on behalf of the Gargoyle people. I may have
        * need of your assistance at another time, should you be willing to come to my aid 
        * again.<br><br>	Until then, farewell.*/
        public override object Complete => 1150903;      

        public JourneyToTheAthenaeumIsleQuest()
        {
            AddObjective(new SlayObjective(typeof(MinionOfScelestus), "Minion of Scelestus", 10));

            for (int i = 0; i < m_Types.Length; i++)
            {
                ObtainObjective obtain = new ObtainObjective(m_Types[i], m_Names[i], 1);

                AddObjective(obtain);
            }

            AddReward(new BaseReward(typeof(ChronicleOfTheGargoyleQueen1), 1, "Chronicle of the Gargoyle Queen Vol. I"));
        }

        public override bool RenderObjective(MondainQuestGump gump, bool offer)
        {
            int offset = 172;

            BaseObjective first = null;

            for (var index = 0; index < Objectives.Count; index++)
            {
                var o = Objectives[index];

                if (o is SlayObjective)
                {
                    first = o;
                    break;
                }
            }

            SlayObjective slay = first as SlayObjective;            

            for (int i = 0; i < Objectives.Count - 1; i++)
            {
                if (i != 0 && i % 3 == 0)
                {
                    gump.SecObjectivesButtons();
                    offset = 172;
                }

                if (i == 0)
                {
                    gump.AddHtmlLocalized(98, offset, 312, 16, 1072204, 0x15F90, false, false); // Slay

                    if (slay != null)
                    {
                        gump.AddLabel(133, offset, 0x481, slay.MaxProgress.ToString()); // Count
                        gump.AddLabel(163, offset, 0x481, slay.Name); // Name

                        offset += 16;

                        if (!offer)
                        {
                            gump.AddHtmlLocalized(103, offset, 120, 16, 3000087, 0x15F90, false, false); // Total			
                            gump.AddLabel(223, offset, 0x481, slay.CurProgress.ToString()); // %current progress%

                            offset += 16;
                        }
                    }
                }

                gump.AddHtmlLocalized(98, offset, 312, 80, 1150933 + i, 0x15F90, false, false);
                offset += 80;
            }

            return true;
        }

        private readonly Type[] m_Types =
        {
                typeof(ChallengeRite),          typeof(AnthenaeumDecree),       typeof(LetterFromTheKing),
                typeof(OnTheVoid),              typeof(ShilaxrinarsMemorial),   typeof(ToTheHighScholar),
                typeof(ToTheHighBroodmother),   typeof(ReplyToTheHighScholar),  typeof(AccessToTheIsle),
                typeof(InMemory)
        };

        private readonly string[] m_Names =
        {
                "Obtain Gargish Document - Challenge Rite",             "Obtain Gargish Document - Athenaeum Decree",           "Obtain Gargish Document - Letter from the King",
                "Obtain Gargish Document - On the Void",                "Obtain Gargish Document - Shilaxrinar's Memorial",     "Obtain Gargish Document - To the High Scholar",
                "Obtain Gargish Document - To the High Broodmother",    "Obtain Gargish Document - Reply to the High Scholar",  "Obtain Gargish Document - Access to the Isle",
                "Obtain Gargish Document - In Memory"
        };
    }
}
