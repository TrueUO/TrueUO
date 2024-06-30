using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Server.Guilds;

namespace Server
{
	public abstract class GenericReader
	{
        public abstract Type ReadObjectType();
        
		public abstract string ReadString();
		public abstract DateTime ReadDateTime();
		public abstract TimeSpan ReadTimeSpan();
		public abstract DateTime ReadDeltaTime();
		public abstract long ReadLong();
		public abstract ulong ReadULong();
		public abstract int PeekInt();
		public abstract int ReadInt();
		public abstract uint ReadUInt();
		public abstract short ReadShort();
		public abstract ushort ReadUShort();
		public abstract double ReadDouble();
		public abstract float ReadFloat();
		public abstract byte ReadByte();
		public abstract sbyte ReadSByte();
		public abstract bool ReadBool();
		public abstract int ReadEncodedInt();
		public abstract IPAddress ReadIPAddress();

		public abstract Point3D ReadPoint3D();
		public abstract Point2D ReadPoint2D();
		public abstract Rectangle2D ReadRect2D();
		public abstract Rectangle3D ReadRect3D();
		public abstract Map ReadMap();

		public abstract Item ReadItem();
		public abstract Mobile ReadMobile();
		public abstract BaseGuild ReadGuild();

		public abstract T ReadItem<T>() where T : Item;
		public abstract T ReadMobile<T>() where T : Mobile;

		public abstract ArrayList ReadItemList();
		public abstract ArrayList ReadMobileList();

		public abstract List<Item> ReadStrongItemList();
		public abstract List<T> ReadStrongItemList<T>() where T : Item;

		public abstract List<Mobile> ReadStrongMobileList();
		public abstract List<T> ReadStrongMobileList<T>() where T : Mobile;

		public abstract List<T> ReadStrongGuildList<T>() where T : BaseGuild;

		public abstract Race ReadRace();

		public abstract bool End();
	}

	public abstract class GenericWriter
	{
        public abstract void Close();

        public abstract long Position { get; }

        public abstract void WriteObjectType(object value);
        public abstract void WriteObjectType(Type value);

        public abstract void Write(string value);
		public abstract void Write(DateTime value);
		public abstract void Write(TimeSpan value);
		public abstract void Write(long value);
		public abstract void Write(ulong value);
		public abstract void Write(int value);
		public abstract void Write(uint value);
		public abstract void Write(short value);
		public abstract void Write(ushort value);
		public abstract void Write(double value);
		public abstract void Write(float value);
		public abstract void Write(byte value);
		public abstract void Write(sbyte value);
		public abstract void Write(bool value);
		public abstract void WriteEncodedInt(int value);
		public abstract void Write(IPAddress value);

		public abstract void WriteDeltaTime(DateTime value);

		public abstract void Write(Point3D value);
		public abstract void Write(Point2D value);
		public abstract void Write(Rectangle2D value);
		public abstract void Write(Rectangle3D value);
		public abstract void Write(Map value);

		public abstract void Write(Item value);
		public abstract void Write(Mobile value);
		public abstract void Write(BaseGuild value);

		public abstract void WriteItem<T>(T value) where T : Item;
		public abstract void WriteMobile<T>(T value) where T : Mobile;

		public abstract void Write(Race value);

		public abstract void Write(List<Item> list);
		public abstract void Write(List<Item> list, bool tidy);

		public abstract void WriteItemList<T>(List<T> list) where T : Item;
		public abstract void WriteItemList<T>(List<T> list, bool tidy) where T : Item;

		public abstract void Write(List<Mobile> list);
		public abstract void Write(List<Mobile> list, bool tidy);

		public abstract void WriteMobileList<T>(List<T> list) where T : Mobile;
		public abstract void WriteMobileList<T>(List<T> list, bool tidy) where T : Mobile;

		public abstract void WriteGuildList<T>(List<T> list, bool tidy) where T : BaseGuild;
    }
    
	public class BinaryFileWriter : GenericWriter, IDisposable
	{
		private readonly bool _PrefixStrings;
		private Stream _File;

		protected virtual int BufferSize => 81920;

		private readonly byte[] _Buffer;

		private int _Index;

		private readonly Encoding _Encoding;

		public BinaryFileWriter(Stream strm, bool prefixStr)
		{
			_PrefixStrings = prefixStr;
			_Encoding = Utility.UTF8;
			_Buffer = new byte[BufferSize];
			_File = strm;
		}

		public BinaryFileWriter(string filename, bool prefixStr)
		{
			_PrefixStrings = prefixStr;
			_Buffer = new byte[BufferSize];
			_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			_Encoding = Utility.UTF8WithEncoding;
		}

		private long _Position;
		public override long Position => _Position + _Index;

