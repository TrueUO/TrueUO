namespace Server
{
	public abstract class SaveStrategy
	{
		public abstract string Name { get; }
		public static SaveStrategy Acquire()
        {
            if (Core.MultiProcessor)
            {
                return new DynamicSaveStrategy();
            }

            return new DynamicSaveStrategy();
        }

		public abstract void Save(bool permitBackgroundWrite);

		public abstract void ProcessDecay();
	}
}
