namespace Server.Misc
{
    public class Animations
    {
        public static void AnimateRequest(Mobile from, string actionName)
        {
            int action;
            switch (actionName)
            {
                case "bow":
                    action = 0;
                    break;
                case "salute":
                    action = 1;
                    break;
                default:
                    return;
            }

            if (from.Alive && !from.Mounted && (from.Body.IsHuman || from.Body.IsGargoyle))
            {
                from.Animate(AnimationType.Emote, action);
            }
        }
    }
}
