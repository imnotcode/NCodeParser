using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCodeParser.IO
{
	public class NovelExplorer
	{
		public async void asdf()
		{
			using (var client = new CookieAwareWebClient())
			{
				client.Headers.Add("User-Agent: Other");
				client.UseDefaultCredentials = true;

				string URL = "";

				var bytes = client.DownloadData(URL);
				var downloadedString = Encoding.UTF8.GetString(bytes);
			}
		}
	}
}
