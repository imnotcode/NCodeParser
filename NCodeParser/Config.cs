using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NCodeParser.IO;
using NCodeParser.Model;

namespace NCodeParser
{
	public static class Config
	{
		public static string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;

		public static INIManager INIManager
		{
			get;
			private set;
		}

		public static List<Novel> NovelList = new List<Novel>();

		public static string NovelDirectory = "";

		public static void Init()
		{
			INIManager = new INIManager();

			NovelList = INIManager.GetNovels();
		}

		public static void Save()
		{
			INIManager.SetNovels(NovelList);
		}
	}
}
