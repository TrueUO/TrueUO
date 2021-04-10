using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System.Collections.Generic;

namespace Server.Items
{
    public class Dices : Item, ITelekinesisable, ISecurable
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level { get; set; }

        [Constructable]
        public Dices()
            : base(0xFA7)
        {
            Weight = 1.0;

            Level = SecureLevel.Friends;
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public Dices(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                return;

            Roll(from);
        }

        public void OnTelekinesis(Mobile from)
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
            Effects.PlaySound(Location, Map, 0x1F5);

            Roll(from);
        }

        public void Roll(Mobile from)
        {
            int one = Utility.Random(1, 6);
            int two = Utility.Random(1, 6);

            if (RootParent is PlayerMobile)
            {
                from.Send(new MessageLocalizedAffix(Serial.Zero, 0, MessageType.Regular, 0x3B2, 3, 1042713, "", AffixType.Prepend, from.Name + " ", ""));
                from.Send(new MessageLocalizedAffix(Serial.Zero, 0, MessageType.Regular, 0x3B2, 3, 1042714, "", AffixType.Append, " " + one, ""));
                from.Send(new MessageLocalizedAffix(Serial.Zero, 0, MessageType.Regular, 0x3B2, 3, 1042715, "", AffixType.Append, " " + two, ""));
                from.Send(new MessageLocalizedAffix(Serial.Zero, 0, MessageType.Regular, 0x3B2, 3, 1042716, "", AffixType.Append, " " + (one + two), ""));

                return;
            }

            if (RootParent == null)
            {
                SendLocalizedMessage(MessageType.Label, 1042713, AffixType.Prepend, from.Name + " ", ""); // The first die rolls to a stop and shows:
                SendLocalizedMessage(MessageType.Label, 1042714, AffixType.Append, " " + one, ""); // The first die rolls to a stop and shows:
                SendLocalizedMessage(MessageType.Label, 1042715, AffixType.Append, " " + two, ""); // The second die stops and shows:
                SendLocalizedMessage(MessageType.Label, 1042716, AffixType.Append, " " + (one + two), ""); // Total for this roll:
                return;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write((int)Level);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
			
			Level = (SecureLevel)reader.ReadInt();            
        }
    }
}
