using HtmlAgilityPack;
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

		public List<Episode> DownloadList(Novel novel)
		{
			Downloading = true;

			try
			{
				if (novel.Type == NovelType.Normal || novel.Type == NovelType.R18)
				{
					using (var client = new CookieAwareWebClient())
					{
						client.Headers.Add("User-Agent: Other");
						client.UseDefaultCredentials = true;

						string URL = (novel.Type == NovelType.Normal ? NCodeURL : NCode18URL) + novel.Code + "/";

						if (novel.Type == NovelType.R18)
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

							var cookieString = new StringBuilder();
							foreach (var Value in Values)
							{
								cookieString.Append(Value.Key);
								cookieString.Append("=");
								cookieString.Append(Value.Value);
								cookieString.Append(";");
								cookieString.Append(" ");
							}

							cookieString = cookieString.Remove(cookieString.Length - 1, 1);

							client.CookieContainer.SetCookies(new Uri(URL), cookieString.ToString());
						}

						var bytes = client.DownloadData(URL);
						var downloadedString = Encoding.UTF8.GetString(bytes);
						var MatchCollection = new Regex("<a href=\"/" + novel.Code + "/([0-9]*)/\">(.*)</a>", RegexOptions.IgnoreCase).Matches(downloadedString);

						var document = new HtmlDocument();
						document.LoadHtml(downloadedString);

						if (string.IsNullOrWhiteSpace(novel.Name))
						{
							novel.Name = document.DocumentNode.Descendants("title").FirstOrDefault().InnerText;
						}

						if (string.IsNullOrWhiteSpace(novel.Desc))
						{
							novel.Desc = document.GetElementbyId("novel_ex").InnerText;
						}

						var episodes = new List<Episode>();
						for (int i = 0; i < MatchCollection.Count; i++)
						{
							var episode = new Episode
							{
								Number = i + 1,
								URLNumber = (i + 1).ToString(),
								Title = MatchCollection[i].Value.Split('>')[1].Split('<')[0]
							};

							episodes.Add(episode);
						}

						return episodes;
					}
				}
				else if (novel.Type == NovelType.Kakuyomu)
				{
					using (var client = new WebClient())
					{
						client.Headers.Add("User-Agent: Other");
						client.UseDefaultCredentials = true;

						string URL = KakuyomuURL + novel.Code;

						var bytes = client.DownloadData(URL);
						var downloadedString = Encoding.UTF8.GetString(bytes);
						var Regex1 = new Regex("\"widget-toc-episode-titleLabel js-vertical-composition-item\">");
						var Regex2 = new Regex("/episodes/");

						var document = new HtmlDocument();
						document.LoadHtml(downloadedString);

						if (string.IsNullOrWhiteSpace(novel.Name))
						{
							novel.Name = document.GetElementbyId("workTitle").InnerText;
						}

						if (string.IsNullOrWhiteSpace(novel.Desc))
						{
							novel.Desc = document.GetElementbyId("introduction").InnerText;
						}

						var Matches1 = Regex1.Matches(downloadedString);
						var Matches2 = Regex2.Matches(downloadedString);

						var Episodes = new List<Episode>();
						var Dict = new Dictionary<string, Episode>();

						int Count = 0;
						for (int i = 0, j = 0; i < Matches1.Count && j < Matches2.Count; i++, j++)
						{
							int StartIndex1 = Matches1[i].Index + Matches1[i].Length;
							int EndIndex1 = downloadedString.IndexOf("</span>", StartIndex1) - 1;

							if (StartIndex1 < 0 || EndIndex1 < 0)
							{
								continue;
							}

							int StartIndex2 = Matches2[j].Index + Matches2[j].Length;
							int EndIndex2 = downloadedString.IndexOf("\"", StartIndex2) - 1;

							if (StartIndex2 < 0 || EndIndex2 < 0)
							{
								i--;
								continue;
							}

							string Title = downloadedString.Substring(StartIndex1, EndIndex1 - StartIndex1 + 1);
							string StringNumber = downloadedString.Substring(StartIndex2, EndIndex2 - StartIndex2 + 1);

							if (StringNumber == novel.Code)
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

		public void DownloadNovel(Novel novel, int startIndex, int endIndex, bool merging, bool loadOnly = false)
		{
			Downloading = true;

			try
			{
				if (!loadOnly && !Directory.Exists(novel.Name))
				{
					Directory.CreateDirectory(novel.Name);
				}

				if (novel.Type == NovelType.Normal || novel.Type == NovelType.R18)
				{
					var regex1 = new Regex("<p class=\"novel_subtitle\">(.*)</p>", RegexOptions.Compiled);
					var regex2 = new Regex("\"Lp[0-9]*\">(.*)</p>", RegexOptions.Multiline);
					var regex3 = new Regex("\"L[0-9]*\">(.*)</p>", RegexOptions.Multiline);
					var regex4 = new Regex("\"La[0-9]*\">(.*)</p>", RegexOptions.Multiline);

					var dict = new Dictionary<int, string>();
					int count = 0;

					for (int i = startIndex; i <= endIndex; i++)
					{
						Console.WriteLine(i);
						var builder = new StringBuilder();

						var client = new CookieAwareWebClient();
						client.Headers.Add("User-Agent: Other");
						client.UseDefaultCredentials = true;

						string nCodeURL = novel.Type == NovelType.Normal ? this.NCodeURL : NCode18URL;
						string url = string.Format($"{nCodeURL}{novel.Code}/{novel.Episodes[i].URLNumber}");

						if (novel.Type == NovelType.R18)
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

							var cookieString = new StringBuilder();
							foreach (var Value in Values)
							{
								cookieString.Append(Value.Key);
								cookieString.Append("=");
								cookieString.Append(Value.Value);
								cookieString.Append(";");
								cookieString.Append(" ");
							}

							cookieString = cookieString.Remove(cookieString.Length - 1, 1);

							client.CookieContainer.SetCookies(new Uri(url), cookieString.ToString());
						}

						var bytes = client.DownloadData(url);
						string input = Encoding.UTF8.GetString(bytes);
						var collection = regex1.Matches(input);

						var document = new HtmlDocument();
						document.LoadHtml(input);

						builder.Append(document.GetElementbyId("novel_color").InnerText);

						var result = builder.ToString();
						result = result.Replace("&nbsp;", "");
						result = result.Replace("<ruby>", "");
						result = result.Replace("</ruby>", "");
						result = result.Replace("<rp>", "");
						result = result.Replace("</rp>", "");
						result = result.Replace("<rb>", "");
						result = result.Replace("</rb>", "");
						result = result.Replace("<rt>", "");
						result = result.Replace("</rt>", "");
						result = result.Replace("<br />", Environment.NewLine + Environment.NewLine);
						result = result.Replace("&quot;", "\"");
						result = result.Replace("&lt;", "<");
						result = result.Replace("&gt;", ">");
						result = result.Replace("&quot", "\"");
						result = result.Replace("&lt", "<");
						result = result.Replace("&gt", ">");

						dict.Add(i, result);

						if (loadOnly)
						{
							novel.Episodes[i].Text = result;
						}
						else if (!merging)
						{
							File.WriteAllText(string.Format("{0}\\{1:D4}.txt", novel.Name, i + 1), result, Encoding.UTF8);
						}

						if (!loadOnly)
						{
							ProgressChanged?.Invoke(novel, ++count);
						}

						if (!loadOnly && merging && count - 1 == endIndex - startIndex)
						{
							builder.Clear();

							for (int j = startIndex; j <= endIndex; j++)
							{
								builder.Append(dict[j]);
							}

							result = builder.ToString();

							File.WriteAllText(string.Format("{0}\\{1:D4}~{2:D4}.txt", novel.Name, startIndex + 1, endIndex + 1), result, Encoding.UTF8);
						}
					}
				}
				else
				{
					var Regex1 = new Regex("<p class=\"chapterTitle level1 js-vertical-composition-item\"><span>", RegexOptions.Multiline);
					var Regex2 = new Regex("<p class=\"widget-episodeTitle js-vertical-composition-item\">", RegexOptions.Multiline);
					var Regex3 = new Regex("<p id=\"p\\d+\"", RegexOptions.Multiline);

					var dict = new Dictionary<string, string>();
					int count = 0;

					for (int i = startIndex; i <= endIndex; i++)
					{
						Console.WriteLine(i);
						var builder = new StringBuilder();

						var client = new HttpClient();
						client.DefaultRequestHeaders.Add("User-Agent", "Other");

						string URL = string.Format($"{KakuyomuURL}{novel.Code}/episodes/{novel.Episodes[i].URLNumber}");

						string input = client.GetStringAsync(URL).Result;

						var document = new HtmlDocument();
						document.LoadHtml(input);

						builder.Append(document.GetElementbyId("contentMain-inner").InnerText);

						var result = builder.ToString();
						result = result.Replace("&nbsp;", "");
						result = result.Replace("<em class=\"emphasisDots\">", "");
						result = result.Replace("</em>", "");
						result = result.Replace("<span>", "");
						result = result.Replace("</span>", "");
						result = result.Replace("<ruby>", "");
						result = result.Replace("</ruby>", "");
						result = result.Replace("<rp>", "");
						result = result.Replace("</rp>", "");
						result = result.Replace("<rb>", "");
						result = result.Replace("</rb>", "");
						result = result.Replace("<rt>", "");
						result = result.Replace("</rt>", "");
						result = result.Replace("<br />", "\r\n");

						if (!dict.ContainsKey(novel.Episodes[i].URLNumber))
						{
							dict.Add(novel.Episodes[i].URLNumber, result);

							if (loadOnly)
							{
								novel.Episodes[i].Text = result;
							}
							else if (!merging)
							{
								File.WriteAllText(string.Format("{0}\\{1:D4}.txt", novel.Name, i + 1), result, Encoding.UTF8);
							}
						}

						if (!loadOnly)
						{
							ProgressChanged?.Invoke(novel, ++count);
						}

						if (!loadOnly && merging && count - 1 == endIndex - startIndex)
						{
							builder.Clear();

							for (int j = startIndex; j <= endIndex; j++)
							{
								builder.Append(dict[novel.Episodes[j].URLNumber]);
							}

							result = builder.ToString();

							File.WriteAllText(string.Format("{0}\\{1:D4}~{2:D4}.txt", novel.Name, startIndex + 1, endIndex + 1), result, Encoding.UTF8);
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
