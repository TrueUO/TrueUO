using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class SeedOfLife : Item
    {
        private static readonly Dictionary<PlayerMobile, DateTime> _SeedUsageList = new Dictionary<PlayerMobile, DateTime>();
        private static readonly TimeSpan _Cooldown = TimeSpan.FromMinutes(10);

        public static void Initialize()
        {
            EventSink.AfterWorldSave += CheckCleanup;
        }

        public override int LabelNumber => 1094937;  // seed of life

        [Constructable]
        public SeedOfLife()
            : base(0x1727)
        {
            Hue = 0x491;
            Weight = 1.0;
            Stackable = true;
        }

        public static void CheckCleanup(AfterWorldSaveEventArgs e)
        {
            DoCleanup();
            ManaDraught.DoCleanup();
        }

        public static void DoCleanup()
        {
            List<PlayerMobile> toRemove = new List<PlayerMobile>();

            foreach (PlayerMobile pm in _SeedUsageList.Keys)
            {
                if (_SeedUsageList[pm] < DateTime.Now + _Cooldown)
                {
                    toRemove.Add(pm);
                }
            }

            foreach (PlayerMobile pm in toRemove)
            {
                _SeedUsageList.Remove(pm);
            }

            toRemove.Clear();
        }

        public SeedOfLife(Serial serial)
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
