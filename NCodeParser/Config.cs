using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NCodeParser
{
	public static class Config
	{
		public static string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;
	}
}
