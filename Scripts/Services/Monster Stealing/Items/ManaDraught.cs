using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class ManaDraught : Item
    {
        private static readonly Dictionary<PlayerMobile, DateTime> _DaughtUsageList = new Dictionary<PlayerMobile, DateTime>();
        private static readonly TimeSpan _Cooldown = TimeSpan.FromMinutes(10);

        public override int LabelNumber => 1094938;  // Mana Draught

        [Constructable]
        public ManaDraught()
            : base(0xFFB)
        {
            Hue = 0x48A;
            Weight = 1.0;
        }

        public static void DoCleanup()
        {
            List<PlayerMobile> toRemove = new List<PlayerMobile>();

            foreach (PlayerMobile pm in _DaughtUsageList.Keys)
            {
                if (_DaughtUsageList[pm] < DateTime.Now + _Cooldown)
                {
                    toRemove.Add(pm);
                }
            }

            for (var index = 0; index < toRemove.Count; index++)
            {
                PlayerMobile pm = toRemove[index];

                _DaughtUsageList.Remove(pm);
            }

            toRemove.Clear();
        }

        public ManaDraught(Serial serial)
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
}
