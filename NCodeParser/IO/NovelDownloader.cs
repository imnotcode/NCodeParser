using NCodeParser.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NCodeParser.IO
{
	public class NovelDownloader
	{
		private readonly string NCodeURL = "https://ncode.syosetu.com/";
		private readonly string NCode18URL = "https://novel18.syosetu.com/";
		private readonly string KakuyomuURL = "https://kakuyomu.jp/works/";

		public bool Downloading
		{
			get;
			private set;
		}

		public event EventHandler<int> ProgressChanged;

		public List<Episode> DownloadList(Novel Novel)
		{
			Downloading = true;

			try
			{
				if (Novel.Type == NovelType.Normal || Novel.Type == NovelType.R18)
				{
					using (var Client = new CookieAwareWebClient())
					{
						Client.Headers.Add("User-Agent: Other");
						Client.UseDefaultCredentials = true;

						string URL = (Novel.Type == NovelType.Normal ? NCodeURL : NCode18URL) + Novel.Code + "/";

						if (Novel.Type == NovelType.R18)
						{
							var Values = new Dictionary<string, string>
							{
								{ "over18", "yes" },
								{ "ks2", "f6argh6akx2" },
								{ "sasieno", "0" },
								{ "lineheight", "0" },
								{ "fontsize", "0" },
								{ "novellayout", "0" },
								{ "fix_menu_bar", "1" }
							};

							var CookieString = new StringBuilder();
							foreach (var Value in Values)
							{
								CookieString.Append(Value.Key);
								CookieString.Append("=");
								CookieString.Append(Value.Value);
								CookieString.Append(";");
								CookieString.Append(" ");
							}

							CookieString = CookieString.Remove(CookieString.Length - 1, 1);

							Client.CookieContainer.SetCookies(new Uri(URL), CookieString.ToString());
						}

						var Bytes = Client.DownloadData(URL);
						var DownloadedString = Encoding.UTF8.GetString(Bytes);
						var MatchCollection = new Regex("<a href=\"/" + Novel.Code + "/([0-9]*)/\">(.*)</a>", RegexOptions.IgnoreCase).Matches(DownloadedString);

						var Episodes = new List<Episode>();
						for (int i = 0; i < MatchCollection.Count; i++)
						{
							var Episode = new Episode
							{
								Number = i + 1,
								URLNumber = (i + 1).ToString(),
								Title = MatchCollection[i].Value.Split('>')[1].Split('<')[0]
							};

							Episodes.Add(Episode);
						}

						return Episodes;
					}
				}
				else if (Novel.Type == NovelType.Kakuyomu)
				{
					using (var Client = new WebClient())
					{
						Client.Headers.Add("User-Agent: Other");
						Client.UseDefaultCredentials = true;

						string URL = KakuyomuURL + Novel.Code;

						var Bytes = Client.DownloadData(URL);
						var DownloadedString = Encoding.UTF8.GetString(Bytes);
						var Regex1 = new Regex("\"widget-toc-episode-titleLabel js-vertical-composition-item\">");
						var Regex2 = new Regex("/episodes/");

						var Matches1 = Regex1.Matches(DownloadedString);
						var Matches2 = Regex2.Matches(DownloadedString);

						var Episodes = new List<Episode>();
						var Dict = new Dictionary<string, Episode>();

						int Count = 0;
						for (int i = 0, j = 0; i < Matches1.Count && j < Matches2.Count; i++, j++)
						{
							int StartIndex1 = Matches1[i].Index + Matches1[i].Length;
							int EndIndex1 = DownloadedString.IndexOf("</span>", StartIndex1) - 1;

							if (StartIndex1 < 0 || EndIndex1 < 0)
							{
								continue;
							}

							int StartIndex2 = Matches2[j].Index + Matches2[j].Length;
							int EndIndex2 = DownloadedString.IndexOf("\"", StartIndex2) - 1;

							if (StartIndex2 < 0 || EndIndex2 < 0)
							{
								i--;
								continue;
							}

							string Title = DownloadedString.Substring(StartIndex1, EndIndex1 - StartIndex1 + 1);
							string StringNumber = DownloadedString.Substring(StartIndex2, EndIndex2 - StartIndex2 + 1);

							if (StringNumber == Novel.Code)
							{
								i--;
								continue;
							}

							bool IsSuccess = long.TryParse(StringNumber, out long Number);
							if (!IsSuccess)
							{
								i--;
								continue;
							}

							if (Dict.ContainsKey(StringNumber))
							{
								i--;
								continue;
							}

							var Episode = new Episode
							{
								Number = ++Count,
								URLNumber = StringNumber,
								Title = Title
							};

							Episodes.Add(Episode);
							Dict.Add(Episode.URLNumber, Episode);
						}

						return Episodes;
					}
				}
			}
			catch
			{

			}
			finally
			{
				Downloading = false;
			}

			return null;
		}

		public void DownloadNovel(Novel Novel, int StartIndex, int EndIndex, bool Merging)
		{
			Downloading = true;

			try
			{
				if (Novel.Type == NovelType.Normal || Novel.Type == NovelType.R18)
				{
					if (!Directory.Exists(Novel.Desc))
					{
						Directory.CreateDirectory(Novel.Desc);
					}

					var Regex1 = new Regex("<p class=\"novel_subtitle\">(.*)</p>", RegexOptions.Compiled);
					var Regex2 = new Regex("\"Lp[0-9]*\">(.*)</p>", RegexOptions.Multiline);
					var Regex3 = new Regex("\"L[0-9]*\">(.*)</p>", RegexOptions.Multiline);
					var Regex4 = new Regex("\"La[0-9]*\">(.*)</p>", RegexOptions.Multiline);

					var Dict = new Dictionary<int, string>();
					int Count = 0;

					for (int i = StartIndex; i <= EndIndex; i++)
					{
						Console.WriteLine(i);
						var Builder = new StringBuilder();

						var Client = new CookieAwareWebClient();
						Client.Headers.Add("User-Agent: Other");
						Client.UseDefaultCredentials = true;

						string NCodeURL = Novel.Type == NovelType.Normal ? this.NCodeURL : NCode18URL;
						string URL = string.Format($"{NCodeURL}{Novel.Code}/{Novel.Episodes[i].URLNumber}");

						if (Novel.Type == NovelType.R18)
						{
							var Values = new Dictionary<string, string>
							{
								{ "over18", "yes" },
								{ "ks2", "f6argh6akx2" },
								{ "sasieno", "0" },
								{ "lineheight", "0" },
								{ "fontsize", "0" },
								{ "novellayout", "0" },
								{ "fix_menu_bar", "1" }
							};

							var CookieString = new StringBuilder();
							foreach (var Value in Values)
							{
								CookieString.Append(Value.Key);
								CookieString.Append("=");
								CookieString.Append(Value.Value);
								CookieString.Append(";");
								CookieString.Append(" ");
							}

							CookieString = CookieString.Remove(CookieString.Length - 1, 1);

							Client.CookieContainer.SetCookies(new Uri(URL), CookieString.ToString());
						}

						var Bytes = Client.DownloadData(URL);
						string Input = Encoding.UTF8.GetString(Bytes);
						var Collection = Regex1.Matches(Input);

						Builder.Append(Collection[0].Value.Split('>')[1].Split('<')[0] + "\r\n\r\n");

						if (true)
						{
							Collection = Regex2.Matches(Input);
							if (Collection.Count > 0)
							{
								Builder.Append("\r\n------------------------------------------------\r\n");
							}

							foreach (Match Item in Collection)
							{
								Builder.Append(Item.Groups[1].Value);
								Builder.Append("\r\n");
							}

							Builder.Append("\r\n------------------------------------------------\r\n");
						}

						Collection = Regex3.Matches(Input);
						foreach (Match Item in Collection)
						{
							Builder.Append(Item.Groups[1].Value);
							Builder.Append(Environment.NewLine);
						}

						if (true)
						{
							Collection = Regex4.Matches(Input);
							if (Collection.Count > 0)
							{
								Builder.Append("\r\n------------------------------------------------\r\n");
							}

							foreach (Match Item in Collection)
							{
								Builder.Append(Item.Groups[1].Value);
								Builder.Append("\r\n");
							}
						}

						var Result = Builder.ToString();
						Dict.Add(i, Result);

						if (!Merging)
						{
							Result = Result.Replace("<ruby>", "");
							Result = Result.Replace("</ruby>", "");
							Result = Result.Replace("<rp>", "");
							Result = Result.Replace("</rp>", "");
							Result = Result.Replace("<rb>", "");
							Result = Result.Replace("</rb>", "");
							Result = Result.Replace("<rt>", "");
							Result = Result.Replace("</rt>", "");
							Result = Result.Replace("<br />", Environment.NewLine + Environment.NewLine);
							Result = Result.Replace("&quot;", "\"");
							Result = Result.Replace("&lt;", "<");
							Result = Result.Replace("&gt;", ">");
							Result = Result.Replace("&quot", "\"");
							Result = Result.Replace("&lt", "<");
							Result = Result.Replace("&gt", ">");

							File.WriteAllText(string.Format("{0}\\{1:D4}.txt", Novel.Desc, i + 1), Result, Encoding.UTF8);
						}

						ProgressChanged?.Invoke(Novel, ++Count);

						if (Merging && Count - 1 == EndIndex - StartIndex)
						{
							Builder.Clear();

							for (int j = StartIndex; j <= EndIndex; j++)
							{
								Builder.Append(Dict[j]);
							}

							Result = Builder.ToString();
							Result = Result.Replace("<ruby>", "");
							Result = Result.Replace("</ruby>", "");
							Result = Result.Replace("<rp>", "");
							Result = Result.Replace("</rp>", "");
							Result = Result.Replace("<rb>", "");
							Result = Result.Replace("</rb>", "");
							Result = Result.Replace("<rt>", "");
							Result = Result.Replace("</rt>", "");
							Result = Result.Replace("<br />", Environment.NewLine + Environment.NewLine);
							Result = Result.Replace("&quot;", "\"");
							Result = Result.Replace("&lt;", "<");
							Result = Result.Replace("&gt;", ">");
							Result = Result.Replace("&quot", "\"");
							Result = Result.Replace("&lt", "<");
							Result = Result.Replace("&gt", ">");

							File.WriteAllText(string.Format("{0}\\{1:D4}~{2:D4}.txt", Novel.Desc, StartIndex + 1, EndIndex + 1), Result, Encoding.UTF8);
						}
					}
				}
				else
				{
					if (!Directory.Exists(Novel.Desc))
					{
						Directory.CreateDirectory(Novel.Desc);
					}

					var Regex1 = new Regex("<p class=\"chapterTitle level1 js-vertical-composition-item\"><span>", RegexOptions.Multiline);
					var Regex2 = new Regex("<p class=\"widget-episodeTitle js-vertical-composition-item\">", RegexOptions.Multiline);
					var Regex3 = new Regex("<p id=\"p\\d+\"", RegexOptions.Multiline);

					var Dict = new Dictionary<string, string>();
					int Count = 0;

					for (int i = StartIndex; i <= EndIndex; i++)
					{
						Console.WriteLine(i);
						var Builder = new StringBuilder();

						var Client = new HttpClient();
						Client.DefaultRequestHeaders.Add("User-Agent", "Other");

						string URL = string.Format($"{KakuyomuURL}{Novel.Code}/episodes/{Novel.Episodes[i].URLNumber}");

						string Input = Client.GetStringAsync(URL).Result;

						var Matches1 = Regex1.Matches(Input);
						if (Matches1.Count > 0)
						{
							int StartIndex1 = Matches1[0].Index + Matches1[0].Length;
							int EndIndex1 = Input.IndexOf("</span>", StartIndex1) - 1;

							string ChapterTitle = Input.Substring(StartIndex1, EndIndex1 - StartIndex1 + 1);

							Builder.Append(ChapterTitle);
							Builder.AppendLine();
						}

						var Matches2 = Regex2.Matches(Input);
						if (Matches2.Count > 0)
						{
							int StartIndex2 = Matches2[0].Index + Matches2[0].Length;
							int EndIndex2 = Input.IndexOf("</p>", StartIndex2) - 1;

							string EpisodeTitle = Input.Substring(StartIndex2, EndIndex2 - StartIndex2 + 1);

							Builder.Append(EpisodeTitle);
							Builder.AppendLine();
							Builder.AppendLine();
						}

						var Matches3 = Regex3.Matches(Input);
						for (int j = 0; j < Matches3.Count; j++)
						{
							int StartIndex3 = Matches3[j].Index + 1 + Matches3[j].Length;
							int EndIndex3 = Input.IndexOf("</p>", StartIndex3) - 1;

							string Content = Input.Substring(StartIndex3, EndIndex3 - StartIndex3 + 1);
							if (Content.Contains("class=\"blank\""))
							{
								Builder.AppendLine();
								continue;
							}

							Builder.Append(Content);
							Builder.AppendLine();
						}

						Builder.AppendLine();
						Builder.AppendLine();

						var Result = Builder.ToString();

						if (!Dict.ContainsKey(Novel.Episodes[i].URLNumber))
						{
							Dict.Add(Novel.Episodes[i].URLNumber, Result);

							if (!Merging)
							{
								Result = Result.Replace("<em class=\"emphasisDots\">", "");
								Result = Result.Replace("</em>", "");
								Result = Result.Replace("<span>", "");
								Result = Result.Replace("</span>", "");
								Result = Result.Replace("<ruby>", "");
								Result = Result.Replace("</ruby>", "");
								Result = Result.Replace("<rp>", "");
								Result = Result.Replace("</rp>", "");
								Result = Result.Replace("<rb>", "");
								Result = Result.Replace("</rb>", "");
								Result = Result.Replace("<rt>", "");
								Result = Result.Replace("</rt>", "");
								Result = Result.Replace("<br />", "\r\n");

								File.WriteAllText(string.Format("{0}\\{1:D4}.txt", Novel.Desc, i + 1), Result, Encoding.UTF8);
							}
						}

						ProgressChanged?.Invoke(Novel, ++Count);

						if (Merging && Count - 1 == EndIndex - StartIndex)
						{
							Builder.Clear();

							for (int j = StartIndex; j <= EndIndex; j++)
							{
								Builder.Append(Dict[Novel.Episodes[j].URLNumber]);
							}

							Result = Builder.ToString();

							Result = Result.Replace("<em class=\"emphasisDots\">", "");
							Result = Result.Replace("</em>", "");
							Result = Result.Replace("<span>", "");
							Result = Result.Replace("</span>", "");
							Result = Result.Replace("<ruby>", "");
							Result = Result.Replace("</ruby>", "");
							Result = Result.Replace("<rp>", "");
							Result = Result.Replace("</rp>", "");
							Result = Result.Replace("<rb>", "");
							Result = Result.Replace("</rb>", "");
							Result = Result.Replace("<rt>", "");
							Result = Result.Replace("</rt>", "");
							Result = Result.Replace("<br />", "\r\n");

							/*Result = Result.Replace("<ruby>", "");
							Result = Result.Replace("</ruby>", "");
							Result = Result.Replace("<rp>", "");
							Result = Result.Replace("</rp>", "");
							Result = Result.Replace("<rb>", "");
							Result = Result.Replace("</rb>", "");
							Result = Result.Replace("<rt>", "");
							Result = Result.Replace("</rt>", "");
							Result = Result.Replace("<br />", "\r\n");
							Result = Result.Replace("&quot;", "\"");
							Result = Result.Replace("&lt;", "<");
							Result = Result.Replace("&gt;", ">");
							Result = Result.Replace("&quot", "\"");
							Result = Result.Replace("&lt", "<");
							Result = Result.Replace("&gt", ">");*/

							File.WriteAllText(string.Format("{0}\\{1:D4}~{2:D4}.txt", Novel.Desc, StartIndex + 1, EndIndex + 1), Result, Encoding.UTF8);
						}
					}
				}
			}
			catch
			{

			}
			finally
			{
				Downloading = false;
			}
		}
	}
}
