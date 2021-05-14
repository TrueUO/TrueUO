using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class SalvageBag : Bag
    {
        private bool m_Failure;

        public override int LabelNumber => 1079931;// Salvage Bag

        [Constructable]
        public SalvageBag()
            : this(Utility.RandomBlueHue())
        {
        }

        [Constructable]
        public SalvageBag(int hue)
        {
            Weight = 2.0;
            Hue = hue;
            m_Failure = false;
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive)
            {
                list.Add(new SalvageIngotsEntry(this, IsChildOf(from.Backpack) && Resmeltables()));
                list.Add(new SalvageClothEntry(this, IsChildOf(from.Backpack) && Scissorables()));
                list.Add(new SalvageAllEntry(this, IsChildOf(from.Backpack) && Resmeltables() && Scissorables()));
            }
        }

        private bool Resmeltables() //Where context menu checks for metal items and dragon barding deeds
        {
            for (var index = 0; index < Items.Count; index++)
            {
                Item i = Items[index];

                if (i != null && !i.Deleted)
                {
                    if (i is BaseWeapon weapon)
                    {
                        if (CraftResources.GetType(weapon.Resource) == CraftResourceType.Metal)
                        {
                            return true;
                        }
                    }

                    if (i is BaseArmor armor)
                    {
                        if (CraftResources.GetType(armor.Resource) == CraftResourceType.Metal)
                        {
                            return true;
                        }
                    }

                    if (i is DragonBardingDeed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool Scissorables() //Where context menu checks for Leather items and cloth items
        {
            for (var index = 0; index < Items.Count; index++)
            {
                var i = Items[index];

                if (i != null && !i.Deleted && i is IScissorable)
                {
                    return true;
                }
            }

            return false;
        }

        private bool Resmelt(Mobile from, Item item, CraftResource resource)
        {
            try
            {
                if (CraftResources.GetType(resource) != CraftResourceType.Metal)
                {
                    return false;
                }

                CraftResourceInfo info = CraftResources.GetInfo(resource);

                if (info == null || info.ResourceTypes.Length == 0)
                {
                    return false;
                }

                CraftItem craftItem = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(item.GetType());

                if (craftItem == null || craftItem.Resources.Count == 0)
                {
                    return false;
                }

                CraftRes craftResource = craftItem.Resources.GetAt(0);

                if (craftResource.Amount < 2)
                {
                    return false; // Not enough metal to resmelt
                }

                double difficulty = 0.0;

                switch (resource)
                {
                    case CraftResource.DullCopper:
                        difficulty = 65.0;
                        break;
                    case CraftResource.ShadowIron:
                        difficulty = 70.0;
                        break;
                    case CraftResource.Copper:
                        difficulty = 75.0;
                        break;
                    case CraftResource.Bronze:
                        difficulty = 80.0;
                        break;
                    case CraftResource.Gold:
                        difficulty = 85.0;
                        break;
                    case CraftResource.Agapite:
                        difficulty = 90.0;
                        break;
                    case CraftResource.Verite:
                        difficulty = 95.0;
                        break;
                    case CraftResource.Valorite:
                        difficulty = 99.0;
                        break;
                }

                Type resourceType = info.ResourceTypes[0];
                Item ingot = (Item)Activator.CreateInstance(resourceType);

                double skill = Math.Max(from.Skills[SkillName.Mining].Value, from.Skills[SkillName.Blacksmith].Value);

                if (item is DragonBardingDeed || item is BaseArmor armor && armor.PlayerConstructed || item is BaseWeapon weapon && weapon.PlayerConstructed || item is BaseClothing clothing && clothing.PlayerConstructed)
                {
                    if (skill > 100.0)
                        skill = 100.0;

                    double amount = (((4 + skill) * craftResource.Amount - 4) * 0.0068);

                    if (amount < 2)
                        ingot.Amount = 2;
                    else
                        ingot.Amount = (int)amount;
                }
                else
                {
                    ingot.Amount = 2;
                }

                if (difficulty > skill)
                {
                    m_Failure = true;
                    ingot.Delete();
                }
                else
                    item.Delete();

                from.AddToBackpack(ingot);

                from.PlaySound(0x2A);
                from.PlaySound(0x240);

                return true;
            }
            catch (Exception e)
            {
                Diagnostics.ExceptionLogging.LogException(e);
            }

            return false;
        }

        private void SalvageIngots(Mobile from)
        {
            bool toolFound = false;

            for (var index = 0; index < from.Backpack.Items.Count; index++)
            {
                var i = from.Backpack.Items[index];

                if (i is ITool tool && tool.CraftSystem == DefBlacksmithy.CraftSystem)
                {
                    toolFound = true;
                    break;
                }
            }

            if (!toolFound)
            {
                from.SendLocalizedMessage(1079822); // You need a blacksmithing tool in order to salvage ingots.
                return;
            }

            bool anvil, forge;

            DefBlacksmithy.CheckAnvilAndForge(from, 2, out anvil, out forge);

            if (!forge)
            {
                from.SendLocalizedMessage(1044265); // You must be near a forge.
                return;
            }

            int salvaged = 0;
            int notSalvaged = 0;

            Container sBag = this;

            List<Item> Smeltables = sBag.FindItemsByType<Item>();

            for (int i = Smeltables.Count - 1; i >= 0; i--)
            {
                Item item = Smeltables[i];

                if (item is BaseArmor armor)
                {
                    if (Resmelt(from, armor, armor.Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
                else if (item is BaseWeapon weapon)
                {
                    if (Resmelt(from, weapon, weapon.Resource))
                        salvaged++;
                    else
                        notSalvaged++;
                }
                else if (item is DragonBardingDeed deed)
                {
                    if (Resmelt(from, deed, deed.Resource))
                        salvaged++;

                    else
                        notSalvaged++;
                }
            }
            if (m_Failure)
            {
                from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
                m_Failure = false;
            }
            else
            {
                from.SendLocalizedMessage(1079973, string.Format("{0}\t{1}", salvaged, salvaged + notSalvaged)); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items
            }
        }

        private void SalvageCloth(Mobile from)
        {
            if (!(from.Backpack.FindItemByType(typeof(Scissors)) is Scissors scissors))
            {
                from.SendLocalizedMessage(1079823); // You need scissors in order to salvage cloth.
                return;
            }

            int salvaged = 0;
            int notSalvaged = 0;

            Container sBag = this;

            List<Item> Scissorables = sBag.FindItemsByType<Item>();

            for (int i = Scissorables.Count - 1; i >= 0; i--)
            {
                Item item = Scissorables[i];

                if (item is IScissorable scissorable)
                {
                    if (scissorable.Scissor(from, scissors))
                    {
                        salvaged++;
                    }
                    else
                    {
                        notSalvaged++;
                    }
                }
            }

            from.SendLocalizedMessage(1079974, $"{salvaged}\t{salvaged + notSalvaged}"); // Salvaged: ~1_COUNT~/~2_NUM~ tailored items

            var findItems = FindItemsByType(typeof(Item), true);

            for (var index = 0; index < findItems.Length; index++)
            {
                Item i = findItems[index];

                if (i is Leather || i is Cloth || i is SpinedLeather || i is HornedLeather || i is BarbedLeather || i is Bandage || i is Bone)
                {
                    from.AddToBackpack(i);
                }
            }
        }

        private void SalvageAll(Mobile from)
        {
            SalvageIngots(from);

            SalvageCloth(from);
        }

        private class SalvageAllEntry : ContextMenuEntry
        {
            private readonly SalvageBag m_Bag;

            public SalvageAllEntry(SalvageBag bag, bool enabled)
                : base(6276)
            {
                m_Bag = bag;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                    return;

                Mobile from = Owner.From;

                if (from.CheckAlive())
                    m_Bag.SalvageAll(from);
            }
        }

        private class SalvageIngotsEntry : ContextMenuEntry
        {
            private readonly SalvageBag m_Bag;

            public SalvageIngotsEntry(SalvageBag bag, bool enabled)
                : base(6277)
            {
                m_Bag = bag;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                    return;

                Mobile from = Owner.From;

                if (from.CheckAlive())
                    m_Bag.SalvageIngots(from);
            }
        }

        private class SalvageClothEntry : ContextMenuEntry
        {
            private readonly SalvageBag m_Bag;

            public SalvageClothEntry(SalvageBag bag, bool enabled)
                : base(6278)
            {
                m_Bag = bag;

                if (!enabled)
                    Flags |= CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Bag.Deleted)
                    return;

                Mobile from = Owner.From;

                if (from.CheckAlive())
                    m_Bag.SalvageCloth(from);
            }
        }

        public SalvageBag(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadEncodedInt();
        }
    }
}
