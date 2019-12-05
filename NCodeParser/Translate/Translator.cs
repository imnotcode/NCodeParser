using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCodeParser.Utility;

namespace NCodeParser.Translate
{
	public abstract class Translator
	{
		public async Task<string> Translate(string input)
		{
			var dividedTexts = StringUtil.DivideString(input);
			var tasks = new List<Task<string>>();

			for (int i = 0; i < dividedTexts.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(dividedTexts[i]))
				{
					tasks.Add(TranslateOneLine(dividedTexts[i]));
				}
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);

			var builder = new StringBuilder();
			for (int i = 0, j = 0; i < dividedTexts.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(dividedTexts[i]))
				{
					var translatedText = await tasks[j++].ConfigureAwait(false);

					builder.Append(translatedText);
				}

				builder.Append(Environment.NewLine);
			}

			return builder.ToString();
		}

		protected abstract Task<string> TranslateOneLine(string input);
	}
}
