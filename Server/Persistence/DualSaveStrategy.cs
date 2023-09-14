using System.Threading;

namespace Server
{
	public sealed class DualSaveStrategy : StandardSaveStrategy
	{
        public override string Name => "Dual";

		public override void Save()
		{
            Thread saveItemsThread = new Thread(SaveItems)
			{
				Name = "Item Save Subset"
			};

			saveItemsThread.Start();

			SaveMobiles();
			SaveGuilds();

			saveItemsThread.Join();
        }
	}
}
