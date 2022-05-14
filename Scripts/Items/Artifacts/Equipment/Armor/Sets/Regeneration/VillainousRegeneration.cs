using Server.Mobiles;

namespace Server.Items
{
    public class HelmOfVillainousRegeneration : PlateHelm, IEpiphanyArmor
    {
        public override int LabelNumber => 1150838; // Helm of Villainous Regeneration

        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);        

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public HelmOfVillainousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (var index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (var index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public HelmOfVillainousRegeneration(Serial serial) : base(serial)
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

    public class GorgetOfVillainousRegeneration : PlateGorget, IEpiphanyArmor
    {
        public override int LabelNumber => 1150839; // Gorget of Villainous Regeneration

        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GorgetOfVillainousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (var index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (var index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public GorgetOfVillainousRegeneration(Serial serial) : base(serial)
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

    public class BreastplateOfVillainousRegeneration : PlateChest, IEpiphanyArmor
    {
        public override int LabelNumber => 1150840; // Breastplate of Villainous Regeneration

        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public BreastplateOfVillainousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (var index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (var index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public BreastplateOfVillainousRegeneration(Serial serial) : base(serial)
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

    public class ArmsOfVillainousRegeneration : PlateArms, IEpiphanyArmor
    {
        public override int LabelNumber => 1150841; // Arms of Villainous Regeneration

        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);        

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public ArmsOfVillainousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (var index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (var index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public ArmsOfVillainousRegeneration(Serial serial) : base(serial)
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

    public class GauntletsOfVillainousRegeneration : PlateGloves, IEpiphanyArmor
    {
        public override int LabelNumber => 1150842; // Gauntlets of Villainous Regeneration

        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);        

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GauntletsOfVillainousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                for (var index = 0; index < from.Items.Count; index++)
                {
                    Item armor = from.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                for (var index = 0; index < m.Items.Count; index++)
                {
                    Item armor = m.Items[index];

                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public GauntletsOfVillainousRegeneration(Serial serial) : base(serial)
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

    public class LegsOfVillainousRegeneration : PlateLegs, IEpiphanyArmor
    {
        public override int LabelNumber => 1150843; // Legs of Villainous Regeneration

        public Alignment Alignment => Alignment.Evil;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public LegsOfVillainousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 1778;
            ArmorAttributes.MageArmor = 1;
        }

        public override void AddWeightProperty(ObjectPropertyList list)
        {
            base.AddWeightProperty(list);

            EpiphanyHelper.AddProperties(this, list);
        }

        public override bool OnEquip(Mobile from)
        {
            bool canEquip = base.OnEquip(from);

            if (canEquip)
            {
                foreach (Item armor in from.Items)
                {
                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }

            return canEquip;
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (parent is Mobile m)
            {
                foreach (Item armor in m.Items)
                {
                    if (armor is IEpiphanyArmor)
                    {
                        armor.InvalidateProperties();
                    }
                }
            }
        }

        public LegsOfVillainousRegeneration(Serial serial) : base(serial)
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
