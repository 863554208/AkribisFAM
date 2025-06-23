using AkribisFAM.ViewModel;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using System;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for AKBMessageBox.xaml
    /// For those who want to use this custom view, please also include the AKBMessageBoxVM.cs
    /// </summary>
    public partial class AKBMessageBoxView : Window, IDisposable
    {
        public AKBMessageBoxVM msgBoxVM = new AKBMessageBoxVM();
        public bool Result = false;
        public AKBMessageBoxView()
        {
            InitializeComponent();
            DataContext = msgBoxVM;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Dispose();
            //this.Visibility = Visibility.Hidden;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            Result = false;
            Dispose();
            //this.Visibility = Visibility.Hidden;
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {

            Result = true;
            Dispose();
            //this.Visibility = Visibility.Hidden;
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Dispose();
            //this.Visibility = Visibility.Hidden;


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Result = false;
        }

        private void Window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (!IsMouseOver)
            //{
            //    this.Focus();
            //    this.Background = new SolidColorBrush(Colors.Red);
            //}
        }

        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            //Background = new SolidColorBrush(Colors.Red);
            //Topmost=true;
            //Focus();
            //Activate();
        }

        public void Dispose()
        {
            Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (msgBoxVM.MsgboxBtn == MessageBoxButton.OK) btnOk_Click(sender, e);


            }
        }
    }

    public static class AKBMessageBox
    {
        public enum MessageBoxIcon
        {
            //
            // Summary:
            //     The message box contains no symbols.
            None = 0,
            //
            // Summary:
            //     The message box contains a symbol consisting of a white X in a circle with a
            //     red background.
            Hand = 16,
            //
            // Summary:
            //     The message box contains a symbol consisting of a question mark in a circle.
            //     The question mark message icon is no longer recommended because it does not clearly
            //     represent a specific type of message and because the phrasing of a message as
            //     a question could apply to any message type. In addition, users can confuse the
            //     question mark symbol with a help information symbol. Therefore, do not use this
            //     question mark symbol in your message boxes. The system continues to support its
            //     inclusion only for backward compatibility.
            Question = 32,
            //
            // Summary:
            //     The message box contains a symbol consisting of an exclamation point in a triangle
            //     with a yellow background.
            Exclamation = 48,
            //
            // Summary:
            //     The message box contains a symbol consisting of a lowercase letter i in a circle.
            Asterisk = 64,
            //
            // Summary:
            //     The message box contains a symbol consisting of white X in a circle with a red
            //     background.
            Stop = 16,
            //
            // Summary:
            //     The message box contains a symbol consisting of white X in a circle with a red
            //     background.
            Error = 16,
            //
            // Summary:
            //     The message box contains a symbol consisting of an exclamation point in a triangle
            //     with a yellow background.
            Warning = 48,
            //
            // Summary:
            //     The message box contains a symbol consisting of a lowercase letter i in a circle.
            Information = 64,

            //
            // Summary:
            //     The message box contains a symbol consisting of a lowercase letter i in a circle.
            Check = 80,
            Completed = 80,
            Correct = 80,
            Done = 80,
            Ok = 80,
            Tick = 80
        }

        public static AKBMessageBoxView akbMsgBoxView { get; set; }
        public static void Show(string msg, string header = "Information", MessageBoxIcon msgIcon = MessageBoxIcon.None, MessageBoxButton msgBtn = MessageBoxButton.OK)
        {
            using (AKBMessageBoxView akbMsgBoxView = new AKBMessageBoxView())
            {
                AKBMessageBoxVM vm = akbMsgBoxView.msgBoxVM;
                vm.Message = msg;
                vm.Header = header;
                vm.MsgboxBtn = msgBtn;
                vm.MsgIcon = msgIcon;


                //if (ui != null) ui.Opacity = 0.5;
                //else System.Windows.Application.Current.MainWindow.Opacity = 0.5;

                // Set the Icon type and visiblity
                switch (msgIcon)
                {
                    case MessageBoxIcon.None:
                        vm.IsIconVisible = Visibility.Collapsed;
                        vm.Header = "Information";
                        vm.IconColor = new SolidColorBrush(Colors.Gray);
                        vm.IconWidthHeight = 0;
                        break;
                    case MessageBoxIcon.Error:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.CloseCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Red);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Error";
                        break;
                    case MessageBoxIcon.Question:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.HelpCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Gray);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Question";
                        break;
                    case MessageBoxIcon.Warning:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.AlertCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Orange);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Warning";
                        break;
                    case MessageBoxIcon.Information:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.InformationSlabCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Gray);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Information";
                        break;
                    case MessageBoxIcon.Done:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.CheckCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Green);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Done";
                        break;
                    default:
                        vm.IsIconVisible = Visibility.Collapsed;
                        vm.IconWidthHeight = 0;
                        break;
                }

                // Set the button visiblity
                akbMsgBoxView.msgBoxVM.IsBtnOkVisible = (msgBtn == MessageBoxButton.OK || msgBtn == MessageBoxButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;
                akbMsgBoxView.msgBoxVM.IsBtnCancelVisible = (msgBtn == MessageBoxButton.OKCancel || msgBtn == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;
                akbMsgBoxView.msgBoxVM.IsBtnYesVisible = (msgBtn == MessageBoxButton.YesNo || msgBtn == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;
                akbMsgBoxView.msgBoxVM.IsBtnNoVisible = (msgBtn == MessageBoxButton.YesNo || msgBtn == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;

                //if (callingObject != null) ui.Opacity = 1;
                //else System.Windows.Application.Current.MainWindow.Opacity = 1;

                akbMsgBoxView.Show();
                akbMsgBoxView.Focus();
                akbMsgBoxView.BringIntoView();
            }

        }
        static DependencyObject callingObject;
        public static bool ShowDialog(string msg, string header = "", MessageBoxIcon msgIcon = MessageBoxIcon.None, MessageBoxButton msgBtn = MessageBoxButton.OK, UIElement ui = null)
        {

            using (AKBMessageBoxView akbMsgBoxView = new AKBMessageBoxView())
            {
                AKBMessageBoxVM vm = akbMsgBoxView.msgBoxVM;
                vm.Message = msg;
                vm.MsgboxBtn = msgBtn;
                vm.MsgIcon = msgIcon;


                if (ui != null) ui.Opacity = 0.5;
                else System.Windows.Application.Current.MainWindow.Opacity = 0.5;


                // Set the Icon type and visiblity
                switch (msgIcon)
                {
                    case MessageBoxIcon.None:
                        vm.IsIconVisible = Visibility.Collapsed;
                        vm.Header = "Information";
                        vm.IconColor = new SolidColorBrush(Colors.Gray);
                        vm.IconWidthHeight = 0;
                        break;
                    case MessageBoxIcon.Error:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.CloseCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Red);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Error";
                        break;
                    case MessageBoxIcon.Question:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.HelpCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Gray);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Question";
                        break;
                    case MessageBoxIcon.Warning:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.AlertCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Orange);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Warning";
                        break;
                    case MessageBoxIcon.Information:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.InformationSlabCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Gray);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Information";
                        break;
                    case MessageBoxIcon.Done:
                        vm.IsIconVisible = Visibility.Visible;
                        vm.Icon = PackIconKind.CheckCircle;
                        vm.IconColor = new SolidColorBrush(Colors.Green);
                        vm.IconWidthHeight = 60;
                        vm.Header = "Done";
                        break;
                    default:
                        vm.IsIconVisible = Visibility.Collapsed;
                        vm.IconWidthHeight = 0;
                        break;
                }

                if (header != "") vm.Header = header;

                // Set the button visiblity
                akbMsgBoxView.msgBoxVM.IsBtnOkVisible = (msgBtn == MessageBoxButton.OK || msgBtn == MessageBoxButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;
                akbMsgBoxView.msgBoxVM.IsBtnCancelVisible = (msgBtn == MessageBoxButton.OKCancel || msgBtn == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;
                akbMsgBoxView.msgBoxVM.IsBtnYesVisible = (msgBtn == MessageBoxButton.YesNo || msgBtn == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;
                akbMsgBoxView.msgBoxVM.IsBtnNoVisible = (msgBtn == MessageBoxButton.YesNo || msgBtn == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;

                akbMsgBoxView.ShowDialog();

                if (ui != null) ui.Opacity = 1;
                else System.Windows.Application.Current.MainWindow.Opacity = 1;

                akbMsgBoxView.Focus();
                return akbMsgBoxView.Result;
            }
        }



        //public static bool? ShowDialog(string msg)
        //{
        //    ShowDialog(msg);
        //    return akbMsgBoxView.Result;
        //}
        public static void Show()
        {
            Show("");
        }

        // Helper method to get the calling object (window or user control)


    }
}
