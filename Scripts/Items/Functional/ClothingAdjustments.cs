using Server.Engines.VeteranRewards;
using Server.Network;
using System;
using System.Linq;

namespace Server.Items
{
    public enum ClothRewardHue
    {
        Bronze = 0x972,
        Copper = 0x96D,
        Golden = 0x8A5,
        Agapite = 0x979,
        Verite = 0x89F,
        Valorite = 0x8AB,
        IceGreen = 0x47F,
        IceBlue = 0x482,
        DarkGray = 0x497,
        Fire = 0x489,
        IceWhite = 0x47E,
        JetBlack = 0x001,
        Pink = 0x490,
        Crimson = 0x485,
        GreenForest = 0x4A9,
        RoyalBlue = 0x538
    }

    public class ClothingRewardDefinition
    {
        public Type Type { get; set; }
        public ClothRewardHue Hue { get; set; }
        public int Label { get; set; }

        public ClothingRewardDefinition(Type type, ClothRewardHue hue, int cliloc)
        {
            Type = type;
            Hue = hue;
            Label = cliloc;
        }
    }

    public class ClothingAdjustments : Container
    {
        public static ClothingAdjustments InstanceTram { get; set; }
        public static ClothingAdjustments InstanceFel { get; set; }

        public static void Initialize()
        {
            if (InstanceTram == null)
            {
                InstanceTram = new ClothingAdjustments();
                InstanceTram.MoveToWorld(new Point3D(4331, 997, 7), Map.Trammel);
            }

            if (InstanceFel == null)
            {
                InstanceFel = new ClothingAdjustments();
                InstanceFel.MoveToWorld(new Point3D(4331, 997, 7), Map.Felucca);
            }
        }

        public override int LabelNumber => 1113958; // Clothing Adjustments

        [Constructable]
        public ClothingAdjustments()
            : base(0x9AB)
        {
            Hue = 2958;
            Weight = 0.0;
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113959); // I can't reach that.
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113959); // You can give me a veteran reward clothing you want to exchange for one of a different type but of the same color.
            }
        }

        public static ClothingRewardDefinition[] Definitions = new ClothingRewardDefinition[]
        {
            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Bronze, 1041286),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Bronze, 1041287),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Bronze, 1080366),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Bronze, 1113874),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Bronze, 1113875),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Copper, 1041288),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Copper, 1041289),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Copper, 1080367),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Copper, 1113876),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Copper, 1113877),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Agapite, 1041290),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Agapite, 1041291),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Agapite, 1080369),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Agapite, 1113878),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Agapite, 1113879),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Golden, 1041292),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Golden, 1041293),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Golden, 1080368),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Golden, 1113880),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Golden, 1113881),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Verite, 1041294),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Verite, 1041295),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Verite, 1080370),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Verite, 1113882),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Verite, 1113883),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Valorite, 1041296),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Valorite, 1041297),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Valorite, 1080371),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Valorite, 1113884),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Valorite, 1113885),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.DarkGray, 1049757),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.DarkGray, 1049756),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.DarkGray, 1080374),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.DarkGray, 1113886),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.DarkGray, 1113887),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.IceGreen, 1049759),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.IceGreen, 1049758),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.IceGreen, 1080372),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.IceGreen, 1113888),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.IceGreen, 1113889),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.IceBlue, 1049761),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.IceBlue, 1049760),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.IceBlue, 1080373),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.IceBlue, 1113890),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.IceBlue, 1113891),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.JetBlack, 1049763),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.JetBlack, 1049762),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.JetBlack, 1080377),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.JetBlack, 1113892),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.JetBlack, 1113893),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.IceWhite, 1049765),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.IceWhite, 1049764),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.IceWhite, 1080376),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.IceWhite, 1113894),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.IceWhite, 1113895),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Fire, 1049767),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Fire, 1049766),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Fire, 1080375),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Fire, 1113896),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Fire, 1113897),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Pink, 1080382),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Pink, 1080380),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Pink, 1080378),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Pink, 1113898),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Pink, 1113899),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.Crimson, 1080383),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.Crimson, 1080381),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.Crimson, 1080379),
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.Crimson, 1113900),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.Crimson, 1113901),            

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.GreenForest, 1113902),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.GreenForest, 1113904),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.GreenForest, 1113903),            
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.GreenForest, 1113905),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.GreenForest, 1113906),

            new ClothingRewardDefinition(typeof(RewardCloak), ClothRewardHue.RoyalBlue, 1113910),
            new ClothingRewardDefinition(typeof(RewardRobe), ClothRewardHue.RoyalBlue, 1113912),
            new ClothingRewardDefinition(typeof(RewardDress), ClothRewardHue.RoyalBlue, 1113911),            
            new ClothingRewardDefinition(typeof(RewardGargishFancyRobe), ClothRewardHue.RoyalBlue, 1113913),
            new ClothingRewardDefinition(typeof(RewardGargishRobe), ClothRewardHue.RoyalBlue, 1113914)
        };

        public bool IsAccept(Item item)
        {
            return Definitions.Any(t => t.Type == item.GetType());
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!base.OnDragDrop(from, dropped))
                return false;

            if (!IsAccept(dropped) || dropped is IRewardItem dr && !dr.IsRewardItem)
            {
                from.SendLocalizedMessage(1113955); // You may only trade veteran reward clothing.
                return false;
            }

            int index = 0;

            var clothlist = Definitions.Where(x => (int)x.Hue == dropped.Hue).ToArray();

            for (int i = 0; i < clothlist.Length - 1 ; i++)
            {
                if (clothlist[i].Type == dropped.GetType())
                {
                    index = i;

                    break;
                }
            }

            if (index + 1 > clothlist.Length)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            Item item = (Item)Activator.CreateInstance(clothlist[index].Type, clothlist[index].Hue, clothlist[index].Label);

            if (item != null)
            {
                if (item is IRewardItem r)
                {
                    r.IsRewardItem = true;
                }

                dropped.Delete();
                from.AddToBackpack(item);

                return true;
            }
            else
            {
                item.Delete();
            }

            return false;
        }

        public ClothingAdjustments(Serial serial)
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

            if (Map == Map.Trammel)
            {
                InstanceTram = this;
            }

            if (Map == Map.Felucca)
            {
                InstanceFel = this;
            }
        }
    }
}
