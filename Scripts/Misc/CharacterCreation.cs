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

            PackItem(new Gold(100)); // Starting gold can be customized here
            PackItem(new Bandage(25));
            PackItem(new SkinningKnife());

            EquipItem(new SkinningKnife());

            EquipItem(new Robe());
            EquipItem(new Shoes(Utility.RandomYellowHue()));
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
            {
                return;
            }

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

            newChar.Race = Race.Human;

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

            SetStats(newChar);
            SetSkills(newChar);

            AddBackpack(newChar);

            Race race = Race.Human;

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

            if (TestCenter.Enabled)
            {
                TestCenter.FillBankbox(newChar);
            }

            CityInfo city = args.City;
            Map map = Map.Trammel;

            newChar.MoveToWorld(city.Location, map);

            Utility.PushColor(ConsoleColor.Green);
            Console.WriteLine("Login: {0}: New character being created (account={1})", state, args.Account.Username);
            Utility.PopColor();
            Utility.PushColor(ConsoleColor.DarkGreen);
            Console.WriteLine(" - Character: {0} (serial={1})", newChar.Name, newChar.Serial);
            Console.WriteLine(" - Started: {0} {1} in {2}", city.City, city.Location, city.Map);
            Utility.PopColor();
        }

        private static void SetName(Mobile m, string name)
        {
            name = name.Trim();

            if (!NameVerification.Validate(name, 2, 16, true, false, true, 1, NameVerification.SpaceDashPeriodQuote))
                name = "Generic Player";

            m.Name = name;
        }

        private static void SetStats(Mobile m)
        {
            m.InitStats(20, 20, 20); // 20 str, 20 dex, 20 int starting stats.
        }

        private static void SetSkills(Mobile m)
        {
            SkillNameValue[] skills =
            {
                new SkillNameValue(SkillName.Anatomy, 20), new SkillNameValue(SkillName.Healing, 20),
                new SkillNameValue(SkillName.Swords, 20), new SkillNameValue(SkillName.Tactics, 20)
            };

            for (int i = 0; i < skills.Length; ++i)
            {
                SkillNameValue snv = skills[i];

                if (snv.Value > 0)
                {
                    Skill skill = m.Skills[snv.Name];

                    if (skill != null)
                    {
                        skill.BaseFixedPoint = snv.Value * 10;
                    }
                }
            }
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

            if (m_Mobile != null)
            {
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
    }
}
