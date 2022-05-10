using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests
{
    public class CorpseOfBennetYardley : Item, IConditionalVisibility
    {
        public override bool ForceShowProperties => true;
        public override int LabelNumber => 1158168;

        public CorpseOfBennetYardley()
            : base(Utility.Random(0xECA, 9))
        {
            Movable = false;
        }

        public bool CanBeSeenBy(PlayerMobile pm)
        {
            if (pm.AccessLevel > AccessLevel.Player)
                return true;

            return false;
        }

        public CorpseOfBennetYardley(Serial serial)
            : base(serial)
        {
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

    public class TreasureHuntingBook : Item
    {
        [Constructable]
        public TreasureHuntingBook()
            : base(0xFBE)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m is PlayerMobile mobile && IsChildOf(mobile.Backpack))
            {
                mobile.CloseGump(typeof(InternalGump));
                BaseGump.SendGump(new InternalGump(mobile));
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1158253); // Treasure Hunting: A Practical Approach
            list.Add(1154760, "#1158254"); // By: Vespyr Jones
        }

        public class InternalGump : BaseGump
        {
            public InternalGump(PlayerMobile pm)
                : base(pm, 10, 10)
            {
            }

            public override void AddGumpLayout()
            {
                AddImage(0, 0, 0x761C);
                AddImage(112, 40, 0x655);
                AddHtmlLocalized(113, 350, 342, 280, 1158255, C32216(0x080808), false, true);
            }
        }

        public TreasureHuntingBook(Serial serial)
            : base(serial)
        {
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

    public class PaladinCorpse : Container
    {
        public override int LabelNumber => 1158135;  // the remains of a would-be paladin
        public override bool HandlesOnMovement => true;
        public override bool IsDecoContainer => false;

        public PaladinCorpse()
            : base(0x9F1E)
        {
            DropItem(new WouldBePaladinChronicles());
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (InRange(m.Location, 2) && !InRange(oldLocation, 2))
            {
                PrivateOverheadMessage(MessageType.Regular, 1154, 1158137, m.NetState); // *You notice the skeleton clutching a small journal...*
            }
        }

        public PaladinCorpse(Serial serial)
            : base(serial)
        {
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

    public class WouldBePaladinChronicles : BaseJournal
    {
        public override int LabelNumber => 1094837;  // a journal

        public override TextDefinition Title => null;
        public override TextDefinition Body => 1158138;
        /**the text is mostly a journal chronicling the adventures of a man who wished to join the Paladins of Trinsic.  
         * Of particular note is the final entry...* This is the most shameful entry I will write...for I have fallen
         * short of my goal. My only hope is my failures will serve to assist those who come after me with the courage to 
         * pursue the truth, and with my notes they will find success. I have found strange crystals on the corpses of the 
         * creatures I slay here. When I touch the crystal, I can feel it absorbed into my being. A growing voice inside me 
         * compels me to altars located throughout the dungeon. When the voice within grew loud enough I could no longer 
         * ignore it, I touched my hand to the altar and before me a grand champion stood! I was quick to react to the
         * crushing blow my newly summoned opponent sought to deliver, and I was victorious! Alas, the deeper into the 
         * dungeon I explored, the more powerful the altar champions become and now I find myself in this dire situation.... 
         * To anyone who reads this...do me the honor and defeat the three champions and slay the unbound energy vortexes that 
         * inhabit the deepest depths of this place...for I feel my time here is shor...*/

        public WouldBePaladinChronicles()
        {
            Movable = false;
        }

        public override void OnDoubleClick(Mobile m)
        {
            m.SendGump(new BaseJournalGump(Title, Body));
        }

        public WouldBePaladinChronicles(Serial serial)
            : base(serial)
        {
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
