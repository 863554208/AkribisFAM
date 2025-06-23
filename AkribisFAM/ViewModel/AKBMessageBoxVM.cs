using MaterialDesignThemes.Wpf;
using System.Windows;
using Brush = System.Windows.Media.Brush;
using AkribisFAM.Windows;

namespace AkribisFAM.ViewModel
{
    public class AKBMessageBoxVM : ViewModelBase
    {
   
        private string _header;
		public string Header
		{
			get { return _header; }
			set { _header = value; OnPropertyChanged(); }
		}

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        private AKBMessageBox.MessageBoxIcon _msgIcon;
        public AKBMessageBox.MessageBoxIcon MsgIcon
        {
            get { return _msgIcon; }
            set { _msgIcon = value; OnPropertyChanged(); }
        }

        private MessageBoxButton _msgboxBtn;
        public MessageBoxButton MsgboxBtn
        {
            get { return _msgboxBtn; }
            set { _msgboxBtn = value; OnPropertyChanged(); }
        }


        private Visibility _isBtnYesVisible;
        public Visibility IsBtnYesVisible
        {
            get { return _isBtnYesVisible; }
            set { _isBtnYesVisible = value; OnPropertyChanged(); }
        }

        private Visibility _isBtnNoVisible;
        public Visibility IsBtnNoVisible
        {
            get { return _isBtnNoVisible; }
            set { _isBtnNoVisible = value; OnPropertyChanged(); }
        }

        private Visibility _isBtnOkVisible;
        public Visibility IsBtnOkVisible
        {
            get { return _isBtnOkVisible; }
            set { _isBtnOkVisible = value; OnPropertyChanged(); }
        }

        private Visibility _isBtnCancelVisible;
        public Visibility IsBtnCancelVisible
        {
            get { return _isBtnCancelVisible; }
            set { _isBtnCancelVisible = value; OnPropertyChanged(); }
        }
        private Visibility _isIconVisible;
        public Visibility IsIconVisible
        {
            get { return _isIconVisible; }
            set { _isIconVisible = value; OnPropertyChanged(); }
        }
        private int _iconWidthHeight;
        public int IconWidthHeight
        {
            get { return _iconWidthHeight; }
            set { _iconWidthHeight = value; OnPropertyChanged(); }
        }
        private PackIconKind _icon;
        public PackIconKind Icon
        {
            get { return _icon; }
            set { _icon = value; OnPropertyChanged(); }
        }

        private Brush _iconColor ;
        public Brush IconColor
        {
            get { return _iconColor; }
            set { _iconColor = value; OnPropertyChanged(); }

        }
    }
}
