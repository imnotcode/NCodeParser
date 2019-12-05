using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCodeParser.IO
{
	public class NovelExplorer
	{
		public async Task asdf()
		{
			using (var client = new CookieAwareWebClient())
			{
				client.Headers.Add("User-Agent: Other");
				client.UseDefaultCredentials = true;

				string URL = "https://kakuyomu.jp/";

				var bytes = await client.DownloadDataTaskAsync(new Uri(URL)).ConfigureAwait(false);
				var downloadedString = Encoding.UTF8.GetString(bytes);
			}
		}
	}
}
