namespace Server.Misc
{
    public class LoginStats
    {
        public static void Initialize()
        {
            // Register our event handler
            EventSink.Login += EventSink_Login;
        }

        private static void EventSink_Login(LoginEventArgs args)
        {
            Mobile m = args.Mobile;

            if (m.IsStaff())
            {
                Engines.Help.PageQueue.Pages_OnCalled(m);
            }
        }
    }
}
