using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Services.TownCryer;
using System;

namespace Server.Engines.Quests
{
    public class TreasureHuntingBook : Item
    {
        [Constructable]
        public TreasureHuntingBook()
            : base(0xFBE)
        {
            LootType = LootType.Blessed;
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (m is PlayerMobile mobile && IsChildOf(mobile.Backpack))
            {
                mobile.CloseGump(typeof(InternalGump));
                BaseGump.SendGump(new InternalGump(mobile));
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1158253); // Treasure Hunting: A Practical Approach
            list.Add(1154760, "#1158254"); // By: Vespyr Jones
        }

        public class InternalGump : BaseGump
        {
            public InternalGump(PlayerMobile pm)
                : base(pm, 10, 10)
            {
            }

            public override void AddGumpLayout()
            {
                AddImage(0, 0, 0x761C);
                AddImage(112, 40, 0x655);
                AddHtmlLocalized(113, 350, 342, 280, 1158255, C32216(0x080808), false, true);
            }
        }

        public TreasureHuntingBook(Serial serial)
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

    public class BuriedRichesTreasureMap : TreasureMap
    {
        public TreasureMapChest Chest { get; set; }

        public BuriedRichesTreasureMap(int level)
            : base(level, Map.Trammel)
        {
            LootType = LootType.Blessed;
        }

        public override void Decode(Mobile from)
        {
            if (QuestHelper.HasQuest<TheTreasureChaseQuest>((PlayerMobile)from))
            {
                from.CheckSkill(SkillName.Cartography, 0, 100);
                Decoder = from;

                DisplayTo(from);

                from.SendLocalizedMessage(1158243); // Your time studying Treasure Hunting: A Practical Approach helps you decode the map...
            }
            else
            {
                from.PrivateOverheadMessage(MessageType.Regular, 0x21, 1157850, from.NetState); // *You don't make anything of it.*
                //m.PrivateOverheadMessage(MessageType.Regular, 1154, 1158244, m.NetState); // *You decide to visit the Provisioner at the Adventurer's Supplies in Vesper before trying to decode the map...*
            }
        }

        public override void DisplayTo(Mobile m)
        {
            base.DisplayTo(m);

            m.PlaySound(0x41A);
            m.PrivateOverheadMessage(MessageType.Regular, 1154, 1157722, "Cartography", m.NetState); // *Your proficiency in ~1_SKILL~ reveals more about the item*

            if (m is PlayerMobile mobile && Level == 0)
            {
                mobile.CloseGump(typeof(InternalGump));
                BaseGump.SendGump(new InternalGump(mobile, this));
            }
        }

        public override void OnChestOpened(Mobile from, TreasureMapChest chest)
        {
            if (from is PlayerMobile mobile)
            {
                TheTreasureChaseQuest quest = QuestHelper.GetQuest<TheTreasureChaseQuest>(mobile);

                if (quest != null)
                {
                    if (Level == 0)
                    {
                        TownCryerSystem.CompleteQuest(mobile, 1158239, 1158251, 0x655);
                        /*Your eyes widen as you pry open the old chest and reveal the treasure within! Even this small cache
                         * excites you as the thought of bigger and better treasure looms on the horizon! The map is covered
                         * in ancient runes and marks the location of another treasure hoard. You carefully furl the map and
                         * set off on your next adventure!*/

                        mobile.SendLocalizedMessage(1158245, "", 0x23); // You have found the first zealot treasure! As you dig up the chest a leather bound case appears to contain an additional map. You place it in your backpack for later examination. 
                        chest.DropItem(new BuriedRichesTreasureMap(1));
                    }
                    else
                    {
                        mobile.SendLocalizedMessage(1158246, "", 0x23); // You have found the second zealot treasure! As you dig up the chest a leather bound case appears to contain an additional map. You place it in your backpack for later examination. 
                        quest.CompleteQuest();
                    }
                }
            }
        }

