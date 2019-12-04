using System;
using System.IO;
using GalaSoft.MvvmLight.CommandWpf;

namespace NCodeParser.ViewModel.Options
{
	public class GeneralSettingViewModel : BaseSettingViewModel
	{
		public RelayCommand SetNovelPathCommand
		{
			get;
			private set;
		}

		public string NovelPath
		{
			get
			{
				return _NovelPath;
			}
			set
			{
				_NovelPath = value;
				RaisePropertyChanged();
			}
		}

		private string _NovelPath;

		public GeneralSettingViewModel()
		{
			SetNovelPathCommand = new RelayCommand(OnSetNovelPath);

			NovelPath = Config.NovelPath;
		}

		public override void SetConfig()
		{
			Config.NovelPath = NovelPath;
		}

		private void OnSetNovelPath()
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (string.IsNullOrWhiteSpace(NovelPath))
				{
					dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
				}
				else if (Directory.Exists(NovelPath))
				{
					dialog.SelectedPath = NovelPath;
				}

				var result = dialog.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					NovelPath = dialog.SelectedPath;
				}
			}
		}
	}
}
