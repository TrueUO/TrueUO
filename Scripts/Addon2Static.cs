#region

using System.Collections.Generic;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;

#endregion

namespace Server.Commands
{
    public class Addon2Static
    {
        public static void Initialize()
        {
            CommandSystem.Register("Addon2Static", AccessLevel.Owner, Addon2Static_OnCommand);
            CommandSystem.Register("A2S", AccessLevel.Owner, Addon2Static_OnCommand);
        }

        [Usage("Addon2Static")]
        [Aliases("A2S")]
        [Description("Statify an Addon structure")]
        private static void Addon2Static_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Please select the Addon structure you want to statify");
            e.Mobile.SendMessage("or target yourself to execute the command globally.");
            e.Mobile.Target = new AddonSelector();
        }

        ///// //// /// // / BEGIN TARGET / // /// //// /////
        private class AddonSelector : Target
        {
            public AddonSelector()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is AddonComponent)
                {
                    var design = ((AddonComponent)targeted).Addon;
                    if (design.Components.Count > 0)
                    {
                        for (var i = 0; i < design.Components.Count; ++i)
                        {
                            var component = design.Components[i];
                            var equivalent = new Static(component.HuedItemID);  //( component.ItemID );
                            equivalent.Location = component.Location;  //component.Location;
                            equivalent.Map = component.Map; //component.Map;
                            equivalent.Hue = component.Hue; //component.Map;							
                        }
                    }

                    design.Delete();
                    from.SendMessage("Addon structure statified. You can now freeze it.");
                }
                else if (targeted == from)
                {
                    from.SendGump(new Addon2StaticGump(from));
                }
            }
        }
        ///// //// /// // / END TARGET / // /// //// /////
    }

    public class Addon2StaticGump : Gump
    {
        public Mobile m_From;
        public List<BaseAddon> feluccaAddons = new List<BaseAddon>();
        public List<BaseAddon> trammelAddons = new List<BaseAddon>();
        public List<BaseAddon> malasAddons = new List<BaseAddon>();
        public List<BaseAddon> ilshenarAddons = new List<BaseAddon>();
        public List<BaseAddon> tokunoAddons = new List<BaseAddon>();
        public List<BaseAddon> termurAddons = new List<BaseAddon>();
        public int addonsFel;
        public int addonsTra;
        public int addonsMal;
        public int addonsIls;
        public int addonsTok;
        public int addonsTer;
        public int compsFel;
        public int compsTra;
        public int compsMal;
        public int compsIls;
        public int compsTok;
        public int compsTer;

        public Addon2StaticGump(Mobile from)
            : base(0, 0)
        {

            m_From = from;
            Closable = true;
            Dragable = true;

            AddPage(1);

            AddBackground(0, 0, 455, 260, 5054);
            AddLabel(30, 2, 200, "Select Facets to Convert");

            AddImageTiled(10, 20, 425, 210, 3004);

            foreach (object o in World.Items.Values)
            {
                if (o is AddonComponent)
                {
                    var design = ((AddonComponent)o).Addon;
                    if (design == null || design.Map == null)
                    {
                        continue;
                    }

                    if (design.Map == Map.Felucca)
                    {
                        if (design.Components.Count > 0)
                        {
                            feluccaAddons.Add(design);
                            addonsFel++;
                            for (var i = 0; i < design.Components.Count; ++i)
                            {
                                compsFel++;
                            }
                        }
                    }
                    else if (design.Map == Map.Trammel)
                    {
                        if (design.Components.Count > 0)
                        {
                            trammelAddons.Add(design);
                            addonsTra++;
                            for (var i = 0; i < design.Components.Count; ++i)
                            {
                                compsTra++;
                            }
                        }
                    }
                    else if (design.Map == Map.Malas)
                    {
                        if (design.Components.Count > 0)
                        {
                            malasAddons.Add(design);
                            addonsMal++;
                            for (var i = 0; i < design.Components.Count; ++i)
                            {
                                compsMal++;
                            }
                        }
                    }
                    else if (design.Map == Map.Ilshenar)
                    {
                        if (design.Components.Count > 0)
                        {
                            ilshenarAddons.Add(design);
                            addonsIls++;
                            for (var i = 0; i < design.Components.Count; ++i)
                            {
                                compsIls++;
                            }
                        }
                    }
                    else if (design.Map == Map.Tokuno)
                    {
                        if (design.Components.Count > 0)
                        {
                            tokunoAddons.Add(design);
                            addonsTok++;
                            for (var i = 0; i < design.Components.Count; ++i)
                            {
                                compsTok++;
                            }
                        }
                    }
                    else if (design.Map == Map.TerMur)
                    {
                        if (design.Components.Count > 0)
                        {
                            termurAddons.Add(design);
                            addonsTer++;
                            for (var i = 0; i < design.Components.Count; ++i)
                            {
                                compsTer++;
                            }
                        }
                    }
                }
            }

            AddLabel(40, 26, 200, string.Format("Felucca - {0} Addons, with {1} Components to process.", addonsFel, compsFel));
            AddLabel(40, 51, 200, string.Format("Trammel - {0} Addons, with {1} Components to process.", addonsTra, compsTra));
            AddLabel(40, 76, 200, string.Format("Malas - {0} Addons, with {1} Components to process.", addonsMal, compsMal));
            AddLabel(40, 101, 200, string.Format("Ilshenar - {0} Addons, with {1} Components to process.", addonsIls, compsIls));
            AddLabel(40, 126, 200, string.Format("Tokuno - {0} Addons, with {1} Components to process.", addonsTok, compsTok));
            AddLabel(40, 151, 200, string.Format("TerMur - {0} Addons, with {1} Components to process.", addonsTer, compsTer));

            AddCheck(20, 23, 210, 211, true, 101);
            AddCheck(20, 48, 210, 211, true, 102);
            AddCheck(20, 73, 210, 211, true, 103);
            AddCheck(20, 98, 210, 211, true, 104);
            AddCheck(20, 123, 210, 211, true, 105);
            AddCheck(20, 148, 210, 211, true, 106);

            AddButton(30, 234, 247, 249, 1, GumpButtonType.Reply, 0);
            AddButton(100, 234, 241, 243, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var felCheck = info.IsSwitched(101);
            var traCheck = info.IsSwitched(102);
            var malCheck = info.IsSwitched(103);
            var ilsCheck = info.IsSwitched(104);
            var tokCheck = info.IsSwitched(105);
            var terCheck = info.IsSwitched(106);
            var localCount = 0;
            switch (info.ButtonID)
            {
                case 1:
                    {
                        if (felCheck && addonsFel > 0)
                        {
                            for (var i = 0; i < addonsFel; i++)
                            {
                                Convert2Static(feluccaAddons[i]);
                            }

                            m_From.SendMessage("{0} Addons in Felucca processed, and {1} Components converted to Statics.",
                                addonsFel, compsFel);
                            localCount += addonsFel;
                        }
                        if (traCheck && addonsTra > 0)
                        {
                            for (var i = 0; i < addonsTra; i++)
                            {
                                Convert2Static(trammelAddons[i]);
                            }

                            m_From.SendMessage("{0} Addons in Trammel processed, and {1} Components converted to Statics.",
                                addonsTra, compsTra);
                            localCount += addonsTra;
                        }
                        if (malCheck && addonsMal > 0)
                        {
                            for (var i = 0; i < addonsMal; i++)
                            {
                                Convert2Static(malasAddons[i]);
                            }

                            m_From.SendMessage("{0} Addons in Malas processed, and {1} Components converted to Statics.",
                                addonsMal, compsMal);
                            localCount += addonsMal;
                        }
                        if (ilsCheck && addonsIls > 0)
                        {
                            for (var i = 0; i < addonsIls; i++)
                            {
                                Convert2Static(ilshenarAddons[i]);
                            }

                            m_From.SendMessage("{0} Addons in Ilshenar processed, and {1} Components converted to Statics.",
                                addonsIls, compsIls);
                            localCount += addonsIls;
                        }
                        if (tokCheck && addonsTok > 0)
                        {
                            for (var i = 0; i < addonsTok; i++)
                            {
                                Convert2Static(tokunoAddons[i]);
                            }

                            m_From.SendMessage("{0} Addons in Tokuno processed, and {1} Components converted to Statics.",
                                addonsTok, compsTok);
                            localCount += addonsTok;
                        }
                        if (terCheck && addonsTer > 0)
                        {
                            for (var i = 0; i < addonsTer; i++)
                            {
                                Convert2Static(termurAddons[i]);
                            }

                            m_From.SendMessage("{0} Addons in TerMur processed, and {1} Components converted to Statics.",
                                addonsTer, compsTer);
                            localCount += addonsTer;
                        }
                        if (localCount > 0)
                        {
                            m_From.SendMessage("{0} total Addons converted.", localCount);
                        }
                        else
                        {
                            m_From.SendMessage("No addons were converted.");
                        }

                        break;
                    }
            }
        }

        public void Convert2Static(BaseAddon design)
        {
            if (design.Components.Count > 0)
            {
                for (var i = 0; i < design.Components.Count; ++i)
                {
                    var component = design.Components[i];
                    var equivalent = new Static(component.HuedItemID);  //( component.ItemID );
                    equivalent.Location = component.Location;  //component.Location;
                    equivalent.Map = component.Map; //component.Map;
                    equivalent.Hue = component.Hue; //component.Map;							
                }
            }

            design.Delete();
        }
    }
}