        public override void OnBeginDig(Mobile from)
        {
            if (Completed)
            {
                from.SendLocalizedMessage(503028); // The treasure for this map has already been found.
            }
            else if (Decoder != from)
            {
                from.SendLocalizedMessage(503031); // You did not decode this map and have no clue where to look for the treasure.
            }
            else if (!from.CanBeginAction(typeof(TreasureMap)))
            {
                from.SendLocalizedMessage(503020); // You are already digging treasure.
            }
            else if (from.Map != Facet)
            {
                from.SendLocalizedMessage(1010479); // You seem to be in the right place, but may be on the wrong facet!
            }
            else if (from is PlayerMobile mobile && !QuestHelper.HasQuest<TheTreasureChaseQuest>(mobile))
            {
                mobile.SendLocalizedMessage(1158257); // You must be on the "The Treasure Chase" quest offered via the Town Cryer to dig up this treasure.
            }
            else
            {
                from.SendLocalizedMessage(503033); // Where do you wish to dig?
                from.Target = new DigTarget(this);
            }
        }

        protected override bool HasRequiredSkill(Mobile from)
        {
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (Level == 0)
            {
                list.Add(1158229); // A mysterious treasure map personally given to you by the Legendary Cartographer in Vesper
            }
            else
            {
                list.Add(1158256); // A mysterious treasure map recovered from a treasure hoard
            }
        }

        private class InternalGump : BaseGump
        {
            public BuriedRichesTreasureMap Map { get; set; }

            public InternalGump(PlayerMobile pm, BuriedRichesTreasureMap map)
                : base(pm, 10, 10)
            {
                Map = map;
            }

            public override void AddGumpLayout()
            {
                AddBackground(0, 0, 454, 400, 9380);
                AddHtmlLocalized(177, 53, 235, 20, CenterLoc, "#1158240", C32216(0xA52A2A), false, false); // A Mysterious Treasure Map
                AddHtmlLocalized(177, 80, 235, 40, CenterLoc, Map.Level == 0 ? "#1158241" : "#1158250", C32216(0xA52A2A), false, false); // Given to you by the Master Cartographer

                /*The Cartographer has given you a mysterious treasure map and offered you some tips on how to go about 
                 * recovering the treasure. As the Cartographer leaned in and handed you the furled parchment, she told
                 * you of the origins of this mysterious document. "Legend has it..." she tells you, "this map is the 
                 * lost treasure of an ancient Sosarian Order of Zealots. I'm told over the centuries they would bury 
                 * small portions of their treasure throughout the Britannian countryside in an effort to thwart any 
                 * attempts to recover the hoard in its entirety." Your eyes widen at the thought of a massive treasure
                 * hoard and you can't wait to find it!*/
                AddHtmlLocalized(177, 122, 235, 228, 1158242, true, true);

                AddItem(85, 120, 0x14EB, 0);
            }
        }

        public BuriedRichesTreasureMap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Chest);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Chest = reader.ReadItem() as TreasureMapChest;
        }
    }

    public class TreasureSeekersLockpick : Lockpick
    {
        public override int LabelNumber => 1158258;

        public TreasureSeekersLockpick()
        {
            ItemID = 0x14FD;
        }

        protected override void BeginLockpick(Mobile from, ILockpickable item)
        {
            if (from is PlayerMobile pm && item.Locked && QuestHelper.HasQuest<TheTreasureChaseQuest>(pm) && item is TreasureMapChest chest && chest.TreasureMap is BuriedRichesTreasureMap)
            {
                pm.PlaySound(0x241);

                Timer.DelayCall(TimeSpan.FromMilliseconds(200), () =>
                    {
                        if (item.Locked && from.InRange(chest.GetWorldLocation(), 1))
                        {
                            from.CheckTargetSkill(SkillName.Lockpicking, item, 0, 100);

                            // Success! Pick the lock!
                            from.PrivateOverheadMessage(MessageType.Regular, 1154, 1158252, from.NetState); // *Your recent study of Treasure Hunting helps you pick the lock...*
                            chest.SendLocalizedMessageTo(from, 502076); // The lock quickly yields to your skill.
                            from.PlaySound(0x4A);
                            item.LockPick(from);
                        }
                    });
            }
            else
            {
                base.BeginLockpick(from, item);
            }
        }

        public TreasureSeekersLockpick(Serial serial)
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
