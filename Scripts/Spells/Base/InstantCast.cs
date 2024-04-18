namespace Server.Spells.Base
{
    public interface InstantCast
    {
        public abstract bool OnInstantCast(IEntity target);
    }
}
