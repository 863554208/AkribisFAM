using AkribisFAM.WorkStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for HomingWindow.xaml
    /// </summary>
    public partial class HomingWindow : Window
    {
        private readonly Action _onCancel;
        public HomingWindow(Action onCancel)
        {
            InitializeComponent();
            _onCancel = onCancel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _onCancel?.Invoke(); // This will call CancelSystemHome()
            this.Close();        // Close the homing window
        }
    }
}
