namespace Server
{
    public interface ISaveStrategy
    {
        void ProcessDecay();
        void Save();
    }
}