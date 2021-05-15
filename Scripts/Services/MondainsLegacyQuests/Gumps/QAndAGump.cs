using Server.Engines.Quests;
using Server.Network;
using System.Collections.Generic;

namespace Server.Gumps
{
    public class QAndAGump : Gump
    {
        private readonly Mobile m_From;
        private readonly QuestionAndAnswerObjective m_Objective;
        private readonly BaseQuest m_Quest;
        private readonly int m_Index;

        public QAndAGump(Mobile owner, BaseQuest quest)
            : base(160, 100)
        {
            m_From = owner;
            m_Quest = quest;
            Closable = false;
            Disposable = false;

            foreach (BaseObjective objective in quest.Objectives)
            {
                if (objective is QuestionAndAnswerObjective answerObjective)
                {
                    m_Objective = answerObjective;
                    break;
                }
            }

            if (m_Objective == null)
                return;

            QuestionAndAnswerEntry entry = m_Objective.GetRandomQandA();

            if (entry == null)
                return;

            AddImage(0, 0, 0x4CC);
            AddImage(40, 78, 0x5F);
            AddImageTiled(49, 87, 301, 3, 0x60);
            AddImage(350, 78, 0x61);

            object answer = entry.Answers[Utility.Random(entry.Answers.Length)];

            List<object> selections = new List<object>(entry.WrongAnswers);
            m_Index = Utility.Random(selections.Count); //Gets correct answer
            selections.Insert(m_Index, answer);

            AddHtmlLocalized(30, 40, 340, 36, entry.Question, 0x0, false, false); //question

            for (int i = 0; i < selections.Count; i++)
            {
                object selection = selections[i];

                AddImage(50, 112 + (i * 32), 0x8B0);

                AddButton(49, 111 + (i * 32), 2224, 2224, selection == answer ? 1 : 0, GumpButtonType.Reply, 0);

                if (selection is int iSelection)
                    AddHtmlLocalized(80, 109 + (i * 32), 275, 36, iSelection, 0x0, false, false);
                else
                    AddHtml(80, 109 + (i * 32), 275, 36, string.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", 0x0, selection), false, false);
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 1) //correct answer
            {
                m_Objective.Update(null);

                if (m_Quest.Completed)
                {
                    m_From.PlaySound(0x5B5);
                    m_From.PlaySound(m_From.Female ? 0x30B : 0x41A);
                    m_Quest.OnCompleted();
                    m_From.SendGump(new MondainQuestGump(m_Quest, MondainQuestGump.Section.Complete, false, true));
                }
                else
                {
                    m_From.SendGump(new QAndAGump(m_From, m_Quest));
                }
            }
            else
            {
                m_From.PlaySound(0x5B3);
                m_From.PlaySound(m_From.Female ? 0x310 : 0x41F);
                m_From.SendGump(new MondainQuestGump(m_Quest, MondainQuestGump.Section.Failed, false, true));
                m_Quest.OnResign(false);
            }
        }
    }
}
