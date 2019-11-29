using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace NCodeParser.ViewModel
{
	public class SettingViewModel : ViewModelBase
	{
		public RelayCommand SaveComamnd
		{
			get;
			private set;
		}

		public RelayCommand CancelCommand
		{
			get;
			private set;
		}

		public SettingViewModel()
		{
			InitInstance();
		}

		private void InitInstance()
		{
			SaveComamnd = new RelayCommand(OnSave);
			CancelCommand = new RelayCommand(OnCancel);
		}

		private void OnSave()
		{
			
		}

		private void OnCancel()
		{

		}
	}
}
