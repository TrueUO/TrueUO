using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    [Flipable(0x4513, 0x4514)]
    public class ChickenCoop : Item, ISecurable, IChopable
    {
        public static readonly int MaxStables = 3;

        public override int LabelNumber => 1112570;  // a chicken coop

        private SecureLevel m_Level;        

        [CommandProperty(AccessLevel.GameMaster)]
        public SecureLevel Level
        {
            get => m_Level;
            set => m_Level = value;
        }

        public List<BaseCreature> m_Stored;

        [Constructable]
        public ChickenCoop()
            : base(0x4513)
        {
            Weight = 20;
            m_Level = SecureLevel.CoOwners;
            m_Stored = new List<BaseCreature>();
        }

        public void OnChop(Mobile from)
        {
            if (CheckAccess(from))
            {
                if (IsLockedDown)
                {
                    from.SendLocalizedMessage(1010019); // Locked down resources cannot be used!
                    return;
                }

                Effects.PlaySound(GetWorldLocation(), Map, 0x3B3);
                from.SendLocalizedMessage(500461); // You destroy the item.

                Delete();
            }
        }

        public override void Delete()
        {
            if (m_Stored != null && m_Stored.Count > 0)
            {
                for (int i = 0; i < m_Stored.Count; i++)
                {
                    m_Stored[i].Delete();
                }

                m_Stored.Clear();
            }

            base.Delete();
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (CheckAccess(from))
            {
                SetSecureLevelEntry.AddTo(from, this, list);

                if (m_Stored.Count < MaxStables)
                    list.Add(new StableEntry(this, from));

                if (m_Stored.Count > 0)
                    list.Add(new ClaimEntry(this, from));
            }
        }

        private class StableEntry : ContextMenuEntry
        {
            private readonly ChickenCoop m_Coop;
            private readonly Mobile m_From;

            public StableEntry(ChickenCoop coop, Mobile from)
                : base(1112556, 12) // Stable a chicken
            {
                m_Coop = coop;
                m_From = from;
            }

            public override void OnClick()
            {
                m_Coop.BeginStable(m_From);
            }
        }

        private class ClaimEntry : ContextMenuEntry
        {
            private readonly ChickenCoop m_Coop;
            private readonly Mobile m_From;

            public ClaimEntry(ChickenCoop coop, Mobile from)
                : base(1112557, 12) // Claim a chicken
            {
                m_Coop = coop;
                m_From = from;
            }

            public override void OnClick()
            {
                m_From.CloseGump(typeof(ClaimListGump));
                m_From.SendGump(new ClaimListGump(m_Coop, m_From));
            }
        }

        public ChickenCoop(Serial serial)
            : base(serial)
        {
        }

        private class ClaimListGump : Gump
        {
            private readonly ChickenCoop m_Coop;
            private readonly Mobile m_From;
            private readonly List<BaseCreature> m_List;

            public ClaimListGump(ChickenCoop coop, Mobile from)
                : base(50, 50)
            {
                m_Coop = coop;
                m_From = from;
                m_List = coop.m_Stored;                

                AddPage(0);

                AddBackground(0, 0, 325, 110, 0x2422);
                AddAlphaRegion(5, 5, 315, 100);

                AddHtmlLocalized(15, 15, 275, 20, 1080333, false, false); // Select a pet to retrieve from the stables:

                for (int i = 0; i < MaxStables; ++i)
                {
                    BaseCreature pet = null;

                    if (m_List.Count > i && m_List[i] != null)
                    {
                        pet = m_List[i];
                    }

                    AddButton(15, 39 + (i * 20), 10006, 10006, i + 1, GumpButtonType.Reply, 0);
                    AddHtml(32, 35 + (i * 20), 275, 18, $"<BASEFONT COLOR=#C0C0EE>{(pet == null || pet.Deleted ? "empty" : pet.Name)}</BASEFONT>", false, false);
                }
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                int index = info.ButtonID - 1;

                if (index >= 0 && index < m_List.Count)
                    m_Coop.EndClaimList(m_From, m_List[index]);
            }
        }

        private class StableTarget : Target
        {
            private readonly ChickenCoop m_Post;

            public StableTarget(ChickenCoop post)
                : base(12, false, TargetFlags.None)
            {
                m_Post = post;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is ChickenLizard || targeted is Chicken || targeted is BattleChickenLizard)
                    m_Post.EndStable(from, (BaseCreature)targeted);
                else if (targeted == from)
                    from.SendLocalizedMessage(502672); // HA HA HA! Sorry, I am not an inn.
                else
                    from.SendLocalizedMessage(1112558); // You may only stable chickens in the chicken coop.
            }
        }

        public void EndClaimList(Mobile from, BaseCreature pet)
        {
            if (Deleted || !from.CheckAlive() || !CanUse(from))
                return;

            if ((from.Followers + pet.ControlSlots) <= from.FollowersMax)
            {
                pet.SetControlMaster(from);

                if (pet.Summoned)
                    pet.SummonMaster = from;

                pet.FollowTarget = from;
                pet.ControlOrder = LastOrderType.Follow;

                pet.MoveToWorld(from.Location, from.Map);

                pet.IsStabled = false;
                pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy

                m_Stored.Remove(pet);

                from.SendLocalizedMessage(1042559); // Here you go... and good day to you!
            }
            else
            {
                from.SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
            }
        }

        public void BeginStable(Mobile from)
        {
            if (Deleted || !from.CheckAlive() || !CanUse(from) || !CheckAccess(from))
                return;

            if (m_Stored.Count >= MaxStables)
            {
                from.SendLocalizedMessage(1114325); // There is no more room in your chicken coop!
            }
            else
            {
                from.Target = new StableTarget(this);
                from.SendLocalizedMessage(1112559); // Which chicken do you wish to stable?
            }
        }

        private static readonly Type[] m_ChickenTypes = {
            typeof(Chicken), typeof(ChickenLizard), typeof(BattleChickenLizard)
        };

        public bool CheckType(Mobile m)
        {
            Type type = m.GetType();

            for (int i = 0; i < m_ChickenTypes.Length; i++)
            {
                if (type == m_ChickenTypes[i])
                    return true;
            }

            return false;
        }

        public void EndStable(Mobile from, BaseCreature pet)
        {
            if (Deleted || !from.CheckAlive() || !CanUse(from) || !CheckAccess(from))
                return;

            if (!pet.Controlled || pet.ControlMaster != from)
            {
                from.SendLocalizedMessage(1042562); // You do not own that pet!
            }
            else if (pet.IsDeadPet)
            {
                from.SendLocalizedMessage(1049668); // Living pets only, please.
            }
            else if (pet.Summoned)
            {
                from.SendLocalizedMessage(502673); // I can not stable summoned creatures.
            }
            else if (pet.Allured)
            {
                from.SendLocalizedMessage(1048053); // You can't stable that!
            }
            else if (pet.Body.IsHuman)
            {
                from.SendLocalizedMessage(502672); // HA HA HA! Sorry, I am not an inn.
            }
            else if (pet.Combatant != null && pet.InRange(pet.Combatant, 12) && pet.Map == pet.Combatant.Map)
            {
                from.SendLocalizedMessage(1042564); // I'm sorry.  Your pet seems to be busy.
            }
            else if (m_Stored.Count >= MaxStables)
            {
                from.SendLocalizedMessage(1114325); // There is no more room in your chicken coop!
            }
            else if (!CheckType(pet))
            {
                from.SendLocalizedMessage(1112558); // You may only stable chickens in the chicken coop.
            }
            else
            {
                pet.ControlTarget = null;
                pet.ControlOrder = LastOrderType.Stay;
                pet.Internalize();

                pet.SetControlMaster(null);
                pet.SummonMaster = null;

                pet.IsStabled = true;

                pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy

                if (!m_Stored.Contains(pet))
                    m_Stored.Add(pet);

                from.SendLocalizedMessage(1049677); // Your pet has been stabled.
            }
        }

        public bool CheckAccess(Mobile m)
        {
            BaseHouse h = BaseHouse.FindHouseAt(this);

            return h != null && h.HasSecureAccess(m, m_Level);
        }

        public bool CanUse(Mobile from)
        {
            var use = IsLockedDown || IsSecure;

            if (!use)
            {
                from.SendLocalizedMessage(1112573); // This must be locked down or secured in order to use it.
            }

            return use;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(3); // version

            writer.Write((int)m_Level);
            writer.Write(m_Stored.Count);

            foreach (var bc in m_Stored)
            {
                writer.Write(bc);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_Level = (SecureLevel)reader.ReadInt();
            int count = reader.ReadInt();

            m_Stored = new List<BaseCreature>();

            if (version < 3)
            {
                for (int i = 0; i < count; i++)
                {
                    reader.ReadMobile();
                    int c = reader.ReadInt();

                    for (int j = 0; j < c; j++)
                    {
                        Mobile chicken = reader.ReadMobile();

                        if (chicken != null && chicken is BaseCreature)
                        {
                            BaseCreature bc = chicken as BaseCreature;
                            bc.IsStabled = true;
                            m_Stored.Add(bc);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    Mobile chicken = reader.ReadMobile();

                    if (chicken != null && chicken is BaseCreature bc)
                    {
                        bc.IsStabled = true;
                        m_Stored.Add(bc);
                    }
                }
            }
        }
    }
}
