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

        private readonly Dictionary<int, string> _StringTable;
        private readonly Dictionary<int, StringEntry> _EntryTable;

        public string Language { get; private set; }

        public string this[int number]
        {
            get
            {
                if (_StringTable.TryGetValue(number, out string value))
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

            string path = Core.FindDataFile($"Cliloc.{language}");

            if (path == null)
            {
                Console.WriteLine($"Cliloc.{language} not found");
                Entries = new List<StringEntry>(0);
                return;
            }

            _StringTable = new Dictionary<int, string>();
            _EntryTable = new Dictionary<int, StringEntry>();
            Entries = new List<StringEntry>();

            // Read the entire file into a byte array.
            byte[] fileData = File.ReadAllBytes(path);

            // Check for the new (compressed) format.
            // (ClassicUO checks if the 4th byte is 0x8E.)
            if (fileData.Length > 3 && fileData[3] == 0x8E)
            {
                try
                {
                    fileData = BwtDecompress.Decompress(fileData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Decompression failed: " + ex.Message);
                    return;
                }
            }

            // Use a MemoryStream and BinaryReader to process the cliloc data.
            using (MemoryStream ms = new MemoryStream(fileData))
            using (BinaryReader bin = new BinaryReader(ms))
            {
                // Read header values.
                bin.ReadInt32(); 
                bin.ReadInt16(); 

                // Read each cliloc entry.
                while (ms.Position < ms.Length)
                {
                    int number = bin.ReadInt32();
                    bin.ReadByte(); // flag (ignored)
                    int length = bin.ReadInt16();

                    byte[] textBytes = bin.ReadBytes(length);
                    string text = Encoding.UTF8.GetString(textBytes);

                    StringEntry se = new StringEntry(number, text);
                    Entries.Add(se);

                    _StringTable[number] = text;
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
            if (_StringTable == null || !_StringTable.TryGetValue(number, out string value))
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
