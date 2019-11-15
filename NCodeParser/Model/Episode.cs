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

        private int _Number;
        private string _Title;
		private string _URLNumber;

        public override string ToString()
        {
            return string.Format($"{Number} : {Title}");
        }
    }
}