        public void Flush()
        {
            if (_Index > 0)
            {
                _Position += _Index;

                _File.Write(_Buffer, 0, _Index);

                _Index = 0;
            }
        }

        public override void Close()
        {
            if (_Index > 0)
            {
                Flush();
            }

            _File.Close();
        }

        public override void WriteEncodedInt(int value)
		{
			uint v = (uint)value;

			while (v >= 0x80)
			{
				if (_Index + 1 > _Buffer.Length)
				{
					Flush();
				}

				_Buffer[_Index++] = (byte)(v | 0x80);
				v >>= 7;
			}

			if (_Index + 1 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index++] = (byte)v;
		}

		private byte[] _CharacterBuffer;
		private int _MaxBufferChars;
		private const int _LargeByteBufferSize = 256;

        private void InternalWriteString(string value)
		{
			int length = _Encoding.GetByteCount(value);

			WriteEncodedInt(length);

			if (_CharacterBuffer == null)
			{
				_CharacterBuffer = new byte[_LargeByteBufferSize];
				_MaxBufferChars = _LargeByteBufferSize / _Encoding.GetMaxByteCount(1);
			}

			if (length > _LargeByteBufferSize)
			{
				int current = 0;
				int charsLeft = value.Length;

				while (charsLeft > 0)
				{
					int charCount = charsLeft > _MaxBufferChars ? _MaxBufferChars : charsLeft;
					int byteLength = _Encoding.GetBytes(value, current, charCount, _CharacterBuffer, 0);

					if (_Index + byteLength > _Buffer.Length)
					{
						Flush();
					}

					Buffer.BlockCopy(_CharacterBuffer, 0, _Buffer, _Index, byteLength);
					_Index += byteLength;

					current += charCount;
					charsLeft -= charCount;
				}
			}
			else
			{
				int byteLength = _Encoding.GetBytes(value, 0, value.Length, _CharacterBuffer, 0);

				if (_Index + byteLength > _Buffer.Length)
				{
					Flush();
				}

				Buffer.BlockCopy(_CharacterBuffer, 0, _Buffer, _Index, byteLength);
				_Index += byteLength;
			}
		}

		public override void Write(string value)
		{
			if (_PrefixStrings)
			{
				if (value == null)
				{
					if (_Index + 1 > _Buffer.Length)
					{
						Flush();
					}

					_Buffer[_Index++] = 0;
				}
				else
				{
					if (_Index + 1 > _Buffer.Length)
					{
						Flush();
					}

					_Buffer[_Index++] = 1;

					InternalWriteString(value);
				}
			}
			else
			{
				InternalWriteString(value);
			}
        }

        public override void WriteObjectType(object value)
        {
            WriteObjectType(value?.GetType());
        }

        public override void WriteObjectType(Type value)
        {
        	int hash = ScriptCompiler.FindHashByFullName(value?.FullName);
        	
            WriteEncodedInt(hash);
        }

        public override void Write(DateTime value)
		{
			Write(value.Ticks);
		}

        public override void WriteDeltaTime(DateTime value)
        {
            Write(value.Ticks - DateTime.UtcNow.Ticks);
        }

		public override void Write(IPAddress value)
		{
			Write(Utility.GetLongAddressValue(value));
		}

		public override void Write(TimeSpan value)
		{
			Write(value.Ticks);
		}

		public override void Write(long value)
		{
			if (_Index + 8 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index] = (byte)value;
            _Buffer[_Index + 1] = (byte)(value >> 8);
			_Buffer[_Index + 2] = (byte)(value >> 16);
			_Buffer[_Index + 3] = (byte)(value >> 24);
			_Buffer[_Index + 4] = (byte)(value >> 32);
			_Buffer[_Index + 5] = (byte)(value >> 40);
			_Buffer[_Index + 6] = (byte)(value >> 48);
			_Buffer[_Index + 7] = (byte)(value >> 56);

			_Index += 8;
		}

		public override void Write(ulong value)
		{
			if (_Index + 8 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index] = (byte)value;
			_Buffer[_Index + 1] = (byte)(value >> 8);
			_Buffer[_Index + 2] = (byte)(value >> 16);
			_Buffer[_Index + 3] = (byte)(value >> 24);
			_Buffer[_Index + 4] = (byte)(value >> 32);
			_Buffer[_Index + 5] = (byte)(value >> 40);
			_Buffer[_Index + 6] = (byte)(value >> 48);
			_Buffer[_Index + 7] = (byte)(value >> 56);

			_Index += 8;
		}

		public override void Write(int value)
		{
			if (_Index + 4 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index] = (byte)value;
			_Buffer[_Index + 1] = (byte)(value >> 8);
			_Buffer[_Index + 2] = (byte)(value >> 16);
			_Buffer[_Index + 3] = (byte)(value >> 24);

			_Index += 4;
		}

