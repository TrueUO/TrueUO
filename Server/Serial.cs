#region References
using System;
#endregion

namespace Server
{
	public readonly struct Serial : IComparable, IComparable<Serial>
	{
		private readonly int m_Serial;

		private static Serial m_LastMobile = Zero;
		private static Serial m_LastItem = 0x40000000;

		public static Serial LastMobile => m_LastMobile;
		public static Serial LastItem => m_LastItem;

		public static readonly Serial MinusOne = new Serial(-1);
		public static readonly Serial Zero = new Serial(0);

		public static Serial NewMobile
		{
			get
			{
				while (World.FindMobile(m_LastMobile += 1) != null)
				{
                    // needed.
                }

				return m_LastMobile;
			}
		}

		public static Serial NewItem
		{
			get
			{
				while (World.FindItem(m_LastItem += 1) != null)
				{
                    // needed.
                }

				return m_LastItem;
			}
		}

		private Serial(int serial)
		{
			m_Serial = serial;
		}

		public int Value => m_Serial;

		public bool IsMobile => m_Serial > 0 && m_Serial < 0x40000000;

		public bool IsItem => m_Serial >= 0x40000000 && m_Serial <= 0x7FFFFFFF;

		public bool IsValid => m_Serial > 0;

		public override int GetHashCode()
		{
			return m_Serial;
		}

		public int CompareTo(Serial other)
		{
			return m_Serial.CompareTo(other.m_Serial);
		}

		public int CompareTo(object other)
		{
			if (other is Serial serial)
			{
				return CompareTo(serial);
			}

            if (other == null)
            {
                return -1;
            }

            throw new ArgumentException();
		}

		public override bool Equals(object o)
		{
			if (o == null || !(o is Serial))
			{
				return false;
			}

			return ((Serial)o).m_Serial == m_Serial;
		}

		public static bool operator ==(Serial l, Serial r)
		{
			return l.m_Serial == r.m_Serial;
		}

		public static bool operator !=(Serial l, Serial r)
		{
			return l.m_Serial != r.m_Serial;
		}

		public static bool operator >(Serial l, Serial r)
		{
			return l.m_Serial > r.m_Serial;
		}

		public static bool operator <(Serial l, Serial r)
		{
			return l.m_Serial < r.m_Serial;
		}

		public static bool operator >=(Serial l, Serial r)
		{
			return l.m_Serial >= r.m_Serial;
		}

		public static bool operator <=(Serial l, Serial r)
		{
			return l.m_Serial <= r.m_Serial;
		}

		/*public static Serial operator ++ ( Serial l )
        {
        return new Serial( l + 1 );
        }*/

		public override string ToString()
		{
			return $"0x{m_Serial:X8}";
		}

		public static implicit operator int(Serial a)
		{
			return a.m_Serial;
		}

		public static implicit operator Serial(int a)
		{
			return new Serial(a);
		}
	}
}
