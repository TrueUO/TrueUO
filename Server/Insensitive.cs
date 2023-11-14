#region References
using System.Collections;
#endregion

namespace Server
{
	public static class Insensitive
	{
		private static readonly IComparer _Comparer = CaseInsensitiveComparer.Default;

		public static IComparer Comparer => _Comparer;

		public static int Compare(string a, string b)
		{
			return _Comparer.Compare(a, b);
		}

		public static bool Equals(string a, string b)
		{
			if (a == null && b == null)
			{
				return true;
			}

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            return _Comparer.Compare(a, b) == 0;
		}

		public static bool StartsWith(string a, string b)
		{
			if (a == null || b == null || a.Length < b.Length)
			{
				return false;
			}

			return _Comparer.Compare(a.Substring(0, b.Length), b) == 0;
		}

		public static bool Contains(string a, string b)
		{
			if (a == null || b == null || a.Length < b.Length)
			{
				return false;
			}

			a = a.ToLower();
			b = b.ToLower();

			return a.IndexOf(b) >= 0;
		}
	}
}
