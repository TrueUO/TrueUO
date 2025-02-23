using Server.Menus;

namespace Server.Network.Packets
{
    public sealed class DisplayQuestionMenuPacket : Packet
    {
        public DisplayQuestionMenuPacket(QuestionMenu menu)
            : base(0x7C)
        {
            EnsureCapacity(256);

            m_Stream.Write(((IMenu)menu).Serial);
            m_Stream.Write((short)0);

            string question = menu.Question;

            if (question == null)
            {
                m_Stream.Write((byte)0);
            }
            else
            {
                int questionLength = question.Length;
                m_Stream.Write((byte)questionLength);
                m_Stream.WriteAsciiFixed(question, questionLength);
            }

            string[] answers = menu.Answers;

            int answersLength = (byte)answers.Length;

            m_Stream.Write((byte)answersLength);

            for (int i = 0; i < answersLength; ++i)
            {
                m_Stream.Write(0);

                string answer = answers[i];

                if (answer == null)
                {
                    m_Stream.Write((byte)0);
                }
                else
                {
                    int answerLength = answer.Length;
                    m_Stream.Write((byte)answerLength);
                    m_Stream.WriteAsciiFixed(answer, answerLength);
                }
            }
        }
    }
}
