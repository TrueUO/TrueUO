using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Items
{
    public enum EggStage
    {
        New,
        Stage1,
        Stage2,
        Mature,
        Burnt
    }

    public enum Dryness
    {
        Moist,
        Dry,
        Parched,
        Dehydrated
    }

    public class ChickenLizardEgg : Item
    {
        public virtual bool CanMutate => true;

        private DateTime m_IncubationStart;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime IncubationStart
        {
            get => m_IncubationStart;
            set
            {
                m_IncubationStart = value;
                InvalidateProperties();
            }
        }

        private TimeSpan m_TotalIncubationTime;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TotalIncubationTime
        {
            get => m_TotalIncubationTime;
            set
            {
                m_TotalIncubationTime = value;
                IncubationStart = DateTime.UtcNow;
                InvalidateProperties();
            }
        }

        private bool m_Incubating;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Incubating
        {
            get => m_Incubating;
            set
            {
                if (m_Incubating && !value && IncubationStart < DateTime.UtcNow)
                {
                    TotalIncubationTime += DateTime.UtcNow - IncubationStart;
                }

                m_Incubating = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public EggStage Stage { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Dryness Dryness { get; set; }

        private bool m_IsBattleChicken;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsBattleChicken { get; set; }

        [Constructable]
        public ChickenLizardEgg()
            : base(0x41BD)
        {
            m_Incubating = false;
            m_TotalIncubationTime = TimeSpan.Zero;
            Stage = EggStage.New;
            Dryness = Dryness.Dry;
        }

        public ChickenLizardEgg(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get
            {
                int c = 1112469; // an egg

                if (IncubationStart != DateTime.MinValue)
                {
                    if (Stage == EggStage.Mature)
                    {
                        c = m_IsBattleChicken ? 1112468 : 1112467; // a mature battle chicken lizard egg || a mature egg
                    }
                    else if (Stage == EggStage.Burnt)
                    {
                        c = 1112466; // a burnt egg
                    }
                    else
                    {
                        switch (Dryness)
                        {
                            case Dryness.Moist: c = 1112462; break; // a moist egg
                            case Dryness.Dry: c = 1112463; break; // a dry egg
                            case Dryness.Parched: c = 1112464; break; // a parched egg
                            case Dryness.Dehydrated: c = 1112465; break; // a dehydrated egg
                        }
                    }
                }

                return c;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1113429); // chicken lizard 
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            bool check = base.DropToMobile(from, target, p);

            if (check && m_Incubating)
                Incubating = false;

            return check;
        }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            bool check = base.DropToWorld(from, p);

            if (check && m_Incubating)
                Incubating = false;

            return check;
        }

        public override bool DropToItem(Mobile from, Item target, Point3D p)
        {
            bool check = base.DropToItem(from, target, p);

            if (check && !(Parent is Incubator) && m_Incubating)
                Incubating = false;

            return check;
        }

        public override void OnItemLifted(Mobile from, Item item)
        {
            if (m_Incubating)
                Incubating = false;

            base.OnItemLifted(from, item);
        }

        public void CheckStatus()
        {
            if (Stage == EggStage.Burnt)
                return;

            if (m_Incubating && IncubationStart < DateTime.UtcNow)
                TotalIncubationTime += DateTime.UtcNow - IncubationStart;

            if (m_TotalIncubationTime > TimeSpan.FromHours(24) && Stage == EggStage.New)           //from new to stage 1
            {
                IncreaseStage();
                //Nothing, egg goes to stage 2 regardless if its watered or not
            }
            else if (m_TotalIncubationTime >= TimeSpan.FromHours(48) && Stage == EggStage.Stage1)  //from stage 1 to stage 2
            {
                if (Dryness >= Dryness.Parched)
                {
                    if (Utility.RandomBool())
                        BurnEgg();
                }

                IncreaseStage();
            }
            else if (m_TotalIncubationTime >= TimeSpan.FromHours(72) && Stage == EggStage.Stage2)  //from stage 2 to mature egg 
            {
                if (Dryness >= Dryness.Parched)
                {
                    if (.25 < Utility.RandomDouble())
                        BurnEgg();
                }

                IncreaseStage();
            }
            else if (m_TotalIncubationTime >= TimeSpan.FromHours(120) && Stage == EggStage.Mature)
            {
                BurnEgg();
                IncreaseStage();
            }
        }

        public void Pour(Mobile from, BaseBeverage bev)
        {
            if (!bev.IsEmpty && bev.Pourable && bev.Content == BeverageType.Water && bev.ValidateUse(from, false))
            {
                if (Stage == EggStage.New || Stage == EggStage.Stage1 || Stage == EggStage.Stage2)
                    IncreaseDryness();

                bev.Quantity--;

                from.SendLocalizedMessage(1112461); // You pour some water on the egg, moistening its shell.

                InvalidateProperties();
            }
        }

        public void IncreaseDryness()
        {
            if (Dryness != Dryness.Moist)
            {
                Dryness--;
            }
        }

        public void DecreaseDryness()
        {
            if (Dryness != Dryness.Dehydrated)
            {
                Dryness++;
            }
        }

        public void IncreaseStage()
        {
            if (Stage != EggStage.Burnt)
            {
                Stage++;
                DecreaseDryness();
            }

            switch (Stage)
            {
                default:
                case EggStage.Stage1:
                    ItemID = 0x41BE;
                    break;
                case EggStage.Stage2:
                    ItemID = 0x41BF;
                    break;
                case EggStage.Mature:
                    {
                        ItemID = 0x41BF;

                        Hue = 651; // standart mature hue

                        double chance = .10;
                        if (Dryness == Dryness.Dry)
                            chance = .05;
                        else if (Dryness == Dryness.Parched)
                            chance = .01;
                        else if (Dryness == Dryness.Dehydrated)
                            chance = 0;

                        if (CanMutate && chance >= Utility.RandomDouble())
                        {
                            m_IsBattleChicken = true;
                            Hue = GetRandomBattleChickenLizardHue();
                        }

                        break;
                    }
                case EggStage.Burnt:
                    ItemID = 0x41BF;
                    Hue = 1109; // Brunt hue
                    break;
            }

            InvalidateProperties();
        }

        private int GetRandomBattleChickenLizardHue()
        {
            switch (Utility.Random(20))
            {
                case 0: return 660;     // Green hue
                case 1: return 60;      // Light Green hue
                case 2: return 2001;    // Dark Green hue
                case 3: return 1372;    // Strong Green hue
                case 4: return 71;      // Dryad Green hue
                case 5: return 678;     // Nox Green hue
                case 6: return 291;     // Ice Green hue
                case 7: return 1151;    // Cyan hue
                case 8: return 691;     // Valorite hue
                case 9: return 1154;    // Ice Blue hue
                case 10: return 1165;   // Light Blue hue
                case 11: return 1173;   // Strong Cyan hue
                case 12: return 1301;   // Midnight Blue hue
                case 13: return 1159;   // Blue & Yellow hue
                case 14: return 55;     // Strong Yellow hue
                case 15: return 50;     // Gold hue
                case 16: return 1002;   // Pink hue
                case 17: return 35;     // Red hue
                case 18: return 1168;   // Strong Purple hue
                case 19: return 1105;   // Black hue
            }

            return 0;
        }

        public void BurnEgg()
        {
            Stage = EggStage.Burnt;
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendGump(new ConfirmHatchGump(from, this));
        }

        public void TryHatchEgg(Mobile from)
        {
            if (Stage == EggStage.Mature)
                OnHatch(from);
            else
                CrumbleEgg(from);
        }

        public virtual void OnHatch(Mobile from)
        {
            BaseCreature bc;

            if (m_IsBattleChicken)
            {
                from.SendLocalizedMessage(1112478); // You hatch a battle chicken lizard!!
                bc = new BattleChickenLizard
                {
                    Hue = Hue
                };
            }
            else
            {
                from.SendLocalizedMessage(1112477); // You hatch a chicken lizard.
                bc = new ChickenLizard();
            }

            bc.MoveToWorld(from.Location, from.Map);
            Delete();
        }

        public void CrumbleEgg(Mobile from)
        {
            from.SendLocalizedMessage(1112447); // You hatch the egg but it crumbles in your hands!
            Delete();
        }

        public class ConfirmHatchGump : Gump
        {
            private readonly ChickenLizardEgg m_Egg;
            private readonly Mobile m_From;

            public ConfirmHatchGump(Mobile from, ChickenLizardEgg egg)
                : base(340, 340)
            {
                m_Egg = egg;
                m_From = from;

                AddPage(0);

                AddBackground(0, 0, 291, 99, 0x13BE);
                AddImageTiled(5, 6, 280, 20, 0xA40);
                AddHtmlLocalized(9, 8, 280, 20, 1112444, 0x7FFF, false, false); // Egg Hatching
                AddImageTiled(5, 31, 280, 40, 0xA40);
                AddHtmlLocalized(9, 35, 272, 40, egg.Stage == EggStage.Mature ? 1112446 : 1112445, 0x7FFF, false, false); // Do you wish to hatch your egg now? || Hatching a non-mature or burnt egg will likely destroy it. Do you wish to proceed?
                AddButton(215, 73, 0xFB7, 0xFB8, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(250, 75, 65, 20, 1006044, 0x7FFF, false, false); // OK
                AddButton(5, 73, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(40, 75, 100, 20, 1060051, 0x7FFF, false, false); // CANCEL
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (m_Egg == null || m_Egg.Deleted)
                {
                    return;
                }

                switch (info.ButtonID)
                {
                    case 0:
                        break;
                    case 1:
                        m_Egg.TryHatchEgg(m_From);
                        break;
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1); // version

            writer.Write((int)Dryness);
            writer.Write(IncubationStart);
            writer.Write(m_TotalIncubationTime);
            writer.Write(m_Incubating);
            writer.Write((int)Stage);
            writer.Write(m_IsBattleChicken);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();

            switch(version)
            {
                case 1:
                    {
                        Dryness = (Dryness)reader.ReadInt();
                        m_IncubationStart = reader.ReadDateTime();
                        m_TotalIncubationTime = reader.ReadTimeSpan();
                        m_Incubating = reader.ReadBool();
                        Stage = (EggStage)reader.ReadInt();
                        m_IsBattleChicken = reader.ReadBool();
                        break;
                    }
                case 0:
                    {
                        m_IncubationStart = reader.ReadDateTime();
                        m_TotalIncubationTime = reader.ReadTimeSpan();
                        m_Incubating = reader.ReadBool();
                        Stage = (EggStage)reader.ReadInt();
                        int waterLevel = reader.ReadInt();
                        m_IsBattleChicken = reader.ReadBool();

                        int v = (int)Stage - waterLevel;

                        if (v >= 2 && waterLevel == 0)
                            Dryness = Dryness.Dehydrated;
                        else if (v >= 2)
                            Dryness = Dryness.Parched;
                        else if (v >= 1)
                            Dryness = Dryness.Dry;
                        else
                            Dryness = Dryness.Moist;

                        break;
                    }
            }
        }
    }
}
