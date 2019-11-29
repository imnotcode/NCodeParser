namespace NCodeParser
{
	public class Singleton<T> where T : class, new()
	{
		private static volatile T _Instance;
		private static readonly object SyncRoot = new object();

		public static T Instance
		{
			get
			{
				if (_Instance == null)
				{
					lock (SyncRoot)
					{
						if (_Instance == null)
						{
							_Instance = new T();
						}
					}
				}

				return _Instance;
			}
		}
	}
}
