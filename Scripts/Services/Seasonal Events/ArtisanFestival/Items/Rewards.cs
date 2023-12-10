using Server.Items;

using System;
using System.Linq;

namespace Server.Engines.ArtisanFestival
{
    public class RewardLantern : BaseLight, IFlipable
    {
        public override int LabelNumber => 1125100;

        public override int LitItemID => GetLitID();
        public override int UnlitItemID => GetUnlitID();

        public bool EastFacing => _IdTable.Any(list => Array.IndexOf(list, ItemID) == 0 || Array.IndexOf(list, ItemID) == 2);

        [Constructable]
        public RewardLantern()
            : this(null)
        {
        }

        public RewardLantern(Mobile m)
            : base(GetID(m))
        {
        }

        public RewardLantern(Serial serial)
            : base(serial)
        {
        }

        public void OnFlip(Mobile m)
        {
            int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

            if (list != null)
            {
                int index = Array.IndexOf(list, ItemID);

                if (index == 0 || index == 2)
                {
                    ItemID = list[index + 1];
                }
                else if (index == 1 || index == 3)
                {
                    ItemID = list[index - 1];
                }
            }
        }

        private static int GetID(Mobile m)
        {
            if (m == null)
            {
                return _IdTable[Utility.Random(_IdTable.Length)][0];
            }

            if (m.Karma < 0)
            {
                return _IdTable[Utility.RandomMinMax(4, 7)][0];
            }

            return _IdTable[Utility.RandomMinMax(0, 3)][0];
        }

        private int GetLitID()
        {
            int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

            if (list != null)
            {
                return EastFacing ? list[2] : list[3];
            }

            return ItemID;
        }

        private int GetUnlitID()
        {
            int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

            if (list != null)
            {
                return EastFacing ? list[0] : list[1];
            }

            return ItemID;
        }

