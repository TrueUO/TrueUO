using System;
using System.Collections.Generic;
using System.IO;
using Server.Accounting;
using Server.Diagnostics;
using Server.Engines.Chat;
using Server.Engines.Harvest;
using Server.Engines.Help;
using Server.Engines.Quests;
using Server.Engines.UOStore;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Menus;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Prompts;
using Server.Services.Virtues;
using Server.Targeting;
using static Server.Network.PacketHandlers;

namespace Server.Network
{
    public static class PlayerPackets
    {
        public static void Configure()
        {
            Register(0x00, 104, false, CreateCharacter);
            Register(0x01, 5, false, Disconnect);
            Register(0x02, 7, true, MovementReq);
            Register(0x03, 0, true, AsciiSpeech);
            Register(0x04, 2, true, GodModeRequest);
            Register(0x05, 5, true, AttackReq);
            Register(0x06, 5, true, UseReq);
            Register(0x07, 7, true, LiftReq);
            Register(0x08, 15, true, DropReq);
            Register(0x09, 5, true, LookReq);
            Register(0x0A, 11, true, Edit);
            Register(0x12, 0, true, TextCommand);
            Register(0x13, 10, true, EquipReq);
            Register(0x14, 6, true, ChangeZ);
            Register(0x22, 3, true, Resynchronize);
            Register(0x2C, 2, true, DeathStatusResponse);
            Register(0x34, 10, true, MobileQuery);
            Register(0x3A, 0, true, ChangeSkillLock);
            Register(0x3B, 0, true, VendorBuyReply);
            Register(0x47, 11, true, NewTerrain);
            Register(0x48, 73, true, NewAnimData);
            Register(0x58, 106, true, NewRegion);
            Register(0x5D, 73, false, PlayCharacter);
            Register(0x61, 9, true, DeleteStatic);
            Register(0x6C, 19, true, TargetResponse);
            Register(0x6F, 0, true, SecureTrade);
            Register(0x72, 5, true, SetWarMode);
            Register(0x73, 2, false, PingReq);
            Register(0x75, 35, true, RenameRequest);
            Register(0x79, 9, true, ResourceQuery);
            Register(0x7D, 13, true, MenuResponse);
            Register(0x7E, 2, true, GodviewQuery);
            Register(0x80, 62, false, AccountLogin);
            Register(0x9A, 0, true, AsciiPromptResponse);
            Register(0x9B, 258, true, HelpRequest);
            Register(0xAD, 0, true, UnicodeSpeech);
            Register(0xB1, 0, true, DisplayGumpResponse);
            Register(0xB3, 0, true, ChatAction);
            Register(0xB5, 0x40, true, OpenChatWindowRequest);
            Register(0xB8, 0, true, ProfileReq);
            Register(0xCF, 0, false, AccountLogin);
            Register(0xEC, 0, true, EquipMacro);
            Register(0xED, 0, true, UnequipMacro);
            Register(0xFA, 1, true, UOStoreRequest);
            Register(0xF8, 106, false, CreateCharacter);
            Register(0x8D, 0, false, ECCreateCharacter);

            // Extended
            RegisterExtended(0x1C, true, CastSpell);
            RegisterExtended(0x2C, true, BandageTarget);
            RegisterExtended(0x2D, true, TargetedSpell);
            RegisterExtended(0x2E, true, TargetedSkillUse);
            RegisterExtended(0x30, true, TargetByResourceMacro);
            RegisterExtended(0x33, true, MultiMouseBoatMovementRequest);

            // Encoded
            RegisterEncoded(0x19, true, SetWeaponAbility);
            RegisterEncoded(0x1E, true, EquipLastWeaponRequest);
            RegisterEncoded(0x28, true, GuildGumpRequest);
            RegisterExtended(0x2A, true, HeritageTransform);
            RegisterEncoded(0x32, true, QuestGumpRequest);
        }

        public static void CreateCharacter(NetState state, PacketReader reader)
		{
            reader.Seek(9, SeekOrigin.Current);
            /*
			int unk1 = reader.ReadInt32();
			int unk2 = reader.ReadInt32();
			int unk3 = reader.ReadByte();
            */

			string name = reader.ReadString(30);

			reader.Seek(2, SeekOrigin.Current);
			int flags = reader.ReadInt32();
			reader.Seek(8, SeekOrigin.Current);
			int prof = reader.ReadByte();
			reader.Seek(15, SeekOrigin.Current);

			int genderRace = reader.ReadByte();

			int str = reader.ReadByte();
			int dex = reader.ReadByte();
			int intl = reader.ReadByte();
			int is1 = reader.ReadByte();
			int vs1 = reader.ReadByte();
			int is2 = reader.ReadByte();
			int vs2 = reader.ReadByte();
			int is3 = reader.ReadByte();
			int vs3 = reader.ReadByte();
			int is4 = reader.ReadByte();
			int vs4 = reader.ReadByte();

			int hue = reader.ReadUInt16();
			int hairVal = reader.ReadInt16();
			int hairHue = reader.ReadInt16();
			int hairValf = reader.ReadInt16();
			int hairHuef = reader.ReadInt16();
            reader.ReadByte();
			int cityIndex = reader.ReadByte();

            reader.Seek(8, SeekOrigin.Current);
            /*
			int charSlot = reader.ReadInt32();
			int clientIP = reader.ReadInt32();
            */

			int shirtHue = reader.ReadInt16();
			int pantsHue = reader.ReadInt16();

			/*
			0x00, 0x01
			0x02, 0x03 -> Human Male, Human Female
			0x04, 0x05 -> Elf Male, Elf Female
			0x05, 0x06 -> Gargoyle Male, Gargoyle Female
			*/

			bool female = genderRace % 2 != 0;

            byte raceId = (byte)(genderRace < 4 ? 0 : genderRace / 2 - 1);
			Race race = Race.Races[raceId] ?? Race.DefaultRace;

            hue = race.ClipSkinHue(hue);
			hairHue = race.ClipHairHue(hairHue);
			hairHuef = race.ClipHairHue(hairHuef);

			shirtHue = Math.Max(0, Math.Min(1000, shirtHue));
			pantsHue = Math.Max(0, Math.Min(1000, pantsHue));

			CityInfo[] info = state.CityInfo;
			IAccount a = state.Account;

			if (info == null || a == null || cityIndex >= info.Length)
			{
				state.Dispose();

                return;
            }

            // Check if anyone is using this account
            for (int i = 0; i < a.Length; ++i)
            {
                Mobile check = a[i];

                if (check != null && check.Map != Map.Internal)
                {
                    state.Send(new PopupMessage(PMMessage.CharInWorld));

                    return;
                }
            }

            state.Flags = (ClientFlags)flags;

            CharacterCreationArguments args = new CharacterCreationArguments
            (
                state,
                a,
                name,
                female,
                hue,
                str,
                dex,
                intl,
                info[cityIndex],
                [
                    new SkillNameValue((SkillName)is1, vs1), new SkillNameValue((SkillName)is2, vs2),
                    new SkillNameValue((SkillName)is3, vs3), new SkillNameValue((SkillName)is4, vs4)
                ],
                shirtHue,
                pantsHue,
                hairVal,
                hairHue,
                hairValf,
                hairHuef,
                prof,
                race
            );

            if (state.Version == null)
            {
                state.Send(new ClientVersionReq());
                state.BlockAllPackets = true;
            }

            CharacterCreation.OnCharacterCreation(args);

            Mobile m = args.Mobile;

            if (m != null)
            {
                state.Mobile = m;
                m.NetState = state;

                if (state.Version == null)
                {
                    new LoginTimer(state).Start();
                }
                else
                {
                    DoLogin(state);
                }
            }
            else
            {
                state.BlockAllPackets = false;
                state.Dispose();
            }
        }

