using Server.Gumps;
using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class Veterinarian : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos => m_SBInfos;

        [Constructable]
        public Veterinarian()
            : base("the vet")
        {
            SetSkill(SkillName.AnimalLore, 85.0, 100.0);
            SetSkill(SkillName.Veterinary, 90.0, 100.0);
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBVeterinarian());
        }

        private static readonly Dictionary<Mobile, Timer> _ExpireTable = new Dictionary<Mobile, Timer>();

        public static BaseCreature[] GetDeadPets(Mobile from)
        {
            List<BaseCreature> pets = new List<BaseCreature>();
            IPooledEnumerable eable = from.GetMobilesInRange(12);

            foreach (Mobile m in eable)
            {
                if (m is BaseCreature bc && bc.IsDeadBondedPet && bc.ControlMaster == from && from.InLOS(bc))
                {
                    pets.Add(bc);
                }
            }

            eable.Free();

            if (from.Backpack != null && from.Backpack.FindItemByType(typeof(BrokenAutomatonHead)) is BrokenAutomatonHead head && head.Automaton != null && !head.Automaton.Deleted)
            {
                pets.Add(head.Automaton);
            }

            return pets.ToArray();
        }

        public static int GetResurrectionFee(BaseCreature bc)
        {
            if (bc is KotlAutomaton)
            {
                return 0;
            }

            int fee = (int)(100 + Math.Pow(1.1041, bc.MinTameSkill));

            if (fee > 30000)
            {
                fee = 30000;
            }
            else if (fee < 100)
            {
                fee = 100;
            }

            return fee;
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (InRange(m, 3) && !InRange(oldLocation, 3) && InLOS(m))
            {
                BaseCreature[] pets = GetDeadPets(m);

                if (pets.Length > 0)
                {
                    m.Frozen = true;

                    _ExpireTable[m] = Timer.DelayCall(TimeSpan.FromMinutes(1.0), ResetExpire, m);

                    m.CloseGump(typeof(VetResurrectGump));
                    m.SendGump(new VetResurrectGump(this, pets));
                }
            }
        }

        private static void ResetExpire(Mobile m)
        {
            m.Frozen = false;
            m.CloseGump(typeof(VetResurrectGump));

            if (_ExpireTable.TryGetValue(m, out Timer value))
            {
                if (value != null)
                {
                    value.Stop();
                }

                _ExpireTable.Remove(m);
            }
        }

        public Veterinarian(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }

    public class VetResurrectGump : Gump
    {
        private readonly BaseCreature[] _Pets;

        public VetResurrectGump(Veterinarian vet, BaseCreature[] pets)
            : base(150, 50)
        {
            _Pets = pets;

            AddPage(0);

            Closable = false;

            AddImage(0, 0, 0xE10);
            AddImageTiled(0, 14, 15, 380, 0xE13);
            AddImageTiled(380, 14, 14, 380, 0xE15);
            AddImage(0, 381, 0xE16);
            AddImageTiled(15, 381, 370, 16, 0xE17);
            AddImageTiled(15, 0, 370, 16, 0xE11);
            AddImage(380, 0, 0xE12);
            AddImage(380, 381, 0xE18);
            AddImageTiled(15, 15, 365, 370, 0xA40);

            AddHtmlLocalized(30, 20, 355, 35, 1113193, 0xFFFFFF, false, false); // Ah, thine pet seems to be in dire condition! I can help thee, but must charge a small fee...
            AddHtmlLocalized(30, 72, 345, 40, 1113284, 0x1DB2D, false, false); // Please select the pet you wish to resurrect:
            AddHtmlLocalized(20, 280, 345, 40, 1113286, 0x1DB2D, false, false); // <CENTER>Your pet will suffer 0.2 points of skill-loss if resurrected in this manner.</CENTER>
            AddImageTiled(95, 62, 200, 1, 0x23C5);
            AddImageTiled(15, 325, 365, 1, 0x2393);

            AddButton(110, 343, 0xF7, 0xF8, 1, GumpButtonType.Reply, 0);
            AddButton(230, 343, 0xF2, 0xF1, -1, GumpButtonType.Reply, 0);

            AddImageTiled(15, 14, 365, 1, 0x2393);
            AddImageTiled(380, 14, 1, 370, 0x2391);
            AddImageTiled(15, 385, 365, 1, 0x2393);
            AddImageTiled(15, 14, 1, 370, 0x2391);
            AddImageTiled(0, 0, 395, 1, 0x23C5);
            AddImageTiled(394, 0, 1, 397, 0x23C3);
            AddImageTiled(0, 396, 395, 1, 0x23C5);
            AddImageTiled(0, 0, 1, 397, 0x23C3);

            for (int i = 0, yOffset = 0; i < _Pets.Length; i++, yOffset += 35)
            {
                BaseCreature pet = _Pets[i];

                AddRadio(30, 102 + yOffset, 0x25FF, 0x2602, i == 0, i);
                AddLabel(70, 107 + yOffset, 0x47E, $"{pet.Name}  {Veterinarian.GetResurrectionFee(pet)}");
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            from.Frozen = false;

            switch (info.ButtonID)
            {
                case -1:
                    {
                        // You decide against paying the Veterinarian, and the ghost of your pet looks at you sadly...
                        from.SendLocalizedMessage(1113197);

                        break;
                    }
                case 1:
                    {
                        for (int i = 0; i < _Pets.Length; i++)
                        {
                            BaseCreature pet = _Pets[i];

                            if (info.IsSwitched(i))
                            {
                                int fee = Veterinarian.GetResurrectionFee(pet);

                                if (!pet.IsDeadBondedPet)
                                    from.SendLocalizedMessage(501041); // Target is not dead.
                                else if (!from.CanSee(pet) || !from.InLOS(pet))
                                    from.SendLocalizedMessage(503376); // Target cannot be seen.
                                else if (!from.InRange(pet, 12))
                                    from.SendLocalizedMessage(500643); // Target is too far away.
                                else if (pet.ControlMaster != from)
                                    from.SendLocalizedMessage(1113200); // You must be the owner of that pet to have it resurrected.
                                else if (pet.Corpse != null && !pet.Corpse.Deleted)
                                    from.SendLocalizedMessage(1113279); // That creature's spirit lacks cohesion. Try again in a few minutes.
                                else if (Banker.Withdraw(from, fee))
                                {
                                    pet.PlaySound(0x214);
                                    pet.ResurrectPet();

                                    for (int j = 0; j < pet.Skills.Length; ++j) // Decrease all skills on pet.
                                    {
                                        pet.Skills[j].Base -= 0.2;
                                    }

                                    if (pet.Map == Map.Internal)
                                    {
                                        pet.MoveToWorld(from.Location, from.Map);
                                    }

                                    from.SendLocalizedMessage(1060398, fee.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
                                    from.SendLocalizedMessage(1060022, Banker.GetBalance(from).ToString(), 0x16); // You have ~1_AMOUNT~ gold in cash remaining in your bank box.
                                }
                                else
                                {
                                    from.SendLocalizedMessage(1060020); // Unfortunately, you do not have enough cash in your bank to cover the cost of the healing.
                                }

                                break;
                            }
                        }

                        break;
                    }
            }
        }
    }
}
