using Server.Diagnostics;
using Server.Engines.Help;
using Server.Items;
using System;
using System.Collections.Generic;
using Server.Misc;
using Server.Guilds;
using Server.Mobiles;
using Server.Gumps;
using Server.Services.Virtues;

namespace Server.Network
{
    public static class PlayerPackets
    {
        public static void Configure()
        {
            PacketHandlers.Register(0x12, 0, true, TextCommand);
            PacketHandlers.Register(0x73, 2, false, PingReq);
            PacketHandlers.Register(0x75, 35, true, RenameRequest);
            PacketHandlers.Register(0x9B, 258, true, HelpRequest);
            PacketHandlers.Register(0xB1, 0, true, DisplayGumpResponse);
            PacketHandlers.Register(0xB8, 0, true, ProfileReq);
            PacketHandlers.Register(0xEC, 0, true, EquipMacro);
            PacketHandlers.Register(0xED, 0, true, UnequipMacro);

            // Extended
            PacketHandlers.RegisterExtended(0x1C, true, CastSpell);
            PacketHandlers.RegisterExtended(0x2C, true, BandageTarget);
            PacketHandlers.RegisterExtended(0x2D, true, TargetedSpell);
            PacketHandlers.RegisterExtended(0x2E, true, TargetedSkillUse);

            // Encoded
            PacketHandlers.RegisterEncoded(0x19, true, SetWeaponAbility);
            PacketHandlers.RegisterEncoded(0x28, true, GuildGumpRequest);
            PacketHandlers.RegisterEncoded(0x32, true, QuestGumpRequest);
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
					if (PacketHandlers.VerifyGC(state))
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

        public static void HelpRequest(NetState state, PacketReader pvSrc)
        {
            HelpGump.HelpRequest(state.Mobile);
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
            short spellId = (short)(pvSrc.ReadInt16() - 1);    // zero based;
            Serial target = pvSrc.ReadInt32();

            Spellbook.TargetedSpell(ns.Mobile, World.FindEntity(target), spellId);
        }

        public static void TargetedSkillUse(NetState ns, PacketReader pvSrc)
        {
            short skillId = pvSrc.ReadInt16();
            Serial target = pvSrc.ReadInt32();

            PlayerMobile.TargetedSkillUse(ns.Mobile, World.FindEntity(target), skillId);
        }

        public static void SetWeaponAbility(NetState state, IEntity e, EncodedReader reader)
        {
            WeaponAbility.SetWeaponAbility(state.Mobile, reader.ReadInt32());
        }

        public static void GuildGumpRequest(NetState state, IEntity e, EncodedReader reader)
        {
            Guild.GuildGumpRequest(state.Mobile);
        }

        public static void QuestGumpRequest(NetState state, IEntity e, EncodedReader reader)
        {
        }
    }
}
