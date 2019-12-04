using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NCodeParser
{
	public static class Extensions
	{
		public static void AddAll<T>(this IList<T> List, IList<T> Collection)
		{
			if (List == null || Collection == null)
			{
				return;
			}

			for (int i = 0; i < Collection.Count; i++)
			{
				List.Add(Collection[i]);
			}
		}

		public static T GetWindow<T>(this Application App)
		{
			return App.Windows.OfType<T>().FirstOrDefault();
		}

		public static void Close<T>(this Application App)
		{
			foreach (Window Window in App.Windows)
			{
				if (Window is T)
				{
					Window.Close();
				}
			}
		}

		public static bool IsWindowOpen<T>(this Application App) where T : Window
		{
			return App.GetWindow<T>() != null;
		}
	}
}
