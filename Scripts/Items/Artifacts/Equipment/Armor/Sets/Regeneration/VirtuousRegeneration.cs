using Server.Mobiles;

namespace Server.Items
{
    public class HelmOfVirtuousRegeneration : BoneHelm, IEpiphanyArmor
    {
        public override int LabelNumber => 1150832; // Helm of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);        

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public HelmOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public HelmOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class GorgetOfVirtuousRegeneration : PlateGorget, IEpiphanyArmor
    {
        public override int LabelNumber => 1150833; // Gorget of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int BasePhysicalResistance => 2;
        public override int BaseFireResistance => 4;
        public override int BaseColdResistance => 3;
        public override int BasePoisonResistance => 3;
        public override int BaseEnergyResistance => 4;
        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;
        public override int StrReq => 25;

        [Constructable]
        public GorgetOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Weight = 1.0;
            Hue = 2076;
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

        public GorgetOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class BreastplateOfVirtuousRegeneration : BoneArms, IEpiphanyArmor
    {
        public override int LabelNumber => 1150834; // Breastplate of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);        

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;
        public override int StrReq => 60;

        [Constructable]
        public BreastplateOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Weight = 6.0;
            Hue = 2076;
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

        public BreastplateOfVirtuousRegeneration(Serial serial)
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

    public class ArmsOfVirtuousRegeneration : BoneArms, IEpiphanyArmor
    {
        public override int LabelNumber => 1150835; // Arms of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public ArmsOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public ArmsOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class GauntletsOfVirtuousRegeneration : BoneGloves, IEpiphanyArmor
    {
        public override int LabelNumber => 1150836; // Gauntlets of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GauntletsOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public GauntletsOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class LegsOfVirtuousRegeneration : BoneLegs, IEpiphanyArmor
    {
        public override int LabelNumber => 1150837; // Legs of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public LegsOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public LegsOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class KiltOfVirtuousRegeneration : GargishPlateKilt, IEpiphanyArmor
    {
        public override int LabelNumber => 1150851; // Kilt of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public KiltOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public KiltOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class EarringsOfVirtuousRegeneration : GargishEarrings, IEpiphanyArmor
    {
        public override int LabelNumber => 1150846; // Earrings of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public EarringsOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public EarringsOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class GargishBreastplateOfVirtuousRegeneration : GargishStoneChest, IEpiphanyArmor
    {
        public override int LabelNumber => 1150834; // Breastplate of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GargishBreastplateOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public GargishBreastplateOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class GargishArmsOfVirtuousRegeneration : GargishStoneArms, IEpiphanyArmor
    {
        public override int LabelNumber => 1150835; // Arms of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GargishArmsOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public GargishArmsOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class NecklaceOfVirtuousRegeneration : GargishNecklace, IEpiphanyArmor
    {
        public override int LabelNumber => 1150847; // Necklace of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);        

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public NecklaceOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public NecklaceOfVirtuousRegeneration(Serial serial) : base(serial)
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

    public class GargishLegsOfVirtuousRegeneration : GargishStoneLegs, IEpiphanyArmor
    {
        public override int LabelNumber => 1150837; // Legs of Virtuous Regeneration

        public Alignment Alignment => Alignment.Good;
        public SurgeType Type => SurgeType.Hits;
        public int Frequency => EpiphanyHelper.GetFrequency(Parent as Mobile, this);
        public int Bonus => EpiphanyHelper.GetBonus(Parent as Mobile, this);

        public override int InitMinHits => 255;
        public override int InitMaxHits => 255;

        [Constructable]
        public GargishLegsOfVirtuousRegeneration()
        {
            Quality = ItemQuality.Exceptional;
            Hue = 2076;
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

        public GargishLegsOfVirtuousRegeneration(Serial serial)
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
