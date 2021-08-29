namespace Server
{
	public abstract class SaveStrategy
	{
		public abstract string Name { get; }
		public static SaveStrategy Acquire()
        {
            return new DynamicSaveStrategy();
        }

		public abstract void Save(bool permitBackgroundWrite);

		public abstract void ProcessDecay();
	}
}
