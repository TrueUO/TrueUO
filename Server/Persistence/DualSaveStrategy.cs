using System.Threading;

namespace Server
{
	public sealed class DualSaveStrategy : StandardSaveStrategy
	{
        public override string Name => "Dual";

		public override void Save(bool permitBackgroundWrite)
		{
			PermitBackgroundWrite = permitBackgroundWrite;

			Thread saveItemsThread = new Thread(SaveItems)
			{
				Name = "Item Save Subset"
			};

			saveItemsThread.Start();

			SaveMobiles();
			SaveGuilds();

			saveItemsThread.Join();

			if (permitBackgroundWrite && UseSequentialWriters)  // If we're permitted to write in the background, but we don't anyways, then notify.
            {
                World.NotifyDiskWriteComplete();
            }
        }
	}
}
