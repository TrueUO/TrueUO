using Server.Network;
using Server.Network.Packets;

namespace Server.Menus
{
	public class QuestionMenu : IMenu
	{
		private readonly string[] _Answers;

		private readonly int _Serial;
		private static int _NextSerial;

		int IMenu.Serial => _Serial;

		int IMenu.EntryLength => _Answers.Length;

		public string Question { get; set; }

		public string[] Answers => _Answers;

		public QuestionMenu(string question, string[] answers)
		{
			Question = question;
			_Answers = answers;

			do
			{
				_Serial = ++_NextSerial;
				_Serial &= 0x7FFFFFFF;
			}
			while (_Serial == 0);
		}

		public virtual void OnCancel(NetState state)
		{ }

		public virtual void OnResponse(NetState state, int index)
		{ }

		public void SendTo(NetState state)
		{
			state.AddMenu(this);
			state.Send(new DisplayQuestionMenuPacket(this));
		}
	}
}
