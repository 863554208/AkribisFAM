using AkribisFAM.ViewModel;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Controls;
using System;
using AkribisFAM.Manager;
using System.Windows.Input;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for AKBMessageBox.xaml
    /// For those who want to use this custom view, please also include the AKBMessageBoxVM.cs
    /// </summary>
    public partial class UserLogin : Window, IDisposable
    {
        private readonly DatabaseManager _databaseManager;
        private readonly UserManager _userManager;
        UserVM userVM = new UserVM();


        public UserLogin(UserManager userManager)
        {
            InitializeComponent();
            DataContext = userVM;
            cbxUser.Items.Clear();
            // Add items from the list to the ComboBox
            foreach (string user in userManager.HistoryUsers)
            {
                cbxUser.Items.Add(user);
            }

            // Optionally, set the first item as selected
            if (cbxUser.Items.Count > 0)
            {
                cbxUser.SelectedIndex = 0;
                userVM.Username = cbxUser.Items[0].ToString();
            }
            userVM.LogoutButtonEnabled = false;
            _userManager = userManager;
            tbPassword.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != this)
            {
                Application.Current.MainWindow.Opacity = 0.5;
            }
            bool hasUser = App.userManager.CurrentUser != null;

            userVM.LoginButtonEnabled = hasUser ? false : true;
            userVM.LogoutButtonEnabled = hasUser ? true : false;
            tbPassword.IsEnabled = hasUser ? false : true;
            cbxUser.IsEnabled = hasUser ? false : true;
            userVM.Password = "";
            tbPassword.Password = "";


        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Opacity = 1;
        }
        
        
        private void UpdateInfoAndClose()
        {

            DialogResult = true;

            Visibility = Visibility.Hidden;
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Opacity = 0;
            //Close();

        }

        public void Dispose()
        {
            //Close();
        }

        private void Shutdown(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Visibility = Visibility.Hidden;

            Application.Current.MainWindow.Opacity = 1;
            //Close();
        }


        private void btnLogin(object sender, RoutedEventArgs e)
        {
            userVM.LoginErrorMsg = " ";
            userVM.LoginButtonEnabled = false;
            if (_userManager.Login(userVM.Username, userVM.Password, out string errMsg))
            {   // Login successful
                userVM.LoginButtonEnabled = false;
                userVM.LogoutButtonEnabled = true;
                tbPassword.IsEnabled = false;
                cbxUser.IsEnabled = false;
                userVM.Password = "";
                tbPassword.Password = "";
                //MainWindow.mainVM.CurrentUser = _userManager.CurrentUser;
                //MainWindow.mainVM.UserStatusColor = new SolidColorBrush(Colors.DarkBlue);
                _userManager.AddNewAndSaveHistoryToText(userVM.Username);
                AKBMessageBox.ShowDialog("Login successfully", msgIcon: AKBMessageBox.MessageBoxIcon.Done);
                UpdateInfoAndClose();

            }
            else
            {
                tbPassword.Password = "";

                userVM.LoginErrorMsg = errMsg;
                userVM.LoginButtonEnabled = true;
                userVM.LogoutButtonEnabled = false;
                AKBMessageBox.ShowDialog("Invalid user", msgIcon: AKBMessageBox.MessageBoxIcon.Information);
            }
        }

        private void btnLogout(object sender, RoutedEventArgs e)
        {
            var userToLogout = _userManager.CurrentUser.Username;

            if (AKBMessageBox.ShowDialog("Are you sure you want to log out?", msgIcon: AKBMessageBox.MessageBoxIcon.Question, msgBtn: MessageBoxButton.YesNo) == true)
            {

                userVM.Password = "";
                userVM.LoginButtonEnabled = true;
                userVM.LogoutButtonEnabled = false;
                tbPassword.IsEnabled = true;
                cbxUser.IsEnabled = true;
                _userManager.Logout();
                //MainWindow.mainVM.CurrentUser = _userManager.CurrentUser;
                //MainWindow.mainVM.UserStatusColor = new SolidColorBrush(Colors.Gray);

            }
        }



        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }



        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin(sender, new RoutedEventArgs());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbPassword.Focus();
        }

        private void cbxUser_DropDownOpened(object sender, EventArgs e)
        {
            cbxUser.Items.Clear();
            var users = _userManager.HistoryUsers.ToArray();

            for (int i = users.Length - 1; i >= 0; i--)
            {
                cbxUser.Items.Add(users[i]);
            }
            if (cbxUser.Items.Count > 0)
            {
                cbxUser.SelectedIndex = 0;
            }
        }

        private void tbPassword_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }



}
