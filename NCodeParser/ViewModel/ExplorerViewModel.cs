using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NCodeParser.IO;

namespace NCodeParser.ViewModel
{
	public class ExplorerViewModel : ViewModelBase
	{
		private NovelExplorer Explorer;

		public ExplorerViewModel()
		{
			Explorer = new NovelExplorer();
			Explorer.asdf();
		}
	}
}
