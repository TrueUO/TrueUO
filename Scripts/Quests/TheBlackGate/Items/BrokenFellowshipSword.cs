using Server.Gumps;

namespace Server.Items
{
    public class BrokenFellowshipSword : Item
    {
        public override string DefaultName => "a Broken Fellowship Sword";

        [Constructable]
        public BrokenFellowshipSword()
            : base(0xA33F)
        {
            Hue = 2117;
        }

        public BrokenFellowshipSword(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            QuestRewardGump g = new QuestRewardGump(this, from)
            {
                Title = "Recovered From a Fellowship Killer in Destard",
                Description = "Given by Geoffrey in Recognition for Valiant Efforts Against the Fellowship",
                Line1 = "The blade is in quite poor repair. clearly showing signs of its use in a fierce battle. The former owner fared far worse.",
                Line2 = "Your valiant efforts in Destard stopped the Fellowship Killers from raising an army of dragons against Britannia."
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
