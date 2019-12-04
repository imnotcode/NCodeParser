using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NCodeParser
{
	public static class Extensions
	{
		public static void AddAll<T>(this IList<T> list, IList<T> collection)
		{
			if (list == null || collection == null)
			{
				return;
			}

			for (int i = 0; i < collection.Count; i++)
			{
				list.Add(collection[i]);
			}
		}

		public static T GetWindow<T>(this Application app) where T : Window
		{
			if (app == null)
			{
				return default;
			}

			return app.Windows.OfType<T>().FirstOrDefault();
		}

		public static void Close<T>(this Application app) where T : Window
		{
			if (app == null)
			{
				return;
			}

			foreach (Window Window in app.Windows)
			{
				if (Window is T)
				{
					Window.Close();
				}
			}
		}

		public static bool IsWindowOpen<T>(this Application app) where T : Window
		{
			return app.GetWindow<T>() != null;
		}
	}
}
