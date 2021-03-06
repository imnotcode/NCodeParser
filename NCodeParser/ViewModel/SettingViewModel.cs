﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NCodeParser.View;
using NCodeParser.ViewModel.Options;

namespace NCodeParser.ViewModel
{
	public class SettingViewModel : ViewModelBase
	{
		public RelayCommand<object> TreeChangedCommand
		{
			get;
			private set;
		}

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

		public BaseSettingViewModel CurrentViewModel
		{
			get
			{
				return _CurrentViewModel;
			}
			set
			{
				_CurrentViewModel = value;
				RaisePropertyChanged();
			}
		}

		private BaseSettingViewModel _CurrentViewModel;

		private Dictionary<string, BaseSettingViewModel> ViewModels;

		public SettingViewModel()
		{
			InitInstance();
			InitControls();
		}

		private void InitInstance()
		{
			TreeChangedCommand = new RelayCommand<object>(OnTreeChanged);
			SaveComamnd = new RelayCommand(OnSave);
			CancelCommand = new RelayCommand(OnCancel);
		}

		private void InitControls()
		{
			ViewModels = new Dictionary<string, BaseSettingViewModel>
			{
				{ "General Setting", new GeneralSettingViewModel() },
				{ "Translate Setting", new TranslateSettingViewModel() },
				{ "GSheets Translate", new GSheetsSettingViewModel() },
				{ "Google Translate", new GTransSettingViewModel() }
			};

			SetViewModel(ViewModels.First().Key);
		}

		private void SetViewModel(string key)
		{
			if (!ViewModels.ContainsKey(key))
			{
				return;
			}

			CurrentViewModel = ViewModels[key];
		}

		private void OnTreeChanged(object arg)
		{
			if (!(arg is TreeView treeView))
			{
				return;
			}

			if (!(treeView.SelectedItem is TreeViewItem item))
			{
				return;
			}

			string Header = item.Header.ToString();
			switch (Header)
			{
				case "기본설정":
					SetViewModel("General Setting");

					break;

				case "번역":
				case "일반설정":
					SetViewModel("Translate Setting");

					break;

				case "Google Sheets":
					SetViewModel("GSheets Translate");

					break;

				case "Google Translator":
					SetViewModel("Google Translate");

					break;
			}
		}

		private void OnSave()
		{
			foreach (var keyValue in ViewModels)
			{
				keyValue.Value.SetConfig();
			}

			Config.Save();

			App.Current.Close<SettingWindow>();
		}

		private void OnCancel()
		{
			App.Current.Close<SettingWindow>();
		}
	}
}
