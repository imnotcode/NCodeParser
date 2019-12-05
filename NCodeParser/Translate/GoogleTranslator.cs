using System;
using System.Threading.Tasks;

namespace NCodeParser.Translate
{
	public class GoogleTranslator : Translator
	{
		public GoogleTranslator()
		{

		}

		protected override async Task<string> TranslateOneLine(string input)
		{
			throw new NotImplementedException();
		}
	}
}
