using Server.Engines.JollyRoger;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class RewardArray
    {
        public Mobile Mobile { get; set; }
        public List<ShrineArray> Shrine { get; set; }
        public bool Tabard { get; set; }
        public bool Cloak { get; set; }

        public RewardArray(Mobile m, List<ShrineArray> s)
        {
            Mobile = m;
            Shrine = s;
        }
    }

    public class ShrineArray
    {
        public Shrine Shrine { get; set; }
        public int MasterDeath { get; set; }

        public ShrineArray(Shrine s, int c)
        {
            Shrine = s;
            MasterDeath = c;
        }
    }

    public class WOSAnkhOfSacrifice : BaseAddon
    {
        public static List<RewardArray> _List = new List<RewardArray>();

        [Constructable]
        public WOSAnkhOfSacrifice()
            : base()
        {
            AddComponent(new AnkhOfSacrificeComponent(0x1E5D), 0, 0, 0);
            AddComponent(new AnkhOfSacrificeComponent(0x1E5C), 1, 0, 0);
        }

        public WOSAnkhOfSacrifice(Serial serial)
            : base(serial)
        {
        }

        public override bool HandlesOnMovement => true;

        public static void AddReward(Mobile m, Shrine shrine)
        {
            if (_List.Any(x => x.Mobile == m))
            {
                if (_List.Any(x => x.Shrine.Any(y => y.Shrine == shrine)))
                {
                    _List.Find(x => x.Mobile == m).Shrine.Find(y => y.Shrine == shrine).MasterDeath++;
                }
                else
                {
                    _List.Where(x=> x.Mobile == m).FirstOrDefault().Shrine.Add(new ShrineArray(shrine, 1));
                }
            }
            else
            {
                var sa = new List<ShrineArray>
                {
                    new ShrineArray(shrine, 1)
                };

                var ra = new RewardArray(m, sa);


                _List.Add(ra);
            }
        }

        public static void RemoveReward(Mobile m, Shrine shrine)
        {
            
        }


        public override void OnComponentUsed(AddonComponent component, Mobile from)
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
            int version = reader.ReadEncodedInt();

        }
    }
}
