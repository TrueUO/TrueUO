namespace Server
{
    public interface ISaveStrategy
    {
        void ProcessDecay();
        bool Save();
    }
}
