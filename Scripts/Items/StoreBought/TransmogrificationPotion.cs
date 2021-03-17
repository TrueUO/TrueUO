using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Mobiles.MannequinProperty;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class RobeTransmogrificationPotion : BaseTransmogrificationPotion
    {
        public override Layer ItemLayer => Layer.OuterTorso;

        public override int SlotLabel => 1159503; // Robe Slot

        public override int ValidFailMessage => 1159500; // That is not a valid robe-slot item.

        public override int GumpDesciption => 1159496; // This will allow you to transfer the properties from one robe slot item to another robe slot item.  Use the "Set Source Object" to set the object that you wish to transfer the properties FROM.  Use "Set Destination Object" to set the object that you wish to transfer the properties TO.  The destination object must be free of all magical properties.  You will be presented with a confirmation before the transfer is finalized. The destination object will retain its hue after the transfer. If the source object has a unique name then it will be transferred to the destination object as well. The blessed status will be retained. This process is final and cannot be undone! 

        [Constructable]
        public RobeTransmogrificationPotion()
            : base()
        {
            Hue = 2741;
        }

        public override bool CheckMagicalItem(List<ValuedProperty> props)
        {
            return props.Any(x => x.Value != 0);
        }

        public RobeTransmogrificationPotion(Serial serial)
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

    public class HeadTransmogrificationPotion : BaseTransmogrificationPotion
    {
        public override Layer ItemLayer => Layer.Helm;

        public override int SlotLabel => 1159569; // Head Slot

        public override int ValidFailMessage => 1159559; // That is not a valid head-slot item.

        public override int GumpDesciption => 1159568; // This will allow you to transfer the properties from one shield slot item to another shield slot item.  Use the "Set Source Object" to set the object that you wish to transfer the properties FROM.  Use "Set Destination Object" to set the object that you wish to transfer the properties TO.  The destination object must be free of all magical properties.  You will be presented with a confirmation before the transfer is finalized. The destination object will retain its hue after the transfer. If the source object has a unique name then it will be transferred to the destination object as well. The blessed status will be retained. This process is final and cannot be undone! The resulting item can not be imbued, reforged, or enhanced. 

        [Constructable]
        public HeadTransmogrificationPotion()
            : base()
        {
            Hue = 2736;
        }

        public override bool CheckMagicalItem(List<ValuedProperty> props)
        {
            return props.Any(x => !_ExcludeArmorProperties.Contains(x.GetType()) && x is MageArmorProperty);
        }

        public HeadTransmogrificationPotion(Serial serial)
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

    public class ShieldTransmogrificationPotion : BaseTransmogrificationPotion
    {
        public override Layer ItemLayer => Layer.TwoHanded;

        public override int SlotLabel => 1159562; // Shield Slot

        public override int ValidFailMessage => 1159558; // That is not a valid shield-slot item.

        public override int GumpDesciption => 1159567; // This will allow you to transfer the properties from one head slot item to another head slot item. Use the "Set Source Object" to set the object that you wish to transfer the properties FROM. Use "Set Destination Object" to set the object that you wish to transfer the properties TO. The destination object must be free of all magical properties. You will be presented with a confirmation before the transfer is finalized. The destination object will retain its hue after the transfer. If the source object has a unique name then it will be transferred to the destination object as well. The blessed status will be retained. This process is final and cannot be undone! The resulting item can not be imbued, reforged, or enhanced.

        [Constructable]
        public ShieldTransmogrificationPotion()
            : base()
        {
            Hue = 2732;
        }

        public override bool CheckRules()
        {
            if (Destination.Item is BaseShield == Source.Item is BaseShield || Destination.Item is BaseLight == Source.Item is BaseLight)
            {
                return true;
            }

            return false;
        }
        

        public override bool CheckMagicalItem(List<ValuedProperty> props)
        {
            return props.Any(x => !_ExcludeArmorProperties.Contains(x.GetType()));
        }

        public ShieldTransmogrificationPotion(Serial serial)
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

    [PropertyObject]
    public class ItemProps
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public Item Item { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public List<ValuedProperty> Props { get; set; }
    }

    public class BaseTransmogrificationPotion : Item
    {
        public override int LabelNumber => 1159501;  // Transmogrification Potion
        
        [CommandProperty(AccessLevel.GameMaster)]
        public ItemProps Source { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ItemProps Destination { get; set; }

        public virtual Layer ItemLayer => 0;

        public virtual int SlotLabel => 0;

        public virtual int ValidFailMessage => 0;

        public virtual int GumpDesciption => 0;

        [Constructable]
        public BaseTransmogrificationPotion()
            : base(0xA1E9)
        {
        }

        public BaseTransmogrificationPotion(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(BaseTransmogrificationPotionGump));
                from.SendGump(new BaseTransmogrificationPotionGump(this));
            }
            else
            {
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
            }
        }

        public readonly Type[] _ExcludeArmorProperties = new Type[]
        {
            typeof(PhysicalResistProperty),  // Physical Resist
            typeof(FireResistProperty),  // Fire Resist
            typeof(ColdResistProperty),  // Cold Resist
            typeof(PoisonResistProperty),  // Poison Resist
            typeof(EnergyResistProperty),  // Energy Resist
        };

        public virtual bool CheckRules()
        {
            return true;
        }

        public virtual bool CheckMagicalItem(List<ValuedProperty> props)
        {
            return false;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(SlotLabel);
        }

        public class BaseTransmogrificationPotionGump : Gump
        {
            private readonly BaseTransmogrificationPotion Potion;

            public BaseTransmogrificationPotionGump(BaseTransmogrificationPotion potion)
                : base(100, 100)
            {
                Potion = potion;

                bool isrobe = potion is RobeTransmogrificationPotion;

                AddPage(0);

                AddBackground(0, 0, 370, isrobe ? 520 : 570, 0x6DB);
                AddHtmlLocalized(85, 10, 200, 20, 1114513, "#1159501", 0x67D5, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
                AddItem(160, 50, 0x9D83);
                AddItem(145, 20, 0x376F);
                AddHtmlLocalized(10, 150, 350, isrobe ? 180 : 252, 1114513, string.Format("#{0}", Potion.GumpDesciption), 0x43FF, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
                AddButton(10, isrobe ? 339 : 411, 0x15E1, 0x15E5, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(35, isrobe ? 339 : 411, 150, 20, 1159494, 0x7FFF, false, false); // Set Source Object
                AddButton(185, isrobe ? 339 : 411, 0x15E1, 0x15E5, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(210, isrobe ? 339 : 411, 200, 20, 1159495, 0x7FFF, false, false); // Set Destination Object

                if (Potion.Source != null && Potion.Source.Item != null && !Potion.Source.Item.Deleted)
                {
                    AddItem(50, isrobe ? 375 : 450, Potion.Source.Item.ItemID, Potion.Source.Item.Hue);
                    AddItemProperty(Potion.Source.Item.Serial);
                }

                if (Potion.Destination != null && Potion.Destination.Item != null && !Potion.Destination.Item.Deleted)
                {
                    AddItem(250, isrobe ? 375 : 450, Potion.Destination.Item.ItemID, Potion.Destination.Item.Hue);
                    AddItemProperty(Potion.Destination.Item.Serial);
                }

                AddButton(150, isrobe ? 465 : 515, 0x47B, 0x47C, 3, GumpButtonType.Reply, 0);
                AddHtmlLocalized(137, isrobe ? 445 : 495, 100, 18, 1114513, "#1159497", 0x7E00, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
            }

            private class InternalTarget : Target
            {
                private readonly BaseTransmogrificationPotion Potion;
                private readonly bool IsSource;

                public InternalTarget(BaseTransmogrificationPotion potion, bool source)
                    : base(12, true, TargetFlags.None)
                {
                    Potion = potion;
                    IsSource = source;
                }

                protected override void OnTarget(Mobile from, object targeted)
                {
                    if (!(targeted is Item targetitem) || Potion.Deleted)
                        return;

                    if (!Potion.IsChildOf(from.Backpack) || !targetitem.IsChildOf(from.Backpack))
                    {
                        from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
                        return;
                    }

                    var ip = new ItemProps { Item = targetitem, Props = Mannequin.FindMagicalItemProperty(targetitem) };

                    if (IsSource)
                    {
                        Potion.Source = ip;
                    }
                    else
                    {
                        if (Potion.CheckMagicalItem(ip.Props))
                        {
                            from.SendLocalizedMessage(1159504); // The destination item must be free of any magical properties.
                            return;
                        }
                        else if (targetitem.HasSocket<Transmogrified>())
                        {
                            from.Send(new AsciiMessage(-1, -1, MessageType.Label, 946, 3, "System", "This item has already been transmogrified."));
                        }
                        else
                        {
                            Potion.Destination = ip;
                        }
                    }

                    int message = 0;

                    if (targetitem == null || targetitem.Layer != Potion.ItemLayer)
                    {
                        message = Potion.ValidFailMessage;
                    }
                    else if (Potion.Destination != null && Potion.Destination.Item != null && !Potion.Destination.Item.Deleted &&
                        Potion.Source != null && Potion.Source.Item != null && !Potion.Source.Item.Deleted)
                    {
                        if (Potion.Destination.Item == Potion.Source.Item)
                        {
                            message = 1159518; // You may not set the source and destination objects to the same object!
                        }
                        else if (RaceDefinitions.GetRequiredRace(Potion.Destination.Item) != RaceDefinitions.GetRequiredRace(Potion.Source.Item))
                        {
                            message = 1159560; // You may not set the source and destination objects to objects of different race requirements.
                        }
                        else if (!Potion.CheckRules())
                        {
                            message = Potion.ValidFailMessage;
                        }
                        else if (Potion.Destination.Props.Except(Potion.Source.Props).Any(x => x is MedableArmorProperty))
                        {
                            message = 1159678; // Both source and destination objects must allow the use of the meditation skill (medable) or both block the meditation skill (non-medable).
                        }
                    }

                    if (message == 0)
                    {
                        from.CloseGump(typeof(BaseTransmogrificationPotionGump));
                        from.SendGump(new BaseTransmogrificationPotionGump(Potion));
                    }
                    else
                    {
                        if (IsSource)
                        {
                            Potion.Source = null;
                        }
                        else
                        {
                            Potion.Destination = null;
                        }

                        from.SendLocalizedMessage(message);
                    }                    
                }
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                if (Potion.Deleted)
                    return;

                Mobile m = sender.Mobile;

                switch (info.ButtonID)
                {
                    case 0:
                        break;
                    case 1:
                        m.SendLocalizedMessage(1159498); // Target the object that you wish to transfer properties FROM...
                        Potion.Source = null;
                        m.Target = new InternalTarget(Potion, true);
                        break;
                    case 2:
                        m.SendLocalizedMessage(1159499); // Target the object you wish to transfer properties TO...
                        Potion.Destination = null;
                        m.Target = new InternalTarget(Potion, false);
                        break;
                    case 3:
                        if (!Potion.IsChildOf(m.Backpack))
                        {
                            m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
                            return;
                        }

                        if (Potion.Destination == null || Potion.Destination.Item == null || Potion.Destination.Item.Deleted ||
                        Potion.Source == null || Potion.Source.Item == null || Potion.Source.Item.Deleted)
                        {
                            return;
                        }

                        if (!Potion.Source.Item.IsChildOf(m.Backpack) || !Potion.Destination.Item.IsChildOf(m.Backpack))
                        {
                            Potion.Source = null;
                            m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
                            return;
                        }

                        Potion.Destination.Props = Mannequin.FindMagicalItemProperty(Potion.Destination.Item);

                        if (Potion.CheckMagicalItem(Potion.Destination.Props))
                        {
                            m.SendLocalizedMessage(1159504); // The destination item must be free of any magical properties.
                            return;
                        }

                        if (Potion.Destination.Props.Except(Potion.Source.Props).Any(x => x is MedableArmorProperty))
                        {
                            m.SendLocalizedMessage(1159678); // Both source and destination objects must allow the use of the meditation skill (medable) or both block the meditation skill (non-medable).
                        }

                        m.CloseGump(typeof(BaseTransmogrificationPotionGump));
                        m.SendGump(new BaseTransmogrificationPotionGump(Potion));

                        m.CloseGump(typeof(BaseTransmogrificationPotionConfirmGump));
                        m.SendGump(new BaseTransmogrificationPotionConfirmGump(Potion));

                        break;
                }
            }
        }

        public class BaseTransmogrificationPotionConfirmGump : Gump
        {
            private readonly BaseTransmogrificationPotion Potion;

            public BaseTransmogrificationPotionConfirmGump(BaseTransmogrificationPotion potion)
                : base(100, 100)
            {
                Potion = potion;

                AddPage(0);

                AddBackground(0, 0, 320, 245, 0x6DB);
                AddHtmlLocalized(65, 10, 200, 20, 1114513, "#1159501", 0x67D5, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
                AddHtmlLocalized(15, 50, 295, 140, 1159502, 0x72ED, false, false); // You are about to transmogrify the items you have selected. The source object will be destroyed and the destination object will take on the properties of the source object.  Blessed status will be retained.  Are you sure you wish to proceed?  This process is final and cannot be undone.
                AddButton(30, 200, 0x867, 0x869, 1, GumpButtonType.Reply, 0);
                AddButton(265, 200, 0x867, 0x869, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(33, 180, 100, 50, 1046362, 0x7FFF, false, false); // Yes
                AddHtmlLocalized(273, 180, 100, 50, 1046363, 0x7FFF, false, false); // No
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                Mobile m = sender.Mobile;

                switch (info.ButtonID)
                {
                    case 0:
                    {
                        break;
                    }
                    case 1:
                    {
                        if (!Potion.IsChildOf(m.Backpack))
                        {
                            m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
                            return;
                        }

                        if (Potion.Destination == null || Potion.Destination.Item == null || Potion.Destination.Item.Deleted ||
                        Potion.Source == null || Potion.Source.Item == null || Potion.Source.Item.Deleted)
                        {
                            return;
                        }

                        if (!Potion.Source.Item.IsChildOf(m.Backpack) || !Potion.Destination.Item.IsChildOf(m.Backpack))
                        {
                            Potion.Source = null;
                            m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
                            return;
                        }

                        Potion.Destination.Props = Mannequin.FindMagicalItemProperty(Potion.Destination.Item);

                        if (Potion.CheckMagicalItem(Potion.Destination.Props))
                        {
                            m.SendLocalizedMessage(1159504); // The destination item must be free of any magical properties.
                            return;
                        }

                        if (Potion.Destination.Props.Except(Potion.Source.Props).Any(x => x is MedableArmorProperty))
                        {
                            m.SendLocalizedMessage(1159678); // Both source and destination objects must allow the use of the meditation skill (medable) or both block the meditation skill (non-medable).
                        }

                        m.CloseGump(typeof(BaseTransmogrificationPotionGump));

                        m.PlaySound(491);

                        Potion.Source.Item.AttachSocket(new Transmogrified());

                        var socket = Potion.Source.Item.GetSocket<Transmogrified>();

                        if (socket != null)
                        {
                            socket.SourceName = Potion.Source.Item.Name;
                        }

                        Potion.Source.Item.ItemID = Potion.Destination.Item.ItemID;
                        Potion.Source.Item.Hue = Potion.Destination.Item.Hue;
                        Potion.Source.Item.LootType = Potion.Destination.Item.LootType;
                        Potion.Source.Item.Insured = Potion.Destination.Item.Insured;                        

                        Potion.Destination.Item.Delete();
                        Potion.Delete();

                        break;
                    }
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
