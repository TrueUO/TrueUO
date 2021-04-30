using Server.Engines.Quests;
using Server.Items;

namespace Server.Mobiles
{
    public class Deirdre : HumilityQuestMobile
    {
        public override int Greeting => 1075744; // The cloak thou wearest looks warm.

        [Constructable]
        public Deirdre()
            : base("Dierdre", "the Beggar")
        {
        }

        public Deirdre(Serial serial)
            : base(serial)
        {
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            Race = Race.Human;
            Body = 0x191;

            SpeechHue = 20;

            Hue = Race.RandomSkinHue();
            HairItemID = Race.RandomHair(true);
            HairHue = Race.RandomHairHue();
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Sandals());
            AddItem(new FancyShirt());
            AddItem(new PlainDress());
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
}
