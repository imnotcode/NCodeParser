using System;
using System.Threading.Tasks;
using NCodeParser.Interfaces;

namespace NCodeParser.Translate
{
	public class GoogleTranslator : ITranslator
	{
		public GoogleTranslator()
		{

		}

		public async Task<string> Translate(string input)
		{
			throw new NotImplementedException();
		}
	}
}
