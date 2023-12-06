using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class ParoxysmusAltar : PeerlessAltar
    {
        private static readonly Dictionary<Mobile, Timer> _ProtectionTable = new Dictionary<Mobile, Timer>();

        public override int KeyCount => 16;
        public override MasterKey MasterKey => new ParoxysmusKey();

        public override Type[] Keys => new[]
        {
            typeof(CoagulatedLegs), typeof(PartiallyDigestedTorso),
            typeof(GelatanousSkull), typeof(SpleenOfThePutrefier)
        };

        public override BasePeerless Boss => new ChiefParoxysmus();

        [Constructable]
        public ParoxysmusAltar() : base(0x207A)
        {
            Hue = 0x465;

            BossLocation = new Point3D(6517, 357, 0);
            TeleportDest = new Point3D(6519, 381, 0);
            ExitDest = new Point3D(5623, 3038, 15);
        }

        public override Rectangle2D[] BossBounds => _Bounds;

        private readonly Rectangle2D[] _Bounds =
        {
            new Rectangle2D(6501, 351, 35, 48)
        };

        public static void AddProtection(Mobile m)
        {
            if (_ProtectionTable != null && !_ProtectionTable.ContainsKey(m))
            {
                _ProtectionTable[m] = Timer.DelayCall(TimeSpan.FromMinutes(5), () => Damage(m));
            }
        }

        public static bool IsUnderEffects(Mobile m)
        {
            return _ProtectionTable != null && _ProtectionTable.ContainsKey(m);
        }

        public static void Damage(Mobile m)
        {
            if (_ProtectionTable.TryGetValue(m, out Timer t))
            {
                if (t != null)
                {
                    t.Stop();
                }

                _ProtectionTable.Remove(m);
            }
        }

        public ParoxysmusAltar(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write(_ProtectionTable.Count);

            foreach (KeyValuePair<Mobile, Timer> kvp in _ProtectionTable)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Next);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            int count = reader.ReadInt();

            for (int i = 0; i < count; ++i)
            {
                Mobile m = reader.ReadMobile();
                DateTime end = reader.ReadDateTime();

                _ProtectionTable[m] = Timer.DelayCall(end - DateTime.UtcNow, () => Damage(m));
            }
        }
    }

    public class ParoxysmusIronGate : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public PeerlessAltar Altar { get; set; }

        [Constructable]
        public ParoxysmusIronGate()
            : this(null)
        {
        }

        [Constructable]
        public ParoxysmusIronGate(PeerlessAltar altar)
            : base(0x857)
        {
            Altar = altar;
            Movable = false;
        }

        public ParoxysmusIronGate(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.Alive)
            {
                return;
            }

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
            else if (Altar != null && Altar.Fighters.Contains(from))
            {
                from.SendLocalizedMessage(1075070); // The rusty gate cracks open as you step through...
                from.MoveToWorld(Altar.TeleportDest, Altar.Map);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(Altar);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Altar = reader.ReadItem() as PeerlessAltar;
        }
    }
}
