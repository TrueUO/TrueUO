using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Mobiles
{
    public class UntamedPetsCleaning
    {
        public static void Initialize()
        {
            CleanUntamedPets();
            Timer.DelayCall(TimeSpan.FromHours(12.0), TimeSpan.FromHours(12.0), CleanUntamedPets);
        }

        private static void CleanUntamedPets()
        {
            List<Mobile> list = new List<Mobile>();

            foreach (BaseCreature b in World.Mobiles.Values.OfType<BaseCreature>())
            {
                if (b.RemoveOnSave && !b.Controlled && b.ControlMaster == null)
                {
                    list.Add(b);
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].Delete();
            }

            list.Clear();
            list.TrimExcess();
        }
    }
}