        public static void ECCreateCharacter(NetState state, PacketReader pvSrc)
		{
			int length = pvSrc.Size;

			int unk1 = pvSrc.ReadInt32(); // Pattern
			int charSlot = pvSrc.ReadInt32();
			string name = pvSrc.ReadString(30);
			string unknown1 = pvSrc.ReadString(30); // "Unknown"

			int profession = pvSrc.ReadByte();
			int cityIndex = pvSrc.ReadByte();

			int gender = pvSrc.ReadByte();
			int genderRace = pvSrc.ReadByte();

			int str = pvSrc.ReadByte();
			int dex = pvSrc.ReadByte();
			int intel = pvSrc.ReadByte();

			int hue = pvSrc.ReadInt16();
			int unk5 = pvSrc.ReadInt32(); // 0x00 0x00 0x00 0x00
			int unk6 = pvSrc.ReadInt32(); // 0x00 0x00 0x00 0x00

			// isX = skill amount | vsX = skill
			int is1 = pvSrc.ReadByte();
			int vs1 = pvSrc.ReadByte();
			int is2 = pvSrc.ReadByte();
			int vs2 = pvSrc.ReadByte();
			int is3 = pvSrc.ReadByte();
			int vs3 = pvSrc.ReadByte();
			int is4 = pvSrc.ReadByte();
			int vs4 = pvSrc.ReadByte();

			string unknown2 = pvSrc.ReadString(25); // Pack of 0x00
			int unk7 = pvSrc.ReadByte(); // Another 0x00

			int hairColor = pvSrc.ReadInt16();
			int hairID = pvSrc.ReadInt16();

			int unk8 = pvSrc.ReadByte();
			int unk9 = pvSrc.ReadInt32();
			int unk10 = pvSrc.ReadByte();
			int shirtHue = pvSrc.ReadInt16();
			int shirtID = pvSrc.ReadInt16();
			int unk13 = pvSrc.ReadByte();

			int faceColor = pvSrc.ReadInt16();
			int faceID = pvSrc.ReadInt16();

			int unk14 = pvSrc.ReadByte();

			int beardColor = pvSrc.ReadInt16();
			int beardID = pvSrc.ReadInt16();

			int pantsHue = shirtHue; // Obsolete

            var female = gender != 0;
			var race = Race.Races[(byte)(genderRace - 1)];

			if (race == null)
				race = Race.DefaultRace;

			CityInfo[] info = state.CityInfo;
			IAccount a = state.Account;

			if (info == null || a == null || cityIndex >= info.Length)
			{
				state.Dispose();
			}
			else
			{
				// Check if anyone is using this account
				for (int i = 0; i < a.Length; ++i)
				{
					Mobile check = a[i];

					if (check != null && check.Map != Map.Internal)
					{
						Console.WriteLine("Login: {0}: Account in use", state);
						state.Send(new PopupMessage(PMMessage.CharInWorld));
						return;
					}
				}

                CharacterCreationArguments args = new CharacterCreationArguments
                (
					state, a,
					name, female, hue,
					str, dex, intel,
					info[cityIndex],
                    [
                        new SkillNameValue( (SkillName)is1, vs1 ),
						new SkillNameValue( (SkillName)is2, vs2 ),
						new SkillNameValue( (SkillName)is3, vs3 ),
						new SkillNameValue( (SkillName)is4, vs4 )
                    ],
					shirtHue, pantsHue,
					hairID, hairColor,
					beardID, beardColor,
					profession, race,
					faceID, faceColor
                );

				if (state.Version == null)
				{
					state.Send(new ClientVersionReq());

					state.BlockAllPackets = true;
				}

                CharacterCreation.OnCharacterCreation(args);

				Mobile m = args.Mobile;

				if (m != null)
				{
					state.Mobile = m;
					m.NetState = state;

					if (state.Version == null)
					{
						new LoginTimer(state).Start();
					}
					else
					{
						DoLogin(state);
					}
				}
				else
				{
					state.BlockAllPackets = false;
					state.Dispose();
				}
			}
		}

        public static void DoLogin(NetState state)
        {
            Mobile m = state.Mobile;

            state.Send(new LoginConfirm(m));

            if (m.Map != null)
            {
                state.Send(new MapChange(m));
            }

            state.Send(new MapPatches());

            state.Send(SupportedFeatures.Instantiate(state));

            state.Sequence = 0;

            state.Send(MobileIncoming.Create(state, m, m));

            state.Send(new MobileUpdate(m));

            m.SendEverything();

            m.CheckLightLevels(true);

            state.Send(LoginComplete.Instance);

            state.Send(MobileIncoming.Create(state, m, m));

            state.Send(new MobileStatus(m, m));

            state.Send(Network.SetWarMode.Instantiate(m.Warmode));

            state.Send(SeasonChange.Instantiate(m.GetSeason(), true));

            state.Send(new CurrentTime());

            state.Send(new MapChange(m));

            Console.WriteLine("Client: {0}: Entered World ({1})", state, m);

            m.OnLogin();
            m.SendEverything();
            m.ClearFastwalkStack();
        }

