using Server.Items;
using System;

namespace Server.Misc
{
    public class AnniversaryGiver : GiftGiver
    {
        public static void Initialize()
        {
            GiftGiving.Register(new AnniversaryGiver());
        }

        public override DateTime Start => new DateTime(2021, 10, 15); // When do you want your gift hand out?
        public override DateTime Finish => new DateTime(2021, 11, 15); // When do you want your gift hand out to stop?
        public override TimeSpan MinimumAge => TimeSpan.FromDays(30); // How old does a character have to be to obtain?

        public override void GiveGift(Mobile mob)
        {
            Item token = new Anniversary24GiftToken(); // Here you select your Anniversary Gift Pacakage.

            switch (GiveGift(mob, token))
            {
                case GiftResult.Backpack:
                    mob.SendLocalizedMessage(1159512); // Happy Anniversary! We have placed a gift for you in your backpack.
                    break;
                case GiftResult.BankBox:
                    mob.SendLocalizedMessage(1159513); // Happy Anniversary! We have placed a gift for you in your bank box. 
                    break;
            }
        }
    }
}
