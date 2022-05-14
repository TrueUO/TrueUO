using Server.Engines.Points;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Blackthorn
{
    public class AgentOfTheCrown : BaseTurnInMobile
    {
        public override int TitleLocalization => 1154520;  // Click a minor artifact to turn in for reward points.
        public override int CancelLocalization => 1154519; 	// Bring me items bearing the crest of Minax and I will reward you with valuable items.     
        public override int TurnInLocalization => 1154571;  // Turn In Minax Artifacts
        public override int ClaimLocalization => 1154572;  // Claim Blackthorn Artifacts

        [Constructable]
        public AgentOfTheCrown() : base("the Agent Of The Crown")
        {
        }

        public override void InitBody()
        {
            base.InitBody();

            Name = NameList.RandomName("male");

            Hue = Utility.RandomSkinHue();
            Body = 0x190;
            HairItemID = 0x2047;
            HairHue = 0x46D;
        }

        public override void InitOutfit()
        {
            SetWearable(new ChainChest(), 2106);
            SetWearable(new ThighBoots(), 2106);
            SetWearable(new Obi(), 1775);
            SetWearable(new BodySash(), 1775);
            SetWearable(new GoldRing());
            SetWearable(new Epaulette());
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1154517); // Minax Artifact Turn in Officer
        }

        public override void AwardPoints(PlayerMobile pm, Item item, int amount)
        {
            PointsSystem.Blackthorn.AwardPoints(pm, amount);
        }

        public override bool IsRedeemableItem(Item item)
        {
            if (item is BaseWeapon weapon && weapon.ReforgedSuffix == ReforgedSuffix.Minax)
                return true;
            if (item is BaseArmor armor && armor.ReforgedSuffix == ReforgedSuffix.Minax)
                return true;
            if (item is BaseJewel jewel && jewel.ReforgedSuffix == ReforgedSuffix.Minax)
                return true;
            if (item is BaseClothing clothing && clothing.ReforgedSuffix == ReforgedSuffix.Minax)
                return true;

            return false;
        }

        public override void SendRewardGump(Mobile m)
        {
            if (m.Player && m.CheckAlive())
                m.SendGump(new BlackthornRewardGump(this, m as PlayerMobile));
        }

        public AgentOfTheCrown(Serial serial)
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
