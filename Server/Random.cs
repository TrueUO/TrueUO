using System;
using System.Numerics;

namespace Server
{
	/// <summary>
	///     Handles random number generation.
	/// </summary>
	public static class RandomImpl
	{
		public static Random Random => Random.Shared;

		private static bool Validate<N>(ref N min, ref N max) where N : INumber<N>
		{
			if (min == max)
			{
				return false;
			}

			if (min > max)
			{
				(min, max) = (max, min);
			}

			return true;
		}

		public static bool GetBool()
		{
			return Random.Next() % 2 != 0;
		}

        public static short GetShort(short min, short max, bool maxInclusive = false)
		{
			if (Validate(ref min, ref max))
			{
				if (maxInclusive)
				{
					return (short)(min + Random.Next(max - min + 1));
				}

				return (short)(min + Random.Next(max - min));
			}

			return min;
		}

        public static int GetInt(int max, bool maxInclusive = false)
		{
			return GetInt(0, max, maxInclusive);
		}

		public static int GetInt(int min, int max, bool maxInclusive = false)
		{
			if (Validate(ref min, ref max))
			{
				if (maxInclusive)
				{
					return min + Random.Next(max - min + 1);
				}

				return min + Random.Next(max - min);
			}

			return min;
		}

        public static long GetLong(long min, long max, bool maxInclusive = false)
		{
			if (Validate(ref min, ref max))
			{
				if (maxInclusive)
				{
					return min + Random.NextInt64(max - min + 1L);
				}

				return min + Random.NextInt64(max - min);
			}

			return min;
		}

        public static double GetDouble(bool maxInclusive = false)
		{
			return GetDouble(1d, maxInclusive);
		}

		public static double GetDouble(double max, bool maxInclusive = false)
		{
			return GetDouble(0d, max, maxInclusive);
		}

		public static double GetDouble(double min, double max, bool maxInclusive = false)
		{
			if (Validate(ref min, ref max))
			{
				if (maxInclusive)
				{
					return min + (Random.NextDouble() * (max - min + 0.01d)) - 0.01d;
				}

				return min + (Random.NextDouble() * (max - min));
			}

			return min;
		}

		public static void GetBytes(byte[] buffer)
		{
			GetBytes(buffer.AsSpan());
		}

		public static void GetBytes(Span<byte> buffer)
		{
			if (buffer.Length > 0)
			{
				Random.NextBytes(buffer);
			}
		}

		public static T GetObject<T>(T[] choices)
		{
			return GetObject<T>(choices.AsSpan());
		}

        public static T GetObject<T>(ReadOnlySpan<T> choices)
		{
			var obj = default(T);

			if (!choices.IsEmpty)
			{
				Random.GetItems(choices, new Span<T>(ref obj));
			}

			return obj;
		}
    }
}
