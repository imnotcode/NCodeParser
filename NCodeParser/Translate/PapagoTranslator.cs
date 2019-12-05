using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NCodeParser.Utility;
using Newtonsoft.Json.Linq;

namespace NCodeParser.Translate
{
	public class PapagoTranslator : Translator
	{
		private readonly string PapagoURL = "https://openapi.naver.com/v1/papago/n2mt";
		private readonly string ClientID = "VkvcCDmkbikqlpLZfLVC";
		private readonly string ClientSecret = "cDxywRZf2W";

		public PapagoTranslator()
		{

		}

		protected override async Task<string> TranslateOneLine(string input)
		{
			try
			{
				var parameterBuilder = new StringBuilder();
				parameterBuilder.Append("source=ja&target=ko&text=");
				parameterBuilder.Append(input);

				var data = parameterBuilder.ToString();
				var buffer = Encoding.UTF8.GetBytes(data);

				var request = WebRequest.Create(new Uri(PapagoURL));
				request.Headers.Add("X-Naver-Client-Id", ClientID);
				request.Headers.Add("X-Naver-Client-Secret", ClientSecret);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				request.ContentLength = buffer.Length;

				using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
				{
					await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
					stream.Close();
				}

				using (var response = (HttpWebResponse) await request.GetResponseAsync().ConfigureAwait(false))
				{
					using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
					{
						var downloadedText = await reader.ReadToEndAsync().ConfigureAwait(false);
						var jObject = JObject.Parse(downloadedText);
						var result = jObject["message"]["result"]["translatedText"].ToString();

						return result;
					}
				}
			}
			catch
			{

			}

			return input;
		}
	}
}
