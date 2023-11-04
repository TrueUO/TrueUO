namespace Server
{
	public abstract class SaveStrategy
	{
		public abstract string Name { get; }
		public static SaveStrategy Acquire()
        {
            if (Core.MultiProcessor)
			{
                return new DualSaveStrategy();
            }

            return new StandardSaveStrategy();
        }

		public abstract void Save();

		public abstract void ProcessDecay();
	}
}
