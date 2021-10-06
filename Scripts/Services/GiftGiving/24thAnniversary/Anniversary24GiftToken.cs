using Server.Gumps;

namespace Server.Items
{
    public class Anniversary24GiftToken : Item, IRewardOption
    {
        public override int LabelNumber => 1159739; // 24th Anniversary Gift Token

        [Constructable]
        public Anniversary24GiftToken()
            : base(0x4BC6)
        {
            Hue = 2758;
            LootType = LootType.Blessed;
        }

        public Anniversary24GiftToken(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(RewardOptionGump));
                from.SendGump(new RewardOptionGump(this, 1156888));
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
            }
        }

        public void GetOptions(RewardOptionList list)
        {
            list.Add(1, 1159740); // Opal Encrusted Mobius
            list.Add(2, 1159741); // Opal Standing Harp
            list.Add(3, 1159742); // Ornamental Opal
        }


        public void OnOptionSelected(Mobile from, int choice)
        {
            Bag bag = new Bag
            {
                Hue = 2758
            };

            switch (choice)
            {
                default:
                    bag.Delete();
                    break;
                case 1:
                    {
                        bag.DropItem(new OpalEncrustedMobius());
                        from.AddToBackpack(bag);
                        Delete();
                        break;
                    }
                case 2:
                    {
                        bag.DropItem(new OpalStandingHarp());

                        from.AddToBackpack(bag);
                        Delete();
                        break;
                    }

                case 3:
                    {
                        bag.DropItem(new OrnamentalOpal());
                        from.AddToBackpack(bag);
                        Delete();
                        break;
                    }
            }
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
