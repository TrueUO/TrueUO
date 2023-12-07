using Server.Services.Virtues;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Items
{
    public class CompassionSage : Item
    {
        public override int LabelNumber => 1153872; // Compassion Sage

        private static readonly string _FilePath = Path.Combine("Saves/Misc", "CompassionSage.bin");

        private static readonly Dictionary<Mobile, DateTime> _Table = new Dictionary<Mobile, DateTime>();

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;
        }

        [Constructable]
        public CompassionSage()
            : base(0x1844)
        {
            Hue = 66;
        }

        public CompassionSage(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (_Table.TryGetValue(from, out DateTime value) && value > DateTime.UtcNow)
            {
                from.SendLocalizedMessage(1053004); // You must wait about a day before you can gain in compassion again.
                return;
            }

            _Table.Remove(from);

            bool gainedPath = false;

            if (VirtueHelper.Award(from, VirtueName.Compassion, 1000, ref gainedPath))
            {
                from.SendLocalizedMessage(1053002); // You have gained in compassion.

                if (gainedPath)
                {
                    from.SendLocalizedMessage(1053005); // You have achieved a path in compassion!                    
                }

                _Table[from] = DateTime.UtcNow + TimeSpan.FromDays(1.0);

                Consume();
            }
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                _FilePath,
                writer =>
                {
                    writer.Write(0);

                    writer.Write(_Table.Count);

                    foreach (var l in _Table)
                    {
                        writer.Write(l.Key);
                        writer.Write(l.Value);
                    }
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                _FilePath,
                reader =>
                {
                    int version = reader.ReadInt();
                    int count = reader.ReadInt();

                    for (int i = count; i > 0; i--)
                    {
                        Mobile m = reader.ReadMobile();
                        DateTime dt = reader.ReadDateTime();

                        if (m != null && dt > DateTime.UtcNow)
                        {
                            _Table[m] = dt;
                        }
                    }
                });
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
