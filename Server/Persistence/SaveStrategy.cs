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
            else
            {
                return new StandardSaveStrategy();
            }
        }

        public abstract void Save(SaveMetrics metrics, bool permitBackgroundWrite);

		public abstract void ProcessDecay();
	}
}
