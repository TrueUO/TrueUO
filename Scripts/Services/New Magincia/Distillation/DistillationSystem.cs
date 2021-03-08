using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Engines.Distillation
{
    public enum Group
    {
        WheatBased,
        WaterBased,
        Other
    }

    public enum Liquor
    {
        None,
        Whiskey,
        Bourbon,
        Spirytus,
        Cassis,
        MelonLiquor,
        Mist,
        Akvavit,
        Arak,
        CornWhiskey,
        Brandy
    }

    public class DistillationSystem
    {
        public static readonly TimeSpan MaturationPeriod = TimeSpan.FromHours(48);

        private static readonly List<CraftDefinition> m_CraftDefs = new List<CraftDefinition>();
        public static List<CraftDefinition> CraftDefs => m_CraftDefs;

        private static readonly Dictionary<Mobile, DistillationContext> m_Contexts = new Dictionary<Mobile, DistillationContext>();
        public static Dictionary<Mobile, DistillationContext> Contexts => m_Contexts;

        public static void Initialize()
        {
            // Wheat Based
            m_CraftDefs.Add(new CraftDefinition(Group.WheatBased, Liquor.Whiskey, new[] { typeof(Yeast), typeof(WheatWort) }, new[] { 3, 1 }, MaturationPeriod));
            m_CraftDefs.Add(new CraftDefinition(Group.WheatBased, Liquor.Bourbon, new[] { typeof(Yeast), typeof(WheatWort), typeof(PewterBowlOfCorn) }, new[] { 3, 3, 1 }, MaturationPeriod));
            m_CraftDefs.Add(new CraftDefinition(Group.WheatBased, Liquor.Spirytus, new[] { typeof(Yeast), typeof(WheatWort), typeof(PewterBowlOfPotatos) }, new[] { 3, 1, 1 }, TimeSpan.MinValue));
            m_CraftDefs.Add(new CraftDefinition(Group.WheatBased, Liquor.Cassis, new[] { typeof(Yeast), typeof(WheatWort), typeof(PewterBowlOfPotatos), typeof(TribalBerry) }, new[] { 3, 3, 3, 1 }, MaturationPeriod));
            m_CraftDefs.Add(new CraftDefinition(Group.WheatBased, Liquor.MelonLiquor, new[] { typeof(Yeast), typeof(WheatWort), typeof(PewterBowlOfPotatos), typeof(HoneydewMelon) }, new[] { 3, 3, 3, 1 }, MaturationPeriod));
            m_CraftDefs.Add(new CraftDefinition(Group.WheatBased, Liquor.Mist, new[] { typeof(Yeast), typeof(WheatWort), typeof(JarHoney) }, new[] { 3, 3, 1 }, MaturationPeriod));

            // Water Based
            m_CraftDefs.Add(new CraftDefinition(Group.WaterBased, Liquor.Akvavit, new[] { typeof(Yeast), typeof(Pitcher), typeof(PewterBowlOfPotatos) }, new[] { 1, 3, 1 }, TimeSpan.MinValue));
            m_CraftDefs.Add(new CraftDefinition(Group.WaterBased, Liquor.Arak, new[] { typeof(Yeast), typeof(Pitcher), typeof(Dates) }, new[] { 1, 3, 1 }, MaturationPeriod));
            m_CraftDefs.Add(new CraftDefinition(Group.WaterBased, Liquor.CornWhiskey, new[] { typeof(Yeast), typeof(Pitcher), typeof(PewterBowlOfCorn) }, new[] { 1, 3, 1 }, MaturationPeriod));

            // Other
            m_CraftDefs.Add(new CraftDefinition(Group.Other, Liquor.Brandy, new[] { typeof(Pitcher) }, new[] { 4 }, MaturationPeriod));
        }

        public static void AddContext(Mobile from, DistillationContext context)
        {
            if (from != null)
                m_Contexts[from] = context;
        }

        public static DistillationContext GetContext(Mobile from)
        {
            if (from == null)
                return null;

            if (!m_Contexts.ContainsKey(from))
                m_Contexts[from] = new DistillationContext();

            return m_Contexts[from];
        }

        public static int GetLabel(Liquor liquor, bool strong)
        {
            if (strong)
                return 1150718 + (int)liquor;

            return 1150442 + (int)liquor;
        }

        public static int GetLabel(Group group)
        {
            if (group == Group.Other)
                return 1077435;

            return 1150736 + (int)group;
        }

        public static Liquor GetFirstLiquor(Group group)
        {
            foreach (CraftDefinition def in m_CraftDefs)
            {
                if (def.Group == group)
                    return def.Liquor;
            }

            return Liquor.Whiskey;
        }

        public static CraftDefinition GetFirstDefForGroup(Group group)
        {
            foreach (CraftDefinition def in m_CraftDefs)
            {
                if (def.Group == group)
                    return def;
            }

            return null;
        }

        public static CraftDefinition GetDefinition(Liquor liquor, Group group)
        {
            foreach (CraftDefinition def in m_CraftDefs)
            {
                if (def.Liquor == liquor && def.Group == group)
                    return def;
            }

            return GetFirstDefForGroup(group);
        }

        public static void SendDelayedGump(Mobile from)
        {
            Timer.DelayCall(TimeSpan.FromSeconds(1.5), new TimerStateCallback(SendGump), from);
        }

        public static void SendGump(object o)
        {
            Mobile from = o as Mobile;

            if (from != null)
                from.SendGump(new DistillationGump(from));
        }
    }
}
