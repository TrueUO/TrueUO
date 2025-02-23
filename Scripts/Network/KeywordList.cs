namespace Server
{
	public class KeywordList
	{
		private int[] _Keywords;
		private int _Count;

		public KeywordList()
		{
			_Keywords = new int[8];
			_Count = 0;
		}

		public int Count => _Count;

		public bool Contains(int keyword)
		{
			bool contains = false;

			for (int i = 0; !contains && i < _Count; ++i)
			{
				contains = keyword == _Keywords[i];
			}

			return contains;
		}

		public void Add(int keyword)
		{
			if (_Count + 1 > _Keywords.Length)
			{
				int[] old = _Keywords;
				_Keywords = new int[old.Length * 2];

				for (int i = 0; i < old.Length; ++i)
				{
					_Keywords[i] = old[i];
				}
			}

			_Keywords[_Count++] = keyword;
		}

		private static readonly int[] _EmptyInts = [];

		public int[] ToArray()
		{
			if (_Count == 0)
			{
				return _EmptyInts;
			}

			int[] keywords = new int[_Count];

			for (int i = 0; i < _Count; ++i)
			{
				keywords[i] = _Keywords[i];
			}

			_Count = 0;

			return keywords;
		}
	}
}