        private static readonly int[][] _IdTable =
        {
                    //       OFF             ON
                    //  East    South   East    South
            // Virtue
            new[] { 0x9DE5, 0x9E0A, 0x9DFE, 0x9E0E },
            new[] { 0x9DE7, 0x9E0C, 0x9E01, 0x9E11 },
            new[] { 0xA084, 0xA088, 0xA085, 0xA089 },
            new[] { 0xA08C, 0xA090, 0xA08D, 0xA091 },

            // Vice
            new[] { 0x9DE6, 0x9E0B, 0x9E04, 0x9E14 },
            new[] { 0x9DE8, 0x9E0D, 0x9E07, 0x9E17 },
            new[] { 0xA074, 0xA078, 0xA075, 0xA079 },
            new[] { 0xA07C, 0xA080, 0xA07D, 0xA081 }
        };

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class RewardPillow : Item, IFlipable
    {
        public override int LabelNumber => 1125137;

        [Constructable]
        public RewardPillow()
            : this(null)
        {
        }

        public RewardPillow(Mobile m)
            : base(GetID(m))
        {
        }

        public RewardPillow(Serial serial)
          : base(serial)
        {
        }

        public void OnFlip(Mobile m)
        {
            int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

            if (list != null)
            {
                int index = Array.IndexOf(list, ItemID);

                if (index == 0)
                {
                    ItemID = list[1];
                }
                else
                {
                    ItemID = list[0];
                }
            }
        }

        private static int GetID(Mobile m)
        {
            if (m == null)
            {
                return _IdTable[Utility.Random(_IdTable.Length)][0];
            }

            if (m.Karma < 0)
            {
                return _IdTable[Utility.RandomMinMax(8, 15)][0];
            }

            return _IdTable[Utility.RandomMinMax(0, 7)][0];
        }

        private static readonly int[][] _IdTable =
        {
            // GOOD KARMA
            new[] { 0x9E1D, 0x9E1E },
            new[] { 0x9E1F, 0x9E20 },
            new[] { 0xA09E, 0xA09D },
            new[] { 0xA0A0, 0xA09F },
            new[] { 0xA24A, 0xA24B },
            new[] { 0xA24C, 0xA24D },
            new[] { 0xA491, 0xA492 },
            new[] { 0xA493, 0xA494 },

            // BAD KARMA
            new[] { 0x9E21, 0x9E22 },
            new[] { 0x9E23, 0x9E24 },
            new[] { 0xA099, 0xA09A },
            new[] { 0xA09B, 0xA09C },
            new[] { 0xA24E, 0xA24F },
            new[] { 0xA250, 0xA251 },
            new[] { 0xA495, 0xA496 },
            new[] { 0xA497, 0xA498 }
        };

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class RewardPainting : Item, IFlipable
    {
        public override int LabelNumber => 1125147;

        [Constructable]
        public RewardPainting()
            : this(null)
        {
        }

        public RewardPainting(Mobile m)
            : base(GetID(m))
        {
        }

        public RewardPainting(Serial serial)
            : base(serial)
        {
        }

        public void OnFlip(Mobile m)
        {
            int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

            if (list != null)
            {
                int index = Array.IndexOf(list, ItemID);

                if (index == 0)
                {
                    ItemID = list[1];
                }
                else
                {
                    ItemID = list[0];
                }
            }
        }

        private static int GetID(Mobile m)
        {
            if (m == null)
            {
                return _IdTable[Utility.Random(_IdTable.Length)][0];
            }

            if (m.Karma < 0)
            {
                return _IdTable[Utility.RandomMinMax(8, 15)][0];
            }

            return _IdTable[Utility.RandomMinMax(0, 7)][0];
        }

        private static readonly int[][] _IdTable =
        {
            // GOOD KARMA
            new[] { 0x9E31, 0x9E32 },
            new[] { 0x9E33, 0x9E34 },
            new[] { 0xA0A4, 0xA0A3 },
            new[] { 0xA0A8, 0xA0A7 },
            new[] { 0xA246, 0xA247 },
            new[] { 0xA248, 0xA249 },
            new[] { 0xA48B, 0xA48C },
            new[] { 0xA48D, 0xA48E },

            // BAD KARMA
            new[] { 0x9E2D, 0x9E2E },
            new[] { 0x9E2F, 0x9E30 },
            new[] { 0xA0A6, 0xA0A5 },
            new[] { 0xA0AA, 0xA0A9 },
            new[] { 0xA242, 0xA243 },
            new[] { 0xA244, 0xA245 },
            new[] { 0xA489, 0xA48A },
            new[] { 0xA48F, 0xA490 }
        };

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class RewardSculpture : Item
    {
        public override int LabelNumber => 1125080;

        public bool Active => _IdTable.Any(list => Array.IndexOf(list, ItemID) > 0);

        [Constructable]
        public RewardSculpture()
            : this(null)
        {
        }

        public RewardSculpture(Mobile m)
            : base(GetID(m))
        {
        }

        public RewardSculpture(Serial serial)
           : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Active && !IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

                if (list != null)
                {
                    int index = Array.IndexOf(list, ItemID);

                    if (index == 0)
                    {
                        ItemID = list[1];
                    }
                    else
                    {
                        ItemID = list[0];
                    }
                }
            }
        }

        private static int GetID(Mobile m)
        {
            if (m == null)
            {
                return _IdTable[Utility.Random(_IdTable.Length)][0];
            }

            if (m.Karma < 0)
            {
                return _IdTable[Utility.RandomMinMax(8, 15)][0];
            }

            return _IdTable[Utility.RandomMinMax(0, 7)][0];
        }

        private static readonly int[][] _IdTable =
        {
            // GOOD KARMA
            new[] { 0x9E80, 0x9DEE },
            new[] { 0x9E82, 0x9DF6 },
            new[] { 0xA065, 0xA066 },
            new[] { 0xA06F, 0xA070 },
            new[] { 0xA252, 0xA253 },
            new[] { 0xA257, 0xA258 },
            new[] { 0xA499, 0xA49A },
            new[] { 0xA49E, 0xA49F },

            // BAD KARMA
            new[] { 0x9E81, 0x9DF2 },
            new[] { 0x9E83, 0x9DFA },
            new[] { 0xA060, 0xA061 },
            new[] { 0xA06A, 0xA06B },
            new[] { 0xA25C, 0xA25D },
            new[] { 0xA261, 0xA262 },
            new[] { 0xA4A3, 0xA4A4 },
            new[] { 0xA4A8, 0xA4A9 }
        };

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class RewardStainedGlassWindow : Item, IFlipable
    {
        public override int LabelNumber => 1126510; // stained glass window

        [Constructable]
        public RewardStainedGlassWindow()
            : this(null)
        {
        }

        public RewardStainedGlassWindow(Mobile m)
            : base(GetID(m))
        {
        }

        public RewardStainedGlassWindow(Serial serial)
            : base(serial)
        {
        }

        public void OnFlip(Mobile m)
        {
            int[] list = _IdTable.FirstOrDefault(l => l.Any(id => id == ItemID));

            if (list != null)
            {
                int index = Array.IndexOf(list, ItemID);

                if (index == 0)
                {
                    ItemID = list[1];
                }
                else
                {
                    ItemID = list[0];
                }
            }
        }

        private static int GetID(Mobile m)
        {
            if (m == null)
            {
                return _IdTable[Utility.Random(_IdTable.Length)][0];
            }

            if (m.Karma < 0)
            {
                return _IdTable[Utility.RandomMinMax(8, 15)][0];
            }

            return _IdTable[Utility.RandomMinMax(0, 7)][0];
        }

        private static readonly int[][] _IdTable =
        {
            // GOOD KARMA
            new[] { 0xAD7D, 0xAD7E },
            new[] { 0xAD7F, 0xAD80 },
            new[] { 0xA9DA, 0xA9DB },
            new[] { 0xA9DC, 0xA9DD },
            new[] { 0xA7BF, 0xA7C0 },
            new[] { 0xA7C1, 0xA7C2 },
            new[] { 0xA5FA, 0xA5FB },
            new[] { 0xA5FC, 0xA5FD },

            // BAD KARMA
            new[] { 0xAD79, 0xAD7A },
            new[] { 0xAD7B, 0xAD7C },
            new[] { 0xA9D6, 0xA9D7 },
            new[] { 0xA9D8, 0xA9D9 },
            new[] { 0xA7BB, 0xA7BC },
            new[] { 0xA7BD, 0xA7BE },
            new[] { 0xA5F6, 0xA5F7 },
            new[] { 0xA5F8, 0xA5F9 }
        };

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
