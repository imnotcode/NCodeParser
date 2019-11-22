﻿using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;

namespace NCodeParser.Model
{
	public class Novel : ObservableObject
	{
		public NovelType Type
		{
			get
			{
				return _Type;
			}
			set
			{
				_Type = value;
				RaisePropertyChanged();
			}
		}

		public string Code
		{
			get
			{
				return _Code;
			}
			set
			{
				_Code = value;
				RaisePropertyChanged();
			}
		}

		public string Desc
		{
			get
			{
				return _Desc;
			}
			set
			{
				_Desc = value;
				RaisePropertyChanged();
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
				_UpdateCount = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(StringUpdated));
			}
		}

		public string StringUpdated
		{
			get
			{
				return UpdateCount > 0 ? UpdateCount.ToString() : "";
			}
		}

		public int EpisodeStartIndex
		{
			get
			{
				return _EpisodeStartIndex;
			}
			set
			{
				_EpisodeStartIndex = value;
				RaisePropertyChanged();
			}
		}

		public int EpisodeEndIndex
		{
			get
			{
				return _EpisodeEndIndex;
			}
			set
			{
				_EpisodeEndIndex = value;
				RaisePropertyChanged();
			}
		}

		public int ProgressValue
		{
			get
			{
				return _ProgressValue;
			}
			set
			{
				_ProgressValue = value;
				RaisePropertyChanged();
			}
		}

		public int ProgressMax
		{
			get
			{
				return _ProgressMax;
			}
			set
			{
				_ProgressMax = value;
				RaisePropertyChanged();
			}
		}

		public bool Merging
		{
			get
			{
				return _Merging;
			}
			set
			{
				_Merging = value;
				RaisePropertyChanged();
			}
		}

		public ObservableCollection<Episode> Episodes
		{
			get; private set;
		}

		private NovelType _Type;
		private string _Code;
		private string _Desc;
		private int _UpdateCount;
		private int _EpisodeStartIndex;
		private int _EpisodeEndIndex;
		private int _ProgressValue;
		private int _ProgressMax;
		private bool _Merging;

		public Novel()
		{
			Episodes = new ObservableCollection<Episode>();

			ProgressMax = 1;
			ProgressValue = 0;
		}
	}
}