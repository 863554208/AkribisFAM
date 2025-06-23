namespace AkribisFAM.ViewModel
{
    public class UserVM : ViewModelBase
    {
        private bool _loginButtonEnabled = true;
        public bool LoginButtonEnabled
        {
            get { return _loginButtonEnabled; }
            set { _loginButtonEnabled = value; OnPropertyChanged(); }
        }

        private bool _logoutButtonEnabled = false;
        public bool LogoutButtonEnabled
        {
            get { return _logoutButtonEnabled; }
            set { _logoutButtonEnabled = value; OnPropertyChanged(); }
        }

        private string _username = "";
        public string Username
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }
        
        private string _loginErrorMsg = " "; // added space to avoid buttons moving
        public string LoginErrorMsg
        {
            get { return _loginErrorMsg; }
            set { _loginErrorMsg = value; OnPropertyChanged(); }
        }
    }
}