		public override void Write(uint value)
		{
			if (_Index + 4 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index] = (byte)value;
			_Buffer[_Index + 1] = (byte)(value >> 8);
			_Buffer[_Index + 2] = (byte)(value >> 16);
			_Buffer[_Index + 3] = (byte)(value >> 24);

			_Index += 4;
		}

		public override void Write(short value)
		{
			if (_Index + 2 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index] = (byte)value;
			_Buffer[_Index + 1] = (byte)(value >> 8);

			_Index += 2;
		}

		public override void Write(ushort value)
		{
			if (_Index + 2 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index] = (byte)value;
			_Buffer[_Index + 1] = (byte)(value >> 8);

			_Index += 2;
		}

		public override unsafe void Write(double value)
		{
			if (_Index + 8 > _Buffer.Length)
			{
				Flush();
			}

			fixed (byte* pBuffer = _Buffer)
			{
				*(double*)(pBuffer + _Index) = value;
			}

			_Index += 8;
		}

		public override unsafe void Write(float value)
		{
			if (_Index + 4 > _Buffer.Length)
			{
				Flush();
			}

			fixed (byte* pBuffer = _Buffer)
			{
				*(float*)(pBuffer + _Index) = value;
			}

			_Index += 4;
		}

		public override void Write(byte value)
		{
			if (_Index + 1 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index++] = value;
		}

		public override void Write(sbyte value)
		{
			if (_Index + 1 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index++] = (byte)value;
		}

		public override void Write(bool value)
		{
			if (_Index + 1 > _Buffer.Length)
			{
				Flush();
			}

			_Buffer[_Index++] = (byte)(value ? 1 : 0);
		}

		public override void Write(Point3D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
			Write(value.m_Z);
		}

		public override void Write(Point2D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
		}

		public override void Write(Rectangle2D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Rectangle3D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Map value)
		{
			if (value != null)
			{
				Write((byte)value.MapIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Race value)
		{
			if (value != null)
			{
				Write((byte)value.RaceIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void WriteItem<T>(T value)
		{
			Write(value);
		}

		public override void WriteMobile<T>(T value)
		{
			Write(value);
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}

		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList(list, false);
		}

		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void Write(List<Mobile> list)
		{
			Write(list, false);
		}

		public override void Write(List<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList(list, false);
		}

		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Deleted)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				for (int i = 0; i < list.Count;)
				{
					if (list[i].Disbanded)
					{
						list.RemoveAt(i);
					}
					else
					{
						++i;
					}
				}
			}

			Write(list.Count);

			for (int i = 0; i < list.Count; ++i)
			{
				Write(list[i]);
			}
		}

        public void Dispose()
        {
            if (_Index > 0)
            {
                Flush();
            }

            _File.Close();
        }
    }

	public sealed class BinaryFileReader : GenericReader
	{
		private readonly BinaryReader m_File;

		public BinaryFileReader(BinaryReader br)
		{
			m_File = br;
		}

		public void Close()
		{
			m_File.Close();
		}

		public long Position => m_File.BaseStream.Position;

		public long Seek(long offset, SeekOrigin origin)
		{
			return m_File.BaseStream.Seek(offset, origin);
		}

        public override Type ReadObjectType()
        {
        	int hash = ReadEncodedInt();
        	
            return ScriptCompiler.FindTypeByFullNameHash(hash);
        }

        public override string ReadString()
        {
            if (ReadByte() != 0)
			{
				return m_File.ReadString();
			}

            return null;
        }

		public override DateTime ReadDeltaTime()
		{
			long ticks = m_File.ReadInt64();
			long now = DateTime.UtcNow.Ticks;

			if (ticks > 0 && ticks + now < 0)
			{
				return DateTime.MaxValue;
			}

            if (ticks < 0 && ticks + now < 0)
            {
                return DateTime.MinValue;
            }

            try
			{
				return new DateTime(now + ticks);
			}
			catch (Exception e)
			{
                Diagnostics.ExceptionLogging.LogException(e);

				if (ticks > 0)
				{
					return DateTime.MaxValue;
				}

                return DateTime.MinValue;
            }
		}

		public override IPAddress ReadIPAddress()
		{
			return new IPAddress(m_File.ReadInt64());
		}

		public override int ReadEncodedInt()
		{
			int v = 0, shift = 0;
			byte b;

			do
			{
				b = m_File.ReadByte();
				v |= (b & 0x7F) << shift;
				shift += 7;
			}
			while (b >= 0x80);

			return v;
		}

		public override DateTime ReadDateTime()
		{
			return new DateTime(m_File.ReadInt64());
		}

