using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
