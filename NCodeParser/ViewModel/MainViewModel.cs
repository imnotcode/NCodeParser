using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NCodeParser.Interface;
using NCodeParser.IO;
using NCodeParser.Model;
using NCodeParser.Translate;
using NCodeParser.View;

namespace NCodeParser.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		public RelayCommand AddCommand1
		{
			get;
			private set;
		}

		public RelayCommand AddCommand2
		{
			get;
			private set;
		}

		public RelayCommand AddCommand3
		{
			get;
			private set;
		}

		public RelayCommand SelectAllCommand
		{
			get;
			private set;
		}

		public RelayCommand DownloadCommand
		{
			get;
			private set;
		}

		public RelayCommand ClosingCommand
		{
			get;
			private set;
		}

		public RelayCommand SettingCommand
		{
			get;
			private set;
		}

		public ObservableCollection<Novel> NovelList
		{
			get;
			private set;
		}

		public Novel SelectedNovel
		{
			get
			{
				return _SelectedNovel;
			}
			set
			{
				_SelectedNovel = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(DownloadCommand));

				if (value != null)
				{
					_ = SelectNovel(value);
				}
			}
		}

		public string Code1
		{
			get
			{
				return _Code1;
			}
			set
			{
				_Code1 = value;
				RaisePropertyChanged();
			}
		}

		public string Code2
		{
			get
			{
				return _Code2;
			}
			set
			{
				_Code2 = value;
				RaisePropertyChanged();
			}
		}

		public string Code3
		{
			get
			{
				return _Code3;
			}
			set
			{
				_Code3 = value;
				RaisePropertyChanged();
			}
		}

		public bool ShowProgress
		{
			get
			{
				if (SelectedNovel == null)
				{
					return false;
				}
				else
				{
					return SelectedNovel.ShowProgress;
				}
			}
		}

		public int UpdateCount
		{
			get
			{
				return _UpdateCount;
			}
			set
			{
				if (value > NovelList.Count)
				{
					return;
				}

				_UpdateCount = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(TitleText));
			}
		}

		public string TitleText
		{
			get
			{
				return $"NCodeParser ({UpdateCount}/{NovelList.Count})";
			}
		}

		private Novel _SelectedNovel;
		private string _Code1;
		private string _Code2;
		private string _Code3;
		private int _UpdateCount;

		private NovelDownloader Downloader;
		private ITranslator Translator;

		public MainViewModel()
		{
			InitInstance();
			InitControls();
		}

		private void InitInstance()
		{
			AddCommand1 = new RelayCommand(OnAdd1, CanAdd1);
			AddCommand2 = new RelayCommand(OnAdd2, CanAdd2);
			AddCommand3 = new RelayCommand(OnAdd3, CanAdd3);
			SelectAllCommand = new RelayCommand(OnSelectAll);
			DownloadCommand = new RelayCommand(OnDownload);
			ClosingCommand = new RelayCommand(OnClosing);
			SettingCommand = new RelayCommand(OnSetting);

			Downloader = new NovelDownloader();
			Downloader.ProgressChanged += Downloader_ProgressChanged;

			Translator = new GSheetsTranslator();
			Config.Init();
		}

		private void InitControls()
		{
			NovelList = new ObservableCollection<Novel>();
			NovelList.AddAll(Config.NovelList);

			if (NovelList.Count > 0)
			{
				SelectedNovel = NovelList[0];
			}

			if (!IsInDesignMode)
			{
				Task.Run(() => CheckAllUpdate());
			}
		}

		private async Task SelectNovel(Novel novel)
		{
			if (novel == null)
			{
				return;
			}

			if (novel.Episodes.Count == 0)
			{
				var Episodes = await Task.Run(() =>
				{
					return Downloader.DownloadList(novel);
				});

				if (Episodes != null)
				{
					novel.Episodes.Clear();
					novel.Episodes.AddAll(Episodes);

					await App.Current.Dispatcher.InvokeAsync(() =>
					{
						novel.UIEpisodes.Clear();
						novel.UIEpisodes.AddAll(novel.Episodes);
					});
				}
			}

			if (novel.EpisodeStartIndex == -1 && novel.EpisodeEndIndex == -1)
			{
				novel.EpisodeStartIndex = 0;
				novel.EpisodeEndIndex = novel.Episodes.Count - 1;
			}

			if (novel.Episodes.Count > 0 && string.IsNullOrWhiteSpace(novel.Episodes[0].Text))
			{
				novel.ShowProgress = true;
				RaisePropertyChanged(nameof(ShowProgress));

				await Downloader.DownloadNovel(novel, 0, 0, false, true);

				novel.RaisePropertyChanged(nameof(novel.DescWithPrologue));

				novel.ShowProgress = false;
				RaisePropertyChanged(nameof(ShowProgress));

				var task1 = Translator.Translate(novel.Desc);
				var task2 = Translator.Translate(novel.Episodes[0].Text);

				novel.Desc = await task1;
				novel.Episodes[0].Text = await task2;

				novel.RaisePropertyChanged(nameof(novel.DescWithPrologue));
			}
		}

		private void CheckAllUpdate()
		{
			UpdateCount = 0;

			Parallel.For(0, NovelList.Count, (i) =>
			{
				CheckUpdate(NovelList[i]);

				UpdateCount++;
			});
		}

		private void CheckUpdate(Novel novel)
		{
			if (novel == null)
			{
				return;
			}

			string filePath = AppDomain.CurrentDomain.BaseDirectory + novel.Name;

			if (!Directory.Exists(filePath))
			{
				novel.UpdateCount = novel.Episodes.Count;
				return;
			}

			var myDirectory = new DirectoryInfo(filePath);
			var files = myDirectory.GetFiles("*.txt");

			if (novel.Episodes.Count == 0)
			{
				var Episodes = Downloader.DownloadList(novel);

				if (Episodes != null)
				{
					novel.Episodes.Clear();
					novel.Episodes.AddAll(Episodes);

					App.Current.Dispatcher.InvokeAsync(() =>
					{
						novel.UIEpisodes.Clear();
						novel.UIEpisodes.AddAll(novel.Episodes);
					});
				}
			}

			novel.EpisodeStartIndex = 0;
			novel.EpisodeEndIndex = novel.Episodes.Count - 1;

			if (novel.Episodes.Count == 0)
			{
				novel.UpdateCount = 0;
				return;
			}

			int lastNumber = novel.Episodes[novel.Episodes.Count - 1].Number;
			int max = 0;

			for (int i = 0; i < files.Length; i++)
			{
				var fileName = Path.GetFileNameWithoutExtension(files[i].FullName);

				int number;
				bool isSuccess;

				if (fileName.Contains("~"))
				{
					var splitStrings = fileName.Split('~');

					isSuccess = int.TryParse(splitStrings[splitStrings.Length - 1], out number);
					if (isSuccess && lastNumber == number)
					{
						max = lastNumber;
						break;
					}

					if (isSuccess && max < number)
					{
						max = number;
					}
				}

				isSuccess = int.TryParse(fileName, out number);
				if (!isSuccess)
				{
					continue;
				}

				if (lastNumber == number)
				{
					max = lastNumber;
					break;
				}

				if (max < number)
				{
					max = number;
				}
			}

			novel.UpdateCount = lastNumber - max;

			if (novel.UpdateCount > 0)
			{
				novel.EpisodeStartIndex = novel.EpisodeEndIndex - (novel.UpdateCount - 1);
			}
		}

		private void OnAdd1()
		{
			if (NovelList.Any(R => R.Code == Code1))
			{
				SelectedNovel = NovelList.FirstOrDefault(R => R.Code == Code1);
				Code1 = "";

				MessageBox.Show("이미 등록된 소설입니다.");

				return;
			}

			var novel = new Novel
			{
				Type = NovelType.Normal,
				Code = Code1
			};

			NovelList.Add(novel);

			Code1 = "";
			UpdateCount++;
		}

		private bool CanAdd1()
		{
			if (string.IsNullOrWhiteSpace(Code1))
			{
				return false;
			}

			return true;
		}

		private void OnAdd2()
		{
			if (NovelList.Any(R => R.Code == Code2))
			{
				SelectedNovel = NovelList.FirstOrDefault(R => R.Code == Code2);
				Code2 = "";

				MessageBox.Show("이미 등록된 소설입니다.");

				return;
			}

			var novel = new Novel
			{
				Type = NovelType.R18,
				Code = Code2
			};

			NovelList.Add(novel);

			Code2 = "";
			UpdateCount++;
		}

		private bool CanAdd2()
		{
			if (string.IsNullOrWhiteSpace(Code2))
			{
				return false;
			}

			return true;
		}

		private void OnAdd3()
		{
			if (NovelList.Any(R => R.Code == Code3))
			{
				SelectedNovel = NovelList.FirstOrDefault(R => R.Code == Code3);
				Code3 = "";

				MessageBox.Show("이미 등록된 소설입니다.");

				return;
			}

			var novel = new Novel
			{
				Type = NovelType.Kakuyomu,
				Code = Code3
			};

			NovelList.Add(novel);

			Code3 = "";
			UpdateCount++;
		}

		private bool CanAdd3()
		{
			if (string.IsNullOrWhiteSpace(Code3))
			{
				return false;
			}

			return true;
		}

		private void OnSelectAll()
		{
			if (SelectedNovel == null)
			{
				MessageBox.Show("선택된 소설이 없습니다.");
				return;
			}

			SelectedNovel.EpisodeStartIndex = 0;
			SelectedNovel.EpisodeEndIndex = SelectedNovel.Episodes.Count - 1;
		}

		private async void OnDownload()
		{
			if (SelectedNovel == null)
			{
				MessageBox.Show("선택된 소설이 없습니다.");
				return;
			}

			if (SelectedNovel.Episodes.Count == 0)
			{
				MessageBox.Show("다운로드할 수 없습니다.");
				return;
			}

			SelectedNovel.ProgressMax = (SelectedNovel.EpisodeEndIndex - SelectedNovel.EpisodeStartIndex) + 1;

			await Downloader.DownloadNovel(SelectedNovel, SelectedNovel.EpisodeStartIndex, SelectedNovel.EpisodeEndIndex, SelectedNovel.Merging);
		}

		private void OnClosing()
		{
			Config.NovelList = NovelList.ToList();
			Config.Save();
		}

		private void OnSetting()
		{
			var window = new SettingWindow();
			window.ShowDialog();
		}

		private void Downloader_ProgressChanged(object sender, int Value)
		{
			var novel = sender as Novel;
			novel.ProgressValue = Value;
		}
	}
}