		public override TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(m_File.ReadInt64());
		}

		public override long ReadLong()
		{
			return m_File.ReadInt64();
		}

		public override ulong ReadULong()
		{
			return m_File.ReadUInt64();
		}

		public override int PeekInt()
		{
			int value = 0;
			long returnTo = m_File.BaseStream.Position;

			try
			{
				value = m_File.ReadInt32();
			}
			catch (EndOfStreamException)
			{
				// Ignore this exception, the default value 0 will be returned
			}

			m_File.BaseStream.Seek(returnTo, SeekOrigin.Begin);
			return value;
		}

		public override int ReadInt()
		{
			return m_File.ReadInt32();
		}

		public override uint ReadUInt()
		{
			return m_File.ReadUInt32();
		}

		public override short ReadShort()
		{
			return m_File.ReadInt16();
		}

		public override ushort ReadUShort()
		{
			return m_File.ReadUInt16();
		}

		public override double ReadDouble()
		{
			return m_File.ReadDouble();
		}

		public override float ReadFloat()
		{
			return m_File.ReadSingle();
		}

		public override byte ReadByte()
		{
			return m_File.ReadByte();
		}

		public override sbyte ReadSByte()
		{
			return m_File.ReadSByte();
		}

		public override bool ReadBool()
		{
			return m_File.ReadBoolean();
		}

		public override Point3D ReadPoint3D()
		{
			return new Point3D(ReadInt(), ReadInt(), ReadInt());
		}

		public override Point2D ReadPoint2D()
		{
			return new Point2D(ReadInt(), ReadInt());
		}

		public override Rectangle2D ReadRect2D()
		{
			return new Rectangle2D(ReadPoint2D(), ReadPoint2D());
		}

		public override Rectangle3D ReadRect3D()
		{
			return new Rectangle3D(ReadPoint3D(), ReadPoint3D());
		}

		public override Map ReadMap()
		{
			return Map.Maps[ReadByte()];
		}

		public override Item ReadItem()
		{
			return World.FindItem(ReadInt());
		}

		public override Mobile ReadMobile()
		{
			return World.FindMobile(ReadInt());
		}

		public override BaseGuild ReadGuild()
		{
			return BaseGuild.Find(ReadInt());
		}

		public override T ReadItem<T>()
		{
			return ReadItem() as T;
		}

		public override T ReadMobile<T>()
		{
			return ReadMobile() as T;
		}

		public override ArrayList ReadItemList()
		{
			int count = ReadInt();

			if (count > 0)
			{
				ArrayList list = new ArrayList(count);

				for (int i = 0; i < count; ++i)
				{
					Item item = ReadItem();

					if (item != null)
					{
						list.Add(item);
					}
				}

				return list;
			}

            return new ArrayList();
        }

		public override ArrayList ReadMobileList()
		{
			int count = ReadInt();

			if (count > 0)
			{
				ArrayList list = new ArrayList(count);

				for (int i = 0; i < count; ++i)
				{
					Mobile m = ReadMobile();

					if (m != null)
					{
						list.Add(m);
					}
				}

				return list;
			}

            return new ArrayList();
        }

		public override List<Item> ReadStrongItemList()
		{
			return ReadStrongItemList<Item>();
		}

		public override List<T> ReadStrongItemList<T>()
		{
			int count = ReadInt();

			if (count > 0)
			{
				List<T> list = new List<T>(count);

				for (int i = 0; i < count; ++i)
				{
                    if (ReadItem() is T item)
					{
						list.Add(item);
					}
				}

				return list;
			}

            return new List<T>();
        }

		public override List<Mobile> ReadStrongMobileList()
		{
			return ReadStrongMobileList<Mobile>();
		}

		public override List<T> ReadStrongMobileList<T>()
		{
			int count = ReadInt();

			if (count > 0)
			{
				List<T> list = new List<T>(count);

				for (int i = 0; i < count; ++i)
				{
                    if (ReadMobile() is T m)
					{
						list.Add(m);
					}
				}

				return list;
			}

            return new List<T>();
        }

		public override List<T> ReadStrongGuildList<T>()
		{
			int count = ReadInt();

			if (count > 0)
			{
				List<T> list = new List<T>(count);

				for (int i = 0; i < count; ++i)
				{
                    if (ReadGuild() is T g)
					{
						list.Add(g);
					}
				}

				return list;
			}

            return new List<T>();
        }

		public override Race ReadRace()
		{
			return Race.Races[ReadByte()];
		}

		public override bool End()
		{
			return m_File.PeekChar() == -1;
		}
	}

	public interface ISerializable
	{
		int TypeReference { get; }
		int SerialIdentity { get; }
		void Serialize(GenericWriter writer);
	}
}
