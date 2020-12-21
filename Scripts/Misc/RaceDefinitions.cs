using System;
using System.Linq;

using Server.Items;

namespace Server.Misc
{
    public class RaceDefinitions
    {
        public static void Configure()
        {
            /* Here we configure all races. Some notes:
            * 
            * 1) The first 32 races are reserved for core use.
            * 2) Race 0x7F is reserved for core use.
            * 3) Race 0xFF is reserved for core use.
            * 4) Changing or removing any predefined races may cause server instability.
            */

            RegisterRace(new Human(0, 0));
        }

        public static void RegisterRace(Race race)
        {
            Race.Races[race.RaceIndex] = race;
            Race.AllRaces.Add(race);
        }

        private class Human : Race
        {
            public Human(int raceID, int raceIndex)
                : base(raceID, raceIndex, "Human", "Humans", 400, 401, 402, 403)
            {
            }

            public override bool ValidateHair(bool female, int itemID)
            {
                if (itemID == 0)
                    return true;

                if (female && itemID == 0x2048 || !female && itemID == 0x2046)
                    return false;	//Buns & Receeding Hair

                if (itemID >= 0x203B && itemID <= 0x203D)
                    return true;

                if (itemID >= 0x2044 && itemID <= 0x204A)
                    return true;

                return false;
            }

            public override int RandomHair(bool female)	//Random hair doesn't include baldness
            {
                switch (Utility.Random(9))
                {
                    case 0:
                        return 0x203B;	//Short
                    case 1:
                        return 0x203C;	//Long
                    case 2:
                        return 0x203D;	//Pony Tail
                    case 3:
                        return 0x2044;	//Mohawk
                    case 4:
                        return 0x2045;	//Pageboy
                    case 5:
                        return 0x2047;	//Afro
                    case 6:
                        return 0x2049;	//Pig tails
                    case 7:
                        return 0x204A;	//Krisna
                    default:
                        return female ? 0x2046 : 0x2048;	//Buns or Receeding Hair
                }
            }

            public override bool ValidateFacialHair(bool female, int itemID)
            {
                if (itemID == 0)
                    return true;

                if (female)
                    return false;

                if (itemID >= 0x203E && itemID <= 0x2041)
                    return true;

                if (itemID >= 0x204B && itemID <= 0x204D)
                    return true;

                return false;
            }

            public override int RandomFacialHair(bool female)
            {
                if (female)
                    return 0;

                int rand = Utility.Random(7);

                return (rand < 4 ? 0x203E : 0x2047) + rand;
            }

            public override bool ValidateFace(bool female, int itemID)
            {
                if (itemID.Equals(0))
                    return false;

                if (itemID >= 0x3B44 && itemID <= 0x3B4D)
                    return true;

                return false;
            }

            public override int RandomFace(bool female)
            {
                int rand = Utility.Random(9);

                return 15172 + rand;
            }

            public override int ClipSkinHue(int hue)
            {
                if (hue < 1002)
                {
                    return 1002;
                }

                if (hue > 1058)
                {
                    return 1058;
                }

                return hue;
            }

            public override int RandomSkinHue()
            {
                return Utility.Random(1002, 57) | 0x8000;
            }

            public override int ClipHairHue(int hue)
            {
                if (hue < 1102)
                {
                    return 1102;
                }

                if (hue > 1149)
                {
                    return 1149;
                }

                return hue;
            }

            public override int RandomHairHue()
            {
                return Utility.Random(1102, 48);
            }

            public override int ClipFaceHue(int hue)
            {
                return ClipSkinHue(hue);
            }

            public override int RandomFaceHue()
            {
                return RandomSkinHue();
            }
        }
    }
}
