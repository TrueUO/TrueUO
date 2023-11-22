using System.IO;
using System.Text;
using Server.Network;

namespace Server
{
    public interface ObjectPropertyList
    {
        void Add(int number);
        void Add(int number, string arguments);
        void Add(int number, string format, params object[] args);
        void Add(string text);
        void Add(string format, params object[] args);
    }

    public sealed class ObjectPropertyListPacket : Packet, ObjectPropertyList
    {
		private int _Hash;
		private int _Strings;

		public IEntity Entity { get; }
		public int Hash => 0x40000000 + _Hash;

		public int Header { get; set; }
		public string HeaderArgs { get; set; }

		public ObjectPropertyListPacket(IEntity e)
			: base(0xD6)
		{
			EnsureCapacity(128);

			Entity = e;

			m_Stream.Write((short)1);
			m_Stream.Write(e.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0);
			m_Stream.Write(e.Serial);
		}

		public void Add(int number)
		{
			if (number == 0)
			{
				return;
			}

			AddHash(number);

			if (Header == 0)
			{
				Header = number;
				HeaderArgs = "";
			}

			m_Stream.Write(number);
			m_Stream.Write((short)0);
		}

		public void Terminate()
		{
			m_Stream.Write(0);

			m_Stream.Seek(11, SeekOrigin.Begin);
			m_Stream.Write(_Hash);
		}

		private static byte[] _Buffer = new byte[1024];
		private static readonly Encoding _Encoding = Encoding.Unicode;

		public void AddHash(int val)
		{
			_Hash ^= val & 0x3FFFFFF;
			_Hash ^= (val >> 26) & 0x3F;
		}

		public void Add(int number, string arguments)
		{
			if (number == 0)
			{
				return;
			}

			if (arguments == null)
			{
				arguments = "";
			}

			if (Header == 0)
			{
				Header = number;
				HeaderArgs = arguments;
			}

			AddHash(number);
			AddHash(arguments.GetHashCode());

			m_Stream.Write(number);

			int byteCount = _Encoding.GetByteCount(arguments);

			if (byteCount > _Buffer.Length)
			{
				_Buffer = new byte[byteCount];
			}

			byteCount = _Encoding.GetBytes(arguments, 0, arguments.Length, _Buffer, 0);

			m_Stream.Write((short)byteCount);
			m_Stream.Write(_Buffer, 0, byteCount);
		}

        public void Add(int number, string format, params object[] args)
		{
			Add(number, string.Format(format, args));
		}

		// Each of these are localized to "~1_NOTHING~" which allows the string argument to be used
		private static readonly int[] _StringNumbers = { 1042971, 1070722 };

		private int GetStringNumber()
		{
			return _StringNumbers[_Strings++ % _StringNumbers.Length];
		}

		public void Add(string text)
		{
			Add(GetStringNumber(), text);
		}

        public void Add(string format, params object[] args)
		{
			Add(GetStringNumber(), string.Format(format, args));
		}
	}

	public sealed class OPLInfo : Packet
	{
		public OPLInfo(ObjectPropertyListPacket list)
			: base(0xDC, 9)
		{
			m_Stream.Write(list.Entity.Serial);
			m_Stream.Write(list.Hash);
		}
	}
}
