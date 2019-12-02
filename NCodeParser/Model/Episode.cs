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
				return _Text;
			}
			set
			{
				_Text = value;
				RaisePropertyChanged();
			}
		}

        private int _Number;
        private string _Title;
		private string _URLNumber;
		private string _Text;

        public override string ToString()
        {
            return $"{Number} : {Title}";
        }
    }
}
