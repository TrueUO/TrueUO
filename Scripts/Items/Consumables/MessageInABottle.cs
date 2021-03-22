using System;

namespace Server.Items
{
    public class MessageInABottle : Item
    {
        private Map m_TargetMap;
        private int m_Level;
        [Constructable]
        public MessageInABottle()
            : this(Map.Trammel)
        {
        }

        public MessageInABottle(Map map)
            : this(map, GetRandomLevel())
        {
        }

        [Constructable]
        public MessageInABottle(Map map, int level)
            : base(0xA30C)
        {
            Weight = 1.0;
            m_TargetMap = map;
            m_Level = level;
        }

        public MessageInABottle(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1041080;// a message in a bottle

        [CommandProperty(AccessLevel.GameMaster)]
        public Map TargetMap { get => m_TargetMap; set => m_TargetMap = value; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Level
        {
            get => m_Level;
            set => m_Level = Math.Max(1, Math.Min(value, 4));
        }

        public static int GetRandomLevel()
        {
            if (1 > Utility.Random(25))
                return 4; // ancient

            return Utility.RandomMinMax(1, 3);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(3); // version

            writer.Write(m_Level);
            writer.Write(m_TargetMap);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            m_Level = reader.ReadInt();
            m_TargetMap = reader.ReadMap();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                ReplaceWith(new SOS(m_TargetMap, m_Level));
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 501891); // You extract the message from the bottle.
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }
    }
}
