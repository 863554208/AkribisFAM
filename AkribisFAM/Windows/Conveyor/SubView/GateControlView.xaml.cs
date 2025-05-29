using AkribisFAM.WorkStation;
using System;
using System.Windows.Controls;

namespace AkribisFAM.Windows
{
    /// <summary>
    /// Interaction logic for GateControlView.xaml
    /// </summary>
    public partial class GateControlView : UserControl
    {
        public event EventHandler LifterZUpPressed;
        public event EventHandler LifterZDownPressed;
        
        public event EventHandler GateZUpPressed;
        public event EventHandler GateZDownPressed;

        public GateControlView()
        {
            InitializeComponent();
        }

        private void btnLiftUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LifterZUpPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnLiftDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LifterZDownPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnGateUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GateZUpPressed?.Invoke(this, EventArgs.Empty);
        }

        private void btnGateDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GateZDownPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}
