using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Server.Accounting;
using Server.ContextMenus;
using Server.Diagnostics;
using Server.Gumps;
using Server.HuePickers;
using Server.Items;
using Server.Prompts;
using Server.Targeting;

namespace Server.Network
{
	public enum LRReason : byte
	{
		CannotLift = 0,
		OutOfRange = 1,
		OutOfSight = 2,
		TryToSteal = 3,
		AreHolding = 4,
		Inspecific = 5
	}

    public enum ALRReason : byte
    {
        Invalid = 0x00,
        InUse = 0x01,
        Blocked = 0x02,
        BadPass = 0x03,
        Idle = 0xFE,
        BadComm = 0xFF
    }

	public sealed class DamagePacket : Packet
	{
		public DamagePacket(IEntity entity, int amount)
			: base(0x0B, 7)
		{
			m_Stream.Write(entity.Serial);

			if (amount > 0xFFFF)
			{
				amount = 0xFFFF;
			}
			else if (amount < 0)
			{
				amount = 0;
			}

			m_Stream.Write((ushort)amount);
		}
	}

    public sealed class CancelArrow : Packet
    {
        public CancelArrow(int x, int y, Serial s)
            : base(0xBA, 10)
        {
            m_Stream.Write((byte)0);
            m_Stream.Write((short)x);
            m_Stream.Write((short)y);
            m_Stream.Write(s);
        }
    }

	public sealed class SetArrow : Packet
	{
		public SetArrow(int x, int y, Serial s)
			: base(0xBA, 10)
		{
			m_Stream.Write((byte)1);
			m_Stream.Write((short)x);
			m_Stream.Write((short)y);
			m_Stream.Write(s);
		}
	}

	public sealed class DisplaySecureTrade : Packet
	{
		public DisplaySecureTrade(IEntity them, IEntity first, IEntity second, string name)
			: base(0x6F)
		{
			if (name == null)
			{
				name = "";
			}

			EnsureCapacity(18 + name.Length);

			m_Stream.Write((byte)0); // Display
			m_Stream.Write(them.Serial);
			m_Stream.Write(first.Serial);
			m_Stream.Write(second.Serial);
			m_Stream.Write(true);

			m_Stream.WriteAsciiFixed(name, 30);
		}
	}

	public sealed class CloseSecureTrade : Packet
	{
		public CloseSecureTrade(IEntity cont)
			: base(0x6F)
		{
			EnsureCapacity(17);

			m_Stream.Write((byte)1); // Close
			m_Stream.Write(cont.Serial);
            m_Stream.Write(0);
            m_Stream.Write(0);
            m_Stream.Write(false);
        }
	}

	public enum TradeFlag : byte
	{
		Display = 0x0,
		Close = 0x1,
		Update = 0x2,
		UpdateGold = 0x3,
		UpdateLedger = 0x4
	}

	public sealed class UpdateSecureTrade : Packet
	{
		public UpdateSecureTrade(IEntity cont, bool first, bool second)
			: this(cont, TradeFlag.Update, first ? 1 : 0, second ? 1 : 0)
		{ }

		public UpdateSecureTrade(IEntity cont, TradeFlag flag, int first, int second)
			: base(0x6F)
		{
			EnsureCapacity(17);

			m_Stream.Write((byte)flag);
			m_Stream.Write(cont.Serial);
			m_Stream.Write(first);
			m_Stream.Write(second);
            m_Stream.Write(false);
        }
	}

	public sealed class SecureTradeEquip : Packet
	{
		public SecureTradeEquip(Item item, IEntity m)
			: base(0x25, 21)
		{
			m_Stream.Write(item.Serial);
			m_Stream.Write((short)item.ItemID);
			m_Stream.Write((byte)0);
			m_Stream.Write((short)item.Amount);
			m_Stream.Write((short)item.X);
			m_Stream.Write((short)item.Y);
			m_Stream.Write((byte)0); // Grid Location?
			m_Stream.Write(m.Serial);
			m_Stream.Write((short)item.Hue);
		}
	}

	public sealed class MapPatches : Packet
	{
		public MapPatches()
			: base(0xBF)
		{
			EnsureCapacity(9 + (3 * 8));

			m_Stream.Write((short)0x0018);

			m_Stream.Write(4);

			m_Stream.Write(Map.Felucca.Tiles.Patch.StaticBlocks);
			m_Stream.Write(Map.Felucca.Tiles.Patch.LandBlocks);

			m_Stream.Write(Map.Trammel.Tiles.Patch.StaticBlocks);
			m_Stream.Write(Map.Trammel.Tiles.Patch.LandBlocks);

			m_Stream.Write(Map.Ilshenar.Tiles.Patch.StaticBlocks);
			m_Stream.Write(Map.Ilshenar.Tiles.Patch.LandBlocks);

			m_Stream.Write(Map.Malas.Tiles.Patch.StaticBlocks);
			m_Stream.Write(Map.Malas.Tiles.Patch.LandBlocks);

			//TODO: Should this include newer facets?
		}
	}

	public sealed class DeathAnimation : Packet
	{
		public DeathAnimation(IEntity killed, IEntity corpse)
			: base(0xAF, 13)
		{
			m_Stream.Write(killed.Serial);
			m_Stream.Write(corpse == null ? Serial.Zero : corpse.Serial);
			m_Stream.Write(0);
		}
	}

	public sealed class StatLockInfo : Packet
	{
		public StatLockInfo(Mobile m)
			: base(0xBF)
		{
			EnsureCapacity(12);

			m_Stream.Write((short)0x19);

			if (m.NetState != null && m.NetState.IsEnhancedClient)
			{
				m_Stream.Write((byte)5);
			}
			else
			{
				m_Stream.Write((byte)2);
			}

			m_Stream.Write(m.Serial);
			m_Stream.Write((byte)0);

			int lockBits = 0;

			lockBits |= (int)m.StrLock << 4;
			lockBits |= (int)m.DexLock << 2;
			lockBits |= (int)m.IntLock;

			m_Stream.Write((byte)lockBits);
		}
	}


	public sealed class ChangeCombatant : Packet
	{
		public ChangeCombatant(IEntity combatant)
			: base(0xAA, 5)
		{
			m_Stream.Write(combatant != null ? combatant.Serial : Serial.Zero);
		}
	}

	public sealed class DisplayHuePicker : Packet
	{
		public DisplayHuePicker(HuePicker huePicker)
			: base(0x95, 9)
		{
			m_Stream.Write(huePicker.Serial);
			m_Stream.Write((short)0);
			m_Stream.Write((short)huePicker.ItemID);
		}
	}

	public sealed class UnicodePrompt : Packet
	{
		public UnicodePrompt(Prompt prompt, IEntity to)
			: base(0xC2)
		{
			EnsureCapacity(21);

			Serial senderSerial = prompt.Sender != null ? prompt.Sender.Serial : to.Serial;

			m_Stream.Write(senderSerial);
			m_Stream.Write(prompt.TypeId); //0x2C
			m_Stream.Write(0); // type
			m_Stream.Write(0); // language
			m_Stream.Write((short)0); // text
		}
	}

    public sealed class DeathStatus : Packet
	{
		public static readonly Packet Dead = SetStatic(new DeathStatus(true));
		public static readonly Packet Alive = SetStatic(new DeathStatus(false));

		public static Packet Instantiate(bool dead)
		{
			return dead ? Dead : Alive;
		}

		public DeathStatus(bool dead)
			: base(0x2C, 2)
		{
			m_Stream.Write((byte)(dead ? 0 : 2));
		}
	}

	public enum SpeedControlType : byte
	{
		Disable,
		MountSpeed,
		WalkSpeed,
		WalkSpeedFast,
		TeleportSpeed
	}

	public sealed class SpeedControl : Packet
	{
		public static readonly Packet TeleportSpeed = SetStatic(new SpeedControl(SpeedControlType.TeleportSpeed));
		public static readonly Packet WalkSpeedFast = SetStatic(new SpeedControl(SpeedControlType.WalkSpeedFast));
		public static readonly Packet WalkSpeed = SetStatic(new SpeedControl(SpeedControlType.WalkSpeed));
		public static readonly Packet MountSpeed = SetStatic(new SpeedControl(SpeedControlType.MountSpeed));
		public static readonly Packet Disable = SetStatic(new SpeedControl(SpeedControlType.Disable));

		public SpeedControl(SpeedControlType type)
			: base(0xBF)
		{
			EnsureCapacity(3);

			m_Stream.Write((short)0x26);
			m_Stream.Write((byte)type);
		}
	}

    public sealed class BondedStatus : Packet
	{
		public BondedStatus(int val1, Serial serial, int val2)
			: base(0xBF)
		{
			EnsureCapacity(11);

			m_Stream.Write((short)0x19);
			m_Stream.Write((byte)val1);
			m_Stream.Write(serial);
			m_Stream.Write((byte)val2);
		}
	}

