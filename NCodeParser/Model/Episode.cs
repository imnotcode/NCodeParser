using GalaSoft.MvvmLight;

namespace NCodeParser.Model
{
	public class Episode : ObservableObject
	{
		public int Number
		{
			get { return _Number; }
			set
			{
				_Number = value; RaisePropertyChanged();
			}
		}

		public string Title
		{
			get { return _Title; }
			set
			{
				_Title = value; RaisePropertyChanged();
			}
		}

		public string URLNumber
		{
			get
			{
				return _URLNumber;
			}
			set
			{
				_URLNumber = value;
				RaisePropertyChanged();
			}
		}

		public string Text
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(TranslatedText))
				{
					return TranslatedText;
				}

				return SourceText;
			}
		}

		public string SourceText
		{
			get
			{
				return _Text;
			}
			set
			{
				_Text = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(Text));
			}
		}

		public string TranslatedText
		{
			get
			{
				return _TranslatedText;
			}
			set
			{
				_TranslatedText = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(Text));
			}
		}

		private int _Number;
		private string _Title;
		private string _URLNumber;
		private string _Text;
		private string _TranslatedText;

		public override string ToString()
		{
			return $"{Number} : {Title}";
		}
	}
}
