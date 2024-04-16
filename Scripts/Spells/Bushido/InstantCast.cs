namespace Server.Spells
{
    public interface InstantCast
    {
        public abstract bool OnInstantCast(IEntity target);
    }
}