        public static void Disconnect(NetState state, PacketReader pvSrc)
        {
            int minusOne = pvSrc.ReadInt32();
        }

        public static void MovementReq(NetState state, PacketReader pvSrc)
        {
            Direction dir = (Direction)pvSrc.ReadByte();
            int seq = pvSrc.ReadByte();
            int key = pvSrc.ReadInt32();

            Mobile m = state.Mobile;

            if ((state.Sequence == 0 && seq != 0) || !m.Move(dir))
            {
                state.Send(new MovementRej(seq, m));
                state.Sequence = 0;

                m.ClearFastwalkStack();
            }
            else
            {
                ++seq;

                if (seq == 256)
                {
                    seq = 1;
                }

                state.Sequence = seq;
            }
        }

        private static readonly int[] _EmptyInts = [];
        public static void AsciiSpeech(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            MessageType type = (MessageType)pvSrc.ReadByte();
            int hue = pvSrc.ReadInt16();
            pvSrc.ReadInt16(); // font
            string text = pvSrc.ReadStringSafe().Trim();

            if (text.Length <= 0 || text.Length > 128)
            {
                return;
            }

            if (!Enum.IsDefined(typeof(MessageType), type))
            {
                type = MessageType.Regular;
            }

            from.DoSpeech(text, _EmptyInts, type, Utility.ClipDyedHue(hue));
        }

        public static void GodModeRequest(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                state.Send(new GodModeReply(pvSrc.ReadBoolean()));
            }
        }

        public static void AttackReq(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;
            Mobile m = World.FindMobile(pvSrc.ReadInt32());

            if (m != null)
            {
                from.Attack(m);
            }
        }

        public static void UseReq(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
            {
                int value = pvSrc.ReadInt32();

                if ((value & ~0x7FFFFFFF) != 0)
                {
                    from.OnPaperdollRequest();
                }
                else
                {
                    Serial s = value;

                    if (s.IsMobile)
                    {
                        Mobile m = World.FindMobile(s);

                        if (m != null && !m.Deleted)
                        {
                            from.Use(m);
                        }
                    }
                    else if (s.IsItem)
                    {
                        Item item = World.FindItem(s);

                        if (item != null && !item.Deleted)
                        {
                            from.Use(item);
                        }
                    }
                }

                from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
            }
            else
            {
                from.SendActionMessage();
            }
        }

        public static void LiftReq(NetState state, PacketReader pvSrc)
        {
            Serial serial = pvSrc.ReadInt32();
            int amount = pvSrc.ReadUInt16();
            Item item = World.FindItem(serial);


            state.Mobile.Lift(item, amount, out bool rejected, out LRReason reject);
        }

        public static void DropReq(NetState state, PacketReader pvSrc)
        {
            Serial serial = pvSrc.ReadInt32();
            int x = pvSrc.ReadInt16();
            int y = pvSrc.ReadInt16();
            int z = pvSrc.ReadSByte();
            byte gridloc = pvSrc.ReadByte(); // grid location
            Serial dest = pvSrc.ReadInt32();

            Point3D loc = new Point3D(x, y, z);
            Mobile from = state.Mobile;

            if (serial.IsItem)
            {
                Item dropped = World.FindItem(serial);

                if (dropped != null)
                {
                    dropped.GridLocation = gridloc;
                }
            }

            if (dest.IsMobile)
            {
                from.Drop(World.FindMobile(dest), loc);
            }
            else if (dest.IsItem)
            {
                Item item = World.FindItem(dest);

                if (item is BaseMulti multi && multi.AllowsRelativeDrop)
                {
                    loc.m_X += multi.X;
                    loc.m_Y += multi.Y;
                    from.Drop(loc);
                }
                else
                {
                    from.Drop(item, loc);
                }
            }
            else
            {
                from.Drop(loc);
            }
        }

        public static void LookReq(NetState state, PacketReader pvSrc)
        {
            if (state.Mobile != null)
            {
                HandleSingleClick(state.Mobile, World.FindEntity(pvSrc.ReadInt32()));
            }
        }
        private static void HandleSingleClick(Mobile m, IEntity target)
        {
            if (m == null || target == null || target.Deleted || !m.CanSee(target))
            {
                return;
            }

            if (target is Item ti)
            {
                if (Utility.InUpdateRange(m.Location, ti.GetWorldLocation()))
                {
                    ti.OnAosSingleClick(m);
                }
            }
            else if (target is Mobile tm)
            {
                if (Utility.InUpdateRange(m, tm))
                {
                    tm.OnAosSingleClick(m);
                }
            }
        }

