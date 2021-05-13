using Server.Gumps;
using Server.Network;
using System.Collections.Generic;

namespace Server.Items
{
    public class BonesOfAFallenRanger : Item
    {
        public override string DefaultName => "Bones Of A Fallen Ranger";

        private readonly Dictionary<Mobile, int> list = new Dictionary<Mobile, int>();

        [Constructable]
        public BonesOfAFallenRanger()
            : base(0x1B0C)
        {
            Weight = 0.0;
            Movable = false;
        }

        public BonesOfAFallenRanger(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this, 3))
            {
                from.SendLocalizedMessage(1019045); // I can't reach that.
                return;
            }

            PrivateOverheadMessage(MessageType.Regular, 1150, false, "*The bones have long been picked over by vile creatures. All that remains is a necklace you collect and put in your pack*", from.NetState);

            if (list.ContainsKey(from))
            {
                if (list[from] >= 2)
                {
                    from.SendLocalizedMessage(1071539); // Sorry. You cannot receive another item at this time.
                    return;
                }

                list[from]++;
            }
            else
            {
                list.Add(from, 0);
            }

            from.AddToBackpack(new RangersNecklace());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class RangersNecklace : BaseNecklace
    {
        public override string DefaultName => "Ankh Necklace";

        [Constructable]
        public RangersNecklace()
            : base(0x3BB5)
        {
            Hue = 2498;
        }

        public RangersNecklace(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "An Ankh Necklace",
                Description = "Recovered from a Fallen Ranger in Hythloth",
                Line1 = "You should return the item to Shamino's statue at once!",
            };

            g.RenderString(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
