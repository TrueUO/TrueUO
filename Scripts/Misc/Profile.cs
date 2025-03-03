using System;
using Server.Accounting;
using Server.Network.Packets;

namespace Server.Misc
{
    public class Profile
    {
        public static void ChangeProfileRequest(Mobile beholder, Mobile beheld, string text)
        {
            if (beholder != beheld && beholder.AccessLevel <= beheld.AccessLevel)
            {
                beholder.SendMessage("You do not have permission to do that.");
                return;
            }

            Mobile from = beholder;

            if (from.ProfileLocked)
            {
                from.SendMessage("Your profile is locked. You may not change it.");
            }
            else
            {
                from.Profile = text;
            }
        }

        public static void ProfileRequest(Mobile beholder, Mobile beheld)
        {
            if (!beheld.Player)
            {
                return;
            }

            if (beholder.Map != beheld.Map || !beholder.InRange(beheld, 12) || !beholder.CanSee(beheld))
            {
                return;
            }

            string header = Titles.ComputeTitle(beholder, beheld);

            string footer = "";

            if (beheld.ProfileLocked)
            {
                if (beholder == beheld)
                {
                    footer = "Your profile has been locked.";
                }
                else if (beholder.IsStaff())
                {
                    footer = "This profile has been locked.";
                }
            }

            if (footer.Length == 0 && beholder == beheld)
            {
                footer = GetAccountDuration(beheld);
            }

            string body = beheld.Profile;

            if (body == null || body.Length <= 0)
            {
                body = "";
            }

            beholder.Send(new DisplayProfilePacket(beholder != beheld || !beheld.ProfileLocked, beheld, header, body, footer));
        }

        public static bool Format(double value, string format, out string op)
        {
            if (value >= 1.0)
            {
                op = string.Format(format, (int)value, (int)value != 1 ? "s" : "");
                return true;
            }

            op = null;
            return false;
        }

        private static string GetAccountDuration(Mobile m)
        {
            Account a = m.Account as Account;

            if (a == null)
                return "";

            TimeSpan ts = DateTime.UtcNow - a.Created;

            string v;

            if (Format(ts.TotalDays, "This account is {0} day{1} old.", out v))
                return v;

            if (Format(ts.TotalHours, "This account is {0} hour{1} old.", out v))
                return v;

            if (Format(ts.TotalMinutes, "This account is {0} minute{1} old.", out v))
                return v;

            if (Format(ts.TotalSeconds, "This account is {0} second{1} old.", out v))
                return v;

            return "";
        }
    }
}
