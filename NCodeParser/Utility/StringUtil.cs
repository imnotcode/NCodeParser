using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCodeParser.Utility
{
	public static class StringUtil
	{
		public static string[] DivideString(string input)
		{
			var array = input.Split(new string[] { "\n" }, StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Replace("\r", "");
			}

			return array;
		}
	}
}