        public static void Edit(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                int type = pvSrc.ReadByte(); // 10 = static, 7 = npc, 4 = dynamic
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int id = pvSrc.ReadInt16();
                int z = pvSrc.ReadSByte();
                int hue = pvSrc.ReadUInt16();

                Console.WriteLine("God Client: {0}: Edit {6} ({1}, {2}, {3}) 0x{4:X} (0x{5:X})", state, x, y, z, id, hue, type);
            }
        }

        public static void TextCommand(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			string command = pvSrc.ReadString();

			Mobile m = state.Mobile;

			switch (type)
			{
				case 0x00: // Go
				{
					if (VerifyGC(state))
					{
						try
						{
							string[] split = command.Split(' ');

							int x = Utility.ToInt32(split[0]);
							int y = Utility.ToInt32(split[1]);

							int z;

							if (split.Length >= 3)
							{
								z = Utility.ToInt32(split[2]);
							}
							else if (m.Map != null)
							{
								z = m.Map.GetAverageZ(x, y);
							}
							else
							{
								z = 0;
							}

							m.Location = new Point3D(x, y, z);
						}
						catch (Exception e)
						{
                            ExceptionLogging.LogException(e);
						}
					}

					break;
				}
				case 0xC7: // Animate
				{
                    Animations.AnimateRequest(m, command);

					break;
				}
				case 0x24: // Use skill
				{
					if (!int.TryParse(command.Split(' ')[0], out int skillIndex))
					{
						break;
					}

					Skills.UseSkill(m, skillIndex);

					break;
				}
				case 0x43: // Open spellbook
				{

					if (!int.TryParse(command, out int bookType))
					{
                        bookType = 1;
					}

                    Spellbook.OpenSpellbookRequest(m, bookType);

					break;
				}
				case 0x27: // Cast spell from book
				{
					string[] split = command.Split(' ');

					if (split.Length > 0)
					{
						int spellID = Utility.ToInt32(split[0]) - 1;
						int serial = split.Length > 1 ? Utility.ToInt32(split[1]) : -1;

                        Spellbook.CastSpellRequest(m, spellID, World.FindItem(serial));
					}

					break;
				}
				case 0x58: // Open door
				{
					BaseDoor.OpenDoorMacroUsed(m);

					break;
				}
				case 0x56: // Cast spell from macro
				{
					int spellID = Utility.ToInt32(command) - 1;

                    Spellbook.CastSpellRequest(m, spellID, null);

					break;
				}
				case 0xF4: // Invoke virtues from macro
				{
					int virtueId = Utility.ToInt32(command) - 1;

                    VirtueGump.VirtueMacroRequest(m, virtueId);

					break;
				}
				case 0x2F: // Old scroll double click
				{
					/*
				 * This command is still sent for items 0xEF3 - 0xEF9
				 *
				 * Command is one of three, depending on the item ID of the scroll:
				 * - [scroll serial]
				 * - [scroll serial] [target serial]
				 * - [scroll serial] [x] [y] [z]
				 */
					break;
				}
				default:
				{
					Console.WriteLine("Client: {0}: Unknown text-command type 0x{1:X2}: {2}", state, type, command);
					break;
				}
			}
		}

        public static void EquipReq(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;
            Item item = from.Holding;

            bool valid = item != null && item.HeldBy == from && item.Map == Map.Internal;

            from.Holding = null;

            if (!valid)
            {
                return;
            }

            pvSrc.Seek(5, SeekOrigin.Current);
            Mobile to = World.FindMobile(pvSrc.ReadInt32());

            if (to == null)
            {
                to = from;
            }

            if (!to.AllowEquipFrom(from) || !to.EquipItem(item))
            {
                item.Bounce(from);
            }

            item.ClearBounce();
        }

        public static void ChangeZ(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int z = pvSrc.ReadSByte();

                Console.WriteLine("God Client: {0}: Change Z ({1}, {2}, {3})", state, x, y, z);
            }
        }

        public static void Resynchronize(NetState state, PacketReader pvSrc)
        {
            Mobile m = state.Mobile;

            state.Send(new MobileUpdate(m));

            state.Send(MobileIncoming.Create(state, m, m));

            m.SendEverything();

            state.Sequence = 0;

            m.ClearFastwalkStack();
        }

        public static void DeathStatusResponse(NetState state, PacketReader pvSrc)
        {
            // Ignored
        }

        public static void MobileQuery(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            pvSrc.ReadInt32(); // 0xEDEDEDED
            int type = pvSrc.ReadByte();

            Serial serial = pvSrc.ReadInt32();

            if (serial.IsMobile)
            {
                Mobile m = World.FindMobile(serial);

                if (m != null)
                {
                    switch (type)
                    {
                        case 0x00: // Unknown, sent by godclient
                        {
                            if (VerifyGC(state))
                            {
                                Console.WriteLine("God Client: {0}: Query 0x{1:X2} on {2} '{3}'", state, type, serial, m.Name);
                            }

                            break;
                        }
                        case 0x04: // Stats
                        {
                            m.OnStatsQuery(from);
                            break;
                        }
                        case 0x05:
                        {
                            m.OnSkillsQuery(from);
                            break;
                        }
                        default:
                        {
                            pvSrc.Trace(state);
                            break;
                        }
                    }
                }
            }
            else if (serial.IsItem && World.FindItem(serial) is IDamageable item)
            {
                switch (type)
                {
                    case 0x00:
                    {
                        if (VerifyGC(state))
                        {
                            Console.WriteLine("God Client: {0}: Query 0x{1:X2} on {2} '{3}'", state, type, serial, item.Name);
                        }

                        break;
                    }
                    case 0x04: // Stats
                    {
                        item.OnStatsQuery(from);
                        break;
                    }
                    case 0x05:
                    {
                        break;
                    }
                    default:
                    {
                        pvSrc.Trace(state);
                        break;
                    }
                }
            }
        }

        public static void ChangeSkillLock(NetState state, PacketReader pvSrc)
        {
            Skill s = state.Mobile.Skills[pvSrc.ReadInt16()];

            if (s != null)
            {
                s.SetLockNoRelay((SkillLock)pvSrc.ReadByte());
            }
        }

        public static void VendorBuyReply(NetState state, PacketReader pvSrc)
        {
            pvSrc.Seek(1, SeekOrigin.Begin);

            int msgSize = pvSrc.ReadUInt16();
            Mobile vendor = World.FindMobile(pvSrc.ReadInt32());
            byte flag = pvSrc.ReadByte();

            if (vendor == null)
            {
                return;
            }

            if (vendor.Deleted || !Utility.RangeCheck(vendor.Location, state.Mobile.Location, 10))
            {
                state.Send(new EndVendorBuy(vendor));
                return;
            }

            if (flag == 0x02)
            {
                msgSize -= 1 + 2 + 4 + 1;

                if (msgSize / 7 > 100)
                {
                    return;
                }

                List<BuyItemResponse> buyList = new List<BuyItemResponse>(msgSize / 7);
                for (; msgSize > 0; msgSize -= 7)
                {
                    byte layer = pvSrc.ReadByte();
                    Serial serial = pvSrc.ReadInt32();
                    int amount = pvSrc.ReadInt16();

                    buyList.Add(new BuyItemResponse(serial, amount));
                }

                if (buyList.Count > 0 && vendor is IVendor v && v.OnBuyItems(state.Mobile, buyList))
                {
                    state.Send(new EndVendorBuy(vendor));
                }
            }
            else
            {
                state.Send(new EndVendorBuy(vendor));
            }
        }

        public static void NewTerrain(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int id = pvSrc.ReadUInt16();
                int width = pvSrc.ReadInt16();
                int height = pvSrc.ReadInt16();

                Console.WriteLine("God Client: {0}: New Terrain ({1}, {2})+({3}, {4}) 0x{5:X4}", state, x, y, width, height, id);
            }
        }

        public static void NewAnimData(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                Console.WriteLine("God Client: {0}: New tile animation", state);

                pvSrc.Trace(state);
            }
        }

        public static void NewRegion(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                string name = pvSrc.ReadString(40);
                int unk = pvSrc.ReadInt32();
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int width = pvSrc.ReadInt16();
                int height = pvSrc.ReadInt16();
                int zStart = pvSrc.ReadInt16();
                int zEnd = pvSrc.ReadInt16();
                string desc = pvSrc.ReadString(40);
                int soundFX = pvSrc.ReadInt16();
                int music = pvSrc.ReadInt16();
                int nightFX = pvSrc.ReadInt16();
                int dungeon = pvSrc.ReadByte();
                int light = pvSrc.ReadInt16();

                Console.WriteLine("God Client: {0}: New Region '{1}' ('{2}')", state, name, desc);
            }
        }

        public static void PlayCharacter(NetState state, PacketReader pvSrc)
        {
            pvSrc.Seek(36, SeekOrigin.Current); // 4 = 0xEDEDEDED, 30 = Name, 2 = unknown
            int flags = pvSrc.ReadInt32();
            pvSrc.Seek(24, SeekOrigin.Current);
            int charSlot = pvSrc.ReadInt32();
            pvSrc.Seek(4, SeekOrigin.Current); // int clientIP = pvSrc.ReadInt32();

            IAccount a = state.Account;

            if (a == null || charSlot < 0 || charSlot >= a.Length)
            {
                state.Send(new PopupMessage(PMMessage.CharNoExist));
                state.Dispose();

                return;
            }

            Mobile m = a[charSlot];

            // Check if anyone is using this account
            for (int i = 0; i < a.Length; ++i)
            {
                Mobile check = a[i];

                if (check != null && check.Map != Map.Internal && check != m)
                {
                    state.Send(new PopupMessage(PMMessage.CharInWorld));

                    return;
                }
            }

            if (m == null)
            {
                state.Send(new PopupMessage(PMMessage.CharNoExist));
                state.Dispose();

                return;
            }

            m.NetState?.Dispose();

            state.Flags = (ClientFlags)flags;

            state.Mobile = m;
            m.NetState = state;

            if (state.Version == null)
            {
                state.Send(new ClientVersionReq());

                state.BlockAllPackets = true;

                new LoginTimer(state).Start();
            }
            else
            {
                DoLogin(state);
            }
        }

        public static void DeleteStatic(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int z = pvSrc.ReadInt16();
                int id = pvSrc.ReadUInt16();

                Console.WriteLine("God Client: {0}: Delete Static ({1}, {2}, {3}) 0x{4:X}", state, x, y, z, id);
            }
        }

        public static void TargetResponse(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			int targetID = pvSrc.ReadInt32();
			int flags = pvSrc.ReadByte();
			Serial serial = pvSrc.ReadInt32();
			int x = pvSrc.ReadInt16(), y = pvSrc.ReadInt16(), z = pvSrc.ReadInt16();
			int graphic = pvSrc.ReadUInt16();

			if (targetID == unchecked((int)0xDEADBEEF))
			{
				return;
			}

			Mobile from = state.Mobile;

			Target t = from.Target;

			if (t != null)
			{
				TargetProfile prof = TargetProfile.Acquire(t.GetType());

				if (prof != null)
				{
					prof.Start();
				}

				try
				{
					if (x == -1 && y == -1 && !serial.IsValid)
					{
                        t.Cancel(from, TargetCancelType.Canceled); // User pressed escape
					}
					else if (Target.TargetIDValidation && t.TargetID != targetID)
					{
                        t.Cancel(from, TargetCancelType.Canceled); // Prevent fake target.
                    }
					else
					{
						object toTarget;

						if (type == 1)
						{
							if (graphic == 0)
							{
								toTarget = new LandTarget(new Point3D(x, y, z), from.Map);
							}
							else
							{
								Map map = from.Map;

								if (map == null || map == Map.Internal)
								{
									t.Cancel(from, TargetCancelType.Canceled);
									return;
								}
								else
								{
									StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, !t.DisallowMultis);

									bool valid = false;

									ItemData id = TileData.ItemTable[graphic & TileData.MaxItemValue];
									
									if (id.Surface)
									{
										z -= id.Height;
									}

									for (int i = 0; !valid && i < tiles.Length; ++i)
									{
										if (tiles[i].Z == z && tiles[i].ID == graphic)
										{
											valid = true;
										}
									}

									if (!valid)
									{
										t.Cancel(from, TargetCancelType.Canceled);
										return;
									}
									else
									{
										toTarget = new StaticTarget(new Point3D(x, y, z), graphic);
									}
								}
							}
						}
						else if (serial.IsMobile)
						{
							toTarget = World.FindMobile(serial);
						}
						else if (serial.IsItem)
						{
							toTarget = World.FindItem(serial);
						}
						else
						{
							t.Cancel(from, TargetCancelType.Canceled);
							return;
						}

						t.Invoke(from, toTarget);
					}
				}
				finally
				{
					if (prof != null)
					{
						prof.Finish();
					}
				}
			}
		}

        public static void SecureTrade(NetState state, PacketReader pvSrc)
        {
            switch (pvSrc.ReadByte())
            {
                case 1: // Cancel
                {
                    Serial serial = pvSrc.ReadInt32();

                    if (World.FindItem(serial) is SecureTradeContainer cont)
                    {
                        SecureTrade trade = cont.Trade;

                        if (trade != null)
                        {
                            if (trade.From.Mobile == state.Mobile || trade.To.Mobile == state.Mobile)
                            {
                                trade.Cancel();
                            }
                        }
                    }
                }
                    break;
                case 2: // Check
                {
                    Serial serial = pvSrc.ReadInt32();

                    if (World.FindItem(serial) is SecureTradeContainer cont)
                    {
                        SecureTrade trade = cont.Trade;

                        bool value = pvSrc.ReadInt32() != 0;

                        if (trade != null)
                        {
                            if (trade.From.Mobile == state.Mobile)
                            {
                                trade.From.Accepted = value;
                                trade.Update();
                            }
                            else if (trade.To.Mobile == state.Mobile)
                            {
                                trade.To.Accepted = value;
                                trade.Update();
                            }
                        }
                    }
                }
                    break;
                case 3: // Update Gold
                {
                    Serial serial = pvSrc.ReadInt32();

                    if (World.FindItem(serial) is SecureTradeContainer cont)
                    {
                        int gold = pvSrc.ReadInt32();
                        int plat = pvSrc.ReadInt32();

                        SecureTrade trade = cont.Trade;

                        if (trade != null)
                        {
                            if (trade.From.Mobile == state.Mobile)
                            {
                                trade.From.Gold = gold;
                                trade.From.Plat = plat;
                                trade.UpdateFromCurrency();
                            }
                            else if (trade.To.Mobile == state.Mobile)
                            {
                                trade.To.Gold = gold;
                                trade.To.Plat = plat;
                                trade.UpdateToCurrency();
                            }
                        }
                    }
                }
                    break;
            }
        }

        public static void SetWarMode(NetState state, PacketReader pvSrc)
        {
            if (state.Mobile.IsStaff() || Core.TickCount - state.Mobile.NextActionTime >= 0)
            {
                state.Mobile.DelayChangeWarmode(pvSrc.ReadBoolean());
            }
            else
            {
                state.Mobile.SendActionMessage();
            }
        }

        public static void PingReq(NetState state, PacketReader pvSrc)
        {
            state.Send(PingAck.Instantiate(pvSrc.ReadByte()));
        }

        public static void RenameRequest(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;
            Mobile targ = World.FindMobile(pvSrc.ReadInt32());

            if (targ != null)
            {
                RenameRequests.RenameRequest(from, targ, pvSrc.ReadStringSafe());
            }
        }

        public static void ResourceQuery(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            { }
        }

        public static void MenuResponse(NetState state, PacketReader pvSrc)
        {
            int serial = pvSrc.ReadInt32();
            int menuID = pvSrc.ReadInt16(); // unused in our implementation
            int index = pvSrc.ReadInt16();
            int itemID = pvSrc.ReadInt16();
            int hue = pvSrc.ReadInt16();

            index -= 1; // convert from 1-based to 0-based

            for (var i = 0; i < state.Menus.Count; i++)
            {
                IMenu menu = state.Menus[i];

                if (menu.Serial == serial)
                {
                    state.RemoveMenu(menu);

                    if (index >= 0 && index < menu.EntryLength)
                    {
                        menu.OnResponse(state, index);
                    }
                    else
                    {
                        menu.OnCancel(state);
                    }

                    break;
                }
            }
        }

        public static void GodviewQuery(NetState state, PacketReader pvSrc)
        {
            if (VerifyGC(state))
            {
                Console.WriteLine("God Client: {0}: Godview query 0x{1:X}", state, pvSrc.ReadByte());
            }
        }

        public static void AsciiPromptResponse(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            if (from == null)
            {
                return;
            }

            int serial = pvSrc.ReadInt32();
            int prompt = pvSrc.ReadInt32();
            int type = pvSrc.ReadInt32();
            string text = pvSrc.ReadStringSafe();

            if (text == null || text.Length > 128)
            {
                return;
            }

            Prompt p = from.Prompt;

            if (p != null && p.Sender.Serial == serial && p.TypeId == prompt)
            {
                from.Prompt = null;

                if (type == 0)
                {
                    p.OnCancel(from);
                }
                else
                {
                    p.OnResponse(from, text);
                }
            }
        }

        public static void HelpRequest(NetState state, PacketReader pvSrc)
        {
            HelpGump.HelpRequest(state.Mobile);
        }

        private static readonly KeywordList _KeywordList = new();
        public static void UnicodeSpeech(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            MessageType type = (MessageType)pvSrc.ReadByte();
            int hue = pvSrc.ReadInt16();
            pvSrc.ReadInt16(); // font
            string lang = pvSrc.ReadString(4);
            string text;

            bool isEncoded = (type & MessageType.Encoded) != 0;
            int[] keywords;

            if (isEncoded)
            {
                int value = pvSrc.ReadInt16();
                int count = (value & 0xFFF0) >> 4;
                int hold = value & 0xF;

                if (count < 0 || count > 50)
                {
                    return;
                }

                KeywordList keyList = _KeywordList;

                for (int i = 0; i < count; ++i)
                {
                    int speechID;

                    if ((i & 1) == 0)
                    {
                        hold <<= 8;
                        hold |= pvSrc.ReadByte();
                        speechID = hold;
                        hold = 0;
                    }
                    else
                    {
                        value = pvSrc.ReadInt16();
                        speechID = (value & 0xFFF0) >> 4;
                        hold = value & 0xF;
                    }

                    if (!keyList.Contains(speechID))
                    {
                        keyList.Add(speechID);
                    }
                }

                text = pvSrc.ReadUTF8StringSafe();

                keywords = keyList.ToArray();
            }
            else
            {
                text = pvSrc.ReadUnicodeStringSafe();

                keywords = _EmptyInts;
            }

            text = text.Trim();

            if (text.Length <= 0 || text.Length > 128)
            {
                return;
            }

            type &= ~MessageType.Encoded;

            if (!Enum.IsDefined(typeof(MessageType), type))
            {
                type = MessageType.Regular;
            }

            from.Language = lang;
            from.DoSpeech(text, keywords, type, Utility.ClipDyedHue(hue));
        }

        public static void DisplayGumpResponse(NetState state, PacketReader pvSrc)
		{
			int serial = pvSrc.ReadInt32();
			int typeID = pvSrc.ReadInt32();
			int buttonID = pvSrc.ReadInt32();

            for (var index = 0; index < state.Gumps.Count; index++)
            {
                Gump gump = state.Gumps[index];

                if (gump.Serial == serial && gump.TypeID == typeID)
                {
                    bool buttonExists = buttonID == 0; // 0 is always 'close'

                    if (!buttonExists)
                    {
                        for (var i = 0; i < gump.Entries.Count; i++)
                        {
                            GumpEntry e = gump.Entries[i];

                            if (e is GumpButton button && button.ButtonID == buttonID)
                            {
                                buttonExists = true;
                                break;
                            }

                            if (e is GumpImageTileButton tileButton && tileButton.ButtonID == buttonID)
                            {
                                buttonExists = true;
                                break;
                            }
                        }
                    }

                    if (!buttonExists)
                    {
                        Utility.PushColor(ConsoleColor.Red);
                        state.WriteConsole("Invalid gump response, disconnecting...");
                        Utility.PopColor();
                        state.Dispose();
                        return;
                    }

                    int switchCount = pvSrc.ReadInt32();

                    if (switchCount < 0 || switchCount > gump.Switches)
                    {
                        Utility.PushColor(ConsoleColor.Red);
                        state.WriteConsole("Invalid gump response, disconnecting...");
                        Utility.PopColor();
                        state.Dispose();
                        return;
                    }

                    int[] switches = new int[switchCount];

                    for (int j = 0; j < switches.Length; ++j)
                    {
                        switches[j] = pvSrc.ReadInt32();
                    }

                    int textCount = pvSrc.ReadInt32();

                    if (textCount < 0 || textCount > gump.TextEntries)
                    {
                        Utility.PushColor(ConsoleColor.Red);
                        state.WriteConsole("Invalid gump response, disconnecting...");
                        Utility.PopColor();
                        state.Dispose();
                        return;
                    }

                    TextRelay[] textEntries = new TextRelay[textCount];

                    for (int j = 0; j < textEntries.Length; ++j)
                    {
                        int entryID = pvSrc.ReadUInt16();
                        int textLength = pvSrc.ReadUInt16();

                        if (textLength > 239)
                        {
                            Utility.PushColor(ConsoleColor.Red);
                            state.WriteConsole("Invalid gump response, disconnecting...");
                            Utility.PopColor();
                            state.Dispose();
                            return;
                        }

                        string text = pvSrc.ReadUnicodeStringSafe(textLength);
                        textEntries[j] = new TextRelay(entryID, text);
                    }

                    state.RemoveGump(gump);

                    GumpProfile prof = GumpProfile.Acquire(gump.GetType());

                    if (prof != null)
                    {
                        prof.Start();
                    }

                    gump.OnResponse(state, new RelayInfo(buttonID, switches, textEntries));

                    if (prof != null)
                    {
                        prof.Finish();
                    }

                    return;
                }
            }

            if (typeID == 461)
			{
				// Virtue gump
				int switchCount = pvSrc.ReadInt32();

				if (buttonID == 1 && switchCount > 0)
				{
					Mobile beheld = World.FindMobile(pvSrc.ReadInt32());

					if (beheld != null)
					{
                        VirtueGump.VirtueGumpRequest(state.Mobile, beheld);
					}
				}
				else
				{
					Mobile beheld = World.FindMobile(serial);

					if (beheld != null)
					{
                        VirtueGump.VirtueItemRequest(state.Mobile, beheld, buttonID);
					}
				}
			}
		}

        public static void ChatAction(NetState state, PacketReader pvSrc)
        {
            if (!ChatSystem.Enabled)
                return;

            try
            {
                Mobile from = state.Mobile;
                ChatUser user = ChatUser.GetChatUser(from);

                if (user == null)
                    return;

                string lang = pvSrc.ReadStringSafe(4);
                short actionId = pvSrc.ReadInt16();
                string param = pvSrc.ReadUnicodeString();

                ChatActionHandler handler = ChatActionHandlers.GetHandler(actionId);

                if (handler != null)
                {
                    Channel channel = user.CurrentChannel;

                    if (handler.RequireConference && channel == null)
                    {
                        /* You must be in a conference to do this.
                         * To join a conference, select one from the Conference menu.
                         */
                        user.SendMessage(31);
                    }
                    else
                    {
                        handler.Callback(user, channel, param);
                    }
                }
                else
                {
                    Console.WriteLine("Client: {0}: Unknown chat action 0x{1:X}: {2}", state, actionId, param);
                }
            }
            catch (Exception e)
            {
                ExceptionLogging.LogException(e);
            }
        }

        public static void OpenChatWindowRequest(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            if (!ChatSystem.Enabled)
            {
                from.SendMessage("The chat system has been disabled.");
                return;
            }

            string chatName = from.Name;

            ChatSystem.SendCommandTo(from, ChatCommand.OpenChatWindow, chatName);
            ChatUser.AddChatUser(from);
        }

        public static void ProfileReq(NetState state, PacketReader pvSrc)
        {
            int type = pvSrc.ReadByte();
            Serial serial = pvSrc.ReadInt32();

            Mobile beholder = state.Mobile;
            Mobile beheld = World.FindMobile(serial);

            if (beheld == null)
            {
                return;
            }

            switch (type)
            {
                case 0x00: // display request
                {
                    Profile.ProfileRequest(beholder, beheld);

                    break;
                }
                case 0x01: // edit request
                {
                    pvSrc.ReadInt16(); // Skip
                    int length = pvSrc.ReadUInt16();

                    if (length > 511)
                    {
                        return;
                    }

                    string text = pvSrc.ReadUnicodeString(length);

                    Profile.ChangeProfileRequest(beholder, beheld, text);

                    break;
                }
            }
        }

        public static void AccountLogin(NetState state, PacketReader pvSrc)
        {
            if (state.SentFirstPacket)
            {
                state.Dispose();
                return;
            }

            state.SentFirstPacket = true;

            string username = pvSrc.ReadString(30);
            string password = pvSrc.ReadString(30);

            AccountLoginEventArgs e = new AccountLoginEventArgs(state, username, password);

            EventSink.InvokeAccountLogin(e);

            if (e.Accepted)
            {
                AccountLogin_ReplyAck(state);
            }
            else
            {
                AccountLogin_ReplyRej(state, e.RejectReason);
            }
        }
        private static void AccountLogin_ReplyAck(NetState state)
        {
            ServerListEventArgs e = new ServerListEventArgs(state, state.Account);

            EventSink.InvokeServerList(e);

            if (e.Rejected)
            {
                state.Account = null;
                state.Send(new AccountLoginRej(ALRReason.BadComm));
                state.Dispose();
            }
            else
            {
                ServerInfo[] info = e.Servers.ToArray();

                state.ServerInfo = info;

                state.Send(new AccountLoginAck(info));
            }
        }
        private static void AccountLogin_ReplyRej(NetState state, ALRReason reason)
        {
            state.Send(new AccountLoginRej(reason));
            state.Dispose();
        }

        public static void EquipMacro(NetState ns, PacketReader pvSrc)
        {
            int count = pvSrc.ReadByte();
            List<Serial> serialList = new List<Serial>(count);

            for (var i = 0; i < count; ++i)
            {
                serialList.Add(pvSrc.ReadInt32());
            }

            PlayerMobile.EquipMacro(ns.Mobile, serialList);
        }

        public static void UnequipMacro(NetState ns, PacketReader pvSrc)
        {
            int count = pvSrc.ReadByte();
            List<Layer> layers = new List<Layer>(count);

            for (int i = 0; i < count; ++i)
            {
                layers.Add((Layer)pvSrc.ReadUInt16());
            }

            PlayerMobile.UnequipMacro(ns.Mobile, layers);
        }

        public static void UOStoreRequest(NetState state, PacketReader pvSrc)
        {
            UltimaStore.OpenStore(state.Mobile as PlayerMobile);
        }

        public static void CastSpell(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            if (from == null)
            {
                return;
            }

            Item spellbook = null;

            if (pvSrc.ReadInt16() == 1)
            {
                spellbook = World.FindItem(pvSrc.ReadInt32());
            }

            int spellID = pvSrc.ReadInt16() - 1;

            Spellbook.CastSpellRequest(from, spellID, spellbook);
        }

        public static void BandageTarget(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;

            if (from == null)
            {
                return;
            }

            if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
            {
                Item bandage = World.FindItem(pvSrc.ReadInt32());

                if (bandage == null)
                {
                    return;
                }

                Mobile target = World.FindMobile(pvSrc.ReadInt32());

                if (target == null)
                {
                    return;
                }

                Bandage.BandageTargetRequest(from, bandage, target);

                from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
            }
            else
            {
                from.SendActionMessage();
            }
        }

        public static void TargetedSpell(NetState ns, PacketReader pvSrc)
        {
            short spellId = (short)(pvSrc.ReadInt16() - 1); // zero based;
            Serial target = pvSrc.ReadInt32();

            Spellbook.TargetedSpell(ns.Mobile, World.FindEntity(target), spellId);
        }

        public static void TargetedSkillUse(NetState ns, PacketReader pvSrc)
        {
            short skillId = pvSrc.ReadInt16();
            Serial target = pvSrc.ReadInt32();

            PlayerMobile.TargetedSkillUse(ns.Mobile, World.FindEntity(target), skillId);
        }

        public static void TargetByResourceMacro(NetState ns, PacketReader pvSrc)
        {
            Serial serial = pvSrc.ReadInt32();
            int resourceType = pvSrc.ReadInt16();

            if (serial.IsItem)
            {
                HarvestSystem.TargetByResourceMacro(ns.Mobile, World.FindItem(serial), resourceType);
            }
        }

        public static void MultiMouseBoatMovementRequest(NetState state, PacketReader reader)
        {
            Serial playerSerial = reader.ReadInt32();
            Direction movement = (Direction)reader.ReadByte();
            reader.ReadByte(); // movement direction duplicated
            int speed = reader.ReadByte();

            Mobile mob = World.FindMobile(playerSerial);
            if (mob == null || mob.NetState == null || !mob.Mounted)
                return;

            IMount multi = mob.Mount;
            if (!(multi is BaseBoat))
                return;

            BaseBoat boat = (BaseBoat)multi;
            boat.OnMousePilotCommand(mob, movement, speed);
        }

        // Encoded
        public static void SetWeaponAbility(NetState state, IEntity e, EncodedReader reader)
        {
            WeaponAbility.SetWeaponAbility(state.Mobile, reader.ReadInt32());
        }

        public static void EquipLastWeaponRequest(NetState state, IEntity e, EncodedReader reader)
        {
            PlayerMobile from = state.Mobile as PlayerMobile;

            if (from == null || from.Backpack == null)
                return;

            if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
            {
                BaseWeapon toEquip = from.LastWeapon;
                BaseWeapon toDisarm = from.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;

                if (toDisarm == null)
                    toDisarm = from.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;

                if (toDisarm != null)
                {
                    from.Backpack.DropItem(toDisarm);
                    from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
                }

                if (toEquip != toDisarm && toEquip != null && toEquip.Movable && toEquip.IsChildOf(from.Backpack))
                {
                    from.EquipItem(toEquip);
                    from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
                }
            }
            else
            {
                from.SendActionMessage();
            }
        }

        public static void GuildGumpRequest(NetState state, IEntity e, EncodedReader reader)
        {
            Guild.GuildGumpRequest(state.Mobile);
        }

        public static void HeritageTransform(NetState state, PacketReader reader)
        {
            Mobile m = state.Mobile;

            if (reader.Size == 5)
            {
                m.SendLocalizedMessage(1073645); // You may try this again later...	
            }
            else if (reader.Size == 15 && HeritageQuester.Check(m))
            {
                bool proceed = false;

                if (HeritageQuester.IsPending(m))
                {
                    proceed = true;

                    HeritageQuester quester = HeritageQuester.Pending(m);
                    m.Race = quester.Race;

                    quester.CheckCompleted(m, true); // removes done quests

                    if (m.Race == Race.Elf)
                        m.SendLocalizedMessage(1073653); // You are now fully initiated into the Elven culture.
                    else if (m.Race == Race.Human)
                        m.SendLocalizedMessage(1073654); // You are now fully human.
                }
                else if (RaceChangeToken.IsPending(m))
                {
                    Race race = RaceChangeToken.GetPendingRace(m);

                    if (race != null)
                    {
                        m.Race = race;

                        proceed = true;
                        m.SendLocalizedMessage(1111914); // You have successfully changed your race.

                        RaceChangeToken.OnRaceChange(m);
                    }
                }

                if (proceed)
                {
                    m.Hue = reader.ReadUInt16();
                    m.HairItemID = reader.ReadUInt16();
                    m.HairHue = reader.ReadUInt16();
                    m.FacialHairItemID = reader.ReadUInt16();
                    m.FacialHairHue = reader.ReadUInt16();
                }
            }

            HeritageQuester.RemovePending(m);
            RaceChangeToken.RemovePending(m);
        }

        public static void QuestGumpRequest(NetState state, IEntity e, EncodedReader reader)
        {
            if (state.Mobile is PlayerMobile from)
            {
                from.CloseGump(typeof(MondainQuestGump));
                from.SendGump(new MondainQuestGump(from));
            }
        }
    }
}
