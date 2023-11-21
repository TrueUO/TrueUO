using Server.Gumps;
using Server.Mobiles;

namespace Server.Engines.BulkOrders
{
    public class BOBFilterGump : Gump
    {
        private static readonly int[,] _MaterialFilters =
        {
            { 1044067, 1 }, // Blacksmithy
            { 1062226, 9 }, // Iron
            { 1018332, 10 }, // Dull Copper
            { 1018333, 11 }, // Shadow Iron
            { 1018334, 12 }, // Copper
            { 1018335, 13 }, // Bronze

            { 0, 0 }, // --Blank--
            { 1018336, 14 }, // Golden
            { 1018337, 15 }, // Agapite
            { 1018338, 16 }, // Verite
            { 1018339, 17 }, // Valorite
            { 0, 0 }, // --Blank--

            { 1044094, 2 }, // Tailoring
            { 1044286, 18 }, // Cloth
            { 1062235, 19 }, // Leather
            { 1062236, 20 }, // Spined
            { 1062237, 21 }, // Horned
            { 1062238, 22 }, // Barbed

            { 1044097, 3 }, // Tinkering
            { 1062226, 23 }, // Iron
            { 1018332, 24 }, // Dull Copper
            { 1018333, 25 }, // Shadow Iron
            { 1018334, 26 }, // Copper
            { 1018335, 27 }, // Bronze

            { 0, 0 }, // --Blank--
            { 1018336, 28 }, // Golden
            { 1018337, 29 }, // Agapite
            { 1018338, 30 }, // Verite
            { 1018339, 31 }, // Valorite
            { 0, 0 }, // --Blank--

            { 1044071, 4 }, // Carpentry
            { 1079435, 32 }, // Wood
            { 1071428, 33 }, // Oak
            { 1071429, 34 }, // Ash
            { 1071430, 35 }, // Yew
            { 0, 0 }, // --Blank--

            { 0, 0 }, // --Blank--
            { 1071431, 36 }, // Bloodwood
            { 1071432, 37 }, // Heartwood
            { 1071433, 38 }, // Frostwood
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--

            { 1044068, 5 }, // Fletching
            { 1079435, 39 }, // Wood
            { 1071428, 40 }, // Oak
            { 1071429, 41 }, // Ash
            { 1071430, 42 }, // Yew
            { 0, 0 }, // --Blank--

            { 0, 0 }, // --Blank--
            { 1071431, 43 }, // Bloodwood
            { 1071432, 44 }, // Heartwood
            { 1071433, 45 }, // Frostwood
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--

            { 1044060, 6 }, // Alchemy
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--

            { 1044083, 7 }, // Inscription
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--

            { 1044073, 8 }, // Cooking
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 }, // --Blank--
            { 0, 0 } // --Blank--
        };

        private static readonly int[,] _TypeFilters =
        {
            { 1062229, 0 }, // All
            { 1062224, 1 }, // Small
            { 1062225, 2 }// Large
        };

        private static readonly int[,] _QualityFilters =
        {
            { 1062229, 0 }, // All
            { 1011542, 1 }, // Normal
            { 1060636, 2 }// Exceptional
        };

        private static readonly int[,] _AmountFilters =
        {
            { 1062229, 0 }, // All
            { 1049706, 1 }, // 10
            { 1016007, 2 }, // 15
            { 1062239, 3 }// 20
        };

        private static readonly int[][,] _Filters =
        {
            _TypeFilters,
            _QualityFilters,
            _MaterialFilters,
            _AmountFilters
        };

        private static readonly int[] _XOffsetsType = { 0, 75, 170 };
        private static readonly int[] _XOffsetsQuality = { 0, 75, 170 };
        private static readonly int[] _XOffsetsAmount = { 0, 75, 180, 275 };
        private static readonly int[] _XOffsetsMaterial = { 0, 108, 212, 307, 392, 487 };
        private static readonly int[] _XWidthsSmall = { 50, 50, 70, 50 };
        private static readonly int[] _XWidthsLarge = { 80, 60, 60, 60, 60, 60 };

        private const int _LabelColor = 0x7FFF;

        private readonly PlayerMobile _From;
        private readonly BulkOrderBook _Book;