    [Flags]
	public enum CMEFlags
	{
		None = 0x00,
		Disabled = 0x01,
		Arrow = 0x02,
		Highlighted = 0x04,
		Colored = 0x20
	}

	public sealed class DisplayContextMenu : Packet
	{
		public DisplayContextMenu(ContextMenu menu)
			: base(0xBF)
		{
			ContextMenuEntry[] entries = menu.Entries;

			int length = (byte)entries.Length;

			EnsureCapacity(12 + (length * 8));

			m_Stream.Write((short)0x14);
			m_Stream.Write((short)0x02);

			IEntity target = menu.Target;

			m_Stream.Write(target == null ? Serial.MinusOne : target.Serial);

			m_Stream.Write((byte)length);

			Point3D p;

			if (target is Mobile)
			{
				p = target.Location;
			}
			else if (target is Item item)
			{
				p = item.GetWorldLocation();
			}
			else
			{
				p = Point3D.Zero;
			}

			for (int i = 0; i < length; ++i)
			{
				ContextMenuEntry e = entries[i];

				m_Stream.Write(e.Number);
				m_Stream.Write((short)i);

				int range = e.Range;

				if (range == -1)
				{
					range = 18;
				}

				CMEFlags flags = (e.Enabled && menu.From.InRange(p, range)) ? CMEFlags.None : CMEFlags.Disabled;

				flags |= e.Flags;

				m_Stream.Write((short)flags);
			}
		}
	}

	public sealed class CloseGump : Packet
	{
		public CloseGump(int typeID, int buttonID)
			: base(0xBF)
		{
			EnsureCapacity(13);

			m_Stream.Write((short)0x04);
			m_Stream.Write(typeID);
			m_Stream.Write(buttonID);
		}
	}

