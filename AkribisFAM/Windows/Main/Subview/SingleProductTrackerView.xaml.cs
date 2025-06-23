using System;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for SingleProductTrackerView.xaml
    /// </summary>
    public partial class SingleProductTrackerView : UserControl
    {
        public event EventHandler ButtonPressed;
   
        public SingleProductTrackerView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ButtonPressed?.Invoke(sender, e);
        }
    }
}
