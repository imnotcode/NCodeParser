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
		public static readonly string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;

		public static readonly string ApplicationVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

		public static INIManager INIManager
		{
			get;
			private set;
		}

		public static List<Novel> NovelList
		{
			get;
			set;
		}

		public static string NovelPath
		{
			get
			{
				return _NovelPath;
			}
			set
			{
				if (!string.IsNullOrWhiteSpace(value) && value.Last() != '\\')
				{
					_NovelPath = value + "\\";
				}
				else
				{
					_NovelPath = value;
				}
			}
		}

		public static TranslatorType TranslatorType
		{
			get;
			set;
		}

		public static bool TranslateWithSource
		{
			get;
			set;
		}

		private static string _NovelPath = "";

		public static void Init()
		{
			INIManager = new INIManager();

			NovelPath = INIManager.GetNovelPath();
			NovelList = INIManager.GetNovels();
			TranslatorType = TranslatorType.GSheet; // TODO
			TranslateWithSource = true; // TODO
		}

		public static void Save()
		{
			INIManager.Clear();
			INIManager.SetNovelPath(NovelPath);
			INIManager.SetNovels(NovelList);
			// TODO SetTranslatorType
			// TODO
		}
	}
}
