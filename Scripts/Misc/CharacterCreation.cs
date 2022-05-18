using Server.Accounting;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Misc
{
    public class CharacterCreation
    {
        private static Mobile m_Mobile;

        public static void Initialize()
        {
            // Register our event handler
            EventSink.CharacterCreated += EventSink_CharacterCreated;
        }

        private static void AddBackpack(Mobile m)
        {
            Container pack = m.Backpack;

            if (pack == null)
            {
                pack = new Backpack
                {
                    Movable = false
                };

                m.AddItem(pack);
            }

            PackItem(new Gold(1000)); // Starting gold can be customized here
            PackItem(new SkinningKnife());
        }

        private static void AddShirt(int shirtHue)
        {
            int hue = Utility.ClipDyedHue(shirtHue & 0x3FFF);

            switch (Utility.Random(3))
            {
                case 0:
                    EquipItem(new Shirt(hue), true);
                    break;
                case 1:
                    EquipItem(new FancyShirt(hue), true);
                    break;
                case 2:
                    EquipItem(new Doublet(hue), true);
                    break;
            }
        }

        private static void AddPants(Mobile m, int pantsHue)
        {
            int hue = Utility.ClipDyedHue(pantsHue & 0x3FFF);

            if (m.Female)
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        EquipItem(new Skirt(hue), true);
                        break;
                    case 1:
                        EquipItem(new Kilt(hue), true);
                        break;
                }
            }
            else
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        EquipItem(new LongPants(hue), true);
                        break;
                    case 1:
                        EquipItem(new ShortPants(hue), true);
                        break;
                }
            }
        }

        private static void AddShoes()
        {
            EquipItem(new Shoes(Utility.RandomYellowHue()), true);
        }

        private static Mobile CreateMobile(Account a)
        {
            if (a.Count >= a.Limit)
                return null;

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] == null)
                {
                    return a[i] = new PlayerMobile();
                }
            }

            return null;
        }

        private static void EventSink_CharacterCreated(CharacterCreatedEventArgs args)
        {
            NetState state = args.State;

            if (state == null)
                return;

            Mobile newChar = CreateMobile(args.Account as Account);

            if (newChar == null)
            {
                Utility.PushColor(ConsoleColor.Red);
                Console.WriteLine("Login: {0}: Character creation failed, account full", state);
                Utility.PopColor();
                return;
            }

            args.Mobile = newChar;
            m_Mobile = newChar;

            newChar.Player = true;
            newChar.AccessLevel = args.Account.AccessLevel;
            newChar.Female = args.Female;

            newChar.Race = Race.Human; //Sets body

            newChar.Hue = args.Hue | 0x8000;

            newChar.Hunger = 20;

            if (newChar is PlayerMobile pm)
            {
                double skillcap = Config.Get("PlayerCaps.SkillCap", 1000.0d) / 10;

                if (skillcap != 100.0)
                {
                    for (int i = 0; i < Enum.GetNames(typeof(SkillName)).Length; ++i)
                    {
                        pm.Skills[i].Cap = skillcap;
                    }
                }
            }

            SetName(newChar, args.Name);

            AddBackpack(newChar);

            SetStats(newChar, state, args.Profession, args.Str, args.Dex, args.Int);

            Race race = newChar.Race;

            if (race.ValidateHair(newChar, args.HairID))
            {
                newChar.HairItemID = args.HairID;
                newChar.HairHue = args.HairHue;
            }

            if (race.ValidateFacialHair(newChar, args.BeardID))
            {
                newChar.FacialHairItemID = args.BeardID;
                newChar.FacialHairHue = args.BeardHue;
            }

            int faceID = args.FaceID;

            if (faceID > 0 && race.ValidateFace(newChar.Female, faceID))
            {
                newChar.FaceItemID = faceID;
                newChar.FaceHue = args.FaceHue;
            }
            else
            {
                newChar.FaceItemID = race.RandomFace(newChar.Female);
                newChar.FaceHue = newChar.Hue;
            }

            AddShirt(args.ShirtHue);
            AddPants(newChar, args.PantsHue);
            AddShoes();

            if (TestCenter.Enabled)
            {
                TestCenter.FillBankbox(newChar);
            }

            CityInfo city = args.City;
            Map map = Siege.SiegeShard && city.Map == Map.Trammel ? Map.Felucca : city.Map;

            newChar.MoveToWorld(city.Location, map);

            Utility.PushColor(ConsoleColor.Green);
            Console.WriteLine("Login: {0}: New character being created (account={1})", state, args.Account.Username);
            Utility.PopColor();
            Utility.PushColor(ConsoleColor.DarkGreen);
            Console.WriteLine(" - Character: {0} (serial={1})", newChar.Name, newChar.Serial);
            Console.WriteLine(" - Started: {0} {1} in {2}", city.City, city.Location, city.Map);
            Utility.PopColor();
        }

        private static void FixStats(ref int str, ref int dex, ref int intel, int max)
        {
            int vMax = max - 30;

            int vStr = str - 10;
            int vDex = dex - 10;
            int vInt = intel - 10;

            if (vStr < 0)
                vStr = 0;

            if (vDex < 0)
                vDex = 0;

            if (vInt < 0)
                vInt = 0;

            int total = vStr + vDex + vInt;

            if (total == 0 || total == vMax)
                return;

            double scalar = vMax / (double)total;

            vStr = (int)(vStr * scalar);
            vDex = (int)(vDex * scalar);
            vInt = (int)(vInt * scalar);

            FixStat(ref vStr, (vStr + vDex + vInt) - vMax, vMax);
            FixStat(ref vDex, (vStr + vDex + vInt) - vMax, vMax);
            FixStat(ref vInt, (vStr + vDex + vInt) - vMax, vMax);

            str = vStr + 10;
            dex = vDex + 10;
            intel = vInt + 10;
        }

        private static void FixStat(ref int stat, int diff, int max)
        {
            stat += diff;

            if (stat < 0)
                stat = 0;
            else if (stat > max)
                stat = max;
        }

        private static void SetStats(Mobile m, NetState state, int str, int dex, int intel)
        {
            int max = 90;

            FixStats(ref str, ref dex, ref intel, max);

            if (str < 10 || str > 60 || dex < 10 || dex > 60 || intel < 10 || intel > 60 || str + dex + intel != max)
            {
                str = 10;
                dex = 10;
                intel = 10;
            }

            m.InitStats(str, dex, intel);
        }

        private static void SetName(Mobile m, string name)
        {
            name = name.Trim();

            if (!NameVerification.Validate(name, 2, 16, true, false, true, 1, NameVerification.SpaceDashPeriodQuote))
            {
                name = "Generic Player";
            }

            m.Name = name;
        }

        private static void SetStats(Mobile m, NetState state, int prof, int str, int dex, int intel)
        {
            switch (prof)
            {
                case 1: // Warrior
                    {
                        str = 45;
                        dex = 35;
                        intel = 10;
                        break;
                    }
                case 2: // Magician
                    {
                        str = 25;
                        dex = 20;
                        intel = 45;
                        break;
                    }
                case 3: // Blacksmith
                    {
                        str = 60;
                        dex = 15;
                        intel = 15;
                        break;
                    }
                case 4: // Necromancer
                    {
                        str = 25;
                        dex = 20;
                        intel = 45;
                        break;
                    }
                case 5: // Paladin
                    {
                        str = 45;
                        dex = 20;
                        intel = 25;
                        break;
                    }
                case 6: //Samurai
                    {
                        str = 40;
                        dex = 30;
                        intel = 20;
                        break;
                    }
                case 7: //Ninja
                    {
                        str = 40;
                        dex = 30;
                        intel = 20;
                        break;
                    }
                default:
                    {
                        SetStats(m, state, str, dex, intel);

                        return;
                    }
            }

            m.InitStats(str, dex, intel);
        }

        private static void EquipItem(Item item)
        {
            EquipItem(item, false);
        }

        private static void EquipItem(Item item, bool mustEquip)
        {
            if (m_Mobile != null && m_Mobile.EquipItem(item))
            {
                return;
            }

            Container pack = m_Mobile.Backpack;

            if (!mustEquip && pack != null)
            {
                pack.DropItem(item);
            }
            else
            {
                item.Delete();
            }
        }

        private static void PackItem(Item item)
        {
            Container pack = m_Mobile.Backpack;

            if (pack != null)
            {
                pack.DropItem(item);
            }
            else
            {
                item.Delete();
            }
        }

        private class BadStartMessage : Timer
        {
            readonly Mobile m_Mobile;
            readonly int m_Message;

            public BadStartMessage(Mobile m, int message)
                : base(TimeSpan.FromSeconds(3.5))
            {
                m_Mobile = m;
                m_Message = message;
                Start();
            }

            protected override void OnTick()
            {
                m_Mobile.SendLocalizedMessage(m_Message);
            }
        }
    }
}
