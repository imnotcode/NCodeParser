﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using NCodeParser.IO;
using NCodeParser.Model;
using NCodeParser.Translate;
using NCodeParser.View;

namespace NCodeParser.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		public RelayCommand LoadedCommand
		{
			get;
			private set;
		}

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

		public RelayCommand ExplorerCommand
		{
			get;
			private set;
		}

		public RelayCommand ExitCommand
		{
			get;
			private set;
		}

		public RelayCommand ShowLicenseCommand
		{
			get;
			private set;
		}

		public RelayCommand ShowAboutCommand
		{
			get;
			private set;
		}

		public RelayCommand SettingCommand
		{
			get;
			private set;
		}

		public RelayCommand OpenFolderCommand
		{
			get;
			private set;
		}

		public RelayCommand DeleteNovelCommand
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
				if (NovelList == null)
				{
					return "NCodeParser";
				}

				return $"NCodeParser ({UpdateCount}/{NovelList.Count})";
			}
		}

		private Novel _SelectedNovel;
		private string _Code1;
		private string _Code2;
		private string _Code3;
		private int _UpdateCount;

		private NovelDownloader Downloader;
		private Translator Translator;

		public MainViewModel()
		{
			InitInstance();
			InitControls();
		}

		private void InitInstance()
		{
			LoadedCommand = new RelayCommand(OnLoaded);
			AddCommand1 = new RelayCommand(OnAdd1, CanAdd1);
			AddCommand2 = new RelayCommand(OnAdd2, CanAdd2);
			AddCommand3 = new RelayCommand(OnAdd3, CanAdd3);
			SelectAllCommand = new RelayCommand(OnSelectAll);
			DownloadCommand = new RelayCommand(OnDownload);
			ClosingCommand = new RelayCommand(OnClosing);
			ExplorerCommand = new RelayCommand(OnExplorer);
			ExitCommand = new RelayCommand(OnExit);
			ShowLicenseCommand = new RelayCommand(OnShowLicense);
			ShowAboutCommand = new RelayCommand(OnShowAbout);
			SettingCommand = new RelayCommand(OnSetting);
			OpenFolderCommand = new RelayCommand(OnOpenFolder, CanOpenFolder);
			DeleteNovelCommand = new RelayCommand(OnDeleteNovel, CanDeleteNovel);

			Downloader = new NovelDownloader();
			Downloader.PrologueChanged += Downloader_PrologueChanged;

			Config.Init();
		}

		private async void InitControls()
		{
			NovelList = new ObservableCollection<Novel>();
			NovelList.AddAll(Config.NovelList);

			if (NovelList.Count > 0)
			{
				SelectedNovel = NovelList[0];
			}

			if (!IsInDesignMode)
			{
				await SetTranslator().ConfigureAwait(false);
				await Task.Run(() => CheckAllUpdate()).ConfigureAwait(false);
			}
		}

		private async void OnLoaded()
		{
			string version = await UpdateHelper.GetLatestVersion().ConfigureAwait(false);
			if (string.IsNullOrWhiteSpace(version))
			{
				return;
			}

			string currentVersion = Config.ApplicationVersion;
			if (false) // TODO Compare Version
			{
				var result = MessageBox.Show("TODO");
			}
		}

		private async Task SetTranslator()
		{
			if (Downloader == null)
			{
				Debug.Assert(false);
				return;
			}

			if (Config.TranslatorType == TranslatorType.GSheet)
			{
				var translator = new GSheetsTranslator();
				await translator.InitializeService().ConfigureAwait(false);

				Translator = translator;
			}
			else if (Config.TranslatorType == TranslatorType.Google)
			{
				var translator = new GoogleTranslator();

				Translator = translator;
			}
			else if (Config.TranslatorType == TranslatorType.Papago)
			{
				var translator = new PapagoTranslator();

				Translator = translator;
			}
			else if (Config.TranslatorType == TranslatorType.EZTrans)
			{
				var translator = new EZTranslator();

				Translator = translator;
			}
			else if (Config.TranslatorType == TranslatorType.Bing)
			{
				var translator = new BingTranslator();

				Translator = translator;
			}
			else
			{
				Translator = null;
			}

			Downloader.SetTranslator(Translator);
		}

		private async Task SelectNovel(Novel novel)
		{
			if (Downloader == null)
			{
				Debug.Assert(false);
				return;
			}

			if (novel == null)
			{
				return;
			}

			novel.ShowProgress = true;
			RaisePropertyChanged(nameof(ShowProgress));

			if (novel.Episodes.Count == 0)
			{
				var Episodes = await Task.Run(() => Downloader.DownloadList(novel)).ConfigureAwait(false);

				if (Episodes != null)
				{
					novel.Episodes.Clear();
					novel.Episodes.AddAll(Episodes);

					await App.Current?.Dispatcher?.InvokeAsync(() =>
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

			if (novel.Episodes.Count > 0 && string.IsNullOrWhiteSpace(novel.Episodes[0].SourceText))
			{
				await Downloader.DownloadNovel(novel, 0, 0, false, true).ConfigureAwait(false);
			}

			novel.ShowProgress = false;
			RaisePropertyChanged(nameof(ShowProgress));
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
			if (Downloader == null)
			{
				Debug.Assert(false);
				return;
			}

			if (novel == null)
			{
				return;
			}

			string filePath = Config.NovelPath + novel.Name;
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

					App.Current?.Dispatcher?.InvokeAsync(() =>
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

			SelectedNovel = novel;
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

			SelectedNovel = novel;
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

			SelectedNovel = novel;
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
			var novel = SelectedNovel;
			if (novel == null)
			{
				MessageBox.Show("선택된 소설이 없습니다.");
				return;
			}

			if (novel.Episodes.Count == 0)
			{
				return;
			}

			novel.EpisodeStartIndex = 0;
			novel.EpisodeEndIndex = novel.Episodes.Count - 1;
		}

		private async void OnDownload()
		{
			var novel = SelectedNovel;
			if (novel == null)
			{
				MessageBox.Show("선택된 소설이 없습니다.");
				return;
			}

			if (novel.Episodes.Count == 0)
			{
				MessageBox.Show("다운로드할 수 없습니다.");
				return;
			}

			int startIndex = novel.EpisodeStartIndex;
			int endIndex = novel.EpisodeEndIndex;

			novel.ProgressMax = ((endIndex - startIndex) + 1) * 2;
			novel.ProgressValue = 0;

			await Downloader.DownloadNovel(novel, startIndex, endIndex, true).ConfigureAwait(false);
		}

		private void OnClosing()
		{
			Config.NovelList = NovelList.ToList();
			Config.Save();
		}

		private void OnExplorer()
		{
			var explorerWindow = new ExplorerWindow();
			explorerWindow.Show();
		}

		private void OnExit()
		{
			App.Current?.MainWindow?.Close();
		}

		private void OnShowLicense()
		{
			var window = new LicenseWindow();
			window.ShowDialog();
		}

		private void OnShowAbout()
		{
			var window = new AboutWindow();
			window.ShowDialog();
		}

		private void OnSetting()
		{
			var window = new SettingWindow();
			window.ShowDialog();
		}

		private void OnOpenFolder()
		{
			if (SelectedNovel == null)
			{
				return;
			}

			var path = Config.NovelPath + SelectedNovel.Name;
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			Process.Start(path);
		}

		private bool CanOpenFolder()
		{
			if (SelectedNovel == null)
			{
				return false;
			}

			return true;
		}

		private void OnDeleteNovel()
		{
			if (SelectedNovel == null)
			{
				return;
			}

			NovelList.Remove(SelectedNovel);

			if (NovelList.Count > 0)
			{
				SelectedNovel = NovelList[0];
			}
		}

		private bool CanDeleteNovel()
		{
			if (SelectedNovel == null)
			{
				return false;
			}

			return true;
		}

		private void Downloader_PrologueChanged(object sender, System.EventArgs e)
		{
			var novel = sender as Novel;
			novel.RaisePropertyChanged(nameof(Novel.DescWithPrologue));
		}
	}
}
