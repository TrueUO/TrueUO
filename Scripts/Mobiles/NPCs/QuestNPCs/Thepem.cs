using Server.Engines.BulkOrders;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests
{
    public class Thepem : MondainQuester, ITierQuester
    {
        public TierQuestInfo TierInfo => TierQuestInfo.Thepem;

        [Constructable]
        public Thepem()
            : base("Thepem", "the Apprentice")
        {
            SetSkill(SkillName.Alchemy, 85.0, 100.0);
            SetSkill(SkillName.TasteID, 65.0, 88.0);
        }

        public Thepem(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests => Array.Empty<Type>();

        #region Bulk Orders
        public override BODType BODType => BODType.Alchemy;

        public override bool IsValidBulkOrder(Item item)
        {
            return item is SmallAlchemyBOD || item is LargeAlchemyBOD;
        }

        public override bool SupportsBulkOrders(Mobile from)
        {
            return from is PlayerMobile && from.Skills[SkillName.Alchemy].Base > 0;
        }

        public override void OnSuccessfulBulkOrderReceive(Mobile from)
        {
            if (from is PlayerMobile mobile)
                mobile.NextAlchemyBulkOrder = TimeSpan.Zero;
        }

        #endregion

        public override bool IsActiveVendor => true;

        protected override List<SBInfo> SBInfos => m_SBInfos;

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBAlchemist(this));
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = true;
            Race = Race.Gargoyle;

            Hue = 0x86E1;
            HairItemID = 0x42AB;
            HairHue = 0x385;
        }

        public override void InitOutfit()
        {
            AddItem(new GargishClothLegs(0x70F));
            AddItem(new GargishClothKilt(0x742));
            AddItem(new GargishClothChest(0x4C3));
            AddItem(new GargishClothArms(0x738));
        }

        private static readonly Type[][] m_PileTypes =
        {
                new[] {typeof(DullCopperIngot),  typeof(PileofInspectedDullCopperIngots) },
                new[] {typeof(ShadowIronIngot),  typeof(PileofInspectedShadowIronIngots) },
                new[] {typeof(CopperIngot),      typeof(PileofInspectedCopperIngots) },
                new[] {typeof(BronzeIngot),      typeof(PileofInspectedBronzeIngots) },
                new[] {typeof(GoldIngot),        typeof(PileofInspectedGoldIngots) },
                new[] {typeof(AgapiteIngot),     typeof(PileofInspectedAgapiteIngots) },
                new[] {typeof(VeriteIngot),      typeof(PileofInspectedVeriteIngots) },
                new[] {typeof(ValoriteIngot),    typeof(PileofInspectedValoriteIngots) }
            };

        private const int NeededIngots = 20;

        private Type GetPileType(Item item)
        {
            Type itemType = item.GetType();

            for (int i = 0; i < m_PileTypes.Length; i++)
            {
                Type[] pair = m_PileTypes[i];

                if (itemType == pair[0])
                    return pair[1];
            }

            return null;
        }

        public override bool OnDragDrop(Mobile from, Item item)
        {
            Type pileType = GetPileType(item);

            if (pileType != null)
            {
                if (item.Amount > NeededIngots)
                {
                    SayTo(from, 1113037); // That's too many.
                    return false;
                }

                if (item.Amount < NeededIngots)
                {
                    SayTo(from, 1113036); // That's not enough.
                    return false;
                }

                SayTo(from, 1113040); // Good. I can use this.

                from.AddToBackpack(Activator.CreateInstance(pileType) as Item);
                from.SendLocalizedMessage(1113041); // Now mark the inspected item as a quest item to turn it in.					

                return true;
            }

            if (item is Gold)
            {
                return base.CheckGold(from, item);
            }

            SayTo(from, 1113035); // Oooh, shiney. I have no use for this, though.
            return false;
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
