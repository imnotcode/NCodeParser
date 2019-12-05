using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using NCodeParser.Model;
using NCodeParser.Translate;

namespace NCodeParser.IO
{
	public class NovelDownloader
	{
		private readonly string NCodeURL = "https://ncode.syosetu.com/";
		private readonly string NCode18URL = "https://novel18.syosetu.com/";
		private readonly string KakuyomuURL = "https://kakuyomu.jp/works/";

		public event EventHandler PrologueChanged;

		private Translator Translator;

		public void SetTranslator(Translator translator)
		{
			Translator = translator;
		}

		public List<Episode> DownloadList(Novel novel)
		{
			if (novel == null)
			{
				throw new ArgumentNullException(nameof(novel), "Parameter cannot be null");
			}

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
						var collection = new Regex("<a href=\"/" + novel.Code + "/([0-9]*)/\">(.*)</a>", RegexOptions.IgnoreCase).Matches(downloadedString);

						var document = new HtmlDocument();
						document.LoadHtml(downloadedString);

						if (string.IsNullOrWhiteSpace(novel.Name))
						{
							novel.Name = document.DocumentNode.Descendants("title").FirstOrDefault().InnerText;
						}

						if (string.IsNullOrWhiteSpace(novel.Desc))
						{
							var desc = document.GetElementbyId("novel_ex").InnerText;
							if (Translator != null)
							{
								desc = Translator.Translate(desc).Result;
							}

							novel.Desc = desc;
						}

						if (novel.LastUpdateTime == default)
						{
							var datetimeString = document.DocumentNode.SelectNodes("//dt[@class='long_update']").Last().InnerText;
							datetimeString = datetimeString.Replace("\n", "");
							datetimeString = datetimeString.Replace("（改）", "");

							bool isSuccess = DateTime.TryParseExact(
								datetimeString, "yyyy'/'MM'/'dd' 'HH':'mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);

							if (isSuccess)
							{
								novel.LastUpdateTime = dateTime;
							}
						}

						var episodes = new List<Episode>();
						for (int i = 0; i < collection.Count; i++)
						{
							var episode = new Episode
							{
								Number = i + 1,
								URLNumber = (i + 1).ToString(),
								Title = collection[i].Value.Split('>')[1].Split('<')[0]
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
						var regex1 = new Regex("\"widget-toc-episode-titleLabel js-vertical-composition-item\">");
						var regex2 = new Regex("/episodes/");

						var document = new HtmlDocument();
						document.LoadHtml(downloadedString);

						if (string.IsNullOrWhiteSpace(novel.Name))
						{
							novel.Name = document.GetElementbyId("workTitle").InnerText;
						}

						if (string.IsNullOrWhiteSpace(novel.Desc))
						{
							var desc = document.GetElementbyId("introduction").InnerText;
							if (Translator != null)
							{
								desc = Translator.Translate(desc).Result;
							}

							novel.Desc = desc;
						}

						var matches1 = regex1.Matches(downloadedString);
						var matches2 = regex2.Matches(downloadedString);

						var episodes = new List<Episode>();
						var dict = new Dictionary<string, Episode>();

						int Count = 0;
						for (int i = 0, j = 0; i < matches1.Count && j < matches2.Count; i++, j++)
						{
							int startIndex1 = matches1[i].Index + matches1[i].Length;
							int endIndex1 = downloadedString.IndexOf("</span>", startIndex1, StringComparison.InvariantCulture) - 1;

							if (startIndex1 < 0 || endIndex1 < 0)
							{
								continue;
							}

							int startIndex2 = matches2[j].Index + matches2[j].Length;
							int endIndex2 = downloadedString.IndexOf("\"", startIndex2, StringComparison.InvariantCulture) - 1;

							if (startIndex2 < 0 || endIndex2 < 0)
							{
								i--;
								continue;
							}

							string title = downloadedString.Substring(startIndex1, endIndex1 - startIndex1 + 1);
							string stringNumber = downloadedString.Substring(startIndex2, endIndex2 - startIndex2 + 1);

							if (stringNumber == novel.Code)
							{
								i--;
								continue;
							}

							bool isSuccess = long.TryParse(stringNumber, out long Number);
							if (!isSuccess)
							{
								i--;
								continue;
							}

							if (dict.ContainsKey(stringNumber))
							{
								i--;
								continue;
							}

							var episode = new Episode
							{
								Number = ++Count,
								URLNumber = stringNumber,
								Title = title
							};

							episodes.Add(episode);
							dict.Add(episode.URLNumber, episode);
						}

						return episodes;
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
			finally
			{

			}

			return null;
		}

		public async Task DownloadNovel(Novel novel, int startIndex, int endIndex, bool createFile, bool loadOnly = false)
		{
			if (novel == null)
			{
				return;
			}

			novel.Downloading = true;

			try
			{
				var tasks = new List<Task>();

				for (int i = startIndex; i <= endIndex; i++)
				{
					tasks.Add(DownloadNovel(
						novel, novel.Episodes[i], createFile && !novel.Merging, Translator != null, i == 0, loadOnly));
				}

				await Task.WhenAll(tasks).ConfigureAwait(false);

				if (createFile && novel.Merging)
				{
					string novelPath = Config.NovelPath + novel.Name;

					if (!Directory.Exists(novelPath))
					{
						Directory.CreateDirectory(novelPath);
					}

					var filePath = string.Format(
						CultureInfo.InvariantCulture, "{0}\\{1:D4}~{2:D4}.txt", novelPath, startIndex + 1, endIndex + 1);

					using (var stream = new FileStream(
						filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
					{
						for (int i = startIndex; i <= endIndex; i++)
						{
							var episode = novel.Episodes[i];
							bool translate = Translator != null && !string.IsNullOrWhiteSpace(episode.TranslatedText);
							var text = translate ? episode.TranslatedText : episode.SourceText;

							var encodedText = Encoding.UTF8.GetBytes(text);

							await stream.WriteAsync(encodedText, 0, encodedText.Length).ConfigureAwait(false);

							novel.ProgressValue++;
						}
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
			finally
			{
				novel.Downloading = false;
			}
		}

		private async Task DownloadNovel(Novel novel, Episode episode, bool createFile, bool translate, bool isPrologue, bool loadOnly)
		{
			if (novel.Type == NovelType.Normal || novel.Type == NovelType.R18)
			{
				using (var client = new CookieAwareWebClient())
				{
					client.Headers.Add("User-Agent: Other");
					client.UseDefaultCredentials = true;

					string nCodeURL = novel.Type == NovelType.Normal ? NCodeURL : NCode18URL;
					string url = $"{nCodeURL}{novel.Code}/{episode.URLNumber}";

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

					var bytes = await client.DownloadDataTaskAsync(url).ConfigureAwait(false);
					string input = Encoding.UTF8.GetString(bytes);

					var document = new HtmlDocument();
					document.LoadHtml(input);

					var result = document.GetElementbyId("novel_color").InnerText;
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

					episode.SourceText = result;
				}
			}
			else
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Add("User-Agent", "Other");

					string URL = $"{KakuyomuURL}{novel.Code}/episodes/{episode.URLNumber}";
					string input = await client.GetStringAsync(new Uri(URL)).ConfigureAwait(false);

					var document = new HtmlDocument();
					document.LoadHtml(input);

					var result = document.GetElementbyId("contentMain-inner").InnerText;
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

					episode.SourceText = result;
				}
			}

			if (isPrologue)
			{
				PrologueChanged?.Invoke(novel, new EventArgs());
			}

			if (!loadOnly)
			{
				novel.ProgressValue++;
			}

			if (translate && Translator != null)
			{
				if (!string.IsNullOrWhiteSpace(episode.SourceText))
				{
					episode.TranslatedText = await Translator.Translate(episode.SourceText).ConfigureAwait(false);

					if (isPrologue)
					{
						PrologueChanged?.Invoke(novel, new EventArgs());
					}
				}
			}
			else
			{
				episode.TranslatedText = null;
			}

			if (createFile)
			{
				string novelPath = Config.NovelPath + novel.Name;

				if (!Directory.Exists(novelPath))
				{
					Directory.CreateDirectory(novelPath);
				}

				var filePath = string.Format(CultureInfo.InvariantCulture, "{0}\\{1:D4}.txt", novelPath, episode.Number);
				File.WriteAllText(filePath, episode.TranslatedText ?? episode.SourceText, Encoding.UTF8);

				novel.ProgressValue++;
			}
		}
	}
}
