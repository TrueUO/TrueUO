using Server.Mobiles;

namespace Server.Misc
{
    public class RenameRequests
    {
        public static void RenameRequest(Mobile from, Mobile targ, string name)
        {
            if (from.CanSee(targ) && from.InRange(targ, 12) && targ.CanBeRenamedBy(from))
            {
                name = name.Trim();

                int numExceptions = 0;
                char[] exceptions = NameVerification.Empty;

                if (targ is BaseCreature)
                {
                    exceptions = new[] { ' ' };
                    numExceptions = 5;
                }

                if (NameVerification.Validate(name, 1, 16, true, false, true, numExceptions, exceptions, NameVerification.StartDisallowed, NameVerification.Disallowed))
                {
                    string[] disallowed = ProfanityProtection.Disallowed;

                    for (int i = 0; i < disallowed.Length; i++)
                    {
                        if (name.IndexOf(disallowed[i]) != -1)
                        {
                            from.SendLocalizedMessage(1072622); // That name isn't very polite.
                            return;
                        }
                    }

                    from.SendLocalizedMessage(1072623, $"{targ.Name}\t{name}"); // Pet ~1_OLDPETNAME~ renamed to ~2_NEWPETNAME~.

                    targ.Name = name;
                }
                else
                {
                    from.SendMessage("That name is unacceptable.");
                }
            }
        }
    }
}
