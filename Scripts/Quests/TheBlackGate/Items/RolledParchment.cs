using Server.Gumps;
using Server.Network;
using System.Collections.Generic;

namespace Server.Items
{
    public class RolledParchment : Item
    {
        public override string DefaultName => "a Rolled Parchment";

        private readonly Dictionary<Mobile, int> list = new Dictionary<Mobile, int>();

        [Constructable]
        public RolledParchment()
            : base(0x46B3)
        {
            Hue = 66;
            Weight = 0.0;
            Movable = false;
        }

        public RolledParchment(Serial serial)
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

            PrivateOverheadMessage(MessageType.Regular, 1150, false, "*The parchment seems to have fallen from someone's pocket. You collect it and think it may be of interest to Jaana*", from.NetState);

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

            from.AddToBackpack(new ThreateningNote());            
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

    public class ThreateningNote : Item
    {
        [Constructable]
        public ThreateningNote()
            : base(0x46B3)
        {
            Name = "a Threatening Note";
            Hue = 66;
        }

        public ThreateningNote(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "A Threatening Note",
                Description = "Found at the site of the Yew Winery Break in",
                Line1 = "The note is quite disturbing. The details threaten the accused should they not steal from the Winery!",
                Line2 = "Janna will most definitely be interested in this information!"
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
