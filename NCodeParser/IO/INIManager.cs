using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;
using NCodeParser.Model;

namespace NCodeParser.IO
{
	public class INIManager
	{
		private readonly string NovelDataPath = "Data.ini";

		private IniData NovelData;
		private FileIniDataParser Parser;

		public INIManager()
		{
			Parser = new FileIniDataParser();

			if (File.Exists(NovelDataPath))
			{
				NovelData = Parser.ReadFile(NovelDataPath);
			}
			else
			{
				NovelData = new IniData();
			}
		}

		public List<Novel> GetNovels()
		{
			var NovelList = new List<Novel>();

			foreach (var SectionData in NovelData.Sections)
			{
				string Code = SectionData.Keys["Code"];
				if (string.IsNullOrWhiteSpace(Code))
				{
					continue;
				}

				string Type = SectionData.Keys["Type"];
				if (string.IsNullOrWhiteSpace(Type))
				{
					continue;
				}

				string Desc = SectionData.Keys["Desc"];

				bool IsSuccess = Enum.TryParse(Type, out NovelType NovelType);
				if (!IsSuccess)
				{
					continue;
				}

				NovelList.Add(new Novel
				{
					Code = Code,
					Type = NovelType,
					Name = Desc
				});
			}

			return NovelList;
		}

		public bool SetNovels(IList<Novel> Novels)
		{
			try
			{
				NovelData.Sections.Clear();

				for (int i = 0; i < Novels.Count; i++)
				{
					if (string.IsNullOrWhiteSpace(Novels[i].Code))
					{
						Novels.RemoveAt(i);
						i--;

						continue;
					}

					NovelData[string.Format("Novel{0:D4}", i + 1)]["Code"] = Novels[i].Code;
					NovelData[string.Format("Novel{0:D4}", i + 1)]["Type"] = Novels[i].Type.ToString();
					NovelData[string.Format("Novel{0:D4}", i + 1)]["Desc"] = Novels[i].Name;
				}

				Parser.WriteFile(NovelDataPath, NovelData, Encoding.UTF8);
			}
			catch
			{

			}

			return false;
		}
	}
}