	public sealed class EquipUpdate : Packet
	{
		public EquipUpdate(Item item)
			: base(0x2E, 15)
		{
			Serial parentSerial;

			if (item.Parent is Mobile mobile)
			{
				parentSerial = mobile.Serial;
			}
			else
			{
				Console.WriteLine("Warning: EquipUpdate on item with !(parent is Mobile)");
				parentSerial = Serial.Zero;
			}

			int hue = item.Hue;

			if (item.Parent is Mobile mob && mob.SolidHueOverride >= 0)
			{
                hue = mob.SolidHueOverride;
			}

			m_Stream.Write(item.Serial);
			m_Stream.Write((short)item.ItemID);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)item.Layer);
			m_Stream.Write(parentSerial);
			m_Stream.Write((short)hue);
		}
	}

	public sealed class WorldItem : Packet
	{
		public WorldItem(Item item)
			: base(0xF3, 26)
		{
			m_Stream.Write((short)0x1);

			int itemID = item.ItemID;

			if (item is BaseMulti)
			{
				m_Stream.Write((byte)0x02);
				m_Stream.Write(item.Serial);

				itemID &= 0x3FFF;

				m_Stream.Write((ushort)itemID);

				m_Stream.Write((byte)0);
			}
			else
			{
				if (item is IDamageable)
				{
					m_Stream.Write((byte)0x03);
				}
				else
				{
					m_Stream.Write((byte)0x00);
				}

				m_Stream.Write(item.Serial);

				itemID &= 0xFFFF;

				m_Stream.Write((ushort)itemID);

				m_Stream.Write((byte)0);
			}

			int amount = item.Amount;
			m_Stream.Write((short)amount);
			m_Stream.Write((short)amount);

			Point3D loc = item.Location;
			int x = loc.m_X & 0x7FFF;
			int y = loc.m_Y & 0x3FFF;
			m_Stream.Write((short)x);
			m_Stream.Write((short)y);
			m_Stream.Write((sbyte)loc.m_Z);

			m_Stream.Write((byte)item.Light);
			m_Stream.Write((short)item.Hue);
			m_Stream.Write((byte)item.GetPacketFlags());

			m_Stream.Write((short)0x00); // ??
		}
	}

	public sealed class LiftRej : Packet
	{
		public LiftRej(LRReason reason)
			: base(0x27, 2)
		{
			m_Stream.Write((byte)reason);
		}
	}

    public enum EffectType
	{
		Moving = 0x00,
		Lightning = 0x01,
		FixedXYZ = 0x02,
		FixedFrom = 0x03
	}

	public class ParticleEffect : Packet
	{
		public ParticleEffect(
			EffectType type,
			Serial from,
			Serial to,
			int itemID,
			Point3D fromPoint,
			Point3D toPoint,
			int speed,
			int duration,
			bool fixedDirection,
			bool explode,
			int hue,
			int renderMode,
			int effect,
			int explodeEffect,
			int explodeSound,
			Serial serial,
			int layer,
			int unknown)
			: base(0xC7, 49)
		{
			m_Stream.Write((byte)type);
			m_Stream.Write(from);
			m_Stream.Write(to);
			m_Stream.Write((short)itemID);
			m_Stream.Write((short)fromPoint.m_X);
			m_Stream.Write((short)fromPoint.m_Y);
			m_Stream.Write((sbyte)fromPoint.m_Z);
			m_Stream.Write((short)toPoint.m_X);
			m_Stream.Write((short)toPoint.m_Y);
			m_Stream.Write((sbyte)toPoint.m_Z);
			m_Stream.Write((byte)speed);
			m_Stream.Write((byte)duration);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0);
			m_Stream.Write(fixedDirection);
			m_Stream.Write(explode);
			m_Stream.Write(hue);
			m_Stream.Write(renderMode);
			m_Stream.Write((short)effect);
			m_Stream.Write((short)explodeEffect);
			m_Stream.Write((short)explodeSound);
			m_Stream.Write(serial);
			m_Stream.Write((byte)layer);
			m_Stream.Write((short)unknown);
		}

		public ParticleEffect(
			EffectType type,
			Serial from,
			Serial to,
			int itemID,
			IPoint3D fromPoint,
			IPoint3D toPoint,
			int speed,
			int duration,
			bool fixedDirection,
			bool explode,
			int hue,
			int renderMode,
			int effect,
			int explodeEffect,
			int explodeSound,
			Serial serial,
			int layer,
			int unknown)
			: base(0xC7, 49)
		{
			m_Stream.Write((byte)type);
			m_Stream.Write(from);
			m_Stream.Write(to);
			m_Stream.Write((short)itemID);
			m_Stream.Write((short)fromPoint.X);
			m_Stream.Write((short)fromPoint.Y);
			m_Stream.Write((sbyte)fromPoint.Z);
			m_Stream.Write((short)toPoint.X);
			m_Stream.Write((short)toPoint.Y);
			m_Stream.Write((sbyte)toPoint.Z);
			m_Stream.Write((byte)speed);
			m_Stream.Write((byte)duration);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0);
			m_Stream.Write(fixedDirection);
			m_Stream.Write(explode);
			m_Stream.Write(hue);
			m_Stream.Write(renderMode);
			m_Stream.Write((short)effect);
			m_Stream.Write((short)explodeEffect);
			m_Stream.Write((short)explodeSound);
			m_Stream.Write(serial);
			m_Stream.Write((byte)layer);
			m_Stream.Write((short)unknown);
		}
	}

	public class GraphicalEffect : Packet
	{
		public GraphicalEffect(EffectType type, Serial from, Serial to, int itemID, Point3D fromPoint, Point3D toPoint, int speed, int duration, bool fixedDirection, bool explode)
			: this(type, from, to, itemID, fromPoint, toPoint, speed, duration, fixedDirection, explode ? 1 : 0)
		{
		}

		public GraphicalEffect(EffectType type, Serial from, Serial to, int itemID, Point3D fromPoint, Point3D toPoint, int speed, int duration, bool fixedDirection, int explode)
			: base(0x70, 28)
		{
			m_Stream.Write((byte)type);
			m_Stream.Write(from);
			m_Stream.Write(to);
			m_Stream.Write((short)itemID);
			m_Stream.Write((short)fromPoint.X);
			m_Stream.Write((short)fromPoint.Y);
			m_Stream.Write((sbyte)fromPoint.Z);
			m_Stream.Write((short)toPoint.X);
			m_Stream.Write((short)toPoint.Y);
			m_Stream.Write((sbyte)toPoint.Z);
			m_Stream.Write((byte)speed);
			m_Stream.Write((byte)duration);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0);
			m_Stream.Write(fixedDirection);
			m_Stream.Write((byte)explode);
		}
	}

	public class HuedEffect : Packet
	{
		public HuedEffect(
			EffectType type,
			Serial from,
			Serial to,
			int itemID,
			Point3D fromPoint,
			Point3D toPoint,
			int speed,
			int duration,
			bool fixedDirection,
			bool explode,
			int hue,
			int renderMode)
			: base(0xC0, 36)
		{
			m_Stream.Write((byte)type);
			m_Stream.Write(from);
			m_Stream.Write(to);
			m_Stream.Write((short)itemID);
			m_Stream.Write((short)fromPoint.m_X);
			m_Stream.Write((short)fromPoint.m_Y);
			m_Stream.Write((sbyte)fromPoint.m_Z);
			m_Stream.Write((short)toPoint.m_X);
			m_Stream.Write((short)toPoint.m_Y);
			m_Stream.Write((sbyte)toPoint.m_Z);
			m_Stream.Write((byte)speed);
			m_Stream.Write((byte)duration);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0);
			m_Stream.Write(fixedDirection);
			m_Stream.Write(explode);
			m_Stream.Write(hue);
			m_Stream.Write(renderMode);
		}

		public HuedEffect(
			EffectType type,
			Serial from,
			Serial to,
			int itemID,
			IPoint3D fromPoint,
			IPoint3D toPoint,
			int speed,
			int duration,
			bool fixedDirection,
			bool explode,
			int hue,
			int renderMode)
			: this(type, from, to, itemID, fromPoint, toPoint, speed, duration, fixedDirection, explode, hue, renderMode, 0)
		{
		}

		public HuedEffect(
			EffectType type,
			Serial from,
			Serial to,
			int itemID,
			IPoint3D fromPoint,
			IPoint3D toPoint,
			int speed,
			int duration,
			bool fixedDirection,
			bool explode,
			int hue,
			int renderMode,
			int effect)
			: base(0xC0, 36)
		{
			m_Stream.Write((byte)type);
			m_Stream.Write(from);
			m_Stream.Write(to);
			m_Stream.Write((short)itemID);
			m_Stream.Write((short)fromPoint.X);
			m_Stream.Write((short)fromPoint.Y);
			m_Stream.Write((sbyte)fromPoint.Z);
			m_Stream.Write((short)toPoint.X);
			m_Stream.Write((short)toPoint.Y);
			m_Stream.Write((sbyte)toPoint.Z);
			m_Stream.Write((byte)speed);
			m_Stream.Write((byte)duration);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0);
			m_Stream.Write(fixedDirection);
			m_Stream.Write(explode);
			m_Stream.Write(hue);
			m_Stream.Write(effect);
		}
	}

	public sealed class TargetParticleEffect : ParticleEffect
	{
		public TargetParticleEffect(
			IEntity e, int itemID, int speed, int duration, int hue, int renderMode, int effect, int layer, int unknown)
			: base(
				EffectType.FixedFrom,
				e.Serial,
				Serial.Zero,
				itemID,
				e.Location,
				e.Location,
				speed,
				duration,
				true,
				false,
				hue,
				renderMode,
				effect,
				1,
				0,
				e.Serial,
				layer,
				unknown)
		{ }
	}

	public sealed class TargetEffect : HuedEffect
	{
		public TargetEffect(IEntity e, int itemID, int speed, int duration, int hue, int renderMode)
			: base(
				EffectType.FixedFrom,
				e.Serial,
				Serial.Zero,
				itemID,
				e.Location,
				e.Location,
				speed,
				duration,
				true,
				false,
				hue,
				renderMode)
		{ }
	}

	public sealed class LocationParticleEffect : ParticleEffect
	{
		public LocationParticleEffect(
			IEntity e, int itemID, int speed, int duration, int hue, int renderMode, int effect, int unknown)
			: base(
				EffectType.FixedXYZ,
				e.Serial,
				Serial.Zero,
				itemID,
				e.Location,
				e.Location,
				speed,
				duration,
				true,
				false,
				hue,
				renderMode,
				effect,
				1,
				0,
				e.Serial,
				255,
				unknown)
		{ }
	}

	public sealed class LocationEffect : HuedEffect
	{
		public LocationEffect(IPoint3D p, int itemID, int speed, int duration, int hue, int renderMode)
			: base(EffectType.FixedXYZ, Serial.Zero, Serial.Zero, itemID, p, p, speed, duration, true, false, hue, renderMode)
		{ }
	}

	public sealed class MovingParticleEffect : ParticleEffect
	{
		public MovingParticleEffect(
			IEntity from,
			IEntity to,
			int itemID,
			int speed,
			int duration,
			bool fixedDirection,
			bool explodes,
			int hue,
			int renderMode,
			int effect,
			int explodeEffect,
			int explodeSound,
			EffectLayer layer,
			int unknown)
			: base(
				EffectType.Moving,
				from.Serial,
				to.Serial,
				itemID,
				from.Location,
				to.Location,
				speed,
				duration,
				fixedDirection,
				explodes,
				hue,
				renderMode,
				effect,
				explodeEffect,
				explodeSound,
				Serial.Zero,
				(int)layer,
				unknown)
		{ }
	}

	public sealed class MovingEffect : HuedEffect
	{
		public MovingEffect(
			IEntity from,
			IEntity to,
			int itemID,
			int speed,
			int duration,
			bool fixedDirection,
			bool explodes,
			int hue,
			int renderMode)
			: base(
				EffectType.Moving,
				from.Serial,
				to.Serial,
				itemID,
				from.Location,
				to.Location,
				speed,
				duration,
				fixedDirection,
				explodes,
				hue,
				renderMode)
		{ }
	}

	public sealed class BoltEffect : Packet
	{
		public BoltEffect(IEntity target, int hue)
			: base(0xC0, 36)
		{
			m_Stream.Write((byte)0x01); // type
			m_Stream.Write(target.Serial);
			m_Stream.Write(Serial.Zero);
			m_Stream.Write((short)0); // itemID
			m_Stream.Write((short)target.X);
			m_Stream.Write((short)target.Y);
			m_Stream.Write((sbyte)target.Z);
			m_Stream.Write((short)target.X);
			m_Stream.Write((short)target.Y);
			m_Stream.Write((sbyte)target.Z);
			m_Stream.Write((byte)0); // speed
			m_Stream.Write((byte)0); // duration
			m_Stream.Write((short)0); // unk
			m_Stream.Write(false); // fixed direction
			m_Stream.Write(false); // explode
			m_Stream.Write(hue);
			m_Stream.Write(0); // render mode
		}
	}

	public sealed class BoltEffectNew : Packet
	{
		public BoltEffectNew(IEntity target)
			: base(0x70, 28)
		{
			m_Stream.Write((byte)0x01); // type
			m_Stream.Write(target.Serial);
			m_Stream.Write(Serial.Zero);
			m_Stream.Write((short)0); // itemID
			m_Stream.Write((short)target.X);
			m_Stream.Write((short)target.Y);
			m_Stream.Write((sbyte)target.Z);
			m_Stream.Write((short)target.X);
			m_Stream.Write((short)target.Y);
			m_Stream.Write((sbyte)target.Z);
			m_Stream.Write((byte)0); // speed
			m_Stream.Write((byte)0); // duration
			m_Stream.Write((short)0); // unk
			m_Stream.Write(true); // fixed direction
			m_Stream.Write(true); // explode
		}
	}

	public sealed class ContainerDisplay : Packet
	{
		public ContainerDisplay(Container c)
			: base(0x24, 9)
		{
			m_Stream.Write(c.Serial);
			m_Stream.Write((short)c.GumpID);
			m_Stream.Write((short)0x7D);
		}
	}

	public sealed class ContainerContentUpdate : Packet
	{
		public ContainerContentUpdate(Item item)
			: base(0x25, 21)
		{
			Serial parentSerial;

			if (item.Parent is Item parentItem)
			{
				parentSerial = parentItem.Serial;
			}
			else
			{
				Console.WriteLine("Warning: ContainerContentUpdate on item with !(parent is Item)");
				parentSerial = Serial.Zero;
			}

			m_Stream.Write(item.Serial);
			m_Stream.Write((ushort)item.ItemID);
			m_Stream.Write((byte)0); // signed, itemID offset
			m_Stream.Write((ushort)item.Amount);
			m_Stream.Write((short)item.X);
			m_Stream.Write((short)item.Y);
			m_Stream.Write(item.GridLocation);
			m_Stream.Write(parentSerial);
			m_Stream.Write((ushort)(item.QuestItem ? item.QuestItemHue : item.Hue));
		}
	}

	public sealed class ContainerContent : Packet
	{
		public ContainerContent(Mobile beholder, Item beheld)
			: base(0x3C)
		{
			List<Item> items = beheld.Items;
			int count = items.Count;

			EnsureCapacity(5 + (count * 20));

			long pos = m_Stream.Position;

			int written = 0;

			m_Stream.Write((ushort)0);

			for (int i = 0; i < count; ++i)
			{
				Item child = items[i];

				if (!child.Deleted && beholder.CanSee(child))
				{
					Point3D loc = child.Location;

					if (child.GridLocation == 0xFF)
					{
						child.GridLocation = (byte)(count - written);
					}

					m_Stream.Write(child.Serial);
					m_Stream.Write((ushort)child.ItemID);
					m_Stream.Write((byte)0); // signed, itemID offset
					m_Stream.Write((ushort)child.Amount);
					m_Stream.Write((short)loc.m_X);
					m_Stream.Write((short)loc.m_Y);
					m_Stream.Write(child.GridLocation);
					m_Stream.Write(beheld.Serial);
					m_Stream.Write((ushort)(child.QuestItem ? child.QuestItemHue : child.Hue));

					++written;
				}
			}

			m_Stream.Seek(pos, SeekOrigin.Begin);
			m_Stream.Write((ushort)written);
		}
	}

	public sealed class SetWarMode : Packet
	{
		public static readonly Packet InWarMode = SetStatic(new SetWarMode(true));
		public static readonly Packet InPeaceMode = SetStatic(new SetWarMode(false));

		public static Packet Instantiate(bool mode)
		{
			return mode ? InWarMode : InPeaceMode;
		}

		public SetWarMode(bool mode)
			: base(0x72, 5)
		{
			m_Stream.Write(mode);
			m_Stream.Write((byte)0x00);
			m_Stream.Write((byte)0x32);
			m_Stream.Write((byte)0x00);
		}
	}

    public sealed class RemoveItem : Packet
	{
		public RemoveItem(IEntity item)
			: base(0x1D, 5)
		{
			m_Stream.Write(item.Serial);
		}
	}

	public sealed class RemoveMobile : Packet
	{
		public RemoveMobile(IEntity m)
			: base(0x1D, 5)
		{
			m_Stream.Write(m.Serial);
		}
	}

	public sealed class ServerChange : Packet
	{
		public ServerChange(IPoint3D m, Map map)
			: base(0x76, 16)
		{
			m_Stream.Write((short)m.X);
			m_Stream.Write((short)m.Y);
			m_Stream.Write((short)m.Z);
			m_Stream.Write((byte)0);
			m_Stream.Write((short)0);
			m_Stream.Write((short)0);
			m_Stream.Write((short)map.Width);
			m_Stream.Write((short)map.Height);
		}
	}

	public sealed class SkillUpdate : Packet
	{
		public SkillUpdate(Skills skills)
			: base(0x3A)
		{
			EnsureCapacity(6 + (skills.Length * 9));

			m_Stream.Write((byte)0x02); // type: absolute, capped

			for (int i = 0; i < skills.Length; ++i)
			{
				Skill s = skills[i];

				double v = s.NonRacialValue;
				int uv = (int)(v * 10);

				if (uv < 0)
				{
					uv = 0;
				}
				else if (uv >= 0x10000)
				{
					uv = 0xFFFF;
				}

				m_Stream.Write((ushort)(s.Info.SkillID + 1));
				m_Stream.Write((ushort)uv);
				m_Stream.Write((ushort)s.BaseFixedPoint);
				m_Stream.Write((byte)s.Lock);
				m_Stream.Write((ushort)s.CapFixedPoint);
			}

			m_Stream.Write((short)0); // terminate
		}
	}

    public sealed class SkillChange : Packet
	{
		public SkillChange(Skill skill)
			: base(0x3A)
		{
			EnsureCapacity(13);

			double v = skill.NonRacialValue;
			int uv = (int)(v * 10);

			if (uv < 0)
			{
				uv = 0;
			}
			else if (uv >= 0x10000)
			{
				uv = 0xFFFF;
			}

			m_Stream.Write((byte)0xDF); // type: delta, capped
			m_Stream.Write((ushort)skill.Info.SkillID);
			m_Stream.Write((ushort)uv);
			m_Stream.Write((ushort)skill.BaseFixedPoint);
			m_Stream.Write((byte)skill.Lock);
			m_Stream.Write((ushort)skill.CapFixedPoint);
		}
	}

	public sealed class LaunchBrowser : Packet
	{
		public LaunchBrowser(string url)
			: base(0xA5)
		{
			if (url == null)
			{
				url = "";
			}

			EnsureCapacity(4 + url.Length);

			m_Stream.WriteAsciiNull(url);
		}
	}

	public sealed class MessageLocalized : Packet
	{
		private static readonly MessageLocalized[] m_Cache_IntLoc = new MessageLocalized[15000];
		private static readonly MessageLocalized[] m_Cache_CliLoc = new MessageLocalized[100000];
		private static readonly MessageLocalized[] m_Cache_CliLocCmp = new MessageLocalized[5000];

		public static MessageLocalized InstantiateGeneric(int number)
		{
			MessageLocalized[] cache = null;
			int index = 0;

			if (number >= 3000000)
			{
				cache = m_Cache_IntLoc;
				index = number - 3000000;
			}
			else if (number >= 1000000)
			{
				cache = m_Cache_CliLoc;
				index = number - 1000000;
			}
			else if (number >= 500000)
			{
				cache = m_Cache_CliLocCmp;
				index = number - 500000;
			}

			MessageLocalized p;

			if (cache != null && index < cache.Length)
			{
				p = cache[index];

				if (p == null)
				{
					cache[index] = p = new MessageLocalized(Serial.MinusOne, -1, MessageType.Regular, 0x3B2, 3, number, "System", "");
					p.SetStatic();
				}
			}
			else
			{
				p = new MessageLocalized(Serial.MinusOne, -1, MessageType.Regular, 0x3B2, 3, number, "System", "");
			}

			return p;
		}

		public MessageLocalized(
			Serial serial, int graphic, MessageType type, int hue, int font, int number, string name, string args)
			: base(0xC1)
		{
			if (name == null)
			{
				name = "";
			}
			if (args == null)
			{
				args = "";
			}

			if (hue == 0)
			{
				hue = 0x3B2;
			}

			EnsureCapacity(50 + (args.Length * 2));

			m_Stream.Write(serial);
			m_Stream.Write((short)graphic);
			m_Stream.Write((byte)type);
			m_Stream.Write((short)hue);
			m_Stream.Write((short)font);
			m_Stream.Write(number);
			m_Stream.WriteAsciiFixed(name, 30);
			m_Stream.WriteLittleUniNull(args);
		}
	}

	public sealed class MobileMoving : Packet
	{
		public MobileMoving(Mobile m, int noto)
			: base(0x77, 17)
		{
			Point3D loc = m.Location;

			int hue = m.Hue;

			if (m.SolidHueOverride >= 0)
			{
				hue = m.SolidHueOverride;
			}

			m_Stream.Write(m.Serial);
			m_Stream.Write((short)m.Body);
			m_Stream.Write((short)loc.m_X);
			m_Stream.Write((short)loc.m_Y);
			m_Stream.Write((sbyte)loc.m_Z);
			m_Stream.Write((byte)m.Direction);
			m_Stream.Write((short)hue);
			m_Stream.Write((byte)m.GetPacketFlags());
			m_Stream.Write((byte)noto);
		}
	}

	public sealed class MultiTargetReq : Packet
	{
		public MultiTargetReq(MultiTarget t)
			: base(0x99, 30)
		{
			m_Stream.Write(t.AllowGround);
			m_Stream.Write(t.TargetID);
			m_Stream.Write((byte)t.Flags);

			m_Stream.Fill();

			m_Stream.Seek(18, SeekOrigin.Begin);
			m_Stream.Write((short)t.MultiID);
			m_Stream.Write((short)t.Offset.X);
			m_Stream.Write((short)t.Offset.Y);
			m_Stream.Write((short)t.Offset.Z);
		}
	}

	public sealed class CancelTarget : Packet
	{
		public static readonly Packet Instance = SetStatic(new CancelTarget());

		public CancelTarget()
			: base(0x6C, 19)
		{
			m_Stream.Write((byte)0);
			m_Stream.Write(0);
			m_Stream.Write((byte)3);
			m_Stream.Fill();
		}
	}

	public sealed class TargetReq : Packet
	{
		public TargetReq(Target t)
			: base(0x6C, 19)
		{
			m_Stream.Write(t.AllowGround);
			m_Stream.Write(t.TargetID);
			m_Stream.Write((byte)t.Flags);
			m_Stream.Fill();
		}
	}

	public sealed class DragEffect : Packet
	{
		public DragEffect(IEntity src, IEntity trg, int itemID, int hue, int amount)
			: base(0x23, 26)
		{
			m_Stream.Write((short)itemID);
			m_Stream.Write((byte)0);
			m_Stream.Write((short)hue);
			m_Stream.Write((short)amount);
			m_Stream.Write(src.Serial);
			m_Stream.Write((short)src.X);
			m_Stream.Write((short)src.Y);
			m_Stream.Write((sbyte)src.Z);
			m_Stream.Write(trg.Serial);
			m_Stream.Write((short)trg.X);
			m_Stream.Write((short)trg.Y);
			m_Stream.Write((sbyte)trg.Z);
		}
	}

	public interface IGumpWriter
	{
		int TextEntries { get; set; }
		int Switches { get; set; }

		void AppendLayout(bool val);
		void AppendLayout(int val);
		void AppendLayoutNS(int val);
		void AppendLayout(string text);
		void AppendLayout(byte[] buffer);
		void WriteStrings(List<string> strings);
		void Flush();
	}

	public sealed class DisplayGumpPacked : Packet, IGumpWriter
	{
		public int TextEntries { get; set; }
		public int Switches { get; set; }

		private readonly Gump m_Gump;

		private readonly PacketWriter m_Layout;
		private readonly PacketWriter m_Strings;

		private int m_StringCount;

		public DisplayGumpPacked(Gump gump)
			: base(0xDD)
		{
			m_Gump = gump;

			m_Layout = PacketWriter.CreateInstance(8192);
			m_Strings = PacketWriter.CreateInstance(8192);
		}

		private static readonly byte[] m_True = Gump.StringToBuffer(" 1");
		private static readonly byte[] m_False = Gump.StringToBuffer(" 0");

		private static readonly byte[] m_BeginTextSeparator = Gump.StringToBuffer(" @");
		private static readonly byte[] m_EndTextSeparator = Gump.StringToBuffer("@");

		private static readonly byte[] m_Buffer = new byte[48];

		static DisplayGumpPacked()
		{
			m_Buffer[0] = (byte)' ';
		}

		public void AppendLayout(bool val)
		{
			AppendLayout(val ? m_True : m_False);
		}

		public void AppendLayout(int val)
		{
			string toString = val.ToString();
			int bytes = Encoding.ASCII.GetBytes(toString, 0, toString.Length, m_Buffer, 1) + 1;

			m_Layout.Write(m_Buffer, 0, bytes);
		}

		public void AppendLayoutNS(int val)
		{
			string toString = val.ToString();
			int bytes = Encoding.ASCII.GetBytes(toString, 0, toString.Length, m_Buffer, 1);

			m_Layout.Write(m_Buffer, 1, bytes);
		}

		public void AppendLayout(string text)
		{
			AppendLayout(m_BeginTextSeparator);

			m_Layout.WriteAsciiFixed(text, text.Length);

			AppendLayout(m_EndTextSeparator);
		}

		public void AppendLayout(byte[] buffer)
		{
			m_Layout.Write(buffer, 0, buffer.Length);
		}

		public void WriteStrings(List<string> strings)
		{
			m_StringCount = strings.Count;

			for (int i = 0; i < strings.Count; ++i)
			{
				string v = strings[i];

				if (v == null)
				{
					v = string.Empty;
				}

				m_Strings.Write((ushort)v.Length);
				m_Strings.WriteBigUniFixed(v, v.Length);
			}
		}

		public void Flush()
		{
			EnsureCapacity(28 + (int)m_Layout.Length + (int)m_Strings.Length);

			m_Stream.Write(m_Gump.Serial);
			m_Stream.Write(m_Gump.TypeID);
			m_Stream.Write(m_Gump.X);
			m_Stream.Write(m_Gump.Y);

			// Note: layout MUST be null terminated
			m_Layout.Write((byte)0);
			WritePacked(m_Layout);

			m_Stream.Write(m_StringCount);

			WritePacked(m_Strings);

			PacketWriter.ReleaseInstance(m_Layout);
			PacketWriter.ReleaseInstance(m_Strings);
		}

		private const int GumpBufferSize = 0x10000;
		private static readonly BufferPool m_PackBuffers = new BufferPool("Gump", 4, GumpBufferSize);

		private void WritePacked(PacketWriter src)
		{
			byte[] buffer = src.UnderlyingStream.GetBuffer();
			int length = (int)src.Length;

			if (length == 0)
			{
				m_Stream.Write(0);
				return;
			}

			int wantLength = 1 + (buffer.Length * 1024 / 1000);

			wantLength += 4095;
			wantLength &= ~4095;

			byte[] m_PackBuffer;
			lock (m_PackBuffers)
				m_PackBuffer = m_PackBuffers.AcquireBuffer();

			if (m_PackBuffer.Length < wantLength)
			{
				//Console.WriteLine("Notice: DisplayGumpPacked creating new {0} byte buffer", wantLength);
				m_PackBuffer = new byte[wantLength];
			}

			int packLength = m_PackBuffer.Length;

			Compression.Pack(m_PackBuffer, ref packLength, buffer, length, ZLibQuality.Default);

			m_Stream.Write(4 + packLength);
			m_Stream.Write(length);
			m_Stream.Write(m_PackBuffer, 0, packLength);

			lock (m_PackBuffers)
				m_PackBuffers.ReleaseBuffer(m_PackBuffer);
		}
	}

	public sealed class PlaySound : Packet
	{
		public PlaySound(int soundID, IPoint3D target)
			: base(0x54, 12)
		{
			m_Stream.Write((byte)1); // flags
			m_Stream.Write((short)soundID);
			m_Stream.Write((short)0); // volume
			m_Stream.Write((short)target.X);
			m_Stream.Write((short)target.Y);
			m_Stream.Write((short)target.Z);
		}
	}

	public sealed class PlayMusic : Packet
	{
		public static readonly Packet InvalidInstance = SetStatic(new PlayMusic(MusicName.Invalid));

		private static readonly Packet[] m_Instances = new Packet[60];

		public static Packet GetInstance(MusicName name)
		{
			if (name == MusicName.Invalid)
			{
				return InvalidInstance;
			}

			int v = (int)name;
			Packet p;

			if (v >= 0 && v < m_Instances.Length)
			{
				p = m_Instances[v];

				if (p == null)
				{
					m_Instances[v] = p = SetStatic(new PlayMusic(name));
				}
			}
			else
			{
				p = new PlayMusic(name);
			}

			return p;
		}

		public PlayMusic(MusicName name)
			: base(0x6D, 3)
		{
			m_Stream.Write((short)name);
		}
	}

	public sealed class MapChange : Packet
	{
		public MapChange(IEntity m)
			: base(0xBF)
		{
			EnsureCapacity(6);

			m_Stream.Write((short)0x08);
			m_Stream.Write((byte)(m.Map == null ? 0 : m.Map.MapID));
		}
	}

	public sealed class SeasonChange : Packet
	{
		private static readonly SeasonChange[][] m_Cache =
        {
            new SeasonChange[2], new SeasonChange[2], new SeasonChange[2], new SeasonChange[2], new SeasonChange[2]
        };

		public static SeasonChange Instantiate(int season)
		{
			return Instantiate(season, true);
		}

		public static SeasonChange Instantiate(int season, bool playSound)
        {
            if (season >= 0 && season < m_Cache.Length)
			{
				int idx = playSound ? 1 : 0;

				SeasonChange p = m_Cache[season][idx];

				if (p == null)
				{
					m_Cache[season][idx] = p = new SeasonChange(season, playSound);
					p.SetStatic();
				}

				return p;
			}

            return new SeasonChange(season, playSound);
        }

		public SeasonChange(int season)
			: this(season, true)
		{ }

		public SeasonChange(int season, bool playSound)
			: base(0xBC, 3)
		{
			m_Stream.Write((byte)season);
			m_Stream.Write(playSound);
		}
	}

	public sealed class SupportedFeatures : Packet
	{
		public static FeatureFlags Value { get; set; }

		public static SupportedFeatures Instantiate(NetState ns)
		{
			return new SupportedFeatures(ns);
		}

		public SupportedFeatures(NetState ns)
			: base(0xB9, 5)
		{
			FeatureFlags flags = ExpansionInfo.CoreExpansion.SupportedFeatures;

			flags |= Value;

			IAccount acct = ns.Account;

			if (acct != null && acct.Limit >= 6)
			{
				flags |= FeatureFlags.LiveAccount;
				flags &= ~FeatureFlags.UOTD;

				if (acct.Limit > 6)
				{
					flags |= FeatureFlags.SeventhCharacterSlot;
				}
				else
				{
					flags |= FeatureFlags.SixthCharacterSlot;
				}
			}

			m_Stream.Write((uint)flags);
		}
	}

	public static class AttributeNormalizer
	{
		public static int Maximum { get; set; } = 25;

		public static bool Enabled { get; set; } = true;

		public static void Write(PacketWriter stream, int cur, int max)
		{
			if (Enabled && max != 0)
			{
				stream.Write((short)Maximum);
				stream.Write((short)(cur * Maximum / max));
			}
			else
			{
				stream.Write((short)max);
				stream.Write((short)cur);
			}
		}

		public static void WriteReverse(PacketWriter stream, int cur, int max)
		{
			if (Enabled && max != 0)
			{
				stream.Write((short)(cur * Maximum / max));
				stream.Write((short)Maximum);
			}
			else
			{
				stream.Write((short)cur);
				stream.Write((short)max);
			}
		}
	}

	public sealed class MobileHits : Packet
	{
		public MobileHits(IDamageable m)
			: base(0xA1, 9)
		{
			m_Stream.Write(m.Serial);
			m_Stream.Write((short)m.HitsMax);
			m_Stream.Write((short)m.Hits);
		}
	}

	public sealed class MobileHitsN : Packet
	{
		public MobileHitsN(IDamageable d)
			: base(0xA1, 9)
		{
			m_Stream.Write(d.Serial);
			AttributeNormalizer.Write(m_Stream, d.Hits, d.HitsMax);
		}
	}

	public sealed class MobileMana : Packet
	{
		public MobileMana(Mobile m)
			: base(0xA2, 9)
		{
			m_Stream.Write(m.Serial);
			m_Stream.Write((short)m.ManaMax);
			m_Stream.Write((short)m.Mana);
		}
	}

	public sealed class MobileManaN : Packet
	{
		public MobileManaN(Mobile m)
			: base(0xA2, 9)
		{
			m_Stream.Write(m.Serial);
			AttributeNormalizer.Write(m_Stream, m.Mana, m.ManaMax);
		}
	}

	public sealed class MobileStam : Packet
	{
		public MobileStam(Mobile m)
			: base(0xA3, 9)
		{
			m_Stream.Write(m.Serial);
			m_Stream.Write((short)m.StamMax);
			m_Stream.Write((short)m.Stam);
		}
	}

	public sealed class MobileStamN : Packet
	{
		public MobileStamN(Mobile m)
			: base(0xA3, 9)
		{
			m_Stream.Write(m.Serial);
			AttributeNormalizer.Write(m_Stream, m.Stam, m.StamMax);
		}
	}

	public sealed class MobileAttributes : Packet
	{
		public MobileAttributes(Mobile m)
			: base(0x2D, 17)
		{
			m_Stream.Write(m.Serial);

			m_Stream.Write((short)m.HitsMax);
			m_Stream.Write((short)m.Hits);

			m_Stream.Write((short)m.ManaMax);
			m_Stream.Write((short)m.Mana);

			m_Stream.Write((short)m.StamMax);
			m_Stream.Write((short)m.Stam);
		}
	}

	public sealed class MobileAttributesN : Packet
	{
		public MobileAttributesN(Mobile m)
			: base(0x2D, 17)
		{
			m_Stream.Write(m.Serial);

			AttributeNormalizer.Write(m_Stream, m.Hits, m.HitsMax);
			AttributeNormalizer.Write(m_Stream, m.Mana, m.ManaMax);
			AttributeNormalizer.Write(m_Stream, m.Stam, m.StamMax);
		}
	}

	public sealed class MobileAnimation : Packet
	{
		public MobileAnimation(IEntity m, int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
			: base(0x6E, 14)
		{
			m_Stream.Write(m.Serial);
			m_Stream.Write((short)action);
			m_Stream.Write((short)frameCount);
			m_Stream.Write((short)repeatCount);
			m_Stream.Write(!forward); // protocol has really "reverse" but I find this more intuitive
			m_Stream.Write(repeat);
			m_Stream.Write((byte)delay);
		}
	}

	public sealed class NewMobileAnimation : Packet
	{
		public NewMobileAnimation(IEntity m, AnimationType type, int action, int delay)
			: base(0xE2, 10)
		{
			m_Stream.Write(m.Serial);
			m_Stream.Write((short)type);
			m_Stream.Write((short)action);
			m_Stream.Write((byte)delay);
		}
	}

	public sealed class MobileStatusCompact : Packet
	{
		public MobileStatusCompact(bool canBeRenamed, IDamageable d)
			: base(0x11)
		{
			string name = d.Name == null ? "" : d.Name;

			EnsureCapacity(43);

			m_Stream.Write(d.Serial);
			m_Stream.WriteAsciiFixed(name, 30);

			AttributeNormalizer.WriteReverse(m_Stream, d.Hits, d.HitsMax);

			m_Stream.Write(canBeRenamed);

			m_Stream.Write((byte)0); // type
		}
	}

	public sealed class MobileStatus : Packet
	{
		public MobileStatus(Mobile m)
			: this(m, m)
		{
		}

		public MobileStatus(Mobile beholder, Mobile beheld)
			: base(0x11)
		{
			string name = beheld.Name;

			if (name == null)
			{
				name = "";
			}

			int type = 0;

			if (beholder == beheld)
				type = 6;

			bool isEnhancedClient = beholder.NetState != null && beholder.NetState.IsEnhancedClient;

			int size;
			
			if (type == 0)
				size = 43;
			else if (isEnhancedClient)
				size = 151;
			else
				size = 121;

			EnsureCapacity(size);

			m_Stream.Write(beheld.Serial);

			m_Stream.WriteAsciiFixed(name, 30);

			if (beholder == beheld)
			{
				WriteAttr(beheld.Hits, beheld.HitsMax);
			}
			else
			{
				WriteAttrNorm(beheld.Hits, beheld.HitsMax);
			}

			m_Stream.Write(beheld.CanBeRenamedBy(beholder));

			m_Stream.Write((byte)type);

			if (type > 0)
			{
				m_Stream.Write(beheld.Female);

				m_Stream.Write((short)beheld.Str);
				m_Stream.Write((short)beheld.Dex);
				m_Stream.Write((short)beheld.Int);

				WriteAttr(beheld.Stam, beheld.StamMax);
				WriteAttr(beheld.Mana, beheld.ManaMax);

				m_Stream.Write(beheld.TotalGold);
				m_Stream.Write((short)beheld.PhysicalResistance);
				m_Stream.Write((short)(Mobile.BodyWeight + beheld.TotalWeight));

				m_Stream.Write((short)beheld.MaxWeight);
				m_Stream.Write((byte)(beheld.Race.RaceID + 1)); // Would be 0x00 if it's a non-ML enabled account but...

				m_Stream.Write((short)beheld.StatCap);

				m_Stream.Write((byte)beheld.Followers);
				m_Stream.Write((byte)beheld.FollowersMax);

				m_Stream.Write((short)beheld.FireResistance); // Fire
				m_Stream.Write((short)beheld.ColdResistance); // Cold
				m_Stream.Write((short)beheld.PoisonResistance); // Poison
				m_Stream.Write((short)beheld.EnergyResistance); // Energy
				m_Stream.Write((short)beheld.Luck); // Luck

				IWeapon weapon = beheld.Weapon;

				int min = 0, max = 0;

				if (weapon != null)
				{
					weapon.GetStatusDamage(beheld, out min, out max);
				}

				m_Stream.Write((short)min); // Damage min
				m_Stream.Write((short)max); // Damage max

				m_Stream.Write(beheld.TithingPoints);

				int count = isEnhancedClient ? 28 : 14;

				for (int i = 0; i <= count; ++i)
				{
					m_Stream.Write((short)beheld.GetAOSStatus(i));
				}
			}
		}

		private void WriteAttr(int current, int maximum)
		{
			m_Stream.Write((short)current);
			m_Stream.Write((short)maximum);
		}

		private void WriteAttrNorm(int current, int maximum)
		{
			AttributeNormalizer.WriteReverse(m_Stream, current, maximum);
		}
	}

	public sealed class HealthbarPoison : Packet
	{
		public HealthbarPoison(Mobile m)
			: base(0x17)
		{
			EnsureCapacity(12);

			m_Stream.Write(m.Serial);
			m_Stream.Write((short)1);

			m_Stream.Write((short)1);

			Poison p = m.Poison;

			if (p != null)
			{
				m_Stream.Write((byte)(p.Level + 1));
			}
			else
			{
				m_Stream.Write((byte)0);
			}
		}
	}

	public sealed class HealthbarYellow : Packet
	{
		public HealthbarYellow(Mobile m)
			: base(0x17)
		{
			EnsureCapacity(12);

			m_Stream.Write(m.Serial);
			m_Stream.Write((short)1);

			m_Stream.Write((short)2);

			if (m.Blessed || m.YellowHealthbar)
			{
				m_Stream.Write((byte)1);
			}
			else
			{
				m_Stream.Write((byte)0);
			}
		}
	}

	public sealed class HealthbarYellowEC : Packet
	{
		public HealthbarYellowEC(Mobile m)
			: base(0x16)
		{
			EnsureCapacity(12);

			m_Stream.Write(m.Serial);

			m_Stream.Write((short)1);
			m_Stream.Write((short)2);

			if (m.Blessed || m.YellowHealthbar)
			{
				m_Stream.Write((byte)1);
			}
			else
			{
				m_Stream.Write((byte)0);
			}
		}
	}

	public sealed class HealthbarPoisonEC : Packet
	{
		public HealthbarPoisonEC(Mobile m)
			: base(0x16)
		{
			EnsureCapacity(12);

			m_Stream.Write(m.Serial);

			m_Stream.Write((short)1);
			m_Stream.Write((short)1);

			Poison p = m.Poison;

			if (p != null)
			{
				m_Stream.Write((byte)(p.Level + 1));
			}
			else
			{
				m_Stream.Write((byte)0);
			}
		}
	}

	public sealed class MobileUpdate : Packet
	{
		public MobileUpdate(Mobile m)
			: base(0x20, 19)
		{
			int hue = m.Hue;

			if (m.SolidHueOverride >= 0)
			{
				hue = m.SolidHueOverride;
			}

			m_Stream.Write(m.Serial);
			m_Stream.Write((short)m.Body);
			m_Stream.Write((byte)0);
			m_Stream.Write((short)hue);
			m_Stream.Write((byte)m.GetPacketFlags());
			m_Stream.Write((short)m.X);
			m_Stream.Write((short)m.Y);
			m_Stream.Write((short)0);
			m_Stream.Write((byte)m.Direction);
			m_Stream.Write((sbyte)m.Z);
		}
	}

	public sealed class MobileIncoming : Packet
	{
		public static Packet Create(NetState ns, Mobile beholder, Mobile beheld)
		{
			return new MobileIncoming(beholder, beheld);
		}

		private static readonly ThreadLocal<int[]> m_DupedLayersTL = new ThreadLocal<int[]>(() => { return new int[256]; });
		private static readonly ThreadLocal<int> m_VersionTL = new ThreadLocal<int>();

		public Mobile m_Beheld;

		public MobileIncoming(Mobile beholder, Mobile beheld)
			: base(0x78)
		{
			m_Beheld = beheld;

			int m_Version = ++m_VersionTL.Value;
			int[] m_DupedLayers = m_DupedLayersTL.Value;

			List<Item> eq = beheld.Items;
			int count = eq.Count;

			if (beheld.HairItemID > 0)
			{
				count++;
			}

			if (beheld.FacialHairItemID > 0)
			{
				count++;
			}

			if (beheld.FaceItemID > 0)
			{
				count++;
			}

			EnsureCapacity(23 + (count * 9));

			int hue = beheld.Hue;

			if (beheld.SolidHueOverride >= 0)
			{
				hue = beheld.SolidHueOverride;
			}

			m_Stream.Write(beheld.Serial);
			m_Stream.Write((short)beheld.Body);
			m_Stream.Write((short)beheld.X);
			m_Stream.Write((short)beheld.Y);
			m_Stream.Write((sbyte)beheld.Z);
			m_Stream.Write((byte)beheld.Direction);
			m_Stream.Write((short)hue);
			m_Stream.Write((byte)beheld.GetPacketFlags());
			m_Stream.Write((byte)Notoriety.Compute(beholder, beheld));

			for (int i = 0; i < eq.Count; ++i)
			{
				Item item = eq[i];

				byte layer = (byte)item.Layer;

				if (!item.Deleted && beholder.CanSee(item) && m_DupedLayers[layer] != m_Version)
				{
					m_DupedLayers[layer] = m_Version;

					hue = item.Hue;

					if (beheld.SolidHueOverride >= 0)
					{
						hue = beheld.SolidHueOverride;
					}

					int itemID = item.ItemID & 0xFFFF;

					m_Stream.Write(item.Serial);
					m_Stream.Write((ushort)itemID);
					m_Stream.Write(layer);

					m_Stream.Write((short)hue);
				}
			}

			if (beheld.HairItemID > 0)
			{
				if (m_DupedLayers[(int)Layer.Hair] != m_Version)
				{
					m_DupedLayers[(int)Layer.Hair] = m_Version;
					hue = beheld.HairHue;

					if (beheld.SolidHueOverride >= 0)
					{
						hue = beheld.SolidHueOverride;
					}

					int itemID = beheld.HairItemID & 0xFFFF;

					m_Stream.Write(HairInfo.FakeSerial(beheld));
					m_Stream.Write((ushort)itemID);
					m_Stream.Write((byte)Layer.Hair);

					m_Stream.Write((short)hue);
				}
			}

			if (beheld.FacialHairItemID > 0)
			{
				if (m_DupedLayers[(int)Layer.FacialHair] != m_Version)
				{
					m_DupedLayers[(int)Layer.FacialHair] = m_Version;
					hue = beheld.FacialHairHue;

					if (beheld.SolidHueOverride >= 0)
					{
						hue = beheld.SolidHueOverride;
					}

					int itemID = beheld.FacialHairItemID & 0xFFFF;

					m_Stream.Write(FacialHairInfo.FakeSerial(beheld));
					m_Stream.Write((ushort)itemID);
					m_Stream.Write((byte)Layer.FacialHair);

					m_Stream.Write((short)hue);
				}
			}

			if (beheld.FaceItemID > 0)
			{
				if (m_DupedLayers[(int)Layer.Face] != m_Version)
				{
					m_DupedLayers[(int)Layer.Face] = m_Version;
					hue = beheld.FaceHue;

					if (beheld.SolidHueOverride >= 0)
					{
						hue = beheld.SolidHueOverride;
					}

					int itemID = beheld.FaceItemID & 0xFFFF;

					m_Stream.Write(FaceInfo.FakeSerial(beheld));
					m_Stream.Write((ushort)itemID);
					m_Stream.Write((byte)Layer.Face);

					m_Stream.Write((short)hue);
				}
			}

			m_Stream.Write(0); // terminate
		}
	}

	public sealed class AsciiMessage : Packet
	{
		public AsciiMessage(Serial serial, int graphic, MessageType type, int hue, int font, string name, string text)
			: base(0x1C)
		{
			if (name == null)
			{
				name = "";
			}

			if (text == null)
			{
				text = "";
			}

			if (hue == 0)
			{
				hue = 0x3B2;
			}

			EnsureCapacity(45 + text.Length);

			m_Stream.Write(serial);
			m_Stream.Write((short)graphic);
			m_Stream.Write((byte)type);
			m_Stream.Write((short)hue);
			m_Stream.Write((short)font);
			m_Stream.WriteAsciiFixed(name, 30);
			m_Stream.WriteAsciiNull(text);
		}
	}

	public sealed class UnicodeMessage : Packet
	{
		public UnicodeMessage(
			Serial serial, int graphic, MessageType type, int hue, int font, string lang, string name, string text)
			: base(0xAE)
		{
			if (string.IsNullOrEmpty(lang))
			{
				lang = "ENU";
			}
			if (name == null)
			{
				name = "";
			}
			if (text == null)
			{
				text = "";
			}

			if (hue == 0)
			{
				hue = 0x3B2;
			}

			EnsureCapacity(50 + (text.Length * 2));

			m_Stream.Write(serial);
			m_Stream.Write((short)graphic);
			m_Stream.Write((byte)type);
			m_Stream.Write((short)hue);
			m_Stream.Write((short)font);
			m_Stream.WriteAsciiFixed(lang, 4);
			m_Stream.WriteAsciiFixed(name, 30);
			m_Stream.WriteBigUniNull(text);
		}
	}

	public sealed class MovementAck : Packet
	{
		private static readonly MovementAck[][] m_Cache =
        {
			new MovementAck[256], new MovementAck[256], new MovementAck[256], new MovementAck[256], new MovementAck[256],
			new MovementAck[256], new MovementAck[256], new MovementAck[256]
		};

		public static MovementAck Instantiate(int seq, Mobile m)
		{
			int noto = Notoriety.Compute(m, m);

			MovementAck p = m_Cache[noto][seq];

			if (p == null)
			{
				m_Cache[noto][seq] = p = new MovementAck(seq, noto);
				p.SetStatic();
			}

			return p;
		}

		private MovementAck(int seq, int noto)
			: base(0x22, 3)
		{
			m_Stream.Write((byte)seq);
			m_Stream.Write((byte)noto);
		}
	}

	public sealed class CityInfo
	{
		private Point3D m_Location;

		public CityInfo(string city, string building, int description, int x, int y, int z, Map m)
		{
			City = city;
			Building = building;
			Description = description;
			m_Location = new Point3D(x, y, z);
			Map = m;
		}

        public CityInfo(string city, string building, int description, int x, int y, int z)
			: this(city, building, description, x, y, z, Map.Trammel)
		{ }

        public string City { get; set; }
		public string Building { get; set; }
		public int Description { get; set; }
		public int X { get => m_Location.X; set => m_Location.X = value; }
		public int Y { get => m_Location.Y; set => m_Location.Y = value; }
		public int Z { get => m_Location.Z; set => m_Location.Z = value; }
		public Point3D Location { get => m_Location; set => m_Location = value; }
		public Map Map { get; set; }
	}

	public enum AffixType : byte
	{
		Append = 0x00,
		Prepend = 0x01,
		System = 0x02
	}

	public sealed class MessageLocalizedAffix : Packet
	{
		public MessageLocalizedAffix(
			Serial serial,
			int graphic,
			MessageType messageType,
			int hue,
			int font,
			int number,
			string name,
			AffixType affixType,
			string affix,
			string args)
			: this(null,
				serial,
				graphic,
				messageType,
				hue,
				font,
				number,
				name,
				affixType,
				affix,
				args)
		{
		}

		public MessageLocalizedAffix(
			NetState state,
			Serial serial,
			int graphic,
			MessageType messageType,
			int hue,
			int font,
			int number,
			string name,
			AffixType affixType,
			string affix,
			string args)
			: base(0xCC)
		{
			if (name == null)
			{
				name = "";
			}

			if (affix == null)
			{
				affix = "";
			}

			if (args == null)
			{
				args = "";
			}

			if (hue == 0)
			{
				hue = 0x3B2;
			}

			EnsureCapacity(52 + affix.Length + (args.Length * 2));

			m_Stream.Write(serial);
			m_Stream.Write((short)graphic);
			m_Stream.Write((byte)messageType);
			m_Stream.Write((short)hue);
			m_Stream.Write((short)font);
			m_Stream.Write(number);
			m_Stream.Write((byte)affixType);
			m_Stream.WriteAsciiFixed(name, 30);
			m_Stream.WriteAsciiNull(affix);

			if (state != null && state.IsEnhancedClient)
			{
				m_Stream.WriteLittleUniNull(args);
			}
			else
			{
				m_Stream.WriteBigUniNull(args);
			}
		}
	}

	public sealed class ServerInfo
	{
		public string Name { get; set; }

		public int FullPercent { get; set; }

		public int TimeZone { get; set; }

		public IPEndPoint Address { get; set; }

		public ServerInfo(string name, int fullPercent, TimeZoneInfo tZi, IPEndPoint address)
		{
			Name = name;
			FullPercent = fullPercent;
			TimeZone = tZi.GetUtcOffset(DateTime.Now).Hours;
			Address = address;
		}
	}

	[Flags]
	public enum PacketState
	{
		Inactive = 0x00,
		Static = 0x01,
		Acquired = 0x02,
		Accessed = 0x04,
		Buffered = 0x08,
		Warned = 0x10
	}

	public abstract class Packet
	{
		protected PacketWriter m_Stream;

		private readonly int m_PacketID;
		private readonly int m_Length;

		private PacketState m_State;

        public PacketState State => m_State;

		protected Packet(int packetID)
		{
			m_PacketID = packetID;

			if (Core.Profiling)
			{
				PacketSendProfile prof = PacketSendProfile.Acquire(GetType());
				prof.Increment();
			}
		}

		protected Packet(int packetID, int length)
			: this(packetID, length, PacketWriter.CreateInstance(length))
		{ }

		protected Packet(int packetID, int length, PacketWriter stream)
		{
			m_PacketID = packetID;
			m_Length = length;

			m_Stream = stream;
			m_Stream.Write((byte)packetID);

			if (Core.Profiling)
			{
				PacketSendProfile prof = PacketSendProfile.Acquire(GetType());
				prof.Increment();
			}
		}

		public void EnsureCapacity(int length)
		{
			m_Stream = PacketWriter.CreateInstance(length);// new PacketWriter( length );
			m_Stream.Write((byte)m_PacketID);
			m_Stream.Write((short)0);
		}

		public PacketWriter UnderlyingStream => m_Stream;

		private const int CompressorBufferSize = 0x10000;
		private static readonly BufferPool m_CompressorBuffers = new BufferPool("Compressor", 4, CompressorBufferSize);

		private const int BufferSize = 4096;
		private static readonly BufferPool m_Buffers = new BufferPool("Compressed", 16, BufferSize);

		public static T SetStatic<T>(T p) where T : Packet
		{
			if (p != null)
				p.SetStatic();

			return p;
		}

		public static T Acquire<T>(T p) where T : Packet
		{
			if (p != null)
				p.Acquire();

			return p;
		}

		public static void Release<T>(T p) where T : Packet
		{
			if (p != null)
				p.Release();
		}

		public static void Release<T>(ref T p) where T : Packet
		{
			if (p != null)
				p.Release();

			p = null;
		}

		public void SetStatic()
		{
			m_State |= PacketState.Static | PacketState.Acquired;
		}

		public void Acquire()
		{
			m_State |= PacketState.Acquired;
		}

		public void OnSend()
		{
			Core.Set();

			lock (this)
			{
				if ((m_State & (PacketState.Acquired | PacketState.Static)) == 0)
				{
					Free();
				}
			}
		}

		private void Free()
		{
			if (m_CompiledBuffer == null)
			{
				return;
			}

			if ((m_State & PacketState.Buffered) != 0)
			{
				m_Buffers.ReleaseBuffer(m_CompiledBuffer);
			}

			m_State &= ~(PacketState.Static | PacketState.Acquired | PacketState.Buffered);

			m_CompiledBuffer = null;
		}

		public void Release()
		{
			if ((m_State & PacketState.Acquired) != 0)
			{
				Free();
			}
		}

		private byte[] m_CompiledBuffer;
		private int m_CompiledLength;

		public byte[] Compile(bool compress, out int length)
		{
			lock (this)
			{
				if (m_CompiledBuffer == null)
				{
					if ((m_State & PacketState.Accessed) == 0)
					{
						m_State |= PacketState.Accessed;
					}
					else
					{
						if ((m_State & PacketState.Warned) == 0)
						{
							m_State |= PacketState.Warned;

							try
							{
								using (StreamWriter op = new StreamWriter("net_opt.log", true))
								{
									op.WriteLine("Redundant compile for packet {0}, use Acquire() and Release()", GetType());
									op.WriteLine(new StackTrace());
								}
							}
							catch (Exception e)
							{
                                ExceptionLogging.LogException(e);
							}
						}

						m_CompiledBuffer = Array.Empty<byte>();
						m_CompiledLength = 0;

						length = m_CompiledLength;
						return m_CompiledBuffer;
					}

					InternalCompile(compress);
				}

				length = m_CompiledLength;
				return m_CompiledBuffer;
			}
		}

		private void InternalCompile(bool compress)
		{
			if (m_Length == 0)
			{
				long streamLen = m_Stream.Length;

				m_Stream.Seek(1, SeekOrigin.Begin);
				m_Stream.Write((ushort)streamLen);
			}
			else if (m_Stream.Length != m_Length)
			{
				int diff = (int)m_Stream.Length - m_Length;

				Console.WriteLine("Packet: 0x{0:X2}: Bad packet length! ({1}{2} bytes)", m_PacketID, diff >= 0 ? "+" : "", diff);
			}

			MemoryStream ms = m_Stream.UnderlyingStream;

			m_CompiledBuffer = ms.GetBuffer();
			int length = (int)ms.Length;

			if (compress)
			{
				byte[] buffer;

				lock (m_CompressorBuffers)
					buffer = m_CompressorBuffers.AcquireBuffer();

				Compression.Compress(m_CompiledBuffer, 0, length, buffer, ref length);

				if (length <= 0)
				{
					Console.WriteLine(
						"Warning: Compression buffer overflowed on packet 0x{0:X2} ('{1}') (length={2})",
						m_PacketID,
						GetType().Name,
						length);
					using (StreamWriter op = new StreamWriter("compression_overflow.log", true))
					{
						op.WriteLine(
							"{0} Warning: Compression buffer overflowed on packet 0x{1:X2} ('{2}') (length={3})",
							DateTime.UtcNow,
							m_PacketID,
							GetType().Name,
							length);
						op.WriteLine(new StackTrace());
					}
				}
				else
				{
					m_CompiledLength = length;

					if (length > BufferSize || (m_State & PacketState.Static) != 0)
					{
						m_CompiledBuffer = new byte[length];
					}
					else
					{
						lock (m_Buffers)
							m_CompiledBuffer = m_Buffers.AcquireBuffer();
						m_State |= PacketState.Buffered;
					}

					Buffer.BlockCopy(buffer, 0, m_CompiledBuffer, 0, length);

					lock (m_CompressorBuffers)
						m_CompressorBuffers.ReleaseBuffer(buffer);
				}
			}
			else if (length > 0)
			{
				byte[] old = m_CompiledBuffer;
				m_CompiledLength = length;

				if (length > BufferSize || (m_State & PacketState.Static) != 0)
				{
					m_CompiledBuffer = new byte[length];
				}
				else
				{
					lock (m_Buffers)
						m_CompiledBuffer = m_Buffers.AcquireBuffer();
					m_State |= PacketState.Buffered;
				}

				Buffer.BlockCopy(old, 0, m_CompiledBuffer, 0, length);
			}

			PacketWriter.ReleaseInstance(m_Stream);
			m_Stream = null;
		}
	}
}