        public BOBFilterGump(PlayerMobile from, BulkOrderBook book)
            : base(12, 24)
        {
            from.CloseGump(typeof(BOBGump));
            from.CloseGump(typeof(BOBFilterGump));

            _From = from;
            _Book = book;

            BOBFilter f = from.UseOwnFilter ? from.BOBFilter : book.Filter;

            AddPage(0);

            AddBackground(10, 10, 600, 695, 5054);

            AddImageTiled(18, 20, 583, 676, 2624);
            AddAlphaRegion(18, 20, 583, 676);

            AddImage(5, 5, 10460);
            AddImage(585, 5, 10460);
            AddImage(5, 690, 10460);
            AddImage(585, 690, 10460);

            AddHtmlLocalized(270, 32, 200, 32, 1062223, _LabelColor, false, false); // Filter Preference

            AddHtmlLocalized(26, 64, 120, 32, 1062228, _LabelColor, false, false); // Bulk Order Type
            AddFilterList(25, 96, _XOffsetsType, 40, _TypeFilters, _XWidthsSmall, f.Type, 0);

            AddHtmlLocalized(320, 64, 50, 32, 1062215, _LabelColor, false, false); // Quality
            AddFilterList(320, 96, _XOffsetsQuality, 40, _QualityFilters, _XWidthsSmall, f.Quality, 1);

            AddHtmlLocalized(26, 130, 120, 32, 1062232, _LabelColor, false, false); // Material Type
            AddFilterList(25, 162, _XOffsetsMaterial, 35, _MaterialFilters, _XWidthsLarge, f.Material, 2);

            AddHtmlLocalized(26, 608, 120, 32, 1062217, _LabelColor, false, false); // Amount
            AddFilterList(25, 640, _XOffsetsAmount, 40, _AmountFilters, _XWidthsSmall, f.Quantity, 3);

            AddHtmlLocalized(75, 670, 120, 32, 1062477, (from.UseOwnFilter ? _LabelColor : 16927), false, false); // Set Book Filter
            AddButton(40, 670, 4005, 4007, 1, GumpButtonType.Reply, 0);

            AddHtmlLocalized(235, 670, 120, 32, 1062478, (from.UseOwnFilter ? 16927 : _LabelColor), false, false); // Set Your Filter
            AddButton(200, 670, 4005, 4007, 2, GumpButtonType.Reply, 0);

            AddHtmlLocalized(405, 670, 120, 32, 1062231, _LabelColor, false, false); // Clear Filter
            AddButton(370, 670, 4005, 4007, 3, GumpButtonType.Reply, 0);

            AddHtmlLocalized(540, 670, 50, 32, 1011046, _LabelColor, false, false); // APPLY
            AddButton(505, 670, 4017, 4018, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(Network.NetState sender, RelayInfo info)
        {
            BOBFilter f = _From.UseOwnFilter ? _From.BOBFilter : _Book.Filter;

            int index = info.ButtonID;

            switch (index)
            {
                case 0: // Apply
                    {
                        _From.SendGump(new BOBGump(_From, _Book));

                        break;
                    }
                case 1: // Set Book Filter
                    {
                        _From.UseOwnFilter = false;
                        _From.SendGump(new BOBFilterGump(_From, _Book));

                        break;
                    }
                case 2: // Set Your Filter
                    {
                        _From.UseOwnFilter = true;
                        _From.SendGump(new BOBFilterGump(_From, _Book));

                        break;
                    }
                case 3: // Clear Filter
                    {
                        f.Clear();
                        _From.SendGump(new BOBFilterGump(_From, _Book));

                        break;
                    }
                default:
                    {
                        index -= 4;

                        int type = index % 4;
                        index /= 4;

                        int[][,] filter = _Filters;

                        if (type >= 0 && type < filter.Length)
                        {
                            int[,] filters = filter[type];

                            if (index >= 0 && index < filters.GetLength(0))
                            {
                                if (filters[index, 0] == 0)
                                    break;

                                switch (type)
                                {
                                    case 0:
                                        f.Type = filters[index, 1];
                                        break;
                                    case 1:
                                        f.Quality = filters[index, 1];
                                        break;
                                    case 2:
                                        f.Material = filters[index, 1];
                                        break;
                                    case 3:
                                        f.Quantity = filters[index, 1];
                                        break;
                                }

                                _From.SendGump(new BOBFilterGump(_From, _Book));
                            }
                        }

                        break;
                    }
            }
        }

        private void AddFilterList(int x, int y, int[] xOffsets, int yOffset, int[,] filters, int[] xWidths, int filterValue, int filterIndex)
        {
            for (int i = 0; i < filters.GetLength(0); ++i)
            {
                int number = filters[i, 0];

                if (number == 0)
                {
                    continue;
                }

                bool isSelected = (filters[i, 1] == filterValue);

                if (!isSelected && (i % xOffsets.Length) == 0)
                {
                    isSelected = (filterValue == 0);
                }

                AddHtmlLocalized(x + 35 + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), xWidths[i % xOffsets.Length], 32, number, isSelected ? 16927 : _LabelColor, false, false);
                AddButton(x + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), 4005, 4007, 4 + filterIndex + (i * 4), GumpButtonType.Reply, 0);
            }
        }
    }
}
