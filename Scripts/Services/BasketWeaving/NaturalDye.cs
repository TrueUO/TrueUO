using Server.Engines.Plants;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
    public class NaturalDye : Item, IPigmentHue
    {
        private PlantPigmentHue m_Hue;
        private int m_UsesRemaining;
        [Constructable]
        public NaturalDye()
            : this(PlantPigmentHue.None)
        {
        }

        [Constructable]
        public NaturalDye(PlantPigmentHue hue)
            : base(0x182B)
        {
            Weight = 1.0;
            PigmentHue = hue;
            m_UsesRemaining = 5;
        }

        public NaturalDye(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlantPigmentHue PigmentHue
        {
            get => m_Hue;
            set
            {
                m_Hue = value;
                // set any invalid pigment hue to Plain
                if (m_Hue != PlantPigmentHueInfo.GetInfo(m_Hue).PlantPigmentHue)
                    m_Hue = PlantPigmentHue.Plain;
                Hue = PlantPigmentHueInfo.GetInfo(m_Hue).Hue;
                InvalidateProperties();
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get => m_UsesRemaining;
            set
            {
                m_UsesRemaining = value;
                InvalidateProperties();
            }
        }
        public override int LabelNumber => 1112136;// natural dye

        public bool RetainsColorFrom => true;

        public override bool ForceShowProperties => true;

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            PlantPigmentHueInfo hueInfo = PlantPigmentHueInfo.GetInfo(m_Hue);

            if (Amount > 1)
                list.Add(PlantPigmentHueInfo.IsBright(m_Hue) ? 1113277 : 1113276, "{0}\t{1}", Amount, "#" + hueInfo.Name);  // ~1_COLOR~ Softened Reeds
            else
                list.Add(hueInfo.IsBright() ? 1112138 : 1112137, "#" + hueInfo.Name);  // ~1_COLOR~ natural dye
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version

            writer.Write((int)m_Hue);
            writer.Write(m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_Hue = (PlantPigmentHue)reader.ReadInt();
                    m_UsesRemaining = reader.ReadInt();
                    break;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendLocalizedMessage(1112139); // Select the item you wish to dye.
            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private readonly NaturalDye m_Item;
            public InternalTarget(NaturalDye item)
                : base(1, false, TargetFlags.None)
            {
                m_Item = item;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Item.Deleted)
                    return;

                if (targeted is Item item)
                {
                    bool valid = item is IDyable || item is BaseTalisman || item is BaseBook || item is BaseClothing ||
                                 item is BaseJewel || item is BaseStatuette || item is BaseWeapon || item is Runebook ||
                                 item is Spellbook || item is DecorativePlant || item is ShoulderParrot || item.IsArtifact ||
                                 item is ThreeTieredCake || BasePigmentsOfTokuno.IsValidItem(item);

                    if (item is HoodedShroudOfShadows || item is MonkRobe)
                    {
                        from.SendLocalizedMessage(1042083); // You cannot dye that.
                        return;
                    }

                    if (!valid && FurnitureAttribute.Check(item))
                    {
                        if (!from.InRange(m_Item.GetWorldLocation(), 1) || !from.InRange(item.GetWorldLocation(), 1))
                        {
                            from.SendLocalizedMessage(500446); // That is too far away.
                            return;
                        }

                        BaseHouse house = BaseHouse.FindHouseAt(item);

                        if (house == null || !house.IsLockedDown(item) && !house.IsSecure(item))
                        {
                            from.SendLocalizedMessage(501022); // Furniture must be locked down to paint it.
                            return;
                        }

                        if (!house.IsCoOwner(from))
                        {
                            from.SendLocalizedMessage(501023); // You must be the owner to use this item.
                            return;
                        }

                        valid = true;
                    }
                    else if (!item.IsChildOf(from.Backpack))
                    {
                        from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
                        return;
                    }

                    if (!valid && item is BaseArmor armor)
                    {
                        CraftResourceType restype = CraftResources.GetType(armor.Resource);

                        if ((CraftResourceType.Leather == restype || CraftResourceType.Metal == restype) && ArmorMaterialType.Bone != armor.MaterialType)
                        {
                            valid = true;
                        }
                    }

                    // need to add any bags, chests, boxes, crates not IDyable but dyable by natural dyes

                    if (valid)
                    {
                        item.Hue = PlantPigmentHueInfo.GetInfo(m_Item.PigmentHue).Hue;
                        from.PlaySound(0x23E);

                        if (--m_Item.UsesRemaining > 0)
                            m_Item.InvalidateProperties();
                        else
                            m_Item.Delete();

                        return;
                    }
                }

                from.SendLocalizedMessage(1042083); // You cannot dye that.
            }
        }
    }
}
