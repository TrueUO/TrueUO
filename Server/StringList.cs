using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server
{
    public class StringList
    {
        public static StringList Localization { get; }

        static StringList()
        {
            Localization = new StringList();
        }

        public List<StringEntry> Entries { get; set; }

        public Dictionary<int, string> StringTable;
        private readonly Dictionary<int, StringEntry> _EntryTable;

        public string Language { get; private set; }

        public string this[int number]
        {
            get
            {
                if (StringTable.TryGetValue(number, out string value))
                {
                    return value;
                }

                return null;
            }
        }

        public StringList()
            : this("enu")
        {
        }

        public StringList(string language)
            : this(language, true)
        {
        }

        public StringList(string language, bool format)
        {
            Language = language;            

            string path = Core.FindDataFile(string.Format("Cliloc.{0}", language));

            if (path == null)
            {
                Console.WriteLine("Cliloc.{0} not found", language);
                Entries = new List<StringEntry>(0);
                return;
            }

            StringTable = new Dictionary<int, string>();
            _EntryTable = new Dictionary<int, StringEntry>();
            Entries = new List<StringEntry>();

            using (BinaryReader bin = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                byte[] buffer = new byte[1024];

                bin.ReadInt32();
                bin.ReadInt16();

                while (bin.BaseStream.Length != bin.BaseStream.Position)
                {
                    int number = bin.ReadInt32();
                    bin.ReadByte(); // flag
                    int length = bin.ReadInt16();

                    if (length > buffer.Length)
                        buffer = new byte[(length + 1023) & ~1023];

                    bin.Read(buffer, 0, length);
                    string text = Encoding.UTF8.GetString(buffer, 0, length);

                    StringEntry se = new StringEntry(number, text);
                    Entries.Add(se);

                    StringTable[number] = text;
                    _EntryTable[number] = se;
                }
            }
        }

        public StringEntry GetEntry(int number)
        {
            if (_EntryTable == null || !_EntryTable.TryGetValue(number, out StringEntry value))
            {
                return null;
            }

            return value;
        }

        public string GetString(int number)
        {
            if (StringTable == null || !StringTable.TryGetValue(number, out string value))
            {
                return null;
            }

            return value;
        }
    }

    public class StringEntry
    {
        public int Number { get; }
        public string Text { get; }

        public StringEntry(int number, string text)
        {
            Number = number;
            Text = text;
        }

        private string _FmtTxt;
        private static readonly Regex _RegEx = new Regex(@"~(\d+)[_\w]+~", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly object[] _Args = { "", "", "", "", "", "", "", "", "", "", "" };

        public string Format(params object[] args)
        {
            if (_FmtTxt == null)
            {
                _FmtTxt = _RegEx.Replace(Text, "{$1}");
            }

            for (int i = 0; i < args.Length && i < 10; i++)
            {
                _Args[i + 1] = args[i];
            }

            return string.Format(_FmtTxt, _Args);
        }
    }
}
