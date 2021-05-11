using Server.Items;
using Server.Mobiles;
using Server.Engines.RisingTide;

using System;

namespace Server.Engines.Points
{
    public class RisingTide : PointsSystem
    {
        public override PointsType Loyalty => PointsType.RisingTide;
        public override TextDefinition Name => m_Name;
        public override bool AutoAdd => true;
        public override double MaxPoints => double.MaxValue;
        public override bool ShowOnLoyaltyGump => false;

        private readonly TextDefinition m_Name = null;

        public static readonly double CargoChance = 0.1;

        public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
        {
            from.SendLocalizedMessage(1158910, ((int)points).ToString()); // You have ~1_COUNT~ doubloons!
        }

        public override void ProcessKill(Mobile victim, Mobile damager)
        {
            if (RisingTideEvent.Instance.Running && victim is BaseCreature bc && damager is PlayerMobile)
            {
                PlunderBeaconAddon beacon = GetPlunderBeacon(bc);

                if (beacon != null)
                {
                    if (CargoChance > Utility.RandomDouble())
                    {
                        damager.AddToBackpack(new MaritimeCargo());
                        damager.SendLocalizedMessage(1158907); // You recover maritime trade cargo!
                    }
                }
                else
                    for (var index = 0; index < CargoDropsTypes.Length; index++)
                    {
                        var type = CargoDropsTypes[index];

                        if (type == bc.GetType())
                        {
                            double chance = CargoChance;

                            if (bc is BaseShipCaptain)
                            {
                                chance = 0.33;
                            }

                            if (chance > Utility.RandomDouble())
                            {
                                Container corpse = bc.Corpse;

                                corpse?.DropItem(new MaritimeCargo());
                            }

                            break;
                        }
                    }
            }
        }

        private readonly Type[] CargoDropsTypes =
        {
            typeof(PirateCaptain), typeof(MerchantCaptain), typeof(PirateCrew), typeof(MerchantCrew)
        };

        public static PlunderBeaconAddon GetPlunderBeacon(BaseCreature bc)
        {
            if (PlunderBeaconSpawner.Spawner != null)
            {
                foreach (System.Collections.Generic.List<PlunderBeaconAddon> list in PlunderBeaconSpawner.Spawner.PlunderBeacons.Values)
                {
                    PlunderBeaconAddon addon = null;

                    for (var index = 0; index < list.Count; index++)
                    {
                        var beacon = list[index];

                        if (beacon.Crew.Contains(bc) || beacon.Spawn.ContainsKey(bc) && beacon.Spawn[bc])
                        {
                            addon = beacon;
                            break;
                        }
                    }

                    if (addon != null)
                    {
                        return addon;
                    }
                }
            }

            return null;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            if (PlunderBeaconSpawner.Spawner != null)
            {
                writer.Write(0);
                PlunderBeaconSpawner.Spawner.Serialize(writer);
            }
            else
            {
                writer.Write(1);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                reader.ReadBool();
            }

            if (reader.ReadInt() == 0)
            {
                var spawner = new PlunderBeaconSpawner();
                PlunderBeaconSpawner.Spawner = spawner;

                spawner.Deserialize(reader);
            }
        }
    }
}
