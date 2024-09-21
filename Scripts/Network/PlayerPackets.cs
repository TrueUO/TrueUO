using Server.Diagnostics;
using Server.Engines.Help;
using Server.Items;
using System;
using System.Collections.Generic;
using Server.Misc;
using Server.Guilds;
using Server.Mobiles;

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
            PacketHandlers.Register(0xB8, 0, true, ProfileReq);
            PacketHandlers.Register(0xEC, 0, false, EquipMacro);
            PacketHandlers.Register(0xED, 0, false, UnequipMacro);

            // Extended
            PacketHandlers.RegisterExtended(0x1C, true, CastSpell);
            PacketHandlers.RegisterExtended(0x2C, true, BandageTarget);
            PacketHandlers.RegisterExtended(0x2D, true, TargetedSpell);
            PacketHandlers.RegisterExtended(0x2E, true, TargetedSkillUse);

            // Encoded
            PacketHandlers.RegisterEncoded(0x28, true, GuildGumpRequest);
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
					int virtueID = Utility.ToInt32(command) - 1;

					EventSink.InvokeVirtueMacroRequest(new VirtueMacroRequestEventArgs(m, virtueID));

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

        public static void GuildGumpRequest(NetState state, IEntity e, EncodedReader reader)
        {
            Guild.GuildGumpRequest(state.Mobile);
        }
    }
}
