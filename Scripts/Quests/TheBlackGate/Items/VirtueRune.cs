using Server.Gumps;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class VirtueRune : Item, IFlipable
    {
        [Constructable]
        public VirtueRune()
            : this(virtueList.ElementAt(Utility.Random(virtueList.Count)))
        {
        }

        public VirtueRune(KeyValuePair<string, int> random)
           : this(random.Key, random.Value)
        {
        }

        public VirtueRune(string name, int id)
            : base(id)
        {
            Name = name;
        }

        public VirtueRune(Serial serial)
            : base(serial)
        {
        }

        public static int GetRandomVirtueID()
        {
            return virtueList.ElementAt(Utility.Random(virtueList.Count)).Value;
        }

        public void OnFlip(Mobile m)
        {
            if (virtueList.ContainsValue(ItemID))
            {
                ItemID++;
            }
            else
            {
                ItemID--;
            }
        }

        private static readonly Dictionary<string, int> _virtueList = new Dictionary<string, int> 
        {
            {"Compassion", 0xA51B },
            {"Honesty", 0xA519},
            {"Honor", 0xA51D},
            {"Humility", 0xA521},
            {"Justice", 0xA51F},
            {"Sacrifice", 0xA523},
            {"Spirituality", 0xA517},
            {"Valor", 0xA525}
        };

        public static Dictionary<string, int> virtueList => _virtueList;

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = Name,
                Description = "Given to you by Julia as a thanks for restoring protection to the Shrines of Britannia",
                Line1 = "The artisanship of the heavy stone rune is like nothing you have ever seen. The sigil is carved and adorned with exacting precision.",
                Line2 = "Despite the the size and heft of the stone, it is uncharacteristically easy to move."
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
